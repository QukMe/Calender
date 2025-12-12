using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Schedule.Models;

namespace Schedule.Services
{
    public class GoogleCalendarService : ICloudCalendarService
    {
        private static string[] Scopes = { CalendarService.Scope.Calendar };
        private static string ApplicationName = "Schedule";

        private const string TargetCalendarName = "ГрГУ Расписание";

        private CalendarService service;
        private string calendarId;

        public GoogleCalendarService()
        {
            Authenticate();
            calendarId = GetOrCreateCalendarId();
        }

        private void Authenticate()
        {
            UserCredential credential;
            string credPath = "token.json";
            var clientSecrets = GoogleClientSecrets.FromFile("credentials.json").Secrets;
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;

            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        private string GetOrCreateCalendarId()
        {
            var calendars = service.CalendarList.List().Execute().Items;
            if (calendars != null)
            {
                foreach (var cal in calendars)
                {
                    if (cal.Summary == TargetCalendarName)
                    {
                        return cal.Id;
                    }
                }
            }

            Calendar newCalendar = new Google.Apis.Calendar.v3.Data.Calendar
            {
                Summary = TargetCalendarName,
                TimeZone = "Europe/Minsk"
            };

            var createdCal = service.Calendars.Insert(newCalendar).Execute();
            return createdCal.Id;
        }

        public async Task<Dictionary<string, Event>> GetExistingEventsAsync(DateTime start, DateTime end)
        {
            var request = service.Events.List(calendarId);
            request.TimeMin = start;
            request.TimeMax = end;
            request.SingleEvents = true;

            Events events = await request.ExecuteAsync();

            var result = new Dictionary<string, Event>();

            if (events.Items != null)
            {
                foreach (var eventItem in events.Items)
                {
                    string desc = eventItem.Description ?? "";
                    if (desc.Contains("[ID:"))
                    {
                        int startIndex = desc.IndexOf("[ID:") + 4;
                        int endIndex = desc.IndexOf("]", startIndex);
                        if (endIndex > startIndex)
                        {
                            string idStr = desc.Substring(startIndex, endIndex - startIndex).Trim();
                            if (!result.ContainsKey(idStr)) result.Add(idStr, eventItem);
                        }
                    }
                }
            }

            return result;
        }

        public async Task<string> AddEventAsync(Lesson lesson, DateTime date)
        {
            Event newEvent = CreateEventObject(lesson, date);
            Event createdEvent = await service.Events.Insert(newEvent, calendarId).ExecuteAsync();
            await Task.Delay(200);
            return createdEvent.HtmlLink;
        }

        public async Task UpdateEventAsync(Event googleEvent, Lesson lesson, DateTime date)
        {
            Event updatedData = CreateEventObject(lesson, date);
            googleEvent.Summary = updatedData.Summary;
            googleEvent.Description = updatedData.Description;
            googleEvent.Start = updatedData.Start;
            googleEvent.End = updatedData.End;
            googleEvent.Location = updatedData.Location;
            await service.Events.Update(googleEvent, calendarId, googleEvent.Id).ExecuteAsync();
            await Task.Delay(200);
        }


        public async Task DeleteEventAsync(string googleEventId)
        {
            await service.Events.Delete(calendarId, googleEventId).ExecuteAsync();
            await Task.Delay(200);
        }

        private Event CreateEventObject(Lesson lesson, DateTime date)
        {
            DateTime startDt = date.Date + lesson.StartTime;
            DateTime endDt = date.Date + lesson.EndTime;

            TimeSpan minskOffset = TimeSpan.FromHours(3);
            DateTimeOffset startOffset = new DateTimeOffset(startDt, minskOffset);
            DateTimeOffset endOffset = new DateTimeOffset(endDt, minskOffset);

            string description = $"Группы: {lesson.Groups}\n" +
                                 $"[ID: {lesson.Id}]";

            return new Event()
            {
                Summary = $"{lesson.Title} ({lesson.Type})",
                Location = $"{lesson.Address}, ауд. {lesson.Room}",
                Description = description,
                Start = new EventDateTime() { DateTimeDateTimeOffset = startOffset },
                End = new EventDateTime() { DateTimeDateTimeOffset = endOffset },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>()
                    {
                        new EventReminder() { Method = "popup", Minutes = 5 }
                    }
                }
            };
        }
    }
}