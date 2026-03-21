import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../providers/auth_provider.dart';
import '../providers/violation_provider.dart';
import '../models/violation.dart';

class SAODashboard extends StatefulWidget {
  const SAODashboard({super.key});

  @override
  State<SAODashboard> createState() => _SAODashboardState();
}

class _SAODashboardState extends State<SAODashboard> {
  String _selectedFilter = 'all';

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
        title: const Text('SAO Dashboard'),
        backgroundColor: Colors.purple.shade800,
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

          final filteredViolations = _filterViolations(violationProvider.violations);

          return Column(
            children: [
              // Summary Cards
              Container(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  children: [
                    Row(
                      children: [
                        Expanded(child: _buildSummaryCard('Total Violations', violationProvider.violations.length, Icons.warning, Colors.red)),
                        const SizedBox(width: 8),
                        Expanded(child: _buildSummaryCard('Pending Cases', _getPendingCount(violationProvider.violations), Icons.pending, Colors.orange)),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Row(
                      children: [
                        Expanded(child: _buildSummaryCard('Referred to Guidance', _getReferredToGuidanceCount(violationProvider.violations), Icons.psychology, Colors.purple)),
                        const SizedBox(width: 8),
                        Expanded(child: _buildSummaryCard('Cleared Today', _getClearedTodayCount(violationProvider.violations), Icons.check_circle, Colors.green)),
                      ],
                    ),
                  ],
                ),
              ),

              // Filter Chips
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                child: SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      FilterChip(
                        label: const Text('All'),
                        selected: _selectedFilter == 'all',
                        onSelected: (selected) => setState(() => _selectedFilter = 'all'),
                      ),
                      const SizedBox(width: 8),
                      FilterChip(
                        label: const Text('Warning'),
                        selected: _selectedFilter == 'warning',
                        onSelected: (selected) => setState(() => _selectedFilter = 'warning'),
                      ),
                      const SizedBox(width: 8),
                      FilterChip(
                        label: const Text('Parent Notified'),
                        selected: _selectedFilter == 'parentNotified',
                        onSelected: (selected) => setState(() => _selectedFilter = 'parentNotified'),
                      ),
                      const SizedBox(width: 8),
                      FilterChip(
                        label: const Text('Referred to SAO'),
                        selected: _selectedFilter == 'referredToSAO',
                        onSelected: (selected) => setState(() => _selectedFilter = 'referredToSAO'),
                      ),
                      const SizedBox(width: 8),
                      FilterChip(
                        label: const Text('Referred to Guidance'),
                        selected: _selectedFilter == 'referredToGuidance',
                        onSelected: (selected) => setState(() => _selectedFilter = 'referredToGuidance'),
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 8),

              // Violations List
              Expanded(
                child: filteredViolations.isEmpty
                    ? const Center(child: Text('No violations found'))
                    : ListView.builder(
                        itemCount: filteredViolations.length,
                        itemBuilder: (context, index) {
                          final violation = filteredViolations[index];
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
                              title: Text(violation.violationDescription),
                              subtitle: Text(
                                'Student ID: ${violation.studentId}\n'
                                'Date: ${DateFormat('MMM dd, yyyy').format(violation.date)}\n'
                                'Offense: #${violation.offenseCount}',
                              ),
                              trailing: Chip(
                                label: Text(violation.statusDescription),
                                backgroundColor: Colors.green.shade100,
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
                                        const SizedBox(height: 8),
                                      ],
                                      Text('Reported By: ${violation.reportedBy ?? 'Unknown'}'),
                                      const SizedBox(height: 16),
                                      Row(
                                        children: [
                                          if (violation.status == ViolationStatus.referredToSAO) ...[
                                            Expanded(
                                              child: ElevatedButton(
                                                onPressed: () => _referToGuidance(violation),
                                                style: ElevatedButton.styleFrom(
                                                  backgroundColor: Colors.purple,
                                                  foregroundColor: Colors.white,
                                                ),
                                                child: const Text('Refer to Guidance'),
                                              ),
                                            ),
                                            const SizedBox(width: 8),
                                            Expanded(
                                              child: ElevatedButton(
                                                onPressed: () => _clearViolation(violation),
                                                style: ElevatedButton.styleFrom(
                                                  backgroundColor: Colors.green,
                                                  foregroundColor: Colors.white,
                                                ),
                                                child: const Text('Clear Violation'),
                                              ),
                                            ),
                                          ] else if (violation.status == ViolationStatus.parentNotified) ...[
                                            Expanded(
                                              child: ElevatedButton(
                                                onPressed: () => _confirmParentNotification(violation),
                                                style: ElevatedButton.styleFrom(
                                                  backgroundColor: Colors.orange,
                                                  foregroundColor: Colors.white,
                                                ),
                                                child: const Text('Confirm Parent Contact'),
                                              ),
                                            ),
                                          ],
                                        ],
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
          );
        },
      ),
    );
  }

  Widget _buildSummaryCard(String title, int count, IconData icon, Color color) {
    return Card(
      color: Colors.red.shade50,
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
                color: Colors.red.shade800,
              ),
              textAlign: TextAlign.center,
            ),
          ],
        ),
      ),
    );
  }

  List<Violation> _filterViolations(List<Violation> violations) {
    switch (_selectedFilter) {
      case 'warning':
        return violations.where((v) => v.status == ViolationStatus.warning).toList();
      case 'parentNotified':
        return violations.where((v) => v.status == ViolationStatus.parentNotified).toList();
      case 'referredToSAO':
        return violations.where((v) => v.status == ViolationStatus.referredToSAO).toList();
      case 'referredToGuidance':
        return violations.where((v) => v.status == ViolationStatus.referredToGuidance).toList();
      default:
        return violations;
    }
  }

  int _getPendingCount(List<Violation> violations) {
    return violations.where((v) => 
      v.status == ViolationStatus.warning || 
      v.status == ViolationStatus.parentNotified ||
      v.status == ViolationStatus.referredToSAO
    ).length;
  }

  int _getReferredToGuidanceCount(List<Violation> violations) {
    return violations.where((v) => v.status == ViolationStatus.referredToGuidance).length;
  }

  int _getClearedTodayCount(List<Violation> violations) {
    final today = DateTime.now();
    return violations.where((v) => 
      v.status == ViolationStatus.cleared &&
      v.date.year == today.year &&
      v.date.month == today.month &&
      v.date.day == today.day
    ).length;
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

  Color _getStatusColor(ViolationStatus status) {
    switch (status) {
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
      case ViolationStatus.cleared:
        return Colors.green;
      default:
        return Colors.grey;
    }
  }

  void _referToGuidance(Violation violation) async {
    final confirmed = await _showConfirmationDialog(
      context,
      'Refer to Guidance',
      'Are you sure you want to refer this case to the Guidance Office?',
    );

    if (confirmed) {
      final violationProvider = Provider.of<ViolationProvider>(context, listen: false);
      await violationProvider.updateViolationStatus(violation.id, ViolationStatus.referredToGuidance);
      
      if (violationProvider.error == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Case referred to Guidance Office'),
            backgroundColor: Colors.green,
          ),
        );
      }
    }
  }

  void _clearViolation(Violation violation) async {
    final confirmed = await _showConfirmationDialog(
      context,
      'Clear Violation',
      'Are you sure you want to clear this violation?',
    );

    if (confirmed) {
      final violationProvider = Provider.of<ViolationProvider>(context, listen: false);
      await violationProvider.updateViolationStatus(violation.id, ViolationStatus.cleared);
      
      if (violationProvider.error == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Violation cleared'),
            backgroundColor: Colors.green,
          ),
        );
      }
    }
  }

  void _confirmParentNotification(Violation violation) async {
    final confirmed = await _showConfirmationDialog(
      context,
      'Confirm Parent Contact',
      'Have you contacted the parents/guardians?',
    );

    if (confirmed) {
      final violationProvider = Provider.of<ViolationProvider>(context, listen: false);
      await violationProvider.updateViolationStatus(violation.id, ViolationStatus.referredToSAO);
      
      if (violationProvider.error == null) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Parent contact confirmed. Case escalated to SAO.'),
            backgroundColor: Colors.green,
          ),
        );
      }
    }
  }

  Future<bool> _showConfirmationDialog(BuildContext context, String title, String message) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(title),
        content: Text(message),
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
    return result ?? false;
  }

  void _logout(BuildContext context) async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    await authProvider.logout();
    if (mounted) {
      Navigator.of(context).pushReplacementNamed('/login');
    }
  }
}
