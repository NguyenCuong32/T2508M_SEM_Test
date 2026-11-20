import 'dart:convert';
import 'dart:io';

import 'package:my_order/order.dart';

/// Path to order.json (next to pubspec.yaml).
final File orderFile =
    File.fromUri(Platform.script.resolve('../order.json'));

/// The JSON string given in the exam paper.
const String jsonString = '''
[
  {
    "Item": "A1000",
    "ItemName": "Iphone 15",
    "Price": 1200,
    "Currency": "USD",
    "Quantity": 1
  },
  {
    "Item": "A1001",
    "ItemName": "Iphone 16",
    "Price": 1500,
    "Currency": "USD",
    "Quantity": 1
  }
]
''';

/// 2. List of orders read from the JSON string.
List<Order> orders = [];

/// Currencies available in the form.
const List<String> currencies = ['USD', 'EUR', 'VND', 'JPY'];

/// Reads the JSON string and converts it into Order objects.
List<Order> parseOrders(String jsonString) {
  final List<dynamic> data = jsonDecode(jsonString);
  return data.map((e) => Order.fromJson(e as Map<String, dynamic>)).toList();
}

/// Writes all orders to order.json.
Future<void> saveOrders() async {
  const encoder = JsonEncoder.withIndent('  ');
  await orderFile
      .writeAsString(encoder.convert(orders.map((o) => o.toJson()).toList()));
}

Future<void> main() async {
  // Read the content from the JSON string and save the objects to the orders list.
  orders = parseOrders(jsonString);

  // order.json keeps inserted orders. On first run it is created from the JSON string;
  // later runs reload the file so inserted orders are not lost.
  if (await orderFile.exists()) {
    orders = parseOrders(await orderFile.readAsString());
  } else {
    await saveOrders();
  }

  final server = await HttpServer.bind(InternetAddress.loopbackIPv4, 8080);
  print('Server running at http://localhost:${server.port}');

  await for (final HttpRequest request in server) {
    try {
      await handleRequest(request);
    } catch (e) {
      request.response
        ..statusCode = HttpStatus.internalServerError
        ..write('Error: $e');
      await request.response.close();
    }
  }
}

Future<void> handleRequest(HttpRequest request) async {
  final path = request.uri.path;

  // 3 + 5. Show the list, with search by ItemName.
  if (request.method == 'GET' && path == '/') {
    final query = request.uri.queryParameters;
    await sendHtml(
      request,
      buildPage(
        keyword: query['search'] ?? '',
        added: query['added'],
        deleted: query['deleted'],
      ),
    );
    return;
  }

  // Action: delete an order and rewrite order.json.
  if (request.method == 'POST' && path == '/delete') {
    final body = await utf8.decoder.bind(request).join();
    final form = Uri.splitQueryString(body);
    final index = int.tryParse(form['index'] ?? '');
    final search = form['search'] ?? '';

    String location = '/';
    if (index != null && index >= 0 && index < orders.length) {
      final removed = orders.removeAt(index);
      await saveOrders();
      location = '/?deleted=${Uri.encodeQueryComponent(removed.item)}';
      if (search.isNotEmpty) {
        location += '&search=${Uri.encodeQueryComponent(search)}';
      }
    }

    request.response
      ..statusCode = HttpStatus.seeOther
      ..headers.set(HttpHeaders.locationHeader, location);
    await request.response.close();
    return;
  }

  // 4. Insert an order into order.json.
  if (request.method == 'POST' && path == '/add') {
    final body = await utf8.decoder.bind(request).join();
    final form = Uri.splitQueryString(body);

    final item = (form['item'] ?? '').trim();
    final itemName = (form['itemName'] ?? '').trim();
    final price = double.tryParse(form['price'] ?? '');
    final quantity = int.tryParse(form['quantity'] ?? '');
    final currency = (form['currency'] ?? 'USD').trim().toUpperCase();

    if (!currencies.contains(currency) ||
        item.isEmpty ||
        itemName.isEmpty ||
        price == null ||
        price < 0 ||
        quantity == null ||
        quantity <= 0) {
      await sendHtml(
        request,
        buildPage(
          error: 'Invalid data. Enter Item, Item Name, '
              'Price (≥ 0) and Quantity (≥ 1).',
          form: form,
        ),
        status: HttpStatus.badRequest,
      );
      return;
    }

    orders.add(Order(
      item: item,
      itemName: itemName,
      price: price,
      currency: currency,
      quantity: quantity,
    ));
    await saveOrders();

    request.response
      ..statusCode = HttpStatus.seeOther
      ..headers.set(HttpHeaders.locationHeader,
          '/?added=${Uri.encodeQueryComponent(item)}');
    await request.response.close();
    return;
  }

  request.response
    ..statusCode = HttpStatus.notFound
    ..write('404 Not Found');
  await request.response.close();
}

Future<void> sendHtml(HttpRequest request, String html,
    {int status = HttpStatus.ok}) async {
  request.response
    ..statusCode = status
    ..headers.contentType = ContentType.html
    ..write(html);
  await request.response.close();
}

String esc(String s) => const HtmlEscape().convert(s);

/// Money format: 1200 -> "$1,200.00" (USD), others -> "1,200.00 EUR".
String formatMoney(double value, String currency) {
  final parts = value.toStringAsFixed(2).split('.');
  final digits = parts[0];
  final buf = StringBuffer();
  for (var i = 0; i < digits.length; i++) {
    if (i > 0 && (digits.length - i) % 3 == 0 && digits[i - 1] != '-') {
      buf.write(',');
    }
    buf.write(digits[i]);
  }
  final number = '$buf.${parts[1]}';
  return currency == 'USD' ? '\$$number' : '$number $currency';
}


// ----------------------------------------------------------------------------
// UI (HTML + CSS + a little JS for instant search)
// ----------------------------------------------------------------------------

String _svg(String body, [int size = 20]) =>
    '<svg viewBox="0 0 24 24" width="$size" height="$size" fill="none" '
    'stroke="currentColor" stroke-width="2" stroke-linecap="round" '
    'stroke-linejoin="round" aria-hidden="true">$body</svg>';

const String _cartPath =
    '<circle cx="9" cy="20" r="1.5"/><circle cx="18" cy="20" r="1.5"/>'
    '<path d="M2 3h3l2.7 12.4a2 2 0 0 0 2 1.6h8.6a2 2 0 0 0 2-1.6L22 7H6"/>';
final String icCart = _svg(_cartPath);
final String icCartSm = _svg(_cartPath, 18);
final String icPlus =
    _svg('<circle cx="12" cy="12" r="9"/><path d="M12 8v8M8 12h8"/>', 18);
final String icTrash = _svg(
    '<path d="M4 7h16M10 11v6M14 11v6M5 7l1 12a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2l1-12M9 7V4h6v3"/>',
    16);
final String icSearch =
    _svg('<circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/>', 16);
final String icReceipt = _svg(
    '<path d="M6 3h12v18l-3-2-3 2-3-2-3 2z"/><path d="M9 8h6M9 12h6"/>');
final String icBox = _svg(
    '<path d="M12 3 3 7.5v9L12 21l9-4.5v-9z"/><path d="m3 7.5 9 4.5 9-4.5M12 12v9"/>');
final String icMoney = _svg(
    '<rect x="2" y="6" width="20" height="12" rx="2"/><circle cx="12" cy="12" r="2.5"/><path d="M6 10v4M18 10v4"/>');
final String icCheck = _svg('<path d="m5 12 5 5 9-10"/>', 18);
final String icAlert =
    _svg('<circle cx="12" cy="12" r="9"/><path d="M12 8v5M12 16h.01"/>', 18);

/// Avatar colors based on the first letter of the item name.
const List<List<String>> _avatarColors = [
  ['#e8f0fe', '#1958c4'],
  ['#e6f4ea', '#1a7f37'],
  ['#fef3e2', '#a15c07'],
  ['#f3e8fd', '#7e3bbf'],
  ['#fde8ef', '#b4235a'],
  ['#e0f5f5', '#0e7470'],
];

String avatar(String name) {
  final letter = name.isEmpty ? '?' : name[0].toUpperCase();
  final c = _avatarColors[letter.codeUnitAt(0) % _avatarColors.length];
  return '<span class="avatar" style="background:${c[0]};color:${c[1]}">'
      '${esc(letter)}</span>';
}

/// Sums amounts per currency: "$2,700.00 · 800.00 EUR".
String sumByCurrency(Iterable<Order> list) {
  final totals = <String, double>{};
  for (final o in list) {
    totals[o.currency] = (totals[o.currency] ?? 0) + o.price * o.quantity;
  }
  if (totals.isEmpty) return formatMoney(0, 'USD');
  return totals.entries.map((e) => formatMoney(e.value, e.key)).join(' · ');
}

String buildPage({
  String keyword = '',
  String? added,
  String? deleted,
  String? error,
  Map<String, String> form = const {},
}) {
  final kw = keyword.trim();
  final filtered = kw.isEmpty
      ? orders
      : orders
          .where((o) => o.itemName.toLowerCase().contains(kw.toLowerCase()))
          .toList();

  final totalQty = orders.fold<int>(0, (s, o) => s + o.quantity);
  final filteredQty = filtered.fold<int>(0, (s, o) => s + o.quantity);

  final rows = StringBuffer();
  for (var i = 0; i < filtered.length; i++) {
    final o = filtered[i];
    final isNew = added != null && o.item == added;
    rows.writeln(
        '<tr class="item-row${isNew ? ' new' : ''}" '
        'data-name="${esc(o.itemName.toLowerCase())}" '
        'data-qty="${o.quantity}" data-sub="${o.price * o.quantity}" '
        'data-cur="${esc(o.currency)}">'
        '<td class="muted">${i + 1}</td>'
        '<td><span class="code">${esc(o.item)}</span></td>'
        '<td><div class="product">${avatar(o.itemName)}'
        '<span class="name">${esc(o.itemName)}</span></div></td>'
        '<td class="num">${o.quantity}</td>'
        '<td class="num">${formatMoney(o.price, o.currency)}</td>'
        '<td><span class="badge">${esc(o.currency)}</span></td>'
        '<td class="num strong">${formatMoney(o.price * o.quantity, o.currency)}</td>'
        '<td class="action">'
        '<form method="POST" action="/delete" '
        'data-msg="Delete ${esc(o.item)} from the cart?" '
        'onsubmit="return confirm(this.dataset.msg)">'
        '<input type="hidden" name="index" value="${orders.indexOf(o)}">'
        '<input type="hidden" name="search" value="${esc(kw)}">'
        '<button type="submit" class="btn-del" title="Delete" '
        'aria-label="Delete ${esc(o.item)}">$icTrash</button>'
        '</form></td>'
        '</tr>');
  }

  final emptyText = kw.isEmpty
      ? 'Your cart is empty. Add an item to get started.'
      : 'No orders found for “${esc(kw)}”.';

  String v(String key) => esc(form[key] ?? '');

  final toast = error != null
      ? '<div class="toast err" role="alert">$icAlert<span>${esc(error)}</span></div>'
      : added != null
          ? '<div class="toast ok" role="status">$icCheck<span>Added <b>${esc(added)}</b> to the cart</span></div>'
          : deleted != null
              ? '<div class="toast ok" role="status">$icCheck<span>Deleted <b>${esc(deleted)}</b> from the cart</span></div>'
              : '';

  final selectedCurrency = form['currency'] ?? 'USD';
  final currencyOptions = currencies
      .map((c) =>
          '<option value="$c"${c == selectedCurrency ? ' selected' : ''}>$c</option>')
      .join();

  final infoText = kw.isEmpty
      ? 'All orders (${orders.length})'
      : '${filtered.length} result${filtered.length == 1 ? '' : 's'} for “${esc(kw)}”';

  return '''
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>My Order</title>
  <style>
    :root {
      --bg: #f4f5f7; --card: #ffffff; --text: #1f2328; --muted: #6b7280;
      --faint: #9ca3af; --border: #e5e7eb; --soft: #f9fafb;
      --primary: #1f6feb; --primary-dark: #1958c4; --primary-soft: #e8f0fe;
      --ok: #1a7f37; --ok-soft: #e6f4ea; --err: #c62828; --err-soft: #fdecea;
      --err-border: #f5c2c0;
    }
    * { box-sizing: border-box; }
    html, body { height: 100%; }
    body { margin: 0; background: var(--bg); color: var(--text); display: flex;
      flex-direction: column; font-family: "Segoe UI", Roboto, Arial, sans-serif;
      font-size: 15px; }
    a { color: var(--primary); text-decoration: none; }
    a:hover { text-decoration: underline; }
    .container { width: 100%; max-width: 1600px; margin: 0 auto; padding: 0 32px; }

    header { background: var(--card); border-bottom: 1px solid var(--border); }
    .bar { display: flex; align-items: center; justify-content: space-between; height: 64px; }
    .brand { display: flex; align-items: center; gap: 10px; }
    .logo { width: 38px; height: 38px; border-radius: 10px; background: var(--primary);
      color: #fff; display: flex; align-items: center; justify-content: center; }
    .brand h1 { margin: 0; font-size: 20px; font-weight: 600; }
    .cart-btn { position: relative; width: 42px; height: 42px; border-radius: 50%;
      border: 1px solid var(--border); display: flex; align-items: center;
      justify-content: center; color: var(--text); }
    .cart-count { position: absolute; top: -4px; right: -4px; min-width: 20px; height: 20px;
      padding: 0 5px; border-radius: 999px; background: var(--primary); color: #fff;
      font-size: 11px; font-weight: 700; display: flex; align-items: center;
      justify-content: center; border: 2px solid var(--card); }

    /* main fills the remaining height so the cards reach the footer */
    main { flex: 1; display: flex; padding: 24px 0; }
    main > .container { display: flex; flex-direction: column; }
    .stats { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; margin-bottom: 20px; }
    .stat { background: var(--card); border: 1px solid var(--border); border-radius: 12px;
      padding: 18px; display: flex; align-items: center; gap: 14px; }
    .stat-icon { width: 44px; height: 44px; border-radius: 12px; flex-shrink: 0;
      display: flex; align-items: center; justify-content: center; }
    .stat p { margin: 0 0 4px; color: var(--muted); font-size: 13px; }
    .stat strong { font-size: 22px; font-weight: 600; }

    .grid { flex: 1; display: grid; grid-template-columns: 380px minmax(0, 1fr); gap: 20px; }
    .card { background: var(--card); border: 1px solid var(--border); border-radius: 12px; padding: 24px; }
    aside { display: flex; flex-direction: column; }
    .form-card { flex: 1; display: flex; flex-direction: column; }
    .form-card form { flex: 1; display: flex; flex-direction: column; }
    .form-note { margin-top: auto; padding-top: 18px; color: var(--muted); font-size: 13px;
      line-height: 1.5; border-top: 1px dashed var(--border); }
    .form-note b { color: var(--text); font-weight: 600; }
    .card h2 { margin: 0 0 18px; font-size: 16px; font-weight: 600;
      display: flex; align-items: center; gap: 8px; }

    label { display: block; font-size: 13px; font-weight: 500; color: var(--text); margin: 0 0 6px; }
    .field { margin-bottom: 14px; }
    .row2 { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    input, select { width: 100%; height: 40px; padding: 0 12px; font: inherit; color: var(--text);
      border: 1px solid var(--border); border-radius: 8px; background: #fff; outline: none;
      transition: border-color .15s, box-shadow .15s; }
    input::placeholder { color: var(--faint); opacity: 1; }
    select { cursor: pointer; }
    input:focus, select:focus { border-color: var(--primary); box-shadow: 0 0 0 3px var(--primary-soft); }
    button { height: 40px; padding: 0 16px; font: inherit; font-weight: 600; border-radius: 8px;
      cursor: pointer; display: inline-flex; align-items: center; justify-content: center; gap: 6px; }
    .btn-primary { width: 100%; margin-top: 6px; border: none; background: var(--primary); color: #fff; }
    .btn-primary:hover { background: var(--primary-dark); }
    .btn-outline { border: 1px solid var(--border); background: #fff; color: var(--text); }
    .btn-outline:hover { border-color: var(--primary); color: var(--primary); }

    .list-card { display: flex; flex-direction: column; }
    .search { display: flex; gap: 8px; margin-bottom: 12px; }
    .search-box { position: relative; flex: 1; }
    .search-box svg { position: absolute; left: 12px; top: 12px; color: var(--faint); }
    .search-box input { padding-left: 36px; }
    .info { color: var(--muted); font-size: 13px; margin-bottom: 12px; }

    .table-wrap { flex: 1; overflow: auto; border: 1px solid var(--border); border-radius: 10px; }
    .summary { margin-top: 16px; padding: 16px 20px; border-radius: 10px; background: var(--soft);
      border: 1px solid var(--border); display: flex; align-items: center;
      justify-content: space-between; gap: 16px; flex-wrap: wrap; }
    .summary .items { display: flex; gap: 32px; color: var(--muted); font-size: 14px; }
    .summary .items b { display: block; color: var(--text); font-size: 18px; font-weight: 600; }
    .summary .grand { text-align: right; color: var(--muted); font-size: 14px; }
    .summary .grand b { display: block; color: var(--primary); font-size: 24px; font-weight: 700; }
    table { width: 100%; border-collapse: collapse; }
    th { text-align: left; font-size: 12px; font-weight: 600; color: var(--muted);
      padding: 11px 14px; background: var(--soft); border-bottom: 1px solid var(--border);
      white-space: nowrap; }
    td { padding: 15px 14px; border-bottom: 1px solid var(--border); vertical-align: middle; }
    tbody tr:last-child td { border-bottom: none; }
    tbody tr.item-row:hover { background: var(--soft); }
    tr.new { background: var(--ok-soft); }
    .num { text-align: right; white-space: nowrap; }
    .strong { font-weight: 600; }
    .muted { color: var(--muted); }
    .product { display: flex; align-items: center; gap: 10px; }
    .avatar { width: 34px; height: 34px; border-radius: 8px; flex-shrink: 0; display: flex;
      align-items: center; justify-content: center; font-weight: 700; font-size: 15px; }
    .name { font-weight: 500; }
    .code { font-family: Consolas, "Courier New", monospace; font-size: 13px;
      background: #f3f4f6; padding: 2px 6px; border-radius: 6px; }
    .badge { font-size: 12px; font-weight: 600; color: var(--primary);
      background: var(--primary-soft); padding: 2px 8px; border-radius: 999px; }
    th.action, td.action { text-align: center; width: 70px; }
    td.action form { margin: 0; }
    .btn-del { width: 34px; height: 34px; padding: 0; border: 1px solid var(--err-border);
      background: #fff; color: var(--err); }
    .btn-del:hover { background: var(--err-soft); border-color: var(--err); }
    .empty { text-align: center; color: var(--muted); padding: 40px 14px; }
    .hidden { display: none; }

    .toast { position: fixed; top: 76px; right: 24px; z-index: 10; max-width: 380px;
      display: flex; align-items: center; gap: 10px; padding: 12px 16px; border-radius: 10px;
      font-size: 14px; box-shadow: 0 8px 24px rgba(0,0,0,.12);
      animation: toast-in .25s ease-out, toast-out .3s ease-in 3s forwards; }
    .toast svg { flex-shrink: 0; }
    .toast.ok { background: var(--ok-soft); color: var(--ok); border: 1px solid #b7dfc2; }
    .toast.err { background: var(--err-soft); color: var(--err); border: 1px solid var(--err-border);
      animation: toast-in .25s ease-out; }
    @keyframes toast-in { from { opacity: 0; transform: translateY(-8px); } }
    @keyframes toast-out { to { opacity: 0; transform: translateY(-8px); visibility: hidden; } }

    footer { border-top: 1px solid var(--border); background: var(--card);
      color: var(--muted); font-size: 13px; }
    footer .container { padding-top: 16px; padding-bottom: 16px; text-align: center; }

    @media (max-width: 900px) {
      .grid { grid-template-columns: minmax(0, 1fr); }
      .stats { grid-template-columns: minmax(0, 1fr); }
      .container { padding: 0 16px; }
      .summary .grand { text-align: left; }
    }
  </style>
</head>
<body>
  $toast

  <header>
    <div class="container bar">
      <div class="brand">
        <div class="logo">$icCart</div>
        <h1>My Order</h1>
      </div>
      <div class="cart-btn" title="${orders.length} orders in the cart">
        $icCart<span class="cart-count">${orders.length}</span>
      </div>
    </div>
  </header>

  <main>
    <div class="container">
      <section class="stats">
        <div class="stat">
          <div class="stat-icon" style="background:#e8f0fe;color:#1958c4">$icReceipt</div>
          <div><p>Orders</p><strong>${orders.length}</strong></div>
        </div>
        <div class="stat">
          <div class="stat-icon" style="background:#fef3e2;color:#a15c07">$icBox</div>
          <div><p>Total quantity</p><strong>$totalQty</strong></div>
        </div>
        <div class="stat">
          <div class="stat-icon" style="background:#e6f4ea;color:#1a7f37">$icMoney</div>
          <div><p>Total amount</p><strong>${sumByCurrency(orders)}</strong></div>
        </div>
      </section>

      <div class="grid">
        <aside>
          <section class="card form-card">
            <h2>$icPlus Add item</h2>
            <form method="POST" action="/add">
              <div class="field">
                <label for="item">Item</label>
                <input id="item" name="item" placeholder="e.g. A1002" value="${v('item')}" required>
              </div>
              <div class="field">
                <label for="itemName">Item Name</label>
                <input id="itemName" name="itemName" placeholder="e.g. Iphone 15 Pro" value="${v('itemName')}" required>
              </div>
              <div class="row2">
                <div class="field">
                  <label for="price">Price</label>
                  <input id="price" name="price" type="number" min="0" step="any" placeholder="0.00" value="${v('price')}" required>
                </div>
                <div class="field">
                  <label for="quantity">Quantity</label>
                  <input id="quantity" name="quantity" type="number" min="1" step="1" placeholder="1" value="${v('quantity')}" required>
                </div>
              </div>
              <div class="field">
                <label for="currency">Currency</label>
                <select id="currency" name="currency">$currencyOptions</select>
              </div>
              <button type="submit" class="btn-primary">$icCartSm Add Item to Cart</button>
              <p class="form-note">Orders are saved to <b>order.json</b>. Item, Item Name, Price and Quantity are required.</p>
            </form>
          </section>
        </aside>

        <section class="card list-card">
          <form method="GET" action="/" class="search">
            <div class="search-box">
              $icSearch
              <input id="search" name="search" placeholder="Search by item name…" value="${esc(kw)}" autocomplete="off">
            </div>
            <button type="submit" class="btn-outline">Search</button>
          </form>
          <div class="info"><span id="info">$infoText</span><span id="show-all" class="${kw.isEmpty ? 'hidden' : ''}"> · <a href="/">Show all</a></span></div>
          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Id</th><th>Item</th><th>Item Name</th>
                  <th class="num">Quantity</th><th class="num">Price</th><th>Currency</th>
                  <th class="num">Subtotal</th><th class="action">Action</th>
                </tr>
              </thead>
              <tbody>
                $rows
                <tr id="empty-row" class="${filtered.isEmpty ? '' : 'hidden'}"><td colspan="8" class="empty">$emptyText</td></tr>
              </tbody>
            </table>
          </div>
          <div class="summary">
            <div class="items">
              <div>Items<b id="total-count">${filtered.length}</b></div>
              <div>Quantity<b id="total-qty">$filteredQty</b></div>
            </div>
            <div class="grand">Total<b id="total-amount">${sumByCurrency(filtered)}</b></div>
          </div>
        </section>
      </div>
    </div>
  </main>

  <footer>
    <div class="container">My Order · Introduction to Dart Programming</div>
  </footer>

  <script>
    // Instant search: filter visible rows while typing (the Search button still submits to the server).
    (function () {
      var input = document.getElementById('search');
      var rows = Array.prototype.slice.call(document.querySelectorAll('tr.item-row'));
      var info = document.getElementById('info');
      var showAll = document.getElementById('show-all');
      var emptyRow = document.getElementById('empty-row');
      var totalCount = document.getElementById('total-count');
      var totalQty = document.getElementById('total-qty');
      var totalAmount = document.getElementById('total-amount');
      var initialInfo = info.textContent;
      var initialQ = input.value.trim().toLowerCase();

      function money(v, cur) {
        var s = v.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        return cur === 'USD' ? '\$' + s : s + ' ' + cur;
      }

      input.addEventListener('input', function () {
        var q = input.value.trim().toLowerCase();
        var count = 0, qty = 0, sums = {}, order = [];
        rows.forEach(function (r) {
          var match = r.dataset.name.indexOf(q) !== -1;
          r.classList.toggle('hidden', !match);
          if (match) {
            count++;
            qty += Number(r.dataset.qty);
            var cur = r.dataset.cur;
            if (!(cur in sums)) { sums[cur] = 0; order.push(cur); }
            sums[cur] += Number(r.dataset.sub);
          }
        });
        if (q === initialQ) {
          info.textContent = initialInfo;
        } else if (q === '') {
          info.textContent = 'Showing ' + count + ' of ' + rows.length + ' loaded orders';
        } else {
          info.textContent = count + (count === 1 ? ' result' : ' results') + ' for “' + input.value.trim() + '”';
        }
        showAll.classList.toggle('hidden', q === '' && initialQ === '');
        emptyRow.classList.toggle('hidden', count !== 0);
        if (count === 0) {
          emptyRow.firstElementChild.textContent = q === ''
            ? 'Your cart is empty. Add an item to get started.'
            : 'No orders found for “' + input.value.trim() + '”.';
        }
        totalCount.textContent = count;
        totalQty.textContent = qty;
        totalAmount.textContent = order.length
          ? order.map(function (c) { return money(sums[c], c); }).join(' · ')
          : money(0, 'USD');
      });
    })();
  </script>
</body>
</html>
''';
}
