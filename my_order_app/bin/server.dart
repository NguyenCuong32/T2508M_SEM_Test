import 'dart:convert';
import 'dart:io';
import 'package:shelf/shelf.dart';
import 'package:shelf/shelf_io.dart' as io;
import 'package:shelf_router/shelf_router.dart';
import 'package:shelf_cors_headers/shelf_cors_headers.dart';

// Path to order.json (relative to where server is run from)
final String _jsonPath =
    '${Directory.current.path}/web/order.json';

Future<void> main() async {
  final router = Router();

  // GET /api/orders — return all orders
  router.get('/api/orders', (Request req) async {
    final file = File(_jsonPath);
    if (!await file.exists()) {
      return Response.ok('[]', headers: {'Content-Type': 'application/json'});
    }
    final content = await file.readAsString();
    return Response.ok(content, headers: {'Content-Type': 'application/json'});
  });

  // POST /api/orders — receive full orders list and write to file
  router.post('/api/orders', (Request req) async {
    final body = await req.readAsString();
    // Validate JSON
    try {
      jsonDecode(body);
    } catch (_) {
      return Response(400, body: 'Invalid JSON');
    }
    final file = File(_jsonPath);
    // Pretty-print before saving
    final pretty =
        const JsonEncoder.withIndent('  ').convert(jsonDecode(body));
    await file.writeAsString(pretty);
    return Response.ok('{"status":"ok"}',
        headers: {'Content-Type': 'application/json'});
  });

  final handler = Pipeline()
      .addMiddleware(corsHeaders()) // allow web app on :8080 to call :8081
      .addMiddleware(logRequests())
      .addHandler(router.call);

  final server = await io.serve(handler, 'localhost', 8081);
  print('✅  Order API running at http://${server.address.host}:${server.port}');
  print('   Writing to: $_jsonPath');
}
