using CommunityToolkit.Mvvm.Input;
using Iterate.Services;

namespace Iterate.ViewModels;

public partial class SubjectsViewModel : ViewModelBase
{
    private readonly Router _router;

    public SubjectsViewModel(Router router)
    {
        _router = router;
    }

    [RelayCommand]
    private void GoToHome() => _router.NavigateTo<HomeViewModel>();
}