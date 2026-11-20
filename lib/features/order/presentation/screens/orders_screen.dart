import 'package:material_ui/material_ui.dart';

import 'package:my_order/core/constants/app_constants.dart';
import 'package:my_order/core/utils/formatters.dart';

import '../../data/models/order_model.dart';
import '../controllers/order_controller.dart';
import '../widgets/order_form.dart';

const ink = AppConstants.ink;
const muted = AppConstants.muted;
const accent = AppConstants.accent;

class OrdersScreen extends StatefulWidget {
  const OrdersScreen({super.key, required this.createController});

  final OrderController Function() createController;

  @override
  State<OrdersScreen> createState() => _OrdersScreenState();
}

class _OrdersScreenState extends State<OrdersScreen> {
  final _search = TextEditingController();
  late final OrderController _controller;

  @override
  void initState() {
    super.initState();
    _controller = widget.createController()..load();
  }

  @override
  void dispose() {
    _search.dispose();
    _controller.dispose();
    super.dispose();
  }

  void _clearSearch() {
    _search.clear();
    _controller.search('');
  }

  Future<void> _addOrder() async {
    FocusScope.of(context).unfocus();
    final order = await showModalBottomSheet<Order>(
      context: context,
      isScrollControlled: true,
      useSafeArea: true,
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(28)),
      ),
      builder: (context) => OrderForm(existingItems: _controller.existingItems),
    );
    if (order == null || !mounted) {
      return;
    }
    final saved = await _controller.addOrder(order);
    if (!mounted) return;
    if (saved) {
      _search.clear();
      _notify('Đã thêm ${order.itemName}');
    } else {
      _notify('Không thể lưu đơn hàng. Vui lòng thử lại.');
    }
  }

  void _notify(String message) {
    ScaffoldMessenger.of(context)
        .showSnackBar(SnackBar(content: Text(message)));
  }

  Future<void> _delete(Order order) async {
    FocusScope.of(context).unfocus();
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Xóa đơn hàng?'),
        content: Text('Bạn muốn xóa ${order.itemName} (${order.item})?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Hủy'),
          ),
          FilledButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Xóa'),
          ),
        ],
      ),
    );
    if (confirmed != true || !mounted) return;
    final saved = await _controller.deleteOrder(order);
    if (!mounted) return;
    _notify(
      saved
          ? 'Đã xóa ${order.itemName}'
          : 'Không thể lưu đơn hàng. Vui lòng thử lại.',
    );
  }

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(listenable: _controller, builder: _buildScreen);
  }

  Widget _buildScreen(BuildContext context, Widget? child) {
    final visible = _controller.visibleOrders;
    final quantities = _controller.quantities;
    final totals = _controller.totals;
    return Scaffold(
      appBar: AppBar(
        toolbarHeight: 68,
        titleSpacing: 24,
        title: FittedBox(
          fit: BoxFit.scaleDown,
          alignment: Alignment.centerLeft,
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: accent,
                  borderRadius: BorderRadius.circular(13),
                ),
                child: const Icon(
                  Icons.shopping_bag_outlined,
                  color: Colors.white,
                  size: 25,
                ),
              ),
              const SizedBox(width: 12),
              const Text(
                AppConstants.title,
                style: TextStyle(
                  color: ink,
                  fontSize: 24,
                  fontWeight: FontWeight.w800,
                  letterSpacing: -1,
                ),
              ),
            ],
          ),
        ),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 24),
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: const Color(0xFFEAF0E8),
                borderRadius: BorderRadius.circular(24),
              ),
              child: const Row(
                children: [
                  Icon(Icons.circle, size: 7, color: Color(0xFF5C7951)),
                  SizedBox(width: 6),
                  Text(
                    'Offline',
                    style: TextStyle(
                      color: Color(0xFF526B49),
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
      body: _controller.loading
          ? const Center(child: CircularProgressIndicator())
          : _controller.loadFailed
          ? Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.error_outline, size: 48, color: accent),
                    const SizedBox(height: 16),
                    const Text(
                      'Không thể đọc dữ liệu đơn hàng. Vui lòng thử lại.',
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 16),
                    FilledButton(
                      onPressed: _controller.load,
                      child: const Text('Thử lại'),
                    ),
                  ],
                ),
              ),
            )
          : RefreshIndicator(
              onRefresh: _controller.load,
              child: LayoutBuilder(
                builder: (context, constraints) {
                  final side = ((constraints.maxWidth - 720) / 2).clamp(
                    20.0,
                    double.infinity,
                  );
                  return ListView(
                    physics: const AlwaysScrollableScrollPhysics(),
                    padding: EdgeInsets.fromLTRB(side, 10, side, 32),
                    children: [
                      const Text(
                        'Đơn hàng của bạn',
                        style: TextStyle(
                          color: ink,
                          fontSize: 30,
                          fontWeight: FontWeight.w800,
                          letterSpacing: -1,
                        ),
                      ),
                      const SizedBox(height: 18),
                      Container(
                        padding: const EdgeInsets.all(24),
                        decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(24),
                          gradient: const LinearGradient(
                            colors: [Color(0xFF282057), Color(0xFF5345B0)],
                            begin: Alignment.topLeft,
                            end: Alignment.bottomRight,
                          ),
                          boxShadow: const [
                            BoxShadow(
                              color: Color(0x264C42B4),
                              blurRadius: 24,
                              offset: Offset(0, 9),
                            ),
                          ],
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              children: [
                                const Icon(
                                  Icons.receipt_long_outlined,
                                  size: 19,
                                  color: Color(0xFFDAD3FF),
                                ),
                                const SizedBox(width: 8),
                                const Expanded(
                                  child: Text(
                                    'Tổng giá trị đơn hàng',
                                    style: TextStyle(
                                      color: Color(0xFFE8E3FF),
                                      fontSize: 13,
                                    ),
                                  ),
                                ),
                                Text(
                                  '${_controller.orderCount} đơn',
                                  style: const TextStyle(
                                    color: Color(0xFFDAD3FF),
                                    fontSize: 12,
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 14),
                            if (totals.isEmpty)
                              const Text(
                                '0.00 USD',
                                style: TextStyle(
                                  color: Colors.white,
                                  fontSize: 30,
                                  fontWeight: FontWeight.w700,
                                ),
                              ),
                            for (final entry in totals.entries)
                              Padding(
                                padding: const EdgeInsets.only(bottom: 4),
                                child: Text(
                                  '${money(entry.value)} ${entry.key}',
                                  style: const TextStyle(
                                    color: Colors.white,
                                    fontSize: 30,
                                    fontWeight: FontWeight.w700,
                                    letterSpacing: -0.7,
                                  ),
                                ),
                              ),
                            const SizedBox(height: 8),
                            Text(
                              '$quantities sản phẩm',
                              style: const TextStyle(
                                color: Color(0xFFE8E3FF),
                                fontSize: 12,
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 28),
                      Row(
                        children: [
                          const Expanded(
                            child: Text(
                              'Danh sách đơn hàng',
                              style: TextStyle(
                                color: ink,
                                fontSize: 18,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                          ),
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 10,
                              vertical: 5,
                            ),
                            decoration: BoxDecoration(
                              color: AppConstants.tint,
                              borderRadius: BorderRadius.circular(20),
                            ),
                            child: Text(
                              '${visible.length}/${_controller.orderCount}',
                              style: const TextStyle(
                                color: accent,
                                fontSize: 12,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      TextField(
                        key: const Key('search'),
                        controller: _search,
                        onChanged: _controller.search,
                        decoration: InputDecoration(
                          hintText: 'Tìm theo tên sản phẩm...',
                          prefixIcon: const Icon(Icons.search, color: muted),
                          suffixIcon: !_controller.hasQuery
                              ? null
                              : IconButton(
                                  tooltip: 'Xóa tìm kiếm',
                                  onPressed: _clearSearch,
                                  icon: const Icon(Icons.close),
                                ),
                        ),
                      ),
                      const SizedBox(height: 14),
                      FilledButton.icon(
                        key: const Key('addOrder'),
                        onPressed: _controller.saving ? null : _addOrder,
                        icon: _controller.saving
                            ? const SizedBox(
                                width: 18,
                                height: 18,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                ),
                              )
                            : const Icon(Icons.add, size: 21),
                        label: Text(
                          _controller.saving ? 'Đang lưu...' : 'Thêm đơn hàng',
                        ),
                      ),
                      const SizedBox(height: 20),
                      if (visible.isEmpty)
                        Container(
                          padding: const EdgeInsets.symmetric(
                            vertical: 40,
                            horizontal: 20,
                          ),
                          child: Column(
                            children: [
                              const Icon(
                                Icons.inventory_2_outlined,
                                size: 48,
                                color: muted,
                              ),
                              const SizedBox(height: 14),
                              Text(
                                !_controller.hasQuery
                                    ? 'Chưa có đơn hàng'
                                    : 'Không tìm thấy đơn hàng',
                                style: const TextStyle(
                                  fontWeight: FontWeight.w700,
                                  color: ink,
                                ),
                              ),
                              const SizedBox(height: 8),
                              Text(
                                !_controller.hasQuery
                                    ? 'Thêm đơn hàng đầu tiên của bạn.'
                                    : 'Thử tìm bằng tên sản phẩm khác.',
                                style: const TextStyle(color: muted),
                              ),
                            ],
                          ),
                        ),
                      for (final order in visible)
                        Padding(
                          padding: const EdgeInsets.only(bottom: 12),
                          child: _OrderCard(
                            order: order,
                            onDelete: _controller.saving
                                ? null
                                : () => _delete(order),
                          ),
                        ),
                      const SizedBox(height: 16),
                      const Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(Icons.lock_outline, size: 13, color: muted),
                          SizedBox(width: 6),
                          Flexible(
                            child: Text(
                              'Dữ liệu của bạn được lưu cục bộ',
                              textAlign: TextAlign.center,
                              style: TextStyle(fontSize: 11, color: muted),
                            ),
                          ),
                        ],
                      ),
                    ],
                  );
                },
              ),
            ),
    );
  }
}

class _OrderCard extends StatelessWidget {
  const _OrderCard({required this.order, required this.onDelete});

  final Order order;
  final VoidCallback? onDelete;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: AppConstants.outline),
      ),
      child: Column(
        children: [
          Row(
            children: [
              Container(
                width: 48,
                height: 54,
                decoration: BoxDecoration(
                  color: AppConstants.tint,
                  borderRadius: BorderRadius.circular(13),
                ),
                child: const Icon(
                  Icons.inventory_2_outlined,
                  color: accent,
                  size: 27,
                ),
              ),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      order.itemName,
                      style: const TextStyle(
                        fontSize: 17,
                        fontWeight: FontWeight.w700,
                        color: ink,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      order.item,
                      style: const TextStyle(
                        color: muted,
                        fontSize: 12,
                        letterSpacing: 0.5,
                      ),
                    ),
                  ],
                ),
              ),
              IconButton(
                tooltip: 'Xóa ${order.itemName}',
                onPressed: onDelete,
                icon: const Icon(Icons.delete_outline, color: muted, size: 21),
              ),
            ],
          ),
          const Padding(
            padding: EdgeInsets.symmetric(vertical: 10),
            child: Divider(height: 1, color: AppConstants.outline),
          ),
          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text(
                      'ĐƠN GIÁ × SỐ LƯỢNG',
                      style: TextStyle(
                        color: muted,
                        fontSize: 9,
                        letterSpacing: 0.8,
                      ),
                    ),
                    const SizedBox(height: 5),
                    Text(
                      '${money(order.price)} ${order.currency} × ${order.quantity}',
                      style: const TextStyle(color: ink, fontSize: 12),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 8),
              Flexible(
                child: Text(
                  '${money(order.total)} ${order.currency}',
                  textAlign: TextAlign.right,
                  style: const TextStyle(
                    color: accent,
                    fontSize: 16,
                    fontWeight: FontWeight.w800,
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
