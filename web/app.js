/**
 * Introduction to Dart Programming - SET01
 * Web Application Logic (app.js)
 *
 * Implements:
 * 1. Class Order (Question 1)
 * 2. Parse JSON string to List of Orders (Question 2)
 * 3. Insert & Edit (Update) into order.json (Question 3 + Edit Feature)
 * 4. Search Order by ItemName (Question 4)
 * 5. Bộ lọc & Sắp xếp dữ liệu (Filter & Sort by Currency, Price Range, Name, Price)
 * 6. Full sync with Dart backend API (/api/orders) or LocalStorage
 */

// Initial JSON string given in the exam paper
const INITIAL_ORDERS_JSON_STRING =
  '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": 1500,"Currency": "USD","Quantity":1}]';

/**
 * Question 1: Order Class
 */
class Order {
  constructor({ Item, ItemName, Price, Currency, Quantity }) {
    this.item = (Item || '').toString().trim();
    this.itemName = (ItemName || '').toString().trim();
    this.price = typeof Price === 'number' ? Price : parseFloat(Price) || 0;
    this.currency = (Currency || 'USD').toString().trim();
    this.quantity = typeof Quantity === 'number' ? Quantity : parseInt(Quantity, 10) || 1;
  }

  static fromJson(json) {
    return new Order({
      Item: json.Item ?? json.item,
      ItemName: json.ItemName ?? json.itemName,
      Price: json.Price ?? json.price,
      Currency: json.Currency ?? json.currency,
      Quantity: json.Quantity ?? json.quantity,
    });
  }

  toJson() {
    return {
      Item: this.item,
      ItemName: this.itemName,
      Price: this.price,
      Currency: this.currency,
      Quantity: this.quantity,
    };
  }

  /**
   * Helper to format price with dots (e.g. 1200 -> "1.200") as shown in the exam picture
   */
  get formattedPrice() {
    const isWhole = Number.isInteger(this.price);
    const raw = isWhole ? this.price.toString() : this.price.toFixed(2);
    const parts = raw.split('.');
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    return parts.length > 1 ? parts.join(',') : parts[0];
  }

  get total() {
    return this.price * this.quantity;
  }

  get formattedTotal() {
    const isWhole = Number.isInteger(this.total);
    const raw = isWhole ? this.total.toString() : this.total.toFixed(2);
    const parts = raw.split('.');
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    return parts.length > 1 ? parts.join(',') : parts[0];
  }
}

// In-memory list of Order objects & Edit state
let orders = [];
let hasBackend = false;
let editingIndex = null; // null: adding mode, number: editing order at this index

// DOM Elements
const orderForm = document.getElementById('orderForm');
const itemInput = document.getElementById('itemInput');
const itemNameInput = document.getElementById('itemNameInput');
const priceInput = document.getElementById('priceInput');
const quantityInput = document.getElementById('quantityInput');
const currencyInput = document.getElementById('currencyInput');
const addToCartBtn = document.getElementById('addToCartBtn');
const cancelEditBtn = document.getElementById('cancelEditBtn');
const editingBadge = document.getElementById('editingBadge');
const editingOrderTitle = document.getElementById('editingOrderTitle');

// Filter & Search Controls
const searchInput = document.getElementById('searchInput');
const clearSearchBtn = document.getElementById('clearSearchBtn');
const currencyFilter = document.getElementById('currencyFilter');
const priceFilter = document.getElementById('priceFilter');
const sortFilter = document.getElementById('sortFilter');
const filterCountBadge = document.getElementById('filterCountBadge');

// Table & Summary
const ordersTableBody = document.getElementById('ordersTableBody');
const statusMessage = document.getElementById('statusMessage');
const downloadJsonBtn = document.getElementById('downloadJsonBtn');
const resetDefaultsBtn = document.getElementById('resetDefaultsBtn');
const totalQuantityElem = document.getElementById('totalQuantity');
const totalAmountElem = document.getElementById('totalAmount');

/**
 * Question 2: Parse JSON string to List of Order objects
 */
function parseOrdersFromString(jsonString) {
  try {
    const parsed = JSON.parse(jsonString);
    if (Array.isArray(parsed)) {
      return parsed.map((item) => Order.fromJson(item));
    }
    return [];
  } catch (err) {
    console.error('Failed to parse JSON string:', err);
    return [];
  }
}

/**
 * Question 4 & Bộ lọc: Search, Filter and Sort
 */
function getFilteredAndSortedOrders() {
  const query = (searchInput.value || '').trim().toLowerCase();
  const selectedCurrency = currencyFilter ? currencyFilter.value : 'ALL';
  const selectedPriceRange = priceFilter ? priceFilter.value : 'ALL';
  const selectedSort = sortFilter ? sortFilter.value : 'added';

  // 1. Filter by Search Query (ItemName)
  let result = orders.filter((order) => {
    if (!query) return true;
    return order.itemName.toLowerCase().includes(query) ||
           order.item.toLowerCase().includes(query);
  });

  // 2. Filter by Currency
  if (selectedCurrency !== 'ALL') {
    result = result.filter((order) =>
      order.currency.toUpperCase() === selectedCurrency.toUpperCase()
    );
  }

  // 3. Filter by Price Range
  if (selectedPriceRange === 'under1300') {
    result = result.filter((order) => order.price < 1300);
  } else if (selectedPriceRange === '1300-1600') {
    result = result.filter((order) => order.price >= 1300 && order.price <= 1600);
  } else if (selectedPriceRange === 'above1600') {
    result = result.filter((order) => order.price > 1600);
  }

  // 4. Sort
  result = [...result];
  switch (selectedSort) {
    case 'newest':
      result.reverse();
      break;
    case 'nameAsc':
      result.sort((a, b) => a.itemName.toLowerCase().localeCompare(b.itemName.toLowerCase()));
      break;
    case 'nameDesc':
      result.sort((a, b) => b.itemName.toLowerCase().localeCompare(a.itemName.toLowerCase()));
      break;
    case 'priceAsc':
      result.sort((a, b) => a.price - b.price);
      break;
    case 'priceDesc':
      result.sort((a, b) => b.price - a.price);
      break;
    case 'added':
    default:
      // keep original insertion order
      break;
  }

  return result;
}

/**
 * Initialize Data from Server or Local Storage
 */
async function initializeData() {
  try {
    const res = await fetch('/api/orders');
    if (res.ok) {
      const data = await res.json();
      hasBackend = true;
      orders = (data.orders || []).map((o) => Order.fromJson(o));
      showToast('Đã kết nối với Dart Backend Server! Tải dữ liệu từ order.json.');
      renderTable();
      return;
    }
  } catch (_) {
    hasBackend = false;
  }

  // Fallback to localStorage or initial exam string
  const saved = localStorage.getItem('order_json_content');
  if (saved) {
    orders = parseOrdersFromString(saved);
  } else {
    orders = parseOrdersFromString(INITIAL_ORDERS_JSON_STRING);
    syncLocalStorage();
  }
  renderTable();
}

/**
 * Render Orders Table
 */
function renderTable() {
  const query = searchInput.value.trim();
  const filtered = getFilteredAndSortedOrders();

  clearSearchBtn.style.display = query.length > 0 ? 'block' : 'none';
  if (filterCountBadge) {
    filterCountBadge.innerHTML = `Hiển thị: <strong>${filtered.length}/${orders.length}</strong> đơn hàng`;
  }

  ordersTableBody.innerHTML = '';

  if (filtered.length === 0) {
    const emptyRow = document.createElement('tr');
    emptyRow.innerHTML = `
      <td colspan="7" class="empty-state-cell">
        ${query ? `Không tìm thấy đơn hàng nào khớp với từ khóa "${query}".` : 'Không có đơn hàng nào khớp với bộ lọc đã chọn.'}
      </td>
    `;
    ordersTableBody.appendChild(emptyRow);
  } else {
    filtered.forEach((order, index) => {
      // Find true index in orders array for editing and deletion
      const trueIndex = orders.indexOf(order);
      const isCurrentlyEditing = editingIndex === trueIndex;
      const row = document.createElement('tr');
      if (isCurrentlyEditing) {
        row.style.backgroundColor = '#fff9db';
      }

      row.innerHTML = `
        <td>${index + 1}</td>
        <td><strong>${escapeHtml(order.item)}</strong></td>
        <td>${escapeHtml(order.itemName)}</td>
        <td>${order.quantity}</td>
        <td>${order.formattedPrice}</td>
        <td>${escapeHtml(order.currency)}</td>
        <td class="col-center">
          <div class="action-cell-btns">
            <!-- Edit button (Icon bút chì) -->
            <button
              type="button"
              class="btn-edit-row"
              title="Sửa đơn hàng ${escapeHtml(order.item)} - ${escapeHtml(order.itemName)}"
              onclick="handleStartEdit(${trueIndex})"
            >
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round">
                <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
              </svg>
            </button>

            <!-- Delete button (Icon thùng rác) -->
            <button
              type="button"
              class="btn-delete-row"
              title="Xóa đơn hàng ${escapeHtml(order.item)}"
              onclick="handleDeleteOrder(${trueIndex})"
            >
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="3 6 5 6 21 6"></polyline>
                <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                <line x1="10" y1="11" x2="10" y2="17"></line>
                <line x1="14" y1="11" x2="14" y2="17"></line>
              </svg>
            </button>
          </div>
        </td>
      `;
      ordersTableBody.appendChild(row);
    });
  }

  updateCartSummary(filtered);
}

/**
 * Question 3: Thêm mới hoặc Sửa dữ liệu đơn hàng (Insert / Update)
 */
async function handleFormSubmit(event) {
  event.preventDefault();

  const item = itemInput.value.trim();
  const itemName = itemNameInput.value.trim();
  const price = parseFloat(priceInput.value);
  const quantity = parseInt(quantityInput.value, 10);
  const currency = currencyInput.value.trim() || 'USD';

  if (!item || !itemName) {
    showToast('Vui lòng nhập Item và Item Name!', true);
    return;
  }

  if (isNaN(price) || price < 0) {
    showToast('Vui lòng nhập Price là số hợp lệ!', true);
    return;
  }

  if (isNaN(quantity) || quantity < 1) {
    showToast('Vui lòng nhập Quantity >= 1!', true);
    return;
  }

  const orderData = new Order({
    Item: item,
    ItemName: itemName,
    Price: price,
    Currency: currency,
    Quantity: quantity,
  });

  // Chế độ Sửa đơn hàng (Update Mode)
  if (editingIndex !== null) {
    const originalItem = orders[editingIndex].itemName;
    if (hasBackend) {
      try {
        const res = await fetch(`/api/orders/${editingIndex}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(orderData.toJson()),
        });
        if (res.ok) {
          const data = await res.json();
          orders = (data.orders || []).map((o) => Order.fromJson(o));
        } else {
          orders[editingIndex] = orderData;
        }
      } catch (_) {
        orders[editingIndex] = orderData;
      }
    } else {
      orders[editingIndex] = orderData;
      syncLocalStorage();
    }

    showToast(`Đã cập nhật thành công đơn hàng "${orderData.itemName}" (${orderData.item})!`);
    cancelEditOrder();
    renderTable();
    return;
  }

  // Chế độ Thêm mới đơn hàng (Insert Mode)
  if (hasBackend) {
    try {
      const res = await fetch('/api/orders', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(orderData.toJson()),
      });

      if (res.ok) {
        const data = await res.json();
        orders = (data.orders || []).map((o) => Order.fromJson(o));
        showToast(`Đã thêm thành công "${orderData.itemName}" (${orderData.item}) vào order.json!`);
      } else {
        orders.push(orderData);
        showToast(`Đã thêm "${orderData.itemName}" vào giỏ hàng!`);
      }
    } catch (_) {
      orders.push(orderData);
      showToast(`Đã thêm "${orderData.itemName}" vào giỏ hàng!`);
    }
  } else {
    orders.push(orderData);
    syncLocalStorage();
    showToast(`Đã thêm "${orderData.itemName}" (${orderData.item}) vào danh sách đơn hàng!`);
  }

  // Reset form
  orderForm.reset();
  currencyInput.value = 'USD';
  quantityInput.value = '1';
  itemInput.focus();

  renderTable();
}

/**
 * Bắt đầu chỉnh sửa đơn hàng (Start Edit)
 */
window.handleStartEdit = function (index) {
  if (index < 0 || index >= orders.length) return;
  const target = orders[index];
  editingIndex = index;

  // Điền dữ liệu vào form
  itemInput.value = target.item;
  itemNameInput.value = target.itemName;
  priceInput.value = target.price;
  quantityInput.value = target.quantity;
  currencyInput.value = target.currency;

  // Hiển thị trạng thái sửa
  if (editingBadge && editingOrderTitle) {
    editingOrderTitle.textContent = `${target.item} - ${target.itemName}`;
    editingBadge.style.display = 'flex';
  }
  if (cancelEditBtn) {
    cancelEditBtn.style.display = 'inline-block';
  }
  addToCartBtn.textContent = 'Lưu thay đổi (Update)';
  addToCartBtn.style.backgroundColor = '#d35400';

  // Cuộn nhẹ lên form và focus vào tên sản phẩm
  orderForm.scrollIntoView({ behavior: 'smooth', block: 'center' });
  itemNameInput.focus();

  renderTable();
  showToast(`Đang sửa đơn hàng "${target.itemName}". Sửa thông tin và bấm "Lưu thay đổi".`);
};

/**
 * Hủy chế độ chỉnh sửa (Cancel Edit)
 */
window.cancelEditOrder = function () {
  editingIndex = null;
  orderForm.reset();
  currencyInput.value = 'USD';
  quantityInput.value = '1';

  if (editingBadge) {
    editingBadge.style.display = 'none';
  }
  if (cancelEditBtn) {
    cancelEditBtn.style.display = 'none';
  }
  addToCartBtn.textContent = 'Add Item to Cart';
  addToCartBtn.style.backgroundColor = '';

  renderTable();
};

/**
 * Xóa đơn hàng (Delete Order)
 */
window.handleDeleteOrder = async function (index) {
  if (index < 0 || index >= orders.length) return;
  const target = orders[index];

  if (!confirm(`Bạn có chắc muốn xóa đơn hàng "${target.itemName}" (${target.item}) khỏi order.json?`)) {
    return;
  }

  // Nếu đang sửa chính dòng này thì hủy chế độ sửa
  if (editingIndex === index) {
    cancelEditOrder();
  }

  if (hasBackend) {
    try {
      const res = await fetch(`/api/orders/${index}`, { method: 'DELETE' });
      if (res.ok) {
        const data = await res.json();
        orders = (data.orders || []).map((o) => Order.fromJson(o));
        showToast(`Đã xóa đơn hàng "${target.itemName}" khỏi order.json!`);
      } else {
        orders.splice(index, 1);
      }
    } catch (_) {
      orders.splice(index, 1);
    }
  } else {
    orders.splice(index, 1);
    syncLocalStorage();
    showToast(`Đã xóa "${target.itemName}" khỏi danh sách!`);
  }

  renderTable();
};

/**
 * Gợi ý mẫu nhanh (Quick Prefill)
 */
window.prefillForm = function (item, itemName, price, currency, quantity) {
  if (editingIndex !== null) {
    cancelEditOrder();
  }
  itemInput.value = item;
  itemNameInput.value = itemName;
  priceInput.value = price;
  currencyInput.value = currency;
  quantityInput.value = quantity;
  showToast(`Đã chọn mẫu: ${itemName}`);
};

/**
 * Cập nhật thông tin giỏ hàng
 */
function updateCartSummary(currentList) {
  const totalQty = currentList.reduce((acc, o) => acc + o.quantity, 0);
  const totalAmt = currentList.reduce((acc, o) => acc + o.total, 0);

  totalQuantityElem.textContent = totalQty;
  const formattedAmt = totalAmt.toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  totalAmountElem.textContent = `${formattedAmt} USD`;
}

/**
 * Đồng bộ LocalStorage
 */
function syncLocalStorage() {
  const json = JSON.stringify(orders.map((o) => o.toJson()), null, 2);
  localStorage.setItem('order_json_content', json);
}

/**
 * Tải về tệp order.json
 */
function downloadOrderJson() {
  const json = JSON.stringify(orders.map((o) => o.toJson()), null, 2);
  const blob = new Blob([json], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'order.json';
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
  showToast('Đã tải xuống tệp order.json thành công!');
}

/**
 * Khôi phục dữ liệu ban đầu
 */
function resetToExamDefaults() {
  if (confirm('Khôi phục danh sách về 2 đơn hàng mẫu ban đầu trong đề thi?')) {
    cancelEditOrder();
    orders = parseOrdersFromString(INITIAL_ORDERS_JSON_STRING);
    syncLocalStorage();
    searchInput.value = '';
    if (currencyFilter) currencyFilter.value = 'ALL';
    if (priceFilter) priceFilter.value = 'ALL';
    if (sortFilter) sortFilter.value = 'added';
    renderTable();
    showToast('Đã khôi phục dữ liệu ban đầu từ đề thi!');
  }
}

/**
 * Hiển thị thông báo Toast
 */
let toastTimeout;
function showToast(text, isError = false) {
  clearTimeout(toastTimeout);
  statusMessage.textContent = text;
  statusMessage.className = `status-message ${isError ? 'error' : ''}`;
  statusMessage.style.display = 'block';

  toastTimeout = setTimeout(() => {
    statusMessage.style.display = 'none';
  }, 4500);
}

function escapeHtml(str) {
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

// -------------------------------------------------------------
// Chặn gõ chữ vào ô Price & Quantity
// -------------------------------------------------------------
priceInput.addEventListener('keypress', (e) => {
  if (!/[0-9.]/.test(e.key)) {
    e.preventDefault();
  }
  if (e.key === '.' && priceInput.value.includes('.')) {
    e.preventDefault();
  }
});

priceInput.addEventListener('input', () => {
  let val = priceInput.value.replace(/[^0-9.]/g, '');
  const parts = val.split('.');
  if (parts.length > 2) {
    val = parts[0] + '.' + parts.slice(1).join('');
  }
  priceInput.value = val;
});

quantityInput.addEventListener('keypress', (e) => {
  if (!/[0-9]/.test(e.key)) {
    e.preventDefault();
  }
});

quantityInput.addEventListener('input', () => {
  quantityInput.value = quantityInput.value.replace(/[^0-9]/g, '');
});

// -------------------------------------------------------------
// Đăng ký Event Listeners
// -------------------------------------------------------------
orderForm.addEventListener('submit', handleFormSubmit);

searchInput.addEventListener('input', renderTable);
clearSearchBtn.addEventListener('click', () => {
  searchInput.value = '';
  renderTable();
  searchInput.focus();
});

if (currencyFilter) currencyFilter.addEventListener('change', renderTable);
if (priceFilter) priceFilter.addEventListener('change', renderTable);
if (sortFilter) sortFilter.addEventListener('change', renderTable);

downloadJsonBtn.addEventListener('click', downloadOrderJson);
resetDefaultsBtn.addEventListener('click', resetToExamDefaults);

// Khởi chạy khi tải trang
initializeData();
