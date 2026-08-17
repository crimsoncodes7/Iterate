using Iterate.Services;

namespace Iterate.ViewModels;

public partial class ProductivityViewModel : ViewModelBase
{
    private readonly Router _router;

    public ProductivityViewModel(Router router)
    {
        _router = router;
    }
}