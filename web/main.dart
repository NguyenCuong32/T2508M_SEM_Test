import 'dart:convert';
import 'dart:html';

import 'package:my_order_app/order.dart';

/// Client-side list of Order objects (5 Marks)
List<Order> orders = <Order>[];

late FormElement orderForm;
late InputElement itemInput;
late InputElement itemNameInput;
late InputElement priceInput;
late InputElement quantityInput;
late InputElement currencyInput;
late InputElement searchInput;
late ButtonElement clearSearchBtn;
late ButtonElement downloadJsonBtn;
late TableSectionElement ordersTableBody;
late DivElement statusMessage;

Future<void> main() async {
  orderForm = querySelector('#orderForm') as FormElement;
  itemInput = querySelector('#itemInput') as InputElement;
  itemNameInput = querySelector('#itemNameInput') as InputElement;
  priceInput = querySelector('#priceInput') as InputElement;
  quantityInput = querySelector('#quantityInput') as InputElement;
  currencyInput = querySelector('#currencyInput') as InputElement;
  searchInput = querySelector('#searchInput') as InputElement;
  clearSearchBtn = querySelector('#clearSearchBtn') as ButtonElement;
  downloadJsonBtn = querySelector('#downloadJsonBtn') as ButtonElement;
  ordersTableBody = querySelector('#ordersTableBody') as TableSectionElement;
  statusMessage = querySelector('#statusMessage') as DivElement;

  // Read string and save to objects, display list of orders to screen
  orders = parseOrdersFromString(initialOrdersJson);
  await loadOrdersFromServer();
  renderOrdersTable();

  // Handle Insert Order into order.json
  orderForm.onSubmit.listen((Event event) async {
    event.preventDefault();
    await insertOrder();
  });

  // Handle Search Order by ItemName
  searchInput.onInput.listen((_) {
    renderOrdersTable();
  });

  clearSearchBtn.onClick.listen((_) {
    searchInput.value = '';
    renderOrdersTable();
  });

  downloadJsonBtn.onClick.listen((_) {
    downloadOrderJson();
  });
}

Future<void> loadOrdersFromServer() async {
  try {
    final String responseText = await HttpRequest.getString('/api/orders');
    final Map<String, dynamic> decoded =
        Map<String, dynamic>.from(jsonDecode(responseText) as Map);
    final List<dynamic> list = decoded['orders'] as List<dynamic>;
    orders = list
        .map((dynamic item) => Order.fromJson(Map<String, dynamic>.from(item as Map)))
        .toList();
  } catch (_) {
    // Fallback to initialOrdersJson if running without backend API
    orders = parseOrdersFromString(initialOrdersJson);
  }
}

void renderOrdersTable() {
  ordersTableBody.children.clear();
  final String query = searchInput.value ?? '';
  final List<Order> filtered = searchOrdersByItemName(orders, query);

  if (filtered.isEmpty) {
    final TableRowElement emptyRow = TableRowElement()..classes.add('empty-row');
    final TableCellElement cell = TableCellElement()
      ..colSpan = 7
      ..text = 'No orders found matching "$query".';
    emptyRow.children.add(cell);
    ordersTableBody.children.add(emptyRow);
    return;
  }

  for (int i = 0; i < filtered.length; i++) {
    final Order order = filtered[i];
    final int originalIndex = orders.indexOf(order);
    final TableRowElement tr = TableRowElement();

    tr.children.addAll([
      TableCellElement()..text = '${i + 1}',
      TableCellElement()..text = order.item,
      TableCellElement()..text = order.itemName,
      TableCellElement()..text = '${order.quantity}',
      TableCellElement()..text = order.formattedPrice,
      TableCellElement()..text = order.currency,
      _buildActionCell(originalIndex),
    ]);

    ordersTableBody.children.add(tr);
  }
}

TableCellElement _buildActionCell(int index) {
  final TableCellElement td = TableCellElement()..classes.add('action-cell');
  final ButtonElement deleteBtn = ButtonElement()
    ..type = 'button'
    ..classes.add('btn-delete')
    ..title = 'Delete Order'
    ..setInnerHtml(
      '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">'
      '<polyline points="3 6 5 6 21 6"></polyline>'
      '<path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>'
      '</svg>',
      treeSanitizer: NodeTreeSanitizer.trusted,
    );

  deleteBtn.onClick.listen((_) async {
    await deleteOrder(index);
  });

  td.children.add(deleteBtn);
  return td;
}

Future<void> insertOrder() async {
  final String item = (itemInput.value ?? '').trim();
  final String itemName = (itemNameInput.value ?? '').trim();
  final num? price = num.tryParse((priceInput.value ?? '').trim());
  final int? quantity = int.tryParse((quantityInput.value ?? '').trim());
  final String currency = (currencyInput.value ?? '').trim().isEmpty
      ? 'USD'
      : (currencyInput.value ?? '').trim().toUpperCase();

  if (item.isEmpty || itemName.isEmpty || price == null || quantity == null || quantity <= 0) {
    showStatus('Please fill in valid Item, Item Name, Price, and Quantity.', isError: true);
    return;
  }

  final Order newOrder = Order(
    item: item,
    itemName: itemName,
    price: price,
    currency: currency,
    quantity: quantity,
  );

  try {
    final HttpRequest req = await HttpRequest.request(
      '/api/orders',
      method: 'POST',
      sendData: jsonEncode(newOrder.toJson()),
      requestHeaders: {'Content-Type': 'application/json'},
    );
    if (req.status == 201 || req.status == 200) {
      await loadOrdersFromServer();
    } else {
      orders.add(newOrder);
    }
  } catch (_) {
    orders.add(newOrder);
  }

  orderForm.reset();
  currencyInput.value = 'USD';
  renderOrdersTable();
  showStatus('Added "${newOrder.itemName}" (${newOrder.item}) to order.json.');
}

Future<void> deleteOrder(int index) async {
  if (index < 0 || index >= orders.length) return;
  final Order target = orders[index];

  try {
    final HttpRequest req = await HttpRequest.request(
      '/api/orders/$index',
      method: 'DELETE',
    );
    if (req.status == 200) {
      await loadOrdersFromServer();
    } else {
      orders.removeAt(index);
    }
  } catch (_) {
    orders.removeAt(index);
  }

  renderOrdersTable();
  showStatus('Removed "${target.itemName}" (${target.item}) from order.json.');
}

void downloadOrderJson() {
  final String jsonContent = encodeOrdersToJson(orders);
  final Blob blob = Blob([jsonContent], 'application/json');
  final String url = Url.createObjectUrlFromBlob(blob);
  final AnchorElement anchor = AnchorElement(href: url)
    ..setAttribute('download', 'order.json')
    ..click();
  Url.revokeObjectUrl(url);
  anchor.remove();
}

void showStatus(String message, {bool isError = false}) {
  statusMessage.text = message;
  statusMessage.classes.toggle('error', isError);
}
