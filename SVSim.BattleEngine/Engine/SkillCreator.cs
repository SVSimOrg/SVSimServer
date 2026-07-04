using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wizard;
using Wizard.Battle.Resource;
using Wizard.Battle.View;

public class SkillCreator
{
	public class SkillBuildInfo
	{
		public readonly string _type = "";

		public readonly string _timing = "";

		public readonly string _condition = "";

		public readonly List<SkillFilterCreator.ContentInfo> _parsedConditionOld;

		public readonly List<string> _parsedConditionNew;

		public readonly string _target = "";

		public readonly List<SkillFilterCreator.ContentInfo> _parsedTargetOld;

		public readonly List<string> _parsedTargetNew;

		public readonly SkillFilterCreator.ContentInfo[] _parsedOption;

		public readonly string _option = "";

		public readonly string _preprocess = "";

		public readonly SkillFilterCreator.ContentInfo[] _parsedPreprocess;

		public readonly HandCardFrameEffectType _handCardFrameEffectType = HandCardFrameEffectType.NULL;

		public readonly string _icon = "";

		public readonly string _effectPath = "";

		public readonly EffectMgr.EngineType _engineType;

		public readonly string _sePath = "";

		public readonly float _effectTime;

		public readonly EffectMgr.MoveType _effectMoveType;

		public readonly EffectMgr.TargetType _effectTargetType;

		public BattleCardBase _previousSkillOwner;

		public readonly string _voice = string.Empty;

		public readonly List<string> _checkTargetKeywords = new List<string>();

		public SkillBuildInfo(string type, string timing, string condition, string target, string option, string preprocess, HandCardFrameEffectType frameEffectType = HandCardFrameEffectType.NONE, string icon = "", int[] tokenID = null, string effectPath = "", EffectMgr.EngineType engineType = EffectMgr.EngineType.NONE, string sePath = "", float effectTime = 0f, EffectMgr.MoveType effectMoveType = EffectMgr.MoveType.NONE, EffectMgr.TargetType effectTargetType = EffectMgr.TargetType.NONE, string voice = "")
		{
			_type = type;
			_timing = timing;
			_condition = condition;
			_target = target;
			_option = option;
			_preprocess = preprocess;
			_handCardFrameEffectType = frameEffectType;
			_icon = icon;
			_effectPath = effectPath;
			_engineType = engineType;
			_sePath = sePath;
			_effectTime = effectTime;
			_effectMoveType = effectMoveType;
			_effectTargetType = effectTargetType;
			_voice = voice;
			ParseOption(option, ref _parsedOption);
			ParseCondition(condition, ref _parsedConditionOld, ref _parsedConditionNew);
			ParseTarget(target, ref _parsedTargetOld, ref _parsedTargetNew, ref _checkTargetKeywords);
			ParsePreprocess(preprocess, ref _parsedPreprocess);
		}

		public bool IsSameSkill(SkillBuildInfo info)
		{
			if (_type == info._type && _timing == info._timing && _condition == info._condition && _target == info._target && _option == info._option)
			{
				return _preprocess == info._preprocess;
			}
			return false;
		}
	}

	public class CardSkillsBuildInfo
	{
		public List<SkillBuildInfo> normalSkillBuildInfos;

		public List<SkillBuildInfo> evolveSkillBuildInfos;
	}

	public static string[] NEWSTYLE_HEAD_TEXT = new string[13]
	{
		SkillFilterCreator.ContentKeyword.me.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.op.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.both.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.self.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.attacker.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.be_attacked.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.summoned_card.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.played_card.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.skill_drew_card.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.skill_update_deck_card.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.last_target.ToStringCustom() + ".",
		SkillFilterCreator.ContentKeyword.in_hand.ToStringCustom() + ".",
		"{"
	};

	protected readonly BattleCardBase _ownerCard;

	protected readonly BattlePlayerBase _selfBattlPlayer;

	protected readonly BattlePlayerBase _opponentBattlePlayer;

	protected readonly IBattleResourceMgr _battleResourceMgr;

	public SkillCreator(BattleCardBase ownerCard, BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer, IBattleResourceMgr battleResourceMgr)
	{
		_ownerCard = ownerCard;
		_selfBattlPlayer = selfBattlePlayer;
		_opponentBattlePlayer = opponentBattlePlayer;
		_battleResourceMgr = battleResourceMgr;
	}

	private SkillBase CreateSkillFactory(string skillName, SkillBuildInfo buildInfo, SkillParameter skillParam)
	{
		SkillKeywordInfo.SkillKeyword keyword = SkillKeywordInfo.GetKeyword(skillName);
		bool flag = skillParam.ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase && !skillParam.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork;
		switch (keyword)
		{
		case SkillKeywordInfo.SkillKeyword.damage_modifier:
			if (!flag)
			{
				return new Skill_damage_modifier(skillParam, buildInfo._option);
			}
			return new NetworkSkill_damage_modifier(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.attach_skill:
			if (!flag)
			{
				return new SingleSkill_attach_skill(skillParam, buildInfo._option);
			}
			return new NetworkSkill_attach_skill(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.attack_by_life:
			if (!flag)
			{
				return new Skill_attack_by_life(skillParam, buildInfo._option);
			}
			return new NetworkSkill_attack_by_life(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.attack_count:
			if (!flag)
			{
				return new Skill_attack_count(skillParam, buildInfo._option);
			}
			return new NetworkSkill_attack_count(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.banish:
			if (!flag)
			{
				return new Skill_banish(skillParam, buildInfo._option);
			}
			return new NetworkSkill_banish(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.cant_activate_fanfare:
			if (!flag)
			{
				return new Skill_cant_activate_fanfare(skillParam, buildInfo._option);
			}
			return new NetworkSkill_cant_activate_fanfare(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.cant_activate_shortage_deck_win:
			return new Skill_cant_activate_shortage_deck_win(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.cant_attack:
			if (!flag)
			{
				return new Skill_cant_attack(skillParam, buildInfo._option);
			}
			return new NetworkSkill_cant_attack(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.cant_summon:
			return new Skill_cant_summon(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.cant_play:
			if (!flag)
			{
				return new Skill_cant_play(skillParam, buildInfo._option);
			}
			return new NetworkSkill_cant_play(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.change_affiliation:
			if (!flag)
			{
				return new Skill_change_affiliation(skillParam, buildInfo._option);
			}
			return new NetworkSkill_change_affiliation(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.change_cemetery:
			if (!flag)
			{
				return new Skill_change_cemetery(skillParam, buildInfo._option);
			}
			return new NetworkSkill_change_cemetery(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.change_rally_count:
			return new Skill_change_rally_count(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.update_deck:
			if (!flag)
			{
				return new Skill_update_deck(skillParam, buildInfo._option);
			}
			return new NetworkSkill_update_deck(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.chant_count_change:
			if (!flag)
			{
				return new Skill_chant_count_change(skillParam, buildInfo._option);
			}
			return new NetworkSkill_chant_count_change(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.consume_ep_modifier:
			if (!flag)
			{
				return new Skill_consume_ep_modifier(skillParam, buildInfo._option);
			}
			return new NetworkSkill_consume_ep_modifier(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.cost_change:
			if (!flag)
			{
				return new Skill_cost_change(skillParam, buildInfo._option);
			}
			return new NetworkSkill_cost_change(skillParam.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase, skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.damage:
			if (!flag)
			{
				return new Skill_damage(skillParam, buildInfo._option);
			}
			return new NetworkSkill_damage(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.damage_cut:
			if (!flag)
			{
				return new Skill_damage_cut(skillParam, buildInfo._option);
			}
			return new NetworkSkill_damage_cut(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.destroy:
			if (!flag)
			{
				return new Skill_destroy(skillParam, buildInfo._option);
			}
			return new NetworkSkill_destroy(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.discard:
			if (!flag)
			{
				return new Skill_discard(skillParam, buildInfo._option);
			}
			return new NetworkSkill_discard(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.drain:
			if (!flag)
			{
				return new Skill_drain(skillParam, buildInfo._option);
			}
			return new NetworkSkill_drain(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.draw:
			return new Skill_draw(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.evolve:
			if (!flag)
			{
				return new Skill_evolve(skillParam, buildInfo._option);
			}
			return new NetworkSkill_evolve(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.extra_turn:
			if (!flag)
			{
				return new Skill_extra_turn(skillParam, buildInfo._option);
			}
			return new NetworkSkill_extra_turn(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.force_berserk:
			if (!flag)
			{
				return new Skill_force_berserk(skillParam, buildInfo._option);
			}
			return new NetworkSkill_force_berserk(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.force_skill_target:
			if (!flag)
			{
				return new Skill_force_skill_target(skillParam, buildInfo._option);
			}
			return new NetworkSkill_force_skill_target(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.attract_skill_target:
			if (!flag)
			{
				return new Skill_attract_skill_target(skillParam, buildInfo._option);
			}
			return new NetworkSkill_attract_skill_target(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.force_avarice:
			if (!flag)
			{
				return new Skill_force_avarice(skillParam, buildInfo._option);
			}
			return new NetworkSkill_force_avarice(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.force_wrath:
			if (!flag)
			{
				return new Skill_force_wrath(skillParam, buildInfo._option);
			}
			return new NetworkSkill_force_wrath(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.guard:
			if (!flag)
			{
				return new Skill_guard(skillParam, buildInfo._option);
			}
			return new NetworkSkill_guard(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.heal:
			if (!flag)
			{
				return new Skill_heal(skillParam, buildInfo._option);
			}
			return new NetworkSkill_heal(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.ignore_guard:
			if (!flag)
			{
				return new Skill_ignore_guard(skillParam, buildInfo._option);
			}
			return new NetworkSkill_ignore_guard(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.independent:
			if (!flag)
			{
				return new Skill_independent(skillParam, buildInfo._option);
			}
			return new NetworkSkill_independent(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.not_be_debuffed:
			return new Skill_not_be_debuffed(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.indestructible:
			if (!flag)
			{
				return new Skill_indestructible(skillParam, buildInfo._option);
			}
			return new NetworkSkill_indestructible(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.killer:
			if (!flag)
			{
				return new Skill_killer(skillParam, buildInfo._option);
			}
			return new NetworkSkill_killer(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.lose:
			if (!flag)
			{
				return new Skill_lose(skillParam, buildInfo._option);
			}
			return new NetworkSkill_lose(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.metamorphose:
			if (!flag)
			{
				return new Skill_metamorphose(skillParam, buildInfo._option);
			}
			return new NetworkSkill_metamorphose(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.fusion_metamorphose:
			if (!flag)
			{
				return new Skill_fusion_metamorphose(skillParam, buildInfo._option);
			}
			return new NetworkSkill_fusion_metamorphose(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.transform:
			return new Skill_transform(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.none:
			return new Skill_none(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.not_be_attacked:
			if (!flag)
			{
				return new Skill_not_be_attacked(skillParam, buildInfo._option);
			}
			return new NetworkSkill_not_be_attacked(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.play_count_change:
			if (!flag)
			{
				return new Skill_play_count_change(skillParam, buildInfo._option);
			}
			return new NetworkSkill_play_count_change(skillParam.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase, skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.possess_ep_modifier:
			if (!flag)
			{
				return new Skill_possess_ep_modifier(skillParam, buildInfo._option);
			}
			return new NetworkSkill_possess_ep_modifier(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.power_down:
			if (!flag)
			{
				return new Skill_power_down(skillParam, buildInfo._option);
			}
			return new NetworkSkill_power_down(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.powerup:
			if (!flag)
			{
				return new Skill_powerup(skillParam, buildInfo._option);
			}
			return new NetworkSkill_powerup(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.power_modifier:
			if (!flag)
			{
				return new Skill_power_modifier(skillParam, buildInfo._option);
			}
			return new NetworkSkill_power_modifier(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.pp_fixeduse:
			return new Skill_pp_fixeduse(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.pp_modifier:
			if (!flag)
			{
				return new Skill_pp_modifier(skillParam, buildInfo._option);
			}
			return new NetworkSkill_pp_modifier(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.bp_modifier:
			if (!flag)
			{
				return new Skill_bp_modifier(skillParam, buildInfo._option);
			}
			return new NetworkSkill_bp_modifier(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.quick:
			if (!flag)
			{
				return new Skill_quick(skillParam, buildInfo._option);
			}
			return new NetworkSkill_quick(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.reflection:
			if (!flag)
			{
				return new Skill_reflection(skillParam, buildInfo._option);
			}
			return new NetworkSkill_reflection(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.remove_by_banish:
			if (!flag)
			{
				return new Skill_remove_by_banish(skillParam, buildInfo._option);
			}
			return new NetworkSkill_remove_by_banish(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.remove_by_destroy:
			if (!flag)
			{
				return new Skill_remove_by_destroy(skillParam, buildInfo._option);
			}
			return new NetworkSkill_remove_by_destroy(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.return_card:
			if (!flag)
			{
				return new Skill_return_card(skillParam, buildInfo._option);
			}
			return new NetworkSkill_return_card(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.rush:
			if (!flag)
			{
				return new Skill_rush(skillParam, buildInfo._option);
			}
			return new NetworkSkill_rush(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.select:
			if (!flag)
			{
				return new Skill_select(skillParam, buildInfo._option);
			}
			return new NetworkSkill_select(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.shield:
			if (!flag)
			{
				return new Skill_shield(skillParam, buildInfo._option);
			}
			return new NetworkSkill_shield(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.sneak:
			if (!flag)
			{
				return new Skill_sneak(skillParam, buildInfo._option);
			}
			return new NetworkSkill_sneak(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.special_win:
			if (!flag)
			{
				return new Skill_special_win(skillParam, buildInfo._option);
			}
			return new NetworkSkill_special_win(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.special_lose:
			if (!flag)
			{
				return new Skill_special_lose(skillParam, buildInfo._option);
			}
			return new NetworkSkill_special_lose(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.spell_charge:
			if (!flag)
			{
				return new Skill_spell_charge(skillParam, buildInfo._option);
			}
			return new NetworkSkill_spell_charge(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.summon_card:
			if (!flag)
			{
				return new Skill_summon_card(skillParam, buildInfo._option);
			}
			return new NetworkSkill_summon_card(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.summon_token:
			if (!flag)
			{
				return new Skill_summon_token(skillParam, buildInfo._option);
			}
			return new NetworkSkill_summon_token(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.token_draw:
			if (!flag)
			{
				return new Skill_token_draw(skillParam, buildInfo._option);
			}
			return new NetworkSkill_token_draw(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.special_token_draw:
			return new Skill_special_token_draw(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.trigger:
			if (!flag)
			{
				return new Skill_trigger(skillParam, buildInfo._option);
			}
			return new NetworkSkill_trigger(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.turn_start_fixed_pp:
			return new Skill_turn_start_fixed_pp(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.unite:
			if (!flag)
			{
				return new Skill_unite(skillParam, buildInfo._option);
			}
			return new NetworkSkill_unite(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.untouchable:
			if (!flag)
			{
				return new Skill_untouchable(skillParam, buildInfo._option);
			}
			return new NetworkSkill_untouchable(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.repeat_skill:
			if (!flag)
			{
				return new Skill_repeat_skill(skillParam, buildInfo._option);
			}
			return new NetworkSkill_repeat_skill(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.choice:
			return new Skill_choice(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.shortage_deck_win:
			if (!flag)
			{
				return new Skill_shortage_deck_win(skillParam, buildInfo._option);
			}
			return new NetworkSkill_shortage_deck_win(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.generic_value_modifier:
			if (!flag)
			{
				return new Skill_generic_value_modifier(skillParam, buildInfo._option);
			}
			return new NetworkSkill_generic_value_modifier(skillParam.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase, skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.change_union_burst_count:
			if (!flag)
			{
				return new Skill_change_union_burst_count(skillParam, buildInfo._option);
			}
			return new NetworkSkill_change_union_burst_count(skillParam.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase, skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.change_skybound_art_count:
			return new Skill_change_skybound_art_count(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.change_super_skybound_art_count:
			if (!flag)
			{
				return new Skill_change_super_skybound_art_count(skillParam, buildInfo._option);
			}
			return new NetworkSkill_change_super_skybound_art_count(skillParam.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase, skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.change_white_ritual_stack:
			if (!flag)
			{
				return new Skill_change_white_ritual_stack(skillParam, buildInfo._option);
			}
			return new NetworkSkill_change_white_ritual_stack(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.stack_white_ritual:
			if (!flag)
			{
				return new Skill_stack_white_ritual(skillParam, buildInfo._option);
			}
			return new NetworkSkill_stack_white_ritual(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.rob_skill:
			if (!flag)
			{
				return new Skill_rob_skill(skillParam, buildInfo._option);
			}
			return new NetworkSkill_rob_skill(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.random_array:
			if (!flag)
			{
				return new Skill_random_array(skillParam, buildInfo._option);
			}
			return new NetworkSkill_random_array(skillParam.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase, skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.no_duplication_random_array:
			return new Skill_no_duplication_random_array(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.cant_evolution:
			if (!flag)
			{
				return new Skill_cant_evolution(skillParam, buildInfo._option);
			}
			return new NetworkSkill_cant_evolution(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.invoke_skill:
			if (!flag)
			{
				return new Skill_invoke_skill(skillParam, buildInfo._option);
			}
			return new NetworkSkill_invoke_skill(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.token_draw_modifier:
			return new Skill_token_draw_modifier(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.fusion:
			return new Skill_fusion(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.random_attack:
			return new Skill_random_attack(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.clear_destroyed_and_discarded_card_list:
			if (!flag)
			{
				return new Skill_clear_destroyed_and_discarded_card_list(skillParam, buildInfo._option);
			}
			return new NetworkSkill_clear_destroyed_and_discarded_card_list(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.clear_summoned_card_list:
			return new Skill_clear_summoned_card_list(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.geton:
			return new Skill_geton(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.getoff:
			return new Skill_getoff(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.not_decrease_pp:
			return new Skill_not_decrease_pp(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.heal_modifier:
			return new Skill_heal_modifier(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.life_zero_activate_leon_skill:
			return new Skill_life_zero_activate_leon_skill(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.copy_skill:
			if (!flag)
			{
				return new Skill_copy_skill(skillParam, buildInfo._option);
			}
			return new NetworkSkill_copy_skill(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.loop_skill:
			return new Skill_loop_skill(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.evolve_to_other:
			return new Skill_evolve_to_other(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.not_attached_resident_chant_count_change:
			return new Skill_not_attached_resident_chant_count_change(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.invoke_emote:
			return new Skill_invoke_emote(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.invoke_voice:
			return new Skill_invoke_voice(skillParam, buildInfo._option);
		case SkillKeywordInfo.SkillKeyword.can_play_self:
			return new Skill_can_play_self(skillParam, buildInfo._option);
		default:
			return new Skill_none(skillParam, buildInfo._option);
		}
	}

	public SkillBase Create(SkillBuildInfo buildInfo, IEnumerable<SkillPreprocessBase> lastSkillPreprocessCollection = null, bool isAttachSkill = false, SkillBase attachSkill = null)
	{
		string skillName = buildInfo._type;
		string text = "1";
		int num = buildInfo._type.IndexOf('@');
		if (num >= 0)
		{
			skillName = buildInfo._type.Substring(0, num);
			text = buildInfo._type.Substring(num + 1);
		}
		SkillParameter skillParameter = new SkillParameter();
		skillParameter.selfBattlePlayer = _selfBattlPlayer;
		skillParameter.opponentBattlePlayer = _opponentBattlePlayer;
		skillParameter.ownerCard = _ownerCard;
		skillParameter.buildInfo = buildInfo;
		skillParameter.afterFallPos = () => _ownerCard.BattleCardView.CardWrapObject.transform.position;
		skillParameter.resourceMgr = _battleResourceMgr;
		SkillBase skillBase = CreateSkillFactory(skillName, buildInfo, skillParameter);
		if (attachSkill != null)
		{
			Skill_attach_skill.GiveIndividualId(attachSkill, skillBase);
		}
		skillBase.CallCountTextValue = text;
		skillBase.SetCallCountText(text);
		SetupSkillTiming(skillBase, buildInfo._timing);
		skillBase.SetIsOnceCallTiming();
		SetupSkillConditionOld(skillBase.ConditionFilterCollection, buildInfo._parsedConditionOld, _ownerCard, skillBase);
		SetupSkillConditionNew(skillBase, _ownerCard, buildInfo._parsedConditionNew);
		SetupSkillTargetNew(skillBase, _ownerCard, buildInfo._parsedTargetNew);
		SetupSkillTargetOld(skillBase.ApplyFilterCollection, _ownerCard, buildInfo._parsedTargetOld, skillBase);
		SetupSkillPreprocess(skillBase, buildInfo._parsedPreprocess, lastSkillPreprocessCollection);
		skillBase.SetIsAttachSkill(isAttachSkill, _ownerCard.IsInplay);
		skillBase.SkillCreateEnd();
		return skillBase;
	}

	private static void SetupSkillTiming(SkillBase skill, string timing)
	{
		skill.SkillTimingText = timing;
		if (timing != null)
		{
			switch (timing)
			{
			case "none":
				break;
			case "when_play":
				skill.OnWhenPlayStart |= CreateStartMethod();
				break;
			case "when_hand_to_not_play":
				skill.OnWhenHandToNotPlayStart |= CreateStartMethod();
				break;
			case "when_destroy":
				skill.OnWhenDestroyStart |= CreateStartMethod();
				break;
			case "when_destroy_other":
				skill.OnWhenDestroyOtherStart |= CreateStartMethod();
				break;
			case "when_discard":
				skill.OnDisCardStart |= CreateStartMethod();
				break;
			case "when_discard_other":
				skill.OnDisCardOtherStart |= CreateStartMethod();
				break;
			case "when_necromance":
				skill.OnWhenNecromance |= CreateStartMethod();
				break;
			case "when_use_white_ritual_stack":
				skill.OnWhenUseWhiteRitualStack |= CreateStartMethod();
				break;
			case "when_summon":
				skill.OnWhenSummonStart |= CreateStartMethod();
				break;
			case "self_turn_start":
				skill.OnSelfTurnStartStart |= CreateStartMethod();
				break;
			case "turn_start":
				skill.OnSelfTurnStartStart |= CreateStartMethod();
				break;
			case "when_turn_start_immediate":
				skill.OnWhenTurnStartStartImmediate |= CreateStartMethod();
				break;
			case "self_turn_end":
				skill.OnSelfTurnEndStart |= CreateStartMethod();
				break;
			case "op_turn_start":
				skill.OnOpponentTurnStartStart |= CreateStartMethod();
				break;
			case "op_turn_end":
				skill.OnOpponentTurnEndStart |= CreateStartMethod();
				break;
			case "when_attack":
				skill.OnBeforeAttackStart |= CreateStartMethod();
				break;
			case "when_attack_self_and_other":
				skill.OnBeforeAttackSelfAndOtherStart |= CreateStartMethod();
				break;
			case "when_attack_after":
				skill.OnAfterAttackStart |= CreateStartMethod();
				break;
			case "when_attack_self_and_other_after":
				skill.OnAfterAttackSelfAndOtherStart |= CreateStartMethod();
				break;
			case "when_use_ep_self_and_other":
				skill.OnWhenUseEpSelfAndOtherStart |= CreateStartMethod();
				break;
			case "when_evolve":
				skill.OnWhenEvolveStart |= CreateStartMethod();
				break;
			case "when_evolve_other":
				skill.OnWhenEvolveOtherStart |= CreateStartMethod();
				break;
			case "when_evolve_self_and_other":
				skill.OnWhenEvolveSelfAndOtherStart |= CreateStartMethod();
				break;
			case "when_evolve_before":
				skill.OnWhenEvolveBeforeStart |= CreateStartMethod();
				break;
			case "when_play_other":
				skill.OnWhenPlayOtherStart |= CreateStartMethod();
				break;
			case "when_summon_other":
				skill.OnWhenSummonOtherStart |= CreateStartMethod();
				break;
			case "when_summon_self_and_other":
				skill.OnWhenSummonSelfAndOtherStart |= CreateStartMethod();
				break;
			case "when_spell_charge":
				skill.OnWhenSpellChargeStart |= CreateStartMethod();
				break;
			case "when_skill_chant_count_gain":
				skill.OnWhenChantCountChangeStart |= CreateStartMethod();
				break;
			case "when_chant_count_gain":
				skill.OnWhenChantCountGain |= CreateStartMethod();
				break;
			case "when_chant_count_gain_self_and_other":
				skill.OnWhenChantCountGainSelfAndOther |= CreateStartMethod();
				break;
			case "when_healing_self_and_other":
				skill.OnWhenHealingSelfAndOtherStart |= CreateStartMethod();
				break;
			case "when_healing_other":
				skill.OnWhenHealOtherStart |= CreateStartMethod();
				break;
			case "when_damage":
				skill.OnWhenDamageStart |= CreateStartMethod();
				break;
			case "when_damage_self_and_other":
				skill.OnWhenDamageSelfAndOtherStart |= CreateStartMethod();
				break;
			case "when_fight":
				skill.OnWhenFightStart |= CreateStartMethod();
				break;
			case "when_buff":
				skill.OnWhenBuffStart |= CreateStartMethod();
				break;
			case "when_return":
				skill.OnWhenReturnStart |= CreateStartMethod();
				break;
			case "when_return_other":
				skill.OnWhenReturnOtherStart |= CreateStartMethod();
				break;
			case "when_return_skill_activate":
				skill.OnWhenReturnSkillActivateStart |= CreateStartMethod();
				break;
			case "when_add_to_deck":
				skill.OnWhenAddToDeckStart |= CreateStartMethod();
				break;
			case "when_enhance":
				skill.OnWhenEnhanceStart |= CreateStartMethod();
				break;
			case "when_accelerate_other":
				skill.OnWhenAccelerateStart |= CreateStartMethod();
				break;
			case "when_crystallize_other":
				skill.OnWhenCrystallizeStart |= CreateStartMethod();
				break;
			case "when_burial_rite_other":
				skill.OnWhenBurialRiteOther |= CreateStartMethod();
				break;
			case "when_choice_play":
				skill.OnWhenChoicePlayStart |= CreateStartMethod();
				break;
			case "when_choice_evolve":
				skill.OnWhenChoiceEvolveStart |= CreateStartMethod();
				break;
			case "when_choice_brave":
				skill.OnWhenChoiceBrave |= CreateStartMethod();
				break;
			case "when_banish":
				skill.OnWhenBanish |= CreateStartMethod();
				break;
			case "when_banish_other":
				skill.OnWhenBanishOther |= CreateStartMethod();
				break;
			case "when_resonance_start":
				skill.OnWhenResonanceStart |= CreateStartMethod();
				break;
			case "when_accelerate":
				skill.OnWhenAccelerate |= CreateStartMethod();
				break;
			case "when_crystallize":
				skill.OnWhenCrystallize |= CreateStartMethod();
				break;
			case "when_leave":
				skill.OnWhenLeave |= CreateStartMethod();
				break;
			case "when_draw":
				skill.OnWhenDraw |= CreateStartMethod();
				break;
			case "when_draw_other":
				skill.OnWhenDrawOtherStart |= CreateStartMethod();
				break;
			case "when_pp_healing":
				skill.OnWhenPpHealStart |= CreateStartMethod();
				break;
			case "when_fusion":
				skill.OnWhenFusion |= CreateStartMethod();
				break;
			case "when_fusioned":
				skill.OnWhenFusioned |= CreateStartMethod();
				break;
			case "when_fusion_metamorphose":
				skill.OnWhenFusionMetamorphose |= CreateStartMethod();
				break;
			case "when_fight_after":
				skill.OnAfterFightStart |= CreateStartMethod();
				break;
			case "when_fusion_other":
				skill.OnWhenFusionOtherStart |= CreateStartMethod();
				break;
			case "when_geton":
				skill.OnWhenGetOnStart |= CreateStartMethod();
				break;
			case "when_getoff":
				skill.OnWhenGetOff |= CreateStartMethod();
				break;
			case "when_leave_other":
				skill.OnWhenLeaveOther |= CreateStartMethod();
				break;
			case "when_attach_ability":
				skill.OnWhenAttachAbility |= CreateStartMethod();
				break;
			case "when_buff_self_and_other":
				skill.OnWhenBuffSelfAndOther |= CreateStartMethod();
				break;
			case "when_debuff_self_and_other":
				skill.OnWhenDebuffSelfAndOther |= CreateStartMethod();
				break;
			case "when_debuff_include_set_max_life":
				skill.OnWhenDebuffIncludeSetMaxLife |= CreateStartMethod();
				break;
			case "when_healing":
				skill.OnWhenHealing |= CreateStartMethod();
				break;
			case "when_shortage_deck":
				skill.OnWhenShortageDeck |= CreateStartMethod();
				break;
			case "when_shortage_deck_win_skill_activate":
				skill.OnWhenShortageDeckWinSkillActivate |= CreateStartMethod();
				break;
			case "when_special_lose":
				skill.OnWhenSpecialLose |= CreateStartMethod();
				break;
			case "when_change_inplay_immediate":
				skill.OnWhenChangeInPlayImmediate |= CreateStartMethod();
				skill.PreprocessList.Add(new SkillResidentPreprocessReturnHandActiveReset(skill));
				break;
			case "when_change_inplay_selfhand":
				skill.OnWhenChangeInplaySelfhand |= CreateStartMethod();
				skill.PreprocessList.Add(new SkillResidentPreprocessReturnHandActiveReset(skill));
				break;
			case "when_change_inplay":
				skill.OnWhenChangeInPlay |= CreateStartMethod();
				skill.PreprocessList.Add(new SkillResidentPreprocessReturnHandActiveReset(skill));
				break;
			case "when_change_class_life_inplay":
				skill.OnWhenChangeClassLifeInplay |= CreateStartMethod();
				skill.PreprocessList.Add(new SkillResidentPreprocessReturnHandActiveReset(skill));
				break;
			case "when_change_pptotal":
				skill.OnWhenChangePPTotal |= CreateStartMethod();
				skill.PreprocessList.Add(new SkillResidentPreprocessReturnHandActiveReset(skill));
				break;
			case "when_change_class_life_selfhand":
				skill.OnWhenChangeClassLifeSelfhand |= CreateStartMethod();
				skill.PreprocessList.Add(new SkillResidentPreprocessReturnHandActiveReset(skill));
				break;
			case "when_add_to_hand":
				skill.OnWhenAddToHand |= CreateStartMethod();
				skill.PreprocessList.Add(new SkillResidentPreprocessReturnHandActiveReset(skill));
				break;
			case "when_battle_start":
				skill.OnWhenBattleStart |= CreateStartMethod();
				break;
			}
		}
	}

	private static uint CreateStartMethod()
	{
		return 1u;
	}

	public static bool IsNewStyle(string text)
	{
		return NEWSTYLE_HEAD_TEXT.Any(text.StartsWith);
	}

	private static string[] DivideContents(string content)
	{
		List<string> list = new List<string>();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < content.Length; i++)
		{
			if (content[i] == '{')
			{
				num++;
			}
			if (content[i] == '}')
			{
				num2++;
			}
			if (content[i] == '&' && num == num2)
			{
				list.Add(content.Substring(num3, i - num3));
				num3 = i + 1;
			}
		}
		list.Add(content.Substring(num3, content.Length - num3));
		return list.ToArray();
	}

	private static void ParsePreprocess(string preprocess, ref SkillFilterCreator.ContentInfo[] retInfos)
	{
		retInfos = ParseContentInfos(preprocess);
	}

	private static void ParseOption(string option, ref SkillFilterCreator.ContentInfo[] retInfos)
	{
		retInfos = ParseContentInfos(option);
	}

	public static void ParseCondition(string condition, ref List<SkillFilterCreator.ContentInfo> retOldInfos, ref List<string> retNewInfos)
	{
		string[] array = DivideContents(condition);
		retNewInfos = new List<string>();
		retOldInfos = new List<SkillFilterCreator.ContentInfo>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (IsNewStyle(array[i]))
			{
				retNewInfos.Add(array[i]);
				continue;
			}
			SkillFilterCreator.ParseContentInfo(array[i], out var retParsedInfo);
			retOldInfos.Add(retParsedInfo);
		}
	}

	public static void ParseTarget(string target, ref List<SkillFilterCreator.ContentInfo> retOldInfos, ref List<string> retNewInfos, ref List<string> checkKeywords)
	{
		string[] array = DivideContents(target);
		retNewInfos = new List<string>();
		retOldInfos = new List<SkillFilterCreator.ContentInfo>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (IsNewStyle(array[i]))
			{
				retNewInfos.Add(array[i]);
				checkKeywords.AddRange(GetCheckKeywords(array[i]));
				continue;
			}
			SkillFilterCreator.ParseContentInfo(array[i], out var retParsedInfo);
			retOldInfos.Add(retParsedInfo);
			if (retParsedInfo.Name != SkillFilterCreator.ContentKeyword.exclution)
			{
				checkKeywords.AddRange(GetCheckKeywords(retParsedInfo.Value.ToString()));
				checkKeywords.AddRange(GetCheckKeywords(retParsedInfo.ValueStr));
			}
		}
	}

	private static string[] GetCheckKeywords(string str)
	{
		return str.Replace("{", string.Empty).Replace("}", string.Empty).Split('.');
	}

	private static void SetupSkillConditionNew(SkillBase skill, BattleCardBase ownerCard, List<string> parsedInfos)
	{
		if (parsedInfos != null)
		{
			int i = 0;
			for (int count = parsedInfos.Count; i < count; i++)
			{
				SkillFilterCreator.SetupCondition(skill.ConditionFilterCollection, parsedInfos[i], ownerCard, skill);
			}
		}
	}

	public static void SetupSkillConditionOld(ConditionSkillFilterCollection conditionFilterCollection, List<SkillFilterCreator.ContentInfo> parsedInfos, BattleCardBase ownerCard, SkillBase skill)
	{
		if (parsedInfos == null)
		{
			return;
		}
		bool flag = true;
		int i = 0;
		for (int count = parsedInfos.Count; i < count; i++)
		{
			SkillFilterCreator.ContentInfo info = parsedInfos[i];
			switch (info.Name)
			{
			case SkillFilterCreator.ContentKeyword.character:
				conditionFilterCollection.BattlePlayerFilter = SkillFilterCreator.CreateBattlePlayerFilter(info.Value);
				break;
			case SkillFilterCreator.ContentKeyword.target:
				conditionFilterCollection.TargetFilter = CreateTargetFilterOld(info, info.Value, ownerCard, skill);
				break;
			case SkillFilterCreator.ContentKeyword.card_type:
				conditionFilterCollection.CardFilterList.Add(SkillFilterCreator.CreateCardTypeFilter(info.Value));
				flag = false;
				break;
			case SkillFilterCreator.ContentKeyword.tribe:
				conditionFilterCollection.CardFilterList.Add(SkillFilterCreator.CreateTribeFilter(info));
				break;
			case SkillFilterCreator.ContentKeyword.summon_moment_tribe:
				conditionFilterCollection.CardFilterList.Add(new SkillSummonMomentTribeFilter(SkillFilterCreator.ParseEnum(info.ValueStr, CardBasePrm.TribeType.MAX), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.play_moment_tribe:
				conditionFilterCollection.CardFilterList.Add(new SkillPlayMomentTribeFilter(SkillFilterCreator.ParseEnum(info.ValueStr, CardBasePrm.TribeType.MAX), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.play_moment_spell_charge:
				conditionFilterCollection.CardFilterList.Add(new SkillPlayMomentSpellChargeFilter());
				break;
			case SkillFilterCreator.ContentKeyword.update_deck_moment_tribe:
				conditionFilterCollection.CardFilterList.Add(new SkillUpdateDeckMomentTribeFilter(SkillFilterCreator.ParseEnum(info.ValueStr, CardBasePrm.TribeType.MAX), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.clan:
				conditionFilterCollection.CardFilterList.Add(SkillFilterCreator.CreateClanFilter(info, ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.ability:
				conditionFilterCollection.CardFilterList.Add(SkillFilterCreator.CreateAbilityFilter(info.Value, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.evolution:
				conditionFilterCollection.CardFilterList.Add(new SkillEvolutionCardFilter(SkillFilterCreator.ParseBoolean(info.ValueStr)));
				break;
			case SkillFilterCreator.ContentKeyword.chant_count:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterChantCountFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.buff_count:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterBuffCountFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.buff_life_count:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterBuffLifeCountFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.status_cost:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterCostFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.status_offense:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterOffenseFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.status_life:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterLifeFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.id:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterIdFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.base_card_id:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterBaseCardIdFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.destroy:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterDestroyFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.previous_turn_attacked:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterPreviousTurnAttackedFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.has_skill:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterHasSkillFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.destroyed_by:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterDestroyedByFilter(info.Value));
				break;
			case SkillFilterCreator.ContentKeyword.returned_by:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterReturnedByFilter(ownerCard, info.Value));
				break;
			case SkillFilterCreator.ContentKeyword.destroyed_by_ability:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterDestroyedByAbilityFilter(info.Value));
				break;
			case SkillFilterCreator.ContentKeyword.destroyed_by_card_id:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterDestroyedByCardIdFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.destroyed_by_card_id_and_ability:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterDestroyedByCardIdAndAbilityFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.attack_count:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterAttackCountFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.max_attack_count:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterMaxAttackCountFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.berserk:
			{
				SkillConditionHalfLife item10 = new SkillConditionHalfLife(info.ValueStr);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item10);
				break;
			}
			case SkillFilterCreator.ContentKeyword.avarice:
			{
				SkillConditionAvarice item9 = new SkillConditionAvarice(info.ValueStr);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item9);
				break;
			}
			case SkillFilterCreator.ContentKeyword.wrath:
			{
				SkillConditionWrath item8 = new SkillConditionWrath(info.ValueStr);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item8);
				break;
			}
			case SkillFilterCreator.ContentKeyword.awake:
			{
				SkillConditionAwake item7 = new SkillConditionAwake(info.ValueStr);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item7);
				break;
			}
			case SkillFilterCreator.ContentKeyword.evolvable_turn:
			{
				SkillConditionEvolvableTurn item6 = new SkillConditionEvolvableTurn(info.ValueStr);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item6);
				break;
			}
			case SkillFilterCreator.ContentKeyword.resonance:
			{
				SkillConditionResonance item5 = new SkillConditionResonance(info.ValueStr);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item5);
				break;
			}
			case SkillFilterCreator.ContentKeyword.burial_rite:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionBurialRite(ownerCard, info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.pp_count:
			{
				SkillConditionPP item4 = new SkillConditionPP(ownerCard, int.Parse(info.ValueStr), info.Operator);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item4);
				break;
			}
			case SkillFilterCreator.ContentKeyword.trigger:
			{
				SkillConditionTrigger item3 = new SkillConditionTrigger(ownerCard, info.ValueStr);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item3);
				break;
			}
			case SkillFilterCreator.ContentKeyword.play_count:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionPlayCount(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.cemetery_count:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillCemeteryFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.attacker:
				conditionFilterCollection.ConditionCheckerFilterList.Add(CreateAttackerChecker(info.Value, ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.be_attacked:
				conditionFilterCollection.ConditionCheckerFilterList.Add(CreateBeAttackedChecker(info.Value, ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.in_hand:
				conditionFilterCollection.ConditionCheckerFilterList.Add(CreateInHandChecker(info.Value, ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.necromance:
				conditionFilterCollection.ConditionCheckerFilterList.Add(CreateNecromanceChecker(info.Value, ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.turn:
			{
				SkillConditionTurn item2 = new SkillConditionTurn(info.ValueStr, ownerCard);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item2);
				break;
			}
			case SkillFilterCreator.ContentKeyword.last_life:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterLastLifeFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.game_changed_max_life_count:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterChangeMaxLifeCountFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.base_offense:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterBaseOffenseFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.base_life:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterBaseLifeFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.shortage_deck_win:
			{
				SkillConditionShortageDeckWin item = new SkillConditionShortageDeckWin(info.ValueStr);
				conditionFilterCollection.ConditionCheckerFilterList.Add(item);
				break;
			}
			case SkillFilterCreator.ContentKeyword.reanimated_card:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionReanimatedSelf(ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.deck_summoned_card:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionDeckSelfSummonedSelf(ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.attaching_ability:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionAttachingAbility(info.Value));
				break;
			case SkillFilterCreator.ContentKeyword.is_odd_spell_charge:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionOddEvenSpellCharge(ownerCard, info.ValueStr, isOdd: true));
				break;
			case SkillFilterCreator.ContentKeyword.is_even_spell_charge:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionOddEvenSpellCharge(ownerCard, info.ValueStr, isOdd: false));
				break;
			case SkillFilterCreator.ContentKeyword.is_odd_offense:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionOddEvenOffense(ownerCard, info.ValueStr, isOdd: true));
				break;
			case SkillFilterCreator.ContentKeyword.is_even_offense:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionOddEvenOffense(ownerCard, info.ValueStr, isOdd: false));
				break;
			case SkillFilterCreator.ContentKeyword.display_other_users_message:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionDisplayOtherUsersMessage(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.me_language:
				conditionFilterCollection.ConditionCheckerFilterList.Add(new SkillConditionMeLanguage(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.charge_count:
				conditionFilterCollection.CardFilterList.Add(new SkillParameterChargeCountFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.is_inplay:
				conditionFilterCollection.CardFilterList.Add(new SkillConditionIsInplayCardFilter(SkillFilterCreator.ParseBoolean(info.ValueStr)));
				break;
			case SkillFilterCreator.ContentKeyword.select_cardlist:
				conditionFilterCollection.CardFilterList.Add(new SkillOnlyOneCardFilter(int.Parse(info.ValueStr)));
				break;
			case SkillFilterCreator.ContentKeyword.load_target:
				conditionFilterCollection.CardFilterList.Add(new SkillLoadTargetFilter(ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.not_unique_base_card_id_card:
				conditionFilterCollection.CardFilterList.Add(new SkillTargetNotUniqueBaseCardIdFilter());
				break;
			case SkillFilterCreator.ContentKeyword.attacked:
				if (ownerCard.SelfBattlePlayer.BattleMgr is SingleBattleMgr)
				{
					conditionFilterCollection.CardFilterList.Add(new SkillAttackedCardFilter(info.ValueStr));
				}
				break;
			}
		}
		conditionFilterCollection.BattlePlayerFilter = conditionFilterCollection.BattlePlayerFilter ?? new SelfBattlePlayerFilter();
		conditionFilterCollection.TargetFilter = conditionFilterCollection.TargetFilter ?? new SkillTargetSelfFilter(ownerCard);
		if (flag)
		{
			if (ownerCard.IsUnit)
			{
				conditionFilterCollection.CardFilterList.Add(new SkillUnitFilter());
			}
			else if (ownerCard.IsSpell)
			{
				conditionFilterCollection.CardFilterList.Add(new SkillSpellFilter());
			}
			else if (ownerCard.IsChantField)
			{
				conditionFilterCollection.CardFilterList.Add(new SkillChantFieldFilter());
			}
			else if (ownerCard.IsField)
			{
				conditionFilterCollection.CardFilterList.Add(new SkillFieldFilter());
			}
			else if (ownerCard.IsClass)
			{
				conditionFilterCollection.CardFilterList.Add(new SkillClassFilter());
			}
		}
		conditionFilterCollection.CardFilterList.TrimExcess();
		conditionFilterCollection.ConditionCheckerFilterList.TrimExcess();
	}

	private static ISkillCustomSelectFilter CreateCustomSelectFilter(SkillFilterCreator.ContentInfo info, SkillFilterCreator.ContentKeyword keyword, BattleCardBase ownerCard)
	{
		return keyword switch
		{
			SkillFilterCreator.ContentKeyword.over_cost_from_last_target => new SkillTargetOverCostFromLastTargetFilter(), 
			SkillFilterCreator.ContentKeyword.equal_or_less_cost_from_last_target => new SkillTargetEqualOrLessCostFromLastTarget(info.CusomValue), 
			SkillFilterCreator.ContentKeyword.in_order_from_oldest => new SkillInOrderFromOldestFilter(), 
			_ => null, 
		};
	}

	private static void SetupSkillTargetNew(SkillBase skill, BattleCardBase ownerCard, List<string> parsedInfos)
	{
		if (parsedInfos != null)
		{
			int i = 0;
			for (int count = parsedInfos.Count; i < count; i++)
			{
				SkillFilterCreator.SetupTarget(skill.ApplyFilterCollection, parsedInfos[i], ownerCard, skill);
			}
		}
	}

	private static ISkillTargetFilter CreateTargetFilterOld(SkillFilterCreator.ContentInfo info, SkillFilterCreator.ContentKeyword keyword, BattleCardBase ownerCard, SkillBase skill)
	{
		bool flag = ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase && !ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork;
		switch (keyword)
		{
		case SkillFilterCreator.ContentKeyword.inplay:
			return new SkillTargetInPlayFilter();
		case SkillFilterCreator.ContentKeyword.deck:
			return new SkillTargetDeckFilter();
		case SkillFilterCreator.ContentKeyword.battle_start_deck:
			return new SkillTargetBattleStartDeckFilter();
		case SkillFilterCreator.ContentKeyword.cemetery:
			return new SkillTargetCemeteryFilter();
		case SkillFilterCreator.ContentKeyword.hand:
			return new SkillTargetHandFilter();
		case SkillFilterCreator.ContentKeyword.hand_self:
			return new SkillTargetHandSelfFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.hand_other_self:
			return new SkillTargetHandOtherSelfFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.hand_other_oldest:
			return new SkillTargetHandOtherOldestFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.return_card:
			return new SkillTargetReturnCardFilter();
		case SkillFilterCreator.ContentKeyword.cant_attack_all_return_card:
			return new SkillTargetCantAttackAllReturnCardFilter();
		case SkillFilterCreator.ContentKeyword.self:
			return new SkillTargetSelfFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.deck_self:
			return new SkillTargetDeckSelfFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.inplay_self:
			return new SkillTargetInPlaySelfFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.inplay_other_self:
			return new SkillTargetInPlayOtherSelfFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.newer_inplay_other_self:
			return new SkillTargetNewerInPlayOtherSelfFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.attacker:
			return new SkillTargetAttackerFilter();
		case SkillFilterCreator.ContentKeyword.be_attacked:
			return new SkillTargetBeAttackedFilter();
		case SkillFilterCreator.ContentKeyword.other_than_be_attacked:
			return new SkillTargetOtherThanBeAttackedFilter();
		case SkillFilterCreator.ContentKeyword.necromance:
			return new SkillTargetNecromanceFilter();
		case SkillFilterCreator.ContentKeyword.played_card:
			return new SkillTargetPlayedCardFilter();
		case SkillFilterCreator.ContentKeyword.enhance_card:
			return new SkillTargetEnhanceCardfilter();
		case SkillFilterCreator.ContentKeyword.accelerated_card:
			return new SkillTargetAcceleratedCardfilter();
		case SkillFilterCreator.ContentKeyword.crystallized_card:
			return new SkillTargetCrystallizedCardfilter();
		case SkillFilterCreator.ContentKeyword.summoned_card:
			return new SkillTargetSummonedCardFilter();
		case SkillFilterCreator.ContentKeyword.burial_rite_card:
			return new SkillTargetBurialRiteCardFilter();
		case SkillFilterCreator.ContentKeyword.burial_rite_card_list:
			return new SkillTargetBurialRiteCardListFilter();
		case SkillFilterCreator.ContentKeyword.burial_rite_this_turn_card_list:
			return new SkillTargetBurialRiteThisTurnCardListFilter();
		case SkillFilterCreator.ContentKeyword.reanimated_this_turn_card_list:
			return new SkillTargetReanimatedThisTurnCardListFilter();
		case SkillFilterCreator.ContentKeyword.banished_card:
			return new SkillTargetBanishedCardFilter();
		case SkillFilterCreator.ContentKeyword.inplay_banished_card:
			return new SkillTargetInplayBanishedCardFilter();
		case SkillFilterCreator.ContentKeyword.hand_banished_card:
			return new SkillTargetHandBanishedCardFilter();
		case SkillFilterCreator.ContentKeyword.inplay_banished_this_turn_card_list:
			return new SkillTargetInplayBanishedThisTurnCardListFilter();
		case SkillFilterCreator.ContentKeyword.hand_banished_this_turn_card_list:
			return new SkillTargetHandBanishedThisTurnCardListFilter();
		case SkillFilterCreator.ContentKeyword.inplay_banished_card_list:
			return new SkillTargetInplayBanishedCardListFilter();
		case SkillFilterCreator.ContentKeyword.return_this_turn_card_list:
			return new SkillTargetReturnThisTurnCardListFilter();
		case SkillFilterCreator.ContentKeyword.destroyed_card:
			return new SkillTargetDestroyedCardFilter();
		case SkillFilterCreator.ContentKeyword.destroyed_card_list:
			return new SkillTargetDestroyedCardListFilter();
		case SkillFilterCreator.ContentKeyword.turn_destroyed_cards:
			return new SkillTurnDestroyedFilter(info.ValueStr);
		case SkillFilterCreator.ContentKeyword.lastword_card_list:
			return new SkillTargetDestroyedWhenDestroyCardListFilter();
		case SkillFilterCreator.ContentKeyword.destroyed_this_turn_card_list:
			return new SkillTargetDestroyedThisTurnCardListFilter();
		case SkillFilterCreator.ContentKeyword.discard_this_turn_card_list:
			return new SkillTargetDiscardThisTurnCardListFilter();
		case SkillFilterCreator.ContentKeyword.discarded_card:
			return new SkillTargetDiscardedCardFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.damaged_card:
			return new SkillTargetDamagedCardFilter();
		case SkillFilterCreator.ContentKeyword.not_damaged_card:
			return new SkillTargetNotDamagedCardFilter();
		case SkillFilterCreator.ContentKeyword.skill_drew_card:
			return new SkillTargetSkillDrewCardFilter();
		case SkillFilterCreator.ContentKeyword.skill_update_deck_card:
			return new SkillTargetSkillUpdateDeckCardFilter();
		case SkillFilterCreator.ContentKeyword.last_target:
			if (!flag)
			{
				return new SkillTargetLastTargetFilter(info.CusomValue);
			}
			return new NetworkSkillTargetLastTargetFilter(info.CusomValue, skill);
		case SkillFilterCreator.ContentKeyword.inplay_last_target:
			return new SkillTargetInplayLastTargetFilter(info.CusomValue);
		case SkillFilterCreator.ContentKeyword.destroyed_last_target:
			return new SkillTargetDestroyedLastTargetFilter(info.CusomValue);
		case SkillFilterCreator.ContentKeyword.banished_last_target:
			return new SkillTargetBanishedLastTargetFilter(info.CusomValue);
		case SkillFilterCreator.ContentKeyword.cant_attack_all_last_target:
			return new SkillTargetCantAttackAllLastTargetFilter(info.CusomValue);
		case SkillFilterCreator.ContentKeyword.in_hand:
			return new SkillTargetInHandCardFilter();
		case SkillFilterCreator.ContentKeyword.skill_summoned_card:
			return new SkillTargetSkillSummonedCardFilter();
		case SkillFilterCreator.ContentKeyword.healing_card:
			return new SkillTargetHealingCardFilter();
		case SkillFilterCreator.ContentKeyword.received_damage_card:
			return new SkillTargetReceivedDamageCardFilter();
		case SkillFilterCreator.ContentKeyword.give_damage_card:
			return new SkillTargetGiveDamageCardFilter();
		case SkillFilterCreator.ContentKeyword.evolution_card:
			return new SkillTargetEvolutionCardFilter();
		case SkillFilterCreator.ContentKeyword.fusion_ingrediented_card:
			return new SkillTargetFusionIngredientCardsFilter();
		case SkillFilterCreator.ContentKeyword.through_fusion_ingrediented_card:
			return new SkillTargetThroughFusionIngredientedCardFilter(ownerCard, info.ValueStr);
		case SkillFilterCreator.ContentKeyword.fusion_ingrediented_card_list_include_this_fusion:
			return new SkillTargetFusionIngredientedCardListIncludeThisFusion(ownerCard);
		case SkillFilterCreator.ContentKeyword.game_fusion_ingrediented_cards:
			return new SkillTargetGameFusionIngredientedCards();
		case SkillFilterCreator.ContentKeyword.game_fusion_ingrediented_and_discard_cards:
			return new SkillTargetGameFusionIngredientedAndDiscardCards();
		case SkillFilterCreator.ContentKeyword.evolved_card_list:
			return new SkillTargetEvolvedCardListFilter();
		case SkillFilterCreator.ContentKeyword.discard:
			return new SkillTargetDiscardFilter();
		case SkillFilterCreator.ContentKeyword.unique_base_card_id_card:
			return new SkillTargetUniqueBaseCardIDCardFilter();
		case SkillFilterCreator.ContentKeyword.fight_target:
			return new SkillTargetFightTargetFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.selected_cards:
			return new SkillTargetSelectedCardsFilter();
		case SkillFilterCreator.ContentKeyword.chosen_cards:
			return new SkillTargetChosenCardsFilter();
		case SkillFilterCreator.ContentKeyword.turn_play_cards:
			return new SkillTargetTurnPlayCardsFilter();
		case SkillFilterCreator.ContentKeyword.turn_draw_cards:
			return new SkillTargetTurnDrawCardsFilter();
		case SkillFilterCreator.ContentKeyword.turn_token_draw_skill_id:
			return new SkillTargetTurnTokenDrawSkillIdFilter(info.ValueStr);
		case SkillFilterCreator.ContentKeyword.turn_summon_cards:
			return new SkillTargetTurnSummonCardsFilter();
		case SkillFilterCreator.ContentKeyword.past_summon_cards:
			return new SkillTargetPastSummonCardsFilter(ownerCard, info.ValueStr);
		case SkillFilterCreator.ContentKeyword.turn_play_cards_other_self:
			return new SkillTargetTurnPlayCardsOtherSelfFilter(ownerCard, info.ValueStr);
		case SkillFilterCreator.ContentKeyword.game_play_cards_other_self:
			return new SkillTargetGamePlayCardsOtherSelfFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.game_play_cards:
			return new SkillTargetGamePlayCardsFilter();
		case SkillFilterCreator.ContentKeyword.game_summon_cards:
			return new SkillTargetGameSummonCardsFilter();
		case SkillFilterCreator.ContentKeyword.game_summon_cards_other:
			return new SkillTargetGameSummonCardsOtherFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.inplay_self_and_class:
			return new SkillTargetInplaySelfAndClassFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.token_draw_card:
			return new SkillTargetTokenDrawCardFilter();
		case SkillFilterCreator.ContentKeyword.inplay_and_hand:
			return new SkillTargetInplayAndHandFilter();
		case SkillFilterCreator.ContentKeyword.reanimated_card:
			return new SkillTargetReanimatedCardFilter();
		case SkillFilterCreator.ContentKeyword.game_accelerated_cards:
			return new SkillTargetGameAcceleratedCardsFilter();
		case SkillFilterCreator.ContentKeyword.game_accelerated_cards_other_self:
			return new SkillTargetGameAcceleratedCardsOtherSelfFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.game_crystallized_cards:
			return new SkillTargetGameCrystallizedCardsFilter();
		case SkillFilterCreator.ContentKeyword.game_skill_activated:
			return new SkillTargetGameSkillActivatedFilter();
		case SkillFilterCreator.ContentKeyword.deck_draw_card:
			return new SkillTargetDeckDrawCardFilter();
		case SkillFilterCreator.ContentKeyword.drew_over_hand_limit:
			return new SkillTargetDrewOverHandLimitFilter();
		case SkillFilterCreator.ContentKeyword.left_cards:
			return new SkillTargetLeftCardsFilter();
		case SkillFilterCreator.ContentKeyword.inplay_buffing_cards:
			return new SkillTargetInplayBuffingCardsFilter();
		case SkillFilterCreator.ContentKeyword.inplay_debuffing_cards:
			return new SkillTargetInplayDebuffingCardsFilter();
		case SkillFilterCreator.ContentKeyword.game_deck_draw_cards:
			return new SkillTargetGameDeckDrawCardsFilter();
		case SkillFilterCreator.ContentKeyword.game_draw_cards:
			return new SkillTargetGameDrawCardsFilter();
		case SkillFilterCreator.ContentKeyword.game_add_update_deck_cards:
			return new SkillTargetGameAddUpdateDeckCardsFilter();
		case SkillFilterCreator.ContentKeyword.game_left_cards:
			return new SkillTargetGameLeftCardsFilter();
		case SkillFilterCreator.ContentKeyword.left_this_turn_card_list:
			return new SkillTargetLeftThisTurnCardListFilter();
		case SkillFilterCreator.ContentKeyword.before_transform_card:
			return new SkillTargetBeforeTransformCardFilter(ownerCard);
		case SkillFilterCreator.ContentKeyword.discard_card_list:
			return new SkillTargetDiscardCardListFilter();
		case SkillFilterCreator.ContentKeyword.chant_count_change_cards:
			return new SkillTargetChantCountChangeCardsFilter();
		case SkillFilterCreator.ContentKeyword.shortage_deck_win_cards:
			return new SkillTargetShortageDeckWinCards();
		case SkillFilterCreator.ContentKeyword.load_target:
			return new SkillTargetLoadTargetFilter(ownerCard);
		default:
			return null;
		}
	}

	private static ISkillConditionChecker CreateAttackerChecker(SkillFilterCreator.ContentKeyword keyword, BattleCardBase ownerCard)
	{
		return keyword switch
		{
			SkillFilterCreator.ContentKeyword.self => new SkillConditionAttackerIsSelf(ownerCard), 
			SkillFilterCreator.ContentKeyword.other => new SkillConditionAttackerIsOther(ownerCard), 
			_ => null, 
		};
	}

	private static ISkillConditionChecker CreateBeAttackedChecker(SkillFilterCreator.ContentKeyword keyword, BattleCardBase ownerCard)
	{
		return keyword switch
		{
			SkillFilterCreator.ContentKeyword.self => new SkillConditionBeAttackedIsSelf(ownerCard), 
			SkillFilterCreator.ContentKeyword.other => new SkillConditionBeAttackedIsOther(ownerCard), 
			SkillFilterCreator.ContentKeyword.destroy => new SkillConditionBeAttackedIsDestroy(), 
			_ => null, 
		};
	}

	private static ISkillConditionChecker CreateInHandChecker(SkillFilterCreator.ContentKeyword keyword, BattleCardBase ownerCard)
	{
		return keyword switch
		{
			SkillFilterCreator.ContentKeyword.self => new SkillConditionInHandIsSelf(ownerCard), 
			SkillFilterCreator.ContentKeyword.other => new SkillConditionInHandIsOther(ownerCard), 
			_ => null, 
		};
	}

	private static ISkillConditionChecker CreateNecromanceChecker(SkillFilterCreator.ContentKeyword keyword, BattleCardBase ownerCard)
	{
		return keyword switch
		{
			SkillFilterCreator.ContentKeyword.self => new SkillConditionBeAttackedIsSelf(ownerCard), 
			SkillFilterCreator.ContentKeyword.other => new SkillConditionBeAttackedIsOther(ownerCard), 
			_ => null, 
		};
	}

	public void SetupSkillTargetOld(ApplySkillTargetFilterCollection filterCollection, BattleCardBase ownerCard, List<SkillFilterCreator.ContentInfo> parsedInfo, SkillBase skill)
	{
		bool flag = true;
		int i = 0;
		for (int count = parsedInfo.Count; i < count; i++)
		{
			SkillFilterCreator.ContentInfo info = parsedInfo[i];
			switch (info.Name)
			{
			case SkillFilterCreator.ContentKeyword.character:
				filterCollection.BattlePlayerFilter = SkillFilterCreator.CreateBattlePlayerFilter(info.Value);
				break;
			case SkillFilterCreator.ContentKeyword.target:
				filterCollection.TargetFilter = CreateTargetFilterOld(info, info.Value, ownerCard, skill);
				break;
			case SkillFilterCreator.ContentKeyword.turn_destroyed_cards:
				filterCollection.CardFilterList.Add(new SkillCardTurnDestroyedFilter(ownerCard, info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.custom_select:
				filterCollection.ApplyCustomSelectFilterList.Add(CreateCustomSelectFilter(info, info.Value, ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.card_type:
				filterCollection.CardFilterList.Add(SkillFilterCreator.CreateCardTypeFilter(info.Value));
				flag = false;
				break;
			case SkillFilterCreator.ContentKeyword.tribe:
				filterCollection.CardFilterList.Add(SkillFilterCreator.CreateTribeFilter(info));
				break;
			case SkillFilterCreator.ContentKeyword.summon_moment_tribe:
				filterCollection.CardFilterList.Add(new SkillSummonMomentTribeFilter(SkillFilterCreator.ParseEnum(info.ValueStr, CardBasePrm.TribeType.MAX), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.play_moment_tribe:
				filterCollection.CardFilterList.Add(new SkillPlayMomentTribeFilter(SkillFilterCreator.ParseEnum(info.ValueStr, CardBasePrm.TribeType.MAX), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.play_moment_spell_charge:
				filterCollection.CardFilterList.Add(new SkillPlayMomentSpellChargeFilter());
				break;
			case SkillFilterCreator.ContentKeyword.update_deck_moment_tribe:
				filterCollection.CardFilterList.Add(new SkillUpdateDeckMomentTribeFilter(SkillFilterCreator.ParseEnum(info.ValueStr, CardBasePrm.TribeType.MAX), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.clan:
				filterCollection.CardFilterList.Add(SkillFilterCreator.CreateClanFilter(info, ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.ability:
				filterCollection.CardFilterList.Add(SkillFilterCreator.CreateAbilityFilter(info.Value, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.evolution:
				filterCollection.CardFilterList.Add(new SkillEvolutionCardFilter(SkillFilterCreator.ParseBoolean(info.ValueStr)));
				break;
			case SkillFilterCreator.ContentKeyword.is_inplay:
				filterCollection.CardFilterList.Add(new SkillConditionIsInplayCardFilter(SkillFilterCreator.ParseBoolean(info.ValueStr)));
				break;
			case SkillFilterCreator.ContentKeyword.random_count:
				filterCollection.ApplySelectFilter = CreateRandomSelectFilter(info.ValueStr);
				break;
			case SkillFilterCreator.ContentKeyword.random_count_until:
				filterCollection.ApplySelectFilter = CreateRandomSelectUntilFilter(info.ValueStr, ownerCard.SelfBattlePlayer, skill);
				break;
			case SkillFilterCreator.ContentKeyword.id_no_duplication_random_count:
				filterCollection.ApplySelectFilter = CreateIdNoDuplicationRandomSelectFilter(info.ValueStr);
				break;
			case SkillFilterCreator.ContentKeyword.no_duplication_random_count_in_order:
				filterCollection.ApplySelectFilter = CreateNoDuplicationRandomSelectInOrderFilter(info.ValueStr, skill);
				break;
			case SkillFilterCreator.ContentKeyword.cost_no_duplication_random_count:
				filterCollection.ApplySelectFilter = CreateCostNoDuplicationRandomSelectFilter(info.ValueStr);
				break;
			case SkillFilterCreator.ContentKeyword.select_count:
				filterCollection.ApplySelectFilter = new SkillUserSelectFilter(info.ValueStr);
				break;
			case SkillFilterCreator.ContentKeyword.choice_count:
				filterCollection.ApplySelectFilter = new SkillChoiceSelectFilter(info.ValueStr);
				break;
			case SkillFilterCreator.ContentKeyword.each_same_base_card_id_count_until:
				filterCollection.ApplySelectFilter = CreateSkillRandomEachSameBaseCardIdFilter(int.Parse(info.ValueStr), ownerCard.SelfBattlePlayer, skill);
				break;
			case SkillFilterCreator.ContentKeyword.chant_count:
				filterCollection.CardFilterList.Add(new SkillParameterChantCountFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.buff_count:
				filterCollection.CardFilterList.Add(new SkillParameterBuffCountFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.buff_life_count:
				filterCollection.CardFilterList.Add(new SkillParameterBuffLifeCountFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.status_cost:
				filterCollection.CardFilterList.Add(new SkillParameterCostFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.base_cost:
				filterCollection.CardFilterList.Add(new SkillParameterBaseCostFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.status_offense:
				filterCollection.CardFilterList.Add(new SkillParameterOffenseFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.status_life:
				filterCollection.CardFilterList.Add(new SkillParameterLifeFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.base_offense:
				filterCollection.CardFilterList.Add(new SkillParameterBaseOffenseFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.base_life:
				filterCollection.CardFilterList.Add(new SkillParameterBaseLifeFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.id:
				filterCollection.CardFilterList.Add(new SkillParameterIdFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.base_card_id:
				filterCollection.CardFilterList.Add(new SkillParameterBaseCardIdFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.destroy:
				filterCollection.CardFilterList.Add(new SkillParameterDestroyFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.previous_turn_attacked:
				filterCollection.CardFilterList.Add(new SkillParameterPreviousTurnAttackedFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.has_skill:
				filterCollection.CardFilterList.Add(new SkillParameterHasSkillFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.destroyed_by:
				filterCollection.CardFilterList.Add(new SkillParameterDestroyedByFilter(info.Value));
				break;
			case SkillFilterCreator.ContentKeyword.returned_by:
				filterCollection.CardFilterList.Add(new SkillParameterReturnedByFilter(ownerCard, info.Value));
				break;
			case SkillFilterCreator.ContentKeyword.destroyed_by_ability:
				filterCollection.CardFilterList.Add(new SkillParameterDestroyedByAbilityFilter(info.Value));
				break;
			case SkillFilterCreator.ContentKeyword.destroyed_by_card_id:
				filterCollection.CardFilterList.Add(new SkillParameterDestroyedByCardIdFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.destroyed_by_card_id_and_ability:
				filterCollection.CardFilterList.Add(new SkillParameterDestroyedByCardIdAndAbilityFilter(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.exclution:
				filterCollection.ApplyExclutionFilterList.Add(CreateExclutionFilter(info.ValueStr, skill));
				break;
			case SkillFilterCreator.ContentKeyword.attack_count:
				filterCollection.CardFilterList.Add(new SkillParameterAttackCountFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.max_attack_count:
				filterCollection.CardFilterList.Add(new SkillParameterMaxAttackCountFilter(info.ValueStr, info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.none:
				filterCollection.BattlePlayerFilter = filterCollection.BattlePlayerFilter ?? new NullBattlePlayerFilter();
				filterCollection.TargetFilter = filterCollection.TargetFilter ?? new SkillTargetNullFilter();
				filterCollection.CardFilterList.Add(new SkillNullFilter());
				filterCollection.ApplyExclutionFilterList.Add(new SkillExclutionNoneFilter());
				filterCollection.ApplySelectFilter = filterCollection.ApplySelectFilter ?? new SkillSelectNullFilter();
				flag = false;
				break;
			case SkillFilterCreator.ContentKeyword.last_life:
				filterCollection.CardFilterList.Add(new SkillParameterLastLifeFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.game_changed_max_life_count:
				filterCollection.CardFilterList.Add(new SkillParameterChangeMaxLifeCountFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.select_index:
				filterCollection.ApplySelectFilter = new SkillSelectIndexFilter(int.Parse(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.select_cardlist:
				filterCollection.ApplySelectFilter = new SkillOnlyOneFilter(info.ValueStr);
				break;
			case SkillFilterCreator.ContentKeyword.limit_upper_count_from_oldest:
				filterCollection.ApplySelectFilter = new SkillLimitUpperCountFromOldestFilter(int.Parse(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.limit_upper_count_from_newest:
				filterCollection.ApplySelectFilter = new SkillLimitUpperCountFromNewestFilter(int.Parse(info.ValueStr));
				break;
			case SkillFilterCreator.ContentKeyword.charge_count:
				filterCollection.CardFilterList.Add(new SkillParameterChargeCountFilter(int.Parse(info.ValueStr), info.Operator));
				break;
			case SkillFilterCreator.ContentKeyword.life_is_larger:
				filterCollection.CardFilterList.Add(new SkillLifeIsLarger());
				break;
			case SkillFilterCreator.ContentKeyword.attack_is_larger:
				filterCollection.CardFilterList.Add(new SkillAttackIsLarger());
				break;
			case SkillFilterCreator.ContentKeyword.load_target:
				filterCollection.CardFilterList.Add(new SkillLoadTargetFilter(ownerCard));
				break;
			case SkillFilterCreator.ContentKeyword.not_unique_base_card_id_card:
				filterCollection.CardFilterList.Add(new SkillTargetNotUniqueBaseCardIdFilter());
				break;
			case SkillFilterCreator.ContentKeyword.attacked:
				if (ownerCard.SelfBattlePlayer.BattleMgr is SingleBattleMgr)
				{
					filterCollection.CardFilterList.Add(new SkillAttackedCardFilter(info.ValueStr));
				}
				break;
			}
		}
		filterCollection.BattlePlayerFilter = filterCollection.BattlePlayerFilter ?? new SelfBattlePlayerFilter();
		filterCollection.TargetFilter = filterCollection.TargetFilter ?? new SkillTargetSelfFilter(ownerCard);
		if (flag)
		{
			filterCollection.CardFilterList.Add(new SkillUnitFilter());
		}
		filterCollection.ApplySelectFilter = filterCollection.ApplySelectFilter ?? new SkillSelectAllFilter();
	}

	protected virtual ISkillSelectFilter CreateRandomSelectFilter(string count)
	{
		return new SkillRandomSelectFilter(count);
	}

	protected virtual ISkillSelectFilter CreateRandomSelectUntilFilter(string count, BattlePlayerBase player, SkillBase skill)
	{
		if (!(skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase) || skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork)
		{
			return new SkillRandomSelectUntilFilter(count, player);
		}
		return new NetworkSkillRandomSelectUntilFilter(count, player, skill);
	}

	protected virtual ISkillSelectFilter CreateIdNoDuplicationRandomSelectFilter(string count)
	{
		return new SkillIdNoDuplicationRandomSelectFilter(count);
	}

	protected virtual ISkillSelectFilter CreateNoDuplicationRandomSelectInOrderFilter(string option, SkillBase skill)
	{
		return new SkillNoDuplicationRandomSelectInOrderFilter(option, skill);
	}

	protected virtual ISkillSelectFilter CreateCostNoDuplicationRandomSelectFilter(string count)
	{
		return new SkillCostNoDuplicationRandomSelectFilter(count);
	}

	protected virtual ISkillSelectFilter CreateSkillRandomEachSameBaseCardIdFilter(int count, BattlePlayerBase player, SkillBase skill)
	{
		if (!(skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase) || skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork)
		{
			return new SkillRandomEachSameBaseCardIdFilter(count, player);
		}
		return new NetworkSkillRandomEachSameBaseCardIdFilter(count, player, skill);
	}

	protected virtual ISkillExclutionFilter CreateExclutionFilter(string text, SkillBase skill)
	{
		if (text.First() == '{')
		{
			return new SkillExclutionApplyFilter(_ownerCard, text, skill);
		}
		return new SkillExclutionNoneFilter();
	}

	private static SkillPreprocessBase MakeSkillPreprocess(SkillBase skill, SkillFilterCreator.ContentInfo info, ref bool isExecutionCheck, ref bool isReferencePreviousPreprocess)
	{
		bool flag = skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase && !skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork;
		SkillPreprocessBase skillPreprocessBase = null;
		switch (info.Name)
		{
		case SkillFilterCreator.ContentKeyword.use_pp:
			skillPreprocessBase = new SkillPreprocessUsePp(int.Parse(info.ValueStr));
			break;
		case SkillFilterCreator.ContentKeyword.necromance:
			skillPreprocessBase = new SkillPreprocessNecromance(skill, info.ValueStr, info.Operator);
			break;
		case SkillFilterCreator.ContentKeyword.burial_rite:
			skillPreprocessBase = new SkillPreprocessBurialRite(skill, skill.SkillPrm.ownerCard);
			break;
		case SkillFilterCreator.ContentKeyword.skill_random_count:
			skillPreprocessBase = new SkillPreprocessRandomCount(skill.SkillPrm.ownerCard, int.Parse(info.ValueStr));
			isExecutionCheck = true;
			break;
		case SkillFilterCreator.ContentKeyword.fixed_generic_value:
		case SkillFilterCreator.ContentKeyword.fixed_generic_value_initial:
			skillPreprocessBase = new SkillPreprocessFixedGenericValue(skill.SkillPrm.ownerCard, info.Operator, int.Parse(info.ValueStr));
			isExecutionCheck = true;
			break;
		case SkillFilterCreator.ContentKeyword.destroy_tribe:
			skillPreprocessBase = new SkillPreprocessDestroyTribe(skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.turn_start_skill:
			skillPreprocessBase = new SkillPreprocessTurnStartSkill(skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.turn_start_stop:
			skillPreprocessBase = new SkillPreprocessTurnStartStop(info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.turn_start_skill_after_stop:
			skillPreprocessBase = new SkillPreprocessTurnStartSkillAfterStop(info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.turn_end_skill_after_stop:
			skillPreprocessBase = new SkillPreprocessTurnEndSkillAfterStop(info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.turn_end_stop:
			skillPreprocessBase = new SkillPreprocessTurnEndStop(info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.turn_end_remove:
			skillPreprocessBase = new SkillPreprocessTurnEndRemove(info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.self_turn_end_remove:
			skillPreprocessBase = new SkillPreprocessSelfTurnEndRemove(skill, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.remove_from_inplay_stop:
			skillPreprocessBase = new SkillPreprocessRemoveFromInPlayStop();
			break;
		case SkillFilterCreator.ContentKeyword.evolution_end_stop:
			skillPreprocessBase = new SkillPreprocessEvolutionEndStop(skill.SkillPrm.ownerCard);
			break;
		case SkillFilterCreator.ContentKeyword.damage_after_stop:
			skillPreprocessBase = new SkillPreprocessDamageAfterStop();
			break;
		case SkillFilterCreator.ContentKeyword.damage_give_stop:
			skillPreprocessBase = new SkillPreprocessDamageGiveStop();
			break;
		case SkillFilterCreator.ContentKeyword.reflection_after_stop:
			skillPreprocessBase = new SkillPreprocessReflectionAfterStop();
			break;
		case SkillFilterCreator.ContentKeyword.remove_after_action:
			skillPreprocessBase = new SkillPreprocessRemoveAfterAction(info.ValueStr, skill);
			break;
		case SkillFilterCreator.ContentKeyword.evolution_end_remove:
			skillPreprocessBase = new SkillPreprocessWhenEvolveEndRemove(skill);
			break;
		case SkillFilterCreator.ContentKeyword.unit_attack_stop:
			skillPreprocessBase = new SkillPreprocessUnitAttackStop(info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.dont_select_start:
			skillPreprocessBase = new SkillPreprocessDontSelectStart();
			break;
		case SkillFilterCreator.ContentKeyword.inplay_period_of_time:
			skillPreprocessBase = new SkillPreprocessInPlayPeriodOfTime(skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.turn_end_period_of_stop_time:
			skillPreprocessBase = new SkillPreprocessTurnEndPeriodOfStopTime(skill, skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.turn_start_skill_after_period_of_stop_time:
			skillPreprocessBase = new SkillPreprocessTurnStartSkillAfterPeriodOfStopTime(skill, skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.turn_end_stop_and_remove:
			skillPreprocessBase = new SkillPreprocessTurnEndStopAndRemove(info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword._ref_prev:
			skillPreprocessBase = new SkillPreprocessReferencePrevious();
			isReferencePreviousPreprocess = true;
			break;
		case SkillFilterCreator.ContentKeyword.per_turn:
			skillPreprocessBase = new SkillPreprocessTimesPerTurn(skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.per_game:
			skillPreprocessBase = new SkillPreprocessTimesPerGame(int.Parse(info.ValueStr));
			break;
		case SkillFilterCreator.ContentKeyword.any_condition:
		{
			List<SkillPreprocessBase> list = new List<SkillPreprocessBase>();
			string preprocess = info.ValueStr.Substring(1, info.ValueStr.Length - 2);
			SkillFilterCreator.ContentInfo[] retInfos = null;
			ParsePreprocess(preprocess, ref retInfos);
			bool isExecutionCheck2 = false;
			bool isReferencePreviousPreprocess2 = false;
			SkillFilterCreator.ContentInfo[] array = retInfos;
			foreach (SkillFilterCreator.ContentInfo info2 in array)
			{
				SkillPreprocessBase skillPreprocessBase2 = MakeSkillPreprocess(skill, info2, ref isExecutionCheck2, ref isReferencePreviousPreprocess2);
				if (skillPreprocessBase2 != null)
				{
					list.Add(skillPreprocessBase2);
					if (isExecutionCheck2)
					{
						isExecutionCheck = true;
					}
					if (isReferencePreviousPreprocess2)
					{
						isReferencePreviousPreprocess2 = true;
					}
				}
			}
			skillPreprocessBase = new SkillPreprocessAnyCondition(list);
			break;
		}
		case SkillFilterCreator.ContentKeyword.none:
			return null;
		case SkillFilterCreator.ContentKeyword.use_ep:
			skillPreprocessBase = new SkillPreprocessUseEvolutionPoint(int.Parse(info.ValueStr), skill.SkillPrm.ownerCard);
			break;
		case SkillFilterCreator.ContentKeyword.once_per_action:
			skillPreprocessBase = new SkillPreprocessOncePerAction(skill);
			break;
		case SkillFilterCreator.ContentKeyword.open_card:
			skillPreprocessBase = new SkillPreprocessOpenCard(skill.SkillPrm.ownerCard);
			break;
		case SkillFilterCreator.ContentKeyword.only_random_index:
			skillPreprocessBase = new SkillPreprocessRandomArrayIndex(skill.SkillPrm.ownerCard, info.ValueStr.Split(':').Select(int.Parse).ToArray());
			isExecutionCheck = true;
			break;
		case SkillFilterCreator.ContentKeyword.preprocess_condition:
			skillPreprocessBase = (flag ? new NetworkSkillPreprocessConditionCheck(skill, info.ValueStr) : new SkillPreprocessConditionCheck(skill, info.ValueStr));
			break;
		case SkillFilterCreator.ContentKeyword.dont_activate:
			skillPreprocessBase = new SkillPreprocessDontActivate(skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.wrapping_and_adding_skill_activated_count:
			skillPreprocessBase = new SkillPreprocessWrappingAndAddingSkillActivatedCount(skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.skill_activate_count_by_simultaneous_destroyed_card_list:
			skillPreprocessBase = new SkillPreprocessSkillActivateCountBySimultaneousDestroyedCardList(skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.skill_activate_count_by_simultaneous_buffing_cards:
			skillPreprocessBase = new SkillPreprocessSkillActivateCountBySimultaneousBuffingCards(skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.skill_activate_count_by_simultaneous_summoned_card:
			skillPreprocessBase = new SkillPreprocessSkillActivateCountBySimultaneousSummonedCard(skill.SkillPrm.ownerCard, info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.skill_id:
			skillPreprocessBase = new SkillPreprocessSkillId(long.Parse(info.ValueStr));
			break;
		case SkillFilterCreator.ContentKeyword.skill_stop_and_remove_by_when_summon_other:
			skillPreprocessBase = new SkillPreprocessSkillStopAndRemoveByWhenSummonOther(info.ValueStr);
			break;
		case SkillFilterCreator.ContentKeyword.is_activate_summon_card:
			skillPreprocessBase = new SkillPreprocessIsActivateSummonCard(skill, info.ValueStr);
			break;
		default:
			return null;
		}
		return skillPreprocessBase;
	}

	private static void SetupSkillPreprocess(SkillBase skill, SkillFilterCreator.ContentInfo[] parsedInfo, IEnumerable<SkillPreprocessBase> lastSkillPreprocessCollection)
	{
		try
		{
			int i = 0;
			for (int num = parsedInfo.Length; i < num; i++)
			{
				SkillFilterCreator.ContentInfo info = parsedInfo[i];
				bool isReferencePreviousPreprocess = false;
				bool isExecutionCheck = false;
				SkillPreprocessBase skillPreprocessBase = MakeSkillPreprocess(skill, info, ref isExecutionCheck, ref isReferencePreviousPreprocess);
				if (skillPreprocessBase == null)
				{
					continue;
				}
				if (isReferencePreviousPreprocess)
				{
					foreach (SkillPreprocessBase item in lastSkillPreprocessCollection)
					{
						if (!(item is SkillPreprocessReferencePrevious))
						{
							skill.ConditionCheckerList.Add(item);
						}
					}
				}
				else if (!isExecutionCheck)
				{
					skill.ConditionCheckerList.Add(skillPreprocessBase);
				}
				skill.PreprocessList.Add(skillPreprocessBase);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Preprocessのマスター記述にエラーがあります。" + ex);
		}
	}

	public static SkillFilterCreator.ContentInfo[] ParseContentInfos(string text)
	{
		if (Regex.Match(text, "\\(.+?\\)").Value == "")
		{
			string[] array = null;
			array = ((!text.Contains(SkillFilterCreator.ContentKeyword.preprocess_condition.ToString())) ? text.Split('&') : ParenthesisParseContentInfos(text));
			SkillFilterCreator.ContentInfo[] array2 = new SkillFilterCreator.ContentInfo[array.Count()];
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				SkillFilterCreator.ParseContentInfo(array[i], out array2[i]);
			}
			return array2;
		}
		string[] array3 = ParenthesisParseContentInfos(text);
		SkillFilterCreator.ContentInfo[] array4 = new SkillFilterCreator.ContentInfo[array3.Count()];
		int j = 0;
		for (int num2 = array3.Length; j < num2; j++)
		{
			SkillFilterCreator.ParseContentInfo(array3[j], out array4[j]);
		}
		return array4;
	}

	public static string[] ParenthesisParseContentInfos(string text)
	{
		int num = 0;
		int num2 = 0;
		List<string> list = new List<string>();
		int i;
		for (i = 0; i < text.Length; i++)
		{
			if (num == 0 && text[i] == '&')
			{
				list.Add(text.Substring((num2 == 0) ? num2 : (num2 + 1), i - ((num2 == 0) ? num2 : (num2 + 1))));
				num2 = i;
			}
			if (text[i] == '(')
			{
				num++;
			}
			if (text[i] == ')')
			{
				num--;
			}
		}
		if (text[num2] == '&')
		{
			list.Add(text.Substring(num2 + 1, i - (num2 + 1)));
		}
		else
		{
			list.Add(text.Substring(num2, i));
		}
		return list.ToArray();
	}

	public static CardSkillsBuildInfo CreateBuildInfo(CardParameter cardParameter)
	{
		string[] array = SplitBothSkillText(cardParameter.Skill);
		string[] array2 = SplitBothSkillText(cardParameter.SkillTiming);
		string[] array3 = SplitBothSkillText(cardParameter.SkillCondition);
		string[] array4 = SplitBothSkillText(cardParameter.SkillTarget);
		string[] array5 = SplitBothSkillText(cardParameter.SkillOption);
		string[] array6 = SplitBothSkillText(cardParameter.SkillPreprocess);
		string[] array7 = CardIconControl.SplitAndCompleteIconStr(cardParameter.SkillIcon, array);
		CardSkillsBuildInfo cardSkillsBuildInfo = new CardSkillsBuildInfo();
		cardSkillsBuildInfo.normalSkillBuildInfos = CreateOneCardSkillBuildInfo(array[0], array2[0], array3[0], array4[0], array5[0], array6[0], cardParameter.SkillHandCardFrameEffectType, array7[0], cardParameter.SkillEffectPath, cardParameter.SkillEffectEnginType, cardParameter.SkillSe, cardParameter.SkillEffectTime, cardParameter.SkillMoveType, cardParameter.SkillEffectTargetType);
		if (array.Length >= 2)
		{
			cardSkillsBuildInfo.evolveSkillBuildInfos = CreateOneCardSkillBuildInfo(array[1], array2[1], array3[1], array4[1], array5[1], array6[1], new HandCardFrameEffectType[1], array7[1], cardParameter.EvoSkillEffectPath, cardParameter.EvoSkillEffectEnginType, cardParameter.EvoSkillSe, cardParameter.EvoSkillEffectTime, cardParameter.EvoSkillMoveType, cardParameter.EvoSkillEffectTargetType);
		}
		else
		{
			cardSkillsBuildInfo.evolveSkillBuildInfos = new List<SkillBuildInfo>();
		}
		return cardSkillsBuildInfo;
	}

	public static string[] SplitBothSkillText(string text)
	{
		if (text == null)
		{
			return new string[1] { "none" };
		}
		text = text.Replace(" ", "");
		if (string.IsNullOrEmpty(text))
		{
			return new string[1] { "none" };
		}
		return text.Split(new string[1] { "//" }, StringSplitOptions.RemoveEmptyEntries);
	}

	private static List<SkillBuildInfo> CreateOneCardSkillBuildInfo(string type, string timing, string condition, string target, string option, string preprocess, HandCardFrameEffectType[] frameEffectType, string icon, string[] effectPath, EffectMgr.EngineType[] engineType, string[] sePath, string[] effectTime, EffectMgr.MoveType[] effectMoveType, EffectMgr.TargetType[] effectTargetType)
	{
		string[] array = type.Split(',');
		string[] array2 = timing.Split(',');
		string[] array3 = condition.Split(',');
		string[] array4 = target.Split(',');
		string[] array5 = option.Split(',');
		string[] array6 = preprocess.Split(',');
		string[] array7 = icon.Split(',');
		List<SkillBuildInfo> list = new List<SkillBuildInfo>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(new SkillBuildInfo(GetElementAtString(array, i), GetElementAtString(array2, i), GetElementAtString(array3, i), GetElementAtString(array4, i), GetElementAtString(array5, i), GetElementAtString(array6, i), GetElementAt(frameEffectType, i, HandCardFrameEffectType.NONE), GetElementAtString(array7, i), null, GetElementAtString(effectPath, i), GetElementAt(engineType, i, EffectMgr.EngineType.NONE), GetElementAtString(sePath, i), GetElementAtFloat(effectTime, i), GetElementAt(effectMoveType, i, EffectMgr.MoveType.NONE), GetElementAt(effectTargetType, i, EffectMgr.TargetType.NONE)));
		}
		return list;
	}

	public static float GetElementAtFloat(string[] array, int index)
	{
		float elementAt = GetElementAt(array, index, float.Parse, float.NegativeInfinity);
		if (elementAt < 0f)
		{
			elementAt = GetElementAt(array, 0, float.Parse, 0f);
		}
		return elementAt;
	}

	public static string GetElementAtString(string[] array, int index)
	{
		return GetElementAt(array, index, (string s) => s, "");
	}

	public static T GetElementAt<T>(string[] array, int index, Func<string, T> converter, T defaultValue)
	{
		if (array == null)
		{
			return defaultValue;
		}
		if (array.Length <= index)
		{
			return defaultValue;
		}
		try
		{
			return converter(array[index]);
		}
		catch (Exception)
		{
			return defaultValue;
		}
	}

	public static T GetElementAt<T>(T[] array, int index, T defaultValue)
	{
		if (array == null)
		{
			return defaultValue;
		}
		if (array.Length <= index)
		{
			return defaultValue;
		}
		return array[index];
	}
}
