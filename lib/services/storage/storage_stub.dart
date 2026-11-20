import 'storage_interface.dart';

class InMemoryStorage implements StorageInterface {
  String _data = '';

  @override
  Future<String> readOrderJson() async {
    return _data;
  }

  @override
  Future<void> writeOrderJson(String content) async {
    _data = content;
  }
}

StorageInterface createStorage() => InMemoryStorage();

