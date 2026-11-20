// ignore_for_file: deprecated_member_use

import 'dart:convert';
import 'dart:html';
import '../lib/order.dart';
import '../lib/order_manager.dart';

/// Default JSON string specified in the SET01 Exam Paper (Page 2):
const defaultExamJson =
    '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": '
    '"USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": '
    '1500,"Currency": "USD","Quantity":1}]';

/// Full sample dataset matching Page 1 screenshot (A1000 - A1008):
const fullMockDataJson = '''[
  {"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity": 1},
  {"Item": "A1001","ItemName": "Iphone 16","Price": 1500,"Currency": "USD","Quantity": 1},
  {"Item": "A1002","ItemName": "Iphone 15 Pro","Price": 1200,"Currency": "USD","Quantity": 1},
  {"Item": "A1003","ItemName": "Iphone 15 Pro Max","Price": 1200,"Currency": "USD","Quantity": 1},
  {"Item": "A1004","ItemName": "Iphone 15 Plus","Price": 1200,"Currency": "USD","Quantity": 1},
  {"Item": "A1005","ItemName": "Iphone 14","Price": 1200,"Currency": "USD","Quantity": 1},
  {"Item": "A1006","ItemName": "Iphone 14 Pro","Price": 1200,"Currency": "USD","Quantity": 1},
  {"Item": "A1007","ItemName": "Iphone 13","Price": 1200,"Currency": "USD","Quantity": 1},
  {"Item": "A1008","ItemName": "Iphone 13 Pro","Price": 1200,"Currency": "USD","Quantity": 1}
]''';

const localStorageKey = 'fpt_aptech_orders_data';

final OrderManager manager = OrderManager();
String currentSearchFilter = '';
String? editingItemCode;

void main() {
  print('Initializing FPT Aptech Dart Web App with Edit feature & Validation...');

  // Setup DOM Event Listeners
  setupEventListeners();

  // Setup Real-time Input Validation Listeners
  setupValidationListeners();

  // Setup Numeric Input Restrictions (Price & Quantity chỉ được nhập số)
  setupNumericInputRestrictions();

  // Load initial data
  loadInitialData();
}

/// Attach all event handlers to DOM elements
void setupEventListeners() {
  // Order Form submission (Enter key or Submit button)
  final orderForm = querySelector('#order-form');
  orderForm?.onSubmit.listen((Event e) {
    e.preventDefault();
    if (editingItemCode != null) {
      handleUpdateOrder();
    } else {
      handleAddOrder();
    }
  });

  // Add Item to Cart button
  final btnAdd = querySelector('#btn-add-item');
  btnAdd?.onClick.listen((Event e) {
    e.preventDefault();
    handleAddOrder();
  });

  // Update Item button (Sửa đơn hàng)
  final btnUpdate = querySelector('#btn-update-item');
  btnUpdate?.onClick.listen((Event e) {
    e.preventDefault();
    handleUpdateOrder();
  });

  // Cancel edit button
  final btnCancel = querySelector('#btn-cancel-edit');
  btnCancel?.onClick.listen((_) => handleCancelEdit());

  // Search input and buttons
  final inputSearch = querySelector('#input-search') as InputElement?;
  inputSearch?.onInput.listen((_) {
    currentSearchFilter = inputSearch.value ?? '';
    renderTable();
  });

  final btnSearch = querySelector('#btn-search');
  btnSearch?.onClick.listen((_) {
    currentSearchFilter = inputSearch?.value ?? '';
    renderTable();
  });

  final btnClearSearch = querySelector('#btn-clear-search');
  btnClearSearch?.onClick.listen((_) {
    if (inputSearch != null) inputSearch.value = '';
    currentSearchFilter = '';
    renderTable();
  });

  // Export & Reload buttons
  final btnExport = querySelector('#btn-export-json');
  btnExport?.onClick.listen((_) => exportJsonFile());

  final btnReload = querySelector('#btn-reload-json');
  btnReload?.onClick.listen((_) => resetToDefaultData());

  final btnLoadMock = querySelector('#btn-load-mock');
  btnLoadMock?.onClick.listen((_) => loadFullMockData());

  final btnFillSample = querySelector('#btn-fill-sample');
  btnFillSample?.onClick.listen((_) => fillSampleInputForm());

  // Allow pressing Enter in form inputs to submit or update
  final formInputs = querySelectorAll('.order-form input');
  for (final element in formInputs) {
    if (element is InputElement) {
      element.onKeyPress.listen((KeyboardEvent e) {
        if (e.keyCode == KeyCode.ENTER) {
          e.preventDefault();
          if (editingItemCode != null) {
            handleUpdateOrder();
          } else {
            handleAddOrder();
          }
        }
      });
    }
  }
}

/// Load data with fallback hierarchy:
///   1. Server API GET /api/orders
///   2. Local file 'order.json' via HttpRequest
///   3. Browser localStorage
///   4. Default exam paper string
Future<void> loadInitialData() async {
  // 1. Try Server API
  try {
    final response = await HttpRequest.getString('/api/orders');
    if (response.trim().isNotEmpty && response != '[]') {
      manager.loadFromJsonString(response);
      saveToLocalStorage();
      renderTable();
      showAlert('Loaded orders from server (order.json).', type: 'info');
      return;
    }
  } catch (_) {
    // Server not running or not accessible, proceed to next fallback
  }

  // 2. Try localStorage
  final localData = window.localStorage[localStorageKey];
  if (localData != null && localData.trim().isNotEmpty) {
    try {
      manager.loadFromJsonString(localData);
      renderTable();
      return;
    } catch (_) {}
  }

  // 3. Try fetching static order.json
  try {
    final response = await HttpRequest.getString('order.json');
    if (response.trim().isNotEmpty) {
      manager.loadFromJsonString(response);
      saveToLocalStorage();
      renderTable();
      return;
    }
  } catch (_) {}

  // 4. Fallback to default exam string
  manager.loadFromJsonString(defaultExamJson);
  saveToLocalStorage();
  renderTable();
}

/// Renders the orders table and search status based on currentSearchFilter
void renderTable() {
  final tbody = querySelector('#orders-tbody');
  final emptyState = querySelector('#empty-state');
  final searchStatus = querySelector('#search-status');
  if (tbody == null) return;

  tbody.children.clear();

  // Filter orders by ItemName (Requirement: Allows search Order by ItemName)
  final filteredOrders = manager.searchByItemName(currentSearchFilter);

  // Update search status message
  if (searchStatus != null) {
    if (currentSearchFilter.trim().isNotEmpty) {
      searchStatus.text = 'Found ${filteredOrders.length} order(s) matching "$currentSearchFilter"';
    } else {
      searchStatus.text = 'Total orders: ${manager.count}';
    }
  }

  if (filteredOrders.isEmpty) {
    emptyState?.classes.remove('hidden');
    return;
  } else {
    emptyState?.classes.add('hidden');
  }

  // Populate rows
  for (var i = 0; i < filteredOrders.length; i++) {
    final order = filteredOrders[i];
    final row = TableRowElement();

    // Highlight row if currently being edited
    if (editingItemCode != null && order.item == editingItemCode) {
      row.classes.add('row-editing');
    }

    // Id column: matching screenshot displaying "1" (or sequential index)
    final tdId = TableCellElement()..text = '1';
    tdId.classes.add('td-id');

    // Item column
    final tdItem = TableCellElement()..text = order.item;

    // Item Name column
    final tdName = TableCellElement()..text = order.itemName;

    // Quantity column
    final tdQty = TableCellElement()..text = order.quantity.toString();

    // Price column (formatted as 1.200)
    final tdPrice = TableCellElement()..text = order.formattedPrice;

    // Currency column
    final tdCurrency = TableCellElement()..text = order.currency;

    // Action column with Edit (pencil) and Delete (trash can) buttons
    final tdAction = TableCellElement();
    final actionContainer = DivElement()..className = 'action-buttons';

    // Edit button (Sửa)
    final btnEdit = ButtonElement()
      ..className = 'btn-edit'
      ..title = 'Sửa / Edit order ${order.item}'
      ..setInnerHtml('''
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
          <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
        </svg>
      ''', treeSanitizer: NodeTreeSanitizer.trusted);
    btnEdit.onClick.listen((_) => startEditOrder(order));

    // Delete button (Xóa)
    final btnDelete = ButtonElement()
      ..className = 'btn-delete'
      ..title = 'Xóa / Delete order ${order.item}'
      ..setInnerHtml('''
        <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="3 6 5 6 21 6"></polyline>
          <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
          <line x1="10" y1="11" x2="10" y2="17"></line>
          <line x1="14" y1="11" x2="14" y2="17"></line>
        </svg>
      ''', treeSanitizer: NodeTreeSanitizer.trusted);
    btnDelete.onClick.listen((_) => handleDeleteOrder(order.item));

    actionContainer.children.addAll([btnEdit, btnDelete]);
    tdAction.children.add(actionContainer);

    row.children.addAll([tdId, tdItem, tdName, tdQty, tdPrice, tdCurrency, tdAction]);
    tbody.children.add(row);
  }
}

/// Populates the form values for editing an order (Chức năng Sửa)
void startEditOrder(Order order) {
  editingItemCode = order.item;

  final inputItem = querySelector('#input-item') as InputElement?;
  final inputItemName = querySelector('#input-item-name') as InputElement?;
  final inputPrice = querySelector('#input-price') as InputElement?;
  final inputQuantity = querySelector('#input-quantity') as InputElement?;
  final inputCurrency = querySelector('#input-currency') as InputElement?;

  if (inputItem != null) {
    inputItem.value = order.item;
    inputItem.readOnly = true; // Key/SKU is locked during update
    inputItem.style.backgroundColor = '#f1f5f9';
  }
  if (inputItemName != null) inputItemName.value = order.itemName;
  if (inputPrice != null) {
    inputPrice.value = order.formattedPrice;
  }
  if (inputQuantity != null) inputQuantity.value = order.quantity.toString();
  if (inputCurrency != null) inputCurrency.value = order.currency;

  // Toggle button visibility
  querySelector('#btn-add-item')?.classes.add('hidden');
  querySelector('#btn-update-item')?.classes.remove('hidden');
  querySelector('#btn-cancel-edit')?.classes.remove('hidden');

  // Highlight row in table
  renderTable();

  // Focus on item name input and smooth scroll
  inputItemName?.focus();
  inputItemName?.select();
  window.scrollTo(0, 0);

  showAlert('Đang sửa đơn hàng "${order.item}". Chỉnh sửa các trường rồi bấm "Update Order".', type: 'info');
}

// ============================================================================
// Input Validation System
// ============================================================================

void showFieldError(String fieldId, String message) {
  final group = querySelector('#group-$fieldId');
  final errSpan = querySelector('#err-$fieldId');
  group?.classes.add('has-error');
  if (errSpan != null) {
    errSpan.text = message;
    errSpan.classes.remove('hidden');
  }
}

void clearFieldError(String fieldId) {
  final group = querySelector('#group-$fieldId');
  final errSpan = querySelector('#err-$fieldId');
  group?.classes.remove('has-error');
  errSpan?.classes.add('hidden');
}

void clearAllFieldErrors() {
  clearFieldError('item');
  clearFieldError('item-name');
  clearFieldError('price');
  clearFieldError('quantity');
  clearFieldError('currency');
}

/// Sets up real-time onInput and onBlur listeners to clear/show validation errors
void setupValidationListeners() {
  final inputItem = querySelector('#input-item') as InputElement?;
  final inputItemName = querySelector('#input-item-name') as InputElement?;
  final inputPrice = querySelector('#input-price') as InputElement?;
  final inputQuantity = querySelector('#input-quantity') as InputElement?;
  final inputCurrency = querySelector('#input-currency') as InputElement?;

  // Clear errors on typing
  inputItem?.onInput.listen((_) => clearFieldError('item'));
  inputItemName?.onInput.listen((_) => clearFieldError('item-name'));
  inputPrice?.onInput.listen((_) => clearFieldError('price'));
  inputQuantity?.onInput.listen((_) => clearFieldError('quantity'));
  inputCurrency?.onInput.listen((_) => clearFieldError('currency'));

  // Validate on blur
  inputItem?.onBlur.listen((_) {
    final val = inputItem.value?.trim() ?? '';
    if (val.isEmpty) {
      showFieldError('item', 'Item code is required (e.g. A1000).');
    } else if (val.length < 2 || val.length > 20) {
      showFieldError('item', 'Item code must be 2-20 characters.');
    } else if (editingItemCode == null && manager.findByItem(val) != null) {
      showFieldError('item', 'Item code "$val" already exists in cart.');
    } else {
      clearFieldError('item');
    }
  });

  inputItemName?.onBlur.listen((_) {
    final val = inputItemName.value?.trim() ?? '';
    if (val.isEmpty) {
      showFieldError('item-name', 'Item Name is required (e.g. Iphone 15).');
    } else if (val.length < 2) {
      showFieldError('item-name', 'Item Name must be at least 2 characters.');
    } else {
      clearFieldError('item-name');
    }
  });

  inputPrice?.onBlur.listen((_) {
    final val = inputPrice.value?.trim() ?? '';
    if (val.isEmpty) {
      showFieldError('price', 'Price is required (e.g. 1200 or 1.200).');
    } else {
      final p = parsePrice(val);
      if (p == null) {
        showFieldError('price', 'Price must be a valid number (e.g. 1200 or 1.200).');
      } else if (p <= 0) {
        showFieldError('price', 'Price must be greater than 0.');
      } else {
        clearFieldError('price');
      }
    }
  });

  inputQuantity?.onBlur.listen((_) {
    final val = inputQuantity.value?.trim() ?? '';
    if (val.isEmpty) {
      showFieldError('quantity', 'Quantity is required (at least 1).');
    } else {
      final q = int.tryParse(val);
      if (q == null || q <= 0) {
        showFieldError('quantity', 'Quantity must be a positive integer (e.g. 1, 2, 3).');
      } else {
        clearFieldError('quantity');
      }
    }
  });

  inputCurrency?.onBlur.listen((_) {
    final val = inputCurrency.value?.trim() ?? '';
    if (val.isEmpty) {
      showFieldError('currency', 'Currency is required (e.g. USD).');
    } else if (val.length < 2 || val.length > 5) {
      showFieldError('currency', 'Currency must be 2-5 characters (e.g. USD, VND).');
    } else {
      clearFieldError('currency');
    }
  });
}

/// Restricts Price and Quantity fields to numeric input only (phần này chỉ nhập được số)
void setupNumericInputRestrictions() {
  final inputPrice = querySelector('#input-price') as InputElement?;
  final inputQuantity = querySelector('#input-quantity') as InputElement?;

  // Allowed navigation / control keys
  bool isControlKey(KeyboardEvent e) {
    if (e.ctrlKey || e.metaKey || e.altKey) return true;
    final code = e.keyCode;
    return code == KeyCode.BACKSPACE ||
        code == KeyCode.TAB ||
        code == KeyCode.ENTER ||
        code == KeyCode.ESC ||
        code == KeyCode.LEFT ||
        code == KeyCode.RIGHT ||
        code == KeyCode.DELETE ||
        code == KeyCode.HOME ||
        code == KeyCode.END;
  }

  // 1. Price: only digits 0-9 and separator dots/commas (. or ,)
  inputPrice?.onKeyDown.listen((KeyboardEvent e) {
    if (isControlKey(e)) return;
    final char = e.key;
    if (char != null && char.length == 1) {
      if (!RegExp(r'[0-9.,]').hasMatch(char)) {
        e.preventDefault();
        showFieldError('price', 'Ô Price chỉ được nhập số!');
      }
    }
  });

  inputPrice?.onInput.listen((_) {
    if (inputPrice.value != null) {
      final sanitized = inputPrice.value!.replaceAll(RegExp(r'[^0-9.,]'), '');
      if (sanitized != inputPrice.value) {
        inputPrice.value = sanitized;
        showFieldError('price', 'Ô Price chỉ được nhập số!');
      } else if (sanitized.isNotEmpty) {
        clearFieldError('price');
      }
    }
  });

  // 2. Quantity: strictly whole digits 0-9
  inputQuantity?.onKeyDown.listen((KeyboardEvent e) {
    if (isControlKey(e)) return;
    final char = e.key;
    if (char != null && char.length == 1) {
      if (!RegExp(r'[0-9]').hasMatch(char)) {
        e.preventDefault();
        showFieldError('quantity', 'Ô Quantity chỉ được nhập số!');
      }
    }
  });

  inputQuantity?.onInput.listen((_) {
    if (inputQuantity.value != null) {
      final sanitized = inputQuantity.value!.replaceAll(RegExp(r'[^0-9]'), '');
      if (sanitized != inputQuantity.value) {
        inputQuantity.value = sanitized;
        showFieldError('quantity', 'Ô Quantity chỉ được nhập số!');
      } else if (sanitized.isNotEmpty) {
        clearFieldError('quantity');
      }
    }
  });
}

/// Comprehensive form validation before Add or Update
bool validateInputs({bool isEditing = false}) {
  clearAllFieldErrors();
  bool isValid = true;
  String? firstInvalidField;
  String? alertMessage;

  final inputItem = querySelector('#input-item') as InputElement?;
  final inputItemName = querySelector('#input-item-name') as InputElement?;
  final inputPrice = querySelector('#input-price') as InputElement?;
  final inputQuantity = querySelector('#input-quantity') as InputElement?;
  final inputCurrency = querySelector('#input-currency') as InputElement?;

  final item = inputItem?.value?.trim() ?? '';
  final itemName = inputItemName?.value?.trim() ?? '';
  final priceRaw = inputPrice?.value?.trim() ?? '';
  final quantityRaw = inputQuantity?.value?.trim() ?? '';
  final currency = inputCurrency?.value?.trim() ?? '';

  // 1. Validate Item
  if (item.isEmpty) {
    showFieldError('item', 'Item code is required (e.g. A1000).');
    isValid = false;
    firstInvalidField ??= 'input-item';
    alertMessage ??= 'Please enter an Item code (e.g. A1000).';
  } else if (item.length < 2 || item.length > 20) {
    showFieldError('item', 'Item code must be 2-20 characters.');
    isValid = false;
    firstInvalidField ??= 'input-item';
    alertMessage ??= 'Item code must be 2-20 characters.';
  } else if (!isEditing && manager.findByItem(item) != null) {
    showFieldError('item', 'Item code "$item" already exists in cart.');
    isValid = false;
    firstInvalidField ??= 'input-item';
    alertMessage ??= 'Item code "$item" already exists in cart!';
  }

  // 2. Validate Item Name
  if (itemName.isEmpty) {
    showFieldError('item-name', 'Item Name is required (e.g. Iphone 15).');
    isValid = false;
    firstInvalidField ??= 'input-item-name';
    alertMessage ??= 'Please enter an Item Name (e.g. Iphone 15).';
  } else if (itemName.length < 2) {
    showFieldError('item-name', 'Item Name must be at least 2 characters.');
    isValid = false;
    firstInvalidField ??= 'input-item-name';
    alertMessage ??= 'Item Name must be at least 2 characters.';
  }

  // 3. Validate Price
  if (priceRaw.isEmpty) {
    showFieldError('price', 'Price is required (e.g. 1200 or 1.200).');
    isValid = false;
    firstInvalidField ??= 'input-price';
    alertMessage ??= 'Please enter a valid Price (e.g. 1200 or 1.200).';
  } else {
    final price = parsePrice(priceRaw);
    if (price == null) {
      showFieldError('price', 'Price must be a valid number (e.g. 1200 or 1.200).');
      isValid = false;
      firstInvalidField ??= 'input-price';
      alertMessage ??= 'Price must be a valid number (e.g. 1200 or 1.200).';
    } else if (price <= 0) {
      showFieldError('price', 'Price must be greater than 0.');
      isValid = false;
      firstInvalidField ??= 'input-price';
      alertMessage ??= 'Price must be greater than 0.';
    }
  }

  // 4. Validate Quantity
  if (quantityRaw.isEmpty) {
    showFieldError('quantity', 'Quantity is required (at least 1).');
    isValid = false;
    firstInvalidField ??= 'input-quantity';
    alertMessage ??= 'Please enter Quantity (at least 1).';
  } else {
    final quantity = int.tryParse(quantityRaw);
    if (quantity == null || quantity <= 0) {
      showFieldError('quantity', 'Quantity must be a positive integer (e.g. 1, 2, 3).');
      isValid = false;
      firstInvalidField ??= 'input-quantity';
      alertMessage ??= 'Quantity must be a positive integer (e.g. 1, 2, 3).';
    }
  }

  // 5. Validate Currency
  if (currency.isEmpty) {
    showFieldError('currency', 'Currency is required (e.g. USD).');
    isValid = false;
    firstInvalidField ??= 'input-currency';
    alertMessage ??= 'Currency is required (e.g. USD).';
  } else if (currency.length < 2 || currency.length > 5) {
    showFieldError('currency', 'Currency must be 2-5 characters (e.g. USD, VND).');
    isValid = false;
    firstInvalidField ??= 'input-currency';
    alertMessage ??= 'Currency must be 2-5 characters (e.g. USD, VND).';
  }

  if (!isValid) {
    // Shake form visually to catch user's attention
    final form = querySelector('#order-form');
    form?.classes.add('shake');
    Future.delayed(const Duration(milliseconds: 400), () => form?.classes.remove('shake'));

    // Focus the first invalid field
    if (firstInvalidField != null) {
      (querySelector('#$firstInvalidField') as InputElement?)?.focus();
    }

    final finalMsg = alertMessage ?? 'Vui lòng kiểm tra và sửa các trường bị lỗi.';
    showAlert(finalMsg, type: 'error');
    window.alert(finalMsg);
  }

  return isValid;
}

/// Handles updating an existing order (Lưu chỉnh sửa)
Future<void> handleUpdateOrder() async {
  if (editingItemCode == null) return;
  if (!validateInputs(isEditing: true)) return;

  final inputItem = querySelector('#input-item') as InputElement?;
  final inputItemName = querySelector('#input-item-name') as InputElement?;
  final inputPrice = querySelector('#input-price') as InputElement?;
  final inputQuantity = querySelector('#input-quantity') as InputElement?;
  final inputCurrency = querySelector('#input-currency') as InputElement?;

  final item = inputItem?.value?.trim() ?? editingItemCode!;
  final itemName = inputItemName?.value?.trim() ?? '';
  final price = parsePrice(inputPrice?.value?.trim() ?? '') ?? 0.0;
  final quantity = int.parse(inputQuantity?.value?.trim() ?? '1');
  final currency = (inputCurrency?.value?.trim() ?? '').isNotEmpty
      ? inputCurrency!.value!.trim()
      : 'USD';

  final updatedOrder = Order(
    item: item,
    itemName: itemName,
    price: price,
    currency: currency,
    quantity: quantity,
  );

  // 1. Try to send PUT to server API to update order.json on disk
  try {
    await HttpRequest.request(
      '/api/orders',
      method: 'PUT',
      sendData: jsonEncode(updatedOrder.toJson()),
      requestHeaders: {'Content-Type': 'application/json'},
    );
  } catch (_) {}

  // 2. Update in-memory manager
  manager.updateOrder(updatedOrder);

  // 3. Save to localStorage
  saveToLocalStorage();

  // 4. Reset form back to Add mode
  handleCancelEdit(suppressAlert: true);

  showAlert('Cập nhật đơn hàng "$item - $itemName" thành công!', type: 'success');
}

/// Cancels edit mode and resets the form
void handleCancelEdit({bool suppressAlert = false}) {
  editingItemCode = null;
  clearAllFieldErrors();

  final inputItem = querySelector('#input-item') as InputElement?;
  final inputItemName = querySelector('#input-item-name') as InputElement?;
  final inputPrice = querySelector('#input-price') as InputElement?;
  final inputQuantity = querySelector('#input-quantity') as InputElement?;
  final inputCurrency = querySelector('#input-currency') as InputElement?;

  if (inputItem != null) {
    inputItem.value = '';
    inputItem.readOnly = false;
    inputItem.style.backgroundColor = '';
  }
  if (inputItemName != null) inputItemName.value = '';
  if (inputPrice != null) inputPrice.value = '';
  if (inputQuantity != null) inputQuantity.value = '';
  if (inputCurrency != null) inputCurrency.value = 'USD';

  // Toggle buttons
  querySelector('#btn-add-item')?.classes.remove('hidden');
  querySelector('#btn-update-item')?.classes.add('hidden');
  querySelector('#btn-cancel-edit')?.classes.add('hidden');

  renderTable();

  if (!suppressAlert) {
    showAlert('Đã hủy chế độ sửa.', type: 'info');
  }
}

/// Handles form validation and inserting a new order
Future<void> handleAddOrder() async {
  if (!validateInputs(isEditing: false)) return;

  final inputItem = querySelector('#input-item') as InputElement?;
  final inputItemName = querySelector('#input-item-name') as InputElement?;
  final inputPrice = querySelector('#input-price') as InputElement?;
  final inputQuantity = querySelector('#input-quantity') as InputElement?;
  final inputCurrency = querySelector('#input-currency') as InputElement?;

  final item = inputItem?.value?.trim() ?? '';
  final itemName = inputItemName?.value?.trim() ?? '';
  final price = parsePrice(inputPrice?.value?.trim() ?? '') ?? 0.0;
  final quantity = int.parse(inputQuantity?.value?.trim() ?? '1');
  final currency = (inputCurrency?.value?.trim() ?? '').isNotEmpty
      ? inputCurrency!.value!.trim()
      : 'USD';

  final newOrder = Order(
    item: item,
    itemName: itemName,
    price: price,
    currency: currency,
    quantity: quantity,
  );

  // Requirement: "Allows inserting an order into order.json."
  // 1. Try to post to server API to write into order.json directly
  try {
    final response = await HttpRequest.request(
      '/api/orders',
      method: 'POST',
      sendData: jsonEncode(newOrder.toJson()),
      requestHeaders: {'Content-Type': 'application/json'},
    );
    if (response.status == 201 || response.status == 200) {
      print('Saved order to server order.json successfully');
    }
  } catch (_) {
    // In standalone client-only mode, it persists via localStorage & provides instant export
  }

  // 2. Add to in-memory manager
  manager.addOrder(newOrder, replaceIfExists: true);

  // 3. Persist to localStorage
  saveToLocalStorage();

  // 4. Update UI
  renderTable();

  // 5. Clear inputs (keep currency as USD)
  inputItem?.value = '';
  inputItemName?.value = '';
  inputPrice?.value = '';
  inputQuantity?.value = '';
  inputCurrency?.value = 'USD';
  clearAllFieldErrors();
  inputItem?.focus();

  showAlert('Order "$item - $itemName" added to cart successfully!', type: 'success');
}

/// Handles deleting an order
Future<void> handleDeleteOrder(String itemCode) async {
  final confirmed = window.confirm('Are you sure you want to delete order "$itemCode"?');
  if (!confirmed) return;

  // If deleting currently edited item, cancel edit
  if (editingItemCode == itemCode) {
    handleCancelEdit(suppressAlert: true);
  }

  // Try server delete
  try {
    await HttpRequest.request('/api/orders/$itemCode', method: 'DELETE');
  } catch (_) {}

  final success = manager.deleteOrder(itemCode);
  if (success) {
    saveToLocalStorage();
    renderTable();
    showAlert('Order "$itemCode" removed from cart.', type: 'info');
  }
}

/// Saves current orders to localStorage
void saveToLocalStorage() {
  window.localStorage[localStorageKey] = manager.toJsonString(pretty: true);
}

/// Allows downloading the updated order.json file directly from the browser
void exportJsonFile() {
  final jsonString = manager.toJsonString(pretty: true);
  final blob = Blob([jsonString], 'application/json');
  final url = Url.createObjectUrlFromBlob(blob);
  AnchorElement(href: url)
    ..setAttribute('download', 'order.json')
    ..click();
  Url.revokeObjectUrl(url);
  showAlert('order.json downloaded successfully!', type: 'success');
}

/// Resets data to the default exam JSON string
void resetToDefaultData() {
  handleCancelEdit(suppressAlert: true);
  manager.loadFromJsonString(defaultExamJson);
  saveToLocalStorage();
  renderTable();
  showAlert('Orders reset to original exam default data.', type: 'info');
}

/// Loads full dataset matching Page 1 screenshot (A1000 - A1008)
void loadFullMockData() {
  handleCancelEdit(suppressAlert: true);
  manager.loadFromJsonString(fullMockDataJson);
  saveToLocalStorage();
  renderTable();
  showAlert('Đã nạp 9 đơn hàng mẫu (A1000 - A1008) như trong hình đề bài!', type: 'success');
}

/// Parses price input string safely, supporting formats like "1200", "1.200", "1,200", "1200.50"
double? parsePrice(String raw) {
  var cleaned = raw.trim();
  if (cleaned.isEmpty) return null;
  // If formatted like 1.200 (dot followed by 3 digits) as thousands separator
  if (RegExp(r'^\d{1,3}(\.\d{3})+$').hasMatch(cleaned)) {
    cleaned = cleaned.replaceAll('.', '');
  } else if (RegExp(r'^\d{1,3}(,\d{3})+$').hasMatch(cleaned)) {
    cleaned = cleaned.replaceAll(',', '');
  } else {
    cleaned = cleaned.replaceAll(',', '');
  }
  return double.tryParse(cleaned);
}

/// Helper to auto-fill sample values into the input form for quick testing
void fillSampleInputForm() {
  handleCancelEdit(suppressAlert: true);
  clearAllFieldErrors();

  final inputItem = querySelector('#input-item') as InputElement?;
  final inputItemName = querySelector('#input-item-name') as InputElement?;
  final inputPrice = querySelector('#input-price') as InputElement?;
  final inputQuantity = querySelector('#input-quantity') as InputElement?;
  final inputCurrency = querySelector('#input-currency') as InputElement?;

  // Determine next item SKU
  final nextIndex = manager.count + 1000;
  final nextItemCode = 'A$nextIndex';

  if (inputItem != null) inputItem.value = nextItemCode;
  if (inputItemName != null) inputItemName.value = 'Iphone 16 Pro Max';
  if (inputPrice != null) inputPrice.value = '1.600';
  if (inputQuantity != null) inputQuantity.value = '1';
  if (inputCurrency != null) inputCurrency.value = 'USD';

  inputItemName?.focus();
  showAlert('Đã điền sẵn giá trị mẫu vào Form ($nextItemCode). Bấm "Add Item to Cart" để thêm!', type: 'info');
}

/// Shows user feedback message
void showAlert(String message, {String type = 'success'}) {
  final alertBox = querySelector('#alert-box');
  if (alertBox == null) return;

  alertBox.text = message;
  alertBox.classes.remove('hidden');
  alertBox.classes.remove('alert-success');
  alertBox.classes.remove('alert-error');
  alertBox.classes.remove('alert-info');

  switch (type) {
    case 'error':
      alertBox.classes.add('alert-error');
      break;
    case 'info':
      alertBox.classes.add('alert-info');
      break;
    default:
      alertBox.classes.add('alert-success');
  }

  // Auto-hide after 3.5 seconds
  Future.delayed(const Duration(milliseconds: 3500), () {
    alertBox.classes.add('hidden');
  });
}
