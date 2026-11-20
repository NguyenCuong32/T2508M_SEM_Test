# Orderly — Order Management

A Dart powered order management website based on the attached SET01 practical brief. It loads the supplied order JSON, searches by product name, and saves new orders to `order.json`.

## Run

Requires Dart SDK 3 or newer.

```powershell
dart compile js web/main.dart -o web/main.dart.js
dart run bin/server.dart
```

Open [http://localhost:8080](http://localhost:8080). The app stores new orders in the project-root `order.json` file.

To use another port, set `PORT` before starting the server:

```powershell
$env:PORT = '8081'
dart run bin/server.dart
```

## Project structure

- `bin/server.dart` — Dart `Order` model, JSON API, and web server.
- `web/main.dart` — Dart client logic compiled to JavaScript for the browser.
- `web/index.html` / `web/styles.css` — responsive website layout and styling.
- `order.json` — persistent order data, seeded with the two example orders.
