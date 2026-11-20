import 'dart:io';
import 'storage_interface.dart';

class IoStorage implements StorageInterface {
  final String path;

  IoStorage({this.path = 'order.json'});

  @override
  Future<String> readOrderJson() async {
    try {
      final file = File(path);
      if (await file.exists()) {
        return await file.readAsString();
      }
    } catch (_) {}
    return '';
  }

  @override
  Future<void> writeOrderJson(String content) async {
    try {
      final file = File(path);
      await file.writeAsString(content);
    } catch (_) {}
  }
}

StorageInterface createStorage() => IoStorage();

