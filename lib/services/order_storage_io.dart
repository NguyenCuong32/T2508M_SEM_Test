import 'dart:convert';
import 'dart:io';

import 'package:path_provider/path_provider.dart';

import '../models/order.dart';
import 'order_storage.dart';

OrderStorage createPlatformOrderStorage() => IoOrderStorage();

/// Native IO implementation of [OrderStorage] for Android (e.g., Pixel 7 Pro)
/// and desktop platforms:
/// - Writes directly to a physical `order.json` file using `dart:io`.
/// - On Android Emulator, also syncs with `http://10.0.2.2:8080/api/orders`
///   if the local Dart server is running, so project `order.json` stays updated.
class IoOrderStorage implements OrderStorage {
  String _resolvedPath = OrderStorage.fileName;

  @override
  bool get supportsBrowserDownload => false;

  @override
  String get storageLocation => _resolvedPath;

  Future<File> _resolveOrderFile() async {
    // On desktop runs inside project root, write directly to ./order.json if present.
    if (!Platform.isAndroid && !Platform.isIOS) {
      final File localProjectFile = File(OrderStorage.fileName);
      if (await localProjectFile.exists()) {
        _resolvedPath = localProjectFile.absolute.path;
        return localProjectFile;
      }
    }

    Directory targetDir;
    try {
      targetDir = await getApplicationDocumentsDirectory();
    } catch (_) {
      targetDir = Directory.systemTemp;
    }

    if (!await targetDir.exists()) {
      await targetDir.create(recursive: true);
    }

    final String fullPath =
        '${targetDir.path}${Platform.pathSeparator}${OrderStorage.fileName}';
    _resolvedPath = fullPath;
    return File(fullPath);
  }

  Future<String?> _tryFetchFromHostServer() async {
    if (!Platform.isAndroid) return null;
    final HttpClient client = HttpClient()
      ..connectionTimeout = const Duration(milliseconds: 1200);
    try {
      final HttpClientRequest req =
          await client.getUrl(Uri.parse('http://10.0.2.2:8080/api/orders'));
      final HttpClientResponse res =
          await req.close().timeout(const Duration(milliseconds: 1500));
      if (res.statusCode == HttpStatus.ok) {
        final String body = await utf8.decoder.bind(res).join();
        if (body.trim().isNotEmpty) return body;
      }
    } catch (_) {
      // Host server not running -> continue with local device order.json.
    } finally {
      client.close(force: true);
    }
    return null;
  }

  Future<void> _trySyncToHostServer(String jsonString) async {
    if (!Platform.isAndroid) return;
    final HttpClient client = HttpClient()
      ..connectionTimeout = const Duration(milliseconds: 1200);
    try {
      final HttpClientRequest req =
          await client.postUrl(Uri.parse('http://10.0.2.2:8080/api/orders'));
      req.headers.contentType = ContentType.json;
      req.write(jsonString);
      await req.close().timeout(const Duration(milliseconds: 1500));
    } catch (_) {
      // Host server not running -> local Android order.json is already saved.
    } finally {
      client.close(force: true);
    }
  }

  @override
  Future<List<Order>> loadOrders(String defaultJson) async {
    final String? serverJson = await _tryFetchFromHostServer();
    if (serverJson != null) {
      try {
        final List<Order> orders = Order.listFromJsonString(serverJson);
        final File file = await _resolveOrderFile();
        await file.writeAsString(serverJson, flush: true);
        return orders;
      } catch (_) {
        // Fall through to local file.
      }
    }

    try {
      final File file = await _resolveOrderFile();
      if (await file.exists()) {
        final String content = await file.readAsString();
        if (content.trim().isNotEmpty) {
          return Order.listFromJsonString(content);
        }
      }
    } catch (_) {
      // Fall back to default JSON if file does not exist or cannot be parsed.
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
    final String jsonString = '${Order.listToJsonString(orders)}\n';
    try {
      final File file = await _resolveOrderFile();
      await file.writeAsString(jsonString, flush: true);
      await _trySyncToHostServer(jsonString);
      return StorageSaveResult(
        success: true,
        location: file.path,
        message: 'Saved directly to ${file.path}',
      );
    } catch (error) {
      return StorageSaveResult(
        success: false,
        location: _resolvedPath,
        message: 'Failed to write ${OrderStorage.fileName}: $error',
      );
    }
  }

  @override
  Future<StorageSaveResult> exportOrders(List<Order> orders) {
    return saveOrders(orders, triggerDownload: true);
  }
}
