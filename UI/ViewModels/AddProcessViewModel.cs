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
using Hardware.Info;

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

  ConfiguredProcess? ToEdit { get; }

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

  [ObservableProperty] ConfiguredProcess? _toEdit;

  public ViewModelActivator Activator { get; } = new();
  public string UrlPathSegment => nameof(AddProcessViewModel).RemoveVmPostfix();
  public IScreen HostScreen { get; }

  readonly TaskCompletionSource<ConfiguredProcess?> _resultSource = new();
  public Task<ConfiguredProcess?> Result => _resultSource.Task;

  AffinityMode _affinityMode;
  long _affinityValue;

  public AddProcessViewModel(IScreen screen) 
  {
    HostScreen = screen;
  }

  void HandleAffinityModeChange()
  {
    (_affinityValue, _affinityMode) = (null as object) switch
    {
      _ when IsEvenAffinityModeChosen => EvenAffinityModeFirstNValue switch
      {
        { } str when int.TryParse(str, out var firstNEven) => (firstNEven, AffinityMode.FirstNEven),
        null or "" => (0 /* any value not matter */, AffinityMode.AllEven),
        _ => (_affinityValue, _affinityMode),
      },

      _ when IsFirstNAffinityModeChosen && int.TryParse(FirstNAffinityModeValue, out var firstN) 
        => (firstN, AffinityMode.FirstN),

      _ when IsLastNAffinityModeChosen && int.TryParse(LastNAffinityModeValue, out var lastN) 
        => (lastN, AffinityMode.LastN),

      _ when IsCustomAffinityModeChosen 
        && long.TryParse(CustomAffinityModeValue.Remove("0x"), System.Globalization.NumberStyles.HexNumber, null, out var newMask)
        => (newMask, AffinityMode.CustomBitmask),

      _ => (_affinityValue, _affinityMode)
    };

    CustomAffinityModeValue = AffinityApi.BitmaskFrom(_affinityMode, _affinityValue).ToString("X");
  }

  partial void OnIsEvenAffinityModeChosenChanged(bool value) => HandleAffinityModeChange();

  partial void OnIsFirstNAffinityModeChosenChanged(bool value) => HandleAffinityModeChange();

  partial void OnIsLastNAffinityModeChosenChanged(bool value) => HandleAffinityModeChange();

  partial void OnIsCustomAffinityModeChosenChanged(bool value) => HandleAffinityModeChange();

  partial void OnEvenAffinityModeFirstNValueChanged(string value) => HandleAffinityModeChange();

  partial void OnFirstNAffinityModeValueChanged(string value) => HandleAffinityModeChange();

  partial void OnLastNAffinityModeValueChanged(string value) => HandleAffinityModeChange();

  partial void OnCustomAffinityModeValueChanged(string value) => HandleAffinityModeChange();

  partial void OnToEditChanged(ConfiguredProcess? value)
  {
    if(value is not null)
    {
      ToEdit = value;
      ProcessName = value.Name;

      (IsEvenAffinityModeChosen, EvenAffinityModeFirstNValue) = value.AffinityMode switch
      {
        AffinityMode.AllEven => (true, ""),
        AffinityMode.FirstNEven => (true, value.AffinityValue.ToString()),
        _ => (false, "")
      };

      (IsFirstNAffinityModeChosen, FirstNAffinityModeValue) = value.AffinityMode is AffinityMode.FirstN
        ? (true, value.AffinityValue.ToString())
        : (false, "");

      (IsLastNAffinityModeChosen, LastNAffinityModeValue) = value.AffinityMode is AffinityMode.LastN
        ? (true, value.AffinityValue.ToString())
        : (false, "");

      (IsCustomAffinityModeChosen, CustomAffinityModeValue) = value.AffinityMode is AffinityMode.CustomBitmask
        ? (true, value.AffinityValue.ToString("X"))
        : (false, "");

      HandleAffinityModeChange();
    }
  }

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
      AffinityValue = _affinityValue
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
  [ObservableProperty] ConfiguredProcess? _toEdit = null;

  public DesignAddProcessViewModel()
  {
    if (App.IsDesignMode) Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }

  [RelayCommand]
  Task ChooseProcess() => Task.CompletedTask;

  [RelayCommand]
  void AddProcess()
  {
    ToEdit = ToEdit is not null
      ? null
      : new();
  }

  [RelayCommand]
  void Cancel()
  {
  }
}