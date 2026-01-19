using System;
using System.Collections.Generic;

namespace CatalogoBCV.Models.ViewModels
{
    public class UserMetricsViewModel
    {
        // Tab 1
        public List<UserUsageMetric> UserUsage { get; set; } = new();
        public List<UserLoginMetric> UserLogins { get; set; } = new();

        // Tab 2
        public List<DailyLoginMetric> Last30DaysLogins { get; set; } = new();
    }

    public class UserUsageMetric
    {
        public string Username { get; set; } = string.Empty;
        public double TotalHours { get; set; }
    }

    public class UserLoginMetric
    {
        public string Username { get; set; } = string.Empty;
        public int TotalLogins { get; set; }
    }

    public class DailyLoginMetric
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
