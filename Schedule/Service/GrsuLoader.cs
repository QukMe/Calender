using Newtonsoft.Json;
using Schedule.Models;

namespace Schedule.Services
{
    public class GrsuLoader : IScheduleLoader
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<List<Teacher>> GetTeachersAsync()
        {
            string url = "https://api.grsu.by/1.x/app2/getTeachers";
            string json = await client.GetStringAsync(url);
            var response = JsonConvert.DeserializeObject<TeacherListResponse>(json);
            if (response != null)
            {
                return response.Items;
            }
            else
            {
                return new List<Teacher>();
            }
        }

        public TeacherSchedule LoadSchedule(int teacherId, DateTime start, DateTime end)
        {
            string startStr = start.ToString("dd.MM.yyyy");
            string endStr = end.ToString("dd.MM.yyyy");
            string url =
                $"http://api.grsu.by/1.x/app2/getTeacherSchedule?teacherId={teacherId}&dateStart={startStr}&dateEnd={endStr}";

            HttpResponseMessage response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            string json = response.Content.ReadAsStringAsync().Result;

            JsonDtoRoot dto = JsonConvert.DeserializeObject<JsonDtoRoot>(json);
            List<DaySchedule> cleanDays = new List<DaySchedule>();

            if (dto.Days != null)
            {
                foreach (var dtoDay in dto.Days)
                {
                    List<Lesson> cleanLessons = new List<Lesson>();
                    HashSet<string> addedPairs = new HashSet<string>();

                    if (dtoDay.Lessons != null)
                    {
                        foreach (var dtoLesson in dtoDay.Lessons)
                        {
                            string title = dtoLesson.Title?.Trim();
                            string timeStart = dtoLesson.TimeStart?.Trim();

                            string uniqueKey = $"{timeStart}_{title}";

                            if (addedPairs.Contains(uniqueKey))
                            {
                                continue;
                            }

                            TimeSpan.TryParse(timeStart, out TimeSpan tStart);
                            TimeSpan.TryParse(dtoLesson.TimeEnd, out TimeSpan tEnd);

                            string groupsString = "";
                            if (dtoLesson.Groups != null && dtoLesson.Groups.Count > 0)
                            {
                                var groupNames = dtoLesson.Groups.Select(g => g.Title);
                                groupsString = string.Join(", ", groupNames);
                            }

                            Lesson lesson = new Lesson(
                                dtoLesson.Id, title, dtoLesson.Type,
                                dtoLesson.Room, dtoLesson.Address, tStart, tEnd,
                                groupsString
                            );
                            cleanLessons.Add(lesson);
                            addedPairs.Add(uniqueKey);
                        }
                    }

                    cleanDays.Add(new DaySchedule(dtoDay.Date, cleanLessons));
                }
            }

            return new TeacherSchedule(teacherId, cleanDays);
        }

        private class JsonDtoRoot
        {
            [JsonProperty("days")] public List<JsonDtoDay> Days { get; set; }
        }

        private class JsonDtoDay
        {
            [JsonProperty("date")] public DateTime Date { get; set; }
            [JsonProperty("lessons")] public List<JsonDtoLesson> Lessons { get; set; }
        }

        private class JsonDtoLesson
        {
            [JsonProperty("id")] public int Id { get; set; }
            [JsonProperty("timeStart")] public string TimeStart { get; set; }
            [JsonProperty("timeEnd")] public string TimeEnd { get; set; }
            [JsonProperty("title")] public string Title { get; set; }
            [JsonProperty("type")] public string Type { get; set; }
            [JsonProperty("address")] public string Address { get; set; }
            [JsonProperty("room")] public string Room { get; set; }

            [JsonProperty("groups")] public List<JsonDtoGroup> Groups { get; set; }
        }

        private class JsonDtoGroup
        {
            [JsonProperty("title")] public string Title { get; set; }
        }
    }
}