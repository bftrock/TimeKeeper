using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeKeeper;

internal class WeekTimeWorked
{
    public string ConnStr { get; set; } = "Data Source=C:\\Users\\bftro\\AppData\\Local\\TimeKeeper\\TimeKeeper.sqlite";

    private DateTime _WeekCommencing = GetWeekCommencing(DateTime.Now);
    public DateTime WeekCommencing
    {
        get => _WeekCommencing;
        set
        {
            _WeekCommencing = GetWeekCommencing(value);
            LoadFromDatabase();
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

    public void LoadFromDatabase()
    {
        string sql =
            "SELECT tasks.name, time.date, time.time_worked, tasks.id " +
            "FROM tasks JOIN time ON tasks.id = time.task_id " +
            "WHERE time.date >= $start AND time.date < $end";
        using var connection = new SqliteConnection(ConnStr);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$start", WeekCommencing.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$end", WeekCommencing.AddDays(7).ToString("yyyy-MM-dd"));
        using var reader = command.ExecuteReader();
        _Tasks.Clear();
        while (reader.Read())
        {
            var taskId = reader.GetInt32(3);
            TaskTimeWorked ttw;
            if (Tasks.ContainsKey(taskId))
            {
                ttw = Tasks[taskId];
            }
            else
            {
                ttw = new()
                {
                    TaskName = reader.GetString(0),
                    TaskId = taskId,
                };
                Tasks[taskId] = ttw;
            }
            DateTime dt = DateTime.Parse(reader.GetString(1));
            double timeWorked = reader.GetDouble(2);
            ttw.SetTimeWorked(dt, timeWorked);
        }
    }

    public void SaveToDatabase()
    {
        string sql = 
            "INSERT INTO time (task_id, date, time_worked) " +
            "VALUES ($tid, $dt, $tw) " +
            "ON CONFLICT (task_id, date) DO UPDATE SET time_worked = excluded.time_worked";
        using var connection = new SqliteConnection(ConnStr);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$tid", 1);
        command.Parameters.AddWithValue("$dt", "");
        command.Parameters.AddWithValue("$tw", 1.1);
        foreach (var kvp in Tasks)
        {
            command.Parameters["$tid"].Value = kvp.Key;
            for (int j = 0; j < 7; j++)
            {
                double timeWorked = kvp.Value.GetTimeWorked((DayOfWeek)j);
                if (timeWorked > 0)
                {
                    command.Parameters["$tw"].Value = timeWorked;
                    DateTime taskDate = WeekCommencing.AddDays(j);
                    command.Parameters["$dt"].Value = taskDate.ToString("yyyy-MM-dd");
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public static DateTime GetWeekCommencing(DateTime dt)
    {
        return dt.AddDays(-(int)dt.DayOfWeek).Date;
    }
}
