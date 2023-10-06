using UI.ViewModels;

using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;
using System;

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
        })
        .DisposeWith(d);
    });
  }
}
