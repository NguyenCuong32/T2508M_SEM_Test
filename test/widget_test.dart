import 'package:flutter_test/flutter_test.dart';
import 'package:order_management/main.dart';
import 'package:order_management/models/order.dart';
import 'package:order_management/services/order_service.dart';
import 'package:order_management/services/storage/storage_stub.dart';

void main() {
  test('Order model and json serialization', () {
    final order = Order(
      item: 'A1000',
      itemName: 'Iphone 15',
      price: 1200.0,
      currency: 'USD',
      quantity: 1,
    );

    final map = order.toJson();
    expect(map['Item'], 'A1000');
    expect(map['ItemName'], 'Iphone 15');
    expect(map['Price'], 1200.0);
    expect(map['Currency'], 'USD');
    expect(map['Quantity'], 1);

    final fromJsonOrder = Order.fromJson(map);
    expect(fromJsonOrder.item, 'A1000');
    expect(fromJsonOrder.itemName, 'Iphone 15');
    expect(fromJsonOrder.price, 1200.0);
    expect(fromJsonOrder.currency, 'USD');
    expect(fromJsonOrder.quantity, 1);
  });

  test('OrderService operations', () async {
    final service = OrderService(storage: InMemoryStorage());
    final loaded = await service.loadOrders();
    expect(loaded.length, 2);
    expect(loaded[0].item, 'A1000');
    expect(loaded[1].item, 'A1001');

    final newOrder = Order(
      item: 'A1002',
      itemName: 'Iphone 17',
      price: 1800.0,
      currency: 'USD',
      quantity: 2,
    );
    await service.insertOrder(newOrder);
    expect(service.orders.length, 3);

    final searchResults = service.searchOrderByItemName('Iphone 17');
    expect(searchResults.length, 1);
    expect(searchResults.first.item, 'A1002');

    final searchEmpty = service.searchOrderByItemName('NonExistent');
    expect(searchEmpty.isEmpty, true);

    await service.deleteOrderByItem('A1000');
    expect(service.orders.length, 2);
  });

  testWidgets('OrderApp smoke test', (WidgetTester tester) async {
    await tester.runAsync(() async {
      await tester.pumpWidget(const OrderApp());
      await Future.delayed(const Duration(milliseconds: 100));
      await tester.pump();
      await Future.delayed(const Duration(milliseconds: 100));
      await tester.pump();
    });

    expect(find.text('My Order'), findsOneWidget);
    expect(find.text('Add Item to Cart'), findsOneWidget);
    expect(find.text('Số 8, Tôn Thất Thuyết, Cầu Giấy, Hà Nội'), findsOneWidget);
  });
}
