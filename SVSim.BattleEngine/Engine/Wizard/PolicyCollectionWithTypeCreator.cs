namespace Wizard;

public static class PolicyCollectionWithTypeCreator
{
	public static PolicyCollectionWithTypeBase Create(AIPolicyType type)
	{
		AIPolicyCollection aIPolicyCollection = null;
		switch (type)
		{
		case AIPolicyType.AllyPlayBonus:
			aIPolicyCollection = new AllyPlayBonusPolicyCollection();
			break;
		case AIPolicyType.AllyPlayBonusRate:
			aIPolicyCollection = new AllyPlayBonusRatePolicyCollection();
			break;
		case AIPolicyType.DisableLethalCheck:
			aIPolicyCollection = new DisableLethalCheckPolicyCollection();
			break;
		case AIPolicyType.EpValue:
			aIPolicyCollection = new EpValuePolicyCollection();
			break;
		case AIPolicyType.ModUnitRate:
			aIPolicyCollection = new ModUnitRatePolicyCollection();
			break;
		case AIPolicyType.UnitBonus:
			aIPolicyCollection = new UnitBonusPolicyCollection();
			break;
		case AIPolicyType.PlayptnBonus:
			aIPolicyCollection = new PlayptnBonusPolicyCollection();
			break;
		case AIPolicyType.BarrierBonus:
			aIPolicyCollection = new BarrierBonusPolicyCollection();
			break;
		case AIPolicyType.PlayBreak:
			aIPolicyCollection = new PlayBreakPolicyCollection();
			break;
		case AIPolicyType.EmoOnTurnEnd:
			aIPolicyCollection = new EmoOnTurnEndPolicyCollection();
			break;
		case AIPolicyType.EmoOnTurnStart:
			aIPolicyCollection = new EmoOnTurnStartPolicyCollection();
			break;
		case AIPolicyType.DelayTurnEndTime:
			aIPolicyCollection = new AIDelayTurnEndTimePolicyCollection();
			break;
		case AIPolicyType.EmoOnLeaderDamaged:
			aIPolicyCollection = new EmoOnLeaderDamagedPolicyCollection();
			break;
		case AIPolicyType.SetReferenceId:
			aIPolicyCollection = new SetReferenceIdPolicyCollection();
			break;
		case AIPolicyType.SetReferenceTribe:
			aIPolicyCollection = new SetReferenceTribePolicyCollection();
			break;
		case AIPolicyType.PlayerEmoOnTurnEnd:
			aIPolicyCollection = new PlayerEmoOnTurnEndPolicyCollection();
			break;
		case AIPolicyType.PlayerEmoOnTurnStart:
			aIPolicyCollection = new PlayerEmoOnTurnStartPolicyCollection();
			break;
		case AIPolicyType.PlayerEmoOnLeaderDamaged:
			aIPolicyCollection = new PlayerEmoOnLeaderDamagedPolicyCollection();
			break;
		case AIPolicyType.MoveFirstBonus:
			aIPolicyCollection = new MoveFirstBonusPolicyCollection();
			break;
		case AIPolicyType.GameStartAttachTag:
			aIPolicyCollection = new GameStartAttachTagPolicyCollection();
			break;
		default:
			return null;
		}
		return new PolicyCollectionWithSingleType(type, aIPolicyCollection);
	}
}
