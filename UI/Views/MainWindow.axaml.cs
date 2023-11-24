using UI.ViewModels;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.ReactiveUI;

using ReactiveUI;

namespace UI.Views;

public partial class MainWindow : ReactiveWindow<IMainWindowViewModel>
{
  public MainWindow()
  {
    InitializeComponent();

    this.WhenActivated(d =>
    {
      this.WhenAnyValue(v => v.ViewModel)
        .WhereNotNull()
        .Subscribe(vm =>
        {
          // can be bind in axaml after fix https://github.com/AvaloniaUI/Avalonia/issues/13300 
          WindowState = vm.WindowState;
          ShowInTaskbar = vm.MinimizeToTrayAtStartup is false;

          Observable
            .FromEventPattern<WindowClosingEventArgs>(
              h => Closing += h,
              h => Closing -= h)
            .Select(e => e.EventArgs)
            .Subscribe(args =>
            {
              if (vm.HideInTrayInsteadOfClosing)
              {
                args.Cancel = true;
                ShowInTaskbar = false;
                WindowState = WindowState.Minimized;
              }
            })
            .DisposeWith(d);

          this.WhenAnyValue(v => v.WindowState)
            .Subscribe(newState => ShowInTaskbar = newState is not WindowState.Minimized || ShowInTaskbar)
            .DisposeWith(d);
          
          vm.SetWindowPosition
            .RegisterHandler(interaction =>
            {
              Position = interaction.Input;
              interaction.SetOutput(default);
            })
            .DisposeWith(d);

          Observable
            .FromEventPattern<PixelPointEventArgs>(h => PositionChanged += h, h => PositionChanged -= h)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(eventPattern => vm.WindowPosition = eventPattern.EventArgs.Point)
            .DisposeWith(d);
        })
        .DisposeWith(d);
    });
  }
}
