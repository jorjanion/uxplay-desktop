using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using UxPlayDesktop.Models;

namespace UxPlayDesktop.ViewModels;

public class LogsViewModel : ViewModelBase
{
    private readonly ObservableCollection<LogEntry> _allLogs = [];
    private readonly object _logsLock = new();
    private string _searchFilter = string.Empty;
    private LogLevel? _selectedLevelFilter;
    private bool _autoScroll = true;
    private readonly ICollectionView? _filteredLogsView;

    public ObservableCollection<LogEntry> Logs => _allLogs;
    public ICollectionView FilteredLogs => _filteredLogsView ?? CollectionViewSource.GetDefaultView(_allLogs);

    public bool HasLogs => _allLogs.Count > 0;

    public string FilterText
    {
        get => _searchFilter;
        set => SearchFilter = value;
    }

    public string SearchFilter
    {
        get => _searchFilter;
        set
        {
            if (SetField(ref _searchFilter, value))
            {
                OnPropertyChanged(nameof(FilterText));
                RefreshFilter();
            }
        }
    }

    public LogLevel? SelectedLevelFilter
    {
        get => _selectedLevelFilter;
        set
        {
            if (SetField(ref _selectedLevelFilter, value))
            {
                RefreshFilter();
            }
        }
    }

    public bool AutoScroll
    {
        get => _autoScroll;
        set => SetField(ref _autoScroll, value);
    }

    public ICommand CopyCommand => CopyLogsCommand;
    public ICommand ClearCommand => ClearLogsCommand;
    public ICommand ClearLogsCommand { get; }
    public ICommand CopyLogsCommand { get; }

    public LogsViewModel()
    {
        BindingOperations.EnableCollectionSynchronization(_allLogs, _logsLock);
        try
        {
            _filteredLogsView = CollectionViewSource.GetDefaultView(_allLogs);
        }
        catch { }

        ClearLogsCommand = new RelayCommand(ClearLogs);
        CopyLogsCommand = new RelayCommand(CopyLogs);
    }

    public void AddEntry(LogEntry entry)
    {
        lock (_logsLock)
        {
            // Limit buffer to last 2000 log items
            if (_allLogs.Count > 2000)
            {
                _allLogs.RemoveAt(0);
            }
            _allLogs.Add(entry);
        }
        OnPropertyChanged(nameof(HasLogs));
    }

    public void ClearLogs()
    {
        lock (_logsLock)
        {
            _allLogs.Clear();
        }
        OnPropertyChanged(nameof(HasLogs));
    }

    public void CopyLogs()
    {
        lock (_logsLock)
        {
            var sb = new StringBuilder();
            foreach (var log in _allLogs)
            {
                sb.AppendLine($"[{log.FormattedTime}] [{log.Level}] {log.Message}");
            }

            try
            {
                Clipboard.SetText(sb.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to copy to clipboard: {ex.Message}");
            }
        }
    }

    private void RefreshFilter()
    {
        var view = _filteredLogsView ?? CollectionViewSource.GetDefaultView(_allLogs);
        if (view == null) return;

        if (string.IsNullOrWhiteSpace(_searchFilter) && !_selectedLevelFilter.HasValue)
        {
            view.Filter = null;
        }
        else
        {
            view.Filter = item =>
            {
                if (item is not LogEntry entry) return false;

                if (_selectedLevelFilter.HasValue && entry.Level != _selectedLevelFilter.Value)
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(_searchFilter))
                {
                    return entry.Message.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase);
                }

                return true;
            };
        }
        view.Refresh();
    }
}
