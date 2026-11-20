import 'dart:io';

import 'package:my_order_app/order.dart';

void main() async {
  stdout.writeln('Running Unit Tests for my_order_app...');

  // 1. Test Order class initialization and getters
  final Order o1 = Order(
    item: 'A1000',
    itemName: 'Iphone 15',
    price: 1200,
    currency: 'USD',
    quantity: 1,
  );
  assert(o1.item == 'A1000', 'Item must be A1000');
  assert(o1.itemName == 'Iphone 15', 'ItemName must be Iphone 15');
  assert(o1.price == 1200, 'Price must be 1200');
  assert(o1.formattedPrice == '1.200', 'Formatted price must be 1.200');
  assert(o1.total == 1200, 'Total must be 1200');

  // 2. Test toJson & fromJson
  final Map<String, dynamic> jsonMap = o1.toJson();
  assert(jsonMap['Item'] == 'A1000');
  assert(jsonMap['ItemName'] == 'Iphone 15');
  assert(jsonMap['Price'] == 1200);
  assert(jsonMap['Currency'] == 'USD');
  assert(jsonMap['Quantity'] == 1);

  final Order o1FromJson = Order.fromJson(jsonMap);
  assert(o1FromJson.item == o1.item);
  assert(o1FromJson.price == o1.price);

  // 3. Test parsing from exam JSON string
  final List<Order> parsedList = parseOrdersFromString(initialOrdersJson);
  assert(parsedList.length == 2, 'Must parse exactly 2 orders from initialOrdersJson');
  assert(parsedList[0].item == 'A1000');
  assert(parsedList[1].item == 'A1001');
  assert(parsedList[1].itemName == 'Iphone 16');
  assert(parsedList[1].formattedPrice == '1.500');

  // 4. Test searching by ItemName
  final List<Order> search15 = searchOrdersByItemName(parsedList, 'Iphone 15');
  assert(search15.length == 1, 'Should find 1 Iphone 15');
  assert(search15.first.item == 'A1000');

  final List<Order> searchCaseInsensitive = searchOrdersByItemName(parsedList, 'iphone');
  assert(searchCaseInsensitive.length == 2, 'Should find both Iphone orders');

  final List<Order> searchNone = searchOrdersByItemName(parsedList, 'NonExistent');
  assert(searchNone.isEmpty, 'Should return empty list for non-matching query');

  // 5. Test JSON file write & read
  final String testFile = 'test_orders_temp.json';
  await saveOrdersToFile(testFile, parsedList);
  final List<Order> loadedFromDisk = await loadOrdersFromFile(testFile);
  assert(loadedFromDisk.length == 2, 'Disk loaded orders length must be 2');

  // 6. Test insert into JSON file
  final Order newOrder = Order(
    item: 'A1002',
    itemName: 'Iphone 16 Pro Max',
    price: 1800,
    currency: 'USD',
    quantity: 2,
  );
  await insertOrderIntoJsonFile(testFile, newOrder);
  final List<Order> updatedList = await loadOrdersFromFile(testFile);
  assert(updatedList.length == 3, 'Updated list must have 3 orders');
  assert(updatedList.last.item == 'A1002');
  assert(updatedList.last.total == 3600);

  // 7. Test update order in JSON file
  final Order editedOrder = Order(
    item: 'A1000',
    itemName: 'Iphone 15 Plus Updated',
    price: 1300,
    currency: 'USD',
    quantity: 3,
  );
  final bool updateSuccess = await updateOrderInJsonFile(testFile, 0, editedOrder);
  assert(updateSuccess, 'Update must succeed');
  final List<Order> listAfterUpdate = await loadOrdersFromFile(testFile);
  assert(listAfterUpdate[0].itemName == 'Iphone 15 Plus Updated');
  assert(listAfterUpdate[0].price == 1300);
  assert(listAfterUpdate[0].quantity == 3);

  // 8. Test filter and sort
  final List<Order> sortedByPrice = filterAndSortOrders(
    listAfterUpdate,
    sort: OrderSortOption.priceAsc,
  );
  assert(sortedByPrice.first.price <= sortedByPrice.last.price, 'Must be sorted ascending by price');

  final List<Order> filteredByCurrency = filterAndSortOrders(
    listAfterUpdate,
    currency: 'USD',
  );
  assert(filteredByCurrency.length == 3, 'All 3 are USD');

  // Clean up temporary test file
  final File temp = File(testFile);
  if (await temp.exists()) {
    await temp.delete();
  }

  stdout.writeln('ALL TESTS PASSED SUCCESSFULLY! (8/8 test groups passed)');
}
