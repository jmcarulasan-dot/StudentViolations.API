import '../models/user.dart';
import '../models/violation.dart';
import 'memory_service.dart';

class DatabaseService {
  static final MemoryService _memoryService = MemoryService();

  static void initialize() {
    _memoryService.initializeSeedData();
  }

  static Future<User?> login(String username, String password) async {
    // Simulate async operation
    await Future.delayed(const Duration(milliseconds: 100));
    return _memoryService.login(username, password);
  }

  static Future<User?> register({
    required String username,
    required String password,
    required String name,
    required UserRole role,
    String? gradeSection,
    String? contactNumber,
  }) async {
    // Simulate async operation
    await Future.delayed(const Duration(milliseconds: 100));
    return _memoryService.register(
      username: username,
      password: password,
      name: name,
      role: role,
      gradeSection: gradeSection,
      contactNumber: contactNumber,
    );
  }

  static Future<List<User>> getAllStudents() async {
    // Simulate async operation
    await Future.delayed(const Duration(milliseconds: 50));
    return _memoryService.getAllStudents();
  }

  static Future<List<User>> getAllUsers() async {
    // Simulate async operation
    await Future.delayed(const Duration(milliseconds: 50));
    return _memoryService.getAllUsers();
  }

  static Future<void> addViolation(Violation violation) async {
    // Simulate async operation
    await Future.delayed(const Duration(milliseconds: 50));
    _memoryService.addViolation(violation);
  }

  static Future<List<Violation>> getStudentViolations(String studentId) async {
    // Simulate async operation
    await Future.delayed(const Duration(milliseconds: 50));
    return _memoryService.getStudentViolations(studentId);
  }

  static Future<List<Violation>> getAllViolations() async {
    // Simulate async operation
    await Future.delayed(const Duration(milliseconds: 50));
    return _memoryService.getAllViolations();
  }

  static Future<void> updateViolationStatus(String violationId, ViolationStatus status) async {
    // Simulate async operation
    await Future.delayed(const Duration(milliseconds: 50));
    _memoryService.updateViolationStatus(violationId, status);
  }

  static Future<int> getViolationCount(String studentId, ViolationType type) async {
    // Simulate async operation
    await Future.delayed(const Duration(milliseconds: 50));
    return _memoryService.getViolationCount(studentId, type);
  }
}
