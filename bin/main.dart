import 'dart:io';

import 'package:my_order_app/order.dart';

/// Console application demonstrating all requirements in the exam paper:
/// 1. Create a class with the name "Order" (2 Marks)
/// 2. Read the content from the string and save objects to list orders, display to screen (5 Marks)
/// 3. Allows inserting an order into order.json (4 Marks)
/// 4. Allows search Order by ItemName (3 Marks)
Future<void> main() async {
  stdout.writeln('===============================================================');
  stdout.writeln('   FPT APTECH - INTRODUCTION TO DART PROGRAMMING (SET01)');
  stdout.writeln('===============================================================\n');

  const String jsonFilePath = 'order.json';

  // Question 2: Read content from the initial string and save objects to list orders
  stdout.writeln('[STEP 1] Parsing initial JSON string into List<Order> objects...');
  final List<Order> initialOrders = parseOrdersFromString(initialOrdersJson);
  stdout.writeln('Successfully parsed ${initialOrders.length} orders from string:\n');
  _printOrdersTable(initialOrders);

  // Question 3: Save to and load from order.json
  stdout.writeln('\n[STEP 2] Initializing "$jsonFilePath" with orders...');
  await saveOrdersToFile(jsonFilePath, initialOrders);
  stdout.writeln('Saved ${initialOrders.length} orders to "$jsonFilePath".');

  // Insert a new order into order.json
  stdout.writeln('\n[STEP 3] Inserting a new order into "$jsonFilePath"...');
  final Order newOrder = Order(
    item: 'A1002',
    itemName: 'Iphone 16 Pro Max',
    price: 1800,
    currency: 'USD',
    quantity: 1,
  );
  await insertOrderIntoJsonFile(jsonFilePath, newOrder);
  stdout.writeln('Inserted order: ${newOrder.item} - ${newOrder.itemName} (Price: ${newOrder.formattedPrice} ${newOrder.currency})');

  // Read back all orders from order.json
  final List<Order> updatedOrders = await loadOrdersFromFile(jsonFilePath);
  stdout.writeln('\nUpdated list of orders read from "$jsonFilePath" (${updatedOrders.length} items):');
  _printOrdersTable(updatedOrders);

  // Question 4: Allows search Order by ItemName
  stdout.writeln('\n[STEP 4] Searching Order by ItemName...');
  const String searchKeyword = 'Iphone 15';
  stdout.writeln('Search query: "$searchKeyword"');
  final List<Order> searchResults = searchOrdersByItemName(updatedOrders, searchKeyword);
  stdout.writeln('Found ${searchResults.length} matching order(s):');
  _printOrdersTable(searchResults);

  // Search example with partial name
  const String searchPartial = '16';
  stdout.writeln('\nSearch query: "$searchPartial"');
  final List<Order> searchResults2 = searchOrdersByItemName(updatedOrders, searchPartial);
  stdout.writeln('Found ${searchResults2.length} matching order(s):');
  _printOrdersTable(searchResults2);

  stdout.writeln('\n===============================================================');
  stdout.writeln('   ALL EXAM REQUIREMENTS DEMONSTRATED SUCCESSFULLY!');
  stdout.writeln('   To run the web app with UI, run: dart run bin/server.dart');
  stdout.writeln('===============================================================');
}

void _printOrdersTable(List<Order> orders) {
  if (orders.isEmpty) {
    stdout.writeln('  (No orders found)');
    return;
  }

  stdout.writeln('  +----+---------+----------------------+----------+------------+----------+');
  stdout.writeln('  | Id | Item    | Item Name            | Quantity | Price      | Currency |');
  stdout.writeln('  +----+---------+----------------------+----------+------------+----------+');

  for (int i = 0; i < orders.length; i++) {
    final Order o = orders[i];
    final String idStr = '${i + 1}'.padRight(2);
    final String itemStr = o.item.padRight(7);
    final String nameStr = o.itemName.padRight(20);
    final String qtyStr = '${o.quantity}'.padRight(8);
    final String priceStr = o.formattedPrice.padLeft(10);
    final String currStr = o.currency.padRight(8);

    stdout.writeln('  | $idStr | $itemStr | $nameStr | $qtyStr | $priceStr | $currStr |');
  }

  stdout.writeln('  +----+---------+----------------------+----------+------------+----------+');
}
