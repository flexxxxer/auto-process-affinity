using UI.ViewModels;

using Avalonia.ReactiveUI;

namespace UI.Views;
public partial class AboutView : ReactiveUserControl<IAboutViewModel>
{
  public AboutView()
  {
    InitializeComponent();
  }
}
