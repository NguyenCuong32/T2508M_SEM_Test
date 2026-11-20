import 'dart:convert';
import 'dart:io';

import 'package:my_order_app/order.dart';

/// List of Order objects stored in memory and synchronized with order.json
List<Order> orders = <Order>[];

final File orderJsonFile = File('order.json');

/// Load orders from the exam's JSON string and/or order.json file
Future<void> initializeOrders() async {
  // 1. Read the content from the exam string and save objects to list orders
  orders = parseOrdersFromString(initialOrdersJson);

  // 2. If order.json already exists and is non-empty, read from order.json;
  //    otherwise create order.json with the initial orders list.
  if (await orderJsonFile.exists()) {
    try {
      final String fileContent = await orderJsonFile.readAsString();
      if (fileContent.trim().isNotEmpty) {
        orders = parseOrdersFromString(fileContent);
      } else {
        await saveOrdersToFile();
      }
    } catch (_) {
      await saveOrdersToFile();
    }
  } else {
    await saveOrdersToFile();
  }
}

/// Persist the current list of orders into order.json
Future<void> saveOrdersToFile() async {
  final String jsonContent = encodeOrdersToJson(orders);
  await orderJsonFile.writeAsString('$jsonContent\n');
}

Future<void> main(List<String> args) async {
  await initializeOrders();

  final int port = int.tryParse(Platform.environment['PORT'] ?? '') ?? 8080;
  final HttpServer server = await HttpServer.bind(InternetAddress.anyIPv4, port);
  stdout.writeln('===========================================================');
  stdout.writeln('My Order Website is running at: http://localhost:$port');
  stdout.writeln('Loaded ${orders.length} order(s) into List<Order> orders.');
  stdout.writeln('===========================================================');

  await for (final HttpRequest request in server) {
    await _handleRequest(request);
  }
}

Future<void> _handleRequest(HttpRequest request) async {
  final HttpResponse response = request.response;

  // Enable CORS headers for flexible local development
  response.headers.set('Access-Control-Allow-Origin', '*');
  response.headers.set('Access-Control-Allow-Methods', 'GET, POST, DELETE, OPTIONS');
  response.headers.set('Access-Control-Allow-Headers', 'Content-Type');

  if (request.method == 'OPTIONS') {
    response.statusCode = HttpStatus.noContent;
    await response.close();
    return;
  }

  final String path = request.uri.path;

  try {
    // REST API: GET /api/orders?search=...
    if (path == '/api/orders' && request.method == 'GET') {
      final String searchQuery = request.uri.queryParameters['search'] ?? '';
      final List<Order> filteredOrders = searchOrdersByItemName(orders, searchQuery);
      _sendJson(response, HttpStatus.ok, {
        'orders': filteredOrders.map((Order o) => o.toJson()).toList(),
        'total': orders.length,
      });
      return;
    }

    // REST API: POST /api/orders -> Insert an order into order.json
    if (path == '/api/orders' && request.method == 'POST') {
      final String body = await utf8.decoder.bind(request).join();
      final Map<String, dynamic> data =
          Map<String, dynamic>.from(jsonDecode(body) as Map);

      final Order newOrder = Order.fromJson(data);
      if (newOrder.item.trim().isEmpty || newOrder.itemName.trim().isEmpty) {
        _sendJson(response, HttpStatus.badRequest, {
          'error': 'Item and ItemName are required.',
        });
        return;
      }

      orders.add(newOrder);
      await saveOrdersToFile();

      _sendJson(response, HttpStatus.created, {
        'message': 'Order inserted into order.json successfully.',
        'order': newOrder.toJson(),
        'orders': orders.map((Order o) => o.toJson()).toList(),
      });
      return;
    }

    // REST API: DELETE /api/orders/:index -> Delete an order from order.json
    if (path.startsWith('/api/orders/') && request.method == 'DELETE') {
      final String indexPart = path.substring('/api/orders/'.length);
      final int? index = int.tryParse(indexPart);

      if (index == null || index < 0 || index >= orders.length) {
        _sendJson(response, HttpStatus.badRequest, {
          'error': 'Invalid order index.',
        });
        return;
      }

      final Order removed = orders.removeAt(index);
      await saveOrdersToFile();

      _sendJson(response, HttpStatus.ok, {
        'message': 'Order deleted from order.json.',
        'removed': removed.toJson(),
        'orders': orders.map((Order o) => o.toJson()).toList(),
      });
      return;
    }

    // Serve order.json directly if requested
    if (path == '/order.json' && request.method == 'GET') {
      if (await orderJsonFile.exists()) {
        final String content = await orderJsonFile.readAsString();
        response.statusCode = HttpStatus.ok;
        response.headers.contentType =
            ContentType('application', 'json', charset: 'utf-8');
        response.write(content);
        await response.close();
        return;
      }
    }

    // Serve static files from web/
    await _serveStaticFile(path, response);
  } catch (e) {
    _sendJson(response, HttpStatus.internalServerError, {
      'error': e.toString(),
    });
  }
}

void _sendJson(HttpResponse response, int statusCode, Object payload) {
  response.statusCode = statusCode;
  response.headers.contentType =
      ContentType('application', 'json', charset: 'utf-8');
  response.write(jsonEncode(payload));
  response.close();
}

Future<void> _serveStaticFile(String requestPath, HttpResponse response) async {
  String relativePath = requestPath == '/' ? '/index.html' : requestPath;
  // Prevent directory traversal
  if (relativePath.contains('..')) {
    response.statusCode = HttpStatus.forbidden;
    await response.close();
    return;
  }

  final File file = File('web$relativePath');
  if (!await file.exists()) {
    response.statusCode = HttpStatus.notFound;
    response.write('404 Not Found');
    await response.close();
    return;
  }

  response.statusCode = HttpStatus.ok;
  response.headers.contentType = _getContentType(relativePath);
  await response.addStream(file.openRead());
  await response.close();
}

ContentType _getContentType(String path) {
  if (path.endsWith('.html')) {
    return ContentType('text', 'html', charset: 'utf-8');
  } else if (path.endsWith('.css')) {
    return ContentType('text', 'css', charset: 'utf-8');
  } else if (path.endsWith('.js')) {
    return ContentType('application', 'javascript', charset: 'utf-8');
  } else if (path.endsWith('.json')) {
    return ContentType('application', 'json', charset: 'utf-8');
  } else if (path.endsWith('.dart')) {
    return ContentType('application', 'dart', charset: 'utf-8');
  }
  return ContentType.binary;
}
