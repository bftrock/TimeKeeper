using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TimeKeeper;

public partial class MainWindow : Window
{
    private const string START_TIMER = "Start timer";
    private const string STOP_TIMER = "Stop timer";
    private const string ACTIVE_STYLE = "TimingActiveStyle";
    private const string INACTIVE_STYLE = "TimingInactiveStyle";
    private readonly WeekTimeWorked WeekTimeWorked;
    private readonly Task[] AvailableTasks;
    private Task? TimedTask;
    private Button? TimedTaskButton;
    private TextBox? TimedTaskTextBox;
    private DispatcherTimer? TaskTimer;
    private readonly IDataStore DataStore;

    public MainWindow()
    {
        InitializeComponent();
        DataStore = new Database();
        DataStore.CreateStore();
        WeekTimeWorked = new(DataStore);
        WeekCommencingPicker.SelectedDate = DateTime.Now;
        AvailableTasks = DataStore.GetAllTasks();
        AvailableTasksCmb.ItemsSource = AvailableTasks;
    }

    private void ClearGrid()
    {
        for (int i = 0; i < 10; i++)
        {
            var lbl1 = (Label)MainForm.FindName($"TaskName{i}");
            if (lbl1 != null)
            {
                lbl1.Content = "";
            }
            for (int j = 0; j < 7; j++)
            {
                var tb1 = (TextBox)MainForm.FindName($"Tb{i}{j}");
                if (tb1 != null)
                {
                    tb1.Text = "";
                    tb1.IsEnabled = false;
                }
            }
            var btn1 = (Button)MainForm.FindName($"TimerBtn{i}");
            if (btn1 != null)
            {
                btn1.IsEnabled = false;
            }
            var tb2 = (TextBlock)MainForm.FindName($"TaskSum{i}");
            if (tb2 != null)
            {
                tb2.Text = "";
            }
        }
        for (int j = 0; j < 7; j++)
        {
            var tb3 = (TextBlock)MainForm.FindName($"DaySum{j}");
            if (tb3 != null)
            {
                tb3.Text = "";
            }
        }
    }

    private void UpdateGrid()
    {
        void UpdateTextBox(string name, double value, int taskId)
        {
            var tb = (TextBox)MainForm.FindName(name);
            if (tb != null)
            {
                TimeSpan ts = TimeSpan.FromHours(value);
                tb.Text = FormatHoursAsTime(value);
                tb.IsEnabled = true;
                tb.Tag = taskId;
            }
        }

        int i, j;
        i = 0;
        var taskTotals = WeekTimeWorked.TaskTotals;
        foreach (var kvp in WeekTimeWorked.Tasks)
        {
            int taskId = kvp.Key;
            Task task = kvp.Value;
            var lbl1 = (Label)MainForm.FindName($"TaskName{i}");
            if (lbl1 != null)
            {
                lbl1.Content = task.FullName;
            }
            for (j = 0; j < 7; j++)
            {
                var timeWorked = task.GetTimeWorked((DayOfWeek)j);
                UpdateTextBox($"Tb{i}{j}", timeWorked, taskId);
            }
            var tb2 = (TextBlock)MainForm.FindName($"TaskSum{i}");
            if (tb2 != null)
            {
                tb2.Text = FormatHoursAsTime(taskTotals[i]);
            }
            var btn1 = (Button)MainForm.FindName($"TimerBtn{i}");
            if (btn1 != null)
            {
                btn1.IsEnabled = true;
                btn1.Tag = task;
            }
            i++;
        }
        WeekTotal.Text = FormatHoursAsTime(WeekTimeWorked.WeekTotal);
        var dayTotals = WeekTimeWorked.DayTotals;
        for (j = 0; j < 7; j++)
        {
            var tb3 = (TextBlock)MainForm.FindName($"DaySum{j}");
            if (tb3 != null)
            {
                tb3.Text = FormatHoursAsTime(dayTotals[j]);
            }
        }
    }

    private void WeekCommencingPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        var picker = (DatePicker)sender;
        if (picker != null && picker.SelectedDate.HasValue)
        {
            WeekTimeWorked.WeekCommencing = picker.SelectedDate.Value;
        }
        ClearGrid();
        UpdateGrid();
    }

    private void Tb_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            double hours;
            try
            {
                hours = ParseTimeEntry(tb.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                tb.SelectAll();
                return;
            }
            int col = int.Parse(tb.Name[^1..]);
            int taskId = (int)tb.Tag;
            var task = WeekTimeWorked.Tasks[taskId];
            DateTime dateWorked = WeekTimeWorked.WeekCommencing.AddDays(col);
            task.SetTimeWorked(dateWorked, hours);
            UpdateGrid();
            WeekTimeWorked.Save();
        }
    }

    private static double ParseTimeEntry(string entry)
    {
        string timePattern = @"(\d{0,2}):(\d{2})";
        string numberPattern = @"^\d*\.?\d+$";
        if (entry.Length == 0)
        {
            return 0;
        }
        Match matchTime = Regex.Match(entry, timePattern, RegexOptions.IgnoreCase);
        if (matchTime.Success)
        {
            int hours = 0;
            int minutes = 0;
            if (matchTime.Groups[1].Value.Length > 0)
            {
                hours = int.Parse(matchTime.Groups[1].Value);
            }
            if (matchTime.Groups[2].Value.Length > 0)
            {
                minutes = int.Parse(matchTime.Groups[2].Value);
            }
            TimeSpan span = new(hours, minutes, 0);
            return span.TotalHours;
        }
        Match matchDecimal = Regex.Match(entry, numberPattern, RegexOptions.IgnoreCase);
        if (matchDecimal.Success)
        {
            return double.Parse(entry);
        }
        else
        {
            throw new Exception("Invalid input format");
        }
    }

    private static string FormatHoursAsTime(double hours)
    {
        var ts = TimeSpan.FromMinutes(Math.Round(60 * hours));
        return hours > 0 ? $"{Math.Floor(ts.TotalHours):F0}:{ts.Minutes:00}" : "";
    }

    private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
    {
        if (AvailableTasksCmb.SelectedIndex >= 0)
        {
            if (AvailableTasksCmb.SelectedItem is Task task)
            {
                if (!WeekTimeWorked.Tasks.ContainsKey(task.Id))
                {
                    WeekTimeWorked.Tasks.Add(task.Id, task);
                    UpdateGrid();
                }
            }
        }
    }

    private void TimerBtn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            Task task = (Task)btn.Tag;
            if (btn.Content.ToString() == START_TIMER)
            {
                // If this is the first time timing is used, create the timer.
                if (TaskTimer is null)
                {
                    TaskTimer = new()
                    {
                        Interval = TimeSpan.FromSeconds(10)
                    };
                    TaskTimer.Tick += TaskTimer_Tick;
                }

                // If a task is already being timed, stop timing it.
                if (TaskTimer.IsEnabled && TimedTask != null && TimedTaskTextBox != null && TimedTaskButton != null)
                {
                    TaskTimer.IsEnabled = false;
                    TimedTask.StopTiming(DateTime.Now);
                    if (FindResource(INACTIVE_STYLE) is Style style)
                    {
                        TimedTaskTextBox.Style = style;
                    }
                    TimedTaskButton.Content = START_TIMER;
                }

                // Start timing the selected task.
                TimedTask = task;
                TimedTaskButton = btn;
                int i = int.Parse(btn.Name[^1..]);
                int j = (int)DateTime.Now.DayOfWeek;
                if (MainForm.FindName($"Tb{i}{j}") is TextBox tb)
                {
                    if (FindResource(ACTIVE_STYLE) is Style style)
                    {
                        tb.Style = style;
                    }
                    tb.Text = "0:00";
                    TimedTaskTextBox = tb;
                }
                TimedTask.StartTiming(DateTime.Now);
                btn.Content = STOP_TIMER;
                TaskTimer.IsEnabled = true;
            }
            else
            {
                // Stop timing the selected task.
                if (TaskTimer != null && TimedTask != null && TimedTaskTextBox != null)
                {
                    // Already timing a task so stop timing that one.
                    TaskTimer.IsEnabled = false;
                    TimedTask.StopTiming(DateTime.Now);
                    if (FindResource(INACTIVE_STYLE) is Style style)
                    {
                        TimedTaskTextBox.Style = style;
                    }
                }
                btn.Content = START_TIMER;
            }
            UpdateGrid();
        }
    }

    private void TaskTimer_Tick(object? sender, EventArgs e)
    {
        if (TimedTask != null)
        {
            TimedTask.UpdateTiming(DateTime.Now);
            WeekTimeWorked.Save();
            UpdateGrid();
        }
    }

    private void CreateTaskBtn_Click(object sender, RoutedEventArgs e)
    {
        AddTask addTaskForm = new();
        var result = addTaskForm.ShowDialog();
        if (result.HasValue && result.Value == true)
        {
            Task task = new()
            {
                Name = addTaskForm.TaskNameTb.Text,
                Code = addTaskForm.TaskCodeTb.Text,
            };
            DataStore.AddNewTask(task);
            AvailableTasksCmb.ItemsSource = DataStore.GetAllTasks();
        }
    }

    private void Summary_Click(object sender, RoutedEventArgs e)
    {
        Summary summary = new(WeekTimeWorked);
        summary.ShowDialog();
    }
}
