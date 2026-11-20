import 'dart:convert';
import 'dart:html';

import 'package:my_order_app/order.dart';

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
late ButtonElement resetDefaultsBtn;
late TableSectionElement ordersTableBody;
late Element statusMessage;
late Element totalQuantityElem;
late Element totalAmountElem;

void main() async {
  orderForm = querySelector('#orderForm') as FormElement;
  itemInput = querySelector('#itemInput') as InputElement;
  itemNameInput = querySelector('#itemNameInput') as InputElement;
  priceInput = querySelector('#priceInput') as InputElement;
  quantityInput = querySelector('#quantityInput') as InputElement;
  currencyInput = querySelector('#currencyInput') as InputElement;
  searchInput = querySelector('#searchInput') as InputElement;
  clearSearchBtn = querySelector('#clearSearchBtn') as ButtonElement;
  downloadJsonBtn = querySelector('#downloadJsonBtn') as ButtonElement;
  resetDefaultsBtn = querySelector('#resetDefaultsBtn') as ButtonElement;
  ordersTableBody = querySelector('#ordersTableBody') as TableSectionElement;
  statusMessage = querySelector('#statusMessage') as Element;
  totalQuantityElem = querySelector('#totalQuantity') as Element;
  totalAmountElem = querySelector('#totalAmount') as Element;

  // Question 2: Read initial string and save to objects
  orders = parseOrdersFromString(initialOrdersJson);
  await loadOrdersFromBackend();
  renderTable();

  // Validate input: block non-numeric characters for price and quantity
  priceInput.onKeyPress.listen((KeyboardEvent e) {
    final String char = String.fromCharCode(e.charCode);
    if (!RegExp(r'[0-9.]').hasMatch(char)) {
      e.preventDefault();
    }
    if (char == '.' && (priceInput.value ?? '').contains('.')) {
      e.preventDefault();
    }
  });

  priceInput.onInput.listen((_) {
    String val = (priceInput.value ?? '').replaceAll(RegExp(r'[^0-9.]'), '');
    final List<String> parts = val.split('.');
    if (parts.length > 2) {
      val = '${parts[0]}.${parts.sublist(1).join()}';
    }
    priceInput.value = val;
  });

  quantityInput.onKeyPress.listen((KeyboardEvent e) {
    final String char = String.fromCharCode(e.charCode);
    if (!RegExp(r'[0-9]').hasMatch(char)) {
      e.preventDefault();
    }
  });

  quantityInput.onInput.listen((_) {
    quantityInput.value = (quantityInput.value ?? '').replaceAll(RegExp(r'[^0-9]'), '');
  });

  // Question 3: Allows inserting an order
  orderForm.onSubmit.listen((Event e) async {
    e.preventDefault();
    await handleInsert();
  });

  // Question 4: Allows search Order by ItemName
  searchInput.onInput.listen((_) => renderTable());

  clearSearchBtn.onClick.listen((_) {
    searchInput.value = '';
    renderTable();
  });

  downloadJsonBtn.onClick.listen((_) => downloadJson());
  resetDefaultsBtn.onClick.listen((_) => resetDefaults());
}

Future<void> loadOrdersFromBackend() async {
  try {
    final String response = await HttpRequest.getString('/api/orders');
    final Map<String, dynamic> data =
        Map<String, dynamic>.from(jsonDecode(response) as Map);
    final List<dynamic> list = data['orders'] as List<dynamic>;
    orders = list
        .map((dynamic item) => Order.fromJson(Map<String, dynamic>.from(item as Map)))
        .toList();
    showToast('Loaded orders from order.json via Dart backend server.');
  } catch (_) {
    orders = parseOrdersFromString(initialOrdersJson);
  }
}

void renderTable() {
  ordersTableBody.children.clear();
  final String query = (searchInput.value ?? '').trim();
  final List<Order> filtered = searchOrdersByItemName(orders, query);

  clearSearchBtn.style.display = query.isNotEmpty ? 'block' : 'none';

  if (filtered.isEmpty) {
    final TableRowElement row = TableRowElement();
    final TableCellElement cell = TableCellElement()
      ..colSpan = 7
      ..classes.add('empty-state-cell')
      ..text = query.isNotEmpty
          ? 'No orders found matching "$query".'
          : 'No orders available.';
    row.children.add(cell);
    ordersTableBody.children.add(row);
  } else {
    for (int i = 0; i < filtered.length; i++) {
      final Order order = filtered[i];
      final int trueIndex = orders.indexOf(order);
      final TableRowElement tr = TableRowElement();

      tr.children.addAll([
        TableCellElement()..text = '${i + 1}',
        TableCellElement()..text = order.item,
        TableCellElement()..text = order.itemName,
        TableCellElement()..text = '${order.quantity}',
        TableCellElement()..text = order.formattedPrice,
        TableCellElement()..text = order.currency,
        _buildActionCell(trueIndex, order.itemName),
      ]);

      ordersTableBody.children.add(tr);
    }
  }

  updateSummary(filtered);
}

TableCellElement _buildActionCell(int index, String name) {
  final TableCellElement td = TableCellElement()..classes.add('col-center');
  final ButtonElement btn = ButtonElement()
    ..type = 'button'
    ..classes.add('btn-delete-row')
    ..title = 'Delete $name'
    ..setInnerHtml(
      '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">'
      '<polyline points="3 6 5 6 21 6"></polyline>'
      '<path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>'
      '</svg>',
      treeSanitizer: NodeTreeSanitizer.trusted,
    );

  btn.onClick.listen((_) async {
    await deleteOrder(index);
  });

  td.children.add(btn);
  return td;
}

Future<void> handleInsert() async {
  final String item = (itemInput.value ?? '').trim();
  final String itemName = (itemNameInput.value ?? '').trim();
  final num? price = num.tryParse((priceInput.value ?? '').trim());
  final int? qty = int.tryParse((quantityInput.value ?? '').trim());
  final String currency = (currencyInput.value ?? '').trim().isEmpty
      ? 'USD'
      : (currencyInput.value ?? '').trim().toUpperCase();

  if (item.isEmpty || itemName.isEmpty || price == null || qty == null || qty < 1) {
    showToast('Please enter valid Item, Item Name, Price, and Quantity.', isError: true);
    return;
  }

  final Order newOrder = Order(
    item: item,
    itemName: itemName,
    price: price,
    currency: currency,
    quantity: qty,
  );

  try {
    final HttpRequest req = await HttpRequest.request(
      '/api/orders',
      method: 'POST',
      sendData: jsonEncode(newOrder.toJson()),
      requestHeaders: {'Content-Type': 'application/json'},
    );
    if (req.status == 201 || req.status == 200) {
      await loadOrdersFromBackend();
    } else {
      orders.add(newOrder);
    }
  } catch (_) {
    orders.add(newOrder);
  }

  orderForm.reset();
  currencyInput.value = 'USD';
  quantityInput.value = '1';
  renderTable();
  showToast('Added "${newOrder.itemName}" (${newOrder.item}) into order.json.');
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
      await loadOrdersFromBackend();
    } else {
      orders.removeAt(index);
    }
  } catch (_) {
    orders.removeAt(index);
  }

  renderTable();
  showToast('Removed "${target.itemName}" from order.json.');
}

void updateSummary(List<Order> list) {
  final int totalQty = list.fold<int>(0, (sum, o) => sum + o.quantity);
  final num totalAmt = list.fold<num>(0, (sum, o) => sum + o.total);

  totalQuantityElem.text = '$totalQty';
  totalAmountElem.text = '${_formatNumber(totalAmt)} USD';
}

String _formatNumber(num val) {
  final bool isWhole = val == val.roundToDouble();
  final String raw = isWhole ? val.toInt().toString() : val.toStringAsFixed(2);
  final List<String> parts = raw.split('.');
  final String intPart = parts[0].replaceAllMapped(
    RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
    (Match m) => '${m[1]}.',
  );
  return parts.length > 1 ? '$intPart,${parts[1]}' : intPart;
}

void downloadJson() {
  final String jsonStr = encodeOrdersToJson(orders);
  final Blob blob = Blob([jsonStr], 'application/json');
  final String url = Url.createObjectUrlFromBlob(blob);
  final AnchorElement anchor = AnchorElement(href: url)
    ..setAttribute('download', 'order.json')
    ..click();
  Url.revokeObjectUrl(url);
  anchor.remove();
}

void resetDefaults() {
  orders = parseOrdersFromString(initialOrdersJson);
  searchInput.value = '';
  renderTable();
  showToast('Reset to initial exam orders.');
}

void showToast(String msg, {bool isError = false}) {
  statusMessage.text = msg;
  statusMessage.classes.toggle('error', isError);
  statusMessage.style.display = 'block';
}
