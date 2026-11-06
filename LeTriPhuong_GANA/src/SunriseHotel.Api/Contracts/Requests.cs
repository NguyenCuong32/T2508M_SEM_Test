using System.ComponentModel.DataAnnotations;

namespace SunriseHotel.Api.Contracts;

public sealed record AskHotelRequest([Required, StringLength(1000, MinimumLength = 1)] string Question);

public sealed record ParseBookingRequest([Required, StringLength(2000, MinimumLength = 1)] string Text);

public sealed record AssistantChatRequest([Required, StringLength(1000, MinimumLength = 1)] string Message);
