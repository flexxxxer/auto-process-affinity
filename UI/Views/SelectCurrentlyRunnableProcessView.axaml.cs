using UI.ViewModels;

using System.ComponentModel;
using System.Linq;

using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

namespace UI.Views;

public partial class SelectCurrentlyRunnableProcessView : ReactiveUserControl<SelectCurrentlyRunnableProcessViewModel>
{
  public SelectCurrentlyRunnableProcessView()
  {
    InitializeComponent();
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    base.OnLoaded(e);

    ProcessesDataGrid.Columns
      .Single(c => c.Tag is "ProcessNameColumn")
      .Sort(ListSortDirection.Ascending);

    SearchTextBox.Focus();
  }
}