using UI.ViewModels;

using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using ReactiveUI;
using UI.ViewModels.Entities;

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