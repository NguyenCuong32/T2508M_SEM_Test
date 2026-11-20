import '../models/order.dart';
import 'order_storage.dart';

OrderStorage createPlatformOrderStorage() => _MemoryOrderStorage();

class _MemoryOrderStorage implements OrderStorage {
  String? _cachedJson;

  @override
  bool get supportsBrowserDownload => false;

  @override
  String get storageLocation => 'In-memory (${OrderStorage.fileName})';

  @override
  Future<List<Order>> loadOrders(String defaultJson) async {
    final String source = _cachedJson ?? defaultJson;
    return Order.listFromJsonString(source);
  }

  @override
  Future<StorageSaveResult> saveOrders(
    List<Order> orders, {
    bool triggerDownload = false,
  }) async {
    _cachedJson = Order.listToJsonString(orders);
    return const StorageSaveResult(
      success: true,
      location: OrderStorage.fileName,
      message: 'Saved to in-memory order.json',
    );
  }

  @override
  Future<StorageSaveResult> exportOrders(List<Order> orders) {
    return saveOrders(orders, triggerDownload: true);
  }
}
