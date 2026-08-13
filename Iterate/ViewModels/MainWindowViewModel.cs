using Iterate.Services;

namespace Iterate.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public Router Router { get; }

    public MainWindowViewModel(Router router)
    {
        Router = router;
    }
}