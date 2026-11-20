/// Model class representing a single Order.
/// Conforms to the JSON specification provided in SET01 Exam Paper:
///   - Item: Product code / SKU (e.g. "A1000")
///   - ItemName: Name of the product (e.g. "Iphone 15")
///   - Price: Unit price (e.g. 1200)
///   - Currency: Currency code (e.g. "USD")
///   - Quantity: Number of items ordered (e.g. 1)
class Order {
  final String item;
  final String itemName;
  final double price;
  final String currency;
  final int quantity;

  /// Primary constructor
  const Order({
    required this.item,
    required this.itemName,
    required this.price,
    required this.currency,
    required this.quantity,
  });

  /// Factory constructor to deserialize an [Order] from a JSON map.
  /// Handles both numeric and string representations safely.
  factory Order.fromJson(Map<String, dynamic> json) {
    // Parse item (SKU / Code)
    final rawItem = json['Item'] ?? json['item'] ?? '';

    // Parse itemName
    final rawItemName = json['ItemName'] ?? json['itemName'] ?? '';

    // Parse price safely
    final rawPrice = json['Price'] ?? json['price'];
    double parsedPrice = 0.0;
    if (rawPrice is num) {
      parsedPrice = rawPrice.toDouble();
    } else if (rawPrice is String) {
      parsedPrice = double.tryParse(rawPrice.replaceAll(',', '')) ?? 0.0;
    }

    // Parse currency
    final rawCurrency = json['Currency'] ?? json['currency'] ?? 'USD';

    // Parse quantity safely
    final rawQuantity = json['Quantity'] ?? json['quantity'];
    int parsedQuantity = 1;
    if (rawQuantity is int) {
      parsedQuantity = rawQuantity;
    } else if (rawQuantity is num) {
      parsedQuantity = rawQuantity.toInt();
    } else if (rawQuantity is String) {
      parsedQuantity = int.tryParse(rawQuantity) ?? 1;
    }

    return Order(
      item: rawItem.toString().trim(),
      itemName: rawItemName.toString().trim(),
      price: parsedPrice,
      currency: rawCurrency.toString().trim(),
      quantity: parsedQuantity,
    );
  }

  /// Capitalized getters matching the exact JSON key names
  String get Item => item;
  String get ItemName => itemName;
  double get Price => price;
  String get Currency => currency;
  int get Quantity => quantity;

  /// Calculates total price = price * quantity
  double get totalPrice => price * quantity;

  /// Formats the price matching the exam display format (e.g. 1.200)
  String get formattedPrice {
    final isWhole = price % 1 == 0;
    final valueStr = isWhole ? price.toInt().toString() : price.toStringAsFixed(2);
    // Add dot separator for thousands (e.g., 1200 -> 1.200 as seen in exam screenshot)
    return valueStr.replaceAllMapped(
      RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
      (Match m) => '${m[1]}.',
    );
  }

  /// Formats the total price with thousand separator
  String get formattedTotalPrice {
    final isWhole = totalPrice % 1 == 0;
    final valueStr = isWhole ? totalPrice.toInt().toString() : totalPrice.toStringAsFixed(2);
    return valueStr.replaceAllMapped(
      RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
      (Match m) => '${m[1]}.',
    );
  }

  /// Serializes this [Order] back to a Map matching the required JSON format
  Map<String, dynamic> toJson() {
    return {
      'Item': item,
      'ItemName': itemName,
      'Price': price % 1 == 0 ? price.toInt() : price,
      'Currency': currency,
      'Quantity': quantity,
    };
  }

  /// Creates a copy of this order with optional updated fields
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

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is Order &&
          runtimeType == other.runtimeType &&
          item == other.item &&
          itemName == other.itemName &&
          price == other.price &&
          currency == other.currency &&
          quantity == other.quantity;

  @override
  int get hashCode =>
      item.hashCode ^
      itemName.hashCode ^
      price.hashCode ^
      currency.hashCode ^
      quantity.hashCode;

  @override
  String toString() {
    return 'Order(Item: "$item", ItemName: "$itemName", Price: $price, Currency: "$currency", Quantity: $quantity)';
  }
}
