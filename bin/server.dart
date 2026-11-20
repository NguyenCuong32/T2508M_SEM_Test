import 'dart:convert';
import 'dart:io';

import 'package:my_order/data/default_orders.dart';

/// Pure Dart HTTP server that:
/// 1. Serves the Flutter Web build (`build/web`) on http://localhost:8080
/// 2. Reads & writes orders directly to `order.json` on disk via:
///    - GET  /api/orders  -> reads order.json
///    - POST /api/orders  -> saves updated JSON array to order.json
Future<void> main(List<String> args) async {
  final int port = args.isNotEmpty ? (int.tryParse(args[0]) ?? 8080) : 8080;
  final Directory projectRoot = Directory.current;
  final File orderFile =
      File('${projectRoot.path}${Platform.pathSeparator}order.json');
  final Directory webDir =
      Directory('${projectRoot.path}${Platform.pathSeparator}build${Platform.pathSeparator}web');

  await _ensureOrderFileExists(orderFile);

  final HttpServer server =
      await HttpServer.bind(InternetAddress.anyIPv4, port);
  stdout.writeln('=================================================');
  stdout.writeln(' My Order Server running at http://localhost:$port');
  stdout.writeln(' Order JSON file : ${orderFile.absolute.path}');
  stdout.writeln(' Web build folder: ${webDir.absolute.path}');
  stdout.writeln('=================================================');

  await for (final HttpRequest request in server) {
    try {
      if (request.uri.path == '/api/orders') {
        await _handleOrdersApi(request, orderFile);
      } else {
        await _handleStaticFile(request, webDir);
      }
    } catch (error) {
      request.response.statusCode = HttpStatus.internalServerError;
      request.response.headers.contentType = ContentType.json;
      request.response.write(jsonEncode(<String, dynamic>{
        'error': error.toString(),
      }));
      await request.response.close();
    }
  }
}

Future<void> _ensureOrderFileExists(File orderFile) async {
  if (!await orderFile.exists() ||
      (await orderFile.readAsString()).trim().isEmpty) {
    final dynamic decoded = jsonDecode(defaultOrdersJson);
    final String pretty = const JsonEncoder.withIndent('  ').convert(decoded);
    await orderFile.writeAsString('$pretty\n', flush: true);
  }
}

void _applyCorsHeaders(HttpResponse response) {
  response.headers
    ..set('Access-Control-Allow-Origin', '*')
    ..set('Access-Control-Allow-Methods', 'GET, POST, PUT, OPTIONS')
    ..set('Access-Control-Allow-Headers', 'Content-Type')
    ..set('Cache-Control', 'no-store');
}

Future<void> _handleOrdersApi(HttpRequest request, File orderFile) async {
  _applyCorsHeaders(request.response);

  if (request.method == 'OPTIONS') {
    request.response.statusCode = HttpStatus.noContent;
    await request.response.close();
    return;
  }

  if (request.method == 'GET') {
    await _ensureOrderFileExists(orderFile);
    final String content = await orderFile.readAsString();
    request.response.statusCode = HttpStatus.ok;
    request.response.headers.contentType = ContentType.json;
    request.response.write(content);
    await request.response.close();
    return;
  }

  if (request.method == 'POST' || request.method == 'PUT') {
    final String body = await utf8.decoder.bind(request).join();
    final dynamic decoded = jsonDecode(body);
    if (decoded is! List) {
      request.response.statusCode = HttpStatus.badRequest;
      request.response.headers.contentType = ContentType.json;
      request.response.write(jsonEncode(<String, dynamic>{
        'saved': false,
        'error': 'Payload must be a JSON array of orders.',
      }));
      await request.response.close();
      return;
    }

    final String prettyJson =
        '${const JsonEncoder.withIndent('  ').convert(decoded)}\n';
    await orderFile.writeAsString(prettyJson, flush: true);
    stdout.writeln(
      '[order.json] Saved ${decoded.length} item(s) -> ${orderFile.path}',
    );

    request.response.statusCode = HttpStatus.ok;
    request.response.headers.contentType = ContentType.json;
    request.response.write(jsonEncode(<String, dynamic>{
      'saved': true,
      'count': decoded.length,
      'file': orderFile.path,
    }));
    await request.response.close();
    return;
  }

  request.response.statusCode = HttpStatus.methodNotAllowed;
  await request.response.close();
}

Future<void> _handleStaticFile(HttpRequest request, Directory webDir) async {
  String reqPath = request.uri.path;
  if (reqPath == '/' || reqPath.isEmpty) {
    reqPath = '/index.html';
  }

  // Prevent directory traversal outside build/web
  final String normalized = reqPath.replaceAll('\\', '/');
  if (normalized.contains('..')) {
    request.response.statusCode = HttpStatus.forbidden;
    await request.response.close();
    return;
  }

  File file = File('${webDir.path}$normalized');
  if (!await file.exists()) {
    file = File('${webDir.path}/index.html');
  }

  if (await file.exists()) {
    request.response.headers.contentType = _contentTypeFor(file.path);
    request.response.headers.set('Cache-Control', 'no-cache');
    await request.response.addStream(file.openRead());
  } else {
    request.response.statusCode = HttpStatus.notFound;
    request.response.write(
      'Web build not found. Run "flutter build web" first.',
    );
  }
  await request.response.close();
}

ContentType _contentTypeFor(String path) {
  final String lower = path.toLowerCase();
  if (lower.endsWith('.html')) return ContentType.html;
  if (lower.endsWith('.js') || lower.endsWith('.mjs')) {
    return ContentType('application', 'javascript', charset: 'utf-8');
  }
  if (lower.endsWith('.css')) {
    return ContentType('text', 'css', charset: 'utf-8');
  }
  if (lower.endsWith('.json')) return ContentType.json;
  if (lower.endsWith('.png')) return ContentType('image', 'png');
  if (lower.endsWith('.svg')) return ContentType('image', 'svg+xml');
  if (lower.endsWith('.wasm')) return ContentType('application', 'wasm');
  if (lower.endsWith('.ttf')) return ContentType('font', 'ttf');
  if (lower.endsWith('.otf')) return ContentType('font', 'otf');
  if (lower.endsWith('.woff2')) return ContentType('font', 'woff2');
  return ContentType.binary;
}
