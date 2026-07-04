using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayAttachStyle : AIWhenPlayTagArgument
{
	private readonly char[] POLICY_TRIM_CHARS = new char[3] { '{', '}', ' ' };

	public AIPolicyData Policy { get; private set; }

	public AIWhenPlayAttachStyle(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.SelectType = AIScriptTokenArgType.NONE;
		base.Filters = null;
		string[] words = SplitTextToWords(text);
		InitializePolicyFromWords(words);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (field.IsLatestActionField)
		{
			field.StyleQuery.ExecuteAttachStyle(Policy);
		}
	}

	private void InitializePolicyFromWords(string[] words)
	{
		AIPolicyDataAsset aIPolicyDataAsset = new AIPolicyDataAsset();
		aIPolicyDataAsset.Category = words[0].Trim(POLICY_TRIM_CHARS);
		if (int.TryParse(words[1].Trim(POLICY_TRIM_CHARS), out var result))
		{
			aIPolicyDataAsset.Priority = result;
		}
		else
		{
			AIConsoleUtility.LogError("AIWhenPlayAttachStyle Priority Argument Error!! " + words[1]);
			aIPolicyDataAsset.Priority = 0;
		}
		aIPolicyDataAsset.Type = words[2].Trim(POLICY_TRIM_CHARS);
		aIPolicyDataAsset.Arg = words[3].Trim(POLICY_TRIM_CHARS);
		aIPolicyDataAsset.Cond = words[4].Trim(POLICY_TRIM_CHARS);
		aIPolicyDataAsset.ID = -1;
		Policy = new AIPolicyData(aIPolicyDataAsset);
	}
}
