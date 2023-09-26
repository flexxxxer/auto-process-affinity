using Avalonia;
using Avalonia.Layout;
using Avalonia.Styling;

using System.Reactive;
using System.Reactive.Linq;

using ReactiveUI;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UI.ViewModels;

public sealed partial class DesignModeTitleBarViewModel : ObservableObject
{
  [ObservableProperty] ThemeVariant _selectedThemeVariant = ThemeVariant.Default;
  [ObservableProperty] bool _isDarkChecked;

  [ObservableProperty] Orientation _selectedOrientation = Orientation.Horizontal;
  [ObservableProperty] bool _isVerticalOrientationChecked;

  public Interaction<Orientation, Unit> ChangeOrientation { get; } = new();

  public Interaction<Unit, Unit> OpenDevTools { get; } = new();

  public DesignModeTitleBarViewModel()
  {
    if (App.IsDesignMode)
    {
      IsDarkChecked = true;
      IsVerticalOrientationChecked = true;
    }
  }

  partial void OnIsDarkCheckedChanged(bool value)
    => SelectedThemeVariant = value ? ThemeVariant.Dark : ThemeVariant.Light;

  partial void OnSelectedThemeVariantChanged(ThemeVariant value) 
    => Application.Current!.RequestedThemeVariant = value;

  partial void OnIsVerticalOrientationCheckedChanged(bool value)
    => SelectedOrientation = value ? Orientation.Vertical : Orientation.Horizontal;

  partial void OnSelectedOrientationChanged(Orientation value)
  {
    try
    {
      ChangeOrientation.Handle(value).GetAwaiter().GetResult();
    }
    catch (UnhandledInteractionException<Orientation, Unit>)
    {
    }
  }

  [RelayCommand]
  void DevTools()
  {
    try
    {
      OpenDevTools.Handle(default).GetAwaiter().GetResult();
    }
    catch (UnhandledInteractionException<Unit, Unit>)
    {
    }
  }
}
