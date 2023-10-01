using Domain;

using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Styling;

using ReactiveUI;
using ReactiveUI.ExtendedRouting;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

using Splat;
using System.Collections.Generic;
using System;

namespace UI.ViewModels;

public interface IAddProcessViewModel
{
  string ProcessName { get; set; }

  bool IsEvenAffinityModeChosen { get; set; }

  bool IsFirstNAffinityModeChosen { get; set; }

  bool IsLastNAffinityModeChosen { get; set; }

  bool IsCustomAffinityModeChosen { get; set; }

  string EvenAffinityModeFirstNValue { get; set; }

  string FirstNAffinityModeValue { get; set; }

  string LastNAffinityModeValue { get; set; }

  string CustomAffinityModeValue { get; set; }

  IAsyncRelayCommand ChooseProcessCommand { get; }

  IRelayCommand AddProcessCommand { get; }

  IRelayCommand CancelCommand { get; }
}

public sealed partial class AddProcessViewModel : ViewModelBase, IAddProcessViewModel, IActivatableViewModel, IRoutableViewModel
{
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] string _processName = "";
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isEvenAffinityModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isFirstNAffinityModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isLastNAffinityModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isCustomAffinityModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] string _evenAffinityModeFirstNValue = "";
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] string _firstNAffinityModeValue = "";
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] string _lastNAffinityModeValue = "";
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] string _customAffinityModeValue = "";
  [ObservableProperty] long _affinityValue = 0;

  public ViewModelActivator Activator { get; } = new();
  public string UrlPathSegment => nameof(AddProcessViewModel).RemoveVmPostfix();
  public IScreen HostScreen { get; }

  readonly TaskCompletionSource<ConfiguredProcess?> _resultSource = new();
  public Task<ConfiguredProcess?> Result => _resultSource.Task;

  AffinityMode _affinityMode = AffinityMode.AllEven;

  public AddProcessViewModel(IScreen screen) 
  {
    HostScreen = screen;
  }

  partial void OnIsEvenAffinityModeChosenChanged(bool value)
  {
    if(value is false)
    {
      return;
    }
    _affinityMode = value ? AffinityMode.AllEven : _affinityMode;
    if (!EvenAffinityModeFirstNValue.IsNullOrWhiteSpace())
    {
      AffinityValue = long.TryParse(EvenAffinityModeFirstNValue, out var firstNEven)
        ? AffinityApi.FillFirstNEvenOnly((int)firstNEven)
        : AffinityValue;
    }
    else 
    {
      AffinityValue = AffinityApi.FillFirstNEvenOnly(Environment.ProcessorCount / 2);
    }
  }

  partial void OnIsFirstNAffinityModeChosenChanged(bool value)
  {
    if (value is false)
    {
      return;
    }
    _affinityMode = AffinityMode.FirstN;
    if (!FirstNAffinityModeValue.IsNullOrWhiteSpace())
    {
      AffinityValue = long.TryParse(FirstNAffinityModeValue, out var firstN)
        ? AffinityApi.FillFirstN((int)firstN)
        : AffinityValue;
    }
  }

  partial void OnIsLastNAffinityModeChosenChanged(bool value)
  {
    if (value is false)
    {
      return;
    }
    _affinityMode = AffinityMode.LastN;
    if (!LastNAffinityModeValue.IsNullOrWhiteSpace())
    {
      AffinityValue = long.TryParse(LastNAffinityModeValue, out var lastN)
        ? AffinityApi.FillLastN((int)lastN)
        : AffinityValue;
    }
  }

  partial void OnIsCustomAffinityModeChosenChanged(bool value)
    => _affinityMode = value ? AffinityMode.CustomBitmask : _affinityMode;

  partial void OnEvenAffinityModeFirstNValueChanged(string value)
  {
    if (long.TryParse(value, out var firstNEven))
    {
      AffinityValue = AffinityApi.FillFirstNEvenOnly((int)firstNEven);
      _affinityMode = AffinityMode.FirstNEven;
    }
  }

  partial void OnFirstNAffinityModeValueChanged(string value)
    => AffinityValue = long.TryParse(value, out var firstN)
      ? AffinityApi.FillLastN((int)firstN)
      : AffinityValue;

  partial void OnLastNAffinityModeValueChanged(string value)
    => AffinityValue = long.TryParse(value, out var lastN)
      ? AffinityApi.FillLastN((int)lastN)
      : AffinityValue;

  partial void OnAffinityValueChanged(long value)
    => CustomAffinityModeValue = AffinityValue.ToString("X");

  partial void OnCustomAffinityModeValueChanged(string value)
    => AffinityValue = long.TryParse(value.Remove("0x"), System.Globalization.NumberStyles.HexNumber, null, out var newMask)
      ? AffinityApi.FromCustom(newMask)
      : AffinityValue;

  bool CanAddProcess() =>
    !ProcessName.IsNullOrWhiteSpace()
    && (
      (IsEvenAffinityModeChosen && (!EvenAffinityModeFirstNValue.IsNullOrWhiteSpace() || EvenAffinityModeFirstNValue is ""))
      || (IsFirstNAffinityModeChosen && !FirstNAffinityModeValue.IsNullOrWhiteSpace())
      || (IsLastNAffinityModeChosen && !LastNAffinityModeValue.IsNullOrWhiteSpace())
      || (IsCustomAffinityModeChosen && !CustomAffinityModeValue.IsNullOrWhiteSpace())
    );

  [RelayCommand]
  async Task ChooseProcess()
  {
    var vm = await Locator.Current
      .GetRequiredService<SelectCurrentlyRunnableProcessViewModel>()
      .RouteThrought(HostScreen);

    var selectedProcess = await vm.Result;

    if (selectedProcess is not null) ProcessName = selectedProcess.Name;
  }

  [RelayCommand(CanExecute = nameof(CanAddProcess))]
  void AddProcess()
  {
    _resultSource.SetResult(new()
    {
      Name = ProcessName,
      AffinityMode = _affinityMode,
      AffinityValue = AffinityValue
    });
    HostScreen.Router.NavigateBack.Execute();
  }

  [RelayCommand]
  void Cancel()
  {
    _resultSource.SetResult(null);
    HostScreen.Router.NavigateBack.Execute();
  }
}

public sealed partial class DesignAddProcessViewModel : ViewModelBase, IAddProcessViewModel
{
  [ObservableProperty] string _processName = "";
  [ObservableProperty] bool _isEvenAffinityModeChosen;
  [ObservableProperty] bool _isFirstNAffinityModeChosen;
  [ObservableProperty] bool _isLastNAffinityModeChosen;
  [ObservableProperty] bool _isCustomAffinityModeChosen;
  [ObservableProperty] string _evenAffinityModeFirstNValue = "";
  [ObservableProperty] string _firstNAffinityModeValue = "";
  [ObservableProperty] string _lastNAffinityModeValue = "";
  [ObservableProperty] string _customAffinityModeValue = "";

  public DesignAddProcessViewModel()
  {
    if (App.IsDesignMode) Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }

  [RelayCommand]
  Task ChooseProcess() => Task.CompletedTask;

  [RelayCommand]
  void AddProcess()
  {
  }

  [RelayCommand]
  void Cancel()
  {
  }
}