import 'dart:convert';
import 'package:test/test.dart';
import '../lib/order.dart';
import '../lib/order_manager.dart';

void main() {
  group('Requirement 1: Create class Order', () {
    test('Construct Order object with valid values', () {
      final order = Order(
        item: 'A1000',
        itemName: 'Iphone 15',
        price: 1200,
        currency: 'USD',
        quantity: 1,
      );

      expect(order.item, equals('A1000'));
      expect(order.itemName, equals('Iphone 15'));
      expect(order.price, equals(1200.0));
      expect(order.currency, equals('USD'));
      expect(order.quantity, equals(1));
      // Capitalized getters
      expect(order.Item, equals('A1000'));
      expect(order.ItemName, equals('Iphone 15'));
      expect(order.Price, equals(1200.0));
      expect(order.Currency, equals('USD'));
      expect(order.Quantity, equals(1));
    });

    test('Order.fromJson and Order.toJson', () {
      final jsonMap = {
        'Item': 'A1001',
        'ItemName': 'Iphone 16',
        'Price': 1500,
        'Currency': 'USD',
        'Quantity': 2,
      };

      final order = Order.fromJson(jsonMap);
      expect(order.item, equals('A1001'));
      expect(order.itemName, equals('Iphone 16'));
      expect(order.price, equals(1500.0));
      expect(order.currency, equals('USD'));
      expect(order.quantity, equals(2));

      final serialized = order.toJson();
      expect(serialized['Item'], equals('A1001'));
      expect(serialized['ItemName'], equals('Iphone 16'));
      expect(serialized['Price'], equals(1500));
      expect(serialized['Currency'], equals('USD'));
      expect(serialized['Quantity'], equals(2));
    });

    test('Order formatted price matches exam picture', () {
      final order = Order(
        item: 'A1000',
        itemName: 'Iphone 15',
        price: 1200,
        currency: 'USD',
        quantity: 1,
      );
      expect(order.formattedPrice, equals('1.200'));
    });
  });

  group('Requirement 2: Read string and save objects to list orders', () {
    const rawExamString =
        '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": '
        '"USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": '
        '1500,"Currency": "USD","Quantity":1}]';

    test('Parses the exact exam JSON string correctly into List<Order>', () {
      final manager = OrderManager();
      manager.loadFromJsonString(rawExamString);

      expect(manager.count, equals(2));
      expect(manager.orders[0].item, equals('A1000'));
      expect(manager.orders[0].itemName, equals('Iphone 15'));
      expect(manager.orders[0].price, equals(1200.0));
      expect(manager.orders[1].item, equals('A1001'));
      expect(manager.orders[1].itemName, equals('Iphone 16'));
      expect(manager.orders[1].price, equals(1500.0));
    });
  });

  group('Requirement 3 & 4: Insert order and persist/serialize', () {
    test('Allows inserting an order', () {
      final manager = OrderManager();
      manager.addOrder(
        Order(
          item: 'A1002',
          itemName: 'Iphone 15 Pro Max',
          price: 1800,
          currency: 'USD',
          quantity: 2,
        ),
      );

      expect(manager.count, equals(1));
      expect(manager.orders.first.item, equals('A1002'));

      // Serialize back to JSON string
      final jsonString = manager.toJsonString();
      final decoded = jsonDecode(jsonString) as List;
      expect(decoded.length, equals(1));
      expect(decoded.first['Item'], equals('A1002'));
    });

    test('Allows updating/editing an existing order', () {
      final manager = OrderManager();
      manager.addOrder(
        Order(item: 'A1000', itemName: 'Iphone 15', price: 1200, currency: 'USD', quantity: 1),
      );

      final updated = manager.updateOrder(
        Order(item: 'A1000', itemName: 'Iphone 15 Pro Max', price: 1600, currency: 'USD', quantity: 2),
      );

      expect(updated, isTrue);
      expect(manager.count, equals(1));
      expect(manager.orders.first.itemName, equals('Iphone 15 Pro Max'));
      expect(manager.orders.first.price, equals(1600.0));
      expect(manager.orders.first.quantity, equals(2));
    });

    test('Updating non-existent order returns false', () {
      final manager = OrderManager();
      final updated = manager.updateOrder(
        Order(item: 'NONEXISTENT', itemName: 'Unknown', price: 100, currency: 'USD', quantity: 1),
      );
      expect(updated, isFalse);
    });

    test('Allows deleting an order', () {
      final manager = OrderManager();
      manager.addOrder(
        Order(item: 'A1000', itemName: 'Iphone 15', price: 1200, currency: 'USD', quantity: 1),
      );
      manager.addOrder(
        Order(item: 'A1001', itemName: 'Iphone 16', price: 1500, currency: 'USD', quantity: 1),
      );

      final deleted = manager.deleteOrder('A1000');
      expect(deleted, isTrue);
      expect(manager.count, equals(1));
      expect(manager.orders.first.item, equals('A1001'));
    });
  });

  group('Requirement 5: Allows search Order by ItemName', () {
    late OrderManager manager;

    setUp(() {
      manager = OrderManager();
      manager.addOrder(
        Order(item: 'A1000', itemName: 'Iphone 15', price: 1200, currency: 'USD', quantity: 1),
      );
      manager.addOrder(
        Order(item: 'A1001', itemName: 'Iphone 16', price: 1500, currency: 'USD', quantity: 1),
      );
      manager.addOrder(
        Order(item: 'A1002', itemName: 'Samsung Galaxy S24', price: 1100, currency: 'USD', quantity: 2),
      );
    });

    test('Search returns all matching items case-insensitively', () {
      final results = manager.searchByItemName('iphone');
      expect(results.length, equals(2));
      expect(results.map((o) => o.item), containsAll(['A1000', 'A1001']));
    });

    test('Search specific item name', () {
      final results = manager.searchByItemName('16');
      expect(results.length, equals(1));
      expect(results.first.itemName, equals('Iphone 16'));
    });

    test('Search non-existing item returns empty list', () {
      final results = manager.searchByItemName('Xiaomi');
      expect(results, isEmpty);
    });

    test('Empty search query returns all orders', () {
      final results = manager.searchByItemName('');
      expect(results.length, equals(3));
    });
  });

  group('Input Validation Tests', () {
    test('addOrder throws ArgumentError if item is empty', () {
      final manager = OrderManager();
      expect(
        () => manager.addOrder(
          Order(item: '', itemName: 'Iphone 15', price: 1200, currency: 'USD', quantity: 1),
        ),
        throwsArgumentError,
      );
    });

    test('addOrder throws ArgumentError if itemName is empty', () {
      final manager = OrderManager();
      expect(
        () => manager.addOrder(
          Order(item: 'A1000', itemName: '  ', price: 1200, currency: 'USD', quantity: 1),
        ),
        throwsArgumentError,
      );
    });

    test('addOrder throws ArgumentError if price is negative', () {
      final manager = OrderManager();
      expect(
        () => manager.addOrder(
          Order(item: 'A1000', itemName: 'Iphone 15', price: -50, currency: 'USD', quantity: 1),
        ),
        throwsArgumentError,
      );
    });

    test('addOrder throws ArgumentError if quantity is <= 0', () {
      final manager = OrderManager();
      expect(
        () => manager.addOrder(
          Order(item: 'A1000', itemName: 'Iphone 15', price: 1200, currency: 'USD', quantity: 0),
        ),
        throwsArgumentError,
      );
    });
  });
}
