import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:my_order/data/default_orders.dart';
import 'package:my_order/models/order.dart';
import 'package:my_order/screens/my_order_screen.dart';
import 'package:my_order/services/order_storage.dart';
import 'package:my_order/utils/price_formatter.dart';

class FakeOrderStorage implements OrderStorage {
  String? savedJson;
  int exportCount = 0;

  @override
  bool get supportsBrowserDownload => false;

  @override
  String get storageLocation => 'test/order.json';

  @override
  Future<List<Order>> loadOrders(String defaultJson) async {
    return Order.listFromJsonString(savedJson ?? defaultJson);
  }

  @override
  Future<StorageSaveResult> saveOrders(
    List<Order> orders, {
    bool triggerDownload = false,
  }) async {
    savedJson = Order.listToJsonString(orders);
    return const StorageSaveResult(
      success: true,
      location: 'test/order.json',
      message: 'Saved to test/order.json',
    );
  }

  @override
  Future<StorageSaveResult> exportOrders(List<Order> orders) async {
    exportCount++;
    return saveOrders(orders, triggerDownload: true);
  }
}

void main() {
  group('Part A: Order model & JSON serialization', () {
    test('parses default JSON string into List<Order>', () {
      final List<Order> orders = Order.listFromJsonString(defaultOrdersJson);

      expect(orders.length, 2);
      expect(orders[0].Item, 'A1000');
      expect(orders[0].ItemName, 'Iphone 15');
      expect(orders[0].Price, 1200);
      expect(orders[0].Currency, 'USD');
      expect(orders[0].Quantity, 1);

      expect(orders[1].Item, 'A1001');
      expect(orders[1].ItemName, 'Iphone 16');
      expect(orders[1].Price, 1500);
      expect(orders[1].Currency, 'USD');
      expect(orders[1].Quantity, 1);
    });

    test('serializes Order toJson and round-trips cleanly', () {
      const Order order = Order(
        Item: 'A1002',
        ItemName: 'Iphone 17 Pro',
        Price: 1800,
        Currency: 'USD',
        Quantity: 2,
      );

      final Map<String, dynamic> map = order.toJson();
      expect(map['Item'], 'A1002');
      expect(map['ItemName'], 'Iphone 17 Pro');
      expect(map['Price'], 1800);
      expect(map['Currency'], 'USD');
      expect(map['Quantity'], 2);

      final Order reconstructed = Order.fromJson(map);
      expect(reconstructed, order);
    });

    test('formats prices with thousands separator', () {
      expect(formatPrice(1200), '1,200');
      expect(formatPrice(1500), '1,500');
      expect(formatPrice(1250000), '1,250,000');
      expect(formatPrice(1200.5), '1,200.5');
    });
  });

  group('Parts B, C, D: My Order UI, Add/Save, Search, Delete', () {
    testWidgets('displays default orders from JSON and defaults Currency to USD',
        (WidgetTester tester) async {
      final FakeOrderStorage storage = FakeOrderStorage();
      await tester.pumpWidget(
        MaterialApp(home: MyOrderScreen(storage: storage)),
      );
      await tester.pumpAndSettle();

      expect(find.text('My Order'), findsOneWidget);
      expect(find.text('A1000'), findsOneWidget);
      expect(find.text('Iphone 15'), findsOneWidget);
      expect(find.text('1,200'), findsOneWidget);
      expect(find.text('A1001'), findsOneWidget);
      expect(find.text('Iphone 16'), findsOneWidget);
      expect(find.text('1,500'), findsOneWidget);

      final TextFormField currencyField =
          tester.widget<TextFormField>(find.byKey(const Key('currency_field')));
      expect(currencyField.controller?.text, 'USD');
    });

    testWidgets('validates form, adds new Order, and persists to order.json',
        (WidgetTester tester) async {
      final FakeOrderStorage storage = FakeOrderStorage();
      await tester.pumpWidget(
        MaterialApp(home: MyOrderScreen(storage: storage)),
      );
      await tester.pumpAndSettle();

      // Submitting empty form triggers validation errors
      final Finder addButton = find.byKey(const Key('add_item_button'));
      await tester.ensureVisible(addButton);
      await tester.pumpAndSettle();
      await tester.tap(addButton);
      await tester.pumpAndSettle();
      expect(find.text('Please enter Item code'), findsOneWidget);
      expect(find.text('Please enter Item Name'), findsOneWidget);
      expect(find.text('Please enter Price'), findsOneWidget);
      expect(find.text('Please enter Quantity'), findsOneWidget);

      // Fill valid order data
      await tester.enterText(find.byKey(const Key('item_field')), 'A1002');
      await tester.enterText(
        find.byKey(const Key('item_name_field')),
        'MacBook Pro',
      );
      await tester.enterText(find.byKey(const Key('price_field')), '2500');
      await tester.enterText(find.byKey(const Key('quantity_field')), '2');
      await tester.ensureVisible(addButton);
      await tester.pumpAndSettle();
      await tester.tap(addButton);
      await tester.pumpAndSettle();

      expect(find.text('A1002'), findsOneWidget);
      expect(find.text('MacBook Pro'), findsOneWidget);
      expect(find.text('2,500'), findsOneWidget);
      expect(storage.savedJson, isNotNull);

      final List<Order> saved = Order.listFromJsonString(storage.savedJson!);
      expect(saved.length, 3);
      expect(saved.last.Item, 'A1002');
      expect(saved.last.ItemName, 'MacBook Pro');
      expect(saved.last.Price, 2500);
      expect(saved.last.Quantity, 2);
      expect(saved.last.Currency, 'USD');
    });

    testWidgets('filters orders case-insensitively by ItemName and restores on clear',
        (WidgetTester tester) async {
      final FakeOrderStorage storage = FakeOrderStorage();
      await tester.pumpWidget(
        MaterialApp(home: MyOrderScreen(storage: storage)),
      );
      await tester.pumpAndSettle();

      final Finder searchField = find.byKey(const Key('search_field'));
      await tester.ensureVisible(searchField);
      await tester.pumpAndSettle();
      await tester.enterText(searchField, 'iphone 16');
      await tester.pumpAndSettle();

      expect(find.text('Iphone 16'), findsOneWidget);
      expect(find.text('Iphone 15'), findsNothing);

      final Finder clearBtn = find.byKey(const Key('clear_search_button'));
      await tester.ensureVisible(clearBtn);
      await tester.pumpAndSettle();
      await tester.tap(clearBtn);
      await tester.pumpAndSettle();

      expect(find.text('Iphone 15'), findsOneWidget);
      expect(find.text('Iphone 16'), findsOneWidget);
    });

    testWidgets('deletes an order and updates order.json',
        (WidgetTester tester) async {
      final FakeOrderStorage storage = FakeOrderStorage();
      await tester.pumpWidget(
        MaterialApp(home: MyOrderScreen(storage: storage)),
      );
      await tester.pumpAndSettle();

      final Finder deleteButton = find.byKey(const Key('delete_order_A1000'));
      await tester.ensureVisible(deleteButton);
      await tester.pumpAndSettle();
      await tester.tap(deleteButton);
      await tester.pumpAndSettle();

      expect(find.text('Iphone 15'), findsNothing);
      expect(find.text('Iphone 16'), findsOneWidget);

      final List<Order> saved = Order.listFromJsonString(storage.savedJson!);
      expect(saved.length, 1);
      expect(saved.first.Item, 'A1001');
    });

    testWidgets('renders cleanly on Pixel 7 Pro mobile viewport without overflow',
        (WidgetTester tester) async {
      tester.view.physicalSize = const Size(412, 915);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.reset);

      final FakeOrderStorage storage = FakeOrderStorage();
      await tester.pumpWidget(
        MaterialApp(home: MyOrderScreen(storage: storage)),
      );
      await tester.pumpAndSettle();

      expect(find.text('My Order'), findsOneWidget);
      expect(find.text('Add Item to Cart'), findsOneWidget);
      expect(tester.takeException(), isNull);
    });
  });
}
