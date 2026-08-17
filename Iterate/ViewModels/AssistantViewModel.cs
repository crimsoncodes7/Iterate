using Iterate.Services;

namespace Iterate.ViewModels;

public partial class AssistantViewModel : ViewModelBase
{
    private readonly Router _router;

    public AssistantViewModel(Router router)
    {
        _router = router;
    }
}