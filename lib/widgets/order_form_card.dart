import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../models/order.dart';

/// Clean, minimalist 2-column order creation form matching the "My Order" layout:
/// Row 1: Item (left)     | Item Name (right)
/// Row 2: Price (left)    | Quantity (right)
/// Row 3: Currency [USD]  | Add Item to Cart (orange button)
class OrderFormCard extends StatefulWidget {
  final Future<void> Function(Order newOrder) onAddOrder;
  final bool isSaving;

  const OrderFormCard({
    super.key,
    required this.onAddOrder,
    this.isSaving = false,
  });

  @override
  State<OrderFormCard> createState() => _OrderFormCardState();
}

class _OrderFormCardState extends State<OrderFormCard> {
  static const Color _orangeStart = Color(0xFFF97316);
  static const Color _orangeEnd = Color(0xFFEA580C);
  static const Color _orangeHover = Color(0xFFC2410C);
  static const Color _inkPrimary = Color(0xFF0F172A);
  static const Color _inkLabel = Color(0xFF334155);

  final GlobalKey<FormState> _formKey = GlobalKey<FormState>();
  final TextEditingController _itemController = TextEditingController();
  final TextEditingController _itemNameController = TextEditingController();
  final TextEditingController _priceController = TextEditingController();
  final TextEditingController _quantityController = TextEditingController();
  final TextEditingController _currencyController =
      TextEditingController(text: 'USD');

  final FocusNode _itemFocus = FocusNode();
  final FocusNode _itemNameFocus = FocusNode();
  final FocusNode _priceFocus = FocusNode();
  final FocusNode _quantityFocus = FocusNode();
  final FocusNode _currencyFocus = FocusNode();

  bool _autoValidate = false;
  bool _isButtonHovered = false;

  @override
  void dispose() {
    _itemController.dispose();
    _itemNameController.dispose();
    _priceController.dispose();
    _quantityController.dispose();
    _currencyController.dispose();
    _itemFocus.dispose();
    _itemNameFocus.dispose();
    _priceFocus.dispose();
    _quantityFocus.dispose();
    _currencyFocus.dispose();
    super.dispose();
  }

  Future<void> _handleSubmit() async {
    if (!_formKey.currentState!.validate()) {
      setState(() {
        _autoValidate = true;
      });
      return;
    }

    final Order order = Order(
      Item: _itemController.text.trim(),
      ItemName: _itemNameController.text.trim(),
      Price: double.parse(_priceController.text.trim().replaceAll(',', '')),
      Currency: _currencyController.text.trim().toUpperCase(),
      Quantity: int.parse(_quantityController.text.trim()),
    );

    await widget.onAddOrder(order);

    if (!mounted) return;
    _itemController.clear();
    _itemNameController.clear();
    _priceController.clear();
    _quantityController.clear();
    _currencyController.text = 'USD';
    setState(() {
      _autoValidate = false;
    });
    FocusScope.of(context).unfocus();
  }

  InputDecoration _fieldDecoration({required String hintText}) {
    return InputDecoration(
      hintText: hintText,
      hintStyle: const TextStyle(
        color: Color(0xFF94A3B8),
        fontSize: 14.5,
        fontWeight: FontWeight.w400,
      ),
      filled: true,
      fillColor: const Color(0xFFF8FAFC),
      hoverColor: Colors.white,
      isDense: true,
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 15),
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Color(0xFFE2E8F0)),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Color(0xFFE2E8F0)),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: _orangeStart, width: 1.8),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Color(0xFFEF4444), width: 1.3),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: const BorderSide(color: Color(0xFFEF4444), width: 1.8),
      ),
      errorStyle: const TextStyle(fontSize: 12, fontWeight: FontWeight.w500),
      errorMaxLines: 2,
    );
  }

  Widget _buildFieldBlock({
    required String label,
    required Widget child,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: <Widget>[
        SizedBox(
          height: 22,
          child: Text(
            label,
            style: const TextStyle(
              fontSize: 13.5,
              fontWeight: FontWeight.w600,
              color: _inkLabel,
            ),
          ),
        ),
        const SizedBox(height: 6),
        child,
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    final bool isCompact = MediaQuery.sizeOf(context).width < 560;
    final double gap = isCompact ? 12 : 20;

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: const Color(0xFFE2E8F0)),
        boxShadow: const <BoxShadow>[
          BoxShadow(
            color: Color(0x0A0F172A),
            blurRadius: 20,
            offset: Offset(0, 6),
          ),
        ],
      ),
      padding: EdgeInsets.all(isCompact ? 18 : 26),
      child: Form(
        key: _formKey,
        autovalidateMode: _autoValidate
            ? AutovalidateMode.onUserInteraction
            : AutovalidateMode.disabled,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: <Widget>[
            // Row 1: Item (left) | Item Name (right)
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Expanded(
                  child: _buildFieldBlock(
                    label: 'Item',
                    child: TextFormField(
                      key: const Key('item_field'),
                      controller: _itemController,
                      focusNode: _itemFocus,
                      style: const TextStyle(
                        fontSize: 15,
                        fontWeight: FontWeight.w500,
                        color: _inkPrimary,
                      ),
                      textInputAction: TextInputAction.next,
                      onFieldSubmitted: (_) => _itemNameFocus.requestFocus(),
                      decoration: _fieldDecoration(hintText: 'e.g. A1002'),
                      validator: (String? value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'Please enter Item code';
                        }
                        return null;
                      },
                    ),
                  ),
                ),
                SizedBox(width: gap),
                Expanded(
                  child: _buildFieldBlock(
                    label: 'Item Name',
                    child: TextFormField(
                      key: const Key('item_name_field'),
                      controller: _itemNameController,
                      focusNode: _itemNameFocus,
                      style: const TextStyle(
                        fontSize: 15,
                        fontWeight: FontWeight.w500,
                        color: _inkPrimary,
                      ),
                      textInputAction: TextInputAction.next,
                      onFieldSubmitted: (_) => _priceFocus.requestFocus(),
                      decoration: _fieldDecoration(hintText: 'e.g. Iphone 17'),
                      validator: (String? value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'Please enter Item Name';
                        }
                        return null;
                      },
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Row 2: Price (left) | Quantity (right)
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Expanded(
                  child: _buildFieldBlock(
                    label: 'Price',
                    child: TextFormField(
                      key: const Key('price_field'),
                      controller: _priceController,
                      focusNode: _priceFocus,
                      style: const TextStyle(
                        fontSize: 15,
                        fontWeight: FontWeight.w500,
                        color: _inkPrimary,
                      ),
                      keyboardType:
                          const TextInputType.numberWithOptions(decimal: true),
                      inputFormatters: <TextInputFormatter>[
                        FilteringTextInputFormatter.allow(RegExp(r'[0-9.,]')),
                      ],
                      textInputAction: TextInputAction.next,
                      onFieldSubmitted: (_) => _quantityFocus.requestFocus(),
                      decoration: _fieldDecoration(hintText: 'e.g. 1200'),
                      validator: (String? value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'Please enter Price';
                        }
                        final double? parsed =
                            double.tryParse(value.trim().replaceAll(',', ''));
                        if (parsed == null || parsed <= 0) {
                          return 'Price must be > 0';
                        }
                        return null;
                      },
                    ),
                  ),
                ),
                SizedBox(width: gap),
                Expanded(
                  child: _buildFieldBlock(
                    label: 'Quantity',
                    child: TextFormField(
                      key: const Key('quantity_field'),
                      controller: _quantityController,
                      focusNode: _quantityFocus,
                      style: const TextStyle(
                        fontSize: 15,
                        fontWeight: FontWeight.w500,
                        color: _inkPrimary,
                      ),
                      keyboardType: TextInputType.number,
                      inputFormatters: <TextInputFormatter>[
                        FilteringTextInputFormatter.digitsOnly,
                      ],
                      textInputAction: TextInputAction.next,
                      onFieldSubmitted: (_) => _currencyFocus.requestFocus(),
                      decoration: _fieldDecoration(hintText: 'e.g. 1'),
                      validator: (String? value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'Please enter Quantity';
                        }
                        final int? parsed = int.tryParse(value.trim());
                        if (parsed == null || parsed < 1) {
                          return 'Quantity must be >= 1';
                        }
                        return null;
                      },
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),

            // Row 3: Currency (USD default) (left) | Add Item to Cart (right)
            Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: <Widget>[
                Expanded(
                  child: _buildFieldBlock(
                    label: 'Currency',
                    child: TextFormField(
                      key: const Key('currency_field'),
                      controller: _currencyController,
                      focusNode: _currencyFocus,
                      style: const TextStyle(
                        fontSize: 15,
                        fontWeight: FontWeight.w600,
                        color: _inkPrimary,
                      ),
                      textCapitalization: TextCapitalization.characters,
                      textInputAction: TextInputAction.done,
                      onFieldSubmitted: (_) => _handleSubmit(),
                      decoration: _fieldDecoration(hintText: 'USD'),
                      validator: (String? value) {
                        if (value == null || value.trim().isEmpty) {
                          return 'Please enter Currency';
                        }
                        return null;
                      },
                    ),
                  ),
                ),
                SizedBox(width: gap),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: <Widget>[
                      const SizedBox(height: 28),
                      MouseRegion(
                        cursor: widget.isSaving
                            ? SystemMouseCursors.basic
                            : SystemMouseCursors.click,
                        onEnter: (_) => setState(() => _isButtonHovered = true),
                        onExit: (_) => setState(() => _isButtonHovered = false),
                        child: AnimatedContainer(
                          duration: const Duration(milliseconds: 160),
                          height: 50,
                          decoration: BoxDecoration(
                            gradient: LinearGradient(
                              colors: _isButtonHovered
                                  ? const <Color>[_orangeEnd, _orangeHover]
                                  : const <Color>[_orangeStart, _orangeEnd],
                              begin: Alignment.topLeft,
                              end: Alignment.bottomRight,
                            ),
                            borderRadius: BorderRadius.circular(12),
                            boxShadow: <BoxShadow>[
                              BoxShadow(
                                color: _orangeEnd.withValues(
                                  alpha: _isButtonHovered ? 0.36 : 0.22,
                                ),
                                blurRadius: _isButtonHovered ? 16 : 10,
                                offset: const Offset(0, 4),
                              ),
                            ],
                          ),
                          child: Material(
                            color: Colors.transparent,
                            child: InkWell(
                              key: const Key('add_item_button'),
                              borderRadius: BorderRadius.circular(12),
                              onTap: widget.isSaving ? null : _handleSubmit,
                              child: Padding(
                                padding:
                                    const EdgeInsets.symmetric(horizontal: 14),
                                child: Center(
                                  child: widget.isSaving
                                      ? const SizedBox(
                                          width: 20,
                                          height: 20,
                                          child: CircularProgressIndicator(
                                            strokeWidth: 2.2,
                                            color: Colors.white,
                                          ),
                                        )
                                      : const FittedBox(
                                          fit: BoxFit.scaleDown,
                                          child: Row(
                                            mainAxisAlignment:
                                                MainAxisAlignment.center,
                                            children: <Widget>[
                                              Icon(
                                                Icons.add_shopping_cart_rounded,
                                                size: 19,
                                                color: Colors.white,
                                              ),
                                              SizedBox(width: 8),
                                              Text(
                                                'Add Item to Cart',
                                                style: TextStyle(
                                                  fontSize: 15,
                                                  fontWeight: FontWeight.w700,
                                                  color: Colors.white,
                                                  letterSpacing: 0.2,
                                                ),
                                              ),
                                            ],
                                          ),
                                        ),
                                ),
                              ),
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
