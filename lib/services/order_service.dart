import 'dart:convert';
import '../models/order.dart';
import 'storage/storage.dart';

class OrderService {
  static const String initialJsonString =
      '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": 1500,"Currency": "USD","Quantity":1}]';

  final StorageInterface _storage;
  List<Order> _orders = [];

  OrderService({StorageInterface? storage})
      : _storage = storage ?? createStorage();

  List<Order> get orders => List.unmodifiable(_orders);

  List<Order> parseOrdersFromJsonString(String jsonString) {
    final dynamic decoded = jsonDecode(jsonString);
    if (decoded is List) {
      return decoded
          .map((item) => Order.fromJson(item as Map<String, dynamic>))
          .toList();
    }
    return [];
  }

  String encodeOrdersToJsonString(List<Order> list) {
    final mapped = list.map((e) => e.toJson()).toList();
    return const JsonEncoder.withIndent('  ').convert(mapped);
  }

  Future<List<Order>> loadOrders() async {
    final content = await _storage.readOrderJson();
    if (content.trim().isNotEmpty) {
      try {
        _orders = parseOrdersFromJsonString(content);
        return _orders;
      } catch (_) {}
    }
    _orders = parseOrdersFromJsonString(initialJsonString);
    await saveOrdersToJsonFile();
    return _orders;
  }

  Future<void> saveOrdersToJsonFile() async {
    final jsonStr = encodeOrdersToJsonString(_orders);
    await _storage.writeOrderJson(jsonStr);
  }

  Future<bool> insertOrder(Order newOrder) async {
    _orders.add(newOrder);
    await saveOrdersToJsonFile();
    return true;
  }

  Future<bool> deleteOrder(int index) async {
    if (index >= 0 && index < _orders.length) {
      _orders.removeAt(index);
      await saveOrdersToJsonFile();
      return true;
    }
    return false;
  }

  Future<bool> deleteOrderByItem(String itemCode) async {
    final initialLength = _orders.length;
    _orders.removeWhere((element) => element.item == itemCode);
    if (_orders.length != initialLength) {
      await saveOrdersToJsonFile();
      return true;
    }
    return false;
  }

  List<Order> searchOrderByItemName(String query) {
    final trimmed = query.trim().toLowerCase();
    if (trimmed.isEmpty) {
      return _orders;
    }
    return _orders
        .where((order) => order.itemName.toLowerCase().contains(trimmed))
        .toList();
  }
}

