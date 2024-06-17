using MudBlazor;
using PZPKRecorder.Components.Dialogs;

namespace PZPKRecorder.Services;

public enum MessageBoxType
{
    Info = 0,
    Warning = 1,
    Error = 2,
}

internal static  class CustomDialog
{
    public static async Task<bool> ShowMessageDialog(this IDialogService ds, string message, MessageBoxType msgType, bool isConfirm = false, string? OKText = null, string? CancelText = null)
    {
        DialogParameters<MessageDialog> paramters = new();
        paramters.Add(d => d.MsssageType, msgType);
        paramters.Add(d => d.Message, message);
        paramters.Add(d => d.Confirm, isConfirm);
        if (!string.IsNullOrWhiteSpace(OKText)) paramters.Add(d => d.OKText, OKText);
        if (!string.IsNullOrWhiteSpace(CancelText)) paramters.Add(d => d.CancelText, CancelText);

        var dialog = await ds.ShowAsync<MessageDialog>("", paramters);
        var result = await dialog.Result;

        return result.Canceled ? false : true;
    }

    public static Task<bool> ShowAlert(this IDialogService ds, string message, string? buttonText = null)
    {
        return ShowMessageDialog(ds, message, MessageBoxType.Info, false, buttonText);
    }
    public static Task<bool> ShowError(this IDialogService ds, string message, string? buttonText = null)
    {
        return ShowMessageDialog(ds, message, MessageBoxType.Error, false, buttonText);
    }
    public static Task<bool> ShowWarning(this IDialogService ds, string message, string? buttonText = null)
    {
        return ShowMessageDialog(ds, message, MessageBoxType.Error, false, buttonText);
    }
    public static Task<bool> ShowConfirm(this IDialogService ds, string message, string? OKText = null, string? CancelText = null)
    {
        return ShowMessageDialog(ds, message, MessageBoxType.Info, true, OKText, CancelText);
    }
}
