using Cute;
using Wizard.Battle.Recovery;

namespace Wizard;

public class BaseTask : NetworkTask
{
	public override string Url
	{
		get
		{
			string text = "";
			text = ApiType.ApiList[type];
			return $"{CustomPreference.GetApplicationServerURL()}{text}";
		}
	}

	protected ApiType.Type type { get; set; }

	public BaseTask()
	{
		type = ApiType.Type.Load;
	}

	protected override int Parse()
	{
		resultCode = (int)base.ResponseData["data_headers"]["result_code"];
		long setServerTime = base.ResponseData["data_headers"]["servertime"].ToLong();
		PlayerStaticData.UserTime.Set(setServerTime);
		return resultCode;
	}

	public static void OnRequestFailed(ResultCode errorcode)
	{
	}

	public static void OnFailedErrorCode(int code)
	{
	}

	protected void DeleteRecoveryFileIfBattleAlreadyEnded(int resultCode)
	{
		if (resultCode == 1352)
		{
			RecoveryRecordManagerBase.DeleteRecoveryFile();
		}
	}
}
