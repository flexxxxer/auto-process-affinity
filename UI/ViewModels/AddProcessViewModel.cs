using Domain;
using Domain.Infrastructure;

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Styling;

using ReactiveUI.ExtendedRouting;

using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

using Splat;

namespace UI.ViewModels;

public interface IAddProcessViewModel
{
  string ProcessName { get; set; }

  bool IsEvenAffinityModeChosen { get; set; }

  bool IsFirstNAffinityModeChosen { get; set; }

  bool IsLastNAffinityModeChosen { get; set; }

  bool IsCustomAffinityModeChosen { get; set; }

  int? EvenAffinityModeFirstNValue { get; set; }

  int? FirstNAffinityModeValue { get; set; }

  int? LastNAffinityModeValue { get; set; }

  long? CustomAffinityModeValue { get; set; }
  
  bool IsAllWithMatchedNameAffinityApplyingModeChosen { get; set; }
  
  bool IsFirstWithMatchedNameAffinityApplyingModeChosen { get; set; }
  
  bool IsCaseSensitiveAffinityApplyingMode { get; set; }

  ConfiguredProcess? ToEdit { get; }

  IAsyncRelayCommand ChooseProcessCommand { get; }

  IRelayCommand AddProcessCommand { get; }

  IRelayCommand CancelCommand { get; }
}

public sealed partial class AddProcessViewModel : RoutableAndActivatableViewModelBase, IAddProcessViewModel
{
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] string _processName = "";
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isEvenAffinityModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isFirstNAffinityModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isLastNAffinityModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isCustomAffinityModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] int? _evenAffinityModeFirstNValue;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] int? _firstNAffinityModeValue;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] int? _lastNAffinityModeValue;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] long? _customAffinityModeValue;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isAllWithMatchedNameAffinityApplyingModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isFirstWithMatchedNameAffinityApplyingModeChosen;
  [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(AddProcessCommand))] bool _isCaseSensitiveAffinityApplyingMode;

  [ObservableProperty] ConfiguredProcess? _toEdit;

  readonly TaskCompletionSource<ConfiguredProcess?> _resultSource = new();
  public Task<ConfiguredProcess?> Result => _resultSource.Task;

  AffinityApplyingMode _affinityApplyingMode;
  AffinityMode _affinityMode;
  long _affinityValue;

  void HandleAffinityModeChange()
  {
    _affinityApplyingMode = (null as object) switch
    {
      _ when IsAllWithMatchedNameAffinityApplyingModeChosen => AffinityApplyingMode.AllWithMatchedName,
      _ when IsFirstWithMatchedNameAffinityApplyingModeChosen => AffinityApplyingMode.FirstWithMatchedName,
      _ => _affinityApplyingMode
    };
    (_affinityValue, _affinityMode) = (null as object) switch
    {
      _ when IsEvenAffinityModeChosen => EvenAffinityModeFirstNValue switch
      {
        { } firstNValue  => (firstNValue, AffinityMode.FirstNEven),
        null => (0 /* any value not matter */, AffinityMode.AllEven)
      },

      _ when IsFirstNAffinityModeChosen && FirstNAffinityModeValue is { } firstN
        => (firstN, AffinityMode.FirstN),

      _ when IsLastNAffinityModeChosen && LastNAffinityModeValue is { } lastN
        => (lastN, AffinityMode.LastN),

      _ when IsCustomAffinityModeChosen && CustomAffinityModeValue is { } newMask
        => (newMask, AffinityMode.CustomBitmask),

      _ => (_affinityValue, _affinityMode)
    };

    CustomAffinityModeValue = AffinityApi.BitmaskFrom(_affinityMode, _affinityValue);
  }

  // ReSharper disable UnusedParameterInPartialMethod
  partial void OnIsEvenAffinityModeChosenChanged(bool value) => HandleAffinityModeChange();

  partial void OnIsFirstNAffinityModeChosenChanged(bool value) => HandleAffinityModeChange();

  partial void OnIsLastNAffinityModeChosenChanged(bool value) => HandleAffinityModeChange();

  partial void OnIsCustomAffinityModeChosenChanged(bool value) => HandleAffinityModeChange();

  partial void OnEvenAffinityModeFirstNValueChanged(int? value) => HandleAffinityModeChange();

  partial void OnFirstNAffinityModeValueChanged(int? value) => HandleAffinityModeChange();

  partial void OnLastNAffinityModeValueChanged(int? value) => HandleAffinityModeChange();

  partial void OnCustomAffinityModeValueChanged(long? value) => HandleAffinityModeChange();
  
  partial void OnIsAllWithMatchedNameAffinityApplyingModeChosenChanged(bool value) => HandleAffinityModeChange();

  partial void OnIsFirstWithMatchedNameAffinityApplyingModeChosenChanged(bool value) => HandleAffinityModeChange();
  // ReSharper restore UnusedParameterInPartialMethod


  partial void OnToEditChanged(ConfiguredProcess? value)
  {
    if(value is not null)
    {
      ToEdit = value;
      ProcessName = value.Name;
      IsCaseSensitiveAffinityApplyingMode = value.IsCaseSensitive;

      (IsAllWithMatchedNameAffinityApplyingModeChosen, IsFirstWithMatchedNameAffinityApplyingModeChosen) =
        value.AffinityApplyingMode switch
        {
          AffinityApplyingMode.AllWithMatchedName => (true, false),
          AffinityApplyingMode.FirstWithMatchedName => (false, true),
          _ => throw new ArgumentOutOfRangeException()
        };

      (IsEvenAffinityModeChosen, EvenAffinityModeFirstNValue) = value.AffinityMode switch
      {
        AffinityMode.AllEven => (true, null),
        AffinityMode.FirstNEven => (true, (int?)value.AffinityValue),
        _ => (false, null)
      };

      (IsFirstNAffinityModeChosen, FirstNAffinityModeValue) = value.AffinityMode is AffinityMode.FirstN
        ? (true, (int?)value.AffinityValue)
        : (false, null);

      (IsLastNAffinityModeChosen, LastNAffinityModeValue) = value.AffinityMode is AffinityMode.LastN
        ? (true, (int?)value.AffinityValue)
        : (false, null);

      (IsCustomAffinityModeChosen, CustomAffinityModeValue) = value.AffinityMode is AffinityMode.CustomBitmask
        ? (true, (int?)value.AffinityValue)
        : (false, null);

      HandleAffinityModeChange();
    }
  }

  bool CanAddProcess() =>
    !ProcessName.IsNullOrWhiteSpace()
    && (
      (IsEvenAffinityModeChosen && EvenAffinityModeFirstNValue is null or > 1)
      || (IsFirstNAffinityModeChosen && FirstNAffinityModeValue is not null)
      || (IsLastNAffinityModeChosen && LastNAffinityModeValue is not null)
      || (IsCustomAffinityModeChosen && CustomAffinityModeValue is not null)
    );

  [RelayCommand]
  async Task ChooseProcess()
  {
    var vm = await Locator.Current
      .GetRequiredService<SelectCurrentlyRunnableProcessViewModel>()
      .RouteThrough(HostScreen);

    if (await vm.Result is { } selectedProcess) ProcessName = selectedProcess.Name;
  }

  [RelayCommand(CanExecute = nameof(CanAddProcess))]
  void AddProcess()
  {
    _resultSource.SetResult(new()
    {
      Name = ProcessName,
      AffinityMode = _affinityMode,
      AffinityValue = _affinityValue,
      AffinityApplyingMode = _affinityApplyingMode,
      IsCaseSensitive = IsCaseSensitiveAffinityApplyingMode,
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
  [ObservableProperty] int? _evenAffinityModeFirstNValue;
  [ObservableProperty] int? _firstNAffinityModeValue;
  [ObservableProperty] int? _lastNAffinityModeValue;
  [ObservableProperty] long? _customAffinityModeValue;
  [ObservableProperty] bool _isAllWithMatchedNameAffinityApplyingModeChosen;
  [ObservableProperty] bool _isFirstWithMatchedNameAffinityApplyingModeChosen;
  [ObservableProperty] bool _isCaseSensitiveAffinityApplyingMode;
  [ObservableProperty] ConfiguredProcess? _toEdit;

  public DesignAddProcessViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
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