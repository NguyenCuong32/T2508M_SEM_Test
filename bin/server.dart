import 'dart:convert';
import 'dart:io';

class Order {
  const Order({
    required this.item,
    required this.itemName,
    required this.price,
    required this.currency,
    required this.quantity,
  });

  final String item;
  final String itemName;
  final double price;
  final String currency;
  final int quantity;

  factory Order.fromJson(Map<String, dynamic> json) => Order(
        item: json['Item'] as String,
        itemName: json['ItemName'] as String,
        price: (json['Price'] as num).toDouble(),
        currency: json['Currency'] as String,
        quantity: (json['Quantity'] as num).toInt(),
      );

  Map<String, Object> toJson() => {
        'Item': item,
        'ItemName': itemName,
        'Price': price,
        'Currency': currency,
        'Quantity': quantity,
      };
}

Future<void> main(List<String> args) async {
  final root = Directory.current;
  final dataFile = File('${root.path}${Platform.pathSeparator}order.json');
  final webRoot = Directory('${root.path}${Platform.pathSeparator}web');
  if (!await dataFile.exists()) {
    await dataFile.writeAsString(
      jsonEncode([
        const Order(
          item: 'A1000',
          itemName: 'Iphone 15',
          price: 1200,
          currency: 'USD',
          quantity: 1,
        ).toJson(),
        const Order(
          item: 'A1001',
          itemName: 'Iphone 16',
          price: 1500,
          currency: 'USD',
          quantity: 1,
        ).toJson(),
      ]),
    );
  }

  final port = int.tryParse(Platform.environment['PORT'] ?? '') ?? 8080;
  final server = await HttpServer.bind(InternetAddress.loopbackIPv4, port);
  stdout.writeln('Orderly is ready at http://${server.address.address}:$port');
  await for (final request in server) {
    try {
      await _handle(request, dataFile, webRoot);
    } catch (error, stack) {
      stderr.writeln('Request failed: $error\n$stack');
      request.response.statusCode = HttpStatus.internalServerError;
      request.response.headers.contentType = ContentType.json;
      request.response.write(jsonEncode({'error': 'Something went wrong.'}));
      await request.response.close();
    }
  }
}

Future<void> _handle(
  HttpRequest request,
  File dataFile,
  Directory webRoot,
) async {
  final response = request.response;
  response.headers.add('X-Content-Type-Options', 'nosniff');
  response.headers.add('Cache-Control', 'no-store');

  if (request.uri.path == '/api/orders' && request.method == 'GET') {
    final raw = jsonDecode(await dataFile.readAsString()) as List<dynamic>;
    final orders = raw
        .map((entry) => Order.fromJson(entry as Map<String, dynamic>))
        .toList();
    final search =
        request.uri.queryParameters['search']?.trim().toLowerCase() ?? '';
    final filtered = orders
        .where((order) => order.itemName.toLowerCase().contains(search))
        .toList();
    response.headers.contentType = ContentType.json;
    response.write(
      jsonEncode(filtered.map((order) => order.toJson()).toList()),
    );
    await response.close();
    return;
  }

  if (request.uri.path == '/api/orders' && request.method == 'DELETE') {
    final id = request.uri.queryParameters['id'] ?? '';
    final raw = jsonDecode(await dataFile.readAsString()) as List<dynamic>;
    final orders = raw
        .map((entry) => Order.fromJson(entry as Map<String, dynamic>))
        .toList();
    final index = orders.indexWhere((order) => order.item == id);
    response.headers.contentType = ContentType.json;
    if (index < 0) {
      response.statusCode = HttpStatus.notFound;
      response.write(jsonEncode({'error': 'Order not found.'}));
    } else {
      orders.removeAt(index);
      await dataFile.writeAsString(const JsonEncoder.withIndent('  ')
          .convert(orders.map((order) => order.toJson()).toList()));
      response.write(jsonEncode({'success': true}));
    }
    await response.close();
    return;
  }

  if (request.uri.path == '/api/orders' &&
      (request.method == 'POST' || request.method == 'PUT')) {
    final body = await utf8.decoder.bind(request).join();
    final payload = jsonDecode(body) as Map<String, dynamic>;
    final order = Order.fromJson(payload);
    if (order.item.trim().isEmpty ||
        order.itemName.trim().isEmpty ||
        order.price <= 0 ||
        order.quantity <= 0 ||
        order.currency.trim().isEmpty) {
      response.statusCode = HttpStatus.badRequest;
      response.headers.contentType = ContentType.json;
      response.write(jsonEncode({'error': 'Enter valid order details.'}));
      await response.close();
      return;
    }
    final raw = jsonDecode(await dataFile.readAsString()) as List<dynamic>;
    final orders = raw
        .map((entry) => Order.fromJson(entry as Map<String, dynamic>))
        .toList();
    final id = request.uri.queryParameters['id'];
    final index = request.method == 'PUT'
        ? orders.indexWhere((existing) => existing.item == id)
        : -1;
    if (request.method == 'PUT' && index < 0) {
      response.statusCode = HttpStatus.notFound;
      response.headers.contentType = ContentType.json;
      response.write(jsonEncode({'error': 'Order not found.'}));
      await response.close();
      return;
    }
    if (orders.asMap().entries.any(
          (entry) =>
              entry.key != index &&
              entry.value.item.toLowerCase() == order.item.toLowerCase(),
        )) {
      response.statusCode = HttpStatus.conflict;
      response.headers.contentType = ContentType.json;
      response.write(jsonEncode({'error': 'That order ID already exists.'}));
      await response.close();
      return;
    }
    if (request.method == 'PUT') {
      orders[index] = order;
    } else {
      orders.insert(0, order);
    }
    await dataFile.writeAsString(
      const JsonEncoder.withIndent('  ')
          .convert(orders.map((value) => value.toJson()).toList()),
    );
    response.statusCode =
        request.method == 'POST' ? HttpStatus.created : HttpStatus.ok;
    response.headers.contentType = ContentType.json;
    response.write(jsonEncode(order.toJson()));
    await response.close();
    return;
  }

  final requested =
      request.uri.path == '/' ? 'index.html' : request.uri.path.substring(1);
  if (requested.contains('..') ||
      requested.contains('/') ||
      requested.contains('\\')) {
    response.statusCode = HttpStatus.notFound;
    await response.close();
    return;
  }
  final file = File('${webRoot.path}${Platform.pathSeparator}$requested');
  if (!await file.exists()) {
    response.statusCode = HttpStatus.notFound;
    response.write('Not found');
    await response.close();
    return;
  }
  response.headers.contentType = switch (requested.split('.').last) {
    'html' => ContentType.html,
    'css' => ContentType('text', 'css', charset: 'utf-8'),
    'js' => ContentType('application', 'javascript', charset: 'utf-8'),
    _ => ContentType.binary,
  };
  await response.addStream(file.openRead());
  await response.close();
}
