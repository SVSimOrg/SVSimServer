using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_special_token_draw : SkillBase
{

	protected IEnumerable<int> _beforeTransformTokenId;

	protected IEnumerable<int> _afterTransformTokenId;

	protected float _beforeChangeWaitTime;

	protected float _afterChangeWaitTime;

	protected string _transformEffectPath;

	protected BattlePlayerBase _playerSide;

	protected BattlePlayerBase _selfBattlePlayer;

	public override bool IsTargetIndicate => false;

	public override bool IsAllowDestroyTarget => true;

	public Skill_special_token_draw(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (!CreateTokenInfo(parameter.targetCards))
		{
			parameter.calledSkillResultInfo.drawCards = new List<IReadOnlyBattleCardInfo>();
			return NullVfxWithLoading.GetInstance();
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		int cardTotalNum = _playerSide.cardTotalNum;
		_playerSide.cardTotalNum++;
		VfxWith<List<BattleCardBase>> vfxWith = CreateTokenObjectAndView(cardTotalNum, isBeforeTransform: true);
		VfxWith<List<BattleCardBase>> vfxWith2 = CreateTokenObjectAndView(cardTotalNum, isBeforeTransform: false);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWith.Vfx);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWith2.Vfx);
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateTokenDrawVfx(parameter, vfxWith.Value, vfxWith2.Value, _playerSide));
		return vfxWithLoadingSequential;
	}

	private VfxWith<List<BattleCardBase>> CreateTokenObjectAndView(int index, bool isBeforeTransform)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		IEnumerable<int> source = (isBeforeTransform ? _beforeTransformTokenId : _afterTransformTokenId);
		for (int i = 0; i < source.Count(); i++)
		{
			int num = source.ElementAt(i);
			int id = num;
			CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(num);
			if (IsMakeFoil)
			{
				id = cardParameterFromId.FoilCardId;
			}
			BattleCardBase battleCardBase = CreateTokenCard(_playerSide, id, index);
			battleCardBase.SetOnDraw(draw: true);
			list.Add(battleCardBase);
		}
		return new VfxWith<List<BattleCardBase>>(base.SkillPrm.selfBattlePlayer.BattleMgr.LoadCardResources(list), list);
	}

	protected bool CreateTokenInfo(IEnumerable<BattleCardBase> targetCards)
	{
		_selfBattlePlayer = base.SkillPrm.selfBattlePlayer;
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.before_transform_token_draw, "_OPT_NULL_");
		string text2 = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.after_transform_token_draw, "_OPT_NULL_");
		_playerSide = base.SkillPrm.ownerCard.SelfBattlePlayer;
		bool flag = false;
		if (text != "_OPT_NULL_" && text2 != "_OPT_NULL_")
		{
			_beforeTransformTokenId = SkillOptionValue.ParseOptionTokenID(text);
			_afterTransformTokenId = SkillOptionValue.ParseOptionTokenID(text2);
			flag = true;
		}
		if (!flag)
		{
			return false;
		}
		if (_beforeTransformTokenId.Count() == 0 || _afterTransformTokenId.Count() == 0)
		{
			return false;
		}
		return true;
	}

	protected VfxWithLoading CreateTokenDrawVfx(CallParameter parameter, List<BattleCardBase> beforeTransformDrawList, List<BattleCardBase> afterTransformDrawList, BattlePlayerBase playerSide)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		afterTransformDrawList = playerSide.DrawCards(afterTransformDrawList, parameter.skillProcessor, isOpen: false, isMulligan: false, isToken: true, isSkillDraw: true, this, isReservation: false, parameter.calledSkillResultInfo, base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.turn_token_draw_skill_id, -1)).Value.ToList();
		AddLastTarget(parameter, afterTransformDrawList);
		parameter.calledSkillResultInfo.drawCards = BattlePlayerBase.ConvertToSkillInfoCollection(afterTransformDrawList);
		if (!PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_BATTLE_EFFECT) && !string.IsNullOrEmpty(base.SkillPrm.buildInfo._effectPath))
		{
			vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		}
		VfxWithLoading vfxWithLoading = CreateTokenSpawnVfx(this, beforeTransformDrawList.First());
		_transformEffectPath = "cmn_token_draw_1";
		_beforeChangeWaitTime = 0f;
		_afterChangeWaitTime = 0.2f;
		DataMgr.SpecialBattleSetting specialBattleSettingInfo = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().SpecialBattleSettingInfo;
		if (specialBattleSettingInfo != null && specialBattleSettingInfo.SpecialTokenDrawOverrideEffectPair.ContainsKey(afterTransformDrawList.First().CardId))
		{
			_transformEffectPath = specialBattleSettingInfo.SpecialTokenDrawOverrideEffectPair[afterTransformDrawList.First().CardId];
			if (_transformEffectPath.Contains(":"))
			{
				string[] array = _transformEffectPath.Split(':');
				_transformEffectPath = array[0];
				_beforeChangeWaitTime = float.Parse(array[1]);
				if (array.Count() >= 2)
				{
					_afterChangeWaitTime = float.Parse(array[2]);
				}
			}
		}
		vfxWithLoadingSequential.RegisterToMainVfx(NullVfx.GetInstance());
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
		vfxWithLoadingSequential.RegisterToMainVfx(InstantVfx.Create(delegate
		{
			base.SkillPrm.selfBattlePlayer.UpdateHandCardsPlayability();
		}));
		if (IsBattleLog)
		{
			BattleLogManager.GetInstance().AddLogSkillDrawToken(afterTransformDrawList.Where((BattleCardBase s) => s.IsInHand).ToList(), this, isOpen: true);
			BattleLogManager.GetInstance().AddLogSkillDrawToken(afterTransformDrawList.Where((BattleCardBase s) => !s.IsInHand).ToList(), this, isOpen: true, isOverDraw: true);
		}
		return vfxWithLoadingSequential;
	}

	protected virtual BattleCardBase CreateTokenCard(BattlePlayerBase player, int id, int index)
	{
		return player.CreateCard(id, index);
	}

	public static VfxWithLoading CreateTokenSpawnVfx(SkillBase skill, BattleCardBase firstToken)
	{
		// Static helper with `skill` param — route through skill.SkillPrm rather than instance SkillPrm.
		if (skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			return NullVfxWithLoading.GetInstance();
		}
		float animationTime = 0f;
		Color color = firstToken.Clan switch
		{
			CardBasePrm.ClanType.MIN => Global.EFFECT_COLOR_ELF, 
			CardBasePrm.ClanType.ROYAL => Global.EFFECT_COLOR_ROYAL, 
			CardBasePrm.ClanType.WITCH => Global.EFFECT_COLOR_WITCH_1, 
			CardBasePrm.ClanType.DRAGON => Global.EFFECT_COLOR_DRAGON, 
			CardBasePrm.ClanType.NECRO => Global.EFFECT_COLOR_NECROMANCER, 
			CardBasePrm.ClanType.VAMPIRE => Global.EFFECT_COLOR_VANPIRE, 
			CardBasePrm.ClanType.BISHOP => Global.EFFECT_COLOR_BISHOP, 
			CardBasePrm.ClanType.NEMESIS => Global.EFFECT_COLOR_NEMESIS, 
			_ => Color.clear, 
		};
		string text = "cmn_token_draw_1";
		DataMgr.SpecialBattleSetting specialBattleSettingInfo = skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().SpecialBattleSettingInfo;
		if (specialBattleSettingInfo != null && specialBattleSettingInfo.TokenDrawOverrideEffectPair.ContainsKey(firstToken.CardId))
		{
			text = specialBattleSettingInfo.TokenDrawOverrideEffectPair[firstToken.CardId];
			if (text.Contains(":"))
			{
				string[] array = text.Split(':');
				text = array[0];
				animationTime = float.Parse(array[1]);
			}
			color = Color.clear;
		}
		Func<Vector3> func = () => firstToken.BattleCardView.GameObject.transform.position;
		return skill.CreateSkillEffectFromPath(text, "se_" + text, skill.SkillPrm.resourceMgr, EffectMgr.EngineType.SHURIKEN, EffectMgr.MoveType.DIRECT, func, func, animationTime, color);
	}
}
