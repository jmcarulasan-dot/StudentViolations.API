import '../models/user.dart';
import '../models/violation.dart';

class MemoryService {
  static final MemoryService _instance = MemoryService._internal();
  factory MemoryService() => _instance;
  MemoryService._internal();

  final List<User> _users = [];
  final List<Violation> _violations = [];

  // Initialize with seed data
  void initializeSeedData() {
    if (_users.isEmpty) {
      _users.addAll([
        User(
          id: 'guard1',
          username: 'guard',
          password: 'guard123',
          name: 'John Guard',
          role: UserRole.guard,
        ),
        User(
          id: 'student1',
          username: 'student',
          password: 'student123',
          name: 'Juan Santos',
          role: UserRole.student,
          gradeSection: 'Grade 10 - Rose',
          contactNumber: '09123456789',
        ),
        User(
          id: 'sao1',
          username: 'sao',
          password: 'sao123',
          name: 'Maria SAO',
          role: UserRole.sao,
        ),
        User(
          id: 'guidance1',
          username: 'guidance',
          password: 'guidance123',
          name: 'Ana Guidance',
          role: UserRole.guidance,
        ),
      ]);
    }
  }

  // User operations
  User? login(String username, String password) {
    try {
      return _users.firstWhere(
        (user) => user.username == username && user.password == password,
      );
    } catch (e) {
      return null;
    }
  }

  User? register({
    required String username,
    required String password,
    required String name,
    required UserRole role,
    String? gradeSection,
    String? contactNumber,
  }) {
    // Check if username already exists
    if (_users.any((user) => user.username == username)) {
      return null;
    }

    // Generate unique ID
    final userId = '${role.name}_${DateTime.now().millisecondsSinceEpoch}';

    // Create new user
    final newUser = User(
      id: userId,
      username: username,
      password: password,
      name: name,
      role: role,
      gradeSection: gradeSection,
      contactNumber: contactNumber,
    );

    _users.add(newUser);
    return newUser;
  }

  List<User> getAllStudents() {
    return _users.where((user) => user.role == UserRole.student).toList();
  }

  List<User> getAllUsers() {
    return List.from(_users);
  }

  User? getUserById(String userId) {
    try {
      return _users.firstWhere((user) => user.id == userId);
    } catch (e) {
      return null;
    }
  }

  // Violation operations
  void addViolation(Violation violation) {
    _violations.add(violation);
  }

  List<Violation> getStudentViolations(String studentId) {
    return _violations
        .where((violation) => violation.studentId == studentId)
        .toList()
      ..sort((a, b) => b.date.compareTo(a.date)); // Sort by date descending
  }

  List<Violation> getAllViolations() {
    return List.from(_violations)
      ..sort((a, b) => b.date.compareTo(a.date)); // Sort by date descending
  }

  void updateViolationStatus(String violationId, ViolationStatus status) {
    final index = _violations.indexWhere((v) => v.id == violationId);
    if (index != -1) {
      final violation = _violations[index];
      _violations[index] = Violation(
        id: violation.id,
        studentId: violation.studentId,
        type: violation.type,
        date: violation.date,
        remarks: violation.remarks,
        status: status,
        offenseCount: violation.offenseCount,
        reportedBy: violation.reportedBy,
      );
    }
  }

  int getViolationCount(String studentId, ViolationType type) {
    return _violations
        .where((violation) => 
            violation.studentId == studentId && violation.type == type)
        .length;
  }

  // Clear all data (for testing)
  void clearAllData() {
    _users.clear();
    _violations.clear();
  }

  // Get data counts for debugging
  int get userCount => _users.length;
  int get violationCount => _violations.length;
}
