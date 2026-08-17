using Iterate.Services;

namespace Iterate.ViewModels;

public partial class ReviewViewModel : ViewModelBase
{
    private readonly Router _router;

    public ReviewViewModel(Router router)
    {
        _router = router;
    }
}