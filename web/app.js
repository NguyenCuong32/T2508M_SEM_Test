/**
 * Introduction to Dart Programming - SET01
 * Order Management Frontend Logic (Synchronizes with Dart Backend & order.json)
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
    const isWhole = Number.isInteger(this.price);
    const raw = isWhole ? String(this.price) : this.price.toFixed(2);
    const parts = raw.split('.');
    const intPart = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    return parts.length > 1 ? `${intPart},${parts[1]}` : intPart;
  }
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
const downloadJsonBtn = document.getElementById('downloadJsonBtn');
const ordersTableBody = document.getElementById('ordersTableBody');
const statusMessage = document.getElementById('statusMessage');

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
 * Render all objects in orders (or filtered orders) to the table
 */
function renderOrdersTable() {
  ordersTableBody.innerHTML = '';

  const query = searchInput.value;
  const displayedOrders = searchOrdersByItemName(orders, query);

  if (displayedOrders.length === 0) {
    const emptyTr = document.createElement('tr');
    emptyTr.className = 'empty-row';
    const td = document.createElement('td');
    td.colSpan = 7;
    td.textContent = query
      ? `No orders found matching Item Name "${query}".`
      : 'No orders in list.';
    emptyTr.appendChild(td);
    ordersTableBody.appendChild(emptyTr);
    return;
  }

  displayedOrders.forEach((order, idx) => {
    const originalIndex = orders.indexOf(order);
    const tr = document.createElement('tr');

    const idTd = document.createElement('td');
    idTd.textContent = String(idx + 1);

    const itemTd = document.createElement('td');
    itemTd.textContent = order.item;

    const nameTd = document.createElement('td');
    nameTd.textContent = order.itemName;

    const qtyTd = document.createElement('td');
    qtyTd.textContent = String(order.quantity);

    const priceTd = document.createElement('td');
    priceTd.textContent = order.formattedPrice;

    const currencyTd = document.createElement('td');
    currencyTd.textContent = order.currency;

    const actionTd = document.createElement('td');
    actionTd.className = 'action-cell';

    const delBtn = document.createElement('button');
    delBtn.type = 'button';
    delBtn.className = 'btn-delete';
    delBtn.title = `Delete ${order.itemName} (${order.item})`;
    delBtn.innerHTML = `
      <svg width="15" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
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
 * Question 3: Allows inserting an order into order.json (4 Marks)
 */
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
  orderForm.reset();
  currencyInput.value = 'USD';
  renderOrdersTable();
  showStatus(`Order "${newOrder.itemName}" (${newOrder.item}) inserted into order.json.`);
}

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
  showStatus(`Deleted "${target.itemName}" (${target.item}) from order.json.`);
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
downloadJsonBtn.addEventListener('click', downloadOrderJsonFile);

// Initialize on load
initializeOrders();
