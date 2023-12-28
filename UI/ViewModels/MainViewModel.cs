using Domain;
using Domain.Infrastructure;

using UI.DomainWrappers;

using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Styling;

using ReactiveUI;
using ReactiveUI.ExtendedRouting;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Splat;

namespace UI.ViewModels;

public interface IMainViewModel : IScreen
{
  IAsyncRelayCommand GoToSettingsCommand { get; }

  IAsyncRelayCommand GoToAboutCommand { get; }

  IRelayCommand RunAsAdminCommand { get; }
  
  IAsyncRelayCommand ExportSettingsToFileCommand { get; }
  
  IAsyncRelayCommand ExportSettingsToFolderCommand { get; }
  
  IAsyncRelayCommand ImportSettingsFromFileCommand { get; }

  IRelayCommand ExitCommand { get; }
  
  bool IsCustomTitleBarUsed { get; }
  
  bool IsMenuVisible { get; }
  
  bool IsAppRunningWithAdmin { get; }
}

public partial class MainViewModel : ActivatableViewModelBase, IMainViewModel
{
  [ObservableProperty] bool _isCustomTitleBarUsed;
  [ObservableProperty] bool _isMenuVisible;
  [ObservableProperty] bool _isAppRunningWithAdmin;
  
  public RoutingState Router { get; } = new();

  readonly AppSettingChangeService _appSettingsService;
  
  public MainViewModel(MainWindowViewModel mainWindowViewModel, 
    AppSettingChangeService appSettingsService,
    AdminPrivilegesStatus privilegesStatus)
  {
    _appSettingsService = appSettingsService;
    var routeVmChanged = Router
      .CurrentViewModel
      .WhereNotNull();

    var showSystemTitleBarSettingChanged = Observable
      .FromEventPattern<AppSettings>(
        h => appSettingsService.AppSettingsChanged += h,
        h => appSettingsService.AppSettingsChanged -= h)
      .ObserveOn(RxApp.MainThreadScheduler)
      .Select(eventPattern => eventPattern.EventArgs.UiOptions.ShowSystemTitleBar);

    IsCustomTitleBarUsed = appSettingsService.CurrentAppSettings.UiOptions.ShowSystemTitleBar is false;
    IsAppRunningWithAdmin = privilegesStatus.IsAdmin;
    
    // ReSharper disable once AsyncVoidLambda
    this.WhenActivated(async d =>
    {
      routeVmChanged
        .Select(vm => vm switch
        {
          IAddProcessViewModel => "New process rule",
          ISelectCurrentlyRunnableProcessViewModel => "Selecting process",
          ISettingsViewModel => "Settings",
          IStartupViewModel or _ => mainWindowViewModel.DefaultWindowTitleText,
        })
        .Subscribe(newTitle => mainWindowViewModel.WindowTitleText = newTitle)
        .DisposeWith(d);
      
      routeVmChanged
        .Select(vm => vm is IStartupViewModel)
        .Subscribe(isVisible => IsMenuVisible = isVisible)
        .DisposeWith(d);
      
      showSystemTitleBarSettingChanged
        .Subscribe(systemTitleBarUsed => IsCustomTitleBarUsed = systemTitleBarUsed is false)
        .DisposeWith(d);

      _ = await Locator.Current
        .GetRequiredService<StartupViewModel>()
        .RouteThrough(Router);
    });
  }
  
  [RelayCommand]
  async Task GoToSettings()
    => await Locator.Current
      .GetRequiredService<SettingsViewModel>()
      .RouteThrough(this);

  [RelayCommand]
  async Task GoToAbout()
    => await Locator.Current
      .GetRequiredService<AboutViewModel>()
      .RouteThrough(this);

  [RelayCommand]
  void RunAsAdmin()
  {
    Application.Current
      ?.ApplicationLifetime
      ?.TryCastTo<IClassicDesktopStyleApplicationLifetime>()
      ?.Shutdown();
    _ = ProcessApi.RestartWithAdminPrivileges();
  }

  [RelayCommand]
  async Task ExportSettingsToFile()
  {
    var storageProvider = Locator.Current.GetRequiredService<IStorageProvider>();
    var pickedFiles = await storageProvider.OpenFilePickerAsync(new()
    {
      AllowMultiple = false,
    });

    if (pickedFiles is [var file])
    {
      var appSettings = _appSettingsService.CurrentAppSettings.WrapBeforeSerialization();
      await using var writeStream = await file.OpenWriteAsync();
      await JsonSerializer.SerializeAsync(writeStream, appSettings, SerializerOptions.Default);
    }
  }
  
  [RelayCommand]
  async Task ExportSettingsToFolder()
  {
    var storageProvider = Locator.Current.GetRequiredService<IStorageProvider>();
    var pickedFolders = await storageProvider.OpenFolderPickerAsync(new()
    {
      AllowMultiple = false,
    });

    if (pickedFolders is [var folder]
        && await folder.CreateFileAsync("appsettings.json") is {} file)
    {
      var appSettings = _appSettingsService.CurrentAppSettings.WrapBeforeSerialization();
      await using var writeStream = await file.OpenWriteAsync();
      await JsonSerializer.SerializeAsync(writeStream, appSettings, SerializerOptions.Default);
    }
  }
  
  [RelayCommand]
  async Task ImportSettingsFromFile()
  {
    var storageProvider = Locator.Current.GetRequiredService<IStorageProvider>();
    var pickedFiles = await storageProvider.OpenFilePickerAsync(new()
    {
      AllowMultiple = false,
    });

    if (pickedFiles is [var file])
    {
      await using var readStream = await file.OpenReadAsync();
      var newAppSettings = await JsonSerializer.DeserializeAsync<AppSettingsWrapperForHostOptions>(readStream, SerializerOptions.Default);
      if (newAppSettings is not null)
      {
        var fixedAppSettings = newAppSettings.AppSettings.ValidateAndFix();
        await _appSettingsService.MakeChangeAsync(_ => fixedAppSettings);
      }
    }
  }

  [RelayCommand]
  void Exit() => Application.Current
    ?.ApplicationLifetime
    ?.TryCastTo<IClassicDesktopStyleApplicationLifetime>()
    ?.Shutdown();
}

public sealed partial class DesignMainViewModel : ViewModelBase, IMainViewModel
{
  [ObservableProperty] bool _isCustomTitleBarUsed = true;
  [ObservableProperty] bool _isMenuVisible = true;
  [ObservableProperty] bool _isAppRunningWithAdmin;
  
  public RoutingState Router { get; } = new();
  
  public DesignMainViewModel()
  {
    Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
  }

  [RelayCommand]
  Task GoToSettings() => Task.CompletedTask;

  [RelayCommand]
  Task GoToAbout() => Task.CompletedTask;

  [RelayCommand]
  void RunAsAdmin() => IsAppRunningWithAdmin = !IsAppRunningWithAdmin;
  
  [RelayCommand]
  Task ExportSettingsToFile() => Task.CompletedTask;
  
  [RelayCommand]
  Task ExportSettingsToFolder() => Task.CompletedTask;

  [RelayCommand]
  Task ImportSettingsFromFile() => Task.CompletedTask;
  
  [RelayCommand]
  void Exit() { }
}

