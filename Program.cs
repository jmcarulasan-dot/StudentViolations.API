using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StudentViolations.API.Class;
using StudentViolations.API.IRepository;

using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS — allows Flutter and Blazor to call the API from any origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT Authentication — reads settings from appsettings.json under JwtSettings
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Validates the token on every request — checks issuer, audience, expiry, and signature
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey))
    };
});

// Controllers — registers all controller classes in the project
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger — sets up the browser testing UI with JWT support and custom group ordering
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StudentViolations.API", Version = "v1" });

    // Forces Swagger to display endpoint groups in system flow order
    c.OrderActionsBy(api => api.GroupName switch
    {
        "Registration" => "1",
        "Login" => "2",
        "Guard" => "3",
        "Guidance" => "4",
        "Admin" => "5",
        "Student" => "6",
        _ => "7"
    });

    c.DocInclusionPredicate((docName, apiDesc) => true);
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null) return new[] { api.GroupName };
        return new[] { api.ActionDescriptor.RouteValues["controller"] };
    });

    // Adds the Bearer token input box in Swagger UI so we can test protected endpoints
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token only"
    });

    // Applies the Bearer token requirement to all endpoints in Swagger
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Dependency Injection — connects each interface to its class implementation
builder.Services.AddScoped<ILoginRepository, LoginClass>();
builder.Services.AddScoped<IRegisterRepository, RegisterClass>();
builder.Services.AddScoped<IStudentRepository, StudentClass>();
builder.Services.AddScoped<IViolationRepository, ViolationClass>();
builder.Services.AddScoped<IGuardRepository, GuardClass>();
builder.Services.AddScoped<ISAORepository, SAOClass>();

var app = builder.Build();

// Middleware pipeline — order matters, each request passes through these in sequence
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();