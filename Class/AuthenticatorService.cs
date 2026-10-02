using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using QRCoder;

namespace StudentViolations.API.Class
{
    public sealed class AuthenticatorService
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private readonly IDataProtector _protector;

        public AuthenticatorService(IDataProtectionProvider provider) =>
            _protector = provider.CreateProtector("SVS.Authenticator.Totp.v1");

        public string Protect(string secret) => _protector.Protect(secret);
        public string Unprotect(string secret) => _protector.Unprotect(secret);

        public string GenerateSecret() => EncodeBase32(RandomNumberGenerator.GetBytes(20));

        public string CreateProvisioningUri(string username, string secret, string issuer)
        {
            var label = Uri.EscapeDataString($"{issuer}:{username}");
            return $"otpauth://totp/{label}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";
        }

        public byte[] CreateQrPng(string provisioningUri)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(provisioningUri, QRCodeGenerator.ECCLevel.M);
            var png = new PngByteQRCode(data);
            return png.GetGraphic(6);
        }

        public bool VerifyCode(string secret, string? suppliedCode)
        {
            var code = (suppliedCode ?? string.Empty).Trim().Replace(" ", string.Empty);
            if (code.Length != 6 || !code.All(char.IsAsciiDigit)) return false;
            var key = DecodeBase32(secret);
            var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
            for (long offset = -1; offset <= 1; offset++)
            {
                var message = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(counter + offset));
                var hash = HMACSHA1.HashData(key, message);
                var index = hash[^1] & 0x0f;
                var value = ((hash[index] & 0x7f) << 24) | (hash[index + 1] << 16) |
                            (hash[index + 2] << 8) | hash[index + 3];
                var expected = (value % 1_000_000).ToString("D6");
                if (CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(code)))
                    return true;
            }
            return false;
        }

        public static string HashRecoveryCode(string code) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(NormalizeRecoveryCode(code))));

        public static string NormalizeRecoveryCode(string code) =>
            new((code ?? string.Empty).Where(char.IsAsciiLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

        public static IReadOnlyList<string> GenerateRecoveryCodes() =>
            Enumerable.Range(0, 10).Select(_ => Convert.ToHexString(RandomNumberGenerator.GetBytes(5))).ToArray();

        private static string EncodeBase32(byte[] bytes)
        {
            var result = new StringBuilder();
            var buffer = 0;
            var bitsLeft = 0;
            foreach (var value in bytes)
            {
                buffer = (buffer << 8) | value;
                bitsLeft += 8;
                while (bitsLeft >= 5)
                {
                    result.Append(Base32Alphabet[(buffer >> (bitsLeft - 5)) & 31]);
                    bitsLeft -= 5;
                }
            }
            if (bitsLeft > 0) result.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 31]);
            return result.ToString();
        }

        private static byte[] DecodeBase32(string text)
        {
            var result = new List<byte>();
            var buffer = 0;
            var bitsLeft = 0;
            foreach (var c in text.TrimEnd('=').ToUpperInvariant())
            {
                var value = Base32Alphabet.IndexOf(c);
                if (value < 0) throw new FormatException("Invalid authenticator secret.");
                buffer = (buffer << 5) | value;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    result.Add((byte)(buffer >> (bitsLeft - 8)));
                    bitsLeft -= 8;
                }
            }
            return result.ToArray();
        }
    }
}
