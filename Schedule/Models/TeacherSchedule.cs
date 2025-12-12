namespace Schedule.Models;

public class TeacherSchedule
{
    public List<DaySchedule> Days { get; private set; }
    public int TeacherId { get; private set; }

    public TeacherSchedule(int teacherId, List<DaySchedule> days)
    {
        TeacherId = teacherId;
        Days = days;
    }
}