import 'dart:convert';
import 'dart:io';

import 'package:my_order_app/order.dart';

const String orderJsonPath = 'order.json';
List<Order> orders = <Order>[];

Future<void> main() async {
  // Load initial orders from order.json or initialize from exam string
  orders = await loadOrdersFromFile(orderJsonPath);
  if (orders.isEmpty) {
    orders = parseOrdersFromString(initialOrdersJson);
    await saveOrdersToFile(orderJsonPath, orders);
  }

  final int port = int.tryParse(Platform.environment['PORT'] ?? '8080') ?? 8080;
  final HttpServer server = await HttpServer.bind(InternetAddress.anyIPv4, port);

  stdout.writeln('===============================================================');
  stdout.writeln('   FPT APTECH - MY ORDER WEB SERVER (DART)');
  stdout.writeln('===============================================================');
  stdout.writeln('Server running at:');
  stdout.writeln('  -> http://localhost:$port');
  stdout.writeln('  -> http://127.0.0.1:$port');
  stdout.writeln('Open the URL in your browser to view the "My Order" website.');
  stdout.writeln('Press Ctrl+C to stop the server.\n');

  await for (final HttpRequest request in server) {
    await handleRequest(request);
  }
}

Future<void> handleRequest(HttpRequest request) async {
  final HttpResponse response = request.response;

  // CORS headers for local access
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
    // API: GET /api/orders (supports ?search=..., ?currency=..., ?sort=...)
    if (path == '/api/orders' && request.method == 'GET') {
      final String searchQuery = request.uri.queryParameters['search'] ?? '';
      final String currency = request.uri.queryParameters['currency'] ?? 'ALL';
      final String sortParam = request.uri.queryParameters['sort'] ?? 'added';

      OrderSortOption sortOption = OrderSortOption.added;
      if (sortParam == 'newest') sortOption = OrderSortOption.newest;
      if (sortParam == 'nameAsc') sortOption = OrderSortOption.nameAsc;
      if (sortParam == 'nameDesc') sortOption = OrderSortOption.nameDesc;
      if (sortParam == 'priceAsc') sortOption = OrderSortOption.priceAsc;
      if (sortParam == 'priceDesc') sortOption = OrderSortOption.priceDesc;

      final List<Order> results = filterAndSortOrders(
        orders,
        query: searchQuery,
        currency: currency,
        sort: sortOption,
      );

      sendJsonResponse(response, HttpStatus.ok, {
        'orders': results.map((Order o) => o.toJson()).toList(),
        'totalCount': orders.length,
      });
      return;
    }

    // API: PUT /api/orders/:index -> Sửa/Cập nhật đơn hàng trong order.json
    if (path.startsWith('/api/orders/') && request.method == 'PUT') {
      final String indexStr = path.substring('/api/orders/'.length);
      final int? index = int.tryParse(indexStr);

      if (index == null || index < 0 || index >= orders.length) {
        sendJsonResponse(response, HttpStatus.badRequest, {
          'error': 'Invalid order index.',
        });
        return;
      }

      final String body = await utf8.decoder.bind(request).join();
      final Map<String, dynamic> data =
          Map<String, dynamic>.from(jsonDecode(body) as Map);

      final Order updated = Order.fromJson(data);
      if (updated.item.isEmpty || updated.itemName.isEmpty) {
        sendJsonResponse(response, HttpStatus.badRequest, {
          'error': 'Item code and Item Name cannot be empty.',
        });
        return;
      }

      orders[index] = updated;
      await saveOrdersToFile(orderJsonPath, orders);

      sendJsonResponse(response, HttpStatus.ok, {
        'message': 'Order successfully updated in order.json',
        'order': updated.toJson(),
        'orders': orders.map((Order o) => o.toJson()).toList(),
      });
      return;
    }

    // API: POST /api/orders -> Insert order into order.json
    if (path == '/api/orders' && request.method == 'POST') {
      final String body = await utf8.decoder.bind(request).join();
      final Map<String, dynamic> data =
          Map<String, dynamic>.from(jsonDecode(body) as Map);

      final Order newOrder = Order.fromJson(data);
      if (newOrder.item.isEmpty || newOrder.itemName.isEmpty) {
        sendJsonResponse(response, HttpStatus.badRequest, {
          'error': 'Item code and Item Name cannot be empty.',
        });
        return;
      }

      orders.add(newOrder);
      await saveOrdersToFile(orderJsonPath, orders);

      sendJsonResponse(response, HttpStatus.created, {
        'message': 'Order successfully inserted into order.json',
        'order': newOrder.toJson(),
        'orders': orders.map((Order o) => o.toJson()).toList(),
      });
      return;
    }

    // API: DELETE /api/orders/:index -> Delete order by index
    if (path.startsWith('/api/orders/') && request.method == 'DELETE') {
      final String indexStr = path.substring('/api/orders/'.length);
      final int? index = int.tryParse(indexStr);

      if (index == null || index < 0 || index >= orders.length) {
        sendJsonResponse(response, HttpStatus.badRequest, {
          'error': 'Invalid order index.',
        });
        return;
      }

      final Order removed = orders.removeAt(index);
      await saveOrdersToFile(orderJsonPath, orders);

      sendJsonResponse(response, HttpStatus.ok, {
        'message': 'Order successfully deleted from order.json',
        'removed': removed.toJson(),
        'orders': orders.map((Order o) => o.toJson()).toList(),
      });
      return;
    }

    // Serve order.json directly
    if (path == '/order.json' && request.method == 'GET') {
      final File orderFile = File(orderJsonPath);
      if (await orderFile.exists()) {
        final String content = await orderFile.readAsString();
        response.statusCode = HttpStatus.ok;
        response.headers.contentType =
            ContentType('application', 'json', charset: 'utf-8');
        response.write(content);
        await response.close();
        return;
      }
    }

    // Serve static files from web/ folder
    await serveStatic(path, response);
  } catch (e) {
    sendJsonResponse(response, HttpStatus.internalServerError, {
      'error': e.toString(),
    });
  }
}

void sendJsonResponse(HttpResponse response, int statusCode, Object data) {
  response.statusCode = statusCode;
  response.headers.contentType =
      ContentType('application', 'json', charset: 'utf-8');
  response.write(jsonEncode(data));
  response.close();
}

Future<void> serveStatic(String requestPath, HttpResponse response) async {
  String safePath = requestPath == '/' ? '/index.html' : requestPath;
  if (safePath.contains('..')) {
    response.statusCode = HttpStatus.forbidden;
    await response.close();
    return;
  }

  final File file = File('web$safePath');
  if (!await file.exists()) {
    response.statusCode = HttpStatus.notFound;
    response.write('404 Not Found');
    await response.close();
    return;
  }

  response.statusCode = HttpStatus.ok;
  response.headers.contentType = getContentType(safePath);
  await response.addStream(file.openRead());
  await response.close();
}

ContentType getContentType(String path) {
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
  } else if (path.endsWith('.png')) {
    return ContentType('image', 'png');
  } else if (path.endsWith('.svg')) {
    return ContentType('image', 'svg+xml', charset: 'utf-8');
  }
  return ContentType.binary;
}
