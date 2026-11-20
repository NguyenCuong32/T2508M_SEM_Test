class Order {
  String item;
  String itemName;
  num price;
  String currency;
  int quantity;

  Order({
    required this.item,
    required this.itemName,
    required this.price,
    required this.currency,
    required this.quantity,
  });

  // Factory constructor để parse dữ liệu từ JSON Map
  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      item: json['Item']?.toString() ?? '',
      itemName: json['ItemName']?.toString() ?? '',
      price: json['Price'] is num
          ? json['Price']
          : (num.tryParse(json['Price']?.toString() ?? '') ?? 0),
      currency: json['Currency']?.toString() ?? 'USD',
      quantity: json['Quantity'] is int
          ? json['Quantity']
          : (int.tryParse(json['Quantity']?.toString() ?? '') ?? 1),
    );
  }

  // Chuyển đối tượng Order thành JSON Map
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
