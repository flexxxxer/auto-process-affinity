using Domain;

using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Styling;

using ReactiveUI;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using DynamicData;
using DynamicData.Binding;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Options;
using Domain.Infrastructure;
using UI.DomainWrappers;

namespace UI.ViewModels;

public interface ISelectCurrentlyRunnableProcessViewModel
{
  string SearchText { get; set; }

  bool ShouldDescriptionColumnBeHidden { get; }

  CurrentlyRunnedProcessDto? SelectedRunningProcess { get; set; }

  ReadOnlyObservableCollection<CurrentlyRunnedProcessDto> CurrentlyRunningProcesses { get; }

  IRelayCommand ConfirmChoiceCommand { get; }

  IRelayCommand CancelCommand { get; }
}

public partial class SelectCurrentlyRunnableProcessViewModel : RoutableAndActivatableViewModelBase,
  ISelectCurrentlyRunnableProcessViewModel
{
  [ObservableProperty] string _searchText = "";
  [ObservableProperty] bool _shouldDescriptionColumnBeHidden;
  [ObservableProperty] ReadOnlyObservableCollection<CurrentlyRunnedProcessDto> _currentlyRunningProcesses;
  readonly SourceList<CurrentlyRunnedProcessDto> _currentlyRunningProcessesSource = new();

  [ObservableProperty]
  [NotifyCanExecuteChangedFor(nameof(ConfirmChoiceCommand))]
  CurrentlyRunnedProcessDto? _selectedRunningProcess;

  readonly TaskCompletionSource<CurrentlyRunnedProcessDto?> _resultSource = new();
  public Task<CurrentlyRunnedProcessDto?> Result => _resultSource.Task;

  public SelectCurrentlyRunnableProcessViewModel(CurrentlyRunnableProcessesService processesService,
    AppSettingChangeService appSettingsService)
  {
    static Func<CurrentlyRunnedProcessDto, bool> BuildFilter(string? searchText)
        => string.IsNullOrWhiteSpace(searchText)
            ? (_ => true)
            : (p => p.Name.ContainsText(searchText)
              || p.ProcessId.ToString().ContainsText(searchText)
              || p.Description.ContainsText(searchText));

    var searchTextChanged = this.WhenValueChanged(vm => vm.SearchText)
      .Throttle(TimeSpan.FromMilliseconds(300));

    var searchTextAction = searchTextChanged
      .ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(_ => SelectedRunningProcess = null);

    var filterOnlyMatchingText = searchTextChanged
      .Select(BuildFilter);

    var processesSourceObservable = _currentlyRunningProcessesSource
      .Connect()
      .RefCount()
      .Filter(filterOnlyMatchingText)
      .ObserveOn(RxApp.MainThreadScheduler)
      .Bind(out _currentlyRunningProcesses) // must be initialized in constructor
      .DisposeMany()
      .Subscribe(_ => { }, RxApp.DefaultExceptionHandler.OnNext);

    HandleAppSettingsChanged(appSettingsService.CurrentAppSettings);

    // ReSharper disable once AsyncVoidLambda
    this.WhenActivated(async d =>
    {
      await processesService.InitAsync();
      _currentlyRunningProcessesSource.AddRange(processesService.CurrentlyRunningProcesses);

      Observable
        .FromEventPattern<AppSettings>(
          h => appSettingsService.AppSettingsChanged += h,
          h => appSettingsService.AppSettingsChanged -= h)
        .Throttle(TimeSpan.FromSeconds(0.6))
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(eventPattern => HandleAppSettingsChanged(eventPattern.EventArgs))
        .DisposeWith(d);

      // must be after previous line
      Observable
        .FromEventPattern<CurrentlyRunnableProcessesService.UpdateChangeset>(h => processesService.Update += h, h => processesService.Update -= h)
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(eventPattern => HandleCurrentlyRunningProcessesChangeset(eventPattern.EventArgs))
        .DisposeWith(d);

      searchTextAction.DisposeWith(d);
      processesSourceObservable.DisposeWith(d);
    });
  }

  void HandleAppSettingsChanged(AppSettings newAppSettings)
  {
    ShouldDescriptionColumnBeHidden = newAppSettings.UxOptions.HideProcessDescriptionFromSelectingProcessView;
  }

  void HandleCurrentlyRunningProcessesChangeset(CurrentlyRunnableProcessesService.UpdateChangeset changeset)
  {
    if (changeset.Dead is not []) _currentlyRunningProcessesSource.RemoveMany(changeset.Dead);
    if (changeset.Created is not []) _currentlyRunningProcessesSource.AddRange(changeset.Created);
  }

  bool CanConfirmChoice() => SelectedRunningProcess is not null;

  [RelayCommand(CanExecute = nameof(CanConfirmChoice))]
  void ConfirmChoice()
  {
    _resultSource.SetResult(SelectedRunningProcess);
    HostScreen.Router.NavigateBack.Execute();
  }

  [RelayCommand]
  void Cancel()
  {
    _resultSource.SetResult(SelectedRunningProcess);
    HostScreen.Router.NavigateBack.Execute();
  }
}

public sealed partial class DesignSelectCurrentlyRunnableProcessViewModel : ViewModelBase,
  ISelectCurrentlyRunnableProcessViewModel
{
  [ObservableProperty] string _searchText = "";
  [ObservableProperty] bool _shouldDescriptionColumnBeHidden = false;
  [ObservableProperty] ReadOnlyObservableCollection<CurrentlyRunnedProcessDto> _currentlyRunningProcesses = new(new());
  [ObservableProperty] CurrentlyRunnedProcessDto? _selectedRunningProcess;

  public DesignSelectCurrentlyRunnableProcessViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;

    var exampleProcesses = Process.GetProcesses()
      .OrderBy(p => p.ProcessName)
      .Skip(5)
      .Take(15)
      .Select(p => new CurrentlyRunnedProcessDto(p));

    CurrentlyRunningProcesses = new(new(exampleProcesses));
  }

  [RelayCommand]
  void ConfirmChoice() { }

  [RelayCommand]
  void Cancel() { }
}