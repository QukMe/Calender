using System;

namespace Schedule.Models
{
    public class Lesson
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Type { get; private set; }
        public string Room { get; private set; }
        public string Address { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public string Groups { get; private set; }

        public Lesson(int id, string title, string type, string room, string address, TimeSpan start, TimeSpan end,
            string groups)
        {
            Id = id;
            Title = title;
            Type = type;
            Room = room;
            Address = address;
            StartTime = start;
            EndTime = end;
            Groups = groups;
        }

        public override string ToString()
        {
            return $"{StartTime:hh\\:mm} - {Title} ({Room})";
        }
    }
}