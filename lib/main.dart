
import 'package:flutter/material.dart';
import 'screens/order_screen.dart';

void main() {
  runApp(const OrderManagementApp());
}

class OrderManagementApp extends StatelessWidget {
  const OrderManagementApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'Order Management',
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFFC96550),
        ),
      ),
      home: const OrderScreen(),
    );
  }
}
