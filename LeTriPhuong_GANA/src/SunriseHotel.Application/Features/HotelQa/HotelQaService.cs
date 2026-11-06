using SunriseHotel.Application.Abstractions.Ai;
using SunriseHotel.Application.Common.Exceptions;
using SunriseHotel.Application.Prompts;

namespace SunriseHotel.Application.Features.HotelQa;

public sealed record AskHotelResponse(string Question, string Answer);

public interface IHotelQaService
{
    Task<AskHotelResponse> AskAsync(string question, CancellationToken ct = default);
}

/// <summary>Requirement 1: answers guest questions using a receptionist system instruction.</summary>
public sealed class HotelQaService(IGenerativeAiClient ai) : IHotelQaService
{
    public const int MaxQuestionLength = 1000;

    public async Task<AskHotelResponse> AskAsync(string question, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ValidationException("Question is required.");
        if (question.Length > MaxQuestionLength)
            throw new ValidationException($"Question must not exceed {MaxQuestionLength} characters.");

        var answer = await ai.GenerateTextAsync(HotelPrompts.ReceptionistSystemInstruction, question.Trim(), ct);
        return new AskHotelResponse(question.Trim(), answer.Trim());
    }
}
