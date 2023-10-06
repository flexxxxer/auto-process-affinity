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

    this.WhenActivated((CompositeDisposable d) =>
    {
      this.WhenAnyValue(w => w.ViewModel)
        .WhereNotNull()
        .Subscribe(vm =>
        {
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
