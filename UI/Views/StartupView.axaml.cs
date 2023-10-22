using Domain;

using UI.ViewModels;
using UI.ViewModels.Entities;

using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

using ReactiveUI;

namespace UI.Views;

public partial class StartupView : ReactiveUserControl<IStartupViewModel>
{
  public StartupView()
  {
    InitializeComponent();

    this.WhenActivated(d =>
    {
      this.WhenAnyValue(v => v.ViewModel)
        .WhereNotNull()
        .Subscribe(vm =>
        {
          MonitoredProcessesDataGrid.SelectedItem = null;
          vm.SelectedProcesses = new(Array.Empty<MonitoredProcess>());
          
          Observable
            .FromEventPattern<SelectionChangedEventArgs>(
              h => MonitoredProcessesDataGrid.SelectionChanged += h,
              h => MonitoredProcessesDataGrid.SelectionChanged -= h)
            .Subscribe(_ => vm.SelectedProcesses = MonitoredProcessesDataGrid
              .SelectedItems
              .OfType<MonitoredProcess>()
              .ToList()
              .AsReadOnly())
            .DisposeWith(d);
          
          Observable
            .FromEventPattern<TappedEventArgs>(
              h => MonitoredProcessesDataGrid.DoubleTapped += h,
              h => MonitoredProcessesDataGrid.DoubleTapped -= h)
            .Subscribe(e =>
            {
              if (MonitoredProcessesDataGrid.SelectedItems is [var selectedItem]
                  && e.EventArgs.Source?.TryCastTo<Control>().FindAncestorOfType<DataGridRow>() is not null)
              {
                vm.EditMonitoredProcessCommand.ExecuteAsync(selectedItem).NoAwait();
              }
            })
            .DisposeWith(d);

          
          var whenSortingHappenedLastTime = TimeSpan.Zero;
          Observable
            .FromEventPattern<DataGridColumnEventArgs>(
              h => MonitoredProcessesDataGrid.Sorting += h,
              h => MonitoredProcessesDataGrid.Sorting -= h)
            .Subscribe(_ => whenSortingHappenedLastTime = TimeSpan.FromMilliseconds(Environment.TickCount64))
            .DisposeWith(d);
          
          Observable
            .FromEventPattern<SelectionChangedEventArgs>(
              h => MonitoredProcessesDataGrid.SelectionChanged += h,
              h => MonitoredProcessesDataGrid.SelectionChanged -= h)
            .Subscribe(_ =>
            {
              var now = TimeSpan.FromMilliseconds(Environment.TickCount64);
              if (now - whenSortingHappenedLastTime < TimeSpan.FromMilliseconds(150))
              {
                MonitoredProcessesDataGrid.SelectedItem = null;
              }
            })
            .DisposeWith(d);
        })
        .DisposeWith(d);

      Observable
        .FromEventPattern<RoutedEventArgs>(
          h => ResetSelectionOnDataGrid.Click += h,
          h => ResetSelectionOnDataGrid.Click -= h)
        .Subscribe(_ => MonitoredProcessesDataGrid.SelectedItem = null)
        .DisposeWith(d);
    });
  }
}