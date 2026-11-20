import 'dart:convert';
import 'dart:io';

import 'package:flutter/services.dart';
import 'package:path_provider/path_provider.dart';

import 'package:my_order/core/constants/app_constants.dart';

import '../models/order_model.dart';

class OrderLocalDatasource {
  const OrderLocalDatasource({this.directory});

  final Directory? directory;

  Future<File> get file async {
    final folder = directory ?? await getApplicationDocumentsDirectory();
    await folder.create(recursive: true);
    return File(
      '${folder.path}${Platform.pathSeparator}${AppConstants.orderFileName}',
    );
  }

  Future<List<Order>> load() async {
    final target = await file;
    if (!await target.exists()) {
      final source = await rootBundle.loadString(AppConstants.orderAssetPath);
      final orders = decode(source);
      await save(orders);
      return orders;
    }
    return decode(await target.readAsString());
  }

  Future<void> save(List<Order> orders) async {
    final target = await file;
    final temporary = File('${target.path}.tmp');
    await temporary.writeAsString(encode(orders), flush: true);
    await temporary.rename(target.path);
  }

  static List<Order> decode(String source) {
    final decoded = jsonDecode(source);
    if (decoded is! List) {
      throw const FormatException('Expected an order list');
    }
    return decoded.map((value) {
      if (value is! Map<String, dynamic>) {
        throw const FormatException('Invalid order');
      }
      return Order.fromJson(value);
    }).toList();
  }

  static String encode(List<Order> orders) =>
      const JsonEncoder.withIndent('  ')
          .convert(orders.map((o) => o.toJson()).toList());
}
