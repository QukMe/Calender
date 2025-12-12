using Google.Apis.Calendar.v3.Data;
using Schedule.Models;

namespace Schedule.Services
{
    public interface ICloudCalendarService
    {
        Task<string> AddEventAsync(Lesson lesson, DateTime date);

        Task<Dictionary<string, Event>> GetExistingEventsAsync(DateTime start, DateTime end);

        Task UpdateEventAsync(Event googleEvent, Lesson lesson, DateTime date);

        Task DeleteEventAsync(string googleEventId);
    }
}