import 'dart:convert';
import 'order.dart';

/// Service class responsible for managing the collection of [Order]s.
/// Handles:
///   - Loading orders from JSON string
///   - Serializing orders to JSON string
///   - Adding/inserting new orders
///   - Deleting orders
///   - Searching orders by ItemName (case-insensitive substring match)
class OrderManager {
  final List<Order> _orders = [];

  /// Returns an unmodifiable list of current orders
  List<Order> get orders => List.unmodifiable(_orders);

  /// Number of orders in the manager
  int get count => _orders.length;

  /// Loads orders from a JSON string.
  /// Example JSON:
  ///   '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity":1},...]'
  void loadFromJsonString(String jsonString) {
    if (jsonString.trim().isEmpty) return;

    final dynamic parsed = jsonDecode(jsonString);
    if (parsed is List) {
      _orders.clear();
      for (final item in parsed) {
        if (item is Map<String, dynamic>) {
          _orders.add(Order.fromJson(item));
        } else if (item is Map) {
          _orders.add(Order.fromJson(Map<String, dynamic>.from(item)));
        }
      }
    } else {
      throw FormatException('Expected JSON array of orders, got: ${parsed.runtimeType}');
    }
  }

  /// Serializes the current orders list to a JSON string
  String toJsonString({bool pretty = true}) {
    final list = _orders.map((o) => o.toJson()).toList();
    if (pretty) {
      const encoder = JsonEncoder.withIndent('  ');
      return encoder.convert(list);
    }
    return jsonEncode(list);
  }

  /// Inserts a new order into the collection.
  /// Throws [ArgumentError] if order with same Item SKU already exists (optional validation)
  /// or if item/itemName is empty.
  void addOrder(Order order, {bool replaceIfExists = false}) {
    if (order.item.trim().isEmpty) {
      throw ArgumentError('Order Item (SKU) cannot be empty.');
    }
    if (order.itemName.trim().isEmpty) {
      throw ArgumentError('Order ItemName cannot be empty.');
    }
    if (order.price < 0) {
      throw ArgumentError('Order Price cannot be negative.');
    }
    if (order.quantity <= 0) {
      throw ArgumentError('Order Quantity must be greater than zero.');
    }

    final existingIndex = _orders.indexWhere(
      (o) => o.item.toLowerCase() == order.item.toLowerCase(),
    );

    if (existingIndex >= 0) {
      if (replaceIfExists) {
        _orders[existingIndex] = order;
      } else {
        // Just append or replace
        _orders[existingIndex] = order;
      }
    } else {
      _orders.add(order);
    }
  }

  /// Updates an existing order.
  /// Returns `true` if order was found and updated, `false` otherwise.
  bool updateOrder(Order updatedOrder) {
    if (updatedOrder.item.trim().isEmpty) {
      throw ArgumentError('Order Item (SKU) cannot be empty.');
    }
    if (updatedOrder.itemName.trim().isEmpty) {
      throw ArgumentError('Order ItemName cannot be empty.');
    }
    if (updatedOrder.price < 0) {
      throw ArgumentError('Order Price cannot be negative.');
    }
    if (updatedOrder.quantity <= 0) {
      throw ArgumentError('Order Quantity must be greater than zero.');
    }

    final index = _orders.indexWhere(
      (o) => o.item.toLowerCase() == updatedOrder.item.toLowerCase(),
    );

    if (index >= 0) {
      _orders[index] = updatedOrder;
      return true;
    }
    return false;
  }

  /// Removes an order by its Item SKU
  bool deleteOrder(String item) {
    final initialLength = _orders.length;
    _orders.removeWhere((o) => o.item.toLowerCase() == item.toLowerCase());
    return _orders.length < initialLength;
  }

  /// Searches orders by ItemName (case-insensitive substring match)
  /// If [query] is empty, returns all orders.
  List<Order> searchByItemName(String query) {
    final cleanQuery = query.trim().toLowerCase();
    if (cleanQuery.isEmpty) {
      return List.unmodifiable(_orders);
    }
    return _orders.where((o) => o.itemName.toLowerCase().contains(cleanQuery)).toList();
  }

  /// Finds an order by its exact Item SKU
  Order? findByItem(String item) {
    try {
      return _orders.firstWhere((o) => o.item.toLowerCase() == item.toLowerCase());
    } catch (_) {
      return null;
    }
  }

  /// Clears all orders
  void clear() {
    _orders.clear();
  }
}
