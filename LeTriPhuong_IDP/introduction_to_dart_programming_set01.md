# FPT Aptech® - Unleash your potential

**Địa chỉ:** Số 8, Tôn Thất Thuyết, Cầu Giấy, Hà Nội  
**Môn thi:** Introduction to Dart Programming - Exam Paper (SET01)  
**Thời gian:** 60 minutes | **Tổng điểm:** 15 Marks

---

## Question:

### Giao diện minh họa (My Order)

#### Form nhập liệu:
* **Item:** `[ Input ]`
* **Item Name:** `[ Input ]`
* **Price:** `[ Input ]`
* **Quantity:** `[ Input ]`
* **Currency:** `[ USD ]`
* **Nút chức năng:** `[ Add Item to Cart ]`

#### Bảng danh sách đơn hàng:

| Id | Item | Item Name | Quantity | Price | Currency | Action |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | A1000 | Iphone 15 | 1 | 1.200 | USD | 🗑️ |
| 1 | A1001 | Iphone 15 | 1 | 1.200 | USD | 🗑️ |
| 1 | A1002 | Iphone 15 | 1 | 1.200 | USD | 🗑️ |
| 1 | A1003 | Iphone 15 | 1 | 1.200 | USD | 🗑️ |
| 1 | A1004 | Iphone 15 | 1 | 1.200 | USD | 🗑️ |
| 1 | A1005 | Iphone 15 | 1 | 1.200 | USD | 🗑️ |
| 1 | A1006 | Iphone 15 | 1 | 1.200 | USD | 🗑️ |
| 1 | A1007 | Iphone 15 | 1 | 1.200 | USD | 🗑️ |
| 1 | A1008 | Iphone 15 | 1 | 1.200 | USD | 🗑️ |

---

### Yêu cầu đề bài (Requirements)

Give the string json with structure as below. In the string is a list including some items which are ordered.

1. Create a class with the name `"Order"`.
2. Read the content from the string and save objects to list `orders`.
3. Create a website (example as picture) to show all objects in `orders`.
4. Allows inserting an order into `order.json`.
5. Allows search Order by `ItemName`.

**Dữ liệu JSON mẫu:**
```json
[
  {
    "Item": "A1000",
    "ItemName": "Iphone 15",
    "Price": 1200,
    "Currency": "USD",
    "Quantity": 1
  },
  {
    "Item": "A1001",
    "ItemName": "Iphone 16",
    "Price": 1500,
    "Currency": "USD",
    "Quantity": 1
  }
]
```

---

### Marking Schema (Thang điểm)

| Question | Mark |
| :--- | :---: |
| Create class Order | 2 |
| Read string and save to objects, display list of orders to screen. | 5 |
| Insert order | 4 |
| Search order. | 3 |
| Bonus good UI | 1 |
| **Total** | **15** |

---
*Introduction to Dart Programming - Set01 Practical Paper*