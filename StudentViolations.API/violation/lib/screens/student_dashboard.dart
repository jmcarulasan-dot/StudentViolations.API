import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../providers/auth_provider.dart';
import '../providers/violation_provider.dart';
import '../models/violation.dart';

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
      appBar: AppBar(
        title: const Text('Student Dashboard'),
        backgroundColor: Colors.green.shade800,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () => _logout(context),
          ),
        ],
      ),
      body: Consumer2<AuthProvider, ViolationProvider>(
        builder: (context, authProvider, violationProvider, child) {
          final currentUser = authProvider.currentUser;
          final violations = violationProvider.violations;

          if (violationProvider.isLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Student Info Card
                Card(
                  color: Colors.green.shade50,
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Row(
                      children: [
                        CircleAvatar(
                          radius: 32,
                          backgroundColor: Colors.green.shade800,
                          child: Icon(
                            Icons.school,
                            color: Colors.white,
                            size: 32,
                          ),
                        ),
                        const SizedBox(width: 16),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                currentUser?.name ?? 'Student',
                                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              Text(
                                currentUser?.gradeSection ?? 'No Grade/Section',
                                style: Theme.of(context).textTheme.titleMedium,
                              ),
                              Text(
                                'ID: ${currentUser?.id ?? 'N/A'}',
                                style: Theme.of(context).textTheme.bodyMedium,
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Violation Summary
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Violation Summary',
                          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 12),
                        _buildViolationStats(violations),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Current Status
                if (violations.isNotEmpty) ...[
                  Card(
                    color: Colors.green.shade50,
                    child: Padding(
                      padding: const EdgeInsets.all(16.0),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              Icon(
                                _getStatusIcon(violations.first),
                                color: Colors.green.shade800,
                              ),
                              const SizedBox(width: 8),
                              Text(
                                'Current Status',
                                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ],
                          ),
                          const SizedBox(height: 8),
                          Text(
                            violations.first.statusDescription,
                            style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                              color: Colors.green.shade800,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            _getStatusDescription(violations.first),
                            style: Theme.of(context).textTheme.bodyMedium,
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                ],

                // Violation History
                Card(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Text(
                          'Violation History',
                          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      violations.isEmpty
                          ? const Padding(
                              padding: EdgeInsets.all(16.0),
                              child: Center(
                                child: Column(
                                  children: [
                                    Icon(
                                      Icons.check_circle,
                                      size: 64,
                                      color: Colors.green,
                                    ),
                                    SizedBox(height: 8),
                                    Text(
                                      'No violations recorded',
                                      style: TextStyle(
                                        fontSize: 16,
                                        fontWeight: FontWeight.w500,
                                        color: Colors.green,
                                      ),
                                    ),
                                    Text(
                                      'Keep up the good work!',
                                      style: TextStyle(color: Colors.grey),
                                    ),
                                  ],
                                ),
                              ),
                            )
                          : ListView.builder(
                              shrinkWrap: true,
                              physics: const NeverScrollableScrollPhysics(),
                              itemCount: violations.length,
                              itemBuilder: (context, index) {
                                final violation = violations[index];
                                return ListTile(
                                  leading: CircleAvatar(
                                    backgroundColor: _getViolationTypeColor(violation.type),
                                    child: Icon(
                                      _getViolationTypeIcon(violation.type),
                                      color: Colors.white,
                                      size: 20,
                                    ),
                                  ),
                                  title: Text(violation.violationDescription),
                                  subtitle: Text(
                                    'Date: ${DateFormat('MMM dd, yyyy').format(violation.date)}\n'
                                    'Offense: #${violation.offenseCount}',
                                  ),
                                  trailing: Chip(
                                    label: Text(violation.statusDescription),
                                    backgroundColor: Colors.green.shade100,
                                  ),
                                );
                              },
                            ),
                    ],
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildViolationStats(List<Violation> violations) {
    final noIdCount = violations.where((v) => v.type == ViolationType.noId).length;
    final noUniformCount = violations.where((v) => v.type == ViolationType.noUniform).length;
    final piercingCount = violations.where((v) => v.type == ViolationType.piercing).length;
    final coloredHairCount = violations.where((v) => v.type == ViolationType.coloredHair).length;

    return Column(
      children: [
        Row(
          children: [
            Expanded(child: _buildStatCard('No ID', noIdCount, Icons.badge, Colors.red)),
            const SizedBox(width: 8),
            Expanded(child: _buildStatCard('No Uniform', noUniformCount, Icons.person_off, Colors.orange)),
          ],
        ),
        const SizedBox(height: 8),
        Row(
          children: [
            Expanded(child: _buildStatCard('Piercing', piercingCount, Icons.diamond, Colors.purple)),
            const SizedBox(width: 8),
            Expanded(child: _buildStatCard('Colored Hair', coloredHairCount, Icons.face, Colors.blue)),
          ],
        ),
      ],
    );
  }

  Widget _buildStatCard(String title, int count, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.green.shade50,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: Colors.green.shade200),
      ),
      child: Column(
        children: [
          Icon(icon, color: color, size: 24),
          const SizedBox(height: 4),
          Text(
            count.toString(),
            style: TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.bold,
              color: color,
            ),
          ),
          Text(
            title,
            style: TextStyle(
              fontSize: 12,
              color: Colors.green.shade800,
            ),
          ),
        ],
      ),
    );
  }

  Color _getStatusColor(Violation violation) {
    switch (violation.status) {
      case ViolationStatus.warning:
        return Colors.yellow;
      case ViolationStatus.parentNotified:
        return Colors.orange;
      case ViolationStatus.referredToSAO:
        return Colors.red;
      case ViolationStatus.referredToGuidance:
        return Colors.purple;
      case ViolationStatus.disciplinaryAction:
        return Colors.red.shade900;
      default:
        return Colors.green;
    }
  }

  IconData _getStatusIcon(Violation violation) {
    switch (violation.status) {
      case ViolationStatus.warning:
        return Icons.warning;
      case ViolationStatus.parentNotified:
        return Icons.phone;
      case ViolationStatus.referredToSAO:
        return Icons.admin_panel_settings;
      case ViolationStatus.referredToGuidance:
        return Icons.psychology;
      case ViolationStatus.disciplinaryAction:
        return Icons.gavel;
      default:
        return Icons.check_circle;
    }
  }

  String _getStatusDescription(Violation violation) {
    switch (violation.status) {
      case ViolationStatus.warning:
        return 'This is your first offense. Please comply with school rules.';
      case ViolationStatus.parentNotified:
        return 'Your parents/guardians have been notified. Please correct this immediately.';
      case ViolationStatus.referredToSAO:
        return 'You have been referred to the Student Affairs Office. Please report to the SAO office.';
      case ViolationStatus.referredToGuidance:
        return 'You have been referred to the Guidance Office for counseling.';
      case ViolationStatus.disciplinaryAction:
        return 'Disciplinary action has been taken. Please follow the instructions given.';
      default:
        return 'No action required.';
    }
  }

  Color _getViolationTypeColor(ViolationType type) {
    switch (type) {
      case ViolationType.noId:
        return Colors.red;
      case ViolationType.noUniform:
        return Colors.orange;
      case ViolationType.piercing:
        return Colors.purple;
      case ViolationType.coloredHair:
        return Colors.blue;
    }
  }

  IconData _getViolationTypeIcon(ViolationType type) {
    switch (type) {
      case ViolationType.noId:
        return Icons.badge;
      case ViolationType.noUniform:
        return Icons.person_off;
      case ViolationType.piercing:
        return Icons.diamond;
      case ViolationType.coloredHair:
        return Icons.face;
    }
  }

  void _logout(BuildContext context) async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    await authProvider.logout();
    if (mounted) {
      Navigator.of(context).pushReplacementNamed('/login');
    }
  }
}
