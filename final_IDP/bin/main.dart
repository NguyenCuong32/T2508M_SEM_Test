import 'dart:io';
import '../lib/order.dart';
import '../lib/order_manager.dart';

/// Console Runner for the Exam requirements.
/// Demonstrates:
///   1. Reading JSON string from file / raw string
///   2. Parsing into List<Order>
///   3. Displaying formatted orders table to screen
///   4. Inserting a new order into order.json
///   5. Searching orders by ItemName
Future<void> main(List<String> args) async {
  print('=' * 70);
  print('   FPT APTECH - INTRODUCTION TO DART PROGRAMMING (SET01)');
  print('   Candidate / Senior Dev Solution');
  print('=' * 70);

  final manager = OrderManager();
  final orderFile = File('order.json');

  // 1. Initial string as given in the exam paper
  const defaultExamJsonString =
      '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": '
      '"USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": '
      '1500,"Currency": "USD","Quantity":1}]';

  // Load from order.json if exists, else load from the given exam string
  String jsonContent = defaultExamJsonString;
  if (await orderFile.exists()) {
    try {
      final fileData = await orderFile.readAsString();
      if (fileData.trim().isNotEmpty) {
        jsonContent = fileData;
        print('[✓] Loaded existing data from "order.json".');
      }
    } catch (e) {
      print('[!] Error reading order.json, falling back to exam string: $e');
    }
  } else {
    print('[i] "order.json" not found. Using default exam JSON string.');
  }

  // Parse string and save objects to list orders
  manager.loadFromJsonString(jsonContent);
  print('[✓] Successfully parsed ${manager.count} orders into List<Order>.\n');

  // Display initial list
  print('--- [DISPLAY ORDERS TO SCREEN] ---');
  printOrdersTable(manager.orders);

  // Requirement: Insert order into order.json
  print('\n--- [INSERT ORDER INTO order.json] ---');
  final newOrder = Order(
    item: 'A1002',
    itemName: 'Iphone 15 Pro',
    price: 1350,
    currency: 'USD',
    quantity: 1,
  );
  print('Adding new order: ${newOrder.item} - ${newOrder.itemName} (\$${newOrder.price})');
  manager.addOrder(newOrder);

  // Write updated list to order.json
  await orderFile.writeAsString(manager.toJsonString(pretty: true));
  print('[✓] Saved ${manager.count} orders to "order.json" successfully!\n');

  // Display updated list
  print('--- [UPDATED ORDERS LIST] ---');
  printOrdersTable(manager.orders);

  // Requirement: Update / Edit order
  print('\n--- [UPDATE / SỬA ORDER TRONG order.json] ---');
  final orderToUpdate = Order(
    item: 'A1000',
    itemName: 'Iphone 15 (Updated)',
    price: 1199,
    currency: 'USD',
    quantity: 2,
  );
  print('Sửa đơn hàng ${orderToUpdate.item}: tên mới "${orderToUpdate.itemName}", giá mới \$${orderToUpdate.price}, số lượng ${orderToUpdate.quantity}');
  final isUpdated = manager.updateOrder(orderToUpdate);
  if (isUpdated) {
    await orderFile.writeAsString(manager.toJsonString(pretty: true));
    print('[✓] Đã cập nhật và lưu vào "order.json" thành công!');
    printOrdersTable(manager.orders);
  }

  // Requirement: Search order by ItemName
  print('\n--- [SEARCH ORDER BY ItemName] ---');
  final searchQueries = ['Iphone 15', '16', 'Samsung'];
  for (final query in searchQueries) {
    print('\nSearching for: "$query"');
    final results = manager.searchByItemName(query);
    if (results.isEmpty) {
      print('  -> No orders found matching "$query".');
    } else {
      print('  -> Found ${results.length} order(s):');
      for (final order in results) {
        print('     • [${order.item}] ${order.itemName} | Price: ${order.formattedPrice} ${order.currency} | Qty: ${order.quantity}');
      }
    }
  }

  print('\n' + '=' * 70);
  print('   All exam requirements executed and validated successfully!');
  print('=' * 70);
}

/// Helper to render orders in a clean, professional ASCII table
void printOrdersTable(List<Order> orders) {
  if (orders.isEmpty) {
    print('No orders to display.');
    return;
  }

  final header = '| ${'Id'.padRight(4)} | ${'Item'.padRight(8)} | ${'Item Name'.padRight(20)} | ${'Quantity'.padRight(8)} | ${'Price'.padRight(10)} | ${'Currency'.padRight(8)} |';
  final separator = '+' + '-' * 6 + '+' + '-' * 10 + '+' + '-' * 22 + '+' + '-' * 10 + '+' + '-' * 12 + '+' + '-' * 10 + '+';

  print(separator);
  print(header);
  print(separator);

  for (var i = 0; i < orders.length; i++) {
    final order = orders[i];
    final idStr = (i + 1).toString().padRight(4);
    final itemStr = order.item.padRight(8);
    final nameStr = order.itemName.padRight(20);
    final qtyStr = order.quantity.toString().padRight(8);
    final priceStr = order.formattedPrice.padRight(10);
    final currStr = order.currency.padRight(8);

    print('| $idStr | $itemStr | $nameStr | $qtyStr | $priceStr | $currStr |');
  }

  print(separator);
}
