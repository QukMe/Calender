namespace Schedule.Models;

public class DaySchedule
{
    public List<Lesson> Lessons { get; private set; }
    public DateTime Date { get; private set; }

    public DaySchedule(DateTime date, List<Lesson> lessons)
    {
        Date = date;
        Lessons = lessons;
    }
}