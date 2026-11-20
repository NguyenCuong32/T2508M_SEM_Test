import 'dart:js_interop';

import 'package:web/web.dart' as web;

import '../models/order.dart';
import 'order_storage.dart';

OrderStorage createPlatformOrderStorage() => WebOrderStorage();

/// Web implementation of [OrderStorage].
/// - Primary persistence: reads & writes directly to the physical `order.json`
///   file via the local Dart server (`GET/POST /api/orders`).
/// - Fallback persistence: mirrors into `localStorage['order.json']` and
///   supports one-click browser download of `order.json`.
class WebOrderStorage implements OrderStorage {
  static const String _storageKey = OrderStorage.fileName;
  String _location = OrderStorage.fileName;

  @override
  bool get supportsBrowserDownload => true;

  @override
  String get storageLocation => _location;

  String get _apiUrl {
    final Uri base = Uri.base;
    if ((base.host == 'localhost' || base.host == '127.0.0.1') &&
        base.port != 8080) {
      return 'http://localhost:8080/api/orders';
    }
    return '/api/orders';
  }

  @override
  Future<List<Order>> loadOrders(String defaultJson) async {
    // 1. Try loading directly from project order.json via Dart server.
    try {
      final web.Response response = await web.window.fetch(_apiUrl.toJS).toDart;
      if (response.ok) {
        final String body = (await response.text().toDart).toDart;
        if (body.trim().isNotEmpty) {
          final List<Order> parsed = Order.listFromJsonString(body);
          web.window.localStorage.setItem(_storageKey, body);
          _location = OrderStorage.fileName;
          return parsed;
        }
      }
    } catch (_) {
      // Server not running -> fall back cleanly to localStorage.
    }

    // 2. Fallback to browser localStorage['order.json'].
    try {
      final String? stored = web.window.localStorage.getItem(_storageKey);
      if (stored != null && stored.trim().isNotEmpty) {
        _location = "localStorage['$_storageKey']";
        return Order.listFromJsonString(stored);
      }
    } catch (_) {
      // Fall back to exam default JSON if localStorage is invalid.
    }

    final List<Order> defaults = Order.listFromJsonString(defaultJson);
    await saveOrders(defaults);
    return defaults;
  }

  @override
  Future<StorageSaveResult> saveOrders(
    List<Order> orders, {
    bool triggerDownload = false,
  }) async {
    final String jsonString = Order.listToJsonString(orders);

    // Always mirror in localStorage so browser reloads never lose state.
    try {
      web.window.localStorage.setItem(_storageKey, jsonString);
    } catch (_) {
      // Ignore storage quota / privacy mode errors.
    }

    // Write directly to physical order.json file via Dart server.
    bool savedToFile = false;
    try {
      final web.Headers headers = web.Headers();
      headers.set('Content-Type', 'application/json');
      final web.Response response = await web.window
          .fetch(
            _apiUrl.toJS,
            web.RequestInit(
              method: 'POST',
              headers: headers,
              body: jsonString.toJS,
            ),
          )
          .toDart;
      savedToFile = response.ok;
    } catch (_) {
      savedToFile = false;
    }

    if (triggerDownload) {
      _downloadJsonFile(jsonString);
    }

    if (savedToFile) {
      _location = OrderStorage.fileName;
      return StorageSaveResult(
        success: true,
        location: OrderStorage.fileName,
        message: triggerDownload
            ? 'Saved to $_storageKey & downloaded file'
            : 'Saved directly to $_storageKey',
      );
    }

    _location = "localStorage['$_storageKey']";
    return StorageSaveResult(
      success: true,
      location: _location,
      message: triggerDownload
          ? 'Downloaded $_storageKey & synced to browser storage'
          : 'Saved to browser $_storageKey',
    );
  }

  @override
  Future<StorageSaveResult> exportOrders(List<Order> orders) async {
    return saveOrders(orders, triggerDownload: true);
  }

  void _downloadJsonFile(String jsonContent) {
    final web.Blob blob = web.Blob(
      <JSString>[jsonContent.toJS].toJS,
      web.BlobPropertyBag(type: 'application/json;charset=utf-8'),
    );
    final String url = web.URL.createObjectURL(blob);
    final web.HTMLAnchorElement anchor =
        web.document.createElement('a') as web.HTMLAnchorElement
          ..href = url
          ..download = _storageKey
          ..style.display = 'none';
    web.document.body?.append(anchor);
    anchor.click();
    anchor.remove();
    web.URL.revokeObjectURL(url);
  }
}
