import 'package:flutter/foundation.dart';

import '../../data/models/order_model.dart';
import '../../data/repositories/order_repository.dart';

class OrderController extends ChangeNotifier {
  OrderController(this._repository);

  final OrderRepository _repository;
  List<Order> _orders = [];
  String _query = '';
  bool _loading = true;
  bool _saving = false;
  bool _loadFailed = false;
  bool _disposed = false;

  int get orderCount => _orders.length;
  bool get hasQuery => _query.isNotEmpty;
  bool get loading => _loading;
  bool get saving => _saving;
  bool get loadFailed => _loadFailed;

  Set<String> get existingItems => _orders.map((order) => order.item).toSet();
  List<Order> get visibleOrders => Order.search(_orders, _query);
  int get quantities =>
      _orders.fold(0, (total, order) => total + order.quantity);

  Map<String, double> get totals {
    final totals = <String, double>{};
    for (final order in _orders) {
      totals.update(
        order.currency,
        (value) => value + order.total,
        ifAbsent: () => order.total,
      );
    }
    return totals;
  }

  void search(String query) {
    if (query == _query) return;
    _query = query;
    notifyListeners();
  }

  Future<void> load() async {
    _loading = true;
    _loadFailed = false;
    notifyListeners();
    try {
      _orders = await _repository.load();
    } catch (_) {
      _loadFailed = true;
    } finally {
      _loading = false;
      if (!_disposed) notifyListeners();
    }
  }

  Future<bool> addOrder(Order order) =>
      _persist([..._orders, order], clearQuery: true);

  Future<bool> deleteOrder(Order order) =>
      _persist(_orders.where((entry) => entry != order).toList());

  Future<bool> _persist(List<Order> next, {bool clearQuery = false}) async {
    _saving = true;
    notifyListeners();
    try {
      await _repository.save(next);
      _orders = next;
      if (clearQuery) _query = '';
      return true;
    } catch (_) {
      return false;
    } finally {
      _saving = false;
      if (!_disposed) notifyListeners();
    }
  }

  @override
  void dispose() {
    _disposed = true;
    super.dispose();
  }
}
