using System.Collections;
using Cute;
using LitJson;
using Wizard;

public class NtDataTranslateInfo
{

	public string titleStatement = "";

	public string containStatement = "";

	public string titleEmail = "";

	public string emailAddress1 = "";

	public string emailAddress2 = "";

	public string emailAddressTip1 = "";

	public string emailAddressTip2 = "";

	public string titleSuccess = "";

	public string containSuccess = "";

	public string titleTip = "";

	public string containTip = "";

	public string titleRebind = "";

	public string containRebind = "";

	public string ERROR_TITLE = "";

	public string ERROR_CONTAIN1 = "";

	public string ERROR_CONTAIN2 = "";

	public string ERROR_CONTAIN4 = "";

	public string ERROR_CONTAIN5 = "";

	public string ERROR_CONTAIN6 = "";

	public string ERROR_CONTAIN7 = "";

	public string serverUrl = "";

	public string buttonTitle = "";

	public string button_id1 = "";

	public string button_id2 = "";

	public string button_id3 = "";

	public string button_id4 = "";

	public string button_id5 = "";

	public string button_id6 = "";

	public string button_id7 = "";

	public static NtDataTranslateInfo Init()
	{
		SystemText systemText = Data.SystemText;
		return new NtDataTranslateInfo
		{
			titleStatement = systemText.Get("Account_0113"),
			containStatement = systemText.Get("Account_0114"),
			titleEmail = systemText.Get("Account_0115"),
			emailAddress1 = systemText.Get("Account_0116"),
			emailAddress2 = systemText.Get("Account_0117"),
			emailAddressTip1 = systemText.Get("Account_0118"),
			emailAddressTip2 = systemText.Get("Account_0119"),
			titleSuccess = systemText.Get("Account_0120"),
			containSuccess = systemText.Get("Account_0121"),
			titleTip = systemText.Get("Account_0122"),
			containTip = systemText.Get("Account_0123"),
			titleRebind = systemText.Get("Account_0124"),
			containRebind = systemText.Get("Account_0125"),
			ERROR_TITLE = systemText.Get("Account_0126"),
			ERROR_CONTAIN1 = systemText.Get("Account_0127"),
			ERROR_CONTAIN2 = systemText.Get("Account_0128"),
			ERROR_CONTAIN4 = systemText.Get("Account_0130"),
			ERROR_CONTAIN5 = systemText.Get("Account_0131"),
			ERROR_CONTAIN6 = systemText.Get("Account_0132"),
			ERROR_CONTAIN7 = systemText.Get("Account_0133"),
			serverUrl = CustomPreference.GetApplicationServerURL(),
			buttonTitle = systemText.Get("Account_0137"),
			button_id1 = systemText.Get("Account_0134"),
			button_id2 = systemText.Get("Account_0135"),
			button_id3 = systemText.Get("Account_0136"),
			button_id4 = systemText.Get("Account_0136"),
			button_id5 = systemText.Get("Account_0134"),
			button_id6 = systemText.Get("Account_0135"),
			button_id7 = systemText.Get("Account_0136")
		};
	}
}
