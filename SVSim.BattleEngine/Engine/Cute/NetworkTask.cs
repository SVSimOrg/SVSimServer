using System;
using System.Collections.Generic;
using System.Text;
using LitJson;
using MessagePack;
using UnityEngine.Networking;
using Wizard;
using Wizard.Battle.Recovery;

namespace Cute;

public class NetworkTask
{
	public enum ResultCode
	{
		Success,
		Error,
		TimeOut,
		Title,
		Maintenance
	}

	public enum ERROR_CODE_STATUS
	{
		NONE,
		ERROR,
		ERROR_TO_MAINTENANCE_POPUP
	}

	protected Dictionary<string, string> header = new Dictionary<string, string>();

	protected byte[] body;

	protected int resultCode;

	private SkipCuteCheckResultCodes skipCuteCheckResultCodes;

	private bool skipCommonTimeOutPopUp;

	private bool skipCommonHttpStatusErrorPopUp;

	public virtual string Url { get; set; }

	public Action<ResultCode> CallbackOnSuccess { get; set; }

	public Action<ResultCode> CallbackOnFailure { get; set; }

	public Action<int> CallbackOnResultCodeError { get; set; }

	public Action<UnityWebRequest> CallbackOnUnityWebRequestDone { get; set; }

	public Dictionary<string, string> Header => header;

	public byte[] Body => body;

	public PostParams Params { get; set; }

	public JsonData ResponseData { get; private set; }

	public bool IsResourceVersionUpError { get; private set; }

	public bool IsResultSuccess => resultCode == 1;

	public NetworkTask()
	{
		skipCuteCheckResultCodes = new SkipCuteCheckResultCodes();
		Params = new PostParams();
	}

	public void Initialize()
	{
		ResponseData = null;
		resultCode = 0;
	}

	public Dictionary<string, string> PrepareHeaders()
	{
		AddHeaderUdid();
		AddHeaderShortUdid();
		AddHeaderSessionId();
		AddHeaderParam();
		AddHeaderDevice();
		AddHeaderAppVersion();
		AddHeaderResVersion();
		AddHeaderDeviceId();
		AddHeaderDeviceName();
		AddHeaderGraphicsDeviceName();
		AddHeaderIpAddress();
		AddHeaderPlatformOsVersion();
		AddHeaderKeyChain();
		AddHeaderIDFA();
		AddHeaderLocale();
		AddHeaderLanguage();
		AddHeaderCountryCode();
		AddHeaderPlatform();
		AddHeaderIsWSS();
		AddHeaderIsIpv6();
		AddHeaderDevAccessSecretKey();
		AddCardMasterHash();
		return header;
	}

	public byte[] PreparePostData(bool encrypt = true, bool isUseJson = false)
	{
		return CreateBody(encrypt, isUseJson);
	}

	public void SetResponseData(JsonData data)
	{
		ResponseData = data;
		resultCode = getDataHeader()["result_code"].ToInt();
	}

	public ERROR_CODE_STATUS CheckResultCodeToPopupCreate_ReturnStatus(int rc = 0)
	{
		if (isAppVersionUP())
		{
			RecoveryRecordManagerBase.DeleteRecoveryFile();

			return ERROR_CODE_STATUS.ERROR;
		}
		if (isResourceVersionUp())
		{
			IsResourceVersionUpError = true;
			setResourceVersion();
			if (!Url.Contains(CuteNetworkDefine.ApiUrlList[CuteNetworkDefine.ApiType.GameStartCheck]))
			{
				RecoveryRecordManagerBase.DeleteRecoveryFile();

				Parse();
				return ERROR_CODE_STATUS.ERROR;
			}
		}
		if (isSessionError())
		{

			return ERROR_CODE_STATUS.ERROR;
		}
		setSession();
		if (isUnknownServerError() || isServerProcessedError() || isServerDataBaseError())
		{

			return ERROR_CODE_STATUS.ERROR;
		}
		if (isAccountBlockError())
		{

			return ERROR_CODE_STATUS.ERROR;
		}
		if (isNeteaseAccountBlockError())
		{
			NtDataTranslateManager.GetInstance().ShowRejectLogin();
			return ERROR_CODE_STATUS.ERROR;
		}
		if (isAccountLimitedBlockError())
		{
			string accountLimitedBlockEndTime = getAccountLimitedBlockEndTime();

			return ERROR_CODE_STATUS.ERROR;
		}
		if (IsAllMaintenanceError())
		{
			string maintenanceEndTime = getMaintenanceEndTime();

			return ERROR_CODE_STATUS.ERROR_TO_MAINTENANCE_POPUP;
		}
		if (IsEachFunctionMaintenanceError())
		{

			return ERROR_CODE_STATUS.ERROR_TO_MAINTENANCE_POPUP;
		}
		if (IsCardMaintenanceError())
		{
			if (CallbackOnResultCodeError != null)
			{
				CallbackOnResultCodeError(resultCode);
			}
			return ERROR_CODE_STATUS.ERROR_TO_MAINTENANCE_POPUP;
		}
		if (!skipCuteCheckResultCodes.isSkipAll() && !skipCuteCheckResultCodes.Contains(resultCode))
		{
			cuteCheckResultCode();
		}
		Parse();
		if (isServerResultCodeOK())
		{
			if (CallbackOnSuccess != null)
			{
				CallbackOnSuccess(ResultCode.Success);
			}
			return ERROR_CODE_STATUS.NONE;
		}
		if (CallbackOnResultCodeError != null)
		{
			CallbackOnResultCodeError(resultCode);
			return ERROR_CODE_STATUS.ERROR;
		}
		return ERROR_CODE_STATUS.NONE;
	}

	private void cuteCheckResultCode()
	{
	}

	protected virtual string getAccountLimitedBlockEndTime()
	{
		return ResponseData["data"]["account_block_end_time"].ToString();
	}

	protected virtual string getMaintenanceEndTime()
	{
		if (ResponseData["data"].Count > 0 && ResponseData["data"].Keys.Contains("maintenance_end_time") && ResponseData["data"]["maintenance_end_time"].ToString().Length > 0)
		{
			return ResponseData["data"]["maintenance_end_time"].ToString();
		}
		return "";
	}

	protected virtual string getUdid()
	{
		return Certification.Udid;
	}

	protected virtual byte[] CreateBody(bool encrypt = true, bool isUseJson = false)
	{
		if (isUseJson)
		{
			body = _createBodyJson(Params, encrypt);
		}
		else
		{
			body = _createBodyMsgpack(Params, encrypt);
		}
		return body;
	}

	protected byte[] _createBodyJson(PostParams Params, bool encrypt = true)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(JsonMapper.ToJson(Params));
		if (!encrypt)
		{
			return bytes;
		}
		return CryptAES.encrypt(bytes);
	}

	protected byte[] _createBodyMsgpack(PostParams Params, bool encrypt = true)
	{
		byte[] array = MessagePackSerializer.FromJson(JsonMapper.ToJson(Params));
		if (!encrypt)
		{
			return array;
		}
		return CryptAES.encrypt(array);
	}

	protected virtual int Parse()
	{
		return resultCode;
	}

	private void AddHeaderUdid()
	{
		if (Url.Contains(CuteNetworkDefine.ApiUrlList[CuteNetworkDefine.ApiType.SignUp]) || Url.Contains(CuteNetworkDefine.ApiUrlList[CuteNetworkDefine.ApiType.CheckSpecialTitle]) || Url.Contains(CuteNetworkDefine.ApiUrlList[CuteNetworkDefine.ApiType.CheckiCloudUser]) || Url.Contains(CuteNetworkDefine.ApiUrlList[CuteNetworkDefine.ApiType.MigrateiCloudUser]))
		{
			string value = Cryptographer.encode(getUdid());
			header["UDID"] = value;
		}
	}

	private void AddHeaderShortUdid()
	{
		string value = Cryptographer.encode(Certification.ShortUdid.ToString());
		header["SHORT_UDID"] = value;
	}

	private void AddHeaderSessionId()
	{
		header["SID"] = Certification.SessionId;
	}

	private void AddHeaderParam()
	{
		string udid = getUdid();
		string viewer_id = CryptAES.encrypt(0 /* Pre-Phase-5 chunk 39: Cute NetworkTask is client-only; dead headless */.ToString());
		Params.viewer_id = viewer_id;
		Params.steam_id = Certification.SteamID;
		Params.steam_session_ticket = Certification.SteamSessionTicket;
		string text = Convert.ToBase64String(MessagePackSerializer.FromJson(JsonMapper.ToJson(Params)));
		Uri uri = new Uri(Url.Trim());
		string text2 = udid + uri.AbsolutePath + text;
		if (0 /* Pre-Phase-5 chunk 39: Cute NetworkTask is client-only; dead headless */ != 0)
		{
			text2 += 0 /* Pre-Phase-5 chunk 39: Cute NetworkTask is client-only; dead headless */;
		}
		string value = Cryptographer.ComputeHash(text2);
		header["PARAM"] = value;
	}

	private void AddHeaderDevice()
	{
		header["DEVICE"] = Toolbox.DeviceManager.GetDeviceType().ToString();
	}

	private void AddHeaderAppVersion()
	{
		header["APP_VER"] = Toolbox.DeviceManager.GetAppVersionName();
	}

	private void AddHeaderResVersion()
	{
		header["RES_VER"] = Toolbox.SavedataManager.GetResourceVersion();
	}

	private void AddHeaderDeviceId()
	{
		header["DEVICE_ID"] = Toolbox.DeviceManager.GetDeviceUniqueIdentifier();
	}

	private void AddHeaderDeviceName()
	{
		header["DEVICE_NAME"] = Uri.EscapeDataString(Toolbox.DeviceManager.GetDeviceName());
	}

	private void AddHeaderGraphicsDeviceName()
	{
		header["GRAPHICS_DEVICE_NAME"] = Uri.EscapeDataString(Toolbox.DeviceManager.GetGraphicsDeviceName(textureCheck: true));
	}

	private void AddHeaderIpAddress()
	{
		header["IP_ADDRESS"] = Toolbox.DeviceManager.GetIpAddress();
	}

	private void AddHeaderPlatformOsVersion()
	{
		header["PLATFORM_OS_VERSION"] = Uri.EscapeDataString(Toolbox.DeviceManager.GetOsVersion());
	}

	private void AddHeaderPlatform()
	{
		header["PLATFORM"] = CustomPreference.GetPlatform().ToString();
	}

	private void AddHeaderIsWSS()
	{
		header["WSS"] = (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.IS_SELECT_WSS) ? "1" : "0");
	}

	private void AddHeaderIsIpv6()
	{
		header["IPV6_CONNECTION"] = (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.IS_SELECT_IPV6) ? "1" : "0");
	}

	private void AddCardMasterHash()
	{
		string cardMasterHash = CardMasterLocalFileUtility.GetCardMasterHash();
		if (!string.IsNullOrEmpty(cardMasterHash))
		{
			header["CARD_MASTER_HASH"] = cardMasterHash;
		}
	}

	private void AddHeaderDevAccessSecretKey()
	{
	}

	private void AddHeaderKeyChain()
	{
		header["KEYCHAIN"] = Certification.GetKeyChainViewerId();
	}

	private void AddHeaderIDFA()
	{
		header["IDFA"] = Certification.GetIDFA();
	}

	private void AddHeaderLocale()
	{
		header["LOCALE"] = Toolbox.DeviceManager.GetLocale();
	}

	private void AddHeaderLanguage()
	{
		string textLanguage = CustomPreference.GetTextLanguage();
		header["LANGUAGE"] = textLanguage;
	}

	private void AddHeaderCountryCode()
	{
		header["REGION_CODE"] = PlayerStaticData.UserRegionCode;
	}

	private bool isSessionError()
	{
		return resultCode == 201;
	}

	private bool isUnknownServerError()
	{
		return resultCode == 102;
	}

	private bool isAccountBlockError()
	{
		return resultCode == 203;
	}

	private bool isNeteaseAccountBlockError()
	{
		return resultCode == 330;
	}

	private bool isAccountLimitedBlockError()
	{
		return resultCode == 217;
	}

	private bool isServerProcessedError()
	{
		return resultCode == 213;
	}

	private bool isServerDataBaseError()
	{
		return resultCode == 100;
	}

	public bool isServerResultCodeOK()
	{
		if (resultCode != 1 && resultCode != 3502)
		{
			return resultCode == 1768;
		}
		return true;
	}

	private bool IsAllMaintenanceError()
	{
		return resultCode == 101;
	}

	private bool IsEachFunctionMaintenanceError()
	{
		if (resultCode >= 2000)
		{
			return resultCode <= 2999;
		}
		return false;
	}

	private bool IsCardMaintenanceError()
	{
		if (resultCode != 1710)
		{
			return resultCode == 5013;
		}
		return true;
	}

	private void setSession()
	{
		JsonData dataHeader = getDataHeader();
		if (dataHeader.Keys.Contains("sid") && dataHeader["sid"] != null && !string.IsNullOrEmpty(dataHeader["sid"].ToString()))
		{
			Certification.SessionId = dataHeader["sid"].ToString();
		}
	}

	private bool isAppVersionUP()
	{
		if (resultCode == 204)
		{
			return true;
		}
		return false;
	}

	private bool isResourceVersionUp()
	{
		if (getDataHeader().Keys.Contains("required_res_ver"))
		{
			return true;
		}
		return false;
	}

	private JsonData getDataHeader()
	{
		return ResponseData["data_headers"];
	}

	private void setResourceVersion()
	{
		string resourceVersion = getDataHeader()["required_res_ver"].ToString();
		Toolbox.SavedataManager.SetResourceVersion(resourceVersion);
	}

	public void SkipAllCuteResultCodeCheckErrorPopup()
	{
		skipCuteCheckResultCodes.setSkipAll(pSkipAll: true);
	}

	public void SkipCuteTimeOutPopup()
	{
		skipCommonTimeOutPopUp = true;
	}

	public bool isSkipCommonTimeOutPopUp()
	{
		return skipCommonTimeOutPopUp;
	}

	public void SkipCuteHttpStatusErrorPopup()
	{
		skipCommonHttpStatusErrorPopUp = true;
	}

	public bool isSkipCommonHttpStatusErrorPopUp()
	{
		return skipCommonHttpStatusErrorPopUp;
	}
}
