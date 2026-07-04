using System.Collections.Generic;

namespace Wizard;

public class AIVirtualTargetSelectSimulationInfo
{
	public AIVirtualCard OriginalActor;

	public AIVirtualCard RealActor;

	public List<AIVirtualTargetSelectInfo> SelectInfoList;

	public AIOperationType OperationType;

	public PlayedCardInfo NextPlayCardInfo;

	public AISelectedTargetInfoSet FirstActionTargetTemp;

	public bool IsFirstAction;

	public AISelectedTargetInfoSet CurrentSimulationBestTargetTemp;

	public float FieldValueAfterFirstSelectCardPlay;

	public float CurrentSimulationBestTargetTempValue;

	public bool IsRecentlyUpdatedCurrentSimulationBestTargetTempValue;

	private bool _isNextPlay;

	public float PrevRemovalBonus;

	public bool IsInterruptedNextPlay { get; private set; }

	public AIVirtualTargetSelectSimulationInfo()
	{
		FieldValueAfterFirstSelectCardPlay = float.MinValue;
		CurrentSimulationBestTargetTempValue = float.MinValue;
		IsRecentlyUpdatedCurrentSimulationBestTargetTempValue = false;
	}

	public AIVirtualTargetSelectAction CreateSituationForTargetSelectSimulation()
	{
		return new AIVirtualTargetSelectAction(RealActor, OriginalActor, OperationType);
	}

	public AIVirtualTargetSelectSimulationInfo CreateNextPlayCardTargetSelectSimulationInfo(AIVirtualField field, float prevRemovalBonus)
	{
		if (NextPlayCardInfo == null)
		{
			return null;
		}
		AIVirtualCard aIVirtualCard = field.SearchVirtualCard(NextPlayCardInfo.Card);
		if (aIVirtualCard == null)
		{
			return null;
		}
		AIVirtualCard aIVirtualCard2 = NextPlayCardInfo.TransformCard ?? aIVirtualCard;
		AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard2, aIVirtualCard, AIOperationType.PLAY, (AISelectedTargetInfoSet)null);
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		int playSpaceRequired = field.GetPlaySpaceRequired(aIVirtualCard, emptyPlayPtn, aIVirtualTargetSelectAction, needsTokenCount: false);
		if (!aIVirtualCard.IsAbleToPlay(emptyPlayPtn, aIVirtualTargetSelectAction) || field.AllyInplayCards.Count + playSpaceRequired > 5)
		{
			return null;
		}
		List<AIVirtualTargetSelectInfo> list = aIVirtualCard2.CreateAIVirtualSelectInfo(field, aIVirtualTargetSelectAction);
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		return new AIVirtualTargetSelectSimulationInfo
		{
			OriginalActor = aIVirtualCard,
			RealActor = aIVirtualCard2,
			SelectInfoList = list,
			OperationType = AIOperationType.PLAY,
			NextPlayCardInfo = null,
			FirstActionTargetTemp = FirstActionTargetTemp,
			IsFirstAction = false,
			_isNextPlay = true,
			PrevRemovalBonus = prevRemovalBonus
		};
	}

	public void IsCurrentSimulationBestTargetTempNeedsToUpdated(float maxFieldValue)
	{
		if (EnemyAI.IsLargerThan(maxFieldValue, CurrentSimulationBestTargetTempValue))
		{
			CurrentSimulationBestTargetTempValue = maxFieldValue;
			IsRecentlyUpdatedCurrentSimulationBestTargetTempValue = true;
		}
		else
		{
			IsRecentlyUpdatedCurrentSimulationBestTargetTempValue = false;
		}
	}

	public void ForceStopNextPlay()
	{
		if (_isNextPlay)
		{
			IsInterruptedNextPlay = true;
		}
	}
}
