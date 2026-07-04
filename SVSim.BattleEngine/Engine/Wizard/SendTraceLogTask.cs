namespace Wizard;

public class SendTraceLogTask : BaseTask
{
	public class SendTraceLastLogParam : BaseParam
	{
		public string log;
	}

	public class SendTraceLogParam : SendTraceLastLogParam
	{
		public string setting_log;
	}

	public class SendAllTraceLogParam : SendTraceLastLogParam
	{
		public string last_log;

		public string setting_log;
	}

	public SendTraceLogTask(LocalLog.TRACELOG_TYPE logType)
	{
		switch (logType)
		{
		case LocalLog.TRACELOG_TYPE.TRACE_ALL_LOG:
			base.type = ApiType.Type.SendAllTraceLog;
			break;
		case LocalLog.TRACELOG_TYPE.TRACE_LOG:
			base.type = ApiType.Type.SendTraceLog;
			break;
		case LocalLog.TRACELOG_TYPE.TRACE_LAST_LOG:
			base.type = ApiType.Type.SendLastTraceLog;
			break;
		}
	}

	public void SetParameter(string last_log)
	{
		SendTraceLastLogParam sendTraceLastLogParam = new SendTraceLastLogParam();
		sendTraceLastLogParam.log = last_log;
		base.Params = sendTraceLastLogParam;
	}

	public void SetParameter(string log, string setting_log)
	{
		SendTraceLogParam sendTraceLogParam = new SendTraceLogParam();
		sendTraceLogParam.log = log;
		sendTraceLogParam.setting_log = setting_log;
		base.Params = sendTraceLogParam;
	}

	public void SetParameter(string log, string last_log, string setting_log)
	{
		SendAllTraceLogParam sendAllTraceLogParam = new SendAllTraceLogParam();
		sendAllTraceLogParam.log = log;
		sendAllTraceLogParam.last_log = last_log;
		sendAllTraceLogParam.setting_log = setting_log;
		base.Params = sendAllTraceLogParam;
	}

	protected override int Parse()
	{
		int result = base.Parse();
		_ = 1;
		return result;
	}
}
