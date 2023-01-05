namespace TimeKeeper;

internal class Task
{
    public string Name { get; set; } = "TaskName";
    public string Code { get; set; } = "";
    public int Id { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
