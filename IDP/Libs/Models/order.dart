class Order {
  // 1. Khai bao cac thuoc tinh cua Order
  final String Item;
  final String ItemName;
  final double Price;
  final int Quantity;
  final String Currency;

  // 2. Constructor
  Order({
    required this.Item,
    required this.ItemName,
    required this.Price,
    required this.Quantity,
    required this.Currency,
  });

  // 3. Method to convert Order object to JSON
  factory Order.fromJson(Map<String, dynamic> json) {
    return Order(
      Item: json['Item'] as String,
      ItemName: json['ItemName'] as String,
      Price: json['Price'] as double,
      Quantity: json['Quantity'] as int,
      Currency: json['Currency'] as String,
    );
  }

  // 4. Method to convert Order object to JSON
  Map<String, dynamic> toJson() {
    return {
      'Item': Item,
      'ItemName': ItemName,
      'Price': Price,
      'Quantity': Quantity,
      'Currency': Currency,
    };
  }

  // 5. Hien thi thong tin cua Order
  @override
  String toString() {
    return 'Order(Item: $Item, ItemName: $ItemName, Price: $Price, Quantity: $Quantity, Currency: $Currency)';
  }
}