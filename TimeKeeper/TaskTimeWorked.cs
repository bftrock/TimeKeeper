using System;
using System.Linq;

namespace TimeKeeper;

internal class TaskTimeWorked
{
    private readonly double[] TimeWorked = new double[7];

    private DateTime? TimedTaskLastUpdateTime;

    public string TaskName { get; set; } = "TaskName";

    public int TaskId { get; set; }

    public double WeekTotal
    {
        get => TimeWorked.Sum();
    }

    public bool IsTiming { get => TimedTaskLastUpdateTime.HasValue; }

    public double GetTimeWorked(DayOfWeek dow)
    {
        return TimeWorked[(int)dow];
    }

    public void SetTimeWorked(DateTime dt, double timeWorked)
    {
        TimeWorked[(int)dt.DayOfWeek] = timeWorked;
    }

    public void AddTimeWorked(DateTime dt, double timeWorked)
    {
        TimeWorked[(int)dt.DayOfWeek] += timeWorked;
    }

    public void StartTiming(DateTime dt)
    {
        TimedTaskLastUpdateTime = dt;
    }

    public void StopTiming(DateTime dt)
    {
        if (TimedTaskLastUpdateTime.HasValue)
        {
            var hoursWorked = (dt - TimedTaskLastUpdateTime.Value).TotalHours;
            AddTimeWorked(dt, hoursWorked);
            TimedTaskLastUpdateTime = null;
        }
    }

    public void UpdateTiming(DateTime dt)
    {
        if (TimedTaskLastUpdateTime.HasValue)
        {
            var hoursWorked = (dt - TimedTaskLastUpdateTime.Value).TotalHours;
            AddTimeWorked(dt, hoursWorked);
            TimedTaskLastUpdateTime = dt;
        }
    }
}
