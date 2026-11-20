import 'package:flutter/material.dart';

import '../data/default_orders.dart';
import '../models/order.dart';
import '../services/order_storage.dart';
import '../widgets/order_form_card.dart';
import '../widgets/order_table_card.dart';

/// Clean, minimalist "My Order" screen (Dart SET01) responsive for both
/// Flutter Web (Chrome/Edge) and Android Emulator (Pixel 7 Pro).
class MyOrderScreen extends StatefulWidget {
  final OrderStorage? storage;

  const MyOrderScreen({
    super.key,
    this.storage,
  });

  @override
  State<MyOrderScreen> createState() => _MyOrderScreenState();
}

class _MyOrderScreenState extends State<MyOrderScreen> {
  static const Color _primaryOrange = Color(0xFFEA580C);

  late final OrderStorage _storage;
  final TextEditingController _searchController = TextEditingController();

  List<Order> orders = <Order>[];
  String _searchQuery = '';
  bool _isLoading = true;
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    _storage = widget.storage ?? OrderStorage();
    _loadInitialOrders();
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _loadInitialOrders() async {
    final List<Order> loaded = await _storage.loadOrders(defaultOrdersJson);
    if (!mounted) return;
    setState(() {
      orders = loaded;
      _isLoading = false;
    });
  }

  List<IndexedOrder> get _filteredOrders {
    final String normalizedQuery = _searchQuery.trim().toLowerCase();
    final List<IndexedOrder> result = <IndexedOrder>[];

    for (int i = 0; i < orders.length; i++) {
      final Order order = orders[i];
      if (normalizedQuery.isEmpty ||
          order.ItemName.toLowerCase().contains(normalizedQuery)) {
        result.add(IndexedOrder(originalIndex: i, order: order));
      }
    }
    return result;
  }

  Future<void> _addOrder(Order newOrder) async {
    setState(() {
      _isSaving = true;
      orders = <Order>[...orders, newOrder];
    });

    final StorageSaveResult result = await _storage.saveOrders(orders);

    if (!mounted) return;
    setState(() {
      _isSaving = false;
    });

    _showSnackBar(
      message: result.success
          ? 'Added "${newOrder.ItemName}" & saved to order.json'
          : result.message,
      isError: !result.success,
    );
  }

  Future<void> _deleteOrder(int originalIndex, Order order) async {
    if (originalIndex < 0 || originalIndex >= orders.length) return;

    final List<Order> updated = List<Order>.from(orders)
      ..removeAt(originalIndex);
    setState(() {
      orders = updated;
    });

    final StorageSaveResult result = await _storage.saveOrders(orders);
    if (!mounted) return;

    _showSnackBar(
      message: 'Deleted "${order.ItemName}" & updated order.json',
      isError: !result.success,
    );
  }

  Future<void> _exportOrderJson() async {
    final StorageSaveResult result = await _storage.exportOrders(orders);
    if (!mounted) return;
    _showSnackBar(
      message: result.message,
      isError: !result.success,
    );
  }

  void _showSnackBar({required String message, bool isError = false}) {
    final ScaffoldMessengerState messenger = ScaffoldMessenger.of(context);
    messenger.hideCurrentSnackBar();
    messenger.showSnackBar(
      SnackBar(
        content: Row(
          children: <Widget>[
            Icon(
              isError
                  ? Icons.error_outline_rounded
                  : Icons.check_circle_rounded,
              color: Colors.white,
              size: 19,
            ),
            const SizedBox(width: 10),
            Expanded(
              child: Text(
                message,
                style: const TextStyle(
                  fontWeight: FontWeight.w600,
                  fontSize: 13.5,
                ),
              ),
            ),
          ],
        ),
        backgroundColor:
            isError ? const Color(0xFFDC2626) : const Color(0xFF0F172A),
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        margin: const EdgeInsets.all(16),
        duration: const Duration(seconds: 3),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final bool isCompact = MediaQuery.sizeOf(context).width < 560;
    final List<IndexedOrder> visibleOrders = _filteredOrders;

    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            colors: <Color>[
              Color(0xFFFFF7F0),
              Color(0xFFF8FAFC),
              Color(0xFFF1F5F9),
            ],
            stops: <double>[0.0, 0.35, 1.0],
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
          ),
        ),
        child: SafeArea(
          child: _isLoading
              ? const Center(
                  child: CircularProgressIndicator(color: _primaryOrange),
                )
              : GestureDetector(
                  onTap: () => FocusScope.of(context).unfocus(),
                  child: SingleChildScrollView(
                    padding: EdgeInsets.symmetric(
                      horizontal: isCompact ? 14 : 32,
                      vertical: isCompact ? 24 : 40,
                    ),
                    child: Center(
                      child: ConstrainedBox(
                        constraints: const BoxConstraints(maxWidth: 960),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: <Widget>[
                            // Centered Title
                            Text(
                              'My Order',
                              textAlign: TextAlign.center,
                              style: TextStyle(
                                fontSize: isCompact ? 30 : 38,
                                fontWeight: FontWeight.w800,
                                color: _primaryOrange,
                                letterSpacing: -0.8,
                              ),
                            ),
                            SizedBox(height: isCompact ? 22 : 30),

                            // 2-Column Add Order Form
                            OrderFormCard(
                              onAddOrder: _addOrder,
                              isSaving: _isSaving,
                            ),
                            SizedBox(height: isCompact ? 18 : 24),

                            // Search Bar + Order Table
                            OrderTableCard(
                              items: visibleOrders,
                              searchQuery: _searchQuery,
                              searchController: _searchController,
                              onSearchChanged: (String value) {
                                setState(() {
                                  _searchQuery = value;
                                });
                              },
                              onSearchClear: () {
                                _searchController.clear();
                                setState(() {
                                  _searchQuery = '';
                                });
                              },
                              onDelete: _deleteOrder,
                              onExportJson: _exportOrderJson,
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ),
        ),
      ),
    );
  }
}
