using Domain;

namespace UI.DomainWrappers;

public sealed class AdminPrivilegesStatus
{
  public bool IsAdmin { get; } = ProcessApi.IsCurrentProcessRunningWithAdminOrRootPrivileges() ?? false;
}
