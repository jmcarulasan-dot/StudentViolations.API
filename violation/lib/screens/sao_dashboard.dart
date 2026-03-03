import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../providers/auth_provider.dart';
import '../providers/violation_provider.dart';
import '../models/violation.dart';

const _red  = Color(0xFFFD070C);
const _navy = Color(0xFF0F136E);

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
      backgroundColor: const Color(0xFFF5F7FA),
      appBar: AppBar(
        title: const Text('SAO Dashboard'),
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

          final filtered = _filterViolations(violationProvider.violations);

          return Column(
            children: [
              // Summary Cards
              Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  children: [
                    Row(
                      children: [
                        Expanded(child: _buildSummaryCard('Total', violationProvider.violations.length, Icons.warning_rounded, _red)),
                        const SizedBox(width: 10),
                        Expanded(child: _buildSummaryCard('Pending', _getPendingCount(violationProvider.violations), Icons.pending_rounded, Colors.orange)),
                      ],
                    ),
                    const SizedBox(height: 10),
                    Row(
                      children: [
                        Expanded(child: _buildSummaryCard('To Guidance', _getReferredToGuidanceCount(violationProvider.violations), Icons.psychology_rounded, Colors.purple)),
                        const SizedBox(width: 10),
                        Expanded(child: _buildSummaryCard('Cleared Today', _getClearedTodayCount(violationProvider.violations), Icons.check_circle_rounded, Colors.green)),
                      ],
                    ),
                  ],
                ),
              ),

              // Filter Chips
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                child: SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      _filterChip('All', 'all'),
                      const SizedBox(width: 8),
                      _filterChip('Warning', 'warning'),
                      const SizedBox(width: 8),
                      _filterChip('Parent Notified', 'parentNotified'),
                      const SizedBox(width: 8),
                      _filterChip('Referred to SAO', 'referredToSAO'),
                      const SizedBox(width: 8),
                      _filterChip('To Guidance', 'referredToGuidance'),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 12),

              // Violations List
              Expanded(
                child: filtered.isEmpty
                    ? const Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Icon(Icons.inbox_rounded, size: 56, color: Colors.grey),
                            SizedBox(height: 8),
                            Text('No violations found',
                                style: TextStyle(color: Colors.grey, fontSize: 14)),
                          ],
                        ),
                      )
                    : ListView.builder(
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        itemCount: filtered.length,
                        itemBuilder: (context, index) {
                          return _buildViolationCard(filtered[index]);
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
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        boxShadow: [
          BoxShadow(
            color: color.withOpacity(0.12),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
        border: Border.all(color: color.withOpacity(0.15)),
      ),
      child: Column(
        children: [
          Icon(icon, color: color, size: 28),
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

  Widget _filterChip(String label, String value) {
    final selected = _selectedFilter == value;
    return GestureDetector(
      onTap: () => setState(() => _selectedFilter = value),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 180),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
        decoration: BoxDecoration(
          color: selected ? _navy : Colors.white,
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: selected ? _navy : Colors.grey.shade300),
          boxShadow: selected
              ? [BoxShadow(color: _navy.withOpacity(0.2), blurRadius: 6, offset: const Offset(0, 2))]
              : [],
        ),
        child: Text(
          label,
          style: TextStyle(
            fontSize: 12,
            fontWeight: FontWeight.w600,
            color: selected ? Colors.white : Colors.black54,
          ),
        ),
      ),
    );
  }

  Widget _buildViolationCard(Violation violation) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        boxShadow: [
          BoxShadow(
            color: _navy.withOpacity(0.07),
            blurRadius: 8,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: ExpansionTile(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
        leading: CircleAvatar(
          backgroundColor: _getViolationTypeColor(violation.type).withOpacity(0.15),
          child: Icon(_getViolationTypeIcon(violation.type),
              color: _getViolationTypeColor(violation.type), size: 20),
        ),
        title: Text(violation.violationDescription,
            style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600)),
        subtitle: Text(
          'ID: ${violation.studentId}  •  ${DateFormat('MMM dd, yyyy').format(violation.date)}  •  Offense #${violation.offenseCount}',
          style: const TextStyle(fontSize: 11),
        ),
        trailing: Container(
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
          decoration: BoxDecoration(
            color: Colors.green.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Text(violation.statusDescription,
              style: const TextStyle(
                  fontSize: 10, fontWeight: FontWeight.w600, color: Colors.green)),
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
                Row(
                  children: [
                    if (violation.status == ViolationStatus.referredToSAO) ...[
                      Expanded(
                        child: _actionButton(
                          'Refer to Guidance',
                          Icons.psychology_rounded,
                          Colors.purple,
                          () => _referToGuidance(violation),
                        ),
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: _actionButton(
                          'Clear',
                          Icons.check_circle_rounded,
                          Colors.green,
                          () => _clearViolation(violation),
                        ),
                      ),
                    ] else if (violation.status == ViolationStatus.parentNotified) ...[
                      Expanded(
                        child: _actionButton(
                          'Confirm Parent Contact',
                          Icons.phone_rounded,
                          Colors.orange,
                          () => _confirmParentNotification(violation),
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
  }

  Widget _actionButton(String label, IconData icon, Color color, VoidCallback onPressed) {
    return ElevatedButton.icon(
      onPressed: onPressed,
      style: ElevatedButton.styleFrom(
        backgroundColor: color,
        foregroundColor: Colors.white,
        elevation: 2,
        padding: const EdgeInsets.symmetric(vertical: 10),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      ),
      icon: Icon(icon, size: 16),
      label: Text(label, style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w600)),
    );
  }

  List<Violation> _filterViolations(List<Violation> violations) {
    switch (_selectedFilter) {
      case 'warning':           return violations.where((v) => v.status == ViolationStatus.warning).toList();
      case 'parentNotified':    return violations.where((v) => v.status == ViolationStatus.parentNotified).toList();
      case 'referredToSAO':     return violations.where((v) => v.status == ViolationStatus.referredToSAO).toList();
      case 'referredToGuidance':return violations.where((v) => v.status == ViolationStatus.referredToGuidance).toList();
      default:                  return violations;
    }
  }

  int _getPendingCount(List<Violation> v) => v.where((x) =>
      x.status == ViolationStatus.warning ||
      x.status == ViolationStatus.parentNotified ||
      x.status == ViolationStatus.referredToSAO).length;

  int _getReferredToGuidanceCount(List<Violation> v) =>
      v.where((x) => x.status == ViolationStatus.referredToGuidance).length;

  int _getClearedTodayCount(List<Violation> v) {
    final today = DateTime.now();
    return v.where((x) =>
      x.status == ViolationStatus.cleared &&
      x.date.year == today.year &&
      x.date.month == today.month &&
      x.date.day == today.day).length;
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

  void _referToGuidance(Violation violation) async {
    final confirmed = await _confirm('Refer to Guidance',
        'Are you sure you want to refer this case to the Guidance Office?');
    if (confirmed) {
      final vp = Provider.of<ViolationProvider>(context, listen: false);
      await vp.updateViolationStatus(violation.id, ViolationStatus.referredToGuidance);
      if (vp.error == null) _showSnack('Case referred to Guidance Office', Colors.green);
    }
  }

  void _clearViolation(Violation violation) async {
    final confirmed = await _confirm('Clear Violation', 'Are you sure you want to clear this violation?');
    if (confirmed) {
      final vp = Provider.of<ViolationProvider>(context, listen: false);
      await vp.updateViolationStatus(violation.id, ViolationStatus.cleared);
      if (vp.error == null) _showSnack('Violation cleared', Colors.green);
    }
  }

  void _confirmParentNotification(Violation violation) async {
    final confirmed = await _confirm('Confirm Parent Contact', 'Have you contacted the parents/guardians?');
    if (confirmed) {
      final vp = Provider.of<ViolationProvider>(context, listen: false);
      await vp.updateViolationStatus(violation.id, ViolationStatus.referredToSAO);
      if (vp.error == null) _showSnack('Parent contact confirmed. Case escalated to SAO.', Colors.green);
    }
  }

  Future<bool> _confirm(String title, String message) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        title: Text(title, style: const TextStyle(color: _navy, fontWeight: FontWeight.w700)),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('Cancel', style: TextStyle(color: Colors.black54)),
          ),
          ElevatedButton(
            onPressed: () => Navigator.of(context).pop(true),
            style: ElevatedButton.styleFrom(
              backgroundColor: _navy,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
            ),
            child: const Text('Confirm'),
          ),
        ],
      ),
    );
    return result ?? false;
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