using System.Text;
using Schedule.Models;

namespace Schedule.Services
{
    public class SyncService
    {
        private readonly IScheduleLoader _uniLoader;
        private readonly ICloudCalendarService _cloudService;

        public SyncService(IScheduleLoader uniLoader, ICloudCalendarService cloudService)
        {
            _uniLoader = uniLoader;
            _cloudService = cloudService;
        }

        public async Task<string> SyncAsync(int teacherId, DateTime requestedStart, DateTime requestedEnd)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine($"Запрос: {requestedStart:d} - {requestedEnd:d}");

            TeacherSchedule schedule = _uniLoader.LoadSchedule(teacherId, requestedStart, requestedEnd);

            if (schedule.Days.Count == 0)
                return "Расписание пустое. Синхронизация не требуется.";

            DateTime realStart = schedule.Days[0].Date;
            DateTime realEnd = schedule.Days[schedule.Days.Count - 1].Date;

            DateTime searchStart = realStart.AddDays(-1);
            DateTime searchEnd = realEnd.AddDays(1);

            var existingEvents = await _cloudService.GetExistingEventsAsync(searchStart, searchEnd);

            int added = 0;
            int updated = 0;
            int deleted = 0;

            HashSet<string> validLessonIds = new HashSet<string>();

            foreach (var day in schedule.Days)
            {
                foreach (var lesson in day.Lessons)
                {
                    string lessonId = lesson.Id.ToString();

                    validLessonIds.Add(lessonId);

                    if (existingEvents.ContainsKey(lessonId))
                    {
                        await _cloudService.UpdateEventAsync(existingEvents[lessonId], lesson, day.Date);
                        updated++;
                    }
                    else
                    {
                        await _cloudService.AddEventAsync(lesson, day.Date);
                        added++;
                    }
                }
            }

            foreach (var pair in existingEvents)
            {
                string uniIdInGoogle = pair.Key;

                if (!validLessonIds.Contains(uniIdInGoogle))
                {
                    await _cloudService.DeleteEventAsync(pair.Value.Id);
                    deleted++;
                }
            }

            report.AppendLine($"Итог: {added} (Новых) | {updated} (Обновлено) | {deleted} (Удалено)");
            return report.ToString();
        }
    }
}