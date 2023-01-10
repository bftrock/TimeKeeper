namespace TimeKeeper;

internal interface IDataStore
{
    void CreateStore();
    void GetWeekTimeWorked(WeekTimeWorked wtw);
    void SaveWeekTimeWorked(WeekTimeWorked wtw);
    Task[] GetAllTasks();
    void AddNewTask(Task task);
    void DeleteTask(int taskId);
}
