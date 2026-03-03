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
  DatabaseService.initialize();
  runApp(const MyApp());
}

// ── Official ACLC College of Mandaue Brand Colors ─────────────────────────────
class ACLCColors {
  static const red       = Color(0xFFFD070C); // Official ACLC Red
  static const navy      = Color(0xFF0F136E); // Official ACLC Navy Blue
  static const navyLight = Color(0xFF1A1F8F); // Slightly lighter navy
  static const redDark   = Color(0xFFB80004); // Darker red for hover/shadows
  static const white     = Colors.white;
  static const gray      = Color(0xFFF5F7FA); // Light background
  static const cardBg    = Color(0xFFFFFFFF);
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
        title: 'ACLC Student Violation System',
        debugShowCheckedModeBanner: false,
        initialRoute: '/login',
        routes: {
          '/login':     (context) => const LoginScreen(),
          '/register':  (context) => const RegisterScreen(),
          '/dashboard': (context) => const DashboardWrapper(),
        },
        theme: ThemeData(
          useMaterial3: true,

          // Seed the whole app from ACLC Red
          colorScheme: ColorScheme.fromSeed(
            seedColor: ACLCColors.red,
            primary: ACLCColors.red,
            secondary: ACLCColors.navy,
            surface: ACLCColors.cardBg,
            background: ACLCColors.gray,
            onPrimary: Colors.white,
            onSecondary: Colors.white,
            onSurface: Colors.black87,
          ),

          scaffoldBackgroundColor: ACLCColors.gray,

          // ── AppBar ────────────────────────────────────────────────────────
          appBarTheme: const AppBarTheme(
            centerTitle: true,
            elevation: 3,
            backgroundColor: ACLCColors.navy,
            foregroundColor: Colors.white,
            titleTextStyle: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w700,
              color: Colors.white,
              letterSpacing: 0.5,
            ),
            shadowColor: ACLCColors.navy,
          ),

          // ── Cards ─────────────────────────────────────────────────────────
          cardTheme: CardThemeData(
            elevation: 4,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(16),
            ),
            color: ACLCColors.cardBg,
            surfaceTintColor: ACLCColors.cardBg,
            shadowColor: ACLCColors.navy.withOpacity(0.15),
          ),

          // ── Elevated Buttons ──────────────────────────────────────────────
          elevatedButtonTheme: ElevatedButtonThemeData(
            style: ElevatedButton.styleFrom(
              backgroundColor: ACLCColors.red,
              foregroundColor: Colors.white,
              elevation: 3,
              shadowColor: ACLCColors.red.withOpacity(0.4),
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
              textStyle: const TextStyle(
                fontSize: 15,
                fontWeight: FontWeight.w700,
                letterSpacing: 0.8,
              ),
            ),
          ),

          // ── Text Buttons ──────────────────────────────────────────────────
          textButtonTheme: TextButtonThemeData(
            style: TextButton.styleFrom(
              foregroundColor: ACLCColors.navy,
              textStyle: const TextStyle(
                fontWeight: FontWeight.w600,
              ),
            ),
          ),

          // ── Outlined Buttons ──────────────────────────────────────────────
          outlinedButtonTheme: OutlinedButtonThemeData(
            style: OutlinedButton.styleFrom(
              foregroundColor: ACLCColors.navy,
              side: const BorderSide(color: ACLCColors.navy, width: 1.5),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
          ),

          // ── Input Fields ──────────────────────────────────────────────────
          inputDecorationTheme: InputDecorationTheme(
            filled: true,
            fillColor: const Color(0xFFF7F8FC),
            labelStyle: const TextStyle(color: Colors.black54, fontSize: 14),
            prefixIconColor: ACLCColors.navy,
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFFDDE1EE)),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: Color(0xFFDDE1EE)),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: ACLCColors.navy, width: 1.8),
            ),
            errorBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
              borderSide: const BorderSide(color: ACLCColors.red, width: 1.5),
            ),
            contentPadding:
                const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
          ),

          // ── Chip (role chips, tags) ────────────────────────────────────────
          chipTheme: ChipThemeData(
            backgroundColor: Colors.grey.shade100,
            selectedColor: ACLCColors.navy.withOpacity(0.15),
            labelStyle: const TextStyle(
              fontSize: 13,
              fontWeight: FontWeight.w600,
              color: ACLCColors.navy,
            ),
            side: const BorderSide(color: ACLCColors.navy, width: 1),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
          ),

          // ── Floating Action Button ────────────────────────────────────────
          floatingActionButtonTheme: const FloatingActionButtonThemeData(
            backgroundColor: ACLCColors.red,
            foregroundColor: Colors.white,
            elevation: 4,
          ),

          // ── Divider ───────────────────────────────────────────────────────
          dividerTheme: DividerThemeData(
            color: Colors.grey.shade200,
            thickness: 1,
          ),

          // ── SnackBar ──────────────────────────────────────────────────────
          snackBarTheme: SnackBarThemeData(
            backgroundColor: ACLCColors.navy,
            contentTextStyle:
                const TextStyle(color: Colors.white, fontSize: 13),
            behavior: SnackBarBehavior.floating,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
          ),

          // ── Typography ────────────────────────────────────────────────────
          textTheme: const TextTheme(
            bodyMedium: TextStyle(
              color: Colors.black87,
              fontSize: 14,
            ),
            bodySmall: TextStyle(
              color: Colors.black54,
              fontSize: 12,
            ),
            titleLarge: TextStyle(
              fontSize: 22,
              fontWeight: FontWeight.w800,
              color: ACLCColors.navy,
            ),
            titleMedium: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.w700,
              color: ACLCColors.navy,
            ),
            titleSmall: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
              color: ACLCColors.navy,
            ),
            labelLarge: TextStyle(
              fontSize: 15,
              fontWeight: FontWeight.w700,
              color: Colors.white,
              letterSpacing: 0.8,
            ),
          ),
        ),
      ),
    );
  }
}

// ── Dashboard Router ──────────────────────────────────────────────────────────
class DashboardWrapper extends StatelessWidget {
  const DashboardWrapper({super.key});

  @override
  Widget build(BuildContext context) {
    return Consumer<AuthProvider>(
      builder: (context, authProvider, child) {
        final user = authProvider.currentUser;

        if (user == null) return const LoginScreen();

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