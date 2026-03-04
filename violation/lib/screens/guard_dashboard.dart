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
        title: const Text('Guard Dashboard',
            style: TextStyle(fontWeight: FontWeight.w700, fontSize: 18)),
        backgroundColor: _navy,
        foregroundColor: Colors.white,
        elevation: 3,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout_rounded),
            onPressed: () => _logout(context),
            tooltip: 'Logout',
          ),
        ],
      ),
      body: Consumer<ViolationProvider>(
        builder: (context, violationProvider, child) {
          if (violationProvider.isLoading) {
            return const Center(child: CircularProgressIndicator(color: _navy));
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // ── Welcome Card ──────────────────────────────────────────
                _buildWelcomeCard(context),
                const SizedBox(height: 16),

                // ── Record Violation Card ─────────────────────────────────
                _buildRecordCard(context, violationProvider),
                const SizedBox(height: 16),

                // ── Recent Violations ─────────────────────────────────────
                _buildRecentViolationsCard(violationProvider),
                const SizedBox(height: 16),
              ],
            ),
          );
        },
      ),
    );
  }

  // ── Welcome Card ────────────────────────────────────────────────────────────
  Widget _buildWelcomeCard(BuildContext context) {
    final name = Provider.of<AuthProvider>(context, listen: false).currentUser?.name ?? 'Guard';
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        gradient: const LinearGradient(
          colors: [_navy, Color(0xFF1A1F8F)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: _navy.withOpacity(0.3), blurRadius: 12, offset: const Offset(0, 4)),
        ],
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(0.15),
              borderRadius: BorderRadius.circular(12),
            ),
            child: const Icon(Icons.security_rounded, color: Colors.white, size: 30),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Welcome, $name!',
                  style: const TextStyle(
                      fontSize: 17, fontWeight: FontWeight.w800, color: Colors.white),
                ),
                const SizedBox(height: 2),
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

  // ── Record Violation Card ───────────────────────────────────────────────────
  Widget _buildRecordCard(BuildContext context, ViolationProvider violationProvider) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: _navy.withOpacity(0.08), blurRadius: 12, offset: const Offset(0, 4)),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Card Title
            Row(
              children: [
                Container(
                  width: 4, height: 22,
                  decoration: BoxDecoration(color: _red, borderRadius: BorderRadius.circular(2)),
                ),
                const SizedBox(width: 10),
                const Text('Record Violation',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700, color: _navy)),
              ],
            ),
            const SizedBox(height: 18),

            // ── Student Dropdown ────────────────────────────────────────
            DropdownButtonFormField<User>(
              value: _selectedStudent,
              isExpanded: true,
              decoration: InputDecoration(
                labelText: 'Select Student',
                prefixIcon: const Icon(Icons.person_search_rounded, color: _navy, size: 20),
                filled: true,
                fillColor: const Color(0xFFF7F8FC),
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: Color(0xFFDDE1EE))),
                enabledBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: Color(0xFFDDE1EE))),
                focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: _navy, width: 1.8)),
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
              ),
              hint: const Text('Choose a student', style: TextStyle(fontSize: 13)),
              items: violationProvider.students.map((student) {
                return DropdownMenuItem(
                  value: student,
                  child: Text(
                    '${student.name} — ${student.gradeSection ?? 'No Year/Course'}',
                    style: const TextStyle(fontSize: 13),
                    overflow: TextOverflow.ellipsis,
                  ),
                );
              }).toList(),
              onChanged: (value) => setState(() => _selectedStudent = value),
            ),
            const SizedBox(height: 16),

            // ── Violation Type ──────────────────────────────────────────
            const Text('Violation Type',
                style: TextStyle(fontSize: 13, fontWeight: FontWeight.w700, color: _navy)),
            const SizedBox(height: 10),
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
                    padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 9),
                    decoration: BoxDecoration(
                      color: selected ? _red : Colors.grey.shade100,
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(
                        color: selected ? _red : Colors.grey.shade300,
                        width: 1.5,
                      ),
                      boxShadow: selected
                          ? [BoxShadow(color: _red.withOpacity(0.25), blurRadius: 6, offset: const Offset(0, 2))]
                          : [],
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(_getViolationTypeIcon(type),
                            size: 16, color: selected ? Colors.white : _navy),
                        const SizedBox(width: 6),
                        Text(
                          _getViolationTypeLabel(type),
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
            ),
            const SizedBox(height: 16),

            // ── Remarks ─────────────────────────────────────────────────
            TextFormField(
              controller: _remarksController,
              maxLines: 2,
              style: const TextStyle(fontSize: 13),
              decoration: InputDecoration(
                labelText: 'Remarks (Optional)',
                prefixIcon: const Icon(Icons.note_outlined, color: _navy, size: 20),
                filled: true,
                fillColor: const Color(0xFFF7F8FC),
                border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: Color(0xFFDDE1EE))),
                enabledBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: Color(0xFFDDE1EE))),
                focusedBorder: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(12),
                    borderSide: const BorderSide(color: _navy, width: 1.8)),
                contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              ),
            ),
            const SizedBox(height: 18),

            // ── Submit Button ───────────────────────────────────────────
            SizedBox(
              width: double.infinity,
              height: 50,
              child: ElevatedButton.icon(
                onPressed: _selectedStudent != null && _selectedViolationType != null
                    ? () => _recordViolation(context)
                    : null,
                style: ElevatedButton.styleFrom(
                  backgroundColor: _red,
                  disabledBackgroundColor: Colors.grey.shade300,
                  foregroundColor: Colors.white,
                  elevation: 4,
                  shadowColor: _red.withOpacity(0.4),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                ),
                icon: const Icon(Icons.report_rounded, size: 20),
                label: const Text('Submit Violation',
                    style: TextStyle(fontSize: 15, fontWeight: FontWeight.w800, letterSpacing: 0.5)),
              ),
            ),

            // ── Hint text ───────────────────────────────────────────────
            if (_selectedStudent == null || _selectedViolationType == null) ...[
              const SizedBox(height: 10),
              Center(
                child: Text(
                  'Select a student and violation type to enable submit',
                  style: TextStyle(fontSize: 11, color: Colors.grey.shade500),
                  textAlign: TextAlign.center,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  // ── Recent Violations Card ──────────────────────────────────────────────────
  Widget _buildRecentViolationsCard(ViolationProvider violationProvider) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(color: _navy.withOpacity(0.08), blurRadius: 12, offset: const Offset(0, 4)),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
            child: Row(
              children: [
                Container(
                  width: 4, height: 22,
                  decoration: BoxDecoration(color: _navy, borderRadius: BorderRadius.circular(2)),
                ),
                const SizedBox(width: 10),
                const Text('Recent Violations',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.w700, color: _navy)),
                const Spacer(),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: _red.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(
                    '${violationProvider.violations.length} total',
                    style: const TextStyle(fontSize: 11, color: _red, fontWeight: FontWeight.w700),
                  ),
                ),
              ],
            ),
          ),
          const Divider(height: 1),

          // List
          violationProvider.violations.isEmpty
              ? const Padding(
                  padding: EdgeInsets.symmetric(vertical: 40),
                  child: Center(
                    child: Column(
                      children: [
                        Icon(Icons.check_circle_outline_rounded, size: 52, color: Colors.grey),
                        SizedBox(height: 10),
                        Text('No violations recorded yet',
                            style: TextStyle(color: Colors.grey, fontSize: 14)),
                      ],
                    ),
                  ),
                )
              : ListView.separated(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  padding: const EdgeInsets.symmetric(vertical: 8),
                  itemCount: violationProvider.violations.length,
                  separatorBuilder: (_, __) =>
                      const Divider(height: 1, indent: 16, endIndent: 16),
                  itemBuilder: (context, index) {
                    final v = violationProvider.violations[index];
                    return ListTile(
                      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
                      leading: CircleAvatar(
                        backgroundColor: _getViolationTypeColor(v.type).withOpacity(0.15),
                        child: Icon(_getViolationTypeIcon(v.type),
                            color: _getViolationTypeColor(v.type), size: 20),
                      ),
                      title: Text(v.violationDescription,
                          style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600)),
                      subtitle: Text(
                        'ID: ${v.studentId}  •  ${DateFormat('MMM dd, yyyy').format(v.date)}',
                        style: const TextStyle(fontSize: 11, color: Colors.black45),
                      ),
                      trailing: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                        decoration: BoxDecoration(
                          color: _navy.withOpacity(0.1),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text('Offense #${v.offenseCount}',
                            style: const TextStyle(
                                fontSize: 11, fontWeight: FontWeight.w700, color: _navy)),
                      ),
                    );
                  },
                ),
        ],
      ),
    );
  }

  // ── Record Violation Logic ──────────────────────────────────────────────────
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
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: const Row(
              children: [
                Icon(Icons.check_circle_rounded, color: Colors.white, size: 20),
                SizedBox(width: 8),
                Text('Violation recorded successfully!',
                    style: TextStyle(fontWeight: FontWeight.w600)),
              ],
            ),
            backgroundColor: Colors.green,
            behavior: SnackBarBehavior.floating,
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
            margin: const EdgeInsets.all(16),
            duration: const Duration(seconds: 3),
          ),
        );
        setState(() {
          _selectedStudent = null;
          _selectedViolationType = null;
          _remarksController.clear();
        });
      }
    } else {
      if (mounted) {
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
  }

  void _logout(BuildContext context) async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    await authProvider.logout();
    if (mounted) Navigator.of(context).pushReplacementNamed('/login');
  }

  String _getViolationTypeLabel(ViolationType type) {
    switch (type) {
      case ViolationType.noId:        return 'No ID';
      case ViolationType.noUniform:   return 'No Uniform';
      case ViolationType.piercing:    return 'Piercing';
      case ViolationType.coloredHair: return 'Colored Hair';
    }
  }

  IconData _getViolationTypeIcon(ViolationType type) {
    switch (type) {
      case ViolationType.noId:        return Icons.badge_rounded;
      case ViolationType.noUniform:   return Icons.person_off_rounded;
      case ViolationType.piercing:    return Icons.diamond_rounded;
      case ViolationType.coloredHair: return Icons.face_rounded;
    }
  }

  Color _getViolationTypeColor(ViolationType type) {
    switch (type) {
      case ViolationType.noId:        return Colors.red;
      case ViolationType.noUniform:   return Colors.orange;
      case ViolationType.piercing:    return Colors.purple;
      case ViolationType.coloredHair: return Colors.blue;
    }
  }
}