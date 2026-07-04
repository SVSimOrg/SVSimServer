using System.Collections.Generic;

namespace Wizard;

public class AITagPreprocessCollectionContainer
{
	public AITurnEndStopCollection AllyTurnEndStopInfoContainer { get; private set; }

	public AITurnEndStopCollection OpponentTurnEndStopInfoContainer { get; private set; }

	public AITurnStartStopCollection AllyTurnStartStopInfoContainer { get; private set; }

	public AITurnStartStopCollection OpponentTurnStartStopInfoContainer { get; private set; }

	public AILeaveStopCollection LeaveStopInfoContainer { get; private set; }

	public AIAfterDamageStopCollection AfterDamageStopInfoContainer { get; private set; }

	public AITagPreprocessCollectionContainer()
	{
		AllyTurnEndStopInfoContainer = null;
		OpponentTurnEndStopInfoContainer = null;
		AllyTurnStartStopInfoContainer = null;
		OpponentTurnStartStopInfoContainer = null;
		LeaveStopInfoContainer = null;
		AfterDamageStopInfoContainer = null;
	}

	private AITagPreprocessCollectionContainer(AITagPreprocessCollectionContainer container, AIVirtualField field)
	{
		List<AIVirtualCard> allReferableCards = field.CardListSet.AllReferableCards;
		AllyTurnEndStopInfoContainer = ((container.AllyTurnEndStopInfoContainer == null) ? null : container.AllyTurnEndStopInfoContainer.Clone(allReferableCards));
		OpponentTurnEndStopInfoContainer = ((container.OpponentTurnEndStopInfoContainer == null) ? null : container.OpponentTurnEndStopInfoContainer.Clone(allReferableCards));
		AllyTurnStartStopInfoContainer = ((container.AllyTurnStartStopInfoContainer == null) ? null : container.AllyTurnStartStopInfoContainer.Clone(allReferableCards));
		OpponentTurnStartStopInfoContainer = ((container.OpponentTurnStartStopInfoContainer == null) ? null : container.OpponentTurnStartStopInfoContainer.Clone(allReferableCards));
		LeaveStopInfoContainer = ((container.LeaveStopInfoContainer == null) ? null : container.LeaveStopInfoContainer.Clone(allReferableCards));
		AfterDamageStopInfoContainer = ((container.AfterDamageStopInfoContainer == null) ? null : container.AfterDamageStopInfoContainer.Clone(allReferableCards));
	}

	public AITagPreprocessCollectionContainer Clone(AIVirtualField field)
	{
		return new AITagPreprocessCollectionContainer(this, field);
	}

	public bool HasTurnEndStopInfo(bool isAlly)
	{
		if (isAlly)
		{
			return HasAllyTurnEndStopInfo();
		}
		return HasOpponentTurnEndStopInfo();
	}

	public bool HasTurnStartStopInfo(bool isAlly)
	{
		if (isAlly)
		{
			return HasAllyTurnStartStopInfo();
		}
		return HasOpponentTurnStartStopInfo();
	}

	public bool HasAllyTurnEndStopInfo()
	{
		if (AllyTurnEndStopInfoContainer != null)
		{
			return AllyTurnEndStopInfoContainer.HasInfo;
		}
		return false;
	}

	public bool HasOpponentTurnEndStopInfo()
	{
		if (OpponentTurnEndStopInfoContainer != null)
		{
			return OpponentTurnEndStopInfoContainer.HasInfo;
		}
		return false;
	}

	public bool HasAllyTurnStartStopInfo()
	{
		if (AllyTurnStartStopInfoContainer != null)
		{
			return AllyTurnStartStopInfoContainer.HasInfo;
		}
		return false;
	}

	public bool HasOpponentTurnStartStopInfo()
	{
		if (OpponentTurnStartStopInfoContainer != null)
		{
			return OpponentTurnStartStopInfoContainer.HasInfo;
		}
		return false;
	}

	public bool HasLeaveStopInfo()
	{
		if (LeaveStopInfoContainer != null)
		{
			return LeaveStopInfoContainer.HasInfo;
		}
		return false;
	}

	public bool HasAfterDamageStopInfo()
	{
		if (AfterDamageStopInfoContainer != null)
		{
			return AfterDamageStopInfoContainer.HasInfo;
		}
		return false;
	}

	public void AppendAllyTurnEndStopInfo(AITagPreprocessCreationOptionBase option, int defaultDecrementValue = 0)
	{
		if (AllyTurnEndStopInfoContainer == null)
		{
			AllyTurnEndStopInfoContainer = new AITurnEndStopCollection();
		}
		AllyTurnEndStopInfoContainer.AppendInfo(option, defaultDecrementValue);
	}

	public void AppendOpponentTurnEndStopInfo(AITagPreprocessCreationOptionBase option, int defaultDecrementValue = 0)
	{
		if (OpponentTurnEndStopInfoContainer == null)
		{
			OpponentTurnEndStopInfoContainer = new AITurnEndStopCollection();
		}
		OpponentTurnEndStopInfoContainer.AppendInfo(option, defaultDecrementValue);
	}

	public void AppendAllyTurnStartStopInfo(AITagPreprocessCreationOptionBase option)
	{
		if (AllyTurnStartStopInfoContainer == null)
		{
			AllyTurnStartStopInfoContainer = new AITurnStartStopCollection();
		}
		AllyTurnStartStopInfoContainer.AppendInfo(option);
	}

	public void AppendOpponentTurnStartStopInfo(AITagPreprocessCreationOptionBase option)
	{
		if (OpponentTurnStartStopInfoContainer == null)
		{
			OpponentTurnStartStopInfoContainer = new AITurnStartStopCollection();
		}
		OpponentTurnStartStopInfoContainer.AppendInfo(option);
	}

	public void AppendLeaveStopInfo(AITagPreprocessCreationOptionBase option, AIVirtualCard provider)
	{
		if (LeaveStopInfoContainer == null)
		{
			LeaveStopInfoContainer = new AILeaveStopCollection();
		}
		LeaveStopInfoContainer.AppendInfo(option, provider);
	}

	public void AppendAfterDamageStopInfo(AITagPreprocessCreationOptionBase option)
	{
		if (AfterDamageStopInfoContainer == null)
		{
			AfterDamageStopInfoContainer = new AIAfterDamageStopCollection();
		}
		AfterDamageStopInfoContainer.AppendInfo(option);
	}

	public void SimulateAllTurnEndInfo(bool isAlly, AIVirtualTurnEndInfo situation)
	{
		if (HasTurnEndStopInfo(isAlly))
		{
			(isAlly ? AllyTurnEndStopInfoContainer : OpponentTurnEndStopInfoContainer).SimulateActionAll(isAlly, situation);
		}
	}

	public void SimulateAllTurnStartInfo(bool isAlly, AISituationInfo situation)
	{
		if (HasTurnStartStopInfo(isAlly))
		{
			AITurnStartStopCollection obj = (isAlly ? AllyTurnStartStopInfoContainer : OpponentTurnStartStopInfoContainer);
			obj.SimulateActionAll(isAlly, situation);
			obj.Clear();
		}
	}

	public void SimulateWhenLeaveInfo(AIVirtualCard leaveCard, AISituationInfo situation)
	{
		if (HasLeaveStopInfo())
		{
			LeaveStopInfoContainer.SimulateActionAll(leaveCard, situation);
		}
	}

	public void SimulateAfterDamageInfo(AIVirtualCard damagedCard)
	{
		if (HasAfterDamageStopInfo())
		{
			AfterDamageStopInfoContainer.SimulateActionAll(damagedCard);
		}
	}
}
