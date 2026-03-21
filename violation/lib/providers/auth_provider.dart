import 'package:flutter/foundation.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../models/user.dart';
import '../services/database_service.dart';

class AuthProvider with ChangeNotifier {
  User? _currentUser;
  bool _isLoading = false;
  String? _error;

  User? get currentUser => _currentUser;
  bool get isLoading => _isLoading;
  String? get error => _error;
  bool get isAuthenticated => _currentUser != null;

  Future<void> login(String username, String password) async {
    _setLoading(true);
    _error = null;

    try {
      final user = await DatabaseService.login(username, password);
      
      if (user != null) {
        _currentUser = user;
        await _saveSession(user);
        notifyListeners();
      } else {
        _error = 'Invalid username or password';
        notifyListeners();
      }
    } catch (e) {
      _error = 'Login failed: ${e.toString()}';
      notifyListeners();
    } finally {
      _setLoading(false);
    }
  }

  Future<void> register({
    required String username,
    required String password,
    required String name,
    required UserRole role,
    String? gradeSection,
    String? contactNumber,
  }) async {
    _setLoading(true);
    _error = null;

    try {
      final user = await DatabaseService.register(
        username: username,
        password: password,
        name: name,
        role: role,
        gradeSection: gradeSection,
        contactNumber: contactNumber,
      );
      
      if (user != null) {
        _currentUser = user;
        await _saveSession(user);
        notifyListeners();
      } else {
        _error = 'Username already exists';
        notifyListeners();
      }
    } catch (e) {
      _error = 'Registration failed: ${e.toString()}';
      notifyListeners();
    } finally {
      _setLoading(false);
    }
  }

  Future<void> logout() async {
    _currentUser = null;
    await _clearSession();
    notifyListeners();
  }

  Future<void> checkAuthStatus() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final userId = prefs.getString('user_id');
      
      if (userId != null) {
        final users = await DatabaseService.getAllUsers();
        try {
          final user = users.firstWhere((u) => u.id == userId);
          _currentUser = user;
          notifyListeners();
        } catch (e) {
          debugPrint('User not found: $userId');
        }
      }
    } catch (e) {
      debugPrint('Error checking auth status: $e');
    }
  }

  Future<void> _saveSession(User user) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString('user_id', user.id);
    await prefs.setString('user_role', user.role.name);
  }

  Future<void> _clearSession() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('user_id');
    await prefs.remove('user_role');
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
