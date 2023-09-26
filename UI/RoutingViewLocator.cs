using UI.ViewModels;
using UI.Views;

using System;

using ReactiveUI;

namespace UI;

public class RoutingViewLocator : IViewLocator
{
  public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
    => viewModel switch
    {
      null => null,
      AddProcessViewModel vm => new AddProcessView { DataContext = vm },
      SelectCurrentlyRunnableProcessViewModel vm => new SelectCurrentlyRunnableProcessView { DataContext = vm },
      StartupViewModel vm => new StartupView { DataContext = vm },
      _ => throw new IndexOutOfRangeException()
    };
}
