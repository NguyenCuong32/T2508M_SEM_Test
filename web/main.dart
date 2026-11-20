import 'dart:async';
import 'dart:convert';
// ignore: deprecated_member_use
import 'dart:html' as html;

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

  double get total => price * quantity;
}

List<Order> orders = [];
const pageSize = 5;
int currentPage = 1;
int requestVersion = 0;
String currentSearch = '';
String? editingId;
Timer? searchTimer;
Timer? toastTimer;

void main() {
  html.querySelector('#display-date')?.text = _formatDate(DateTime.now());
  html.querySelector('#open-modal')?.onClick.listen((_) => openModal());
  html.querySelector('#close-modal')?.onClick.listen((_) => closeModal());
  html.querySelector('#cancel-modal')?.onClick.listen((_) => closeModal());
  html.querySelector('#modal')?.onClick.listen((event) {
    if (event.target == html.querySelector('#modal')) closeModal();
  });
  html.querySelector('#search-input')?.onInput.listen((_) {
    searchTimer?.cancel();
    currentPage = 1;
    requestVersion++;
    searchTimer = Timer(const Duration(milliseconds: 180), loadOrders);
  });
  html.querySelector('#clear-search')?.onClick.listen((_) {
    searchTimer?.cancel();
    currentPage = 1;
    (html.querySelector('#search-input') as html.InputElement).value = '';
    loadOrders();
  });
  html.querySelector('#previous-page')?.onClick.listen((_) {
    if (currentPage > 1) {
      currentPage--;
      renderOrders();
    }
  });
  html.querySelector('#next-page')?.onClick.listen((_) {
    if (currentPage * pageSize < orders.length) {
      currentPage++;
      renderOrders();
    }
  });
  html.querySelector('#order-form')?.onSubmit.listen((event) async {
    event.preventDefault();
    await saveOrder();
  });
  html.querySelector('#export-button')?.onClick.listen((_) => exportOrders());
  html.window.onKeyDown.listen((event) {
    if ((event.metaKey || event.ctrlKey) &&
        (event.key ?? '').toLowerCase() == 'k') {
      event.preventDefault();
      (html.querySelector('#search-input') as html.InputElement?)?.focus();
    }
    if (event.key == 'Escape') closeModal();
  });
  loadOrders();
}

Future<void> loadOrders() async {
  final version = ++requestVersion;
  final input = html.querySelector('#search-input') as html.InputElement?;
  currentSearch = input?.value?.trim() ?? '';
  final query = Uri(
    queryParameters: currentSearch.isEmpty ? {} : {'search': currentSearch},
  ).query;
  try {
    final result = await html.HttpRequest.request(
      '/api/orders${query.isEmpty ? '' : '?$query'}',
    );
    if (version != requestVersion) return;
    final data = jsonDecode(result.responseText ?? '[]') as List<dynamic>;
    orders = data
        .map((entry) => Order.fromJson(entry as Map<String, dynamic>))
        .toList();
    renderOrders();
  } catch (_) {
    if (version != requestVersion) return;
    showToast('Could not load orders. Refresh to try again.');
  }
}

void renderOrders() {
  final body = html.querySelector('#orders-body');
  if (body == null) return;
  body.children.clear();
  final count = orders.length;
  final pageCount = count == 0 ? 1 : (count + pageSize - 1) ~/ pageSize;
  if (currentPage > pageCount) currentPage = pageCount;
  final start = (currentPage - 1) * pageSize;
  final end = (start + pageSize).clamp(0, count);
  for (final order in orders.sublist(start, end)) {
    final row = html.TableRowElement();
    final icon = order.itemName.toLowerCase().contains('iphone')
        ? '📱'
        : order.itemName.toLowerCase().contains('macbook')
            ? '💻'
            : '📦';
    final tone = icon == '📱'
        ? 'phone'
        : icon == '💻'
            ? 'laptop'
            : '';
    row.innerHtml = '''
      <td><span class="order-id">${_escape(order.item)}</span></td>
      <td><div class="product-cell"><span class="product-thumb $tone">$icon</span>${_escape(order.itemName)}</div></td>
      <td><span class="unit-price">${_money(order.price, order.currency)}</span></td>
      <td><span class="quantity-badge">${order.quantity}</span></td>
      <td><span class="total-price">${_money(order.total, order.currency)}</span></td>
      <td><span class="currency-badge">${_escape(order.currency)}</span></td>
      <td><div class="row-actions"><button class="edit-order">Edit</button><button class="delete-order">Delete</button></div></td>''';
    row.querySelector('.edit-order')?.onClick.listen((_) => openModal(order));
    row
        .querySelector('.delete-order')
        ?.onClick
        .listen((_) => deleteOrder(order));
    body.append(row);
  }
  html.querySelector('#empty-state')?.hidden = count != 0;
  html.querySelector('#total-orders')?.text = '$count';
  html.querySelector('#total-items')?.text =
      '${orders.fold<int>(0, (sum, order) => sum + order.quantity)}';
  final value = orders.fold<double>(0, (sum, order) => sum + order.total);
  final currencies = orders.map((order) => order.currency).toSet();
  final valueLabel = currencies.length <= 1
      ? _money(value, currencies.isEmpty ? 'USD' : currencies.first)
      : '${currencies.length} currencies';
  html.querySelector('#order-value')?.text = valueLabel;
  html.querySelector('#result-count')?.text =
      '$count ${count == 1 ? 'order' : 'orders'}';
  html.querySelector('#footer-count')?.text = count == 0
      ? 'Showing 0 orders'
      : 'Showing ${start + 1}–$end of $count ${count == 1 ? 'order' : 'orders'}';
  (html.querySelector('#previous-page') as html.ButtonElement).disabled =
      currentPage == 1;
  (html.querySelector('#next-page') as html.ButtonElement).disabled =
      currentPage == pageCount;
  html.querySelector('#current-page')?.text = '$currentPage / $pageCount';
  html.querySelector('#nav-count')?.text = count.toString().padLeft(2, '0');
}

Future<void> saveOrder() async {
  final form = html.querySelector('#order-form') as html.FormElement;
  final values = <String, String>{};
  for (final element in form.querySelectorAll('[name]')) {
    final name = element.attributes['name'];
    if (name == null) continue;
    if (element is html.InputElement) values[name] = element.value ?? '';
    if (element is html.SelectElement) values[name] = element.value ?? '';
  }
  final payload = {
    'Item': values['item']?.trim().toUpperCase() ?? '',
    'ItemName': values['itemName']?.trim() ?? '',
    'Price': double.tryParse(values['price'] ?? '') ?? 0,
    'Currency': values['currency'] ?? 'USD',
    'Quantity': int.tryParse(values['quantity'] ?? '') ?? 0,
  };
  final error = html.querySelector('#form-error') as html.DivElement;
  try {
    final isEditing = editingId != null;
    final url = isEditing
        ? '/api/orders?${Uri(queryParameters: {'id': editingId!}).query}'
        : '/api/orders';
    final result = await html.HttpRequest.request(
      url,
      method: isEditing ? 'PUT' : 'POST',
      sendData: jsonEncode(payload),
      requestHeaders: {'Content-Type': 'application/json'},
    );
    if (result.status == 201 || result.status == 200) {
      closeModal();
      form.reset();
      (form.querySelector('[name="quantity"]') as html.InputElement).value =
          '1';
      (html.querySelector('#search-input') as html.InputElement).value = '';
      currentPage = 1;
      showToast(
          'Order ${payload['Item']} ${isEditing ? 'updated' : 'added'} successfully.');
      await loadOrders();
      return;
    }
    final message = (jsonDecode(result.responseText ?? '{}')
            as Map<String, dynamic>)['error'] ??
        'Unable to save order.';
    error.text = '$message';
    error.hidden = false;
  } catch (event) {
    final request = event is html.ProgressEvent ? event.target : null;
    if (request is html.HttpRequest && request.responseText != null) {
      try {
        error.text =
            (jsonDecode(request.responseText!) as Map<String, dynamic>)['error']
                    ?.toString() ??
                'Unable to save order.';
      } catch (_) {
        error.text = 'Unable to save order.';
      }
    } else {
      error.text =
          'Unable to save this order. Check your connection and try again.';
    }
    error.hidden = false;
  }
}

Future<void> deleteOrder(Order order) async {
  if (!html.window.confirm('Delete order ${order.item} (${order.itemName})?'))
    return;
  try {
    await html.HttpRequest.request(
        '/api/orders?${Uri(queryParameters: {'id': order.item}).query}',
        method: 'DELETE');
    showToast('Order ${order.item} deleted.');
    await loadOrders();
  } catch (_) {
    showToast('Unable to delete order. Please try again.');
  }
}

void openModal([Order? order]) {
  editingId = order?.item;
  final form = html.querySelector('#order-form') as html.FormElement;
  form.reset();
  (form.querySelector('[name="item"]') as html.InputElement).value =
      order?.item ?? '';
  (form.querySelector('[name="itemName"]') as html.InputElement).value =
      order?.itemName ?? '';
  (form.querySelector('[name="price"]') as html.InputElement).value =
      order?.price.toString() ?? '';
  (form.querySelector('[name="quantity"]') as html.InputElement).value =
      order?.quantity.toString() ?? '1';
  (form.querySelector('[name="currency"]') as html.SelectElement).value =
      order?.currency ?? 'USD';
  html.querySelector('#modal-title')?.text =
      order == null ? 'Add an order' : 'Edit order';
  html.querySelector('.modal-kicker')?.text =
      order == null ? 'NEW ENTRY' : 'UPDATE ORDER';
  (html.querySelector('#form-error') as html.Element).hidden = true;
  (html.querySelector('#modal') as html.Element).hidden = false;
  (html.querySelector('#order-form [name="item"]') as html.InputElement?)
      ?.focus();
}

void closeModal() {
  final modal = html.querySelector('#modal');
  if (modal != null) modal.hidden = true;
  final error = html.querySelector('#form-error');
  if (error != null) error.hidden = true;
}

void showToast(String message) {
  final toast = html.querySelector('#toast');
  if (toast == null) return;
  toast.text = message;
  toast.classes.add('visible');
  toastTimer?.cancel();
  toastTimer = Timer(
    const Duration(seconds: 3),
    () => toast.classes.remove('visible'),
  );
}

void exportOrders() {
  final content = const JsonEncoder.withIndent('  ')
      .convert(orders.map((order) => order.toJson()).toList());
  final blob = html.Blob([content], 'application/json');
  final url = html.Url.createObjectUrlFromBlob(blob);
  html.AnchorElement(href: url)
    ..download = 'order-export.json'
    ..click();
  html.Url.revokeObjectUrl(url);
  showToast('Order data exported.');
}

String _money(double amount, String currency) {
  final decimals = amount == amount.roundToDouble() ? 0 : 2;
  final fixed = amount.toStringAsFixed(decimals);
  final parts = fixed.split('.');
  final grouped = parts.first.replaceAllMapped(
    RegExp(r'\B(?=(\d{3})+(?!\d))'),
    (_) => ',',
  );
  return '$currency $grouped${parts.length > 1 ? '.${parts[1]}' : ''}';
}

String _escape(String value) => value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');

String _formatDate(DateTime date) {
  const weekdays = [
    'MONDAY',
    'TUESDAY',
    'WEDNESDAY',
    'THURSDAY',
    'FRIDAY',
    'SATURDAY',
    'SUNDAY'
  ];
  const months = [
    'JANUARY',
    'FEBRUARY',
    'MARCH',
    'APRIL',
    'MAY',
    'JUNE',
    'JULY',
    'AUGUST',
    'SEPTEMBER',
    'OCTOBER',
    'NOVEMBER',
    'DECEMBER'
  ];
  return '${weekdays[date.weekday - 1]}, ${months[date.month - 1]} ${date.day}, ${date.year}';
}
