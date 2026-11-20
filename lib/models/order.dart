class Order {
  final String item;
  final String itemName;
  final double price;
  final String currency;
  final int quantity;

  Order({
    required this.item,
    required this.itemName,
    required this.price,
    required this.currency,
    required this.quantity,
  });

  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      item: json['Item']?.toString() ?? '',
      itemName: json['ItemName']?.toString() ?? '',
      price: (json['Price'] is num)
          ? (json['Price'] as num).toDouble()
          : double.tryParse(json['Price']?.toString() ?? '0') ?? 0.0,
      currency: json['Currency']?.toString() ?? 'USD',
      quantity: (json['Quantity'] is num)
          ? (json['Quantity'] as num).toInt()
          : int.tryParse(json['Quantity']?.toString() ?? '1') ?? 1,
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

  Order copyWith({
    String? item,
    String? itemName,
    double? price,
    String? currency,
    int? quantity,
  }) {
    return Order(
      item: item ?? this.item,
      itemName: itemName ?? this.itemName,
      price: price ?? this.price,
      currency: currency ?? this.currency,
      quantity: quantity ?? this.quantity,
    );
  }
}

