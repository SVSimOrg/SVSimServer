using System;
using System.Collections.Generic;

public class RegisterPlayerParameter : RegisterActionBase
{
	private ActionBaseParameter _param;

	private int _paramValue;

	public RegisterPlayerParameter(ActionBaseParameter param, int paramValue, bool self)
	{
		_param = param;
		_paramValue = paramValue;
		IsSelf = self;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = base.MakeSendData();
		dictionary.Add(value: (_param == ActionBaseParameter.set || _param == ActionBaseParameter.cemetery || _param == ActionBaseParameter.maxPP) ? _paramValue : Math.Abs(_paramValue), key: _param.ToString());
		return dictionary;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.playerParam.ToString();
	}

	public override bool IsUseLotCard(RegisterLotCardBase lot)
	{
		return false;
	}
}
