import 'dart:convert';
import 'dart:html';

class Order {
  String item;
  String itemName;
  double price;
  String currency;
  int quantity;

  Order({
    required this.item,
    required this.itemName,
    required this.price,
    required this.currency,
    required this.quantity,
  });

  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      item: json['Item'] ?? '',
      itemName: json['ItemName'] ?? '',
      price: (json['Price'] as num).toDouble(),
      currency: json['Currency'] ?? 'USD',
      quantity: (json['Quantity'] as num).toInt(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'Item': item,
      'ItemName': itemName,
      'Price': price,
      'Currency': currency,
      'Quantity': quantity,
    };
  }
}

const String initialJsonString =
    '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": 1500,"Currency": "USD","Quantity":1}]';

List<Order> orders = [];

void main() {
  loadOrders();
  renderTable(orders);

  querySelector('#addBtn')?.onClick.listen((_) => insertOrder());
  querySelector('#searchInput')?.onInput.listen((_) => searchOrders());
}

void loadOrders() {
  final savedData = window.localStorage['order.json'];
  final jsonSource = (savedData != null && savedData.isNotEmpty)
      ? savedData
      : initialJsonString;

  final List<dynamic> decoded = jsonDecode(jsonSource);
  orders = decoded.map((e) => Order.fromJson(e)).toList();
  saveToOrderJson();
}

void saveToOrderJson() {
  final jsonString = jsonEncode(orders.map((o) => o.toJson()).toList());
  window.localStorage['order.json'] = jsonString;
}

void insertOrder() {
  final itemInput = querySelector('#item') as InputElement;
  final itemNameInput = querySelector('#itemName') as InputElement;
  final priceInput = querySelector('#price') as InputElement;
  final quantityInput = querySelector('#quantity') as InputElement;
  final currencyInput = querySelector('#currency') as InputElement;

  final item = itemInput.value?.trim() ?? '';
  final itemName = itemNameInput.value?.trim() ?? '';
  final price = double.tryParse(priceInput.value?.trim() ?? '');
  final quantity = int.tryParse(quantityInput.value?.trim() ?? '');
  final currency = (currencyInput.value?.trim().isEmpty ?? true)
      ? 'USD'
      : currencyInput.value!.trim();

  if (item.isEmpty || itemName.isEmpty || price == null || quantity == null) {
    window.alert('Vui lòng nhập đầy đủ và đúng định dạng các trường!');
    return;
  }

  orders.add(Order(
    item: item,
    itemName: itemName,
    price: price,
    currency: currency,
    quantity: quantity,
  ));

  saveToOrderJson();

  itemInput.value = '';
  itemNameInput.value = '';
  priceInput.value = '';
  quantityInput.value = '';
  currencyInput.value = 'USD';

  searchOrders();
}

void deleteOrder(int index) {
  orders.removeAt(index);
  saveToOrderJson();
  searchOrders();
}

void searchOrders() {
  final searchInput = querySelector('#searchInput') as InputElement;
  final keyword = searchInput.value?.trim().toLowerCase() ?? '';

  if (keyword.isEmpty) {
    renderTable(orders);
  } else {
    final filtered = orders
        .where((o) => o.itemName.toLowerCase().contains(keyword))
        .toList();
    renderTable(filtered);
  }
}

void renderTable(List<Order> list) {
  final tbody = querySelector('#orderTableBody') as TableSectionElement;
  tbody.children.clear();

  for (int i = 0; i < list.length; i++) {
    final order = list[i];
    final row = TableRowElement();

    row.children.addAll([
      TableCellElement()..text = '${i + 1}',
      TableCellElement()..text = order.item,
      TableCellElement()..text = order.itemName,
      TableCellElement()..text = '${order.quantity}',
      TableCellElement()..text = order.price.toStringAsFixed(0),
      TableCellElement()..text = order.currency,
    ]);

    final actionCell = TableCellElement();
    final deleteBtn = ButtonElement()
      ..className = 'btn-delete'
      ..text = '🗑'
      ..onClick.listen((_) {
        final realIndex = orders.indexOf(order);
        if (realIndex != -1) {
          deleteOrder(realIndex);
        }
      });

    actionCell.children.add(deleteBtn);
    row.children.add(actionCell);
    tbody.children.add(row);
  }
}