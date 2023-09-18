using UI.ViewModels;

using System;
using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

using ReactiveUI;

namespace UI.Views;

public partial class DesignModeTitleBar : ReactiveUserControl<DesignModeTitleBarViewModel>
{
  public DesignModeTitleBar()
  {
    InitializeComponent();

    DataContext = new DesignModeTitleBarViewModel();
    (HorizontalAlignment, VerticalAlignment) = (HorizontalAlignment.Stretch, VerticalAlignment.Top);
    (IsVisible, IsEnabled) = App.IsDesignMode switch
    {
      true => (true, true),
      false => (false, false),
    };
    
    this.WhenActivated((CompositeDisposable d) =>
    {
      this.WhenAnyValue(v => v.ViewModel!)
        .Subscribe(vm =>
        {
          vm
            .ChangeOrientation
            .RegisterHandler(interaction =>
            {
              var window = this.FindAncestorOfType<Window>()!;
              (window.Height, window.Width) = interaction.Input switch
              {
                Orientation.Vertical when window.Height < window.Width => (window.Width, window.Height),
                Orientation.Horizontal when window.Height > window.Width => (window.Width, window.Height),
                _ => (window.Height, window.Width)
              };
              interaction.SetOutput(default);
            })
            .DisposeWith(d);

          vm
            .OpenDevTools
            .RegisterHandler(interaction =>
            {
              var window = this.FindAncestorOfType<Window>()!;
              KeyEventArgs keyEventArgs = new()
              {
                RoutedEvent = KeyDownEvent,
                Key = Key.F12,
                KeyModifiers = KeyModifiers.None,
                Source = this
              };
              window.RaiseEvent(keyEventArgs);
              interaction.SetOutput(default);
            })
            .DisposeWith(d);
        })
        .DisposeWith(d);
    });
  }
}