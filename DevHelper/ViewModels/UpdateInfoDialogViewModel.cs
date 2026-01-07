using CommunityToolkit.Mvvm.ComponentModel;
using Control.Basic;
using Framework.Common;

namespace UniClient;

public partial class UpdateInfoDialogViewModel : ConfirmDialogViewModel
{
    [ObservableProperty] private bool _isForce = false;
    [ObservableProperty] private string _changeLog = string.Empty;
}