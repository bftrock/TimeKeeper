using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimeKeeper;

public partial class MainWindow : Window
{
    private const string ConnStr = "Data Source=C:\\Users\\bftro\\AppData\\Local\\TimeKeeper\\TimeKeeper.sqlite";
    private readonly WeekTimeWorked WeekTimeWorked;
    private readonly Tasks AvailableTasks = new();
    private TaskTimeWorked? TimedTask;
    private DateTime? TimedTaskStartTime;
    private Button? TimedTaskButton;
    private TextBox? TimedTaskTextBox;

    public MainWindow()
    {
        InitializeComponent();
        WeekTimeWorked = new()
        {
            ConnStr = ConnStr,
        };
        WeekCommencingPicker.SelectedDate = DateTime.Now;
        UpdateTaskList();
    }

    private void UpdateTaskList()
    {
        AvailableTasks.LoadFromDatabase();
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
            TaskTimeWorked ttw = kvp.Value;
            var lbl1 = (Label)MainForm.FindName($"TaskName{i}");
            if (lbl1 != null)
            {
                lbl1.Content = ttw.TaskName;
            }
            for (j = 0; j < 7; j++)
            {
                var timeWorked = ttw.GetTimeWorked((DayOfWeek)j);
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
                btn1.Tag = ttw;
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
        if (sender is TextBox tb && tb.Text.Length > 0)
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
            WeekTimeWorked.SaveToDatabase();
        }
    }

    private double ParseTimeEntry(string entry)
    {
        string timePattern = @"(\d{0,2}):(\d{2})";
        string numberPattern = @"^\d*\.?\d+$";
        Match matchTime = Regex.Match(entry, timePattern, RegexOptions.IgnoreCase);
        double result;
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
            result = span.TotalHours;
        }
        else
        {
            Match matchDecimal = Regex.Match(entry, numberPattern, RegexOptions.IgnoreCase);
            if (matchDecimal.Success)
            {
                result = double.Parse(entry);
            }
            else
            {
                throw new Exception("Invalid input format");
            }
        }
        return result;
    }

    private string FormatHoursAsTime(double hours)
    {
        return hours > 0 ? TimeSpan.FromMinutes(Math.Round(60 * hours)).ToString("h\\:mm") : "";
    }

    private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
    {
        if (AvailableTasksCmb.SelectedIndex >= 0)
        {
            if (AvailableTasksCmb.SelectedItem is Task task)
            {
                if (!WeekTimeWorked.Tasks.ContainsKey(task.Id))
                {
                    TaskTimeWorked ttw = new()
                    {
                        TaskId = task.Id,
                        TaskName = task.Name,
                    };
                    WeekTimeWorked.Tasks.Add(task.Id, ttw);
                    UpdateGrid();
                }
            }
        }
    }
}
