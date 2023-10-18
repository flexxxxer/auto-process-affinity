using UI.ViewModels;

using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.ReactiveUI;

using ReactiveUI;

namespace UI.Views;

public partial class SelectCurrentlyRunnableProcessView : ReactiveUserControl<ISelectCurrentlyRunnableProcessViewModel>
{
  public SelectCurrentlyRunnableProcessView()
  {
    InitializeComponent();

    this.WhenActivated(d =>
    {
      // make column with process name sorted but only
      // first time (on view opening)
      // ReSharper disable once AsyncVoidLambda
      this.WhenAnyValue(v => v.ProcessesDataGrid.IsVisible)
        .Where(visible => visible)
        .Take(1)
        .Subscribe(async _ => 
        {
          await Task.Yield();
          SearchTextBox.Focus();
          ProcessesDataGrid.Columns
            .Single(c => c.Tag is "ProcessNameColumn")
            .Sort(ListSortDirection.Ascending);
        })
        .DisposeWith(d);
    });
  }
}