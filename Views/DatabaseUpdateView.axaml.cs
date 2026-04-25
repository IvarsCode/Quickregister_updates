using System;
using Avalonia.Controls;
using Avalonia.Threading;
using QuickRegister.ViewModels;

namespace QuickRegister.Views;

public partial class DatabaseUpdateView : UserControl
{
    public DatabaseUpdateView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is DatabaseUpdateViewModel vm && !RootGrid.IsVisible)
            Dispatcher.UIThread.Post(() => vm.OnSkipRequested?.Invoke());
    }
}
