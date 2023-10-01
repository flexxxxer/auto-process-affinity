using Domain;
using Domain.Infrastructure;

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace UI.DomainWrappers;

public sealed class AppSettingSaveService
{
  readonly IHostEnvironment _hostEnvironment;

  public AppSettingSaveService(IHostEnvironment hostEnvironment)
  {
    _hostEnvironment = hostEnvironment;
  }

  public async Task SaveNewAppSetting(AppSettings appSettings)
  {
    var configFilePath = Path.Combine(_hostEnvironment.ContentRootPath, "appsettings.json");

    await using FileStream fileStream = new(configFilePath, FileMode.Create);
    await JsonSerializer.SerializeAsync(fileStream, appSettings.WrapBeforeSerialization(), SerializerOptions.Default);
  }
}
