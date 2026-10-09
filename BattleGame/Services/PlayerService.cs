using System.ComponentModel.DataAnnotations;
using BattleGame.DTOs;
using BattleGame.Models;
using BattleGame.Repositories.Interfaces;
using BattleGame.Services.Interfaces;

namespace BattleGame.Services;

public sealed class PlayerService(IPlayerRepository repository) : IPlayerService
{
    public async Task<Player> RegisterPlayerAsync(RegisterPlayerDto dto)
    {
        var playerName = InputValidation.RequiredText(dto.PlayerName, "PlayerName", 64);
        var fullName = InputValidation.RequiredText(dto.FullName, "FullName", 128);
        var age = InputValidation.RequiredText(dto.Age, "Age", 10);
        var email = InputValidation.RequiredText(dto.Email, "Email", 64).ToLowerInvariant();
        if (dto.Level < 0)
            throw new ValidationException("Level must be greater than or equal to zero.");
        if (!new EmailAddressAttribute().IsValid(email))
            throw new ValidationException("Email is not valid.");
        if (await repository.GetByEmailAsync(email) is not null)
            throw new DuplicateEmailException();
        return await repository.AddAsync(new Player
        {
            PlayerId = Guid.NewGuid(), PlayerName = playerName, FullName = fullName,
            Age = age, Level = dto.Level, Email = email
        });
    }
}
