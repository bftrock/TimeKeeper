using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;

namespace TimeKeeper;

internal class Database : IDataStore
{
    public Database()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string dbFolderPath = Path.Combine(appDataPath, "TimeKeeper");
        if (!Directory.Exists(dbFolderPath))
        {
            Directory.CreateDirectory(dbFolderPath);
        }
        DatabasePath = Path.Combine(dbFolderPath, "TimeKeeper.db");
        ConnectionString = new SqliteConnectionStringBuilder()
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true,
        }.ToString();
    }

    public string DatabasePath { get; }

    public string ConnectionString { get; }

    public void AddNewTask(Task task)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using SqliteCommand? command1 = connection.CreateCommand();
        command1.CommandText = "SELECT MAX(id) FROM tasks";
        using SqliteDataReader? reader = command1.ExecuteReader();
        int maxId = 1;
        while(reader.Read())
        {
            if (!reader.IsDBNull(0))
            {
                maxId = reader.GetInt32(0);
                maxId++;
            }
        }
        using SqliteCommand? command2 = connection.CreateCommand();
        command2.CommandText = "INSERT INTO tasks (id, name, code) VALUES ($id, $name, $code)";
        command2.Parameters.AddWithValue("$id", maxId);
        command2.Parameters.AddWithValue("$name", task.Name);
        command2.Parameters.AddWithValue("$code", task.Code);
        command2.ExecuteNonQuery();
    }

    public void CreateStore()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        var command1 = connection.CreateCommand();
        command1.CommandText = 
            "CREATE TABLE IF NOT EXISTS tasks (" +
            "id INTEGER PRIMARY KEY," +
            "name TEXT," +
            "code TEXT)";
        command1.ExecuteNonQuery();
        var command2 = connection.CreateCommand();
        command2.CommandText = 
            "CREATE TABLE IF NOT EXISTS time (" +
            "task_id INTEGER," +
            "date TEXT," +
            "time_worked REAL," +
            "PRIMARY KEY(task_id, date)" +
            "FOREIGN KEY(task_id) REFERENCES tasks(id))";
        command2.ExecuteNonQuery();
    }

    public void DeleteTask(int taskId)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using SqliteCommand? command = connection.CreateCommand();
        command.CommandText = "DELETE FROM tasks WHERE tasks.id = $id";
        command.Parameters.AddWithValue("$id", taskId);
        command.ExecuteNonQuery();
    }

    public Task[] GetAllTasks()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using SqliteCommand? command = connection.CreateCommand();
        command.CommandText = "SELECT id, name, code FROM tasks ORDER BY name";
        using SqliteDataReader reader = command.ExecuteReader();
        List<Task> tasks = new();
        while (reader.Read())
        {
            tasks.Add(new Task()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Code = reader.IsDBNull(2) ? "" : reader.GetString(2),
            });
        }
        return tasks.ToArray();
    }

    public void GetWeekTimeWorked(WeekTimeWorked wtw)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using SqliteCommand? command = connection.CreateCommand();
        command.CommandText =
            "SELECT tasks.code, tasks.id, tasks.name, time.date, time.time_worked " +
            "FROM tasks JOIN time ON tasks.id = time.task_id " +
            "WHERE time.date >= $start AND time.date < $end " +
            "ORDER BY tasks.code, tasks.name";
        command.Parameters.AddWithValue("$start", wtw.WeekCommencing.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("$end", wtw.WeekCommencing.AddDays(7).ToString("yyyy-MM-dd"));
        using var reader = command.ExecuteReader();
        wtw.Tasks.Clear();
        while (reader.Read())
        {
            string taskCode = reader.GetString(reader.GetOrdinal("code"));
            string taskName = reader.GetString(reader.GetOrdinal("name"));
            string fullTaskName;
            if (taskCode.Length > 0)
            {
                fullTaskName = $"({taskCode}) {taskName}";
            }
            else
            {
                fullTaskName = taskName;
            }
            int taskId = reader.GetInt32(reader.GetOrdinal("id"));
            TaskTimeWorked ttw;
            if (wtw.Tasks.ContainsKey(taskId))
            {
                ttw = wtw.Tasks[taskId];
            }
            else
            {
                ttw = new()
                {
                    TaskId = taskId,
                    TaskName = fullTaskName,
                };
                wtw.Tasks.Add(ttw.TaskId, ttw);
            }
            ttw.AddTimeWorked(
                DateTime.Parse(reader.GetString(reader.GetOrdinal("date"))),
                reader.GetDouble(reader.GetOrdinal("time_worked")));
        }
    }

    public void SaveWeekTimeWorked(WeekTimeWorked wtw)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using SqliteCommand? command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO time (task_id, date, time_worked) " +
            "VALUES ($tid, $dt, $tw) " +
            "ON CONFLICT (task_id, date) DO UPDATE SET time_worked = excluded.time_worked"; ;
        command.Parameters.AddWithValue("$tid", 1);
        command.Parameters.AddWithValue("$dt", "");
        command.Parameters.AddWithValue("$tw", 1.1);
        foreach (var kvp in wtw.Tasks)
        {
            command.Parameters["$tid"].Value = kvp.Key;
            for (int j = 0; j < 7; j++)
            {
                double timeWorked = kvp.Value.GetTimeWorked((DayOfWeek)j);
                command.Parameters["$tw"].Value = timeWorked;
                DateTime taskDate = wtw.WeekCommencing.AddDays(j);
                command.Parameters["$dt"].Value = taskDate.ToString("yyyy-MM-dd");
                command.ExecuteNonQuery();
            }
        }
    }
}
