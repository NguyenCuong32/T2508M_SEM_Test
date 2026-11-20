import 'dart:io';

import '../datasources/order_local_datasource.dart';
import '../models/order_model.dart';

class OrderRepository {
  OrderRepository({Directory? directory, OrderLocalDatasource? datasource})
    : _datasource = datasource ?? OrderLocalDatasource(directory: directory);

  final OrderLocalDatasource _datasource;

  Future<List<Order>> load() => _datasource.load();

  Future<void> save(List<Order> orders) => _datasource.save(orders);
}
