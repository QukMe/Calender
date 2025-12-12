using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schedule.Models;
using Schedule.Services;
using Calender.Services;

namespace Calender.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _statusText = " ";

    [ObservableProperty] private bool _isBusy = true;

    [ObservableProperty] private DateTimeOffset _startDate = DateTimeOffset.Now;

    [ObservableProperty] private DateTimeOffset _endDate = DateTimeOffset.Now.AddDays(7);

    [ObservableProperty] private ObservableCollection<Teacher> _allTeachers;

    [ObservableProperty] private Teacher _selectedTeacher;

    public MainWindowViewModel()
    {
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            var loader = new GrsuLoader();
            var teachersList = await loader.GetTeachersAsync();

            AllTeachers = new ObservableCollection<Teacher>(teachersList);
            string savedIdStr = SettingsManager.LoadTeacherId();
            if (int.TryParse(savedIdStr, out int savedId))
            {
                SelectedTeacher = teachersList.FirstOrDefault(t => t.Id == savedId);
            }

            StatusText = $"Загружено преподавателей: {teachersList.Count}. Выберите из списка.";
        }
        catch (Exception ex)
        {
            StatusText = $"Ошибка загрузки списка";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Sync()
    {
        if (IsBusy)
        {
            return;
        }

        if (SelectedTeacher == null)
        {
            StatusText = "Выберите преподавателя из списка";
            return;
        }

        IsBusy = true;

        SettingsManager.SaveTeacherId(SelectedTeacher.Id.ToString());

        DateTime start = StartDate.Date;
        DateTime end = EndDate.Date;
        int teacherId = SelectedTeacher.Id;

        string report = await Task.Run(async () =>
        {
            var uniLoader = new GrsuLoader();
            var googleService = new GoogleCalendarService();
            var manager = new SyncService(uniLoader, googleService);
            return await manager.SyncAsync(teacherId, start, end);
        });

        IsBusy = false;
        StatusText = report;
    }

    [RelayCommand]
    private void SetThisWeek()
    {
        DateTime today = DateTime.Now;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        StartDate = today.AddDays(-1 * diff).Date;
        EndDate = StartDate.AddDays(6);
    }

    [RelayCommand]
    private void SetNextWeek()
    {
        StartDate = StartDate.AddDays(7);
        EndDate = EndDate.AddDays(7);
    }
}