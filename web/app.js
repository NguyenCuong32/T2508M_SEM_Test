/**
 * Introduction to Dart Programming - SET01
 * Order Management & Shop Cart Frontend Logic (Synchronizes with Dart Backend & order.json)
 */

// Initial JSON string given in the exam paper:
const INITIAL_ORDERS_JSON_STRING =
  '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": 1500,"Currency": "USD","Quantity":1}]';

/**
 * Question 1: Create a class with the name "Order" (2 Marks)
 */
class Order {
  constructor({ Item, ItemName, Price, Currency = 'USD', Quantity = 1 }) {
    this.item = String(Item ?? '').trim();
    this.itemName = String(ItemName ?? '').trim();
    this.price = Number(Price) || 0;
    this.currency = String(Currency || 'USD').trim();
    this.quantity = parseInt(Quantity, 10) || 1;
  }

  static fromJson(json) {
    return new Order({
      Item: json.Item ?? json.item ?? '',
      ItemName: json.ItemName ?? json.itemName ?? '',
      Price: json.Price ?? json.price ?? 0,
      Currency: json.Currency ?? json.currency ?? 'USD',
      Quantity: json.Quantity ?? json.quantity ?? 1,
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
   * Format price with dot separator (e.g. 1200 -> 1.200) matching the exam paper image
   */
  get formattedPrice() {
    return formatNumberWithDots(this.price);
  }
}

function formatNumberWithDots(numValue) {
  const numeric = Number(numValue) || 0;
  const isWhole = Number.isInteger(numeric);
  const raw = isWhole ? String(numeric) : numeric.toFixed(2);
  const parts = raw.split('.');
  const intPart = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  return parts.length > 1 ? `${intPart},${parts[1]}` : intPart;
}

/**
 * Question 2: Read the content from the string and save objects to list orders (5 Marks)
 */
function parseOrdersFromString(jsonString) {
  const parsed = JSON.parse(jsonString);
  if (!Array.isArray(parsed)) return [];
  return parsed.map((obj) => Order.fromJson(obj));
}

// List of Order objects
let orders = parseOrdersFromString(INITIAL_ORDERS_JSON_STRING);
let hasBackendApi = false;

// DOM Elements
const orderForm = document.getElementById('orderForm');
const itemInput = document.getElementById('itemInput');
const itemNameInput = document.getElementById('itemNameInput');
const priceInput = document.getElementById('priceInput');
const quantityInput = document.getElementById('quantityInput');
const currencyInput = document.getElementById('currencyInput');
const searchInput = document.getElementById('searchInput');
const clearSearchBtn = document.getElementById('clearSearchBtn');
const resetDefaultBtn = document.getElementById('resetDefaultBtn');
const downloadJsonBtn = document.getElementById('downloadJsonBtn');
const ordersTableBody = document.getElementById('ordersTableBody');
const statusMessage = document.getElementById('statusMessage');

// Shop Summary & Live Preview Elements
const navCartCount = document.getElementById('navCartCount');
const navCartTotal = document.getElementById('navCartTotal');
const orderCountBadge = document.getElementById('orderCountBadge');
const totalQtyBadge = document.getElementById('totalQtyBadge');
const totalAmountBadge = document.getElementById('totalAmountBadge');
const jsonPreviewCode = document.getElementById('jsonPreviewCode');

/**
 * Load orders from Dart Server (/api/orders or /order.json),
 * or fallback to localStorage / INITIAL_ORDERS_JSON_STRING
 */
async function initializeOrders() {
  // Always read the exam's JSON string into list orders first as required
  orders = parseOrdersFromString(INITIAL_ORDERS_JSON_STRING);

  try {
    const response = await fetch('/api/orders', { cache: 'no-store' });
    if (response.ok) {
      const data = await response.json();
      if (Array.isArray(data.orders)) {
        orders = data.orders.map((item) => Order.fromJson(item));
        hasBackendApi = true;
      }
    }
  } catch (_) {
    // Try fetching order.json if hosted on a static web server
    try {
      const fileResp = await fetch('../order.json', { cache: 'no-store' });
      if (fileResp.ok) {
        const text = await fileResp.text();
        orders = parseOrdersFromString(text);
      }
    } catch (_) {
      // Load from localStorage if available in standalone browser mode
      const saved = localStorage.getItem('order_json_data');
      if (saved) {
        try {
          orders = parseOrdersFromString(saved);
        } catch (_) {}
      }
    }
  }

  renderOrdersTable();
}

/**
 * Question 4: Allows search Order by ItemName (3 Marks)
 */
function searchOrdersByItemName(orderList, keyword) {
  const query = (keyword || '').trim().toLowerCase();
  if (!query) return orderList;
  return orderList.filter((order) =>
    order.itemName.toLowerCase().includes(query)
  );
}

/**
 * Update Cart Summary Badges & Live order.json Inspector
 */
function updateCartSummary(displayedOrders) {
  const totalOrders = displayedOrders.length;
  const totalQty = displayedOrders.reduce((acc, cur) => acc + (Number(cur.quantity) || 0), 0);
  const totalAmount = displayedOrders.reduce(
    (acc, cur) => acc + (Number(cur.price) || 0) * (Number(cur.quantity) || 1),
    0
  );
  const primaryCurrency = displayedOrders[0]?.currency || 'USD';
  const formattedTotal = `${formatNumberWithDots(totalAmount)} ${primaryCurrency}`;

  if (navCartCount) navCartCount.textContent = `${totalQty} item${totalQty === 1 ? '' : 's'}`;
  if (navCartTotal) navCartTotal.textContent = formattedTotal;
  if (orderCountBadge) {
    orderCountBadge.textContent = `${totalOrders} Order${totalOrders === 1 ? '' : 's'}`;
  }
  if (totalQtyBadge) {
    totalQtyBadge.textContent = `Total Quantity: ${totalQty} item${totalQty === 1 ? '' : 's'}`;
  }
  if (totalAmountBadge) {
    totalAmountBadge.textContent = formattedTotal;
  }
  if (jsonPreviewCode) {
    jsonPreviewCode.textContent = JSON.stringify(
      orders.map((o) => o.toJson()),
      null,
      2
    );
  }
}

/**
 * Render all objects in orders (or filtered orders) to the table
 */
function renderOrdersTable() {
  ordersTableBody.innerHTML = '';

  const query = searchInput.value;
  const displayedOrders = searchOrdersByItemName(orders, query);

  updateCartSummary(displayedOrders);

  if (displayedOrders.length === 0) {
    const emptyTr = document.createElement('tr');
    emptyTr.className = 'empty-row';
    const td = document.createElement('td');
    td.colSpan = 7;
    td.textContent = query
      ? `No orders found matching Item Name "${query}".`
      : 'Your order cart is currently empty. Add an item above!';
    emptyTr.appendChild(td);
    ordersTableBody.appendChild(emptyTr);
    return;
  }

  displayedOrders.forEach((order, idx) => {
    const originalIndex = orders.indexOf(order);
    const tr = document.createElement('tr');

    // Column 1: Id
    const idTd = document.createElement('td');
    idTd.textContent = String(idx + 1);

    // Column 2: Item (SKU)
    const itemTd = document.createElement('td');
    const skuSpan = document.createElement('span');
    skuSpan.className = 'sku-pill';
    skuSpan.textContent = order.item;
    itemTd.appendChild(skuSpan);

    // Column 3: Item Name
    const nameTd = document.createElement('td');
    nameTd.className = 'item-name-cell';
    nameTd.textContent = order.itemName;

    // Column 4: Quantity with interactive shop +/- buttons
    const qtyTd = document.createElement('td');
    const qtyControl = document.createElement('div');
    qtyControl.className = 'qty-control';

    const minusBtn = document.createElement('button');
    minusBtn.type = 'button';
    minusBtn.className = 'qty-btn';
    minusBtn.title = 'Decrease quantity';
    minusBtn.textContent = '−';
    minusBtn.addEventListener('click', () => changeOrderQuantity(originalIndex, -1));

    const qtyNum = document.createElement('span');
    qtyNum.className = 'qty-number';
    qtyNum.textContent = String(order.quantity);

    const plusBtn = document.createElement('button');
    plusBtn.type = 'button';
    plusBtn.className = 'qty-btn';
    plusBtn.title = 'Increase quantity';
    plusBtn.textContent = '+';
    plusBtn.addEventListener('click', () => changeOrderQuantity(originalIndex, 1));

    qtyControl.appendChild(minusBtn);
    qtyControl.appendChild(qtyNum);
    qtyControl.appendChild(plusBtn);
    qtyTd.appendChild(qtyControl);

    // Column 5: Price
    const priceTd = document.createElement('td');
    priceTd.className = 'price-cell';
    priceTd.textContent = order.formattedPrice;

    // Column 6: Currency
    const currencyTd = document.createElement('td');
    const currSpan = document.createElement('span');
    currSpan.className = 'currency-badge';
    currSpan.textContent = order.currency;
    currencyTd.appendChild(currSpan);

    // Column 7: Delete icon button
    const actionTd = document.createElement('td');
    actionTd.className = 'action-cell';

    const delBtn = document.createElement('button');
    delBtn.type = 'button';
    delBtn.className = 'btn-delete';
    delBtn.title = `Delete ${order.itemName} (${order.item})`;
    delBtn.innerHTML = `
      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="3 6 5 6 21 6"></polyline>
        <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
      </svg>
    `;
    delBtn.addEventListener('click', () => handleDeleteOrder(originalIndex));

    actionTd.appendChild(delBtn);

    tr.appendChild(idTd);
    tr.appendChild(itemTd);
    tr.appendChild(nameTd);
    tr.appendChild(qtyTd);
    tr.appendChild(priceTd);
    tr.appendChild(currencyTd);
    tr.appendChild(actionTd);

    ordersTableBody.appendChild(tr);
  });
}

/**
 * Adjust quantity directly in the shop cart table
 */
function changeOrderQuantity(index, delta) {
  if (index < 0 || index >= orders.length) return;
  const target = orders[index];
  const nextQty = target.quantity + delta;
  if (nextQty <= 0) {
    handleDeleteOrder(index);
    return;
  }
  target.quantity = nextQty;
  syncLocalStorage();
  renderOrdersTable();
  showStatus(`Updated "${target.itemName}" quantity to ${nextQty}.`);
}

/**
 * Question 3: Allows inserting an order into order.json (4 Marks)
 */
async function insertNewOrderObject(newOrder) {
  if (hasBackendApi) {
    try {
      const response = await fetch('/api/orders', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newOrder.toJson()),
      });
      if (response.ok) {
        const result = await response.json();
        orders = result.orders.map((o) => Order.fromJson(o));
      } else {
        orders.push(newOrder);
      }
    } catch (_) {
      orders.push(newOrder);
    }
  } else {
    orders.push(newOrder);
  }

  syncLocalStorage();
  renderOrdersTable();
  showStatus(`Added "${newOrder.itemName}" (${newOrder.item}) into order.json.`);
}

async function handleInsertOrder(event) {
  event.preventDefault();

  const item = itemInput.value.trim();
  const itemName = itemNameInput.value.trim();
  const priceValue = parseFloat(priceInput.value);
  const quantityValue = parseInt(quantityInput.value, 10);
  const currency = currencyInput.value.trim() || 'USD';

  if (!item || !itemName || isNaN(priceValue) || priceValue < 0 || isNaN(quantityValue) || quantityValue <= 0) {
    showStatus('Please enter valid Item, Item Name, Price, and Quantity.', true);
    return;
  }

  const newOrder = new Order({
    Item: item,
    ItemName: itemName,
    Price: priceValue,
    Currency: currency.toUpperCase(),
    Quantity: quantityValue,
  });

  await insertNewOrderObject(newOrder);
  orderForm.reset();
  currencyInput.value = 'USD';
}

/**
 * Pre-fill the Order form when clicking "Select Item" on a Featured Shop Card
 */
window.prefillProduct = function (itemCode, itemName, price, currency) {
  itemInput.value = itemCode;
  itemNameInput.value = itemName;
  priceInput.value = price;
  quantityInput.value = 1;
  currencyInput.value = currency || 'USD';
  itemInput.focus();
  showStatus(`Selected "${itemName}" (${itemCode}). Click "Add Item to Cart" to insert.`);
};

/**
 * 1-Click Quick Add from Featured Shop Catalog into order.json
 */
window.quickAddProduct = async function (itemCode, itemName, price, currency) {
  const newOrder = new Order({
    Item: itemCode,
    ItemName: itemName,
    Price: price,
    Currency: currency || 'USD',
    Quantity: 1,
  });
  await insertNewOrderObject(newOrder);
};

/**
 * Delete an order row from orders list and order.json
 */
async function handleDeleteOrder(index) {
  if (index < 0 || index >= orders.length) return;
  const target = orders[index];

  if (hasBackendApi) {
    try {
      const response = await fetch(`/api/orders/${index}`, {
        method: 'DELETE',
      });
      if (response.ok) {
        const result = await response.json();
        orders = result.orders.map((o) => Order.fromJson(o));
      } else {
        orders.splice(index, 1);
      }
    } catch (_) {
      orders.splice(index, 1);
    }
  } else {
    orders.splice(index, 1);
  }

  syncLocalStorage();
  renderOrdersTable();
  showStatus(`Removed "${target.itemName}" (${target.item}) from order.json.`);
}

function resetToExamDefault() {
  orders = parseOrdersFromString(INITIAL_ORDERS_JSON_STRING);
  syncLocalStorage();
  searchInput.value = '';
  renderOrdersTable();
  showStatus('Reset cart to initial exam JSON string (Iphone 15 & Iphone 16).');
}

function syncLocalStorage() {
  const serialized = JSON.stringify(
    orders.map((o) => o.toJson()),
    null,
    2
  );
  localStorage.setItem('order_json_data', serialized);
}

function downloadOrderJsonFile() {
  const serialized = JSON.stringify(
    orders.map((o) => o.toJson()),
    null,
    2
  );
  const blob = new Blob([serialized], { type: 'application/json' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = 'order.json';
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

function showStatus(msg, isError = false) {
  statusMessage.textContent = msg;
  statusMessage.classList.toggle('error', isError);
}

// Event Listeners
orderForm.addEventListener('submit', handleInsertOrder);
searchInput.addEventListener('input', () => renderOrdersTable());
clearSearchBtn.addEventListener('click', () => {
  searchInput.value = '';
  renderOrdersTable();
});
if (resetDefaultBtn) {
  resetDefaultBtn.addEventListener('click', resetToExamDefault);
}
downloadJsonBtn.addEventListener('click', downloadOrderJsonFile);

// Initialize on load
initializeOrders();
