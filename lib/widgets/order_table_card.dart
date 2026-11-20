import 'package:flutter/material.dart';

import '../models/order.dart';
import '../utils/price_formatter.dart';
import 'order_search_bar.dart';

/// Represents a displayed order row while preserving its exact index in `orders`.
class IndexedOrder {
  final int originalIndex;
  final Order order;

  const IndexedOrder({
    required this.originalIndex,
    required this.order,
  });
}

/// Clean, minimalist Order List card combining the search bar and bordered table.
/// Columns: Id | Item | Item Name | Quantity | Price | Currency | Action (Delete)
class OrderTableCard extends StatefulWidget {
  final List<IndexedOrder> items;
  final String searchQuery;
  final TextEditingController searchController;
  final ValueChanged<String> onSearchChanged;
  final VoidCallback onSearchClear;
  final void Function(int originalIndex, Order order) onDelete;
  final VoidCallback onExportJson;

  const OrderTableCard({
    super.key,
    required this.items,
    required this.searchQuery,
    required this.searchController,
    required this.onSearchChanged,
    required this.onSearchClear,
    required this.onDelete,
    required this.onExportJson,
  });

  @override
  State<OrderTableCard> createState() => _OrderTableCardState();
}

class _OrderTableCardState extends State<OrderTableCard> {
  static const Color _orangeStart = Color(0xFFEA580C);
  static const Color _orangeEnd = Color(0xFFF97316);
  static const Color _borderColor = Color(0xFFE2E8F0);
  static const double _minTableWidth = 680;

  final ScrollController _horizontalScrollController = ScrollController();

  @override
  void dispose() {
    _horizontalScrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final bool isCompact = MediaQuery.sizeOf(context).width < 560;

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: _borderColor),
        boxShadow: const <BoxShadow>[
          BoxShadow(
            color: Color(0x0A0F172A),
            blurRadius: 20,
            offset: Offset(0, 6),
          ),
        ],
      ),
      clipBehavior: Clip.antiAlias,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          // Search Bar Toolbar above the table
          Padding(
            padding: EdgeInsets.all(isCompact ? 14 : 18),
            child: Row(
              children: <Widget>[
                Expanded(
                  child: OrderSearchBar(
                    controller: widget.searchController,
                    onChanged: widget.onSearchChanged,
                    onClear: widget.onSearchClear,
                  ),
                ),
                const SizedBox(width: 10),
                Tooltip(
                  message: 'Download order.json',
                  child: InkWell(
                    key: const Key('export_json_button'),
                    onTap: widget.onExportJson,
                    borderRadius: BorderRadius.circular(12),
                    child: Container(
                      height: 44,
                      width: 44,
                      decoration: BoxDecoration(
                        color: const Color(0xFFFFF7ED),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: const Color(0xFFFED7AA)),
                      ),
                      child: const Icon(
                        Icons.download_rounded,
                        size: 20,
                        color: Color(0xFFEA580C),
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),

          // Bordered Responsive Table
          LayoutBuilder(
            builder: (BuildContext context, BoxConstraints constraints) {
              final bool needsScroll = constraints.maxWidth < _minTableWidth;
              final double tableWidth =
                  needsScroll ? _minTableWidth : constraints.maxWidth;

              return Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: <Widget>[
                  if (needsScroll)
                    const Padding(
                      padding: EdgeInsets.fromLTRB(16, 0, 16, 8),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.end,
                        children: <Widget>[
                          Icon(
                            Icons.swipe_rounded,
                            size: 14,
                            color: Color(0xFF94A3B8),
                          ),
                          SizedBox(width: 4),
                          Flexible(
                            child: Text(
                              'Swipe horizontally to view all columns',
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: TextStyle(
                                fontSize: 12,
                                color: Color(0xFF94A3B8),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  Container(
                    decoration: const BoxDecoration(
                      border: Border(
                        top: BorderSide(color: _borderColor),
                      ),
                    ),
                    child: Scrollbar(
                      controller: _horizontalScrollController,
                      thumbVisibility: needsScroll,
                      child: SingleChildScrollView(
                        controller: _horizontalScrollController,
                        scrollDirection: Axis.horizontal,
                        child: SizedBox(
                          width: tableWidth,
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.stretch,
                            children: <Widget>[
                              _buildHeaderRow(),
                              if (widget.items.isEmpty)
                                _buildEmptyState()
                              else
                                ...List<Widget>.generate(
                                  widget.items.length,
                                  (int displayIndex) {
                                    final IndexedOrder entry =
                                        widget.items[displayIndex];
                                    return _OrderTableRow(
                                      key: ValueKey<String>(
                                        'order_row_${entry.originalIndex}_${entry.order.Item}',
                                      ),
                                      displayId: displayIndex + 1,
                                      order: entry.order,
                                      isStriped: displayIndex.isOdd,
                                      isLast: displayIndex ==
                                          widget.items.length - 1,
                                      onDelete: () => widget.onDelete(
                                        entry.originalIndex,
                                        entry.order,
                                      ),
                                    );
                                  },
                                ),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ),
                ],
              );
            },
          ),
        ],
      ),
    );
  }

  Widget _buildHeaderRow() {
    return Container(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          colors: <Color>[_orangeStart, _orangeEnd],
          begin: Alignment.centerLeft,
          end: Alignment.centerRight,
        ),
      ),
      child: IntrinsicHeight(
        child: Row(
          children: <Widget>[
            _buildHeaderCell('Id', width: 64, alignment: Alignment.center),
            _buildHeaderDivider(),
            _buildHeaderCell('Item', width: 118, alignment: Alignment.centerLeft),
            _buildHeaderDivider(),
            Expanded(
              child: _buildHeaderCell(
                'Item Name',
                alignment: Alignment.centerLeft,
              ),
            ),
            _buildHeaderDivider(),
            _buildHeaderCell('Quantity', width: 96, alignment: Alignment.center),
            _buildHeaderDivider(),
            _buildHeaderCell('Price', width: 124, alignment: Alignment.centerRight),
            _buildHeaderDivider(),
            _buildHeaderCell('Currency', width: 100, alignment: Alignment.center),
            _buildHeaderDivider(),
            _buildHeaderCell('Action', width: 78, alignment: Alignment.center),
          ],
        ),
      ),
    );
  }

  Widget _buildHeaderCell(
    String text, {
    double? width,
    required Alignment alignment,
  }) {
    return Container(
      width: width,
      alignment: alignment,
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
      child: Text(
        text,
        maxLines: 1,
        overflow: TextOverflow.ellipsis,
        style: const TextStyle(
          color: Colors.white,
          fontSize: 13.5,
          fontWeight: FontWeight.w700,
          letterSpacing: 0.25,
        ),
      ),
    );
  }

  Widget _buildHeaderDivider() {
    return Container(
      width: 1,
      color: Colors.white.withValues(alpha: 0.22),
    );
  }

  Widget _buildEmptyState() {
    final bool isSearching = widget.searchQuery.trim().isNotEmpty;
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 44),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: <Widget>[
          Icon(
            isSearching
                ? Icons.search_off_rounded
                : Icons.shopping_bag_outlined,
            size: 36,
            color: const Color(0xFFCBD5E1),
          ),
          const SizedBox(height: 10),
          Text(
            isSearching
                ? 'No items found for "${widget.searchQuery.trim()}"'
                : 'No orders yet',
            style: const TextStyle(
              fontSize: 15,
              fontWeight: FontWeight.w600,
              color: Color(0xFF334155),
            ),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 4),
          Text(
            isSearching
                ? 'Clear the search box to show all orders.'
                : 'Use the form above to add a new order.',
            style: const TextStyle(
              fontSize: 13,
              color: Color(0xFF94A3B8),
            ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }
}

class _OrderTableRow extends StatefulWidget {
  final int displayId;
  final Order order;
  final bool isStriped;
  final bool isLast;
  final VoidCallback onDelete;

  const _OrderTableRow({
    super.key,
    required this.displayId,
    required this.order,
    required this.isStriped,
    required this.isLast,
    required this.onDelete,
  });

  @override
  State<_OrderTableRow> createState() => _OrderTableRowState();
}

class _OrderTableRowState extends State<_OrderTableRow> {
  static const Color _borderColor = Color(0xFFF1F5F9);
  bool _isHovered = false;
  bool _isDeleteHovered = false;

  @override
  Widget build(BuildContext context) {
    final Color background = _isHovered
        ? const Color(0xFFFFF7ED)
        : (widget.isStriped ? const Color(0xFFFCFCFD) : Colors.white);

    return MouseRegion(
      onEnter: (_) => setState(() => _isHovered = true),
      onExit: (_) => setState(() => _isHovered = false),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 140),
        decoration: BoxDecoration(
          color: background,
          border: widget.isLast
              ? null
              : const Border(
                  bottom: BorderSide(color: Color(0xFFE2E8F0)),
                ),
        ),
        child: IntrinsicHeight(
          child: Row(
            children: <Widget>[
              // Id
              _buildCell(
                width: 64,
                alignment: Alignment.center,
                child: Text(
                  '${widget.displayId}',
                  style: const TextStyle(
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                    color: Color(0xFF64748B),
                    fontFeatures: <FontFeature>[FontFeature.tabularFigures()],
                  ),
                ),
              ),
              _buildCellDivider(),

              // Item
              _buildCell(
                width: 118,
                alignment: Alignment.centerLeft,
                child: Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 9, vertical: 4),
                  decoration: BoxDecoration(
                    color: const Color(0xFFF1F5F9),
                    borderRadius: BorderRadius.circular(7),
                  ),
                  child: Text(
                    widget.order.Item,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 13.5,
                      fontWeight: FontWeight.w700,
                      color: Color(0xFF334155),
                    ),
                  ),
                ),
              ),
              _buildCellDivider(),

              // Item Name
              Expanded(
                child: _buildCell(
                  alignment: Alignment.centerLeft,
                  child: Text(
                    widget.order.ItemName,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.w600,
                      color: Color(0xFF0F172A),
                    ),
                  ),
                ),
              ),
              _buildCellDivider(),

              // Quantity
              _buildCell(
                width: 96,
                alignment: Alignment.center,
                child: Text(
                  '${widget.order.Quantity}',
                  style: const TextStyle(
                    fontSize: 14.5,
                    fontWeight: FontWeight.w600,
                    color: Color(0xFF334155),
                    fontFeatures: <FontFeature>[FontFeature.tabularFigures()],
                  ),
                ),
              ),
              _buildCellDivider(),

              // Price
              _buildCell(
                width: 124,
                alignment: Alignment.centerRight,
                child: Text(
                  formatPrice(widget.order.Price),
                  style: const TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w700,
                    color: Color(0xFF0F172A),
                    fontFeatures: <FontFeature>[FontFeature.tabularFigures()],
                  ),
                ),
              ),
              _buildCellDivider(),

              // Currency
              _buildCell(
                width: 100,
                alignment: Alignment.center,
                child: Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: const Color(0xFFFFF7ED),
                    borderRadius: BorderRadius.circular(99),
                    border: Border.all(color: const Color(0xFFFED7AA)),
                  ),
                  child: Text(
                    widget.order.Currency,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.w700,
                      color: Color(0xFFEA580C),
                    ),
                  ),
                ),
              ),
              _buildCellDivider(),

              // Delete action
              _buildCell(
                width: 78,
                alignment: Alignment.center,
                child: MouseRegion(
                  onEnter: (_) => setState(() => _isDeleteHovered = true),
                  onExit: (_) => setState(() => _isDeleteHovered = false),
                  child: Tooltip(
                    message: 'Delete ${widget.order.ItemName}',
                    child: InkWell(
                      key: Key('delete_order_${widget.order.Item}'),
                      borderRadius: BorderRadius.circular(10),
                      onTap: widget.onDelete,
                      child: AnimatedContainer(
                        duration: const Duration(milliseconds: 140),
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: _isDeleteHovered
                              ? const Color(0xFFFEE2E2)
                              : Colors.transparent,
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: Icon(
                          Icons.delete_outline_rounded,
                          size: 20,
                          color: _isDeleteHovered
                              ? const Color(0xFFDC2626)
                              : const Color(0xFF94A3B8),
                        ),
                      ),
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

  Widget _buildCell({
    double? width,
    required Alignment alignment,
    required Widget child,
  }) {
    return Container(
      width: width,
      alignment: alignment,
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 15),
      child: child,
    );
  }

  Widget _buildCellDivider() {
    return Container(
      width: 1,
      color: _borderColor,
    );
  }
}
