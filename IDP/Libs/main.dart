import 'dart:convert';
import 'package:flutter/material.dart';

import 'models/order.dart';
import 'data/order_data.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context){
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'My Order',
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(seedColor: const Colors.(0xFFC56A55)),
        home: const OrderListScreen(),
      )
    )
  }
}

class OrderPage extends StatefulWidget {
  const OrderPage({super.key});

  @override
  State<OrderPage> createState() => _OrderPageState();
}

class _OrderPageState extends State<OrderPage> {

  // Danh sach don hang
  late List<Order> orders;

  @override
  void initState() {
    super.initState();

    // Doc JSON va chuyen thanh List<Order>
    final List<dynamic> jsonList = jsonDecode(orderJson);

    orders = jsonList
        .map((json) => Order.fromJson(json))
        .toList();

    debugPrint('Total orders: ${orders.length}');

    for (var order in orders) {
      debugPrint(order.toString());
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Column(
          children: [

            // Header
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(20),
              color: const Color(0xFFC56A55),
              child: const Text(
                'MY ORDER',
                textAlign: TextAlign.center,
                style: TextStyle(
                  color: Colors.white,
                  fontSize: 26,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),

            // Noi dung
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(24),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [

                    // Tieu de
                    const Text(
                      'Order Management',
                      style: TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                      ),
                    ),

                    const SizedBox(height: 8),

                    Text(
                      'Manage your products and orders',
                      style: TextStyle(
                        color: Colors.grey.shade600,
                        fontSize: 14,
                      ),
                    ),

                    const SizedBox(height: 24),

                    // Thong ke
                    Card(
                      elevation: 1,
                      child: Padding(
                        padding: const EdgeInsets.all(20),
                        child: Row(
                          children: [
                            const Icon(
                              Icons.shopping_cart,
                              size: 35,
                              color: Color(0xFFC56A55),
                            ),
                            const SizedBox(width: 16),
                            Column(
                              crossAxisAlignment:
                                  CrossAxisAlignment.start,
                              children: [
                                const Text('Total Orders'),
                                Text(
                                  '${orders.length}',
                                  style: const TextStyle(
                                    fontSize: 26,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ),

                    const SizedBox(height: 24),

                    // Danh sach don hang
                    const Text(
                      'Order List',
                      style: TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                    ),

                    const SizedBox(height: 16),

                    // Bang du lieu
                    Card(
                      elevation: 1,
                      clipBehavior: Clip.antiAlias,
                      child: SingleChildScrollView(
                        scrollDirection: Axis.horizontal,
                        child: DataTable(
                          headingRowColor:
                              WidgetStateProperty.all(
                            const Color(0xFFC56A55),
                          ),
                          headingTextStyle: const TextStyle(
                            color: Colors.white,
                            fontWeight: FontWeight.bold,
                          ),
                          columns: const [
                            DataColumn(label: Text('ID')),
                            DataColumn(label: Text('Item')),
                            DataColumn(label: Text('Item Name')),
                            DataColumn(label: Text('Quantity')),
                            DataColumn(label: Text('Price')),
                            DataColumn(label: Text('Currency')),
                          ],
                          rows: List.generate(
                            orders.length,
                            (index) {
                              final order = orders[index];

                              return DataRow(
                                cells: [
                                  DataCell(Text('${index + 1}')),
                                  DataCell(Text(order.item)),
                                  DataCell(Text(order.itemName)),
                                  DataCell(
                                    Text('${order.quantity}'),
                                  ),
                                  DataCell(
                                    Text(
                                      order.price.toStringAsFixed(2),
                                    ),
                                  ),
                                  DataCell(Text(order.currency)),
                                ],
                              );
                            },
                          ),
                        ),
                      ),
                    ),

                    const SizedBox(height: 30),

                    // Footer
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: const Color(0xFFC56A55),
                        borderRadius: BorderRadius.circular(8),
                      ),
                      child: const Text(
                        'Dart SET01 - Order Management',
                        style: TextStyle(
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}