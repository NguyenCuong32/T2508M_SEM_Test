/// Formats numeric prices with thousands separators (e.g., 1200 -> "1,200").
String formatPrice(num value) {
  final bool isNegative = value < 0;
  final double absValue = value.abs().toDouble();
  final bool isWholeNumber = absValue == absValue.roundToDouble();

  final String raw = isWholeNumber
      ? absValue.round().toString()
      : absValue.toStringAsFixed(2).replaceFirst(RegExp(r'\.?0+$'), '');

  final List<String> parts = raw.split('.');
  final String intPart = parts[0];
  final StringBuffer formattedInt = StringBuffer();

  for (int i = 0; i < intPart.length; i++) {
    final int remaining = intPart.length - i;
    if (i > 0 && remaining % 3 == 0) {
      formattedInt.write(',');
    }
    formattedInt.write(intPart[i]);
  }

  final String prefix = isNegative ? '-' : '';
  if (parts.length > 1 && parts[1].isNotEmpty) {
    return '$prefix$formattedInt.${parts[1]}';
  }
  return '$prefix$formattedInt';
}
