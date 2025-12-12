using System;
using Schedule.Models;

namespace Schedule.Services
{
    public interface IScheduleLoader
    {
        TeacherSchedule LoadSchedule(int teacherId, DateTime start, DateTime end);
    }
}