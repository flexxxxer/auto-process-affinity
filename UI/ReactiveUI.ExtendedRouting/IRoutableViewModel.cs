// ReSharper disable once CheckNamespace
namespace ReactiveUI.ExtendedRouting;

public interface IRoutableViewModel<in TIn> : IRoutableViewModel
{
  TIn InputArgument { init; }
}
