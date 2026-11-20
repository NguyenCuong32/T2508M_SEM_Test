import '../models/order.dart';
import 'order_storage_stub.dart'
    if (dart.library.js_interop) 'order_storage_web.dart'
    if (dart.library.io) 'order_storage_io.dart';

/// Result returned when persisting or exporting `order.json`.
class StorageSaveResult {
  final bool success;
  final String location;
  final String message;

  const StorageSaveResult({
    required this.success,
    required this.location,
    required this.message,
  });
}

/// Platform-agnostic contract for loading, saving, and exporting `order.json`.
abstract class OrderStorage {
  static const String fileName = 'order.json';

  /// Whether this platform runs in a web browser (supports direct file download).
  bool get supportsBrowserDownload;

  /// Human-readable description of where `order.json` is stored.
  String get storageLocation;

  /// Loads persisted orders or falls back to [defaultJson] if none exist yet.
  Future<List<Order>> loadOrders(String defaultJson);

  /// Persists [orders] as JSON into `order.json`.
  Future<StorageSaveResult> saveOrders(
    List<Order> orders, {
    bool triggerDownload = false,
  });

  /// Explicitly exports `order.json` (downloads on Web, writes file on Android/IO).
  Future<StorageSaveResult> exportOrders(List<Order> orders);

  /// Creates the appropriate [OrderStorage] for the current platform.
  factory OrderStorage() => createPlatformOrderStorage();
}
