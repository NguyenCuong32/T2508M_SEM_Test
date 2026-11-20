import 'package:flutter_test/flutter_test.dart';
import 'package:material_ui/material_ui.dart';
import 'package:my_order/features/order/data/repositories/order_repository.dart';
import 'package:my_order/main.dart';
import 'package:my_order/features/order/data/models/order_model.dart';

class MemoryRepository extends OrderRepository {
  List<Order> orders = [
    const Order(
      item: 'A1000',
      itemName: 'Iphone 15',
      price: 1200,
      currency: 'USD',
      quantity: 1,
    ),
    const Order(
      item: 'A1001',
      itemName: 'Iphone 16',
      price: 1500,
      currency: 'USD',
      quantity: 1,
    ),
  ];
  bool failSave = false;
  bool failLoad = false;

  @override
  Future<List<Order>> load() async {
    if (failLoad) {
      throw Exception('load failed');
    }
    return List.of(orders);
  }

  @override
  Future<void> save(List<Order> value) async {
    if (failSave) {
      throw Exception('save failed');
    }
    orders = List.of(value);
  }
}

void main() {
  Future<void> open(
    WidgetTester tester,
    MemoryRepository repository, {
    Size size = const Size(390, 844),
  }) async {
    tester.view.physicalSize = size;
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    await tester.pumpWidget(MyOrderApp(repository: repository));
    await tester.pumpAndSettle();
  }

  Future<void> fillOrder(WidgetTester tester) async {
    await tester.tap(find.byKey(const Key('addOrder')));
    await tester.pumpAndSettle();
    await tester.enterText(find.byKey(const Key('item')), 'A1002');
    await tester.enterText(find.byKey(const Key('itemName')), 'Iphone 17');
    await tester.enterText(find.byKey(const Key('price')), '1600.50');
    await tester.enterText(find.byKey(const Key('quantity')), '2');
    await tester.ensureVisible(find.byKey(const Key('saveOrder')));
    await tester.tap(find.byKey(const Key('saveOrder')));
    await tester.pumpAndSettle();
  }

  testWidgets('Displays totals and searches by ItemName', (tester) async {
    await open(tester, MemoryRepository());
    expect(find.text('2,700.00 USD'), findsOneWidget);
    await tester.enterText(find.byKey(const Key('search')), 'iPhOnE 16');
    await tester.pumpAndSettle();
    expect(find.text('Iphone 16'), findsOneWidget);
    expect(find.text('Iphone 15'), findsNothing);
    await tester.enterText(find.byKey(const Key('search')), 'Unknown');
    await tester.pumpAndSettle();
    expect(find.text('Không tìm thấy đơn hàng'), findsOneWidget);
    await tester.tap(find.byTooltip('Xóa tìm kiếm'));
    await tester.pumpAndSettle();
    expect(find.text('2/2'), findsOneWidget);
  });

  testWidgets('Validates empty, duplicate, and invalid numeric fields', (
    tester,
  ) async {
    await open(tester, MemoryRepository());
    await tester.tap(find.byKey(const Key('addOrder')));
    await tester.pumpAndSettle();
    await tester.tap(find.byKey(const Key('saveOrder')));
    await tester.pumpAndSettle();
    expect(find.text('Vui lòng nhập mã sản phẩm'), findsOneWidget);
    expect(find.text('Vui lòng nhập tên sản phẩm'), findsOneWidget);
    await tester.enterText(find.byKey(const Key('item')), 'a1000');
    await tester.enterText(find.byKey(const Key('price')), '-5');
    await tester.enterText(find.byKey(const Key('quantity')), '0');
    await tester.ensureVisible(find.byKey(const Key('saveOrder')));
    await tester.tap(find.byKey(const Key('saveOrder')));
    await tester.pumpAndSettle();
    expect(find.text('Mã sản phẩm đã tồn tại'), findsOneWidget);
    expect(
      find.text('Nhập giá lớn hơn 0, tối đa 2 số thập phân'),
      findsOneWidget,
    );
    expect(find.text('Nhập số nguyên > 0'), findsOneWidget);
  });

  testWidgets('Adds order and deletes only after confirmation', (tester) async {
    final repository = MemoryRepository();
    await open(tester, repository);
    await fillOrder(tester);
    expect(repository.orders.length, 3);
    expect(repository.orders.last.total, 3201);
    expect(find.text('5,901.00 USD'), findsOneWidget);
    await tester.pump(const Duration(seconds: 5));
    await tester.enterText(find.byKey(const Key('search')), 'Iphone 17');
    await tester.pumpAndSettle();
    await tester.ensureVisible(find.byTooltip('Xóa Iphone 17'));
    await tester.tap(find.byTooltip('Xóa Iphone 17'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Hủy'));
    await tester.pumpAndSettle();
    expect(repository.orders.length, 3);
    await tester.tap(find.byTooltip('Xóa Iphone 17'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Xóa'));
    await tester.pumpAndSettle();
    expect(repository.orders.length, 2);
    expect(find.text('Không tìm thấy đơn hàng'), findsOneWidget);
  });

  testWidgets('Save failure preserves current orders and reports error', (
    tester,
  ) async {
    final repository = MemoryRepository()..failSave = true;
    await open(tester, repository);
    await fillOrder(tester);
    expect(repository.orders.length, 2);
    expect(
      find.text('Không thể lưu đơn hàng. Vui lòng thử lại.'),
      findsOneWidget,
    );
    expect(find.text('2,700.00 USD'), findsOneWidget);
  });

  testWidgets('Load error can be retried', (tester) async {
    final repository = MemoryRepository()..failLoad = true;
    await open(tester, repository);
    expect(find.text('Thử lại'), findsOneWidget);
    repository.failLoad = false;
    await tester.tap(find.text('Thử lại'));
    await tester.pumpAndSettle();
    expect(find.text('2,700.00 USD'), findsOneWidget);
  });

  testWidgets('Small screen, keyboard, and tablet do not overflow', (
    tester,
  ) async {
    await open(tester, MemoryRepository(), size: const Size(320, 640));
    await tester.scrollUntilVisible(
      find.byKey(const Key('addOrder')),
      180,
      scrollable: find.byType(Scrollable).first,
    );
    await tester.tap(find.byKey(const Key('addOrder')));
    await tester.pumpAndSettle();
    tester.view.viewInsets = const FakeViewPadding(bottom: 260);
    addTearDown(tester.view.resetViewInsets);
    await tester.pumpAndSettle();
    await tester.ensureVisible(find.byKey(const Key('saveOrder')));

    await tester.ensureVisible(find.byTooltip('Đóng'));
    await tester.tap(find.byTooltip('Đóng'));
    await tester.pumpAndSettle();
    tester.view.viewInsets = const FakeViewPadding();
    tester.view.physicalSize = const Size(1024, 800);
    await tester.pumpAndSettle();
  });
}
