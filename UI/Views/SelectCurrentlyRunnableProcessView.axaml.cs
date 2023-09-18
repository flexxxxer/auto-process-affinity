using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI;

public partial class SelectCurrentlyRunnableProcessView : ReactiveUserControl<SelectCurrentlyRunnableProcessViewModel>
{
  public SelectCurrentlyRunnableProcessView()
  {
    InitializeComponent();
  }
}