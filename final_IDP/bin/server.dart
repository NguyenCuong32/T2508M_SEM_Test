import 'dart:convert';
import 'dart:io';
import '../lib/order.dart';
import '../lib/order_manager.dart';

const defaultPort = 8080;
const orderFilePath = 'order.json';

Future<void> main(List<String> args) async {
  final port = args.isNotEmpty ? int.tryParse(args[0]) ?? defaultPort : defaultPort;
  final server = await HttpServer.bind(InternetAddress.anyIPv4, port);

  print('=' * 65);
  print('🚀 FPT Aptech Dart Server running at:');
  print('   👉 http://localhost:$port');
  print('   👉 http://127.0.0.1:$port');
  print('   Serving web files from: ./web/');
  print('   Managing database file: ./$orderFilePath');
  print('=' * 65);

  await for (HttpRequest request in server) {
    handleRequest(request);
  }
}

Future<void> handleRequest(HttpRequest request) async {
  final path = request.uri.path;
  final method = request.method;

  // Add CORS headers so the web client can communicate seamlessly
  request.response.headers.add('Access-Control-Allow-Origin', '*');
  request.response.headers.add('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  request.response.headers.add('Access-Control-Allow-Headers', 'Origin, Content-Type, Accept');

  if (method == 'OPTIONS') {
    request.response.statusCode = HttpStatus.ok;
    await request.response.close();
    return;
  }

  try {
    // API routes
    if (path == '/api/orders') {
      if (method == 'GET') {
        await handleGetOrders(request);
        return;
      } else if (method == 'POST') {
        await handlePostOrder(request);
        return;
      } else if (method == 'PUT') {
        await handlePutOrder(request);
        return;
      }
    } else if (path.startsWith('/api/orders/')) {
      if (method == 'DELETE') {
        final itemCode = Uri.decodeComponent(path.replaceFirst('/api/orders/', ''));
        await handleDeleteOrder(request, itemCode);
        return;
      } else if (method == 'PUT') {
        await handlePutOrder(request);
        return;
      }
    }

    // Static file serving
    await serveStatic(request);
  } catch (e, stack) {
    print('[ERROR] $method $path: $e\n$stack');
    request.response.statusCode = HttpStatus.internalServerError;
    request.response.headers.contentType = ContentType.json;
    request.response.write(jsonEncode({'error': e.toString()}));
    await request.response.close();
  }
}

/// GET /api/orders
Future<void> handleGetOrders(HttpRequest request) async {
  final file = File(orderFilePath);
  String content = '[]';
  if (await file.exists()) {
    content = await file.readAsString();
  }
  request.response.statusCode = HttpStatus.ok;
  request.response.headers.contentType = ContentType.json;
  request.response.write(content);
  await request.response.close();
}

/// POST /api/orders
/// Inserts an order into order.json
Future<void> handlePostOrder(HttpRequest request) async {
  final body = await utf8.decoder.bind(request).join();
  if (body.trim().isEmpty) {
    request.response.statusCode = HttpStatus.badRequest;
    request.response.write(jsonEncode({'error': 'Request body is empty'}));
    await request.response.close();
    return;
  }

  final dynamic decoded = jsonDecode(body);
  if (decoded is! Map<String, dynamic> && decoded is! Map) {
    request.response.statusCode = HttpStatus.badRequest;
    request.response.write(jsonEncode({'error': 'Expected JSON object for Order'}));
    await request.response.close();
    return;
  }

  final newOrder = Order.fromJson(Map<String, dynamic>.from(decoded as Map));

  // Input validation
  if (newOrder.item.isEmpty || newOrder.itemName.isEmpty || newOrder.price <= 0 || newOrder.quantity <= 0) {
    request.response.statusCode = HttpStatus.badRequest;
    request.response.headers.contentType = ContentType.json;
    request.response.write(jsonEncode({
      'error': 'Validation failed: item and itemName must not be empty, price > 0, quantity >= 1.'
    }));
    await request.response.close();
    return;
  }

  // Load existing orders
  final manager = OrderManager();
  final file = File(orderFilePath);
  if (await file.exists()) {
    final existing = await file.readAsString();
    if (existing.trim().isNotEmpty) {
      manager.loadFromJsonString(existing);
    }
  }

  // Add the new order
  manager.addOrder(newOrder, replaceIfExists: true);

  // Write updated list back to order.json
  await file.writeAsString(manager.toJsonString(pretty: true));
  print('[SERVER] Inserted order: ${newOrder.item} into $orderFilePath');

  request.response.statusCode = HttpStatus.created;
  request.response.headers.contentType = ContentType.json;
  request.response.write(manager.toJsonString(pretty: true));
  await request.response.close();
}

/// PUT /api/orders
/// Updates an existing order in order.json
Future<void> handlePutOrder(HttpRequest request) async {
  final body = await utf8.decoder.bind(request).join();
  if (body.trim().isEmpty) {
    request.response.statusCode = HttpStatus.badRequest;
    request.response.write(jsonEncode({'error': 'Request body is empty'}));
    await request.response.close();
    return;
  }

  final dynamic decoded = jsonDecode(body);
  if (decoded is! Map<String, dynamic> && decoded is! Map) {
    request.response.statusCode = HttpStatus.badRequest;
    request.response.write(jsonEncode({'error': 'Expected JSON object for Order'}));
    await request.response.close();
    return;
  }

  final updatedOrder = Order.fromJson(Map<String, dynamic>.from(decoded as Map));

  // Input validation
  if (updatedOrder.item.isEmpty || updatedOrder.itemName.isEmpty || updatedOrder.price <= 0 || updatedOrder.quantity <= 0) {
    request.response.statusCode = HttpStatus.badRequest;
    request.response.headers.contentType = ContentType.json;
    request.response.write(jsonEncode({
      'error': 'Validation failed: item and itemName must not be empty, price > 0, quantity >= 1.'
    }));
    await request.response.close();
    return;
  }

  final manager = OrderManager();
  final file = File(orderFilePath);
  if (await file.exists()) {
    final existing = await file.readAsString();
    if (existing.trim().isNotEmpty) {
      manager.loadFromJsonString(existing);
    }
  }

  final updated = manager.updateOrder(updatedOrder);
  if (!updated) {
    // If not found, add it
    manager.addOrder(updatedOrder);
  }

  // Save changes to order.json
  await file.writeAsString(manager.toJsonString(pretty: true));
  print('[SERVER] Updated order: ${updatedOrder.item} in $orderFilePath');

  request.response.statusCode = HttpStatus.ok;
  request.response.headers.contentType = ContentType.json;
  request.response.write(manager.toJsonString(pretty: true));
  await request.response.close();
}

/// DELETE /api/orders/:item
Future<void> handleDeleteOrder(HttpRequest request, String itemCode) async {
  final manager = OrderManager();
  final file = File(orderFilePath);
  if (await file.exists()) {
    final existing = await file.readAsString();
    if (existing.trim().isNotEmpty) {
      manager.loadFromJsonString(existing);
    }
  }

  final removed = manager.deleteOrder(itemCode);
  if (removed) {
    await file.writeAsString(manager.toJsonString(pretty: true));
    print('[SERVER] Deleted order: $itemCode from $orderFilePath');
  }

  request.response.statusCode = HttpStatus.ok;
  request.response.headers.contentType = ContentType.json;
  request.response.write(manager.toJsonString(pretty: true));
  await request.response.close();
}

/// Serves static files from ./web/ and ./order.json
Future<void> serveStatic(HttpRequest request) async {
  var path = request.uri.path;
  if (path == '/' || path.isEmpty) {
    path = '/index.html';
  }

  File file;
  if (path == '/order.json') {
    file = File(orderFilePath);
  } else {
    file = File('web$path');
  }

  if (await file.exists()) {
    final mime = getContentType(file.path);
    request.response.headers.contentType = mime;
    await file.openRead().pipe(request.response);
  } else {
    request.response.statusCode = HttpStatus.notFound;
    request.response.headers.contentType = ContentType.html;
    request.response.write('<h1>404 Not Found</h1><p>Path: $path</p>');
    await request.response.close();
  }
}

ContentType getContentType(String path) {
  if (path.endsWith('.html')) return ContentType.html;
  if (path.endsWith('.css')) return ContentType('text', 'css', charset: 'utf-8');
  if (path.endsWith('.js')) return ContentType('application', 'javascript', charset: 'utf-8');
  if (path.endsWith('.json')) return ContentType.json;
  if (path.endsWith('.svg')) return ContentType('image', 'svg+xml');
  if (path.endsWith('.png')) return ContentType('image', 'png');
  return ContentType.binary;
}
