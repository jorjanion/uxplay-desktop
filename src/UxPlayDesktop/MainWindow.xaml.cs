using System;
using System.ComponentModel;
using System.Windows;
using UxPlayDesktop.ViewModels;

namespace UxPlayDesktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.Settings.MinimizeToTray)
        {
            e.Cancel = true;
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            return;
        }

        base.OnClosing(e);
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState != WindowState.Minimized)
        {
            ShowInTaskbar = true;
        }
    }
}