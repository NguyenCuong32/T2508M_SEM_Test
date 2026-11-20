
import 'dart:convert';
import 'package:flutter/services.dart';
import '../models/order.dart';

class OrderService {
  // Read JSON file and convert to List<Order>
  static Future<List<Order>> loadOrders() async {
    final String response =
    await rootBundle.loadString('assets/order.json');

    final List<dynamic> data = jsonDecode(response);

    return data
        .map((json) => Order.fromJson(json))
        .toList();
  }

  // Convert List<Order> to JSON string
  static String ordersToJson(List<Order> orders) {
    final List<Map<String, dynamic>> data =
    orders.map((order) => order.toJson()).toList();

    return const JsonEncoder.withIndent('  ').convert(data);
  }
}
