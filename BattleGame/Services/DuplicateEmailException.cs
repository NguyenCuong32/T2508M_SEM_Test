namespace BattleGame.Services;

public sealed class DuplicateEmailException() : Exception("Email is already registered.");
