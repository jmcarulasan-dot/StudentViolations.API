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
  String _selectedFilter = 'referred';

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
        title: const Text('SAO Dashboard',
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

          final all = violationProvider.violations;
          final filtered = _filterViolations(all);

          return SingleChildScrollView(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // ── Welcome Card ──────────────────────────────────────
                _buildWelcomeCard(context, all),
                const SizedBox(height: 16),

                // ── Summary Cards ─────────────────────────────────────
                Row(
                  children: [
                    Expanded(child: _buildStatCard('Referred\nto SAO', _getReferredCount(all), Icons.inbox_rounded, _red)),
                    const SizedBox(width: 10),
                    Expanded(child: _buildStatCard('Disciplinary\nAction', _getDisciplinaryCount(all), Icons.gavel_rounded, Colors.orange)),
                    const SizedBox(width: 10),
                    Expanded(child: _buildStatCard('Cleared', _getClearedCount(all), Icons.check_circle_rounded, Colors.green)),
                    const SizedBox(width: 10),
                    Expanded(child: _buildStatCard('Total\nAll', all.length, Icons.summarize_rounded, _navy)),
                  ],
                ),
                const SizedBox(height: 16),

                // ── Notice ────────────────────────────────────────────
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: _navy.withOpacity(0.06),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: _navy.withOpacity(0.15)),
                  ),
                  child: const Row(
                    children: [
                      Icon(Icons.info_outline_rounded, color: _navy, size: 18),
                      SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          'SAO handles cases escalated by the Guidance Office. You are the highest authority.',
                          style: TextStyle(fontSize: 12, color: _navy, fontWeight: FontWeight.w500),
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 14),

                // ── Filter Chips ──────────────────────────────────────
                SingleChildScrollView(
                  scrollDirection: Axis.horizontal,
                  child: Row(
                    children: [
                      _filterChip('Referred to SAO', 'referred'),
                      const SizedBox(width: 8),
                      _filterChip('Disciplinary Action', 'disciplinary'),
                      const SizedBox(width: 8),
                      _filterChip('Cleared', 'cleared'),
                      const SizedBox(width: 8),
                      _filterChip('All Cases', 'all'),
                    ],
                  ),
                ),
                const SizedBox(height: 14),

                // ── Cases List ────────────────────────────────────────
                filtered.isEmpty
                    ? Container(
                        width: double.infinity,
                        padding: const EdgeInsets.symmetric(vertical: 48),
                        decoration: BoxDecoration(
                          color: Colors.white,
                          borderRadius: BorderRadius.circular(16),
                        ),
                        child: const Column(
                          children: [
                            Icon(Icons.inbox_rounded, size: 56, color: Colors.grey),
                            SizedBox(height: 10),
                            Text('No cases found',
                                style: TextStyle(color: Colors.grey, fontSize: 14)),
                          ],
                        ),
                      )
                    : ListView.builder(
                        shrinkWrap: true,
                        physics: const NeverScrollableScrollPhysics(),
                        itemCount: filtered.length,
                        itemBuilder: (context, index) =>
                            _buildViolationCard(filtered[index]),
                      ),
                const SizedBox(height: 16),
              ],
            ),
          );
        },
      ),
    );
  }

  // ── Welcome Card ────────────────────────────────────────────────────────────
  Widget _buildWelcomeCard(BuildContext context, List<Violation> all) {
    final name = Provider.of<AuthProvider>(context, listen: false).currentUser?.name ?? 'SAO';
    final referredCount = _getReferredCount(all);
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
            child: const Icon(Icons.admin_panel_settings_rounded, color: Colors.white, size: 30),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Welcome, $name!',
                    style: const TextStyle(
                        fontSize: 17, fontWeight: FontWeight.w800, color: Colors.white)),
                const SizedBox(height: 2),
                const Text('Student Affairs Office — Highest Authority',
                    style: TextStyle(fontSize: 12, color: Colors.white60)),
                if (referredCount > 0) ...[
                  const SizedBox(height: 6),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                    decoration: BoxDecoration(
                      color: _red.withOpacity(0.25),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Text(
                        '$referredCount case${referredCount > 1 ? 's' : ''} awaiting SAO decision!',
                        style: const TextStyle(
                            fontSize: 12, color: Colors.white, fontWeight: FontWeight.w700)),
                  ),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }

  // ── Stat Card ───────────────────────────────────────────────────────────────
  Widget _buildStatCard(String title, int count, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 14, horizontal: 8),
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
          Icon(icon, color: color, size: 24),
          const SizedBox(height: 4),
          Text(count.toString(),
              style: TextStyle(fontSize: 20, fontWeight: FontWeight.w800, color: color)),
          Text(title,
              style: TextStyle(fontSize: 9, color: color.withOpacity(0.8)),
              textAlign: TextAlign.center),
        ],
      ),
    );
  }

  // ── Filter Chip ─────────────────────────────────────────────────────────────
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
        child: Text(label,
            style: TextStyle(
                fontSize: 12,
                fontWeight: FontWeight.w600,
                color: selected ? Colors.white : Colors.black54)),
      ),
    );
  }

  // ── Violation Card ──────────────────────────────────────────────────────────
  Widget _buildViolationCard(Violation violation) {
    final statusColor = _getStatusColor(violation.status);
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        boxShadow: [
          BoxShadow(color: _navy.withOpacity(0.07), blurRadius: 8, offset: const Offset(0, 3)),
        ],
        border: Border.all(color: statusColor.withOpacity(0.2)),
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
            color: statusColor.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Text(violation.statusDescription,
              style: TextStyle(
                  fontSize: 10, fontWeight: FontWeight.w700, color: statusColor)),
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

                // ── SAO Actions ───────────────────────────────────────
                if (violation.status == ViolationStatus.referredToSAO) ...[
                  const Text('SAO Actions',
                      style: TextStyle(fontWeight: FontWeight.w700, color: _navy, fontSize: 13)),
                  const SizedBox(height: 10),
                  Row(
                    children: [
                      Expanded(
                        child: _actionButton(
                          'Disciplinary Action',
                          Icons.gavel_rounded,
                          Colors.orange,
                          () => _takeDisciplinaryAction(violation),
                        ),
                      ),
                      const SizedBox(width: 8),
                      Expanded(
                        child: _actionButton(
                          'Clear Case',
                          Icons.check_circle_rounded,
                          Colors.green,
                          () => _clearViolation(violation),
                        ),
                      ),
                    ],
                  ),
                ] else if (violation.status == ViolationStatus.cleared) ...[
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: Colors.green.withOpacity(0.08),
                      borderRadius: BorderRadius.circular(10),
                      border: Border.all(color: Colors.green.withOpacity(0.2)),
                    ),
                    child: const Row(
                      children: [
                        Icon(Icons.check_circle_rounded, color: Colors.green, size: 18),
                        SizedBox(width: 8),
                        Text('This case has been cleared',
                            style: TextStyle(
                                color: Colors.green, fontWeight: FontWeight.w600, fontSize: 12)),
                      ],
                    ),
                  ),
                ],
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

  // ── Filters ─────────────────────────────────────────────────────────────────
  List<Violation> _filterViolations(List<Violation> violations) {
    switch (_selectedFilter) {
      case 'referred':
        return violations.where((v) => v.status == ViolationStatus.referredToSAO).toList();
      case 'disciplinary':
        return violations.where((v) => v.status == ViolationStatus.disciplinaryAction).toList();
      case 'cleared':
        return violations.where((v) => v.status == ViolationStatus.cleared).toList();
      default:
        return violations;
    }
  }

  int _getReferredCount(List<Violation> v) =>
      v.where((x) => x.status == ViolationStatus.referredToSAO).length;

  int _getDisciplinaryCount(List<Violation> v) =>
      v.where((x) => x.status == ViolationStatus.disciplinaryAction).length;

  int _getClearedCount(List<Violation> v) =>
      v.where((x) => x.status == ViolationStatus.cleared).length;

  Color _getStatusColor(ViolationStatus status) {
    switch (status) {
      case ViolationStatus.warning:             return Colors.orange;
      case ViolationStatus.parentNotified:      return Colors.blue;
      case ViolationStatus.referredToSAO:       return _red;
      case ViolationStatus.referredToGuidance:  return Colors.teal;
      case ViolationStatus.disciplinaryAction:  return Colors.deepOrange;
      case ViolationStatus.cleared:             return Colors.green;
      default:                                  return Colors.grey;
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

  // ── Actions ─────────────────────────────────────────────────────────────────
  void _takeDisciplinaryAction(Violation violation) async {
    final confirmed = await _confirm(
        'Disciplinary Action',
        'Are you sure you want to impose a disciplinary action on this student?');
    if (confirmed) {
      final vp = Provider.of<ViolationProvider>(context, listen: false);
      await vp.updateViolationStatus(violation.id, ViolationStatus.disciplinaryAction);
      if (vp.error == null) _showSnack('Disciplinary action imposed', Colors.orange);
    }
  }

  void _clearViolation(Violation violation) async {
    final confirmed = await _confirm(
        'Clear Case',
        'Are you sure you want to clear this case?');
    if (confirmed) {
      final vp = Provider.of<ViolationProvider>(context, listen: false);
      await vp.updateViolationStatus(violation.id, ViolationStatus.cleared);
      if (vp.error == null) _showSnack('Case cleared ✅', Colors.green);
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
      content: Text(message, style: const TextStyle(fontWeight: FontWeight.w600)),
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