using CommunityToolkit.Mvvm.Input;
using Framework.Common;
using Irihi.Avalonia.Shared.Contracts;

namespace Control.Basic;

public partial class ConfirmDialogViewModel : UniViewModel, IDialogContext
{
    #region IDialogContext
    public void Close()
    {
        RequestClose?.Invoke(this, null);
    }

    public event EventHandler<object?>? RequestClose;
    #endregion
    
    
    #region Commands
    public event Func<bool>? OnConfirmEvent;
    
    [RelayCommand] private void Ok()
    {
        bool? checkResult = OnConfirmEvent?.Invoke();
        if (false == checkResult)
        {
            return;
        }
        RequestClose?.Invoke(this, new ConfirmDialogResult()
        {
            IsConfirmed = true,
            ReturnParameter = View is ConfirmDialog dialog ? dialog.ReturnParameter : null
        });
    }
    
    [RelayCommand] private void Cancel()
    {
        RequestClose?.Invoke(this, new ConfirmDialogResult()
        {
            IsConfirmed = false,
            ReturnParameter = null
        });
    }
    #endregion
}