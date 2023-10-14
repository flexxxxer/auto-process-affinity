using UI.ViewModels;

using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace UI.Views;

public partial class SelectCurrentlyRunnableProcessView : ReactiveUserControl<ISelectCurrentlyRunnableProcessViewModel>
{
  public SelectCurrentlyRunnableProcessView()
  {
    InitializeComponent();

    this.WhenActivated((CompositeDisposable d) =>
    {
      // make column with process name sorted but only
      // first time (on view opening)
      this.WhenAnyValue(v => v.ProcessesDataGrid.IsVisible)
        .Where(visibility => visibility is true)
        .Take(1)
        .Subscribe(async _ => 
        {
          await Task.Yield();
          ProcessesDataGrid.Columns
            .Single(c => c.Tag is "ProcessNameColumn")
            .Sort(ListSortDirection.Ascending);
        })
        .DisposeWith(d);
    });
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    base.OnLoaded(e);

    SearchTextBox.Focus();
  }
}