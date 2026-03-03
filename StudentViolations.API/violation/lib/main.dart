import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/auth_provider.dart';
import 'providers/violation_provider.dart';
import 'services/database_service.dart';
import 'screens/login_screen.dart';
import 'screens/register_screen.dart';
import 'screens/guard_dashboard.dart';
import 'screens/student_dashboard.dart';
import 'screens/sao_dashboard.dart';
import 'screens/guidance_dashboard.dart';
import 'models/user.dart';

void main() {
  // Initialize the memory service with seed data
  DatabaseService.initialize();
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider()),
        ChangeNotifierProvider(create: (_) => ViolationProvider()),
      ],
      child: MaterialApp(
        title: 'Student Violation System',
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: Colors.blue),
          useMaterial3: true,
          appBarTheme: const AppBarTheme(
            centerTitle: true,
            elevation: 2,
          ),
          cardTheme: const CardThemeData(
            elevation: 4,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.all(Radius.circular(12)),
            ),
          ),
          elevatedButtonTheme: ElevatedButtonThemeData(
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
          ),
        ),
        initialRoute: '/login',
        routes: {
          '/login': (context) => const LoginScreen(),
          '/register': (context) => const RegisterScreen(),
          '/dashboard': (context) => const DashboardWrapper(),
        },
        debugShowCheckedModeBanner: false,
      ),
    );
  }
}

class DashboardWrapper extends StatelessWidget {
  const DashboardWrapper({super.key});

  @override
  Widget build(BuildContext context) {
    return Consumer<AuthProvider>(
      builder: (context, authProvider, child) {
        final user = authProvider.currentUser;
        
        if (user == null) {
          return const LoginScreen();
        }

        switch (user.role) {
          case UserRole.guard:
            return const GuardDashboard();
          case UserRole.student:
            return const StudentDashboard();
          case UserRole.sao:
            return const SAODashboard();
          case UserRole.guidance:
            return const GuidanceDashboard();
          default:
            return const LoginScreen();
        }
      },
    );
  }
}
