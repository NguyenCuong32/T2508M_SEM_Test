import 'dart:convert';
import 'dart:io';

import 'package:flutter_test/flutter_test.dart';
import 'package:my_order/features/order/data/repositories/order_repository.dart';
import 'package:my_order/features/order/data/datasources/order_local_datasource.dart';
import 'package:my_order/features/order/data/models/order_model.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  test('Reads the exact exam JSON and preserves all five fields', () async {
    final orders = OrderLocalDatasource.decode(
      await File('assets/data/order.json').readAsString(),
    );
    expect(orders.length, 2);
    expect(orders.first.item, 'A1000');
    expect(orders.first.itemName, 'Iphone 15');
    expect(orders.first.price, 1200);
    expect(orders.first.currency, 'USD');
    expect(orders.first.quantity, 1);
    expect(orders.last.item, 'A1001');
    expect(orders.last.itemName, 'Iphone 16');
    expect(orders.last.price, 1500);
    expect(
      jsonDecode(OrderLocalDatasource.encode(orders)),
      jsonDecode(await File('assets/data/order.json').readAsString()),
    );
  });

  test(
    'Search matches ItemName without case sensitivity and trims input',
    () async {
      final orders = OrderLocalDatasource.decode(
        await File('assets/data/order.json').readAsString(),
      );
      expect(Order.search(orders, ' IPHONE 16 ').single.item, 'A1001');
      expect(Order.search(orders, 'phone').length, 2);
      expect(Order.search(orders, '').length, 2);
      expect(Order.search(orders, 'A1000'), isEmpty);
      expect(Order.search(orders, 'Samsung'), isEmpty);
    },
  );

  test('Rejects malformed JSON and invalid order fields', () {
    for (final source in [
      'oops',
      '{}',
      '[null]',
      '[{}]',
      '[{"Item":"X","ItemName":"Y","Price":-1,"Currency":"USD","Quantity":1}]',
      '[{"Item":"X","ItemName":"Y","Price":1,"Currency":"USD","Quantity":1.5}]',
    ]) {
      expect(() => OrderLocalDatasource.decode(source), throwsFormatException);
    }
  });

  test('Initializes order.json, persists insertion, reloads, and persists deletion', () async {
    final directory = await Directory.systemTemp.createTemp('my_order_test_');
    addTearDown(() => directory.delete(recursive: true));
    final repository = OrderRepository(directory: directory);
    final orders = await repository.load();
    expect(orders.length, 2);
    expect(await File('${directory.path}/order.json').exists(), isTrue);
    const added = Order(
      item: 'A1002',
      itemName: 'Iphone 17',
      price: 1600.50,
      currency: 'USD',
      quantity: 2,
    );
    await repository.save([...orders, added]);
    final reloaded = await OrderRepository(directory: directory).load();
    expect(reloaded.length, 3);
    expect(reloaded.last.total, 3201);
    expect(reloaded.last.toJson(), added.toJson());
    await repository.save(reloaded.where((o) => o.item != 'A1002').toList());
    expect((await repository.load()).length, 2);
    await repository.save([]);
    expect(await repository.load(), isEmpty);
  });

  test('Does not overwrite corrupted existing data', () async {
    final directory = await Directory.systemTemp.createTemp(
      'my_order_corrupt_',
    );
    addTearDown(() => directory.delete(recursive: true));
    final file = File('${directory.path}/order.json');
    await file.writeAsString('broken');
    await expectLater(
      OrderRepository(directory: directory).load(),
      throwsFormatException,
    );
    expect(await file.readAsString(), 'broken');
  });
}
