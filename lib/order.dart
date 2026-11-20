import 'dart:convert';
import 'dart:io';

/// Initial JSON string provided in the exam paper (Introduction to Dart Programming - SET01)
const String initialOrdersJson =
    '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": 1500,"Currency": "USD","Quantity":1}]';

/// Question 1: Create a class with the name "Order" (2 Marks)
class Order {
  String item;
  String itemName;
  num price;
  String currency;
  int quantity;

  Order({
    required this.item,
    required this.itemName,
    required this.price,
    required this.currency,
    required this.quantity,
  });

  /// Factory constructor to create an Order object from a JSON Map
  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      item: (json['Item'] ?? json['item'] ?? '').toString().trim(),
      itemName: (json['ItemName'] ?? json['itemName'] ?? '').toString().trim(),
      price: json['Price'] is num
          ? json['Price'] as num
          : (json['price'] is num
              ? json['price'] as num
              : num.tryParse((json['Price'] ?? json['price'] ?? '0').toString()) ?? 0),
      currency: (json['Currency'] ?? json['currency'] ?? 'USD').toString().trim(),
      quantity: json['Quantity'] is int
          ? json['Quantity'] as int
          : (json['quantity'] is int
              ? json['quantity'] as int
              : int.tryParse((json['Quantity'] ?? json['quantity'] ?? '1').toString()) ?? 1),
    );
  }

  /// Convert an Order object to a JSON Map matching the exam schema
  Map<String, dynamic> toJson() {
    return {
      'Item': item,
      'ItemName': itemName,
      'Price': price,
      'Currency': currency,
      'Quantity': quantity,
    };
  }

  /// Formatted price with thousands dot separator (e.g. 1200 -> 1.200) as shown in the exam screenshot
  String get formattedPrice {
    final bool isWhole = price == price.roundToDouble();
    final String raw = isWhole ? price.toInt().toString() : price.toStringAsFixed(2);
    final List<String> parts = raw.split('.');
    final String integerPart = parts[0].replaceAllMapped(
      RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
      (Match m) => '${m[1]}.',
    );
    return parts.length > 1 ? '$integerPart,${parts[1]}' : integerPart;
  }

  /// Total price calculation for this order (price * quantity)
  num get total => price * quantity;

  /// Formatted total price
  String get formattedTotal {
    final bool isWhole = total == total.roundToDouble();
    final String raw = isWhole ? total.toInt().toString() : total.toStringAsFixed(2);
    final List<String> parts = raw.split('.');
    final String integerPart = parts[0].replaceAllMapped(
      RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
      (Match m) => '${m[1]}.',
    );
    return parts.length > 1 ? '$integerPart,${parts[1]}' : integerPart;
  }

  @override
  String toString() {
    return 'Order(Item: $item, ItemName: $itemName, Price: $formattedPrice $currency, Quantity: $quantity)';
  }
}

/// Question 2: Read the content from the string and save objects to list orders (5 Marks)
List<Order> parseOrdersFromString(String jsonStr) {
  final dynamic decoded = jsonDecode(jsonStr);
  if (decoded is List) {
    return decoded
        .map((dynamic element) => Order.fromJson(Map<String, dynamic>.from(element as Map)))
        .toList();
  }
  return <Order>[];
}

/// Encode a list of Order objects into a formatted JSON string
String encodeOrdersToJson(List<Order> orders) {
  final List<Map<String, dynamic>> jsonList =
      orders.map((Order o) => o.toJson()).toList();
  return const JsonEncoder.withIndent('  ').convert(jsonList);
}

/// Question 4: Search Order by ItemName (3 Marks)
List<Order> searchOrdersByItemName(List<Order> orders, String query) {
  final String normalized = query.trim().toLowerCase();
  if (normalized.isEmpty) {
    return List<Order>.from(orders);
  }
  return orders
      .where((Order order) => order.itemName.toLowerCase().contains(normalized))
      .toList();
}

/// Load orders from order.json file
Future<List<Order>> loadOrdersFromFile(String filePath) async {
  final File file = File(filePath);
  if (!await file.exists()) {
    // If file doesn't exist, seed it with the default initialOrdersJson
    final List<Order> defaults = parseOrdersFromString(initialOrdersJson);
    await saveOrdersToFile(filePath, defaults);
    return defaults;
  }

  final String content = await file.readAsString();
  if (content.trim().isEmpty) {
    return <Order>[];
  }
  return parseOrdersFromString(content);
}

/// Save orders list to order.json file
Future<void> saveOrdersToFile(String filePath, List<Order> orders) async {
  final File file = File(filePath);
  final String jsonString = encodeOrdersToJson(orders);
  await file.writeAsString('$jsonString\n');
}

/// Update an existing order by index and save to order.json
Future<bool> updateOrderInJsonFile(String filePath, int index, Order updatedOrder) async {
  final List<Order> currentOrders = await loadOrdersFromFile(filePath);
  if (index < 0 || index >= currentOrders.length) {
    return false;
  }
  currentOrders[index] = updatedOrder;
  await saveOrdersToFile(filePath, currentOrders);
  return true;
}

/// Sắp xếp và bộ lọc đơn hàng (Filter & Sort)
enum OrderSortOption { added, newest, nameAsc, nameDesc, priceAsc, priceDesc }

List<Order> filterAndSortOrders(
  List<Order> orders, {
  String query = '',
  String currency = 'ALL',
  double? minPrice,
  double? maxPrice,
  OrderSortOption sort = OrderSortOption.added,
}) {
  List<Order> result = searchOrdersByItemName(orders, query);

  if (currency.toUpperCase() != 'ALL' && currency.trim().isNotEmpty) {
    result = result
        .where((o) => o.currency.toUpperCase() == currency.toUpperCase())
        .toList();
  }

  if (minPrice != null) {
    result = result.where((o) => o.price >= minPrice).toList();
  }

  if (maxPrice != null) {
    result = result.where((o) => o.price <= maxPrice).toList();
  }

  switch (sort) {
    case OrderSortOption.added:
      break;
    case OrderSortOption.newest:
      result = result.reversed.toList();
      break;
    case OrderSortOption.nameAsc:
      result.sort((a, b) => a.itemName.toLowerCase().compareTo(b.itemName.toLowerCase()));
      break;
    case OrderSortOption.nameDesc:
      result.sort((a, b) => b.itemName.toLowerCase().compareTo(a.itemName.toLowerCase()));
      break;
    case OrderSortOption.priceAsc:
      result.sort((a, b) => a.price.compareTo(b.price));
      break;
    case OrderSortOption.priceDesc:
      result.sort((a, b) => b.price.compareTo(a.price));
      break;
  }

  return result;
}

/// Question 3: Allows inserting an order into order.json (4 Marks)
Future<void> insertOrderIntoJsonFile(String filePath, Order newOrder) async {
  final List<Order> currentOrders = await loadOrdersFromFile(filePath);
  currentOrders.add(newOrder);
  await saveOrdersToFile(filePath, currentOrders);
}
