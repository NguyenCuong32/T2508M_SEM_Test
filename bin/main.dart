import 'dart:io';
import 'package:order_management/models/order.dart';
import 'package:order_management/services/order_service.dart';

void main() async {
  final orderService = OrderService();
  stdout.writeln('=== Introduction to Dart Programming - SET01 ===');

  final orders = await orderService.loadOrders();
  stdout.writeln('Loaded ${orders.length} orders from JSON:');
  for (final o in orders) {
    stdout.writeln('- [${o.item}] ${o.itemName} | Price: ${o.price} ${o.currency} | Qty: ${o.quantity}');
  }

  stdout.writeln('\nSearching orders with keyword "Iphone 15":');
  final found = orderService.searchOrderByItemName('Iphone 15');
  for (final o in found) {
    stdout.writeln('- [${o.item}] ${o.itemName}');
  }

  stdout.writeln('\nInserting new order into order.json:');
  final newOrder = Order(
    item: 'A1002',
    itemName: 'Iphone 17 Pro',
    price: 1800,
    currency: 'USD',
    quantity: 1,
  );
  await orderService.insertOrder(newOrder);
  stdout.writeln('Inserted ${newOrder.item}. Total orders now: ${orderService.orders.length}');
}

