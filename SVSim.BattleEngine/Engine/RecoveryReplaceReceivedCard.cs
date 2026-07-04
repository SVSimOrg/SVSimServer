using UnityEngine;
using Wizard;

public class RecoveryReplaceReceivedCard : ReplaceReceivedCard
{
	public RecoveryReplaceReceivedCard(NetworkBattleManagerBase battleMgrBase, CardDataModel cardData)
		: base(battleMgrBase, cardData)
	{
	}

	protected override BattleCardBase CreateActualCard(BattlePlayerBase battlePlayer, bool isCardInDeck, bool isOpenDrawSkill)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(CardId);
		BattleCardBase battleCardBase = _networkBattleMgr.CreateBattleCard(CardId, battlePlayer.IsPlayer, null, cardParameterFromId, battlePlayer, CardIdx);
		InheritedCardData(battleCardBase);
		ReplaceBuffInfoList(battleCardBase, _originalDummyCard);
		battleCardBase.ShallowCopyBuffInfoList(_originalDummyCard);
		Object.DestroyImmediate(_originalDummyCard.BattleCardView.GameObject);
		_originalDummyCard.SelfBattlePlayer.BattleView.HandView.RemoveCardFromViewWithoutRearrange(_originalDummyCard.BattleCardView);
		SettingTargetDeckSelfCardAddDeckSkillCardList(battleCardBase, isCardInDeck);
		RemoveOpenCardRemoveAfterActionSkills(battleCardBase);
		return battleCardBase;
	}
}
