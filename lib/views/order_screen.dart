import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import '../models/order.dart';
import '../services/order_service.dart';

class OrderScreen extends StatefulWidget {
  const OrderScreen({super.key});

  @override
  State<OrderScreen> createState() => _OrderScreenState();
}

class _OrderScreenState extends State<OrderScreen> {
  final OrderService _orderService = OrderService();
  final _formKey = GlobalKey<FormState>();

  final TextEditingController _itemController = TextEditingController();
  final TextEditingController _itemNameController = TextEditingController();
  final TextEditingController _priceController = TextEditingController();
  final TextEditingController _quantityController = TextEditingController();
  final TextEditingController _currencyController =
      TextEditingController(text: 'USD');
  final TextEditingController _searchController = TextEditingController();

  List<Order> _displayOrders = [];
  bool _isLoading = true;

  static const Color primaryRed = Color(0xFFD32F2F);
  static const Color headerRed = Color(0xFFC05A46);
  static const Color titleColor = Color(0xFFD49B35);
  static const double defaultRadius = 4.0;

  @override
  void initState() {
    super.initState();
    _loadData();
    _searchController.addListener(_onSearchChanged);
  }

  @override
  void dispose() {
    _itemController.dispose();
    _itemNameController.dispose();
    _priceController.dispose();
    _quantityController.dispose();
    _currencyController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _loadData() async {
    setState(() {
      _isLoading = true;
    });
    final loaded = await _orderService.loadOrders();
    setState(() {
      _displayOrders = loaded;
      _isLoading = false;
    });
  }

  void _onSearchChanged() {
    setState(() {
      _displayOrders = _orderService.searchOrderByItemName(_searchController.text);
    });
  }

  Future<void> _addItem() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final newOrder = Order(
      item: _itemController.text.trim(),
      itemName: _itemNameController.text.trim(),
      price: double.tryParse(_priceController.text.trim()) ?? 0.0,
      currency: _currencyController.text.trim().isEmpty
          ? 'USD'
          : _currencyController.text.trim(),
      quantity: int.tryParse(_quantityController.text.trim()) ?? 1,
    );

    await _orderService.insertOrder(newOrder);

    _itemController.clear();
    _itemNameController.clear();
    _priceController.clear();
    _quantityController.clear();
    _currencyController.text = 'USD';

    _onSearchChanged();

    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: const Text('Item added to order successfully!'),
        backgroundColor: primaryRed,
        behavior: SnackBarBehavior.floating,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(defaultRadius),
        ),
      ),
    );
  }

  Future<void> _deleteItem(Order order) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(defaultRadius),
        ),
        title: const Text('Delete Item'),
        content: Text('Are you sure you want to delete ${order.item} (${order.itemName})?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: const Text('Cancel', style: TextStyle(color: Colors.grey)),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: primaryRed,
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(defaultRadius),
              ),
            ),
            onPressed: () => Navigator.of(ctx).pop(true),
            child: const Text('Delete'),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      await _orderService.deleteOrderByItem(order.item);
      _onSearchChanged();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Item ${order.item} deleted.'),
          backgroundColor: primaryRed,
          behavior: SnackBarBehavior.floating,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(defaultRadius),
          ),
        ),
      );
    }
  }

  String _formatPrice(double price) {
    if (price % 1 == 0) {
      final formatted = price.toInt().toString().replaceAllMapped(
            RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
            (m) => '${m[1]}.',
          );
      return formatted;
    }
    return price.toStringAsFixed(2);
  }

  InputDecoration _buildInputDecoration(String label, String hint) {
    return InputDecoration(
      labelText: label,
      hintText: hint,
      labelStyle: const TextStyle(color: Colors.black54, fontSize: 13),
      hintStyle: const TextStyle(color: Colors.black26, fontSize: 13),
      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      isDense: true,
      filled: true,
      fillColor: Colors.white,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(defaultRadius),
        borderSide: const BorderSide(color: Color(0xFFCCCCCC)),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(defaultRadius),
        borderSide: const BorderSide(color: Color(0xFFCCCCCC)),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(defaultRadius),
        borderSide: const BorderSide(color: primaryRed, width: 1.5),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(defaultRadius),
        borderSide: const BorderSide(color: primaryRed),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(defaultRadius),
        borderSide: const BorderSide(color: primaryRed, width: 1.5),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: _isLoading
            ? const Center(
                child: CircularProgressIndicator(color: primaryRed),
              )
            : SingleChildScrollView(
                padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
                child: Center(
                  child: ConstrainedBox(
                    constraints: const BoxConstraints(maxWidth: 860),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        const SizedBox(height: 10),
                        const Center(
                          child: Text(
                            'My Order',
                            style: TextStyle(
                              fontSize: 28,
                              fontWeight: FontWeight.bold,
                              color: titleColor,
                              letterSpacing: 0.5,
                            ),
                          ),
                        ),
                        const SizedBox(height: 24),
                        Form(
                          key: _formKey,
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Row(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Expanded(
                                    child: TextFormField(
                                      controller: _itemController,
                                      decoration: _buildInputDecoration('Item', 'Item'),
                                      validator: (val) {
                                        if (val == null || val.trim().isEmpty) {
                                          return 'Please enter item code';
                                        }
                                        return null;
                                      },
                                    ),
                                  ),
                                  const SizedBox(width: 20),
                                  Expanded(
                                    child: TextFormField(
                                      controller: _itemNameController,
                                      decoration: _buildInputDecoration('Item Name', 'Item'),
                                      validator: (val) {
                                        if (val == null || val.trim().isEmpty) {
                                          return 'Please enter item name';
                                        }
                                        return null;
                                      },
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 16),
                              Row(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Expanded(
                                    child: TextFormField(
                                      controller: _priceController,
                                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                                      inputFormatters: [
                                        FilteringTextInputFormatter.allow(RegExp(r'^\d*\.?\d*')),
                                      ],
                                      decoration: _buildInputDecoration('Price', 'Price'),
                                      validator: (val) {
                                        if (val == null || val.trim().isEmpty) {
                                          return 'Please enter price';
                                        }
                                        if (double.tryParse(val.trim()) == null) {
                                          return 'Invalid price';
                                        }
                                        return null;
                                      },
                                    ),
                                  ),
                                  const SizedBox(width: 16),
                                  Expanded(
                                    child: TextFormField(
                                      controller: _quantityController,
                                      keyboardType: TextInputType.number,
                                      inputFormatters: [
                                        FilteringTextInputFormatter.digitsOnly,
                                      ],
                                      decoration: _buildInputDecoration('Quantity', 'Quantity'),
                                      validator: (val) {
                                        if (val == null || val.trim().isEmpty) {
                                          return 'Please enter quantity';
                                        }
                                        final qty = int.tryParse(val.trim());
                                        if (qty == null || qty <= 0) {
                                          return 'Must be > 0';
                                        }
                                        return null;
                                      },
                                    ),
                                  ),
                                  const SizedBox(width: 16),
                                  Expanded(
                                    child: TextFormField(
                                      controller: _currencyController,
                                      decoration: _buildInputDecoration('Currency', 'USD'),
                                      validator: (val) {
                                        if (val == null || val.trim().isEmpty) {
                                          return 'Please enter currency';
                                        }
                                        return null;
                                      },
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 16),
                              Align(
                                alignment: Alignment.centerRight,
                                child: ElevatedButton(
                                  onPressed: _addItem,
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: headerRed,
                                    foregroundColor: Colors.white,
                                    padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(defaultRadius),
                                    ),
                                    elevation: 0,
                                  ),
                                  child: const Text(
                                    'Add Item to Cart',
                                    style: TextStyle(
                                      fontSize: 13,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                              ),
                            ],
                          ),
                        ),
                        const SizedBox(height: 24),
                        Row(
                          children: [
                            Expanded(
                              child: TextField(
                                controller: _searchController,
                                decoration: InputDecoration(
                                  hintText: 'Search order by Item Name...',
                                  hintStyle: const TextStyle(color: Colors.black38, fontSize: 13),
                                  prefixIcon: const Icon(Icons.search, color: primaryRed, size: 20),
                                  suffixIcon: _searchController.text.isNotEmpty
                                      ? IconButton(
                                          icon: const Icon(Icons.clear, size: 18),
                                          onPressed: () {
                                            _searchController.clear();
                                          },
                                        )
                                      : null,
                                  isDense: true,
                                  contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                                  filled: true,
                                  fillColor: Colors.white,
                                  border: OutlineInputBorder(
                                    borderRadius: BorderRadius.circular(defaultRadius),
                                    borderSide: const BorderSide(color: Color(0xFFDDDDDD)),
                                  ),
                                  enabledBorder: OutlineInputBorder(
                                    borderRadius: BorderRadius.circular(defaultRadius),
                                    borderSide: const BorderSide(color: Color(0xFFDDDDDD)),
                                  ),
                                  focusedBorder: OutlineInputBorder(
                                    borderRadius: BorderRadius.circular(defaultRadius),
                                    borderSide: const BorderSide(color: primaryRed, width: 1.5),
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 16),
                        Container(
                          decoration: BoxDecoration(
                            border: Border.all(color: const Color(0xFFEEEEEE)),
                            borderRadius: BorderRadius.circular(defaultRadius),
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: [
                              Container(
                                decoration: const BoxDecoration(
                                  color: headerRed,
                                  borderRadius: BorderRadius.only(
                                    topLeft: Radius.circular(defaultRadius),
                                    topRight: Radius.circular(defaultRadius),
                                  ),
                                ),
                                padding: const EdgeInsets.symmetric(vertical: 10, horizontal: 12),
                                child: const Row(
                                  children: [
                                    SizedBox(
                                      width: 40,
                                      child: Text(
                                        'Id',
                                        style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 13),
                                      ),
                                    ),
                                    Expanded(
                                      flex: 2,
                                      child: Text(
                                        'Item',
                                        style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 13),
                                      ),
                                    ),
                                    Expanded(
                                      flex: 3,
                                      child: Text(
                                        'Item Name',
                                        style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 13),
                                      ),
                                    ),
                                    Expanded(
                                      flex: 2,
                                      child: Text(
                                        'Quantity',
                                        style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 13),
                                      ),
                                    ),
                                    Expanded(
                                      flex: 2,
                                      child: Text(
                                        'Price',
                                        style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 13),
                                      ),
                                    ),
                                    Expanded(
                                      flex: 2,
                                      child: Text(
                                        'Currency',
                                        style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 13),
                                      ),
                                    ),
                                    SizedBox(
                                      width: 44,
                                      child: Text(
                                        '',
                                        style: TextStyle(color: Colors.white),
                                      ),
                                    ),
                                  ],
                                ),
                              ),
                              if (_displayOrders.isEmpty)
                                const Padding(
                                  padding: EdgeInsets.symmetric(vertical: 36),
                                  child: Center(
                                    child: Text(
                                      'No orders found',
                                      style: TextStyle(color: Colors.black45, fontSize: 14),
                                    ),
                                  ),
                                )
                              else
                                ListView.separated(
                                  shrinkWrap: true,
                                  physics: const NeverScrollableScrollPhysics(),
                                  itemCount: _displayOrders.length,
                                  separatorBuilder: (ctx, idx) => const Divider(height: 1, color: Color(0xFFF0F0F0)),
                                  itemBuilder: (ctx, idx) {
                                    final order = _displayOrders[idx];
                                    return Container(
                                      color: Colors.white,
                                      padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 12),
                                      child: Row(
                                        children: [
                                          SizedBox(
                                            width: 40,
                                            child: Text(
                                              '${idx + 1}',
                                              style: const TextStyle(fontSize: 13, color: Colors.black87),
                                            ),
                                          ),
                                          Expanded(
                                            flex: 2,
                                            child: Text(
                                              order.item,
                                              style: const TextStyle(fontSize: 13, color: Colors.black87),
                                            ),
                                          ),
                                          Expanded(
                                            flex: 3,
                                            child: Text(
                                              order.itemName,
                                              style: const TextStyle(fontSize: 13, color: Colors.black87),
                                            ),
                                          ),
                                          Expanded(
                                            flex: 2,
                                            child: Text(
                                              '${order.quantity}',
                                              style: const TextStyle(fontSize: 13, color: Colors.black87),
                                            ),
                                          ),
                                          Expanded(
                                            flex: 2,
                                            child: Text(
                                              _formatPrice(order.price),
                                              style: const TextStyle(fontSize: 13, color: Colors.black87),
                                            ),
                                          ),
                                          Expanded(
                                            flex: 2,
                                            child: Text(
                                              order.currency,
                                              style: const TextStyle(fontSize: 13, color: Colors.black87),
                                            ),
                                          ),
                                          SizedBox(
                                            width: 44,
                                            child: IconButton(
                                              icon: const Icon(
                                                Icons.delete_outline,
                                                color: Color(0xFF7FA2BE),
                                                size: 20,
                                              ),
                                              tooltip: 'Delete item',
                                              onPressed: () => _deleteItem(order),
                                            ),
                                          ),
                                        ],
                                      ),
                                    );
                                  },
                                ),
                            ],
                          ),
                        ),
                        const SizedBox(height: 32),
                        Container(
                          padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
                          decoration: BoxDecoration(
                            color: headerRed,
                            borderRadius: BorderRadius.circular(defaultRadius),
                          ),
                          child: const Text(
                            'Số 8, Tôn Thất Thuyết, Cầu Giấy, Hà Nội',
                            textAlign: TextAlign.left,
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 13,
                              fontWeight: FontWeight.w600,
                            ),
                          ),
                        ),
                        const SizedBox(height: 20),
                      ],
                    ),
                  ),
                ),
              ),
      ),
    );
  }
}

