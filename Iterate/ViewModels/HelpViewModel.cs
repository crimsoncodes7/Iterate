using Iterate.Services;

namespace Iterate.ViewModels;

public partial class HelpViewModel : ViewModelBase
{
    private readonly Router _router;

    public HelpViewModel(Router router)
    {
        _router = router;
    }
}