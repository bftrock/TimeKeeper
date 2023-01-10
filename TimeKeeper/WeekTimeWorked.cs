using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeKeeper;

internal class WeekTimeWorked
{
    public WeekTimeWorked(IDataStore ds)
    {
        Database = ds;
    }

    public IDataStore Database { get; }

    private DateTime _WeekCommencing = GetWeekCommencing(DateTime.Now);
    public DateTime WeekCommencing
    {
        get => _WeekCommencing;
        set
        {
            _WeekCommencing = GetWeekCommencing(value);
            Database.GetWeekTimeWorked(this);
        }
    }

    private Dictionary<int, TaskTimeWorked> _Tasks = new();
    public Dictionary<int, TaskTimeWorked> Tasks
    {
        get => _Tasks;
        set
        {
            _Tasks = value;
        }
    }

    public double[] TaskTotals
    {
        get => Tasks.Select(kvp => kvp.Value.WeekTotal).ToArray();
    }

    public double[] DayTotals
    {
        get
        {
            double[] dayTotals = new double[7];
            foreach (var kvp in Tasks)
            {
                var task = kvp.Value;
                for (int j = 0; j < 7; j++)
                {
                    dayTotals[j] += task.GetTimeWorked((DayOfWeek)j);
                }
            }
            return dayTotals;
        }
    }

    public double WeekTotal
    {
        get => TaskTotals.Sum();
    }

    public void Save()
    {
        Database.SaveWeekTimeWorked(this);
    }

    public static DateTime GetWeekCommencing(DateTime dt)
    {
        return dt.AddDays(-(int)dt.DayOfWeek).Date;
    }
}
