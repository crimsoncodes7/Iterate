using CommunityToolkit.Mvvm.Input;
using Iterate.Services;

namespace Iterate.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly Router _router;

    public HomeViewModel(Router router)
    {
        _router = router;
    }

    [RelayCommand]
    private void GoToSubjects()
    {
        _router.NavigateTo<SubjectsViewModel>();
    }
}