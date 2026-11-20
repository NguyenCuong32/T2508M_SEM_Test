import 'dart:convert';
import 'dart:html';
import 'order.dart';

// Chuỗi JSON mẫu theo đúng đề bài
const String initialJsonString =
    '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity":1},'
    '{"Item": "A1001","ItemName": "Iphone 16","Price": 1500,"Currency": "USD","Quantity":1}]';

// Danh sách lưu trữ các đối tượng Order trong bộ nhớ
List<Order> orders = [];

// Biến lưu trữ đơn hàng đang được chỉnh sửa (null nếu đang ở chế độ thêm mới)
Order? editingOrder;

// Các phần tử DOM HTML
late final InputElement itemInput;
late final InputElement itemNameInput;
late final InputElement priceInput;
late final InputElement quantityInput;
late final InputElement currencyInput;
late final ButtonElement btnAddItem;
late final ButtonElement btnCancelEdit;
late final Element formCardTitle;
late final Element formCardSubtitle;
late final Element btnText;
late final InputElement searchItemInput;
late final TableSectionElement orderTableBody;

void main() {
  // 1. Ánh xạ các phần tử DOM
  itemInput = querySelector('#itemInput') as InputElement;
  itemNameInput = querySelector('#itemNameInput') as InputElement;
  priceInput = querySelector('#priceInput') as InputElement;
  quantityInput = querySelector('#quantityInput') as InputElement;
  currencyInput = querySelector('#currencyInput') as InputElement;
  btnAddItem = querySelector('#btnAddItem') as ButtonElement;
  btnCancelEdit = querySelector('#btnCancelEdit') as ButtonElement;
  formCardTitle = querySelector('#formCardTitle') as Element;
  formCardSubtitle = querySelector('#formCardSubtitle') as Element;
  btnText = querySelector('#btnText') as Element;
  searchItemInput = querySelector('#searchItemInput') as InputElement;
  orderTableBody = querySelector('#orderTableBody') as TableSectionElement;

  // 2. Đọc dữ liệu từ chuỗi JSON và chuyển thành danh sách các đối tượng Order
  loadOrdersFromJson();

  // 3. Hiển thị danh sách ban đầu lên màn hình
  renderOrders(orders);

  // 4. Lắng nghe sự kiện thêm mới hoặc cập nhật Order
  btnAddItem.onClick.listen((_) => handleAddOrUpdateOrder());

  // 5. Lắng nghe sự kiện hủy chỉnh sửa
  btnCancelEdit.onClick.listen((_) => cancelEdit());

  // 6. Lắng nghe sự kiện tìm kiếm theo ItemName
  searchItemInput.onInput.listen((_) => handleSearch());
}

/// Đọc nội dung JSON và lưu vào danh sách orders
void loadOrdersFromJson() {
  try {
    // Ưu tiên đọc từ localStorage nếu trước đó đã có lưu dữ liệu
    final savedData = window.localStorage['order.json'];
    final sourceJson = (savedData != null && savedData.isNotEmpty)
        ? savedData
        : initialJsonString;

    final List<dynamic> decodedList = jsonDecode(sourceJson);
    orders = decodedList
        .map((item) => Order.fromJson(item as Map<String, dynamic>))
        .toList();

    // Đồng bộ vào localStorage giả lập file order.json
    saveOrdersToJson();
  } catch (e) {
    print('Lỗi khi đọc chuỗi JSON: $e');
    orders = [];
  }
}

/// Lưu danh sách order hiện tại vào json (giả lập order.json bằng localStorage)
void saveOrdersToJson() {
  final jsonString = jsonEncode(orders.map((o) => o.toJson()).toList());
  window.localStorage['order.json'] = jsonString;
  print('Đã cập nhật order.json:\n$jsonString');
}

/// Render danh sách Order ra bảng HTML
void renderOrders(List<Order> listToRender) {
  orderTableBody.children.clear();

  if (listToRender.isEmpty) {
    final emptyRow = TableRowElement();
    final emptyCell = TableCellElement()
      ..colSpan = 7
      ..className = 'empty-message'
      ..text = 'No orders found.';
    emptyRow.children.add(emptyCell);
    orderTableBody.children.add(emptyRow);
    return;
  }

  for (var i = 0; i < listToRender.length; i++) {
    final order = listToRender[i];
    final row = TableRowElement();

    // Highlight dòng đang được chỉnh sửa
    if (editingOrder != null && editingOrder!.item == order.item) {
      row.style.backgroundColor = 'var(--primary-light)';
    }

    // Cột Id
    row.children.add(TableCellElement()
      ..style.color = 'var(--text-muted)'
      ..style.fontWeight = '600'
      ..text = '${i + 1}');

    // Cột Item (Mã SP - badge)
    row.children.add(TableCellElement()
      ..innerHtml = '<span class="badge-code">${order.item}</span>');

    // Cột Item Name
    row.children.add(TableCellElement()
      ..style.fontWeight = '600'
      ..text = order.itemName);

    // Cột Quantity
    row.children.add(TableCellElement()
      ..style.textAlign = 'center'
      ..text = '${order.quantity}');

    // Cột Price
    row.children.add(TableCellElement()
      ..style.textAlign = 'right'
      ..innerHtml = '<span class="price-text">${formatPrice(order.price)}</span>');

    // Cột Currency
    row.children.add(TableCellElement()
      ..style.textAlign = 'center'
      ..innerHtml = '<span class="badge-currency">${order.currency}</span>');

    // Cột Actions (Nút Sửa & Nút Xóa có icon và nhãn)
    final actionCell = TableCellElement()..style.textAlign = 'center';
    final actionButtons = DivElement()..className = 'action-buttons';

    final validator = NodeValidatorBuilder.common()..allowSvg();

    // Nút Sửa (Edit button có icon + text)
    final editBtn = ButtonElement()
      ..className = 'btn-action btn-edit'
      ..title = 'Edit this order'
      ..setInnerHtml('''
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
          <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
        </svg>
        <span>Edit</span>
      ''', validator: validator);

    editBtn.onClick.listen((_) {
      startEditOrder(order);
    });

    // Nút Xóa (Delete button có icon + text)
    final deleteBtn = ButtonElement()
      ..className = 'btn-action btn-delete'
      ..title = 'Delete this order'
      ..setInnerHtml('''
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="3 6 5 6 21 6"></polyline>
          <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
        </svg>
        <span>Delete</span>
      ''', validator: validator);

    deleteBtn.onClick.listen((_) {
      deleteOrder(order);
    });

    actionButtons.children.add(editBtn);
    actionButtons.children.add(deleteBtn);
    actionCell.children.add(actionButtons);
    row.children.add(actionCell);

    orderTableBody.children.add(row);
  }
}

/// Bắt đầu chế độ chỉnh sửa sản phẩm
void startEditOrder(Order order) {
  editingOrder = order;

  // Điền thông tin sản phẩm vào form
  itemInput.value = order.item;
  itemInput.disabled = true; // Khóa mã sản phẩm để tránh sửa nhầm mã chính
  itemNameInput.value = order.itemName;
  priceInput.value = order.price.toString();
  quantityInput.value = order.quantity.toString();
  currencyInput.value = order.currency;

  // Cập nhật giao diện form sang Edit Mode
  formCardTitle.text = 'Edit Order Item: ${order.item}';
  formCardSubtitle.text = 'Update details for this product and save changes';
  btnText.text = 'Save Changes';
  btnCancelEdit.style.display = 'inline-flex';

  // Render lại bảng để highlight dòng đang sửa
  handleSearch();

  // Focus vào trường tên sản phẩm
  itemNameInput.focus();
}

/// Hủy chế độ chỉnh sửa, quay về chế độ thêm mới
void cancelEdit() {
  editingOrder = null;

  // Reset form nhập liệu
  itemInput.value = '';
  itemInput.disabled = false;
  itemNameInput.value = '';
  priceInput.value = '';
  quantityInput.value = '1';
  currencyInput.value = 'USD';

  // Khôi phục giao diện form
  formCardTitle.text = 'Add New Order Item';
  formCardSubtitle.text = 'Fill in details to add item into order list';
  btnText.text = 'Add Item to Cart';
  btnCancelEdit.style.display = 'none';

  // Render lại bảng
  handleSearch();
}

/// Xử lý Thêm mới hoặc Cập nhật Order
void handleAddOrUpdateOrder() {
  final item = itemInput.value?.trim() ?? '';
  final itemName = itemNameInput.value?.trim() ?? '';
  final priceText = priceInput.value?.trim() ?? '';
  final quantityText = quantityInput.value?.trim() ?? '';
  final currency = currencyInput.value?.trim() ?? 'USD';

  // Kiểm tra tính hợp lệ của dữ liệu nhập
  if (item.isEmpty || itemName.isEmpty || priceText.isEmpty || quantityText.isEmpty) {
    window.alert('Vui lòng điền đầy đủ tất cả các trường thông tin!');
    return;
  }

  final price = num.tryParse(priceText);
  if (price == null || price <= 0) {
    window.alert('Vui lòng nhập Price là số hợp lệ (> 0)!');
    return;
  }

  final quantity = int.tryParse(quantityText);
  if (quantity == null || quantity <= 0) {
    window.alert('Vui lòng nhập Quantity là số nguyên dương (> 0)!');
    return;
  }

  if (editingOrder != null) {
    // === CHẾ ĐỘ CẬP NHẬT (UPDATE) ===
    editingOrder!.itemName = itemName;
    editingOrder!.price = price;
    editingOrder!.quantity = quantity;
    editingOrder!.currency = currency.isEmpty ? 'USD' : currency;

    saveOrdersToJson();
    final updatedItemName = editingOrder!.itemName;
    cancelEdit();

    window.alert('Đã cập nhật sản phẩm "$updatedItemName" thành công!');
  } else {
    // === CHẾ ĐỘ THÊM MỚI (INSERT) ===
    // Kiểm tra trùng mã Item
    final exists = orders.any((o) => o.item.toLowerCase() == item.toLowerCase());
    if (exists) {
      window.alert('Mã Item "$item" đã tồn tại trong danh sách! Vui lòng chọn mã khác hoặc bấm Sửa.');
      return;
    }

    final newOrder = Order(
      item: item,
      itemName: itemName,
      price: price,
      currency: currency.isEmpty ? 'USD' : currency,
      quantity: quantity,
    );

    orders.add(newOrder);
    saveOrdersToJson();

    // Reset form
    itemInput.value = '';
    itemNameInput.value = '';
    priceInput.value = '';
    quantityInput.value = '1';

    handleSearch();
    window.alert('Đã thêm Order mới thành công!');
  }
}

/// Xử lý tìm kiếm Order theo ItemName
void handleSearch() {
  final keyword = searchItemInput.value?.trim().toLowerCase() ?? '';

  if (keyword.isEmpty) {
    renderOrders(orders);
  } else {
    final filtered = orders.where((order) {
      return order.itemName.toLowerCase().contains(keyword);
    }).toList();
    renderOrders(filtered);
  }
}

/// Xóa một đơn hàng
void deleteOrder(Order order) {
  final confirmed = window.confirm('Bạn có chắc chắn muốn xóa đơn hàng "${order.itemName}" (${order.item})?');
  if (!confirmed) return;

  if (editingOrder != null && editingOrder!.item == order.item) {
    cancelEdit();
  }

  orders.remove(order);
  saveOrdersToJson();
  handleSearch();
}

/// Định dạng giá tiền với dấu chấm phân cách hàng nghìn (ví dụ 1200 -> 1.200)
String formatPrice(num price) {
  final priceInt = price.toInt();
  if (price == priceInt) {
    return priceInt.toString().replaceAllMapped(
      RegExp(r'(\d{1,3})(?=(\d{3})+(?!\d))'),
      (Match m) => '${m[1]}.',
    );
  }
  return price.toString();
}
