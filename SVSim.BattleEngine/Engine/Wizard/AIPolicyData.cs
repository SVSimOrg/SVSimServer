using System.Collections.Generic;

namespace Wizard;

public class AIPolicyData
{
	private AIConditionExpressions _conditionExpr;

	public int ID { get; private set; }

	public AICategory Category { get; private set; }

	public int Priority { get; private set; }

	public AIPolicyType PolicyType { get; private set; }

	public string ARG { get; private set; }

	public string CONDITION { get; private set; }

	public AIScriptArgumentExpressions Argument { get; private set; }

	public AIPolicyData(AIPolicyDataAsset asset)
	{
		ID = asset.ID;
		Category = ConvertStringToCategory(asset.Category);
		Priority = asset.Priority;
		PolicyType = ConvertStringToPolicyType(asset.Type);
		ARG = asset.Arg;
		CONDITION = asset.Cond;
		Argument = CreateArgument(asset.Arg);
		_conditionExpr = new AIConditionExpressions(asset.Cond);
	}

	private AIScriptArgumentExpressions CreateArgument(string arg)
	{
		switch (PolicyType)
		{
		case AIPolicyType.PlayBreak:
			return new AIPlayBreak(arg);
		case AIPolicyType.EmoOnTurnEnd:
		case AIPolicyType.EmoOnTurnStart:
		case AIPolicyType.PlayerEmoOnTurnEnd:
			return new AIEmoteOnTurnTransition(arg);
		case AIPolicyType.AllyPlayBonus:
			return new AIOtherPlayBonus(arg);
		case AIPolicyType.AllyPlayBonusRate:
			return new AIOtherPlayBonusRate(arg);
		case AIPolicyType.DelayTurnEndTime:
			return new AIDelayTurnEndTime(arg);
		case AIPolicyType.MoveFirstBonus:
			return new AIFirstMoveBonus(arg);
		case AIPolicyType.GameStartAttachTag:
			return new AIGameStartAttachTag(arg);
		default:
			return new AIScriptArgumentExpressions(arg);
		}
	}

	public static AICategory ConvertStringToCategory(string str)
	{
		return str switch
		{
			"All" => AICategory.ALL, 
			"Elf" => AICategory.ELF, 
			"Royal" => AICategory.ROYAL, 
			"Witch" => AICategory.WITCH, 
			"Dragon" => AICategory.DRAGON, 
			"Necromance" => AICategory.NECROMANCE, 
			"Vampire" => AICategory.VAMPIRE, 
			"Bishop" => AICategory.BISHOP, 
			"Nemesis" => AICategory.NEMESIS, 
			_ => AICategory.ALL, 
		};
	}

	public static AIPolicyType ConvertStringToPolicyType(string str)
	{
		return str switch
		{
			"epValue" => AIPolicyType.EpValue, 
			"modUnitRate" => AIPolicyType.ModUnitRate, 
			"unitBonus" => AIPolicyType.UnitBonus, 
			"emoOnTurnEnd" => AIPolicyType.EmoOnTurnEnd, 
			"emoOnTurnStart" => AIPolicyType.EmoOnTurnStart, 
			"playptnBonus" => AIPolicyType.PlayptnBonus, 
			"playBreak" => AIPolicyType.PlayBreak, 
			"barrierBonus" => AIPolicyType.BarrierBonus, 
			"allyPlayB" => AIPolicyType.AllyPlayBonus, 
			"allyPlayBonusRate" => AIPolicyType.AllyPlayBonusRate, 
			"disableLethalCheck" => AIPolicyType.DisableLethalCheck, 
			"delayTurnEndTime" => AIPolicyType.DelayTurnEndTime, 
			"emoOnLeaderDamaged" => AIPolicyType.EmoOnLeaderDamaged, 
			"setReferenceId" => AIPolicyType.SetReferenceId, 
			"setReferenceTribe" => AIPolicyType.SetReferenceTribe, 
			"playerEmoOnTurnEnd" => AIPolicyType.PlayerEmoOnTurnEnd, 
			"playerEmoOnTurnStart" => AIPolicyType.PlayerEmoOnTurnStart, 
			"playerEmoOnLeaderDamaged" => AIPolicyType.PlayerEmoOnLeaderDamaged, 
			"moveFirstBonus" => AIPolicyType.MoveFirstBonus, 
			"gameStartAttachTag" => AIPolicyType.GameStartAttachTag, 
			_ => AIPolicyType.None, 
		};
	}

	public bool CheckCondition(AIVirtualCard owner, List<int> playPtn, AIVirtualField field, AISituationInfo situation)
	{
		return _conditionExpr.CheckCondition(owner, playPtn, field, situation);
	}

	public float EvalArg(AIVirtualCard owner, List<int> playPtn, AIVirtualField field, AISituationInfo situation = null)
	{
		return Argument.EvalArg(0, owner, playPtn, field, situation);
	}

	public int EvalId(int index = 0)
	{
		return Argument.EvalID(index);
	}

	public List<int> EvalIdList(int startindex = 0)
	{
		return Argument.EvalIDList(startindex);
	}

	public string EvalText(int index = 0)
	{
		return Argument.EvalText(index);
	}
}
