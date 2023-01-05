using System;
using System.Linq;

namespace TimeKeeper;

internal class TaskTimeWorked
{
    private readonly double[] TimeWorked = new double[7];

    public string TaskName { get; set; } = "TaskName";

    public int TaskId { get; set; }

    public double Sunday
    {
        get => TimeWorked[0];
        set
        {
            TimeWorked[0] = value;
        }
    }

    public double Monday
    {
        get => TimeWorked[1];
        set
        {
            TimeWorked[1] = value;
        }
    }

    public double Tuesday
    {
        get => TimeWorked[2];
        set
        {
            TimeWorked[2] = value;
        }
    }

    public double Wednesday
    {
        get => TimeWorked[3];
        set
        {
            TimeWorked[3] = value;
        }
    }

    public double Thursday
    {
        get => TimeWorked[4];
        set
        {
            TimeWorked[4] = value;
        }
    }

    public double Friday
    {
        get => TimeWorked[5];
        set
        {
            TimeWorked[5] = value;
        }
    }

    public double Saturday
    {
        get => TimeWorked[6];
        set
        {
            TimeWorked[6] = value;
        }
    }

    public double WeekTotal
    {
        get => TimeWorked.Sum();
    }

    public double GetTimeWorked(DayOfWeek dow)
    {
        return TimeWorked[(int)dow];
    }

    public void SetTimeWorked(DateTime dt, double timeWorked)
    {
        TimeWorked[(int)dt.DayOfWeek] = timeWorked;
    }

    public void SetTimeWorked(DayOfWeek dow, double timeWorked)
    {
        TimeWorked[(int)dow] = timeWorked;
    }

    public void AddTimeWorked(DateTime dt, double timeWorked)
    {
        TimeWorked[(int)dt.DayOfWeek] += timeWorked;
    }
}
