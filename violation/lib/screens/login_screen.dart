import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import '../models/user.dart';

const _red  = Color(0xFFFD070C);
const _navy = Color(0xFF0F136E);

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen>
    with SingleTickerProviderStateMixin {
  final _formKey = GlobalKey<FormState>();
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  UserRole? _selectedRole;
  bool _obscurePassword = true;
  late AnimationController _animController;
  late Animation<double> _fadeAnim;
  late Animation<Offset> _slideAnim;

  @override
  void initState() {
    super.initState();
    _animController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 900),
    );
    _fadeAnim = CurvedAnimation(parent: _animController, curve: Curves.easeOut);
    _slideAnim = Tween<Offset>(
      begin: const Offset(0, 0.12),
      end: Offset.zero,
    ).animate(CurvedAnimation(parent: _animController, curve: Curves.easeOutCubic));
    _animController.forward();
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    _animController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          // ── Background gradient ───────────────────────────────────────────
          Container(
            width: double.infinity,
            height: double.infinity,
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                colors: [_navy, Color(0xFF1A1F8F), Color(0xFF0A0D4E)],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
            ),
          ),

          // ── Decorative circles ────────────────────────────────────────────
          Positioned(
            top: -60, right: -60,
            child: _decorCircle(220, _red.withOpacity(0.12)),
          ),
          Positioned(
            top: 80, right: 30,
            child: _decorCircle(80, _red.withOpacity(0.08)),
          ),
          Positioned(
            bottom: -80, left: -80,
            child: _decorCircle(280, _red.withOpacity(0.10)),
          ),
          Positioned(
            bottom: 120, left: 20,
            child: _decorCircle(60, Colors.white.withOpacity(0.04)),
          ),

          // ── Main content ──────────────────────────────────────────────────
          SafeArea(
            child: Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
                child: FadeTransition(
                  opacity: _fadeAnim,
                  child: SlideTransition(
                    position: _slideAnim,
                    child: Column(
                      children: [
                        // ── Header ────────────────────────────────────────
                        _buildHeader(),
                        const SizedBox(height: 28),

                        // ── Card ──────────────────────────────────────────
                        Container(
                          decoration: BoxDecoration(
                            color: Colors.white,
                            borderRadius: BorderRadius.circular(24),
                            boxShadow: [
                              BoxShadow(
                                color: _navy.withOpacity(0.35),
                                blurRadius: 40,
                                offset: const Offset(0, 16),
                              ),
                            ],
                          ),
                          child: Padding(
                            padding: const EdgeInsets.all(28.0),
                            child: Form(
                              key: _formKey,
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  const Text(
                                    'Sign in to continue',
                                    style: TextStyle(fontSize: 13, color: Colors.black45),
                                  ),
                                  const SizedBox(height: 20),

                                  // ── Role Selection ────────────────────
                                  _sectionLabel('Select Role'),
                                  const SizedBox(height: 10),
                                  _buildRoleChips(),
                                  const SizedBox(height: 24),

                                  // ── Username ──────────────────────────
                                  _buildTextField(
                                    controller: _usernameController,
                                    label: 'Username',
                                    icon: Icons.person_outline_rounded,
                                    validator: (v) => (v == null || v.isEmpty)
                                        ? 'Please enter your username'
                                        : null,
                                  ),
                                  const SizedBox(height: 16),

                                  // ── Password ──────────────────────────
                                  _buildTextField(
                                    controller: _passwordController,
                                    label: 'Password',
                                    icon: Icons.lock_outline_rounded,
                                    obscure: _obscurePassword,
                                    suffixIcon: IconButton(
                                      icon: Icon(
                                        _obscurePassword
                                            ? Icons.visibility_off_outlined
                                            : Icons.visibility_outlined,
                                        color: Colors.black38,
                                        size: 20,
                                      ),
                                      onPressed: () => setState(
                                          () => _obscurePassword = !_obscurePassword),
                                    ),
                                    validator: (v) => (v == null || v.isEmpty)
                                        ? 'Please enter your password'
                                        : null,
                                  ),
                                  const SizedBox(height: 28),

                                  // ── Login Button ──────────────────────
                                  _buildLoginButton(),
                                  const SizedBox(height: 16),

                                  // ── Register Link ─────────────────────
                                  Row(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: [
                                      const Text(
                                        "Don't have an account?",
                                        style: TextStyle(fontSize: 13, color: Colors.black54),
                                      ),
                                      TextButton(
                                        onPressed: () =>
                                            Navigator.of(context).pushNamed('/register'),
                                        style: TextButton.styleFrom(
                                          foregroundColor: _navy,
                                          padding: const EdgeInsets.symmetric(horizontal: 6),
                                        ),
                                        child: const Text(
                                          'Register',
                                          style: TextStyle(
                                            fontWeight: FontWeight.w700,
                                            fontSize: 13,
                                            decoration: TextDecoration.underline,
                                          ),
                                        ),
                                      ),
                                    ],
                                  ),

                                  // ── Demo hint ─────────────────────────
                                  if (_selectedRole != null) ...[
                                    const SizedBox(height: 16),
                                    _buildDemoHint(),
                                  ],
                                ],
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  // ── Header ──────────────────────────────────────────────────────────────────
  Widget _buildHeader() {
    return Column(
      children: [
        
        Container(
          width: 100,
          height: 100,
          decoration: BoxDecoration(
            color: Colors.white,
            shape: BoxShape.circle,
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.2),
                blurRadius: 20,
                offset: const Offset(0, 8),
              ),
            ],
          ),
          child: ClipOval(
            child: Padding(
              padding: const EdgeInsets.all(8.0),
              child: Image.asset(
                'assets/images/ACLC.png',
                fit: BoxFit.contain,
              ),
            ),
          ),
        ),
        const SizedBox(height: 16),
        const Text(
          'ACLC College of Mandaue',
          textAlign: TextAlign.center,
          style: TextStyle(
            fontSize: 22,
            fontWeight: FontWeight.w800,
            color: Colors.white,
          ),
        ),
        const SizedBox(height: 4),
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
          decoration: BoxDecoration(
            color: _red.withOpacity(0.18),
            borderRadius: BorderRadius.circular(20),
            border: Border.all(color: _red.withOpacity(0.35)),
          ),
          child: const Text(
            'Student Violation System',
            style: TextStyle(
              fontSize: 12,
              color: Colors.white70,
              letterSpacing: 1.2,
              fontWeight: FontWeight.w500,
            ),
          ),
        ),
      ],
    );
  }

  // ── Role chips ───────────────────────────────────────────────────────────────
  Widget _buildRoleChips() {
    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: UserRole.values.map((role) {
        final selected = _selectedRole == role;
        return GestureDetector(
          onTap: () => setState(() => _selectedRole = selected ? null : role),
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 200),
            padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
            decoration: BoxDecoration(
              color: selected ? _navy : Colors.grey.shade100,
              borderRadius: BorderRadius.circular(10),
              border: Border.all(
                color: selected ? _navy : Colors.grey.shade300,
                width: 1.5,
              ),
              boxShadow: selected
                  ? [BoxShadow(color: _navy.withOpacity(0.25), blurRadius: 8, offset: const Offset(0, 3))]
                  : [],
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Icon(_getRoleIcon(role), size: 16,
                    color: selected ? Colors.white : _navy),
                const SizedBox(width: 6),
                Text(
                  _getRoleDisplayName(role),
                  style: TextStyle(
                    fontSize: 13,
                    fontWeight: FontWeight.w600,
                    color: selected ? Colors.white : _navy,
                  ),
                ),
              ],
            ),
          ),
        );
      }).toList(),
    );
  }

  // ── Text field ───────────────────────────────────────────────────────────────
  Widget _buildTextField({
    required TextEditingController controller,
    required String label,
    required IconData icon,
    bool obscure = false,
    Widget? suffixIcon,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      controller: controller,
      obscureText: obscure,
      style: const TextStyle(fontSize: 14, color: Colors.black87),
      decoration: InputDecoration(
        labelText: label,
        labelStyle: const TextStyle(fontSize: 14, color: Colors.black45),
        prefixIcon: Icon(icon, color: _navy, size: 20),
        suffixIcon: suffixIcon,
        filled: true,
        fillColor: const Color(0xFFF7F8FC),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: Color(0xFFDDE1EE)),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: Color(0xFFDDE1EE)),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _navy, width: 1.8),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _red, width: 1.5),
        ),
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      ),
      validator: validator,
    );
  }

  // ── Login button ─────────────────────────────────────────────────────────────
  Widget _buildLoginButton() {
    return Consumer<AuthProvider>(
      builder: (context, authProvider, _) {
        if (authProvider.isLoading) {
          return const Center(child: CircularProgressIndicator(color: _navy));
        }
        return SizedBox(
          width: double.infinity,
          height: 50,
          child: ElevatedButton(
            onPressed: _selectedRole == null ? null : _login,
            style: ElevatedButton.styleFrom(
              backgroundColor: _red,
              disabledBackgroundColor: Colors.grey.shade300,
              foregroundColor: Colors.white,
              elevation: 4,
              shadowColor: _red.withOpacity(0.4),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            child: const Text(
              'LOGIN',
              style: TextStyle(
                fontSize: 15,
                fontWeight: FontWeight.w800,
                letterSpacing: 1.4,
              ),
            ),
          ),
        );
      },
    );
  }

  // ── Demo hint ────────────────────────────────────────────────────────────────
  Widget _buildDemoHint() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
      decoration: BoxDecoration(
        color: _navy.withOpacity(0.06),
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: _navy.withOpacity(0.15)),
      ),
      child: Row(
        children: [
          Icon(Icons.info_outline_rounded, size: 15, color: _navy.withOpacity(0.6)),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              _getLoginInfo(_selectedRole!),
              style: TextStyle(
                fontSize: 11,
                color: _navy.withOpacity(0.7),
                height: 1.4,
              ),
            ),
          ),
        ],
      ),
    );
  }

  // ── Decorative circle ────────────────────────────────────────────────────────
  Widget _decorCircle(double size, Color color) {
    return Container(
      width: size,
      height: size,
      decoration: BoxDecoration(shape: BoxShape.circle, color: color),
    );
  }

  // ── Section label ────────────────────────────────────────────────────────────
  Widget _sectionLabel(String text) {
    return Text(
      text,
      style: const TextStyle(
        fontSize: 13,
        fontWeight: FontWeight.w700,
        color: _navy,
        letterSpacing: 0.3,
      ),
    );
  }

  // ── Login logic ──────────────────────────────────────────────────────────────
  void _login() async {
    if (_formKey.currentState!.validate() && _selectedRole != null) {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      await authProvider.login(
        _usernameController.text,
        _passwordController.text,
      );

      if (authProvider.currentUser != null) {
        if (mounted) Navigator.of(context).pushReplacementNamed('/dashboard');
      } else {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(authProvider.error ?? 'Login failed'),
              backgroundColor: _red,
              behavior: SnackBarBehavior.floating,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              margin: const EdgeInsets.all(16),
            ),
          );
        }
      }
    }
  }

  // ── Helpers ──────────────────────────────────────────────────────────────────
  String _getRoleDisplayName(UserRole role) {
    switch (role) {
      case UserRole.guard:    return 'Guard';
      case UserRole.student:  return 'Student';
      case UserRole.sao:      return 'SAO';
      case UserRole.guidance: return 'Guidance';
    }
  }

  IconData _getRoleIcon(UserRole role) {
    switch (role) {
      case UserRole.guard:    return Icons.security_rounded;
      case UserRole.student:  return Icons.school_rounded;
      case UserRole.sao:      return Icons.admin_panel_settings_rounded;
      case UserRole.guidance: return Icons.psychology_rounded;
    }
  }

  String _getLoginInfo(UserRole role) {
    switch (role) {
      case UserRole.guard:    return 'Demo — Username: guard  |  Password: guard123';
      case UserRole.student:  return 'Demo — Username: student  |  Password: student123';
      case UserRole.sao:      return 'Demo — Username: sao  |  Password: sao123';
      case UserRole.guidance: return 'Demo — Username: guidance  |  Password: guidance123';
    }
  }
}