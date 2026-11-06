using System.ComponentModel.DataAnnotations;

namespace SunriseHotel.API.DTOs;

public sealed record AskHotelRequest([Required, StringLength(2000)] string Question);
public sealed record ParseBookingRequest([Required, StringLength(2000)] string Text);
public sealed record AssistantChatRequest([Required, StringLength(2000)] string Message);
public sealed record AiAnswer(string Answer);

public sealed record ParsedBooking(
    string? GuestName, string? Phone, string? RoomType,
    DateOnly? CheckIn, DateOnly? CheckOut, int? Guests);

public sealed record BookingResult(
    int BookingId, int RoomId, string RoomNumber, string RoomType, string GuestName,
    string Phone, DateOnly CheckIn, DateOnly CheckOut, int Guests, int Nights,
    decimal TotalPrice, string Status);

public sealed record BookingReportRow(
    int No, string RoomNumber, string RoomType, string GuestName,
    DateOnly CheckIn, DateOnly CheckOut, int Nights, decimal TotalPrice);

public sealed record BookingReportView(
    DateOnly? From, DateOnly? To, IReadOnlyList<BookingReportRow> Rows, string? Error = null);
