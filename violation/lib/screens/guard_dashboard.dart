import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../providers/auth_provider.dart';
import '../providers/violation_provider.dart';
import '../models/violation.dart';
import '../models/user.dart';

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
      appBar: AppBar(
        title: const Text('Guard Dashboard'),
        backgroundColor: Colors.blue.shade800,
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

          return Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Welcome Message
                Card(
                  child: Padding(
                    padding: const EdgeInsets.all(16.0),
                    child: Row(
                      children: [
                        Icon(Icons.security, color: Colors.blue.shade800, size: 32),
                        const SizedBox(width: 12),
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
                              const Text('Record student violations at the gate'),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Violation Recording Section
                Expanded(
                  flex: 2,
                  child: Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16.0),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Record Violation',
                            style: Theme.of(context).textTheme.titleLarge?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          const SizedBox(height: 16),

                          // Student Selection
                          Text('Select Student:', style: Theme.of(context).textTheme.titleMedium),
                          const SizedBox(height: 8),
                          DropdownButtonFormField<User>(
                            value: _selectedStudent,
                            decoration: const InputDecoration(
                              border: OutlineInputBorder(),
                              prefixIcon: Icon(Icons.person_search),
                            ),
                            items: violationProvider.students.map((student) {
                              return DropdownMenuItem(
                                value: student,
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Text(student.name),
                                    Text(
                                      '${student.gradeSection}',
                                      style: Theme.of(context).textTheme.bodySmall,
                                    ),
                                  ],
                                ),
                              );
                            }).toList(),
                            onChanged: (value) {
                              setState(() {
                                _selectedStudent = value;
                              });
                            },
                          ),
                          const SizedBox(height: 16),

                          // Violation Type Selection
                          Text('Violation Type:', style: Theme.of(context).textTheme.titleMedium),
                          const SizedBox(height: 8),
                          Wrap(
                            spacing: 8,
                            runSpacing: 8,
                            children: ViolationType.values.map((type) {
                              return FilterChip(
                                label: Text(_getViolationTypeLabel(type)),
                                selected: _selectedViolationType == type,
                                onSelected: (selected) {
                                  setState(() {
                                    _selectedViolationType = selected ? type : null;
                                  });
                                },
                                avatar: Icon(_getViolationTypeIcon(type)),
                              );
                            }).toList(),
                          ),
                          const SizedBox(height: 16),

                          // Remarks
                          TextFormField(
                            controller: _remarksController,
                            decoration: const InputDecoration(
                              labelText: 'Remarks (Optional)',
                              border: OutlineInputBorder(),
                              prefixIcon: Icon(Icons.note),
                            ),
                            maxLines: 2,
                          ),
                          const SizedBox(height: 16),

                          // Submit Button
                          SizedBox(
                            width: double.infinity,
                            child: ElevatedButton(
                              onPressed: _selectedStudent != null && _selectedViolationType != null
                                  ? () => _recordViolation(context)
                                  : null,
                              style: ElevatedButton.styleFrom(
                                backgroundColor: Colors.red.shade600,
                                foregroundColor: Colors.white,
                                padding: const EdgeInsets.symmetric(vertical: 16),
                              ),
                              child: const Text('Record Violation'),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 16),

                // Recent Violations
                Expanded(
                  flex: 3,
                  child: Card(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Padding(
                          padding: const EdgeInsets.all(16.0),
                          child: Text(
                            'Recent Violations',
                            style: Theme.of(context).textTheme.titleLarge?.copyWith(
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                        ),
                        Expanded(
                          child: violationProvider.violations.isEmpty
                              ? const Center(child: Text('No violations recorded'))
                              : ListView.builder(
                                  itemCount: violationProvider.violations.length,
                                  itemBuilder: (context, index) {
                                    final violation = violationProvider.violations[index];
                                    return ListTile(
                                      leading: Icon(
                                        _getViolationTypeIcon(violation.type),
                                        color: Colors.red.shade600,
                                      ),
                                      title: Text(violation.violationDescription),
                                      subtitle: Text(
                                        'Student ID: ${violation.studentId}\n'
                                        'Date: ${DateFormat('MMM dd, yyyy').format(violation.date)}\n'
                                        'Status: ${violation.statusDescription}',
                                      ),
                                      trailing: Chip(
                                        label: Text('Offense #${violation.offenseCount}'),
                                        backgroundColor: Colors.orange.shade100,
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
            ),
          );
        },
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
        const SnackBar(
          content: Text('Violation recorded successfully'),
          backgroundColor: Colors.green,
        ),
      );
      
      // Reset form
      setState(() {
        _selectedStudent = null;
        _selectedViolationType = null;
        _remarksController.clear();
      });
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(violationProvider.error!),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  void _logout(BuildContext context) async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    await authProvider.logout();
    if (mounted) {
      Navigator.of(context).pushReplacementNamed('/login');
    }
  }

  String _getViolationTypeLabel(ViolationType type) {
    switch (type) {
      case ViolationType.noId:
        return 'No ID';
      case ViolationType.noUniform:
        return 'No Uniform';
      case ViolationType.piercing:
        return 'Piercing';
      case ViolationType.coloredHair:
        return 'Colored Hair';
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
}
