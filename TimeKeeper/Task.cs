using System;
using System.Linq;

namespace TimeKeeper;

public class Task
{
    private readonly double[] TimeWorked = new double[7];

    private DateTime? TimedTaskLastUpdateTime;

    public int Id { get; set; }

    public string Name { get; set; } = "TaskName";

    public string Code { get; set; } = "";

    public string FullName
    {
        get
        {
            if (Code.Length > 0)
            {
                return $"({Code}) {Name}";
            }
            else
            {
                return Name;
            }
        }
    }

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

    public override string ToString()
    {
        return Name;
    }
}
