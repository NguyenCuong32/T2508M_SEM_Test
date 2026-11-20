import 'package:material_ui/material_ui.dart';

import 'core/constants/app_constants.dart';
import 'features/order/data/datasources/order_local_datasource.dart';
import 'features/order/data/repositories/order_repository.dart';
import 'features/order/presentation/controllers/order_controller.dart';
import 'features/order/presentation/screens/orders_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  final repository = OrderRepository(datasource: const OrderLocalDatasource());
  runApp(MyOrderApp(repository: repository));
}

class MyOrderApp extends StatelessWidget {
  const MyOrderApp({super.key, required this.repository});

  final OrderRepository repository;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: AppConstants.title,
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: ColorScheme.fromSeed(seedColor: AppConstants.accent),
        scaffoldBackgroundColor: AppConstants.canvas,
        appBarTheme: const AppBarTheme(
          backgroundColor: AppConstants.canvas,
          surfaceTintColor: Colors.transparent,
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: Colors.white,
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(14),
            borderSide: const BorderSide(color: AppConstants.outline),
          ),
          enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(14),
            borderSide: const BorderSide(color: AppConstants.outline),
          ),
          contentPadding: const EdgeInsets.all(16),
        ),
        filledButtonTheme: FilledButtonThemeData(
          style: FilledButton.styleFrom(
            backgroundColor: AppConstants.accent,
            foregroundColor: Colors.white,
            minimumSize: const Size(48, 52),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(14),
            ),
          ),
        ),
      ),
      home: OrdersScreen(createController: () => OrderController(repository)),
    );
  }
}
