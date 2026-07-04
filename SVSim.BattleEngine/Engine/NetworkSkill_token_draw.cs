using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_token_draw : Skill_token_draw
{
	public List<BattleCardBase> DrawList { get; private set; }

	public NetworkSkill_token_draw(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		DrawList = new List<BattleCardBase>();
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (DrawList.Count > 0)
		{
			List<BattleCardBase> list = new List<BattleCardBase>();
			base.TokenModifierList = new List<int>();
			int i;
			for (i = 0; i < DrawList.Count; i++)
			{
				BattleCardBase battleCardBase = base.SkillPrm.selfBattlePlayer.ReservedCardList.FirstOrDefault((BattleCardBase c) => c.Index == DrawList[i].Index);
				if (battleCardBase != null)
				{
					DrawList[i] = battleCardBase;
					base.SkillPrm.selfBattlePlayer.ReservedCardList.Remove(DrawList[i]);
				}
				list.Add(DrawList[i]);
				TokenDrawModifier tokenDrawModifier = _selfBattlePlayer.Class.SkillApplyInformation.GetTokenDrawModifier(DrawList[i].CardId);
				if (tokenDrawModifier != null)
				{
					base.TokenModifierList.Add(DrawList[i].CardId);
					for (int num = 0; num < tokenDrawModifier.MultiplyCount - 1; num++)
					{
						list.Add(CreateToken(DrawList, DrawList[i].CardId, i, isCopy: false));
					}
				}
			}
			VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				list[num2].SetOnDraw(draw: true);
			}
			vfxWithLoadingSequential.RegisterToLoadingVfx(base.SkillPrm.selfBattlePlayer.BattleMgr.LoadCardResources(list));
			return CreateTokenDrawVfx(parameter, list, vfxWithLoadingSequential, _playerSide, isReservation: true);
		}
		return base.Start(parameter);
	}

	protected override VfxWithLoading CreateTokenDrawVfx(CallParameter parameter, List<BattleCardBase> drawList, VfxWithLoadingSequential vfxWithLoading, BattlePlayerBase playerSide, bool isReservation = false)
	{
		VfxWithLoading result = base.CreateTokenDrawVfx(parameter, drawList, vfxWithLoading, playerSide, isReservation);
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnTokenDraw(base.SkillPrm.ownerCard, drawList, parameter.targetCards.ToList(), playerSide.IsPlayer, IsVisibleTarget, isReservation);
		return result;
	}

	protected override BattleCardBase CreateTokenCard(BattlePlayerBase player, int id, int index, NetworkBattleDefine.NetworkCardPlaceState toState, bool isCopy = false)
	{
		BattleManagerBase.CardCreateInfo info = new BattleManagerBase.CardCreateInfo(id, player.IsPlayer, base.ApplyingTargetFilter is SkillTargetChosenCardsFilter, toState, base.IsOpponentHandCopy, this);
		return SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.CreateBattleCardWithGameObject(info, new BattleManagerBase.IndexInfo(-1, -1, isCopy, index));
	}

	public override bool IsVisibleDrawSkillTarget(BattlePlayerBase selfBattlePlayer, CallParameter parameter)
	{
		if (!selfBattlePlayer.IsPlayer && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)
		{
			if (DrawList.Count <= 0)
			{
				if (parameter.targetCards.Count() <= 0 || (!(base.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter) && !(base.ApplyingTargetFilter is SkillTargetHandFilter) && !(base.ApplyingTargetFilter is SkillTargetDeckFilter) && !(base.ApplyingTargetFilter is SkillTargetChosenCardsFilter) && !(base.ApplyingTargetFilter is SkillTargetDiscardThisTurnCardListFilter) && !IsTargetHandOtherSelfFilter()))
				{
					return !IsHaveApplicableTargetFilter<SkillTargetDiscardCardListFilter>();
				}
				return false;
			}
			return false;
		}
		return true;
	}

	public void CreateTokenCardAttachTiming(IEnumerable<BattleCardBase> targetCards)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(base.SkillPrm.selfBattlePlayer, base.SkillPrm.opponentBattlePlayer);
		SkillCollectionBase.SetupOptionValue(base.OptionValue, playerInfoPair, base.SkillPrm.ownerCard, this);
		if (!CreateTokenInfo(targetCards, isReserve: true))
		{
			return;
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < _tokenIds.Count(); i++)
		{
			BattleCardBase battleCardBase = CreateToken(targetCards.ToList(), _tokenIds.ElementAt(i), i, isCopy: true);
			DrawList.Add(battleCardBase);
			base.SkillPrm.selfBattlePlayer.ReservedCardList.Add(battleCardBase);
			list.Add(battleCardBase);
			BuffInfo buffInfo = base.SkillPrm.selfBattlePlayer.Class.BuffInfoList.LastOrDefault((BuffInfo b) => b.SkillFrom == this);
			if (buffInfo != null)
			{
				buffInfo.TargetCard = battleCardBase;
			}
		}
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnCreateReservedCards(base.SkillPrm.ownerCard, list, base.SkillPrm.ownerCard.IsPlayer);
	}

	private BattleCardBase CreateToken(List<BattleCardBase> targetCards, int tokenId, int index, bool isCopy)
	{
		int id = tokenId;
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(tokenId);
		if (IsMakeFoil)
		{
			id = cardParameterFromId.FoilCardId;
		}
		int index2 = -1;
		if (targetCards.Count() > 0)
		{
			index2 = ((targetCards.Count() < _tokenIds.Count()) ? targetCards.First().Index : targetCards.ElementAt(index).Index);
		}
		return CreateTokenCard(_playerSide, id, index2, NetworkBattleDefine.NetworkCardPlaceState.Reservation, isCopy);
	}
}
