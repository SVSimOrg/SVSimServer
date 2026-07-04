using System.Linq;

public class NetworkSkill_update_deck : Skill_update_deck
{

	public NetworkSkill_update_deck(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override BattleCardBase CreateTokenCard(CallParameter parameter, int tokenId, int repeatCount, int tokenIdIndex, BattlePlayerBase targetPlayer)
	{
		if (base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast)
		{
			return targetPlayer.CreateNextIndexCard(tokenId);
		}
		int targetIndex = -1;
		if (_updateType != "change" && _isUsingTargetCards && repeatCount == -1 && !(base.ApplyingTargetFilter is SkillTargetChosenCardsFilter))
		{
			targetIndex = parameter.targetCards.ToList()[tokenIdIndex].Index;
		}
		int copySelectIndex = -1;
		if (_isUsingTargetCards)
		{
			copySelectIndex = parameter.targetCards.First().Index;
		}
		bool skillCopy = false;
		if (_isUsingTargetCards && (base.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter || base.ApplyingTargetFilter is SkillTargetHandFilter || base.ApplyingTargetFilter is SkillTargetDeckFilter || base.ApplyingTargetFilter is SkillTargetLastTargetFilter))
		{
			skillCopy = true;
		}
		if (!_isReferenceOpponentCard && !_isReferenceFusionedCard && _isUsingTargetCards && !base.IsOpen && (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsWatchBattle || !targetPlayer.IsPlayer))
		{
			CardDataModel cardDataModel = (targetPlayer.BattleMgr as NetworkBattleManagerBase).networkBattleData.GetReceiveData().GetReceiveCardList().FirstOrDefault((CardDataModel c) => c.CardId > 0 && c.Index == targetPlayer.cardTotalNum && c.ToStateList.Any((NetworkBattleDefine.NetworkCardPlaceState s) => s == NetworkBattleDefine.NetworkCardPlaceState.Field) && (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsWatchBattle || c.isOpponent != targetPlayer.IsPlayer));
			if (cardDataModel != null)
			{
				tokenId = cardDataModel.CardId;
			}
		}
		BattleManagerBase.CardCreateInfo info = new BattleManagerBase.CardCreateInfo(tokenId, targetPlayer.IsPlayer, base.IsTargetChoiceSelectSkill, NetworkBattleDefine.NetworkCardPlaceState.Deck, _isReferenceOpponentCard, this);
		return SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.CreateBattleCardWithGameObject(info, new BattleManagerBase.IndexInfo(-1, targetIndex, skillCopy, copySelectIndex), repeatCount, _updateType == "change");
	}
}
