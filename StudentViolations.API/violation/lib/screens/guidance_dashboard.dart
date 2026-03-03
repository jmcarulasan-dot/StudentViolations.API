import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../providers/auth_provider.dart';
import '../providers/violation_provider.dart';
import '../models/violation.dart';

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
      appBar: AppBar(
        title: const Text('Guidance Dashboard'),
        backgroundColor: Colors.teal.shade800,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () => _logout(context),
          ),
        ],
      ),
      body: Consumer<ViolationProvider>(
        builder: (context, violationProvider, child) {
          if (violationProvider.isLoading) {
            return const Center(child: CircularProgressIndicator());
          }

          final referredCases = violationProvider.violations
              .where((v) => v.status == ViolationStatus.referredToGuidance)
              .toList();

          return Column(
            children: [
              // Welcome Section
              Container(
                padding: const EdgeInsets.all(16.0),
                child: Card(
                  color: Colors.teal.shade50,
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Row(
                      children: [
                        Icon(Icons.psychology, color: Colors.teal.shade800, size: 48),
                        const SizedBox(width: 16),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Welcome, ${Provider.of<AuthProvider>(context, listen: false).currentUser?.name}',
                                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              const Text('Guidance Office'),
                              const SizedBox(height: 8),
                              Text(
                                '${referredCases.length} cases referred for counseling',
                                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                  color: Colors.teal.shade800,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),

              // Statistics
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                child: Row(
                  children: [
                    Expanded(child: _buildStatCard('Referred Cases', referredCases.length, Icons.person, Colors.teal)),
                    const SizedBox(width: 8),
                    Expanded(child: _buildStatCard('Completed Sessions', _getCompletedSessionsCount(violationProvider.violations), Icons.check_circle, Colors.green)),
                  ],
                ),
              ),
              const SizedBox(height: 16),

              // Referred Cases List
              Expanded(
                child: Card(
                  margin: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Text(
                          'Referred Cases',
                          style: Theme.of(context).textTheme.titleLarge?.copyWith(
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                      Expanded(
                        child: referredCases.isEmpty
                            ? const Center(
                                child: Column(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Icon(Icons.inbox, size: 64, color: Colors.grey),
                                    SizedBox(height: 8),
                                    Text(
                                      'No referred cases',
                                      style: TextStyle(fontSize: 16, color: Colors.grey),
                                    ),
                                  ],
                                ),
                              )
                            : ListView.builder(
                                itemCount: referredCases.length,
                                itemBuilder: (context, index) {
                                  final violation = referredCases[index];
                                  return Card(
                                    margin: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 4.0),
                                    child: ExpansionTile(
                                      leading: CircleAvatar(
                                        backgroundColor: _getViolationTypeColor(violation.type),
                                        child: Icon(
                                          _getViolationTypeIcon(violation.type),
                                          color: Colors.white,
                                          size: 20,
                                        ),
                                      ),
                                      title: Text('Student ID: ${violation.studentId}'),
                                      subtitle: Text(
                                        'Violation: ${violation.violationDescription}\n'
                                        'Date: ${DateFormat('MMM dd, yyyy').format(violation.date)}\n'
                                        'Offense Count: ${violation.offenseCount}',
                                      ),
                                      trailing: Chip(
                                        label: Text('Offense #${violation.offenseCount}'),
                                        backgroundColor: Colors.orange.shade100,
                                      ),
                                      children: [
                                        Padding(
                                          padding: const EdgeInsets.all(16.0),
                                          child: Column(
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: [
                                              if (violation.remarks != null) ...[
                                                Text('Remarks:', style: Theme.of(context).textTheme.titleMedium),
                                                Text(violation.remarks!),
                                                const SizedBox(height: 12),
                                              ],
                                              Text('Reported By: ${violation.reportedBy ?? 'Unknown'}'),
                                              const SizedBox(height: 16),
                                              Text('Recommended Actions:', style: Theme.of(context).textTheme.titleMedium),
                                              const SizedBox(height: 8),
                                              Wrap(
                                                spacing: 8,
                                                runSpacing: 8,
                                                children: [
                                                  ActionChip(
                                                    avatar: const Icon(Icons.assignment, size: 18),
                                                    label: const Text('Behavior Contract'),
                                                    onPressed: () => _scheduleSession(violation, 'Behavior Contract'),
                                                  ),
                                                  ActionChip(
                                                    avatar: const Icon(Icons.people, size: 18),
                                                    label: const Text('Parent Conference'),
                                                    onPressed: () => _scheduleSession(violation, 'Parent Conference'),
                                                  ),
                                                  ActionChip(
                                                    avatar: const Icon(Icons.cleaning_services, size: 18),
                                                    label: const Text('Community Service'),
                                                    onPressed: () => _scheduleSession(violation, 'Community Service'),
                                                  ),
                                                  ActionChip(
                                                    avatar: const Icon(Icons.check_circle, size: 18),
                                                    label: const Text('Return to Good Standing'),
                                                    onPressed: () => _clearViolation(violation),
                                                  ),
                                                ],
                                              ),
                                              const SizedBox(height: 16),
                                              SizedBox(
                                                width: double.infinity,
                                                child: ElevatedButton(
                                                  onPressed: () => _scheduleCounseling(violation),
                                                  style: ElevatedButton.styleFrom(
                                                    backgroundColor: Colors.teal,
                                                    foregroundColor: Colors.white,
                                                  ),
                                                  child: const Text('Schedule Counseling Session'),
                                                ),
                                              ),
                                            ],
                                          ),
                                        ),
                                      ],
                                    ),
                                  );
                                },
                              ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }

  Widget _buildStatCard(String title, int count, IconData icon, Color color) {
    return Card(
      color: Colors.teal.shade50,
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            Icon(icon, color: color, size: 32),
            const SizedBox(height: 8),
            Text(
              count.toString(),
              style: TextStyle(
                fontSize: 24,
                fontWeight: FontWeight.bold,
                color: color,
              ),
            ),
            Text(
              title,
              style: TextStyle(
                fontSize: 12,
                color: Colors.teal.shade800,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  int _getCompletedSessionsCount(List<Violation> violations) {
    return violations.where((v) => v.status == ViolationStatus.cleared).length;
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

  void _scheduleCounseling(Violation violation) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Schedule Counseling Session'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text('Student ID: ${violation.studentId}'),
            Text('Violation: ${violation.violationDescription}'),
            const SizedBox(height: 16),
            const TextField(
              decoration: InputDecoration(
                labelText: 'Session Date',
                prefixIcon: Icon(Icons.calendar_today),
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 8),
            const TextField(
              decoration: InputDecoration(
                labelText: 'Session Time',
                prefixIcon: Icon(Icons.access_time),
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 8),
            const TextField(
              decoration: InputDecoration(
                labelText: 'Notes',
                border: OutlineInputBorder(),
              ),
              maxLines: 3,
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('Counseling session scheduled'),
                  backgroundColor: Colors.green,
                ),
              );
            },
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
        title: Text('Schedule: $action'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text('Student ID: ${violation.studentId}'),
            Text('Action: $action'),
            const SizedBox(height: 16),
            const TextField(
              decoration: InputDecoration(
                labelText: 'Implementation Date',
                prefixIcon: Icon(Icons.calendar_today),
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 8),
            const TextField(
              decoration: InputDecoration(
                labelText: 'Details/Instructions',
                border: OutlineInputBorder(),
              ),
              maxLines: 3,
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () {
              Navigator.of(context).pop();
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content: Text('$action scheduled successfully'),
                  backgroundColor: Colors.green,
                ),
              );
            },
            child: const Text('Schedule'),
          ),
        ],
      ),
    );
  }

  void _clearViolation(Violation violation) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Clear Violation'),
        content: const Text('Has the student completed all requirements and returned to good standing?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('Cancel'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: const Text('Confirm'),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      final violationProvider = Provider.of<ViolationProvider>(context, listen: false);
      await violationProvider.updateViolationStatus(violation.id, ViolationStatus.cleared);
      
      if (violationProvider.error == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Student returned to good standing'),
            backgroundColor: Colors.green,
          ),
        );
      }
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
