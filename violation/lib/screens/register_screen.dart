import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import '../models/user.dart';

// ── Official ACLC Colors ──────────────────────────────────────────────────────
const _red   = Color(0xFFFD070C);
const _navy  = Color(0xFF0F136E);

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final _gradeSectionController = TextEditingController();
  final _contactController = TextEditingController();

  UserRole? _selectedRole;
  String? _selectedYear;
  String? _selectedCourse;
  bool _obscurePassword = true;
  bool _obscureConfirmPassword = true;

  final List<String> _yearLevels = ['1st Year', '2nd Year', '3rd Year', '4th Year'];
  final List<String> _courses = ['BSIT', 'BSA', 'BSBA', 'BSCS', 'BSHM'];

  @override
  void dispose() {
    _nameController.dispose();
    _usernameController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    _gradeSectionController.dispose();
    _contactController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [_navy, Color(0xFF1A1F8F), Color(0xFF0A0D4E)],
          ),
        ),
        child: SafeArea(
          child: Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.all(24.0),
              child: Column(
                children: [
                  // Header
                  Container(
                    width: 90,
                    height: 90,
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
                  const SizedBox(height: 8),
                  const Text(
                    'ACLC College of Mandaue',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.w800,
                      color: Colors.white,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                    decoration: BoxDecoration(
                      color: _red.withOpacity(0.2),
                      borderRadius: BorderRadius.circular(20),
                      border: Border.all(color: _red.withOpacity(0.4)),
                    ),
                    child: const Text(
                      'Create Account',
                      style: TextStyle(fontSize: 12, color: Colors.white70, letterSpacing: 1.2),
                    ),
                  ),
                  const SizedBox(height: 24),

                  // Card
                  Container(
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(24),
                      boxShadow: [
                        BoxShadow(
                          color: _navy.withOpacity(0.3),
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
                            // Role Selection
                            _sectionLabel('Select Role'),
                            const SizedBox(height: 10),
                            DropdownButtonFormField<UserRole>(
                              value: _selectedRole,
                              decoration: InputDecoration(
                                labelText: 'Role',
                                prefixIcon: const Icon(Icons.badge_rounded, color: _navy, size: 20),
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
                                focusedErrorBorder: OutlineInputBorder(
                                  borderRadius: BorderRadius.circular(12),
                                  borderSide: const BorderSide(color: _navy, width: 1.8),
                                ),
                                errorBorder: OutlineInputBorder(
                                  borderRadius: BorderRadius.circular(12),
                                  borderSide: const BorderSide(color: _red, width: 1.5),
                                ),
                                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                              ),
                              hint: const Text('Select your role'),
                              items: UserRole.values.map((role) {
                                return DropdownMenuItem(
                                  value: role,
                                  child: Row(
                                    children: [
                                      Icon(_getRoleIcon(role), size: 18, color: _navy),
                                      const SizedBox(width: 10),
                                      Text(_getRoleDisplayName(role),
                                          style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w600)),
                                    ],
                                  ),
                                );
                              }).toList(),
                              onChanged: (value) => setState(() => _selectedRole = value),
                              validator: (v) => v == null ? 'Please select a role' : null,
                            ),
                            const SizedBox(height: 24),

                            // Full Name
                            _sectionLabel('Full Name'),
                            const SizedBox(height: 8),
                            _buildTextField(
                              controller: _nameController,
                              label: 'Full Name',
                              icon: Icons.person_outline_rounded,
                              validator: (v) {
                                if (v == null || v.isEmpty) return 'Please enter your full name';
                                if (v.length < 3) return 'Name must be at least 3 characters';
                                return null;
                              },
                            ),
                            const SizedBox(height: 16),

                            // Username
                            _buildTextField(
                              controller: _usernameController,
                              label: 'Username',
                              icon: Icons.alternate_email_rounded,
                              validator: (v) {
                                if (v == null || v.isEmpty) return 'Please enter a username';
                                if (v.length < 3) return 'Username must be at least 3 characters';
                                if (!RegExp(r'^[a-zA-Z0-9_]+$').hasMatch(v))
                                  return 'Letters, numbers, and underscores only';
                                return null;
                              },
                            ),
                            const SizedBox(height: 16),

                            // Password
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
                              validator: (v) {
                                if (v == null || v.isEmpty) return 'Please enter a password';
                                if (v.length < 6) return 'Password must be at least 6 characters';
                                return null;
                              },
                            ),
                            const SizedBox(height: 16),

                            // Confirm Password
                            _buildTextField(
                              controller: _confirmPasswordController,
                              label: 'Confirm Password',
                              icon: Icons.lock_outline_rounded,
                              obscure: _obscureConfirmPassword,
                              suffixIcon: IconButton(
                                icon: Icon(
                                  _obscureConfirmPassword
                                      ? Icons.visibility_off_outlined
                                      : Icons.visibility_outlined,
                                  color: Colors.black38,
                                  size: 20,
                                ),
                                onPressed: () => setState(() =>
                                    _obscureConfirmPassword = !_obscureConfirmPassword),
                              ),
                              validator: (v) {
                                if (v == null || v.isEmpty) return 'Please confirm your password';
                                if (v != _passwordController.text) return 'Passwords do not match';
                                return null;
                              },
                            ),

                            // Student-only fields
                            if (_selectedRole == UserRole.student) ...[
                              const SizedBox(height: 16),
                              // Year Dropdown
                              DropdownButtonFormField<String>(
                                value: _selectedYear,
                                dropdownColor: Colors.white,
                                iconEnabledColor: _navy,
                                style: const TextStyle(color: Colors.black87, fontSize: 14),
                                decoration: InputDecoration(
                                  labelText: 'Year Level',
                                  prefixIcon: const Icon(Icons.calendar_today_rounded, color: _navy, size: 20),
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
                                hint: const Text('Select year level'),
                                items: _yearLevels.map((year) {
                                  return DropdownMenuItem(
                                    value: year,
                                    child: Text(year),
                                  );
                                }).toList(),
                                onChanged: (value) => setState(() => _selectedYear = value),
                                validator: (v) => v == null ? 'Please select your year level' : null,
                              ),
                              const SizedBox(height: 16),

                              // Course Dropdown
                              DropdownButtonFormField<String>(
                                value: _selectedCourse,
                                dropdownColor: Colors.white,
                                iconEnabledColor: _navy,
                                style: const TextStyle(color: Colors.black87, fontSize: 14),
                                decoration: InputDecoration(
                                  labelText: 'Course',
                                  prefixIcon: const Icon(Icons.menu_book_rounded, color: _navy, size: 20),
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
                                hint: const Text('Select course'),
                                items: _courses.map((course) {
                                  return DropdownMenuItem(
                                    value: course,
                                    child: Text(course),
                                  );
                                }).toList(),
                                onChanged: (value) => setState(() => _selectedCourse = value),
                                validator: (v) => v == null ? 'Please select your course' : null,
                              ),
                              const SizedBox(height: 16),
                              _buildTextField(
                                controller: _contactController,
                                label: 'Contact Number',
                                icon: Icons.phone_outlined,
                                hint: 'e.g., 09123456789',
                                keyboardType: TextInputType.phone,
                                validator: (v) {
                                  if (v == null || v.isEmpty)
                                    return 'Please enter your contact number';
                                  if (!RegExp(r'^[0-9]{11}$').hasMatch(v))
                                    return 'Please enter a valid 11-digit number';
                                  return null;
                                },
                              ),
                            ],
                            const SizedBox(height: 28),

                            // Register Button
                            Consumer<AuthProvider>(
                              builder: (context, authProvider, _) {
                                if (authProvider.isLoading) {
                                  return const Center(
                                    child: CircularProgressIndicator(color: _navy),
                                  );
                                }
                                return SizedBox(
                                  width: double.infinity,
                                  height: 50,
                                  child: ElevatedButton(
                                    onPressed: _selectedRole == null ? null : _register,
                                    style: ElevatedButton.styleFrom(
                                      backgroundColor: _navy,
                                      disabledBackgroundColor: Colors.grey.shade300,
                                      foregroundColor: Colors.white,
                                      elevation: 4,
                                      shadowColor: _navy.withOpacity(0.4),
                                      shape: RoundedRectangleBorder(
                                        borderRadius: BorderRadius.circular(12),
                                      ),
                                    ),
                                    child: const Text(
                                      'REGISTER',
                                      style: TextStyle(
                                        fontSize: 15,
                                        fontWeight: FontWeight.w800,
                                        letterSpacing: 1.4,
                                      ),
                                    ),
                                  ),
                                );
                              },
                            ),
                            const SizedBox(height: 16),

                            // Login link
                            Row(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                const Text('Already have an account?',
                                    style: TextStyle(fontSize: 13, color: Colors.black54)),
                                TextButton(
                                  onPressed: () => Navigator.of(context)
                                      .pushReplacementNamed('/login'),
                                  style: TextButton.styleFrom(
                                    foregroundColor: _navy,
                                    padding: const EdgeInsets.symmetric(horizontal: 6),
                                  ),
                                  child: const Text(
                                    'Login',
                                    style: TextStyle(
                                      fontWeight: FontWeight.w700,
                                      fontSize: 13,
                                      decoration: TextDecoration.underline,
                                    ),
                                  ),
                                ),
                              ],
                            ),
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
    );
  }

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

  Widget _buildTextField({
    required TextEditingController controller,
    required String label,
    required IconData icon,
    bool obscure = false,
    Widget? suffixIcon,
    String? hint,
    TextInputType? keyboardType,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      controller: controller,
      obscureText: obscure,
      keyboardType: keyboardType,
      style: const TextStyle(fontSize: 14, color: Colors.black87),
      decoration: InputDecoration(
        labelText: label,
        hintText: hint,
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

  void _register() async {
    if (_formKey.currentState!.validate() && _selectedRole != null) {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      await authProvider.register(
        username: _usernameController.text,
        password: _passwordController.text,
        name: _nameController.text,
        role: _selectedRole!,
        gradeSection: _selectedRole == UserRole.student ? '${_selectedYear ?? ''} - ${_selectedCourse ?? ''}' : null,
        contactNumber: _selectedRole == UserRole.student ? _contactController.text : null,
      );

      if (authProvider.currentUser != null) {
        // Log out immediately so user goes back to login
        await authProvider.logout();
        if (mounted) {
          // Reset form
          _formKey.currentState!.reset();
          _nameController.clear();
          _usernameController.clear();
          _passwordController.clear();
          _confirmPasswordController.clear();
          _gradeSectionController.clear();
          _contactController.clear();
          setState(() {
            _selectedRole = null;
            _selectedYear = null;
            _selectedCourse = null;
          });

          // Show success message
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: const Row(
                children: [
                  Icon(Icons.check_circle_rounded, color: Colors.white, size: 20),
                  SizedBox(width: 8),
                  Text(
                    'Registration successful! Please log in.',
                    style: TextStyle(fontWeight: FontWeight.w600),
                  ),
                ],
              ),
              backgroundColor: Colors.green,
              behavior: SnackBarBehavior.floating,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
              margin: const EdgeInsets.all(16),
              duration: const Duration(seconds: 3),
            ),
          );
        }
      } else {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text(authProvider.error ?? 'Registration failed'),
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
}