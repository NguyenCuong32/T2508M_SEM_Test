// ignore_for_file: non_constant_identifier_names

import 'dart:convert';

/// Model class representing an Order item matching the SET01 JSON schema.
class Order {
  final String Item;
  final String ItemName;
  final double Price;
  final String Currency;
  final int Quantity;

  const Order({
    required this.Item,
    required this.ItemName,
    required this.Price,
    required this.Currency,
    required this.Quantity,
  });

  /// Factory constructor to parse an [Order] from a JSON map.
  factory Order.fromJson(Map<String, dynamic> json) {
    final dynamic rawPrice = json['Price'];
    final double parsedPrice = rawPrice is num
        ? rawPrice.toDouble()
        : double.tryParse(rawPrice?.toString().replaceAll(',', '') ?? '') ?? 0.0;

    final dynamic rawQuantity = json['Quantity'];
    final int parsedQuantity = rawQuantity is num
        ? rawQuantity.toInt()
        : int.tryParse(rawQuantity?.toString() ?? '') ?? 1;

    return Order(
      Item: (json['Item'] ?? '').toString(),
      ItemName: (json['ItemName'] ?? '').toString(),
      Price: parsedPrice,
      Currency: (json['Currency'] ?? 'USD').toString(),
      Quantity: parsedQuantity,
    );
  }

  /// Converts this [Order] instance to a JSON-compatible map.
  Map<String, dynamic> toJson() {
    return <String, dynamic>{
      'Item': Item,
      'ItemName': ItemName,
      'Price': Price == Price.roundToDouble() ? Price.toInt() : Price,
      'Currency': Currency,
      'Quantity': Quantity,
    };
  }

  /// Parses a JSON array string into a [List<Order>].
  static List<Order> listFromJsonString(String jsonString) {
    final dynamic decoded = jsonDecode(jsonString);
    if (decoded is! List) {
      throw const FormatException('Expected a JSON array of orders.');
    }
    return decoded
        .whereType<Map<String, dynamic>>()
        .map(Order.fromJson)
        .toList();
  }

  /// Encodes a [List<Order>] into a standard JSON array string.
  static String listToJsonString(List<Order> orders, {bool pretty = true}) {
    final List<Map<String, dynamic>> payload =
        orders.map((Order order) => order.toJson()).toList();
    if (pretty) {
      return const JsonEncoder.withIndent('  ').convert(payload);
    }
    return jsonEncode(payload);
  }

  Order copyWith({
    String? Item,
    String? ItemName,
    double? Price,
    String? Currency,
    int? Quantity,
  }) {
    return Order(
      Item: Item ?? this.Item,
      ItemName: ItemName ?? this.ItemName,
      Price: Price ?? this.Price,
      Currency: Currency ?? this.Currency,
      Quantity: Quantity ?? this.Quantity,
    );
  }

  @override
  String toString() {
    return 'Order(Item: $Item, ItemName: $ItemName, Price: $Price, '
        'Currency: $Currency, Quantity: $Quantity)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is Order &&
        other.Item == Item &&
        other.ItemName == ItemName &&
        other.Price == Price &&
        other.Currency == Currency &&
        other.Quantity == Quantity;
  }

  @override
  int get hashCode => Object.hash(Item, ItemName, Price, Currency, Quantity);
}
