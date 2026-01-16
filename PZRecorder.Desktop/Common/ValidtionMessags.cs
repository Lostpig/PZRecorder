using PZ.RxAvalonia.DataValidations;

namespace PZRecorder.Desktop.Common;

internal static class ValidtionMessags
{
    public static void SetValidtionMessags()
    {
        RequiredValidation.GlobalMsgGetter = (_) => LD.RequiredInvalidMsg;
        MaxLengthValidation.GlobalMsgGetter = (v) => string.Format(LD.MaxLengthInvalidMsg, v.MaxLength);
        MinLengthValidation.GlobalMsgGetter = (v) => string.Format(LD.MinLengthInvalidMsg, v.MinLength);
        MaxValueValidation<int>.GlobalMsgGetter = (v) => string.Format(LD.MaxValueInvalidMsg, v.MaxValue);
        MinValueValidation<int>.GlobalMsgGetter = (v) => string.Format(LD.MinValueInvalidMsg, v.MinValue);
    }
}
