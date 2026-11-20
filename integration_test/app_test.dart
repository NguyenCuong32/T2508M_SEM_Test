import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:material_ui/material_ui.dart';
import 'package:my_order/features/order/data/repositories/order_repository.dart';
import 'package:my_order/main.dart';
import 'package:my_order/features/order/data/datasources/order_local_datasource.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  Future<void> waitFor(WidgetTester tester, Finder finder) async {
    for (var i = 0; i < 100 && finder.evaluate().isEmpty; i++) {
      await tester.pump(const Duration(milliseconds: 100));
    }
    expect(
      finder,
      findsOneWidget,
      reason: tester
          .widgetList<Text>(find.byType(Text))
          .map((text) => text.data)
          .join(' | '),
    );
  }

  testWidgets('Android JSON lifecycle: load, add, search, reopen, delete', (
    tester,
  ) async {
    final repository = OrderRepository();
    final initial = OrderLocalDatasource.decode(
      await rootBundle.loadString('assets/data/order.json'),
    );
    await repository.save(initial);
    await tester.pumpWidget(MyOrderApp(repository: repository));
    await tester.pumpAndSettle();
    await waitFor(tester, find.text('2,700.00 USD'));
    await tester.ensureVisible(find.byKey(const Key('addOrder')));
    await tester.tap(find.byKey(const Key('addOrder')));
    await tester.pumpAndSettle();
    await tester.enterText(find.byKey(const Key('item')), 'A1002');
    await tester.enterText(find.byKey(const Key('itemName')), 'Iphone 17');
    await tester.enterText(find.byKey(const Key('price')), '1600.50');
    await tester.enterText(find.byKey(const Key('quantity')), '2');
    FocusManager.instance.primaryFocus?.unfocus();
    await tester.pumpAndSettle();
    await tester.ensureVisible(find.byKey(const Key('saveOrder')));
    await tester.tap(find.byKey(const Key('saveOrder')));
    await tester.pumpAndSettle();
    await tester.pump(const Duration(seconds: 5));
    expect((await repository.load()).last.item, 'A1002');
    await waitFor(tester, find.text('5,901.00 USD'));
    await tester.pumpWidget(const SizedBox.shrink());
    await tester.pumpAndSettle();
    await tester.pumpWidget(MyOrderApp(repository: OrderRepository()));
    await tester.pumpAndSettle();
    await waitFor(tester, find.text('5,901.00 USD'));
    await tester.enterText(find.byKey(const Key('search')), ' IPHONE 17 ');
    FocusManager.instance.primaryFocus?.unfocus();
    await tester.pumpAndSettle();
    expect(find.text('Iphone 17'), findsOneWidget);
    expect(find.text('Iphone 15'), findsNothing);
    await tester.ensureVisible(find.byTooltip('Xóa Iphone 17'));
    await tester.tap(find.byTooltip('Xóa Iphone 17'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Xóa'));
    await tester.pumpAndSettle();
    await waitFor(tester, find.text('Không tìm thấy đơn hàng'));
    expect((await OrderRepository().load()).length, 2);
    await tester.tap(find.byTooltip('Xóa tìm kiếm'));
    await tester.pumpAndSettle();
    expect(find.text('2,700.00 USD'), findsOneWidget);
  });
}
