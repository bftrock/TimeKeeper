using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace TimeKeeper;

internal class Tasks : List<Task>
{
    private int _MaxId = 0;

    public string ConnStr { get; set; } = "Data Source=C:\\Users\\bftro\\AppData\\Local\\TimeKeeper\\TimeKeeper.sqlite";

    public int MaxId { get => _MaxId; }

    public void LoadFromDatabase()
    {
        string sql = "SELECT id, name, code FROM tasks ORDER BY id";
        using var connection = new SqliteConnection(ConnStr);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        _MaxId = 0;
        while (reader.Read())
        {
            Task task = new()
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Code = reader.IsDBNull(2) ? "" : reader.GetString(2),
            };
            Add(task);
            _MaxId = Math.Max(_MaxId, task.Id);
        }
    }
}
