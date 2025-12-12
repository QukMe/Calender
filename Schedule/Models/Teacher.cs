using Newtonsoft.Json;

namespace Schedule.Models
{
    public class Teacher
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("surname")] public string Surname { get; set; }

        [JsonProperty("patronym")] public string Patronym { get; set; }
        
        public string FullName
        {
            get { return Surname + " " + Name + " " + Patronym; }
        }

        public override string ToString()
        {
            return FullName;
        }
    }

    public class TeacherListResponse
    {
        [JsonProperty("items")] public List<Teacher> Items { get; set; }
    }
}