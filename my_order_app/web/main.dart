import 'dart:convert';
import 'dart:js_interop';
import 'package:web/web.dart';

// ─── Class Order ────────────────────────────────────────────────
class Order {
  int id;
  String item;
  String itemName;
  double price;
  int quantity;
  String currency;

  Order({
    required this.id,
    required this.item,
    required this.itemName,
    required this.price,
    required this.quantity,
    required this.currency,
  });

  factory Order.fromJson(Map<String, dynamic> json, int id) {
    return Order(
      id: id,
      item: json['Item'] as String,
      itemName: json['ItemName'] as String,
      price: (json['Price'] as num).toDouble(),
      quantity: json['Quantity'] as int,
      currency: json['Currency'] as String,
    );
  }

  Map<String, dynamic> toJson() => {
        'Item': item,
        'ItemName': itemName,
        'Price': price,
        'Quantity': quantity,
        'Currency': currency,
      };
}

// ─── Config ──────────────────────────────────────────────────────
const String _apiUrl = 'http://localhost:8081/api/orders';

// ─── Global state ────────────────────────────────────────────────
List<Order> orders = [];
int _editingId = -1;

// ─── Main ────────────────────────────────────────────────────────
void main() {
  _loadOrders();
  _bindEvents();
}

// ─── Load orders from API (order.json via server) ─────────────────
void _loadOrders() {
  window
      .fetch(_apiUrl.toJS)
      .toDart
      .then((res) => res.text().toDart)
      .then((jsText) {
    _parseAndRender(jsText.toDart);
  }).catchError((_) {
    _showError('Cannot connect to server. Run: dart run bin/server.dart');
    orders = [];
    _renderTable(orders);
    _updateStats();
  });
}

void _parseAndRender(String jsonStr) {
  final List<dynamic> jsonList = jsonDecode(jsonStr) as List<dynamic>;
  orders = jsonList
      .asMap()
      .entries
      .map((e) => Order.fromJson(e.value as Map<String, dynamic>, e.key + 1))
      .toList();
  _renderTable(orders);
  _updateStats();
}

// ─── Save orders list → POST to API → writes order.json ──────────
void _saveOrders() {
  final body = jsonEncode(orders.map((o) => o.toJson()).toList());

  final init = RequestInit(
    method: 'POST',
    body: body.toJS,
    headers: {
      'Content-Type': 'application/json',
    }.jsify() as JSObject,
  );

  window.fetch(_apiUrl.toJS, init).toDart.catchError((_) {
    _showError('Save failed. Is the server running?');
    return Response(''.toJS);
  });
}

// ─── Render table ────────────────────────────────────────────────
void _renderTable(List<Order> list) {
  final tbody =
      document.querySelector('#order-tbody') as HTMLTableSectionElement;

  while (tbody.firstChild != null) {
    tbody.removeChild(tbody.firstChild!);
  }

  if (list.isEmpty) {
    final row = tbody.insertRow(-1);
    final cell = row.insertCell(0);
    cell.colSpan = 7;
    final div = document.createElement('div') as HTMLElement;
    div.className = 'empty-state';
    div.innerHTML =
        '<div class="empty-icon">📭</div><div class="empty-text">No orders found.</div>';
    cell.appendChild(div);
    return;
  }

  for (final order in list) {
    final row = tbody.insertRow(-1);
    if (order.id == _editingId) row.className = 'editing-row';

    _addCell(row, order.id.toString());
    _addCell(row, order.item);
    _addCell(row, order.itemName);
    _addCell(row, order.quantity.toString());
    _addCell(row, order.price.toStringAsFixed(0));
    _addCell(row, order.currency, badge: true);

    final actionCell = row.insertCell(6);
    actionCell.className = 'action-cell';

    final editBtn = document.createElement('button') as HTMLButtonElement;
    editBtn.className = 'edit-btn';
    editBtn.innerHTML = '&#9998;';
    editBtn.title = 'Edit';
    final orderId = order.id;
    editBtn.onClick.listen((_) => _startEdit(orderId));
    actionCell.appendChild(editBtn);

    final delBtn = document.createElement('button') as HTMLButtonElement;
    delBtn.className = 'delete-btn';
    delBtn.innerHTML = '&#128465;';
    delBtn.title = 'Delete';
    delBtn.onClick.listen((_) {
      if (_editingId == orderId) _cancelEdit();
      orders.removeWhere((o) => o.id == orderId);
      _saveOrders();
      _updateStats();
      _applySearch();
    });
    actionCell.appendChild(delBtn);
  }
}

void _addCell(HTMLTableRowElement row, String text, {bool badge = false}) {
  final cell = row.insertCell(-1);
  if (badge) {
    final span = document.createElement('span') as HTMLElement;
    span.className = 'badge';
    span.textContent = text;
    cell.appendChild(span);
  } else {
    cell.textContent = text;
  }
}

// ─── Stats ───────────────────────────────────────────────────────
void _updateStats() {
  final totalItems = orders.fold<int>(0, (s, o) => s + o.quantity);
  final totalValue =
      orders.fold<double>(0, (s, o) => s + o.price * o.quantity);
  final currencies = orders.map((o) => o.currency).toSet().length;

  _setText('#stat-total', orders.length.toString());
  _setText('#stat-items', totalItems.toString());
  _setText('#stat-value', '\$${totalValue.toStringAsFixed(0)}');
  _setText('#stat-currency', currencies.toString());
}

void _setText(String sel, String val) {
  final el = document.querySelector(sel) as HTMLElement?;
  if (el != null) el.textContent = val;
}

// ─── Bind events ─────────────────────────────────────────────────
void _bindEvents() {
  _doc('#btn-add').onClick.listen((_) => _submitForm());
  _doc('#btn-cancel-edit').onClick.listen((_) => _cancelEdit());

  final inputs = document.querySelectorAll('.form-input');
  for (int i = 0; i < inputs.length; i++) {
    final el = inputs.item(i) as HTMLInputElement;
    el.onKeyDown.listen((e) {
      if (e.keyCode == 13) _submitForm();
      if (e.keyCode == 27) _cancelEdit();
    });
  }

  (_doc('#search-input') as HTMLInputElement)
      .onInput
      .listen((_) => _applySearch());
}

// ─── Edit mode ───────────────────────────────────────────────────
void _startEdit(int id) {
  final order = orders.firstWhere((o) => o.id == id);
  _editingId = id;

  (_doc('#input-item') as HTMLInputElement).value = order.item;
  (_doc('#input-item-name') as HTMLInputElement).value = order.itemName;
  (_doc('#input-price') as HTMLInputElement).value =
      order.price.toStringAsFixed(0);
  (_doc('#input-quantity') as HTMLInputElement).value =
      order.quantity.toString();
  (_doc('#input-currency') as HTMLInputElement).value = order.currency;
  (_doc('#edit-id') as HTMLInputElement).value = id.toString();

  _setText('#form-panel-title', '✏️ Edit Order #$id');
  _setText('#btn-add-label', 'Update Order');
  _setText('#btn-add-icon', '✓');
  (_doc('#btn-add') as HTMLElement).className = 'btn-primary btn-update';
  (_doc('#btn-cancel-edit') as HTMLElement).style.display = 'flex';
  (_doc('#form-panel') as HTMLElement).className = 'panel panel-editing';

  (_doc('#form-panel') as HTMLElement).scrollIntoView();
  (_doc('#input-item') as HTMLInputElement).focus();

  _applySearch();
}

void _cancelEdit() {
  _editingId = -1;
  (_doc('#edit-id') as HTMLInputElement).value = '-1';
  _resetForm();
  _applySearch();
}

// ─── Submit (Add or Update) ───────────────────────────────────────
void _submitForm() {
  final editId =
      int.tryParse((_doc('#edit-id') as HTMLInputElement).value) ?? -1;
  if (editId == -1) {
    _insertOrder();
  } else {
    _updateOrder(editId);
  }
}

void _insertOrder() {
  final v = _readFormValues();
  if (v == null) return;

  final newId = orders.isEmpty
      ? 1
      : orders.map((o) => o.id).reduce((a, b) => a > b ? a : b) + 1;

  orders.add(Order(
    id: newId,
    item: v['item']!,
    itemName: v['itemName']!,
    price: double.parse(v['price']!),
    quantity: int.parse(v['qty']!),
    currency: v['currency']!,
  ));

  _saveOrders(); // ← ghi vào order.json
  _resetForm();
  _updateStats();
  _applySearch();
  _showSuccess('Order added & saved to order.json!');
}

void _updateOrder(int id) {
  final v = _readFormValues();
  if (v == null) return;

  final index = orders.indexWhere((o) => o.id == id);
  if (index == -1) return;

  orders[index]
    ..item = v['item']!
    ..itemName = v['itemName']!
    ..price = double.parse(v['price']!)
    ..quantity = int.parse(v['qty']!)
    ..currency = v['currency']!;

  _editingId = -1;
  _saveOrders(); // ← ghi vào order.json
  _resetForm();
  _updateStats();
  _applySearch();
  _showSuccess('Order #$id updated & saved to order.json!');
}

Map<String, String>? _readFormValues() {
  final item = (_doc('#input-item') as HTMLInputElement).value.trim();
  final itemName =
      (_doc('#input-item-name') as HTMLInputElement).value.trim();
  final priceStr = (_doc('#input-price') as HTMLInputElement).value.trim();
  final qtyStr = (_doc('#input-quantity') as HTMLInputElement).value.trim();
  final currency =
      (_doc('#input-currency') as HTMLInputElement).value.trim();

  _clearMessage();

  if (item.isEmpty || itemName.isEmpty || priceStr.isEmpty || qtyStr.isEmpty) {
    _showError('Please fill in all required fields.');
    return null;
  }
  final price = double.tryParse(priceStr);
  if (price == null || price < 0) {
    _showError('Price must be a valid non-negative number.');
    return null;
  }
  final qty = int.tryParse(qtyStr);
  if (qty == null || qty < 1) {
    _showError('Quantity must be a positive integer.');
    return null;
  }

  return {
    'item': item,
    'itemName': itemName,
    'price': price.toString(),
    'qty': qty.toString(),
    'currency': currency.isEmpty ? 'USD' : currency,
  };
}

void _resetForm() {
  (_doc('#input-item') as HTMLInputElement).value = '';
  (_doc('#input-item-name') as HTMLInputElement).value = '';
  (_doc('#input-price') as HTMLInputElement).value = '';
  (_doc('#input-quantity') as HTMLInputElement).value = '';
  (_doc('#input-currency') as HTMLInputElement).value = 'USD';
  (_doc('#edit-id') as HTMLInputElement).value = '-1';

  _setText('#form-panel-title', '➕ Add New Order');
  _setText('#btn-add-label', 'Add to Order List');
  _setText('#btn-add-icon', '＋');
  (_doc('#btn-add') as HTMLElement).className = 'btn-primary';
  (_doc('#btn-cancel-edit') as HTMLElement).style.display = 'none';
  (_doc('#form-panel') as HTMLElement).className = 'panel';
}

// ─── Search ───────────────────────────────────────────────────────
void _applySearch() {
  final query =
      (_doc('#search-input') as HTMLInputElement).value.trim().toLowerCase();
  final filtered = query.isEmpty
      ? orders
      : orders
          .where((o) => o.itemName.toLowerCase().contains(query))
          .toList();
  _renderTable(filtered);
}

// ─── Helpers ─────────────────────────────────────────────────────
Element _doc(String sel) => document.querySelector(sel)!;

void _showError(String msg) {
  final el = _doc('#form-message') as HTMLElement;
  el.textContent = msg;
  el.className = 'message error';
}

void _showSuccess(String msg) {
  final el = _doc('#form-message') as HTMLElement;
  el.textContent = msg;
  el.className = 'message success';
  Future.delayed(const Duration(seconds: 3), _clearMessage);
}

void _clearMessage() {
  final el = _doc('#form-message') as HTMLElement;
  el.textContent = '';
  el.className = 'message';
}
