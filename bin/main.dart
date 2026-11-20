import 'dart:io';

import 'package:my_order_app/order.dart';

/// Console utility demonstrating all exam requirements in Dart:
/// 1. Class Order (lib/order.dart)
/// 2. Read the content from the string and save objects to list orders
/// 3. Insert an order into order.json
/// 4. Search Order by ItemName
Future<void> main() async {
  stdout.writeln('=== Introduction to Dart Programming - SET01 ===');

  // 1. Read the content from the string and save objects to list orders
  List<Order> orders = parseOrdersFromString(initialOrdersJson);
  final File orderFile = File('order.json');

  if (await orderFile.exists()) {
    final String existing = await orderFile.readAsString();
    if (existing.trim().isNotEmpty) {
      orders = parseOrdersFromString(existing);
    }
  } else {
    await orderFile.writeAsString('${encodeOrdersToJson(orders)}\n');
  }

  stdout.writeln('\nCurrent Orders (${orders.length}):');
  _printOrders(orders);

  // Example search by ItemName
  const String searchKeyword = 'Iphone 15';
  final List<Order> found = searchOrdersByItemName(orders, searchKeyword);
  stdout.writeln('\nSearch results for ItemName "$searchKeyword" (${found.length}):');
  _printOrders(found);
}

void _printOrders(List<Order> list) {
  for (int i = 0; i < list.length; i++) {
    final Order o = list[i];
    stdout.writeln(
      '${i + 1}. Item: ${o.item} | ItemName: ${o.itemName} | Quantity: ${o.quantity} | Price: ${o.formattedPrice} | Currency: ${o.currency}',
    );
  }
}
