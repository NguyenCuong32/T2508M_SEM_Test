
import 'dart:convert';
import 'dart:html' as html;

import 'package:flutter/material.dart';

import '../models/order.dart';
import '../services/order_service.dart';

class OrderScreen extends StatefulWidget {
  const OrderScreen({super.key});

  @override
  State<OrderScreen> createState() => _OrderScreenState();
}

class _OrderScreenState extends State<OrderScreen> {
  List<Order> orders = [];
  List<Order> filteredOrders = [];

  bool isLoading = true;

  final itemController = TextEditingController();
  final itemNameController = TextEditingController();
  final priceController = TextEditingController();
  final quantityController = TextEditingController();
  final searchController = TextEditingController();

  String currency = 'USD';

  @override
  void initState() {
    super.initState();
    loadData();
  }

  // Read order.json
  Future<void> loadData() async {
    try {
      final result = await OrderService.loadOrders();

      setState(() {
        orders = result;
        filteredOrders = result;
        isLoading = false;
      });
    } catch (e) {
      setState(() {
        isLoading = false;
      });

      showMessage('Cannot load order.json: $e');
    }
  }

  // Search by ItemName
  void searchOrder(String keyword) {
    setState(() {
      filteredOrders = orders.where((order) {
        return order.itemName
            .toLowerCase()
            .contains(keyword.toLowerCase());
      }).toList();
    });
  }

  // Add new Order
  void addOrder() {
    if (itemController.text.trim().isEmpty ||
        itemNameController.text.trim().isEmpty ||
        priceController.text.trim().isEmpty ||
        quantityController.text.trim().isEmpty) {
      showMessage('Please enter all information');
      return;
    }

    final price = double.tryParse(priceController.text);
    final quantity = int.tryParse(quantityController.text);

    if (price == null || price < 0 ||
        quantity == null || quantity <= 0) {
      showMessage('Price or Quantity is invalid');
      return;
    }

    final exists = orders.any(
          (order) => order.item.toLowerCase() ==
          itemController.text.trim().toLowerCase(),
    );

    if (exists) {
      showMessage('Item already exists');
      return;
    }

    final newOrder = Order(
      item: itemController.text.trim(),
      itemName: itemNameController.text.trim(),
      price: price,
      currency: currency,
      quantity: quantity,
    );

    setState(() {
      orders.add(newOrder);
      filteredOrders = List.from(orders);
    });

    itemController.clear();
    itemNameController.clear();
    priceController.clear();
    quantityController.clear();

    showMessage('Order added successfully');
  }

  // Delete Order
  void deleteOrder(Order order) {
    setState(() {
      orders.remove(order);
      filteredOrders.remove(order);
    });

    showMessage('Order deleted');
  }

  // Export updated JSON file
  void exportJson() {
    final jsonString = OrderService.ordersToJson(orders);

    final bytes = utf8.encode(jsonString);
    final blob = html.Blob(
      [bytes],
      'application/json',
    );

    final url = html.Url.createObjectUrlFromBlob(blob);

    final anchor = html.AnchorElement(href: url)
      ..setAttribute('download', 'order.json')
      ..click();

    html.Url.revokeObjectUrl(url);

    showMessage('JSON exported successfully');
  }

  void showMessage(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message)),
    );
  }

  Widget buildInput(
      String label,
      TextEditingController controller,
      double width, {
        TextInputType keyboardType = TextInputType.text,
      }) {
    return SizedBox(
      width: width,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(label),
          const SizedBox(height: 8),
          TextField(
            controller: controller,
            keyboardType: keyboardType,
            decoration: const InputDecoration(
              border: OutlineInputBorder(),
              contentPadding: EdgeInsets.symmetric(
                horizontal: 12,
                vertical: 12,
              ),
            ),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: isLoading
          ? const Center(child: CircularProgressIndicator())
          : Center(
        child: Container(
          constraints: const BoxConstraints(maxWidth: 1100),
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              const Text(
                'My Order',
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.bold,
                  color: Color(0xFFD5A02D),
                ),
              ),

              const SizedBox(height: 24),

              // Input form
              Wrap(
                spacing: 24,
                runSpacing: 18,
                children: [
                  buildInput(
                    'Item',
                    itemController,
                    220,
                  ),
                  buildInput(
                    'Item Name',
                    itemNameController,
                    300,
                  ),
                  buildInput(
                    'Price',
                    priceController,
                    180,
                    keyboardType: TextInputType.number,
                  ),
                  buildInput(
                    'Quantity',
                    quantityController,
                    130,
                    keyboardType: TextInputType.number,
                  ),
                  SizedBox(
                    width: 150,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text('Currency'),
                        const SizedBox(height: 8),
                        DropdownButtonFormField<String>(
                          value: currency,
                          decoration: const InputDecoration(
                            border: OutlineInputBorder(),
                          ),
                          items: ['USD', 'VND', 'EUR']
                              .map((value) =>
                              DropdownMenuItem(
                                value: value,
                                child: Text(value),
                              ))
                              .toList(),
                          onChanged: (value) {
                            if (value != null) {
                              setState(() {
                                currency = value;
                              });
                            }
                          },
                        ),
                      ],
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 16),

              Wrap(
                alignment: WrapAlignment.end,
                spacing: 12,
                children: [
                  ElevatedButton(
                    onPressed: addOrder,
                    style: ElevatedButton.styleFrom(
                      backgroundColor:
                      const Color(0xFFC96550),
                      foregroundColor: Colors.white,
                    ),
                    child: const Text('Add Item to Cart'),
                  ),
                  ElevatedButton.icon(
                    onPressed: exportJson,
                    icon: const Icon(Icons.download),
                    label: const Text('Export JSON'),
                  ),
                ],
              ),

              const SizedBox(height: 24),

              // Search
              TextField(
                controller: searchController,
                onChanged: searchOrder,
                decoration: const InputDecoration(
                  labelText: 'Search by Item Name',
                  hintText: 'Enter item name...',
                  prefixIcon: Icon(Icons.search),
                  border: OutlineInputBorder(),
                ),
              ),

              const SizedBox(height: 20),

              // Order table
              Expanded(
                child: SingleChildScrollView(
                  child: SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    child: DataTable(
                      headingRowColor:
                      WidgetStateProperty.all(
                        const Color(0xFFC96550),
                      ),
                      headingTextStyle: const TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.bold,
                      ),
                      columns: const [
                        DataColumn(label: Text('Id')),
                        DataColumn(label: Text('Item')),
                        DataColumn(label: Text('Item Name')),
                        DataColumn(label: Text('Quantity')),
                        DataColumn(label: Text('Price')),
                        DataColumn(label: Text('Currency')),
                        DataColumn(label: Text('Action')),
                      ],
                      rows: filteredOrders
                          .asMap()
                          .entries
                          .map((entry) {
                        final index = entry.key;
                        final order = entry.value;

                        return DataRow(
                          cells: [
                            DataCell(Text('${index + 1}')),
                            DataCell(Text(order.item)),
                            DataCell(Text(order.itemName)),
                            DataCell(Text('${order.quantity}')),
                            DataCell(Text(
                              order.price.toStringAsFixed(0),
                            )),
                            DataCell(Text(order.currency)),
                            DataCell(
                              IconButton(
                                icon: const Icon(
                                  Icons.delete_outline,
                                  color: Colors.blueGrey,
                                ),
                                onPressed: () =>
                                    deleteOrder(order),
                              ),
                            ),
                          ],
                        );
                      }).toList(),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  void dispose() {
    itemController.dispose();
    itemNameController.dispose();
    priceController.dispose();
    quantityController.dispose();
    searchController.dispose();
    super.dispose();
  }
}
