import 'dart:convert';

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

  /// Factory constructor to create an Order object from a JSON map
  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      item: (json['Item'] ?? json['item'] ?? '').toString(),
      itemName: (json['ItemName'] ?? json['itemName'] ?? '').toString(),
      price: json['Price'] is num
          ? json['Price'] as num
          : num.tryParse((json['Price'] ?? json['price'] ?? '0').toString()) ?? 0,
      currency: (json['Currency'] ?? json['currency'] ?? 'USD').toString(),
      quantity: json['Quantity'] is int
          ? json['Quantity'] as int
          : int.tryParse((json['Quantity'] ?? json['quantity'] ?? '1').toString()) ?? 1,
    );
  }

  /// Convert an Order object to a JSON map matching the exam schema
  Map<String, dynamic> toJson() {
    return {
      'Item': item,
      'ItemName': itemName,
      'Price': price,
      'Currency': currency,
      'Quantity': quantity,
    };
  }

  /// Helper to format price with thousands dot separator (e.g. 1200 -> 1.200) as shown in the exam picture
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

  @override
  String toString() {
    return 'Order(Item: $item, ItemName: $itemName, Price: $price, Currency: $currency, Quantity: $quantity)';
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

/// Encode list of orders to formatted JSON string for saving to order.json
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
