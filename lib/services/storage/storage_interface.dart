abstract class StorageInterface {
  Future<String> readOrderJson();
  Future<void> writeOrderJson(String content);
}

