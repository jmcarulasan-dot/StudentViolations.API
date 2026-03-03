import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../providers/auth_provider.dart';
import '../providers/violation_provider.dart';
import '../models/violation.dart';

const _red  = Color(0xFFFD070C);
const _navy = Color(0xFF0F136E);

class StudentDashboard extends StatefulWidget {
  const StudentDashboard({super.key});

  @override
  State<StudentDashboard> createState() => _StudentDashboardState();
}

class _StudentDashboardState extends State<StudentDashboard> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final authProvider = Provider.of<AuthProvider>(context, listen: false);
      if (authProvider.currentUser != null) {
        Provider.of<ViolationProvider>(context, listen: false)
            .loadStudentViolations(authProvider.currentUser!.id);
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F7FA),
      appBar: AppBar(
        title: const Text('Student Dashboard'),
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
      body: Consumer2<AuthProvider, ViolationProvider>(
        builder: (context, authProvider, violationProvider, child) {
          final currentUser = authProvider.currentUser;
          final violations = violationProvider.violations;

          if (violationProvider.isLoading) {
            return const Center(child: CircularProgressIndicator(color: _navy));
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Student Info Card
                _buildStudentInfoCard(context, currentUser),
                const SizedBox(height: 16),

                // Violation Summary
                _buildViolationSummaryCard(context, violations),
                const SizedBox(height: 16),

                // Current Status
                if (violations.isNotEmpty) ...[
                  _buildCurrentStatusCard(context, violations),
                  const SizedBox(height: 16),
                ],

                // Violation History
                _buildViolationHistoryCard(context, violations),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildStudentInfoCard(BuildContext context, currentUser) {
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
            width: 60,
            height: 60,
            decoration: BoxDecoration(
              color: Colors.white.withOpacity(0.15),
              shape: BoxShape.circle,
            ),
            child: const Icon(Icons.school_rounded, color: Colors.white, size: 30),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  currentUser?.name ?? 'Student',
                  style: const TextStyle(
                    fontSize: 17,
                    fontWeight: FontWeight.w700,
                    color: Colors.white,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  currentUser?.gradeSection ?? 'No Year/Course',
                  style: const TextStyle(fontSize: 13, color: Colors.white70),
                ),
                Text(
                  'ID: ${currentUser?.id ?? 'N/A'}',
                  style: const TextStyle(fontSize: 12, color: Colors.white60),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildViolationSummaryCard(BuildContext context, List<Violation> violations) {
    final noIdCount = violations.where((v) => v.type == ViolationType.noId).length;
    final noUniformCount = violations.where((v) => v.type == ViolationType.noUniform).length;
    final piercingCount = violations.where((v) => v.type == ViolationType.piercing).length;
    final coloredHairCount = violations.where((v) => v.type == ViolationType.coloredHair).length;

    return _card(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _cardTitle('Violation Summary'),
          const SizedBox(height: 14),
          Row(
            children: [
              Expanded(child: _buildStatTile('No ID', noIdCount, Icons.badge_rounded, Colors.red)),
              const SizedBox(width: 8),
              Expanded(child: _buildStatTile('No Uniform', noUniformCount, Icons.person_off_rounded, Colors.orange)),
            ],
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              Expanded(child: _buildStatTile('Piercing', piercingCount, Icons.diamond_rounded, Colors.purple)),
              const SizedBox(width: 8),
              Expanded(child: _buildStatTile('Colored Hair', coloredHairCount, Icons.face_rounded, Colors.blue)),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildStatTile(String title, int count, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: color.withOpacity(0.08),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withOpacity(0.2)),
      ),
      child: Column(
        children: [
          Icon(icon, color: color, size: 22),
          const SizedBox(height: 4),
          Text(count.toString(),
              style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800, color: color)),
          Text(title,
              style: TextStyle(fontSize: 11, color: color.withOpacity(0.8)),
              textAlign: TextAlign.center),
        ],
      ),
    );
  }

  Widget _buildCurrentStatusCard(BuildContext context, List<Violation> violations) {
    final latest = violations.first;
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: _red.withOpacity(0.05),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: _red.withOpacity(0.2)),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: _red.withOpacity(0.1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(_getStatusIcon(latest), color: _red, size: 22),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('Current Status',
                    style: TextStyle(fontSize: 12, color: Colors.black45)),
                Text(
                  latest.statusDescription,
                  style: const TextStyle(
                    fontSize: 14,
                    fontWeight: FontWeight.w700,
                    color: _red,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  _getStatusDescription(latest),
                  style: const TextStyle(fontSize: 12, color: Colors.black54),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildViolationHistoryCard(BuildContext context, List<Violation> violations) {
    return _card(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _cardTitle('Violation History'),
          const SizedBox(height: 8),
          violations.isEmpty
              ? const Padding(
                  padding: EdgeInsets.symmetric(vertical: 24),
                  child: Center(
                    child: Column(
                      children: [
                        Icon(Icons.check_circle_rounded, size: 56, color: Colors.green),
                        SizedBox(height: 8),
                        Text('No violations recorded',
                            style: TextStyle(
                                fontSize: 15, fontWeight: FontWeight.w600, color: Colors.green)),
                        Text('Keep up the good work!',
                            style: TextStyle(color: Colors.black45, fontSize: 12)),
                      ],
                    ),
                  ),
                )
              : ListView.separated(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: violations.length,
                  separatorBuilder: (_, __) =>
                      const Divider(height: 1),
                  itemBuilder: (context, index) {
                    final v = violations[index];
                    return ListTile(
                      contentPadding: const EdgeInsets.symmetric(horizontal: 0, vertical: 4),
                      leading: CircleAvatar(
                        backgroundColor:
                            _getViolationTypeColor(v.type).withOpacity(0.15),
                        child: Icon(_getViolationTypeIcon(v.type),
                            color: _getViolationTypeColor(v.type), size: 20),
                      ),
                      title: Text(v.violationDescription,
                          style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600)),
                      subtitle: Text(
                        '${DateFormat('MMM dd, yyyy').format(v.date)}  •  Offense #${v.offenseCount}',
                        style: const TextStyle(fontSize: 11),
                      ),
                      trailing: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                        decoration: BoxDecoration(
                          color: _navy.withOpacity(0.08),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(v.statusDescription,
                            style: const TextStyle(
                                fontSize: 10, fontWeight: FontWeight.w600, color: _navy)),
                      ),
                    );
                  },
                ),
        ],
      ),
    );
  }

  Widget _card({required Widget child}) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
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
      child: child,
    );
  }

  Widget _cardTitle(String text) {
    return Row(
      children: [
        Container(width: 4, height: 18,
            decoration: BoxDecoration(color: _navy, borderRadius: BorderRadius.circular(2))),
        const SizedBox(width: 8),
        Text(text,
            style: const TextStyle(fontSize: 15, fontWeight: FontWeight.w700, color: _navy)),
      ],
    );
  }

  IconData _getStatusIcon(Violation v) {
    switch (v.status) {
      case ViolationStatus.warning:             return Icons.warning_rounded;
      case ViolationStatus.parentNotified:      return Icons.phone_rounded;
      case ViolationStatus.referredToSAO:       return Icons.admin_panel_settings_rounded;
      case ViolationStatus.referredToGuidance:  return Icons.psychology_rounded;
      case ViolationStatus.disciplinaryAction:  return Icons.gavel_rounded;
      default:                                  return Icons.check_circle_rounded;
    }
  }

  String _getStatusDescription(Violation v) {
    switch (v.status) {
      case ViolationStatus.warning:
        return 'This is your first offense. Please comply with school rules.';
      case ViolationStatus.parentNotified:
        return 'Your parents/guardians have been notified.';
      case ViolationStatus.referredToSAO:
        return 'Please report to the Student Affairs Office.';
      case ViolationStatus.referredToGuidance:
        return 'You have been referred to the Guidance Office for counseling.';
      case ViolationStatus.disciplinaryAction:
        return 'Disciplinary action has been taken. Follow the instructions given.';
      default:
        return 'No action required.';
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

  IconData _getViolationTypeIcon(ViolationType type) {
    switch (type) {
      case ViolationType.noId:         return Icons.badge_rounded;
      case ViolationType.noUniform:    return Icons.person_off_rounded;
      case ViolationType.piercing:     return Icons.diamond_rounded;
      case ViolationType.coloredHair:  return Icons.face_rounded;
    }
  }

  void _logout(BuildContext context) async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    await authProvider.logout();
    if (mounted) Navigator.of(context).pushReplacementNamed('/login');
  }
}