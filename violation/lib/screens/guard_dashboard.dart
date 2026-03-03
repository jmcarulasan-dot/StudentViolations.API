import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../providers/auth_provider.dart';
import '../providers/violation_provider.dart';
import '../models/violation.dart';
import '../models/user.dart';

const _red  = Color(0xFFFD070C);
const _navy = Color(0xFF0F136E);

class GuardDashboard extends StatefulWidget {
  const GuardDashboard({super.key});

  @override
  State<GuardDashboard> createState() => _GuardDashboardState();
}

class _GuardDashboardState extends State<GuardDashboard> {
  User? _selectedStudent;
  ViolationType? _selectedViolationType;
  final _remarksController = TextEditingController();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      Provider.of<ViolationProvider>(context, listen: false).loadStudents();
      Provider.of<ViolationProvider>(context, listen: false).loadAllViolations();
    });
  }

  @override
  void dispose() {
    _remarksController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F7FA),
      appBar: AppBar(
        title: const Text('Guard Dashboard'),
        backgroundColor: _navy,
        foregroundColor: Colors.white,
        elevation: 3,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout_rounded),
            onPressed: () => _logout(context),
          ),
        ],
      ),
      body: Consumer<ViolationProvider>(
        builder: (context, violationProvider, child) {
          if (violationProvider.isLoading) {
            return const Center(child: CircularProgressIndicator(color: _navy));
          }

          return Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Welcome Card
                _buildWelcomeCard(context),
                const SizedBox(height: 16),

                // Record Violation Card
                Expanded(
                  flex: 2,
                  child: _buildRecordCard(context, violationProvider),
                ),
                const SizedBox(height: 16),

                // Recent Violations
                Expanded(
                  flex: 3,
                  child: _buildRecentViolationsCard(violationProvider),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildWelcomeCard(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [_navy, Color(0xFF1A1F8F)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: _navy.withOpacity(0.3),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(0.15),
              borderRadius: BorderRadius.circular(12),
            ),
            child: const Icon(Icons.security_rounded, color: Colors.white, size: 28),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Welcome, ${Provider.of<AuthProvider>(context, listen: false).currentUser?.name ?? 'Guard'}',
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w700,
                    color: Colors.white,
                  ),
                ),
                const Text(
                  'Record student violations at the gate',
                  style: TextStyle(fontSize: 12, color: Colors.white60),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildRecordCard(BuildContext context, ViolationProvider violationProvider) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: _navy.withOpacity(0.08),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Container(
                  width: 4,
                  height: 20,
                  decoration: BoxDecoration(
                    color: _red,
                    borderRadius: BorderRadius.circular(2),
                  ),
                ),
                const SizedBox(width: 8),
                const Text(
                  'Record Violation',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w700,
                    color: _navy,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Student Dropdown
            DropdownButtonFormField<User>(
              value: _selectedStudent,
              decoration: InputDecoration(
                labelText: 'Select Student',
                prefixIcon: const Icon(Icons.person_search_rounded, color: _navy, size: 20),
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
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
              ),
              items: violationProvider.students.map((student) {
                return DropdownMenuItem(
                  value: student,
                  child: Text('${student.name} — ${student.gradeSection ?? 'No Year/Course'}',
                      style: const TextStyle(fontSize: 13)),
                );
              }).toList(),
              onChanged: (value) => setState(() => _selectedStudent = value),
            ),
            const SizedBox(height: 14),

            // Violation Type Chips
            const Text('Violation Type',
                style: TextStyle(fontSize: 13, fontWeight: FontWeight.w600, color: _navy)),
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: ViolationType.values.map((type) {
                final selected = _selectedViolationType == type;
                return GestureDetector(
                  onTap: () => setState(
                      () => _selectedViolationType = selected ? null : type),
                  child: AnimatedContainer(
                    duration: const Duration(milliseconds: 180),
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 7),
                    decoration: BoxDecoration(
                      color: selected ? _red : Colors.grey.shade100,
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(
                        color: selected ? _red : Colors.grey.shade300,
                        width: 1.5,
                      ),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(_getViolationTypeIcon(type),
                            size: 15,
                            color: selected ? Colors.white : _navy),
                        const SizedBox(width: 5),
                        Text(
                          _getViolationTypeLabel(type),
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                            color: selected ? Colors.white : _navy,
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              }).toList(),
            ),
            const SizedBox(height: 14),

            // Remarks
            TextFormField(
              controller: _remarksController,
              maxLines: 2,
              style: const TextStyle(fontSize: 13),
              decoration: InputDecoration(
                labelText: 'Remarks (Optional)',
                prefixIcon: const Icon(Icons.note_outlined, color: _navy, size: 20),
                filled: true,
                fillColor: const Color(0xFFF7F8FC),
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: Color(0xFFDDE1EE))),
                enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: Color(0xFFDDE1EE))),
                focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: _navy, width: 1.8)),
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              ),
            ),
            const SizedBox(height: 14),

            // Submit Button
            SizedBox(
              width: double.infinity,
              height: 46,
              child: ElevatedButton.icon(
                onPressed: _selectedStudent != null && _selectedViolationType != null
                    ? () => _recordViolation(context)
                    : null,
                style: ElevatedButton.styleFrom(
                  backgroundColor: _red,
                  disabledBackgroundColor: Colors.grey.shade300,
                  foregroundColor: Colors.white,
                  elevation: 3,
                  shadowColor: _red.withOpacity(0.4),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                ),
                icon: const Icon(Icons.report_rounded, size: 18),
                label: const Text('Record Violation',
                    style: TextStyle(fontWeight: FontWeight.w700, letterSpacing: 0.5)),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildRecentViolationsCard(ViolationProvider violationProvider) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: _navy.withOpacity(0.08),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
            child: Row(
              children: [
                Container(
                  width: 4,
                  height: 20,
                  decoration: BoxDecoration(
                    color: _navy,
                    borderRadius: BorderRadius.circular(2),
                  ),
                ),
                const SizedBox(width: 8),
                const Text('Recent Violations',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700, color: _navy)),
                const Spacer(),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                  decoration: BoxDecoration(
                    color: _red.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(
                    '${violationProvider.violations.length} total',
                    style: const TextStyle(fontSize: 11, color: _red, fontWeight: FontWeight.w600),
                  ),
                ),
              ],
            ),
          ),
          const Divider(height: 1),
          Expanded(
            child: violationProvider.violations.isEmpty
                ? const Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(Icons.check_circle_outline_rounded, size: 48, color: Colors.grey),
                        SizedBox(height: 8),
                        Text('No violations recorded',
                            style: TextStyle(color: Colors.grey, fontSize: 14)),
                      ],
                    ),
                  )
                : ListView.separated(
                    padding: const EdgeInsets.symmetric(vertical: 8),
                    itemCount: violationProvider.violations.length,
                    separatorBuilder: (_, __) =>
                        const Divider(height: 1, indent: 16, endIndent: 16),
                    itemBuilder: (context, index) {
                      final v = violationProvider.violations[index];
                      return ListTile(
                        leading: CircleAvatar(
                          backgroundColor: _getViolationTypeColor(v.type).withOpacity(0.15),
                          child: Icon(_getViolationTypeIcon(v.type),
                              color: _getViolationTypeColor(v.type), size: 20),
                        ),
                        title: Text(v.violationDescription,
                            style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600)),
                        subtitle: Text(
                          'ID: ${v.studentId}  •  ${DateFormat('MMM dd, yyyy').format(v.date)}',
                          style: const TextStyle(fontSize: 11),
                        ),
                        trailing: Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                          decoration: BoxDecoration(
                            color: _navy.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: Text('Offense #${v.offenseCount}',
                              style: const TextStyle(
                                  fontSize: 11, fontWeight: FontWeight.w600, color: _navy)),
                        ),
                      );
                    },
                  ),
          ),
        ],
      ),
    );
  }

  void _recordViolation(BuildContext context) async {
    final violationProvider = Provider.of<ViolationProvider>(context, listen: false);
    final authProvider = Provider.of<AuthProvider>(context, listen: false);

    await violationProvider.recordViolation(
      studentId: _selectedStudent!.id,
      type: _selectedViolationType!,
      reportedBy: authProvider.currentUser!.id,
      remarks: _remarksController.text.isEmpty ? null : _remarksController.text,
    );

    if (violationProvider.error == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: const Text('Violation recorded successfully'),
          backgroundColor: _navy,
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
          margin: const EdgeInsets.all(16),
        ),
      );
      setState(() {
        _selectedStudent = null;
        _selectedViolationType = null;
        _remarksController.clear();
      });
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(violationProvider.error!),
          backgroundColor: _red,
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
          margin: const EdgeInsets.all(16),
        ),
      );
    }
  }

  void _logout(BuildContext context) async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    await authProvider.logout();
    if (mounted) Navigator.of(context).pushReplacementNamed('/login');
  }

  String _getViolationTypeLabel(ViolationType type) {
    switch (type) {
      case ViolationType.noId:         return 'No ID';
      case ViolationType.noUniform:    return 'No Uniform';
      case ViolationType.piercing:     return 'Piercing';
      case ViolationType.coloredHair:  return 'Colored Hair';
    }
  }

  IconData _getViolationTypeIcon(ViolationType type) {
    switch (type) {
      case ViolationType.noId:         return Icons.badge_rounded;
      case ViolationType.noUniform:    return Icons.person_off_rounded;
      case ViolationType.piercing:     return Icons.diamond_rounded;
      case ViolationType.coloredHair:  return Icons.face_rounded;
    }
  }

  Color _getViolationTypeColor(ViolationType type) {
    switch (type) {
      case ViolationType.noId:         return Colors.red;
      case ViolationType.noUniform:    return Colors.orange;
      case ViolationType.piercing:     return Colors.purple;
      case ViolationType.coloredHair:  return Colors.blue;
    }
  }
}