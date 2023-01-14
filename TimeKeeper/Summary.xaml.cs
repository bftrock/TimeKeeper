using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TimeKeeper;

/// <summary>
/// Interaction logic for Summary.xaml
/// </summary>
public partial class Summary : Window
{
    public Summary(WeekTimeWorked wtw)
    {
        InitializeComponent();
        var results = wtw.Tasks.Values
            .GroupBy(t => t.Code)
            .Select(g => new {
                Code = g.Key,
                Sunday = g.Select(v => v.GetTimeWorked(DayOfWeek.Sunday)).Sum(),
                Monday = g.Select(v => v.GetTimeWorked(DayOfWeek.Monday)).Sum(),
                Tuesday = g.Select(v => v.GetTimeWorked(DayOfWeek.Tuesday)).Sum(),
                Wednesday = g.Select(v => v.GetTimeWorked(DayOfWeek.Wednesday)).Sum(),
                Thursday = g.Select(v => v.GetTimeWorked(DayOfWeek.Thursday)).Sum(),
                Friday = g.Select(v => v.GetTimeWorked(DayOfWeek.Friday)).Sum(),
                Saturday = g.Select(v => v.GetTimeWorked(DayOfWeek.Saturday)).Sum(),
            });
        int i = 0;
        foreach (var r in results)
        {
            var tb1 = (TextBlock)FindName($"TaskName{i}");
            if (tb1 != null)
            {
                tb1.Text = r.Code;
            }
            var tb2 = (TextBlock)FindName($"Tb{i}0");
            if (tb2 != null)
            {
                tb2.Text = r.Sunday.ToString("F2");
            }
            var tb3 = (TextBlock)FindName($"Tb{i}1");
            if (tb3 != null)
            {
                tb3.Text = r.Monday.ToString("F2");
            }
            var tb4 = (TextBlock)FindName($"Tb{i}2");
            if (tb4 != null)
            {
                tb4.Text = r.Tuesday.ToString("F2");
            }
            var tb5 = (TextBlock)FindName($"Tb{i}3");
            if (tb5 != null)
            {
                tb5.Text = r.Wednesday.ToString("F2");
            }
            var tb6 = (TextBlock)FindName($"Tb{i}4");
            if (tb6 != null)
            {
                tb6.Text = r.Thursday.ToString("F2");
            }
            var tb7 = (TextBlock)FindName($"Tb{i}5");
            if (tb7 != null)
            {
                tb7.Text = r.Friday.ToString("F2");
            }
            var tb8 = (TextBlock)FindName($"Tb{i}6");
            if (tb8 != null)
            {
                tb8.Text = r.Saturday.ToString("F2");
            }
            i++;
        }
    }
}
