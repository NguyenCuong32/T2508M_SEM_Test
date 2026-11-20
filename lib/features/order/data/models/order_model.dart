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

  double get total => price * quantity;

  factory Order.fromJson(Map<String, dynamic> json) {
    final item = json['Item'];
    final name = json['ItemName'];
    final price = json['Price'];
    final currency = json['Currency'];
    final quantity = json['Quantity'];
    if (item is! String ||
        item.trim().isEmpty ||
        name is! String ||
        name.trim().isEmpty ||
        price is! num ||
        !price.isFinite ||
        price <= 0 ||
        currency is! String ||
        !RegExp(r'^[A-Z]{3}$').hasMatch(currency) ||
        quantity is! int ||
        quantity <= 0) {
      throw const FormatException('Invalid order data');
    }
    return Order(
      item: item.trim(),
      itemName: name.trim(),
      price: price.toDouble(),
      currency: currency,
      quantity: quantity,
    );
  }

  Map<String, dynamic> toJson() => {
    'Item': item,
    'ItemName': itemName,
    'Price': price,
    'Currency': currency,
    'Quantity': quantity,
  };

  static List<Order> search(List<Order> orders, String query) {
    final keyword = query.trim().toLowerCase();
    return orders
        .where((order) => order.itemName.toLowerCase().contains(keyword))
        .toList();
  }
}
