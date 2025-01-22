using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using IMP.Models;
using IMP.Services;

namespace IMP
{
    public partial class HistoryPage : ContentPage
    {
        private readonly RealtimeDatabaseService _databaseService;
        private readonly string _userId;

        public ObservableCollection<ScheduledHistoryEntry> ScheduledHistory { get; set; } = new ObservableCollection<ScheduledHistoryEntry>();
        public ObservableCollection<ManualHistoryEntry> ManualHistory { get; set; } = new ObservableCollection<ManualHistoryEntry>();

        public HistoryPage(string userId)
        {
            InitializeComponent(); // Rozwi¹zanie problemu CS0103
            _userId = userId;
            _databaseService = new RealtimeDatabaseService();

            BindingContext = this;

            LoadHistoryAsync();
        }

        private async void LoadHistoryAsync()
        {
            try
            {
                var scheduledHistory = await _databaseService.GetScheduledHistoryAsync(_userId);
                var manualHistory = await _databaseService.GetManualHistoryAsync(_userId);

                Device.BeginInvokeOnMainThread(() =>
                {
                    ScheduledHistory.Clear();
                    foreach (var entry in scheduledHistory)
                    {
                        ScheduledHistory.Add(entry);
                    }

                    ManualHistory.Clear();
                    foreach (var entry in manualHistory)
                    {
                        ManualHistory.Add(entry);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading history: {ex.Message}");
            }
        }
    }
}
