import 'package:flutter_test/flutter_test.dart';
import '../lib/models/order.dart';

void main() {
  group('Order Model Tests', () {
    test('Test convert JSON to Order,'(){
      final json = {
        'Item': 'A1000',
        'ItemName': 'Iphone15',
        'Price': 1200,
        'Currency': 'USD',
        'Quantity': 1
      }

      final order = Order.fromJson(json);
      expect(order.Item, 'A1000');
      expect(order.ItemName, 'Iphone15');
      expect(order.Price, 1200);
      expect(order.Currency, 'USD');
      expect(order.Quantity, 1);
    })

    test('Test convert Order to JSON',(){
      final order = Order(
        'Item': 'A1001',
        'ItemName': 'Iphone16',
        'Price': 1500,
        'Currency': 'USD',
        'Quantity': 1
      )
      final json = order.toJson();
      expect(json['Item'], 'A1001');
      expect(json['ItemName'], 'Iphone16');
      expect(json['Price'], 1500);
      expect(json['Currency'], 'USD');
      expect(json['Quantity'], 1);
    });
  });
}