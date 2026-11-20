import 'package:flutter/material.dart';

import 'screens/my_order_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const MyOrderApp());
}

class MyOrderApp extends StatelessWidget {
  const MyOrderApp({super.key});

  @override
  Widget build(BuildContext context) {
    const Color primaryOrange = Color(0xFFEA580C);

    return MaterialApp(
      title: 'My Order',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: primaryOrange,
          primary: primaryOrange,
          secondary: const Color(0xFFF97316),
          surface: Colors.white,
        ),
        scaffoldBackgroundColor: const Color(0xFFF8FAFC),
        textTheme: const TextTheme(
          bodyMedium: TextStyle(
            color: Color(0xFF0F172A),
            height: 1.35,
          ),
        ),
      ),
      home: const MyOrderScreen(),
    );
  }
}
