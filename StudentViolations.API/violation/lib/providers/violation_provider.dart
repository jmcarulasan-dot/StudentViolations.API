import 'package:flutter/foundation.dart';
import '../models/violation.dart';
import '../models/user.dart';
import '../services/database_service.dart';

class ViolationProvider with ChangeNotifier {
  List<Violation> _violations = [];
  List<User> _students = [];
  bool _isLoading = false;
  String? _error;

  List<Violation> get violations => _violations;
  List<User> get students => _students;
  bool get isLoading => _isLoading;
  String? get error => _error;

  Future<void> loadStudents() async {
    _setLoading(true);
    _error = null;

    try {
      _students = await DatabaseService.getAllStudents();
      notifyListeners();
    } catch (e) {
      _error = 'Failed to load students: ${e.toString()}';
      notifyListeners();
    } finally {
      _setLoading(false);
    }
  }

  Future<void> loadStudentViolations(String studentId) async {
    _setLoading(true);
    _error = null;

    try {
      _violations = await DatabaseService.getStudentViolations(studentId);
      notifyListeners();
    } catch (e) {
      _error = 'Failed to load violations: ${e.toString()}';
      notifyListeners();
    } finally {
      _setLoading(false);
    }
  }

  Future<void> loadAllViolations() async {
    _setLoading(true);
    _error = null;

    try {
      _violations = await DatabaseService.getAllViolations();
      notifyListeners();
    } catch (e) {
      _error = 'Failed to load violations: ${e.toString()}';
      notifyListeners();
    } finally {
      _setLoading(false);
    }
  }

  Future<void> recordViolation({
    required String studentId,
    required ViolationType type,
    required String reportedBy,
    String? remarks,
  }) async {
    _setLoading(true);
    _error = null;

    try {
      final existingViolations = await DatabaseService.getStudentViolations(studentId);
      final sameTypeViolations = existingViolations.where((v) => v.type == type).length;
      
      ViolationStatus status;
      if (sameTypeViolations == 0) {
        status = ViolationStatus.warning;
      } else if (sameTypeViolations == 1) {
        status = ViolationStatus.parentNotified;
      } else if (sameTypeViolations == 2) {
        status = ViolationStatus.referredToSAO;
      } else {
        status = ViolationStatus.referredToGuidance;
      }

      final violation = Violation(
        id: DateTime.now().millisecondsSinceEpoch.toString(),
        studentId: studentId,
        type: type,
        date: DateTime.now(),
        remarks: remarks,
        status: status,
        offenseCount: sameTypeViolations + 1,
        reportedBy: reportedBy,
      );

      await DatabaseService.addViolation(violation);
      await loadAllViolations();
      
      notifyListeners();
    } catch (e) {
      _error = 'Failed to record violation: ${e.toString()}';
      notifyListeners();
    } finally {
      _setLoading(false);
    }
  }

  Future<void> updateViolationStatus(String violationId, ViolationStatus status) async {
    _setLoading(true);
    _error = null;

    try {
      await DatabaseService.updateViolationStatus(violationId, status);
      await loadAllViolations();
      notifyListeners();
    } catch (e) {
      _error = 'Failed to update violation: ${e.toString()}';
      notifyListeners();
    } finally {
      _setLoading(false);
    }
  }

  void _setLoading(bool loading) {
    _isLoading = loading;
    notifyListeners();
  }

  void clearError() {
    _error = null;
    notifyListeners();
  }
}
