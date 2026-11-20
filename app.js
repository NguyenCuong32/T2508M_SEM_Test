const initialJsonString = '[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": 1500,"Currency": "USD","Quantity":1}]';

let orders = [];

function loadOrders() {
  const savedData = localStorage.getItem('order.json');
  const jsonSource = (savedData && savedData.length > 0) ? savedData : initialJsonString;
  orders = JSON.parse(jsonSource);
  saveToOrderJson();
}

function saveToOrderJson() {
  localStorage.setItem('order.json', JSON.stringify(orders));
}

function insertOrder() {
  const itemInput = document.getElementById('item');
  const itemNameInput = document.getElementById('itemName');
  const priceInput = document.getElementById('price');
  const quantityInput = document.getElementById('quantity');
  const currencyInput = document.getElementById('currency');

  const item = itemInput.value.trim();
  const itemName = itemNameInput.value.trim();
  const price = parseFloat(priceInput.value.trim());
  const quantity = parseInt(quantityInput.value.trim(), 10);
  const currency = currencyInput.value.trim() || 'USD';

  if (!item || !itemName || isNaN(price) || isNaN(quantity)) {
    alert('Vui lòng nhập đầy đủ và đúng định dạng các trường!');
    return;
  }

  orders.push({
    Item: item,
    ItemName: itemName,
    Price: price,
    Currency: currency,
    Quantity: quantity
  });

  saveToOrderJson();

  itemInput.value = '';
  itemNameInput.value = '';
  priceInput.value = '';
  quantityInput.value = '';
  currencyInput.value = 'USD';

  searchOrders();
}

function deleteOrder(index) {
  orders.splice(index, 1);
  saveToOrderJson();
  searchOrders();
}

function searchOrders() {
  const keyword = document.getElementById('searchInput').value.trim().toLowerCase();
  if (!keyword) {
    renderTable(orders);
  } else {
    const filtered = orders.filter(o => o.ItemName.toLowerCase().includes(keyword));
    renderTable(filtered);
  }
}

function renderTable(list) {
  const tbody = document.getElementById('orderTableBody');
  tbody.innerHTML = '';

  list.forEach((order, i) => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td>${i + 1}</td>
      <td>${order.Item}</td>
      <td>${order.ItemName}</td>
      <td>${order.Quantity}</td>
      <td>${Number(order.Price).toFixed(0)}</td>
      <td>${order.Currency}</td>
      <td><button class="btn-delete" onclick="deleteOrder(${orders.indexOf(order)})">🗑</button></td>
    `;
    tbody.appendChild(tr);
  });
}

window.addEventListener('DOMContentLoaded', () => {
  loadOrders();
  renderTable(orders);
  document.getElementById('addBtn').addEventListener('click', insertOrder);
  document.getElementById('searchInput').addEventListener('input', searchOrders);
});