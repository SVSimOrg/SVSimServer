namespace Wizard;

public class PlayedCardInfo
{
	public int HandIndex;

	public AIVirtualCard Card;

	public int DrawCount;

	public int UsedCost;

	public int RestPp;

	public PlaySimulationType PlayType;

	public bool IsPassedPlayCondition;

	public bool IsProcessed;

	public bool IsPlayable
	{
		get
		{
			if (UsedCost >= 0 && RestPp >= 0)
			{
				return IsPassedPlayCondition;
			}
			return false;
		}
	}

	public AISimulationPreprocessRecorder PreprocessRecorder { get; private set; }

	public AIVirtualCard TransformCard { get; private set; }

	public AIDiscardInfo DiscardInfo { get; private set; }

	public AISelectedTargetInfoSet PreDecidedSelectTargets { get; private set; }

	public bool HasPreDecidedSelectTargets
	{
		get
		{
			if (PreDecidedSelectTargets != null)
			{
				return PreDecidedSelectTargets.IsAnyTargetExists();
			}
			return false;
		}
	}

	public PlayedCardInfo(int handIndex, AIVirtualCard card)
	{
		HandIndex = handIndex;
		Card = card;
		DrawCount = 0;
		UsedCost = -1;
		RestPp = -1;
		PlayType = PlaySimulationType.Undefined;
		IsPassedPlayCondition = false;
		PreprocessRecorder = null;
		TransformCard = null;
		DiscardInfo = null;
		PreDecidedSelectTargets = null;
		IsProcessed = false;
	}

	public void CloneFromPartlyMatchedInfo(PlayedCardInfo original)
	{
		if (HandIndex == original.HandIndex && Card.IsSameCard(original.Card))
		{
			DrawCount = original.DrawCount;
			UsedCost = original.UsedCost;
			RestPp = original.RestPp;
			PlayType = original.PlayType;
			IsPassedPlayCondition = original.IsPassedPlayCondition;
			TransformCard = original.TransformCard;
		}
		else
		{
			AIConsoleUtility.LogError("PlayedCardInfo.CloneFromPartlyMatchedInfo() error!! Cannot match to original!!!!!");
		}
	}

	public void RegisterPreprocess(AISimulationPreprocessRecorder recorder)
	{
		PreprocessRecorder = recorder;
	}

	public void SetTransformCard(AIVirtualCard newActor)
	{
		TransformCard = newActor;
	}

	public void SetDiscardInfo(AIDiscardInfo info, AIScriptTokenArgType selectType)
	{
		DiscardInfo = info;
		if (info.IsValuable && (selectType == AIScriptTokenArgType.TARGET_SELECT || selectType == AIScriptTokenArgType.SECOND_TARGET_SELECT))
		{
			AISelectedTargetInfo info2 = new AISelectedTargetInfo(info.TargetList, TargetSelectType.NormalRuleBase);
			int num = -1;
			switch (selectType)
			{
			case AIScriptTokenArgType.TARGET_SELECT:
				num = 0;
				break;
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				num = 1;
				break;
			}
			if (num >= 0)
			{
				RegisterPreDecidedTarget(info2, num);
			}
		}
	}

	public void RegisterPreDecidedPreprocessTarget(AISelectedTargetInfo info)
	{
		if (PreDecidedSelectTargets == null)
		{
			PreDecidedSelectTargets = new AISelectedTargetInfoSet();
		}
		PreDecidedSelectTargets.SetPreprocessTarget(info);
	}

	public void RegisterPreDecidedTarget(AISelectedTargetInfo info, int index)
	{
		if (PreDecidedSelectTargets == null)
		{
			PreDecidedSelectTargets = new AISelectedTargetInfoSet();
		}
		PreDecidedSelectTargets.Set(info, index);
	}
}
