/// 1. Class Order
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
      item: json['Item'] as String,
      itemName: json['ItemName'] as String,
      price: (json['Price'] as num).toDouble(),
      currency: json['Currency'] as String,
      quantity: (json['Quantity'] as num).toInt(),
    );
  }

  Map<String, dynamic> toJson() => {
        'Item': item,
        'ItemName': itemName,
        // Write 1200.0 as 1200 to match the sample data.
        'Price': price == price.roundToDouble() ? price.toInt() : price,
        'Currency': currency,
        'Quantity': quantity,
      };

  @override
  String toString() =>
      'Order(item: $item, itemName: $itemName, price: $price, '
      'currency: $currency, quantity: $quantity)';
}
