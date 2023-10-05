using Domain;

namespace UI.DomainWrappers;

public sealed class AdminPrivilegesStatus
{
  public bool IsAdmin { get; }

  public AdminPrivilegesStatus()
  {
    IsAdmin = ProcessApi.IsCurrentProcessRunningWithAdminOrRootPrivileges() ?? false;
  }
}
