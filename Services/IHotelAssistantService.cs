namespace SUNRISEHOTEL.Services
{
    public interface IHotelAssistantService
    {
        Task<string> ChatAsync(string message);
    }
}
