using System.Collections.Generic;
using System.Linq;

public class RegisterExtract : RegisterActionBase
{
	private string _param;

	private bool _isBase;

	public RegisterExtract(bool isSelf, List<BattleCardBase> targetCards, string param, bool isBase)
	{
		targetCards.ForEach(delegate(BattleCardBase x)
		{
			base.IndexList.Add(x.Index);
		});
		IsSelf = isSelf;
		_param = param;
		_isBase = isBase;
	}

	public override Dictionary<string, object> MakeSendData()
	{
		Dictionary<string, object> dictionary = null;
		dictionary = base.MakeSendData();
		if (_isBase)
		{
			dictionary.Add(ActionBaseParameter.isBase.ToString(), "1");
		}
		dictionary.Add(ActionBaseParameter.var.ToString(), new List<object> { "v1" });
		dictionary.Add(ActionBaseParameter.param.ToString(), new List<object> { _param });
		return dictionary;
	}

	public static bool IsExtract(SkillBase skill)
	{
		if (skill.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.CardFilterList.Any((ISkillCardFilter cf) => cf is SkillLastTargetTribeFilter)))
		{
			return true;
		}
		if (skill.ApplyingTargetFilter is SkillTargetDeckFilter)
		{
			return skill.ApplyCardFilterList.Any((ISkillCardFilter s) => (s is SkillParameterBaseCostFilter && (s as SkillParameterBaseCostFilter).GetParameterText().Contains("fixed_generic_value")) ? true : false);
		}
		return false;
	}

	public override string GetUriMsg()
	{
		return RegisterTool.OrderListParameter.extract.ToString();
	}
}
