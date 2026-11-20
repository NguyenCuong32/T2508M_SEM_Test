import 'package:flutter/services.dart';
import 'package:material_ui/material_ui.dart';

import 'package:my_order/core/constants/app_constants.dart';

import '../../data/models/order_model.dart';

class OrderForm extends StatefulWidget {
  const OrderForm({super.key, required this.existingItems});

  final Set<String> existingItems;

  @override
  State<OrderForm> createState() => _OrderFormState();
}

class _OrderFormState extends State<OrderForm> {
  final _form = GlobalKey<FormState>();
  final _item = TextEditingController();
  final _name = TextEditingController();
  final _price = TextEditingController();
  final _quantity = TextEditingController(text: '1');
  String _currency = 'USD';

  @override
  void dispose() {
    for (final controller in [_item, _name, _price, _quantity]) {
      controller.dispose();
    }
    super.dispose();
  }

  void _submit() {
    if (!_form.currentState!.validate()) {
      return;
    }
    FocusScope.of(context).unfocus();
    Navigator.pop(
      context,
      Order(
        item: _item.text.trim().toUpperCase(),
        itemName: _name.text.trim(),
        price: double.parse(_price.text.trim().replaceAll(',', '.')),
        currency: _currency,
        quantity: int.parse(_quantity.text.trim()),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.only(bottom: MediaQuery.viewInsetsOf(context).bottom),
      child: SingleChildScrollView(
        padding: const EdgeInsets.fromLTRB(24, 12, 24, 28),
        child: Form(
          key: _form,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            mainAxisSize: MainAxisSize.min,
            children: [
              Center(
                child: Container(
                  width: 36,
                  height: 4,
                  decoration: BoxDecoration(
                    color: AppConstants.outline,
                    borderRadius: BorderRadius.circular(4),
                  ),
                ),
              ),
              const SizedBox(height: 18),
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(10),
                    decoration: BoxDecoration(
                      color: AppConstants.tint,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: const Icon(
                      Icons.playlist_add,
                      color: AppConstants.accent,
                    ),
                  ),
                  const SizedBox(width: 12),
                  const Expanded(
                    child: Text(
                      'Thêm đơn hàng',
                      style: TextStyle(
                        fontSize: 25,
                        fontWeight: FontWeight.w800,
                        letterSpacing: -0.5,
                      ),
                    ),
                  ),
                  IconButton(
                    tooltip: 'Đóng',
                    onPressed: () => Navigator.pop(context),
                    icon: const Icon(Icons.close),
                  ),
                ],
              ),
              const SizedBox(height: 24),
              TextFormField(
                key: const Key('item'),
                controller: _item,
                textCapitalization: TextCapitalization.characters,
                textInputAction: TextInputAction.next,
                maxLength: 30,
                decoration: const InputDecoration(
                  labelText: 'Mã sản phẩm',
                  hintText: 'A1002',
                  counterText: '',
                ),
                validator: (value) {
                  final item = (value ?? '').trim().toUpperCase();
                  if (item.isEmpty) {
                    return 'Vui lòng nhập mã sản phẩm';
                  }
                  if (!RegExp(r'^[A-Z0-9_-]+$').hasMatch(item)) {
                    return 'Chỉ dùng chữ, số, dấu - hoặc _';
                  }
                  if (widget.existingItems.any(
                    (v) => v.toUpperCase() == item,
                  )) {
                    return 'Mã sản phẩm đã tồn tại';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),
              TextFormField(
                key: const Key('itemName'),
                controller: _name,
                textCapitalization: TextCapitalization.words,
                textInputAction: TextInputAction.next,
                maxLength: 100,
                decoration: const InputDecoration(
                  labelText: 'Tên sản phẩm',
                  hintText: 'Iphone 17',
                  counterText: '',
                ),
                validator: (value) => value == null || value.trim().isEmpty
                    ? 'Vui lòng nhập tên sản phẩm'
                    : null,
              ),
              const SizedBox(height: 16),
              TextFormField(
                key: const Key('price'),
                controller: _price,
                keyboardType: const TextInputType.numberWithOptions(
                  decimal: true,
                ),
                textInputAction: TextInputAction.next,
                maxLength: 15,
                decoration: const InputDecoration(
                  labelText: 'Đơn giá',
                  hintText: '1500',
                  counterText: '',
                ),
                validator: (value) {
                  final raw = (value ?? '').trim().replaceAll(',', '.');
                  final price = double.tryParse(raw);
                  if (!RegExp(r'^\d+(\.\d{1,2})?$').hasMatch(raw) ||
                      price == null ||
                      !price.isFinite ||
                      price <= 0 ||
                      price > 999999999999) {
                    return 'Nhập giá lớn hơn 0, tối đa 2 số thập phân';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    child: TextFormField(
                      key: const Key('quantity'),
                      controller: _quantity,
                      keyboardType: TextInputType.number,
                      inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                      maxLength: 6,
                      decoration: const InputDecoration(
                        labelText: 'Số lượng',
                        counterText: '',
                      ),
                      validator: (value) {
                        final quantity = int.tryParse((value ?? '').trim());
                        return quantity == null || quantity <= 0
                            ? 'Nhập số nguyên > 0'
                            : null;
                      },
                    ),
                  ),
                  const SizedBox(width: 14),
                  Expanded(
                    child: DropdownButtonFormField<String>(
                      initialValue: _currency,
                      decoration: const InputDecoration(labelText: 'Tiền tệ'),
                      items: ['USD', 'VND', 'EUR']
                          .map(
                            (v) => DropdownMenuItem(value: v, child: Text(v)),
                          )
                          .toList(),
                      onChanged: (value) => setState(() => _currency = value!),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 24),
              FilledButton.icon(
                key: const Key('saveOrder'),
                onPressed: _submit,
                icon: const Icon(Icons.add),
                label: const Text('Lưu đơn hàng'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
