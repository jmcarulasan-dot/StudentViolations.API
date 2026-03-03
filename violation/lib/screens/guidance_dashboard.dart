import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../providers/auth_provider.dart';
import '../providers/violation_provider.dart';
import '../models/violation.dart';

const _red  = Color(0xFFFD070C);
const _navy = Color(0xFF0F136E);

class GuidanceDashboard extends StatefulWidget {
  const GuidanceDashboard({super.key});

  @override
  State<GuidanceDashboard> createState() => _GuidanceDashboardState();
}

class _GuidanceDashboardState extends State<GuidanceDashboard> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      Provider.of<ViolationProvider>(context, listen: false).loadAllViolations();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F7FA),
      appBar: AppBar(
        title: const Text('Guidance Dashboard'),
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

          final referredCases = violationProvider.violations
              .where((v) => v.status == ViolationStatus.referredToGuidance)
              .toList();

          return Column(
            children: [
              // Welcome Card
              Padding(
                padding: const EdgeInsets.all(16.0),
                child: _buildWelcomeCard(context, referredCases.length, violationProvider),
              ),

              // Stats Row
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                child: Row(
                  children: [
                    Expanded(child: _buildStatCard('Referred Cases', referredCases.length, Icons.person_rounded, Colors.purple)),
                    const SizedBox(width: 10),
                    Expanded(child: _buildStatCard('Completed', _getCompletedCount(violationProvider.violations), Icons.check_circle_rounded, Colors.green)),
                  ],
                ),
              ),
              const SizedBox(height: 16),

              // Cases List
              Expanded(
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0),
                  child: Container(
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
                              Container(width: 4, height: 18,
                                  decoration: BoxDecoration(color: _navy, borderRadius: BorderRadius.circular(2))),
                              const SizedBox(width: 8),
                              const Text('Referred Cases',
                                  style: TextStyle(fontSize: 15, fontWeight: FontWeight.w700, color: _navy)),
                              const Spacer(),
                              Container(
                                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                decoration: BoxDecoration(
                                  color: Colors.purple.withOpacity(0.1),
                                  borderRadius: BorderRadius.circular(20),
                                ),
                                child: Text('${referredCases.length} cases',
                                    style: const TextStyle(
                                        fontSize: 11, color: Colors.purple, fontWeight: FontWeight.w600)),
                              ),
                            ],
                          ),
                        ),
                        const Divider(height: 1),
                        Expanded(
                          child: referredCases.isEmpty
                              ? const Center(
                                  child: Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: [
                                      Icon(Icons.inbox_rounded, size: 56, color: Colors.grey),
                                      SizedBox(height: 8),
                                      Text('No referred cases',
                                          style: TextStyle(color: Colors.grey, fontSize: 14)),
                                    ],
                                  ),
                                )
                              : ListView.builder(
                                  padding: const EdgeInsets.symmetric(vertical: 8),
                                  itemCount: referredCases.length,
                                  itemBuilder: (context, index) {
                                    return _buildCaseCard(referredCases[index]);
                                  },
                                ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 16),
            ],
          );
        },
      ),
    );
  }

  Widget _buildWelcomeCard(BuildContext context, int caseCount, ViolationProvider vp) {
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
          BoxShadow(color: _navy.withOpacity(0.3), blurRadius: 12, offset: const Offset(0, 4)),
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
            child: const Icon(Icons.psychology_rounded, color: Colors.white, size: 28),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Welcome, ${Provider.of<AuthProvider>(context, listen: false).currentUser?.name ?? 'Guidance'}',
                  style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w700, color: Colors.white),
                ),
                const Text('Guidance Office',
                    style: TextStyle(fontSize: 12, color: Colors.white60)),
                const SizedBox(height: 4),
                Text('$caseCount cases referred for counseling',
                    style: const TextStyle(fontSize: 13, color: Colors.white70)),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatCard(String title, int count, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        boxShadow: [
          BoxShadow(color: color.withOpacity(0.12), blurRadius: 10, offset: const Offset(0, 4)),
        ],
        border: Border.all(color: color.withOpacity(0.15)),
      ),
      child: Column(
        children: [
          Icon(icon, color: color, size: 26),
          const SizedBox(height: 6),
          Text(count.toString(),
              style: TextStyle(fontSize: 22, fontWeight: FontWeight.w800, color: color)),
          Text(title,
              style: TextStyle(fontSize: 11, color: color.withOpacity(0.8)),
              textAlign: TextAlign.center),
        ],
      ),
    );
  }

  Widget _buildCaseCard(Violation violation) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 5),
      decoration: BoxDecoration(
        color: const Color(0xFFF7F8FC),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: ExpansionTile(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        leading: CircleAvatar(
          backgroundColor: _getViolationTypeColor(violation.type).withOpacity(0.15),
          child: Icon(_getViolationTypeIcon(violation.type),
              color: _getViolationTypeColor(violation.type), size: 18),
        ),
        title: Text('Student ID: ${violation.studentId}',
            style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600)),
        subtitle: Text(
          '${violation.violationDescription}  •  ${DateFormat('MMM dd, yyyy').format(violation.date)}',
          style: const TextStyle(fontSize: 11),
        ),
        trailing: Container(
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
          decoration: BoxDecoration(
            color: Colors.orange.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Text('Offense #${violation.offenseCount}',
              style: const TextStyle(
                  fontSize: 10, fontWeight: FontWeight.w600, color: Colors.orange)),
        ),
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Divider(),
                if (violation.remarks != null) ...[
                  const Text('Remarks',
                      style: TextStyle(fontWeight: FontWeight.w700, color: _navy, fontSize: 13)),
                  const SizedBox(height: 4),
                  Text(violation.remarks!, style: const TextStyle(fontSize: 13)),
                  const SizedBox(height: 8),
                ],
                Text('Reported By: ${violation.reportedBy ?? 'Unknown'}',
                    style: const TextStyle(fontSize: 12, color: Colors.black54)),
                const SizedBox(height: 14),

                // Action Chips
                const Text('Recommended Actions',
                    style: TextStyle(fontWeight: FontWeight.w700, color: _navy, fontSize: 13)),
                const SizedBox(height: 8),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    _actionChip('Behavior Contract', Icons.assignment_rounded,
                        () => _scheduleSession(violation, 'Behavior Contract')),
                    _actionChip('Parent Conference', Icons.people_rounded,
                        () => _scheduleSession(violation, 'Parent Conference')),
                    _actionChip('Community Service', Icons.cleaning_services_rounded,
                        () => _scheduleSession(violation, 'Community Service')),
                    _actionChip('Return to Good Standing', Icons.check_circle_rounded,
                        () => _clearViolation(violation)),
                  ],
                ),
                const SizedBox(height: 14),

                // Schedule Counseling Button
                SizedBox(
                  width: double.infinity,
                  height: 44,
                  child: ElevatedButton.icon(
                    onPressed: () => _scheduleCounseling(violation),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: _navy,
                      foregroundColor: Colors.white,
                      elevation: 2,
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                    ),
                    icon: const Icon(Icons.calendar_today_rounded, size: 16),
                    label: const Text('Schedule Counseling Session',
                        style: TextStyle(fontWeight: FontWeight.w600, fontSize: 13)),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _actionChip(String label, IconData icon, VoidCallback onPressed) {
    return ActionChip(
      avatar: Icon(icon, size: 15, color: _navy),
      label: Text(label,
          style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w600, color: _navy)),
      backgroundColor: _navy.withOpacity(0.07),
      side: BorderSide(color: _navy.withOpacity(0.2)),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      onPressed: onPressed,
    );
  }

  int _getCompletedCount(List<Violation> violations) =>
      violations.where((v) => v.status == ViolationStatus.cleared).length;

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

  void _scheduleCounseling(Violation violation) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Schedule Counseling Session',
            style: TextStyle(color: _navy, fontWeight: FontWeight.w700, fontSize: 16)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text('Student ID: ${violation.studentId}',
                style: const TextStyle(fontSize: 13, color: Colors.black54)),
            const SizedBox(height: 12),
            _dialogField('Session Date', Icons.calendar_today_rounded),
            const SizedBox(height: 8),
            _dialogField('Session Time', Icons.access_time_rounded),
            const SizedBox(height: 8),
            _dialogField('Notes', Icons.note_outlined, maxLines: 3),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.of(context).pop(),
              child: const Text('Cancel', style: TextStyle(color: Colors.black54))),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              _showSnack('Counseling session scheduled', Colors.green);
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: _navy,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            child: const Text('Schedule'),
          ),
        ],
      ),
    );
  }

  void _scheduleSession(Violation violation, String action) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Text('Schedule: $action',
            style: const TextStyle(color: _navy, fontWeight: FontWeight.w700, fontSize: 16)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            _dialogField('Implementation Date', Icons.calendar_today_rounded),
            const SizedBox(height: 8),
            _dialogField('Details/Instructions', Icons.note_outlined, maxLines: 3),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.of(context).pop(),
              child: const Text('Cancel', style: TextStyle(color: Colors.black54))),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              _showSnack('$action scheduled successfully', Colors.green);
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: _navy,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            child: const Text('Schedule'),
          ),
        ],
      ),
    );
  }

  Widget _dialogField(String label, IconData icon, {int maxLines = 1}) {
    return TextField(
      maxLines: maxLines,
      style: const TextStyle(fontSize: 13),
      decoration: InputDecoration(
        labelText: label,
        prefixIcon: Icon(icon, color: _navy, size: 18),
        filled: true,
        fillColor: const Color(0xFFF7F8FC),
        border: OutlineInputBorder(borderRadius: BorderRadius.circular(10),
            borderSide: const BorderSide(color: Color(0xFFDDE1EE))),
        enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10),
            borderSide: const BorderSide(color: Color(0xFFDDE1EE))),
        focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(10),
            borderSide: const BorderSide(color: _navy, width: 1.5)),
        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
      ),
    );
  }

  void _clearViolation(Violation violation) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: const Text('Clear Violation',
            style: TextStyle(color: _navy, fontWeight: FontWeight.w700)),
        content: const Text('Has the student completed all requirements and returned to good standing?'),
        actions: [
          TextButton(onPressed: () => Navigator.of(context).pop(false),
              child: const Text('Cancel', style: TextStyle(color: Colors.black54))),
          ElevatedButton(
            onPressed: () => Navigator.of(context).pop(true),
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.green,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            child: const Text('Confirm'),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      final vp = Provider.of<ViolationProvider>(context, listen: false);
      await vp.updateViolationStatus(violation.id, ViolationStatus.cleared);
      if (vp.error == null) _showSnack('Student returned to good standing', Colors.green);
    }
  }

  void _showSnack(String message, Color color) {
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(
      content: Text(message),
      backgroundColor: color,
      behavior: SnackBarBehavior.floating,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      margin: const EdgeInsets.all(16),
    ));
  }

  void _logout(BuildContext context) async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    await authProvider.logout();
    if (mounted) Navigator.of(context).pushReplacementNamed('/login');
  }
}