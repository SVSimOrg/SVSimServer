using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Wizard;
using Wizard.Battle;

public static class SkillFilterCreator
{
	public struct ContentInfo
	{
		public ContentKeyword Name;

		public string Operator;

		public ContentKeyword Value;

		public string ValueStr;

		public string CusomValue;

		public ContentInfo(ContentKeyword name, string op, ContentKeyword value, string valueStr, string customValue = "")
		{
			Name = name;
			Operator = op;
			Value = value;
			ValueStr = valueStr;
			CusomValue = customValue;
		}
	}

	public enum ContentKeyword
	{
		count,
		limit_upper_count,
		tribe,
		summon_moment_tribe,
		play_moment_tribe,
		play_moment_spell_charge,
		update_deck_moment_tribe,
		clan,
		both_clan,
		leader_skin_id,
		evolution,
		ability,
		selectable,
		unique_base_card_id_card,
		base_cost,
		cost,
		last_cost,
		last_used_pp_count,
		base_offense,
		offense,
		is_odd_offense,
		is_even_offense,
		base_life,
		max_life,
		life,
		life_is_larger,
		attack_is_larger,
		is_inplay,
		is_reanimate,
		is_turn,
		display_other_users_message,
		me_language,
		id,
		base_card_id,
		skill_id,
		attack_count,
		add_attack_count,
		max_attack_count,
		destroy,
		strict_destroy,
		attacked,
		previous_turn_attacked,
		has_skill,
		character,
		target,
		card_type,
		chant_count,
		status_cost,
		status_offense,
		status_life,
		berserk,
		avarice,
		wrath,
		awake,
		evolvable_turn,
		resonance,
		pp_count,
		trigger,
		play_count,
		turn_play_other_count,
		game_condition_fulfilled_turn_playcount,
		cemetery_count,
		rally_count,
		deck_banish_count,
		attacker,
		be_attacked,
		other_than_be_attacked,
		in_hand,
		necromance,
		burial_rite,
		fusion,
		turn,
		first_player_turn,
		second_player_turn,
		shortage_deck_win,
		last_necromance_count,
		game_necromance_count,
		game_used_pp_count,
		game_skill_return_card_count,
		turn_skill_return_card_count,
		turn_enhance_card_count,
		game_skill_discard_count,
		turn_skill_discard_count,
		game_buff_count,
		game_skill_metamorphose_count,
		none,
		owner,
		Jatelant_banish_count,
		Jatelant_damage_and_heal_count,
		Jatelant_summon_token_count,
		custom_select,
		random_count,
		random_count_until,
		id_no_duplication_random_count,
		no_duplication_random_count_in_order,
		cost_no_duplication_random_count,
		select_count,
		choice_count,
		exclution,
		in_order_from_oldest,
		random_index,
		use_pp,
		skill_random_count,
		destroy_tribe,
		turn_start_skill,
		turn_start_stop,
		turn_start_skill_after_stop,
		turn_end_skill_after_stop,
		turn_end_stop,
		turn_end_remove,
		self_turn_end_remove,
		remove_from_inplay_stop,
		evolution_end_stop,
		remove_after_action,
		evolution_end_remove,
		unit_attack_stop,
		dont_select_start,
		inplay_period_of_time,
		turn_end_period_of_stop_time,
		turn_start_skill_after_period_of_stop_time,
		turn_end_stop_and_remove,
		damage_after_stop,
		damage_give_stop,
		reflection_after_stop,
		per_turn,
		per_game,
		once_per_action,
		_ref_prev,
		any_condition,
		any_tribe,
		add_damage,
		set_damage,
		add_healing,
		set_healing,
		set_ep,
		cut_amount,
		cut_clipping,
		cut_clipping_range_min,
		cut_clipping_range_max,
		cant_attack,
		cant_attack_all,
		cant_summon,
		life_lower_limit,
		add_count,
		multiply_count,
		gain_count,
		set_chant,
		gain_chant,
		add_chant,
		gain_union_burst_count,
		add_rally_count,
		gain_skybound_art_count,
		gain_super_skybound_art_count,
		set,
		add,
		change,
		damage,
		fixed_damage,
		random_damage,
		turn_damage_value,
		turn_damage_count,
		turn_damage_count_text,
		turn_caused_damage_from_unit,
		turn_heal_value,
		heal_count,
		turn_heal_count,
		turn_heal_count_text,
		turn_amount_value,
		turn_add_pp_count,
		turn_buff_count,
		turn_accelerate_count,
		turn_accelerate_count_text,
		turn_summon_count,
		turn_fusion_count,
		turn_fusion_count_text,
		type,
		add_eptotal,
		add_ep,
		multiply_offense,
		multiply_life,
		add_offense,
		add_life,
		per_target_damage,
		random,
		random_set_range,
		gain_offense,
		gain_life,
		gain_max_life,
		set_offense,
		set_life,
		set_max_life,
		add_max_life,
		fixeduse,
		add_pptotal,
		set_pptotal,
		add_pp,
		gain_pp,
		set_pp,
		decrease_turn_pp,
		add_bp,
		gain_bp,
		random_range,
		loop_range_before_this_skill,
		add_charge,
		diff_charge,
		repeat_count,
		summon_token,
		summon_voice,
		voice_wait_time,
		summon_side,
		player_side,
		token_draw,
		before_transform_token_draw,
		after_transform_token_draw,
		metamorphose,
		card_id,
		healing,
		skill,
		v,
		is_evolve,
		charge_count,
		odd_charge_count,
		even_charge_count,
		is_odd_spell_charge,
		is_even_spell_charge,
		union_burst_count,
		skybound_art_count,
		super_skybound_art_count,
		white_ritual_stack,
		stack_white_ritual,
		unique_base_cost_count,
		unique_tribe_count,
		buff_count,
		buff_life_count,
		fusion_count,
		attaching_ability,
		last_life,
		game_changed_max_life_count,
		game_super_skybound_art_count,
		fixed_generic_value,
		fixed_generic_value_initial,
		random_generic_value,
		skill_activated_count,
		this_turn_skill_activated_count,
		skill_id_activated_count,
		max,
		min,
		half_round_up,
		half_round_down,
		quarter_round_up,
		fifth_round_down,
		sum,
		max_pp,
		pp,
		max_ep,
		ep,
		usable_ep,
		bp,
		me,
		op,
		both,
		main_place,
		inplay,
		deck,
		battle_start_deck,
		hand,
		cemetery,
		inplay_self,
		inplay_other_self,
		newer_inplay_other_self,
		hand_self,
		deck_self,
		hand_other_self,
		hand_other_oldest,
		inplay_self_and_class,
		inplay_and_hand,
		return_card,
		cant_attack_all_return_card,
		last_target,
		discard,
		destroyed_card,
		destroyed_by,
		returned_by,
		destroyed_by_ability,
		destroyed_by_card_id,
		destroyed_by_card_id_and_ability,
		destroyed_card_list,
		lastword_card_list,
		destroyed_this_turn_card_list,
		discard_this_turn_card_list,
		discarded_card,
		healing_card,
		received_damage_card,
		give_damage_card,
		evolution_card,
		fusion_ingrediented_card,
		fusion_ingrediented_card_list,
		through_fusion_ingrediented_card,
		fusion_ingrediented_card_list_include_this_fusion,
		game_fusion_ingrediented_cards,
		game_fusion_ingrediented_and_discard_cards,
		fusion_ingrediented_this_turn_card_list,
		fusion_this_turn_card_list,
		game_fusion_count,
		geton_card,
		getoff_card,
		evolved_card_list,
		game_accelerated_cards,
		game_accelerated_cards_other_self,
		game_crystallized_cards,
		game_skill_activated,
		played_card,
		summoned_card,
		enhance_card,
		accelerated_card,
		crystallized_card,
		didnt_receive_damage_card,
		fight_target,
		selected_cards,
		chosen_cards,
		turn_play_cards,
		turn_draw_cards,
		turn_summon_cards,
		turn_play_cards_other_self,
		game_play_cards_other_self,
		game_play_cards,
		game_play_count,
		burial_rite_card,
		burial_rite_card_list,
		burial_rite_this_turn_card_list,
		last_burial_rite_card_list,
		reanimated_this_turn_card_list,
		banished_card,
		inplay_banished_card,
		hand_banished_card,
		inplay_banished_this_turn_card_list,
		hand_banished_this_turn_card_list,
		inplay_banished_card_list,
		hand_banished_card_list,
		deck_banished_card_list,
		return_this_turn_card_list,
		reanimated_card,
		inplay_last_target,
		destroyed_last_target,
		banished_last_target,
		cant_attack_all_last_target,
		deck_summoned_card,
		past_summon_cards,
		game_summon_cards,
		game_summon_cards_other,
		left_cards,
		game_left_cards,
		left_this_turn_card_list,
		inplay_buffing_cards,
		inplay_debuffing_cards,
		game_deck_draw_cards,
		game_draw_cards,
		game_add_update_deck_cards,
		before_transform_card,
		discard_card_list,
		chant_count_change_cards,
		shortage_deck_win_cards,
		last,
		turn_evolve_count,
		turn_when_healing_count,
		turn_when_healing_count_text,
		self,
		skill_drew_card,
		token_draw_card,
		turn_token_draw_skill_id,
		deck_draw_card,
		drew_over_hand_limit,
		self_skill_drew_card,
		self_skill_return_card,
		skill_update_deck_card,
		unit,
		_class,
		field,
		chant_field,
		not_chant_field,
		spell,
		unit_and_class,
		unit_and_allfield,
		unit_and_chant_field,
		unit_and_not_chant_field,
		spell_and_field,
		play_card_type,
		all,
		attack,
		all_and_self_both_clan,
		_true,
		_false,
		spell_charge,
		sneak,
		guard,
		killer,
		rush,
		quick,
		drain,
		white_ritual,
		destroy_white_ritual,
		when_attack,
		when_destroy,
		when_fight,
		union_burst,
		super_skybound_art,
		when_play,
		when_play_no_select,
		when_play_no_select_and_burial_rite,
		when_play_except_burial_rite,
		enhance,
		when_accelerate,
		when_crystallize,
		reanimate,
		when_evolve,
		self_ability,
		both_ability,
		damaged_card,
		not_damaged_card,
		skill_summoned_card,
		over_cost_from_last_target,
		equal_or_less_cost_from_last_target,
		other,
		duplication,
		repeat_type,
		invoke_type,
		is_allow_destroy_target,
		duplicate_ban_id,
		duplicate_ban,
		load_turn_skill_id,
		save_turn_skill_id,
		damage_count,
		select_index,
		select_cardlist,
		limit_upper_count_from_oldest,
		limit_upper_count_from_newest,
		condition,
		ignore_voice,
		random_voice,
		use_ep,
		open_card,
		save_target,
		load_target,
		save_target_card_id,
		load_target_card_id,
		save_burial_rite_target,
		load_burial_rite_target,
		turn_destroyed_cards,
		or,
		is_open,
		is_random_distinct,
		reset_random_distinct_by,
		except_fusion_card_id,
		except_game_play_card_id,
		include_self,
		is_individual,
		is_activate_summon_card,
		show_battle_log,
		only_random_index,
		shortage_deck_lose,
		game_used_ep_count,
		turn_used_ep_count,
		skill_summoned_card_list,
		skill_drew_card_list,
		distinct_base_card_and_random_index,
		activated_random_array,
		preprocess_condition,
		skill_stop_and_remove_by_when_summon_other,
		option,
		effect,
		is_before_choice_transform,
		inplay_metamorphosed_cards,
		remain_action_count_skill_id,
		remain_action_count_from_self,
		game_resonance_start_count,
		turn_resonance_start_count,
		last_inplay_white_ritual_stack,
		game_used_white_ritual_stack,
		last_used_white_ritual_stack_count,
		skill_healing_value,
		healing_value,
		dont_activate,
		wrapping_and_adding_skill_activated_count,
		skill_activate_count_by_simultaneous_destroyed_card_list,
		skill_activate_count_by_simultaneous_buffing_cards,
		skill_activate_count_by_simultaneous_summoned_card,
		not_unique_base_card_id_card,
		each_same_base_card_id_count_until,
		induction,
		quest_stage_id,
		turn_start_life,
		is_chaos,
		is_unlimited,
		divergence_id,
		is_watch,
		is_fusion_info_or_is_not_sidelog,
		is_buff_detail,
		is_sidelog_by_when_destroy_and_buff_detail,
		is_owner,
		rarity,
		game_quick_attack_cards,
		chara_id,
		invoke_emote,
		invoke_voice,
		ignore_vibration,
		invalid_ability
	}

	public static readonly string[] SET_COUNT_FILTER_NAMES;

	public static readonly string[] COUNT_EXTENSIONS_FILTER_NAMES;

	public static readonly string[] CARD_PARAMETER_FILTER_NAMES;

	public static readonly string[] ENVIRONMENTAL_PARAMETER_FILTER_NAMES;

	public static readonly string[] CALC_FILTER_NAMES;

	public static readonly string[] PLAYER_FILTER_NAMES;

	public static readonly string[] PLAYER_TARGET_FILTER_NAMES;

	public static readonly string[] SINGLE_TARGET_FILTER_NAMES;

	public static readonly string[] CARD_TYPE_FILTER_NAMES;

	public static readonly string[] CARD_CONDITION_FILTER_NAMES;

	public static readonly string[] CARD_PARAMETER_COMPARE_FILTER_NAMES;

	public static readonly string[] CARD_SELECT_FILTER_NAMES;

	private static Dictionary<string, ContentKeyword> _str2Keyword;

	public static readonly string EQUAL;

	public static readonly string NOTEQUAL;

	private static readonly string destroyed_last_target;

	private static readonly string inplay_last_target;

	private static readonly string banished_last_target;

	private static readonly string last_target;

	private static readonly string fixed_generic_value_initial;

	private static readonly string equal_or_less_cost_from_last_target;

	static SkillFilterCreator()
	{
		SET_COUNT_FILTER_NAMES = new string[3]
		{
			ContentKeyword.count.ToStringCustom(),
			ContentKeyword.unique_base_cost_count.ToStringCustom(),
			ContentKeyword.unique_tribe_count.ToStringCustom()
		};
		COUNT_EXTENSIONS_FILTER_NAMES = new string[1] { ContentKeyword.limit_upper_count.ToStringCustom() };
		CARD_PARAMETER_FILTER_NAMES = new string[60]
		{
			ContentKeyword.base_cost.ToStringCustom(),
			ContentKeyword.cost.ToStringCustom(),
			ContentKeyword.last_cost.ToStringCustom(),
			ContentKeyword.last_used_pp_count.ToStringCustom(),
			ContentKeyword.base_offense.ToStringCustom(),
			ContentKeyword.offense.ToStringCustom(),
			ContentKeyword.base_life.ToStringCustom(),
			ContentKeyword.max_life.ToStringCustom(),
			ContentKeyword.life.ToStringCustom(),
			ContentKeyword.charge_count.ToStringCustom(),
			ContentKeyword.odd_charge_count.ToStringCustom(),
			ContentKeyword.even_charge_count.ToStringCustom(),
			ContentKeyword.chant_count.ToStringCustom(),
			ContentKeyword.id.ToStringCustom(),
			ContentKeyword.base_card_id.ToStringCustom(),
			ContentKeyword.attack_count.ToStringCustom(),
			ContentKeyword.max_attack_count.ToStringCustom(),
			ContentKeyword.buff_count.ToStringCustom(),
			ContentKeyword.buff_life_count.ToStringCustom(),
			ContentKeyword.last_life.ToStringCustom(),
			ContentKeyword.game_changed_max_life_count.ToStringCustom(),
			ContentKeyword.fixed_generic_value.ToStringCustom(),
			ContentKeyword.fixed_generic_value_initial.ToStringCustom(),
			ContentKeyword.random_generic_value.ToStringCustom(),
			ContentKeyword.load_target_card_id.ToStringCustom(),
			ContentKeyword.damage_count.ToStringCustom(),
			ContentKeyword.turn_damage_value.ToStringCustom(),
			ContentKeyword.turn_damage_count.ToStringCustom(),
			ContentKeyword.turn_damage_count_text.ToStringCustom(),
			ContentKeyword.turn_caused_damage_from_unit.ToStringCustom(),
			ContentKeyword.turn_heal_value.ToStringCustom(),
			ContentKeyword.heal_count.ToStringCustom(),
			ContentKeyword.turn_heal_count.ToStringCustom(),
			ContentKeyword.turn_heal_count_text.ToStringCustom(),
			ContentKeyword.turn_amount_value.ToStringCustom(),
			ContentKeyword.turn_buff_count.ToStringCustom(),
			ContentKeyword.turn_add_pp_count.ToStringCustom(),
			ContentKeyword.turn_accelerate_count.ToStringCustom(),
			ContentKeyword.turn_accelerate_count_text.ToStringCustom(),
			ContentKeyword.turn_summon_count.ToStringCustom(),
			ContentKeyword.turn_fusion_count.ToStringCustom(),
			ContentKeyword.turn_fusion_count_text.ToStringCustom(),
			ContentKeyword.random_index.ToStringCustom(),
			ContentKeyword.union_burst_count.ToStringCustom(),
			ContentKeyword.skybound_art_count.ToStringCustom(),
			ContentKeyword.super_skybound_art_count.ToStringCustom(),
			ContentKeyword.white_ritual_stack.ToStringCustom(),
			ContentKeyword.remain_action_count_skill_id.ToStringCustom(),
			ContentKeyword.remain_action_count_from_self.ToStringCustom(),
			ContentKeyword.turn_play_other_count.ToStringCustom(),
			ContentKeyword.fusion_count.ToStringCustom(),
			ContentKeyword.turn_start_life.ToStringCustom(),
			ContentKeyword.is_turn.ToStringCustom(),
			ContentKeyword.skill_activated_count.ToStringCustom(),
			ContentKeyword.this_turn_skill_activated_count.ToStringCustom(),
			ContentKeyword.skill_id_activated_count.ToStringCustom(),
			ContentKeyword.distinct_base_card_and_random_index.ToStringCustom(),
			ContentKeyword.activated_random_array.ToStringCustom(),
			ContentKeyword.skill_healing_value.ToStringCustom(),
			ContentKeyword.healing_value.ToStringCustom()
		};
		ENVIRONMENTAL_PARAMETER_FILTER_NAMES = new string[47]
		{
			ContentKeyword.max_pp.ToStringCustom(),
			ContentKeyword.pp.ToStringCustom(),
			ContentKeyword.max_ep.ToStringCustom(),
			ContentKeyword.ep.ToStringCustom(),
			ContentKeyword.usable_ep.ToStringCustom(),
			ContentKeyword.bp.ToStringCustom(),
			ContentKeyword.turn.ToStringCustom(),
			ContentKeyword.first_player_turn.ToStringCustom(),
			ContentKeyword.second_player_turn.ToStringCustom(),
			ContentKeyword.damage.ToStringCustom(),
			ContentKeyword.fixed_damage.ToStringCustom(),
			ContentKeyword.cemetery_count.ToStringCustom(),
			ContentKeyword.last_necromance_count.ToStringCustom(),
			ContentKeyword.game_necromance_count.ToStringCustom(),
			ContentKeyword.game_used_pp_count.ToStringCustom(),
			ContentKeyword.play_count.ToStringCustom(),
			ContentKeyword.game_condition_fulfilled_turn_playcount.ToStringCustom(),
			ContentKeyword.rally_count.ToStringCustom(),
			ContentKeyword.deck_banish_count.ToStringCustom(),
			ContentKeyword.turn_evolve_count.ToStringCustom(),
			ContentKeyword.game_used_ep_count.ToStringCustom(),
			ContentKeyword.turn_used_ep_count.ToStringCustom(),
			ContentKeyword.shortage_deck_lose.ToStringCustom(),
			ContentKeyword.quest_stage_id.ToStringCustom(),
			ContentKeyword.random.ToStringCustom(),
			ContentKeyword.turn_when_healing_count.ToString(),
			ContentKeyword.turn_when_healing_count_text.ToString(),
			ContentKeyword.game_super_skybound_art_count.ToString(),
			ContentKeyword.game_resonance_start_count.ToString(),
			ContentKeyword.turn_resonance_start_count.ToString(),
			ContentKeyword.last_inplay_white_ritual_stack.ToString(),
			ContentKeyword.game_used_white_ritual_stack.ToString(),
			ContentKeyword.last_used_white_ritual_stack_count.ToString(),
			ContentKeyword.load_turn_skill_id.ToString(),
			ContentKeyword.game_skill_return_card_count.ToStringCustom(),
			ContentKeyword.turn_skill_return_card_count.ToStringCustom(),
			ContentKeyword.turn_enhance_card_count.ToStringCustom(),
			ContentKeyword.game_skill_discard_count.ToStringCustom(),
			ContentKeyword.turn_skill_discard_count.ToStringCustom(),
			ContentKeyword.game_buff_count.ToStringCustom(),
			ContentKeyword.game_skill_metamorphose_count.ToStringCustom(),
			ContentKeyword.chara_id.ToString(),
			ContentKeyword.game_play_count.ToString(),
			ContentKeyword.game_fusion_count.ToString(),
			ContentKeyword.Jatelant_banish_count.ToStringCustom(),
			ContentKeyword.Jatelant_damage_and_heal_count.ToStringCustom(),
			ContentKeyword.Jatelant_summon_token_count.ToStringCustom()
		};
		CALC_FILTER_NAMES = new string[7]
		{
			ContentKeyword.max.ToStringCustom(),
			ContentKeyword.min.ToStringCustom(),
			ContentKeyword.half_round_up.ToStringCustom(),
			ContentKeyword.half_round_down.ToStringCustom(),
			ContentKeyword.quarter_round_up.ToStringCustom(),
			ContentKeyword.fifth_round_down.ToStringCustom(),
			ContentKeyword.sum.ToStringCustom()
		};
		PLAYER_FILTER_NAMES = new string[3]
		{
			ContentKeyword.me.ToStringCustom(),
			ContentKeyword.op.ToStringCustom(),
			ContentKeyword.both.ToStringCustom()
		};
		PLAYER_TARGET_FILTER_NAMES = new string[106]
		{
			ContentKeyword.inplay.ToStringCustom(),
			ContentKeyword.deck.ToStringCustom(),
			ContentKeyword.battle_start_deck.ToStringCustom(),
			ContentKeyword.hand.ToStringCustom(),
			ContentKeyword.cemetery.ToStringCustom(),
			ContentKeyword.self.ToStringCustom(),
			ContentKeyword.inplay_self.ToStringCustom(),
			ContentKeyword.inplay_other_self.ToStringCustom(),
			ContentKeyword.newer_inplay_other_self.ToStringCustom(),
			ContentKeyword.hand_self.ToStringCustom(),
			ContentKeyword.deck_self.ToStringCustom(),
			ContentKeyword.hand_other_self.ToStringCustom(),
			ContentKeyword.hand_other_oldest.ToStringCustom(),
			ContentKeyword.inplay_self_and_class.ToStringCustom(),
			ContentKeyword.inplay_and_hand.ToStringCustom(),
			ContentKeyword.return_card.ToStringCustom(),
			ContentKeyword.cant_attack_all_return_card.ToStringCustom(),
			ContentKeyword.last_target.ToStringCustom(),
			ContentKeyword.inplay_last_target.ToStringCustom(),
			ContentKeyword.destroyed_last_target.ToStringCustom(),
			ContentKeyword.banished_last_target.ToStringCustom(),
			ContentKeyword.cant_attack_all_last_target.ToStringCustom(),
			ContentKeyword.in_hand.ToStringCustom(),
			ContentKeyword.discard.ToStringCustom(),
			ContentKeyword.burial_rite_card.ToStringCustom(),
			ContentKeyword.burial_rite_card_list.ToStringCustom(),
			ContentKeyword.burial_rite_this_turn_card_list.ToStringCustom(),
			ContentKeyword.reanimated_this_turn_card_list.ToStringCustom(),
			ContentKeyword.banished_card.ToStringCustom(),
			ContentKeyword.destroyed_card.ToStringCustom(),
			ContentKeyword.destroyed_card_list.ToStringCustom(),
			ContentKeyword.turn_destroyed_cards.ToStringCustom(),
			ContentKeyword.lastword_card_list.ToStringCustom(),
			ContentKeyword.destroyed_this_turn_card_list.ToStringCustom(),
			ContentKeyword.discard_this_turn_card_list.ToStringCustom(),
			ContentKeyword.discarded_card.ToStringCustom(),
			ContentKeyword.damaged_card.ToStringCustom(),
			ContentKeyword.not_damaged_card.ToStringCustom(),
			ContentKeyword.healing_card.ToStringCustom(),
			ContentKeyword.received_damage_card.ToStringCustom(),
			ContentKeyword.give_damage_card.ToStringCustom(),
			ContentKeyword.didnt_receive_damage_card.ToStringCustom(),
			ContentKeyword.evolution_card.ToStringCustom(),
			ContentKeyword.evolved_card_list.ToStringCustom(),
			ContentKeyword.played_card.ToStringCustom(),
			ContentKeyword.enhance_card.ToStringCustom(),
			ContentKeyword.accelerated_card.ToStringCustom(),
			ContentKeyword.crystallized_card.ToStringCustom(),
			ContentKeyword.attacker.ToStringCustom(),
			ContentKeyword.be_attacked.ToStringCustom(),
			ContentKeyword.other_than_be_attacked.ToStringCustom(),
			ContentKeyword.summoned_card.ToStringCustom(),
			ContentKeyword.skill_summoned_card.ToStringCustom(),
			ContentKeyword.reanimated_card.ToString(),
			ContentKeyword.fight_target.ToStringCustom(),
			ContentKeyword.turn_play_cards.ToStringCustom(),
			ContentKeyword.turn_draw_cards.ToStringCustom(),
			ContentKeyword.turn_token_draw_skill_id.ToStringCustom(),
			ContentKeyword.turn_summon_cards.ToStringCustom(),
			ContentKeyword.turn_play_cards_other_self.ToStringCustom(),
			ContentKeyword.past_summon_cards.ToStringCustom(),
			ContentKeyword.game_play_cards_other_self.ToStringCustom(),
			ContentKeyword.game_summon_cards.ToStringCustom(),
			ContentKeyword.game_summon_cards_other.ToStringCustom(),
			ContentKeyword.game_play_cards.ToStringCustom(),
			ContentKeyword.selected_cards.ToStringCustom(),
			ContentKeyword.token_draw_card.ToStringCustom(),
			ContentKeyword.load_target.ToStringCustom(),
			ContentKeyword.load_burial_rite_target.ToStringCustom(),
			ContentKeyword.fusion_ingrediented_card.ToStringCustom(),
			ContentKeyword.through_fusion_ingrediented_card.ToStringCustom(),
			ContentKeyword.fusion_ingrediented_card_list_include_this_fusion.ToStringCustom(),
			ContentKeyword.game_fusion_ingrediented_cards.ToStringCustom(),
			ContentKeyword.game_fusion_ingrediented_and_discard_cards.ToStringCustom(),
			ContentKeyword.fusion_ingrediented_this_turn_card_list.ToStringCustom(),
			ContentKeyword.fusion_this_turn_card_list.ToStringCustom(),
			ContentKeyword.main_place.ToStringCustom(),
			ContentKeyword.inplay_metamorphosed_cards.ToStringCustom(),
			ContentKeyword.game_accelerated_cards.ToStringCustom(),
			ContentKeyword.game_accelerated_cards_other_self.ToStringCustom(),
			ContentKeyword.game_crystallized_cards.ToStringCustom(),
			ContentKeyword.game_skill_activated.ToStringCustom(),
			ContentKeyword.deck_draw_card.ToStringCustom(),
			ContentKeyword.drew_over_hand_limit.ToStringCustom(),
			ContentKeyword.left_cards.ToStringCustom(),
			ContentKeyword.attaching_ability.ToStringCustom(),
			ContentKeyword.inplay_buffing_cards.ToStringCustom(),
			ContentKeyword.inplay_debuffing_cards.ToStringCustom(),
			ContentKeyword.game_deck_draw_cards.ToStringCustom(),
			ContentKeyword.game_draw_cards.ToStringCustom(),
			ContentKeyword.game_add_update_deck_cards.ToStringCustom(),
			ContentKeyword.game_left_cards.ToStringCustom(),
			ContentKeyword.left_this_turn_card_list.ToStringCustom(),
			ContentKeyword.before_transform_card.ToStringCustom(),
			ContentKeyword.inplay_banished_card.ToStringCustom(),
			ContentKeyword.hand_banished_card.ToStringCustom(),
			ContentKeyword.inplay_banished_this_turn_card_list.ToStringCustom(),
			ContentKeyword.hand_banished_this_turn_card_list.ToStringCustom(),
			ContentKeyword.inplay_banished_card_list.ToStringCustom(),
			ContentKeyword.hand_banished_card_list.ToStringCustom(),
			ContentKeyword.deck_banished_card_list.ToStringCustom(),
			ContentKeyword.game_quick_attack_cards.ToStringCustom(),
			ContentKeyword.discard_card_list.ToStringCustom(),
			ContentKeyword.chant_count_change_cards.ToStringCustom(),
			ContentKeyword.shortage_deck_win_cards.ToStringCustom(),
			ContentKeyword.return_this_turn_card_list.ToStringCustom()
		};
		SINGLE_TARGET_FILTER_NAMES = new string[4]
		{
			ContentKeyword.self.ToStringCustom(),
			ContentKeyword.skill_drew_card.ToStringCustom(),
			ContentKeyword.skill_update_deck_card.ToStringCustom(),
			ContentKeyword.unique_base_card_id_card.ToStringCustom()
		};
		CARD_TYPE_FILTER_NAMES = new string[14]
		{
			ContentKeyword.unit.ToStringCustom(),
			ContentKeyword._class.ToStringCustom(),
			ContentKeyword.field.ToStringCustom(),
			ContentKeyword.chant_field.ToStringCustom(),
			ContentKeyword.not_chant_field.ToStringCustom(),
			ContentKeyword.spell.ToStringCustom(),
			ContentKeyword.unit_and_class.ToStringCustom(),
			ContentKeyword.unit_and_allfield.ToStringCustom(),
			ContentKeyword.unit_and_chant_field.ToStringCustom(),
			ContentKeyword.unit_and_not_chant_field.ToStringCustom(),
			ContentKeyword.spell_and_field.ToStringCustom(),
			ContentKeyword.all.ToStringCustom(),
			ContentKeyword.last.ToStringCustom(),
			ContentKeyword.play_card_type.ToStringCustom()
		};
		CARD_CONDITION_FILTER_NAMES = new string[33]
		{
			ContentKeyword.tribe.ToStringCustom(),
			ContentKeyword.summon_moment_tribe.ToStringCustom(),
			ContentKeyword.play_moment_tribe.ToStringCustom(),
			ContentKeyword.play_moment_spell_charge.ToStringCustom(),
			ContentKeyword.update_deck_moment_tribe.ToStringCustom(),
			ContentKeyword.ability.ToStringCustom(),
			ContentKeyword.selectable.ToStringCustom(),
			ContentKeyword.evolution.ToStringCustom(),
			ContentKeyword.clan.ToStringCustom(),
			ContentKeyword.both_clan.ToStringCustom(),
			ContentKeyword.leader_skin_id.ToStringCustom(),
			ContentKeyword.unique_base_card_id_card.ToStringCustom(),
			ContentKeyword.include_self.ToStringCustom(),
			ContentKeyword.life_is_larger.ToStringCustom(),
			ContentKeyword.attack_is_larger.ToStringCustom(),
			ContentKeyword.is_inplay.ToStringCustom(),
			ContentKeyword.is_reanimate.ToStringCustom(),
			ContentKeyword.fusion_ingrediented_card_list.ToStringCustom(),
			ContentKeyword.geton_card.ToStringCustom(),
			ContentKeyword.getoff_card.ToStringCustom(),
			ContentKeyword.avarice.ToStringCustom(),
			ContentKeyword.is_watch.ToStringCustom(),
			ContentKeyword.is_fusion_info_or_is_not_sidelog.ToStringCustom(),
			ContentKeyword.is_buff_detail.ToStringCustom(),
			ContentKeyword.is_sidelog_by_when_destroy_and_buff_detail.ToStringCustom(),
			ContentKeyword.is_owner.ToStringCustom(),
			ContentKeyword.self_skill_drew_card.ToStringCustom(),
			ContentKeyword.self_skill_return_card.ToStringCustom(),
			ContentKeyword.skill_summoned_card_list.ToStringCustom(),
			ContentKeyword.skill_drew_card_list.ToStringCustom(),
			ContentKeyword.not_unique_base_card_id_card.ToStringCustom(),
			ContentKeyword.last_burial_rite_card_list.ToStringCustom(),
			ContentKeyword.attacked.ToStringCustom()
		};
		CARD_PARAMETER_COMPARE_FILTER_NAMES = new string[29]
		{
			ContentKeyword.base_cost.ToStringCustom(),
			ContentKeyword.cost.ToStringCustom(),
			ContentKeyword.base_offense.ToStringCustom(),
			ContentKeyword.offense.ToStringCustom(),
			ContentKeyword.base_life.ToStringCustom(),
			ContentKeyword.max_life.ToStringCustom(),
			ContentKeyword.life.ToStringCustom(),
			ContentKeyword.chant_count.ToStringCustom(),
			ContentKeyword.charge_count.ToStringCustom(),
			ContentKeyword.id.ToStringCustom(),
			ContentKeyword.base_card_id.ToStringCustom(),
			ContentKeyword.attack_count.ToStringCustom(),
			ContentKeyword.max_attack_count.ToStringCustom(),
			ContentKeyword.destroy.ToStringCustom(),
			ContentKeyword.strict_destroy.ToStringCustom(),
			ContentKeyword.buff_count.ToStringCustom(),
			ContentKeyword.buff_life_count.ToStringCustom(),
			ContentKeyword.previous_turn_attacked.ToStringCustom(),
			ContentKeyword.last_life.ToStringCustom(),
			ContentKeyword.game_changed_max_life_count.ToStringCustom(),
			ContentKeyword.has_skill.ToStringCustom(),
			ContentKeyword.destroyed_by.ToStringCustom(),
			ContentKeyword.returned_by.ToStringCustom(),
			ContentKeyword.destroyed_by_ability.ToStringCustom(),
			ContentKeyword.destroyed_by_card_id.ToStringCustom(),
			ContentKeyword.destroyed_by_card_id_and_ability.ToStringCustom(),
			ContentKeyword.is_chaos.ToStringCustom(),
			ContentKeyword.is_unlimited.ToStringCustom(),
			ContentKeyword.rarity.ToStringCustom()
		};
		CARD_SELECT_FILTER_NAMES = new string[2]
		{
			ContentKeyword.select_index.ToStringCustom(),
			ContentKeyword.select_cardlist.ToString()
		};
		_str2Keyword = null;
		EQUAL = "=";
		NOTEQUAL = "!=";
		destroyed_last_target = ContentKeyword.destroyed_last_target.ToStringCustom();
		inplay_last_target = ContentKeyword.inplay_last_target.ToStringCustom();
		banished_last_target = ContentKeyword.banished_last_target.ToStringCustom();
		last_target = ContentKeyword.last_target.ToStringCustom();
		fixed_generic_value_initial = ContentKeyword.fixed_generic_value_initial.ToStringCustom();
		equal_or_less_cost_from_last_target = ContentKeyword.equal_or_less_cost_from_last_target.ToStringCustom();
		SetupStatic();
	}

	public static void SetupStatic()
	{
		if (_str2Keyword != null)
		{
			return;
		}
		_str2Keyword = new Dictionary<string, ContentKeyword>(Enum.GetValues(typeof(ContentKeyword)).Length);
		foreach (ContentKeyword value in Enum.GetValues(typeof(ContentKeyword)))
		{
			_str2Keyword.Add(value.ToStringCustom(), value);
		}
	}

	public static ContentKeyword Str2ContentKeyword(string str)
	{
		if (!_str2Keyword.TryGetValue(str, out var value))
		{
			return ContentKeyword.none;
		}
		return value;
	}

	public static void SetupCondition(ConditionSkillFilterCollection filterCollection, string text, BattleCardBase ownerCard, SkillBase skill)
	{
		if (text.First() == '{')
		{
			if (text.Contains('|') && text.Last() == '}')
			{
				List<string> filtersString = SkillAnyConditionFilter.GetFiltersString(text);
				if (filtersString.Count >= 2)
				{
					filterCollection.AnyConditionFilter.Add(new SkillAnyConditionFilter(text, filtersString, ownerCard, skill));
					return;
				}
			}
			filterCollection.VariableCompareFilter.Add(new SkillVariableComareFilter(text));
			return;
		}
		string[] partTexts = text.Split('.');
		List<string> list = SetupCommon(filterCollection, partTexts, ownerCard, skill);
		if (list == null)
		{
			return;
		}
		foreach (string item in list)
		{
			_ = item;
		}
	}

	public static void SetupTarget(ApplySkillTargetFilterCollection filterCollection, string text, IReadOnlyBattleCardInfo ownerCard, SkillBase skill)
	{
		if (text.First() == '{')
		{
			text = text.Remove(0, 1);
			text = text.Remove(text.Length - 1, 1);
		}
		List<string> list = new List<string>();
		string text2 = string.Empty;
		int num = 0;
		while (text.Length > 0)
		{
			switch (text[0])
			{
			case '.':
				if (num == 0)
				{
					list.Add(text2);
					text2 = string.Empty;
				}
				else
				{
					text2 += text[0];
				}
				break;
			case '{':
				num++;
				text2 += text[0];
				break;
			case '}':
				num--;
				text2 += text[0];
				break;
			default:
				text2 += text[0];
				break;
			}
			text = text.Substring(1, text.Length - 1);
		}
		if (text2 != string.Empty)
		{
			list.Add(text2);
		}
		ApplySkillTargetFilterCollection applySkillTargetFilterCollection = new ApplySkillTargetFilterCollection();
		List<string> list2 = SetupCommon(applySkillTargetFilterCollection, list, ownerCard, skill);
		if (applySkillTargetFilterCollection.ApplySelectFilter == null)
		{
			applySkillTargetFilterCollection.ApplySelectFilter = new SkillSelectAllFilter();
		}
		if (list2 != null)
		{
			foreach (string item in list2)
			{
				_ = item;
			}
			return;
		}
		filterCollection.ApplyAndFilter.Add(applySkillTargetFilterCollection);
	}

	public static void SetupVariable(VariableSkillFilterCollection filterCollection, string text, IReadOnlyBattleCardInfo ownerCard, SkillBase skill)
	{
		IEnumerable<string> partTexts = text.Split('.');
		List<string> list = SetupCommon(filterCollection, partTexts, ownerCard, skill);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			string text2 = list[i];
			ParseContentInfo(text2, out var retParsedInfo);
			if (SET_COUNT_FILTER_NAMES.Contains(text2))
			{
				filterCollection.CardCountFilter = CreateSetCountFilter(retParsedInfo.Name);
				continue;
			}
			SkillCardLimitUpperCountFilter skillCardLimitUpperCountFilter = ownerCard.SelfBattlePlayer.BattleMgr.CheackCalledCreateFilterDictionary(ownerCard, text2, retParsedInfo.ValueStr);
			if (skillCardLimitUpperCountFilter != null)
			{
				filterCollection.CardCountExtensionsFilter = skillCardLimitUpperCountFilter;
				continue;
			}
			if (ENVIRONMENTAL_PARAMETER_FILTER_NAMES.Contains(retParsedInfo.Name.ToStringCustom()))
			{
				filterCollection.EnvCountFilter = CreateEnvironmentalParameterFilter(retParsedInfo.Name, retParsedInfo.ValueStr, retParsedInfo.Operator, skill);
				continue;
			}
			if (CARD_PARAMETER_FILTER_NAMES.Contains(retParsedInfo.Name.ToStringCustom()))
			{
				filterCollection.ParameterSelectFilter = CreateParameterSelectFilter(retParsedInfo.Name, retParsedInfo, skill, ownerCard);
				continue;
			}
			if (CALC_FILTER_NAMES.Contains(text2))
			{
				filterCollection.CalcFilter = CreateCalcSelectFilter(retParsedInfo.Name);
				continue;
			}
			SkillOrFilter skillOrFilter = ownerCard.SelfBattlePlayer.BattleMgr.CheackCalledCreateOrFilterDictionary(ownerCard, text2, retParsedInfo.ValueStr);
			if (skillOrFilter != null)
			{
				filterCollection.OrFilter = skillOrFilter;
			}
			else
			{
				text2.Contains(ContentKeyword.turn_destroyed_cards.ToStringCustom());
			}
		}
	}

	public static ISkillParameterSelectFilter CreateParameterSelectFilter(ContentKeyword keyword, ContentInfo info, SkillBase skill, IReadOnlyBattleCardInfo ownerCard = null)
	{
		return keyword switch
		{
			ContentKeyword.base_cost => new SkillParameterSelectBaseCostFilter(), 
			ContentKeyword.cost => new SkillParameterSelectCostFilter(), 
			ContentKeyword.last_cost => new SkillParameterSelectLastCostFilter(), 
			ContentKeyword.last_used_pp_count => new SkillParameterSelectLastUsedPpCountFilter(), 
			ContentKeyword.base_offense => new SkillParameterSelectBaseOffenseFilter(), 
			ContentKeyword.offense => new SkillParameterSelectOffenseFilter(), 
			ContentKeyword.base_life => new SkillParameterSelectBaseLifeFilter(), 
			ContentKeyword.max_life => new SkillParameterSelectMaxLifeFilter(), 
			ContentKeyword.life => new SkillParameterSelectLifeFilter(), 
			ContentKeyword.charge_count => new SkillParameterSelectChargeCountFilter(), 
			ContentKeyword.odd_charge_count => new SkillParameterSelectOddChargeCountFilter(), 
			ContentKeyword.even_charge_count => new SkillParameterSelectEvenChargeCountFilter(), 
			ContentKeyword.buff_count => new SkillParameterSelectBuffCountFilter(), 
			ContentKeyword.buff_life_count => new SkillParameterSelectBuffCountFilter(), 
			ContentKeyword.chant_count => new SkillParameterSelectChantCountFilter(), 
			ContentKeyword.id => new SkillParameterSelectIdFilter(), 
			ContentKeyword.base_card_id => new SkillParameterSelectBaseCardIdFilter(), 
			ContentKeyword.attack_count => new SkillParameterSelectAttackCountFilter(), 
			ContentKeyword.max_attack_count => new SkillParameterSelectMaxAttackCountFilter(), 
			ContentKeyword.last_life => new SkillParameterSelectLastLifeFilter(), 
			ContentKeyword.random_generic_value => new SkillParameterSelectRandomGenericValueFilter(info.ValueStr, skill), 
			ContentKeyword.fixed_generic_value => new SkillParameterSelectFixedGenericValueFilter(info.ValueStr, skill), 
			ContentKeyword.fixed_generic_value_initial => new SkillParameterSelectFixedGenericValueInitialFilter(info.ValueStr, info.CusomValue, skill), 
			ContentKeyword.load_target_card_id => new SkillParameterLoadTargetCardId(info.ValueStr, skill), 
			ContentKeyword.damage_count => new SkillParameterSelectDamageCount(info.ValueStr), 
			ContentKeyword.turn_damage_value => new SkillParameterTurnDamageValueFilter(info.ValueStr), 
			ContentKeyword.turn_damage_count => new SkillParameterTurnDamageCountFilter(info.ValueStr), 
			ContentKeyword.turn_damage_count_text => new SkillParameterTurnDamageCountTextFilter(info.ValueStr), 
			ContentKeyword.turn_caused_damage_from_unit => new SkillParameterTurnCausedDamageFromUnitFilter(info.ValueStr), 
			ContentKeyword.turn_heal_value => new SkillParameterTurnHealValueFilter(info.ValueStr), 
			ContentKeyword.heal_count => new SkillParameterHealCountFilter(), 
			ContentKeyword.turn_heal_count => new SkillParameterTurnHealCountFilter(info.ValueStr), 
			ContentKeyword.turn_heal_count_text => new SkillParameterTurnHealCountTextFilter(info.ValueStr), 
			ContentKeyword.turn_buff_count => new SkillParameterTurnBuffCountFilter(info.ValueStr), 
			ContentKeyword.turn_amount_value => new SkillParameterTurnAmountValueFilter(info.ValueStr), 
			ContentKeyword.turn_add_pp_count => new SkillParameterTurnPpAddCountFilter(info.ValueStr), 
			ContentKeyword.turn_accelerate_count => new SkillParameterTurnAcceleratedCardCountFilter(info.ValueStr), 
			ContentKeyword.turn_accelerate_count_text => new SkillParameterTurnAcceleratedCardCountTextFilter(info.ValueStr), 
			ContentKeyword.turn_summon_count => new SkillParameterTurnSummonCountFilter(), 
			ContentKeyword.turn_fusion_count => new SkillParameterTurnFusionCountFilter(info.ValueStr), 
			ContentKeyword.turn_fusion_count_text => new SkillParameterTurnFusionCountTextFilter(info.ValueStr), 
			ContentKeyword.random_index => new SkillParameterRandomArrayFilter(info.ValueStr), 
			ContentKeyword.union_burst_count => new SkillParameterSelectUnionBurstCountFilter(), 
			ContentKeyword.skybound_art_count => new SkillParameterSelectSkyboundArtCountFilter(), 
			ContentKeyword.super_skybound_art_count => new SkillParameterSelectSuperSkyboundArtCountFilter(), 
			ContentKeyword.white_ritual_stack => new SkillParameterSelectWhiteRitualCountFilter(), 
			ContentKeyword.remain_action_count_skill_id => new SkillParameterSelectRemainActionCountFilter(info.ValueStr, skill), 
			ContentKeyword.remain_action_count_from_self => new SkillParameterSelectRemainActionCountFromSelfFilter(ownerCard), 
			ContentKeyword.turn_play_other_count => new SkillParameterTurnPlayOtherCountFilter(), 
			ContentKeyword.fusion_count => new SkillParameterFusionCountFilter(), 
			ContentKeyword.turn_start_life => new SkillParameterTurnStartLife(info.ValueStr), 
			ContentKeyword.game_changed_max_life_count => new SkillParameterSelectChangeMaxLifeCountFilter(), 
			ContentKeyword.is_turn => new SkillParameterIsTurnFilter(info.ValueStr), 
			ContentKeyword.skill_activated_count => new SkillParameterSkillActivatedCountFilter(), 
			ContentKeyword.this_turn_skill_activated_count => new SkillParameterThisTurnSkillActivatedCountFilter(), 
			ContentKeyword.skill_id_activated_count => new SkillParameterSkillIdActivatedCountFilter(long.Parse(info.ValueStr)), 
			ContentKeyword.distinct_base_card_and_random_index => new SkillParameterDistinctRandomSelectedFilter(info.ValueStr), 
			ContentKeyword.activated_random_array => new SkillParameterActivatedRandomArrayFilter(int.Parse(info.ValueStr)), 
			ContentKeyword.skill_healing_value => new SkillParameterSkillHealValueFilter(), 
			ContentKeyword.healing_value => new SkillParameterHealValueFilter(), 
			_ => null, 
		};
	}

	private static ISkillCalcFilter CreateCalcSelectFilter(ContentKeyword keyword)
	{
		return keyword switch
		{
			ContentKeyword.max => new SkillCalcMaxFilter(), 
			ContentKeyword.min => new SkillCalcMinFilter(), 
			ContentKeyword.half_round_up => new SkillCalcHalfRoundUp(), 
			ContentKeyword.half_round_down => new SkillCalcHalfRoundDown(), 
			ContentKeyword.quarter_round_up => new SkillCalcQuarterRoundUp(), 
			ContentKeyword.fifth_round_down => new SkillCalcFifthRoundDown(), 
			ContentKeyword.sum => new SkillCalcSumFilter(), 
			_ => null, 
		};
	}

	private static SkillCardCountFilter CreateSetCountFilter(ContentKeyword keyword)
	{
		return keyword switch
		{
			ContentKeyword.count => new SkillCardCountFilter(), 
			ContentKeyword.unique_base_cost_count => new SkillUniqueBaseCostCountFilter(), 
			ContentKeyword.unique_tribe_count => new SkillUniqueTribeCountFilter(), 
			_ => null, 
		};
	}

	private static ISkillEnvironmentalFilter CreateEnvironmentalParameterFilter(ContentKeyword keyword, string value, string op, SkillBase skill)
	{
		return keyword switch
		{
			ContentKeyword.max_pp => new SkillEnvironmentalMaxPPFilter(), 
			ContentKeyword.pp => new SkillEnvironmentalPPFilter(), 
			ContentKeyword.max_ep => new SkillEnvironmentalMaxEPFilter(), 
			ContentKeyword.ep => new SkillEnvironmentalEPFilter(), 
			ContentKeyword.usable_ep => new SkillEnvironmentalUsableEPFilter(), 
			ContentKeyword.bp => new SkillEnvironmentalBpFilter(), 
			ContentKeyword.turn => new SkillEnvironmentalTurnFilter(), 
			ContentKeyword.first_player_turn => new SkillEnvironmentalFirstPlayerTurnFilter(), 
			ContentKeyword.second_player_turn => new SkillEnvironmentalSecondPlayerTurnFilter(), 
			ContentKeyword.damage => new SkillEnvironmentalDefaultDamageFilter(), 
			ContentKeyword.fixed_damage => new SkillEnvironmentalFixedDamageFilter(), 
			ContentKeyword.cemetery_count => new SkillEnvironmentalCemeteryCount(), 
			ContentKeyword.last_necromance_count => new SkillEnvironmentalLastNecromanceCount(), 
			ContentKeyword.game_necromance_count => new SkillEnvironmentalGameNecromanceCount(), 
			ContentKeyword.game_used_pp_count => new SkillEnvironmentalGameUsedPpCount(), 
			ContentKeyword.play_count => new SkillEnvironmentalPlayCount(value), 
			ContentKeyword.game_condition_fulfilled_turn_playcount => new SkillEnvironmentalGameConditionFulfilledTurnPlaycount(value, op), 
			ContentKeyword.rally_count => new SkillEnvironmentalRallyCount(), 
			ContentKeyword.deck_banish_count => new SkillEnvironmentalDeckBanishCount(), 
			ContentKeyword.turn_evolve_count => new SkillEnvironmentalTurnEvolveCount(value), 
			ContentKeyword.shortage_deck_lose => new SkillEnvironmentalShortageDeckLose(), 
			ContentKeyword.game_used_ep_count => new SkillEnvironmentalGameUsedEpCount(), 
			ContentKeyword.turn_used_ep_count => new SkillEnvironmentalTurnUsedEpCount(), 
			ContentKeyword.quest_stage_id => new QuestStageIdFilter(), 
			ContentKeyword.random => new RandomValueFilter(value), 
			ContentKeyword.turn_when_healing_count => new SkillEnvironmentalTurnWhenHealingCount(value, isTextKeyword: false), 
			ContentKeyword.turn_when_healing_count_text => new SkillEnvironmentalTurnWhenHealingCount(value, isTextKeyword: true), 
			ContentKeyword.game_super_skybound_art_count => new SkillEnvironmentalGameSuperSkyboundArtCount(), 
			ContentKeyword.game_resonance_start_count => new SkillEnvironmentalGameResonanceStartCountFilter(), 
			ContentKeyword.turn_resonance_start_count => new SkillEnvironmentalTurnResonanceStartCountFilter(), 
			ContentKeyword.last_inplay_white_ritual_stack => new SkillEnvironmentalLastInplayWhiteRitualStackFilter(), 
			ContentKeyword.game_used_white_ritual_stack => new SkillEnvironmentalGameUsedWhiteRitualCountFilter(), 
			ContentKeyword.last_used_white_ritual_stack_count => new SkillEnvironmentalLastUsedWhiteRitualStackCountFilter(), 
			ContentKeyword.load_turn_skill_id => new SkillEnvironmentalAttachedTurnFilter(value, skill), 
			ContentKeyword.game_skill_return_card_count => new SkillEnvironmentalGameReturnSkillCount(), 
			ContentKeyword.turn_skill_return_card_count => new SkillEnvironmentalTurnReturnSkillCount(value), 
			ContentKeyword.turn_enhance_card_count => new SkillEnvironmentalTurnEnhanceCardCount(value), 
			ContentKeyword.game_skill_discard_count => new SkillEnvironmentalGameDiscardSkillCount(), 
			ContentKeyword.turn_skill_discard_count => new SkillEnvironmentalTurnDiscardSkillCount(value), 
			ContentKeyword.game_buff_count => new SkillEnvironmentalGameBuffCount(), 
			ContentKeyword.game_skill_metamorphose_count => new SkillEnvironmentalGameMetamorphoseSkillCount(), 
			ContentKeyword.chara_id => new SkillEnvironmentalCharaIdFilter(), 
			ContentKeyword.game_play_count => new SkillEnvironmentalGamePlayCountFilter(), 
			ContentKeyword.game_fusion_count => new SkillEnvironmentalGameFusionCountFilter(), 
			ContentKeyword.Jatelant_banish_count => new SkillEnvironmentalJatelantBanishCountFilter(), 
			ContentKeyword.Jatelant_damage_and_heal_count => new SkillEnvironmentalJatelantDamageAndHealCountFilter(), 
			ContentKeyword.Jatelant_summon_token_count => new SkillEnvironmentalJatelantSummonTokenCountFilter(), 
			_ => null, 
		};
	}

	public static List<string> SetupCommon(SkillFilterCollectionBase filterCollection, IEnumerable<string> partTexts, IReadOnlyBattleCardInfo ownerCard, SkillBase skill)
	{
		ownerCard.SelfBattlePlayer.BattleMgr.RemoveUnUseCalledFilterDictionary();
		List<string> list = null;
		foreach (string partText in partTexts)
		{
			ParseContentInfo(partText, out var retParsedInfo);
			string text = retParsedInfo.Name.ToStringCustom();
			if (PLAYER_FILTER_NAMES.Contains(text))
			{
				filterCollection.BattlePlayerFilter = CreateBattlePlayerFilter(retParsedInfo.Name);
				continue;
			}
			if (filterCollection.BattlePlayerFilter == null)
			{
				if (SINGLE_TARGET_FILTER_NAMES.Contains(text))
				{
					filterCollection.TargetFilter = CreateSingleTargetFilter(retParsedInfo.Name, ownerCard);
					continue;
				}
			}
			else if (PLAYER_TARGET_FILTER_NAMES.Contains(text))
			{
				filterCollection.TargetFilter = CreatePlayerTargetFilter(retParsedInfo, retParsedInfo.Name, ownerCard, skill);
				continue;
			}
			if (CARD_TYPE_FILTER_NAMES.Contains(text))
			{
				filterCollection.CardFilterList.Add(CreateCardTypeFilter(retParsedInfo.Name, ownerCard, retParsedInfo.ValueStr));
				continue;
			}
			if (CARD_CONDITION_FILTER_NAMES.Contains(text))
			{
				filterCollection.CardFilterList.Add(CreateCardConditionFilter(partText, ownerCard));
				continue;
			}
			if (CARD_SELECT_FILTER_NAMES.Contains(text))
			{
				filterCollection.SelectFilter = CreateCardSelectFilter(partText);
				continue;
			}
			ISkillCardFilter skillCardFilter = ownerCard.SelfBattlePlayer.BattleMgr.CheackCalledCreateSkillCardFilterDictionary(ownerCard, partText, null);
			if (skillCardFilter != null)
			{
				filterCollection.CardFilterList.Add(skillCardFilter);
				continue;
			}
			if (text.Contains(ContentKeyword.random_count.ToStringCustom()))
			{
				filterCollection.SelectFilter = new SkillRandomSelectFilter(retParsedInfo.ValueStr);
				continue;
			}
			if (list == null)
			{
				list = new List<string>();
			}
			list.Add(partText);
		}
		return list;
	}

	public static ISkillBattlePlayerFilter CreateBattlePlayerFilter(ContentKeyword keyword)
	{
		return keyword switch
		{
			ContentKeyword.me => new SelfBattlePlayerFilter(), 
			ContentKeyword.op => new OpponentBattlePlayerFilter(), 
			ContentKeyword.both => new BothBattlePlayerFilter(), 
			_ => null, 
		};
	}

	public static ISkillTargetFilter CreatePlayerTargetFilter(ContentInfo info, ContentKeyword keyword, IReadOnlyBattleCardInfo ownerCard, SkillBase skill)
	{
		bool flag = ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase && !ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork;
		switch (keyword)
		{
		case ContentKeyword.inplay:
			return new SkillTargetInPlayFilter();
		case ContentKeyword.deck:
			return new SkillTargetDeckFilter();
		case ContentKeyword.battle_start_deck:
			return new SkillTargetBattleStartDeckFilter();
		case ContentKeyword.hand:
			return new SkillTargetHandFilter();
		case ContentKeyword.cemetery:
			return new SkillTargetCemeteryFilter();
		case ContentKeyword.self:
			return new SkillTargetSelfFilter(ownerCard);
		case ContentKeyword.inplay_self:
			return new SkillTargetInPlaySelfFilter(ownerCard);
		case ContentKeyword.inplay_other_self:
			return new SkillTargetInPlayOtherSelfFilter(ownerCard);
		case ContentKeyword.newer_inplay_other_self:
			return new SkillTargetNewerInPlayOtherSelfFilter(ownerCard);
		case ContentKeyword.hand_self:
			return new SkillTargetHandSelfFilter(ownerCard);
		case ContentKeyword.deck_self:
			return new SkillTargetDeckSelfFilter(ownerCard);
		case ContentKeyword.hand_other_self:
			return new SkillTargetHandOtherSelfFilter(ownerCard);
		case ContentKeyword.hand_other_oldest:
			return new SkillTargetHandOtherOldestFilter(ownerCard);
		case ContentKeyword.return_card:
			return new SkillTargetReturnCardFilter();
		case ContentKeyword.cant_attack_all_return_card:
			return new SkillTargetCantAttackAllReturnCardFilter();
		case ContentKeyword.last_target:
			if (!flag)
			{
				return new SkillTargetLastTargetFilter(info.CusomValue);
			}
			return new NetworkSkillTargetLastTargetFilter(info.CusomValue, skill);
		case ContentKeyword.inplay_last_target:
			return new SkillTargetInplayLastTargetFilter(info.CusomValue);
		case ContentKeyword.destroyed_last_target:
			return new SkillTargetDestroyedLastTargetFilter(info.CusomValue);
		case ContentKeyword.banished_last_target:
			return new SkillTargetBanishedLastTargetFilter(info.CusomValue);
		case ContentKeyword.cant_attack_all_last_target:
			return new SkillTargetCantAttackAllLastTargetFilter(info.CusomValue);
		case ContentKeyword.in_hand:
			return new SkillTargetInHandCardFilter();
		case ContentKeyword.discard:
			return new SkillTargetDiscardFilter();
		case ContentKeyword.burial_rite_card:
			return new SkillTargetBurialRiteCardFilter();
		case ContentKeyword.burial_rite_card_list:
			return new SkillTargetBurialRiteCardListFilter();
		case ContentKeyword.burial_rite_this_turn_card_list:
			return new SkillTargetBurialRiteThisTurnCardListFilter();
		case ContentKeyword.reanimated_this_turn_card_list:
			return new SkillTargetReanimatedThisTurnCardListFilter();
		case ContentKeyword.banished_card:
			return new SkillTargetBanishedCardFilter();
		case ContentKeyword.inplay_banished_card:
			return new SkillTargetInplayBanishedCardFilter();
		case ContentKeyword.hand_banished_card:
			return new SkillTargetHandBanishedCardFilter();
		case ContentKeyword.inplay_banished_this_turn_card_list:
			return new SkillTargetInplayBanishedThisTurnCardListFilter();
		case ContentKeyword.hand_banished_this_turn_card_list:
			return new SkillTargetHandBanishedThisTurnCardListFilter();
		case ContentKeyword.inplay_banished_card_list:
			return new SkillTargetInplayBanishedCardListFilter();
		case ContentKeyword.hand_banished_card_list:
			return new SkillTargetHandBanishedCardListFilter();
		case ContentKeyword.deck_banished_card_list:
			return new SkillTargetDeckBanishedCardListFilter();
		case ContentKeyword.return_this_turn_card_list:
			return new SkillTargetReturnThisTurnCardListFilter();
		case ContentKeyword.destroyed_card:
			return new SkillTargetDestroyedCardFilter();
		case ContentKeyword.destroyed_card_list:
			return new SkillTargetDestroyedCardListFilter();
		case ContentKeyword.turn_destroyed_cards:
			return new SkillTurnDestroyedFilter(info.ValueStr);
		case ContentKeyword.lastword_card_list:
			return new SkillTargetDestroyedWhenDestroyCardListFilter();
		case ContentKeyword.destroyed_this_turn_card_list:
			return new SkillTargetDestroyedThisTurnCardListFilter();
		case ContentKeyword.discard_this_turn_card_list:
			return new SkillTargetDiscardThisTurnCardListFilter();
		case ContentKeyword.discarded_card:
			return new SkillTargetDiscardedCardFilter(ownerCard);
		case ContentKeyword.damaged_card:
			return new SkillTargetDamagedCardFilter();
		case ContentKeyword.not_damaged_card:
			return new SkillTargetNotDamagedCardFilter();
		case ContentKeyword.healing_card:
			return new SkillTargetHealingCardFilter();
		case ContentKeyword.received_damage_card:
			return new SkillTargetReceivedDamageCardFilter();
		case ContentKeyword.give_damage_card:
			return new SkillTargetGiveDamageCardFilter();
		case ContentKeyword.evolution_card:
			return new SkillTargetEvolutionCardFilter();
		case ContentKeyword.fusion_ingrediented_card:
			return new SkillTargetFusionIngredientCardsFilter();
		case ContentKeyword.through_fusion_ingrediented_card:
			return new SkillTargetThroughFusionIngredientedCardFilter(ownerCard, info.ValueStr);
		case ContentKeyword.fusion_ingrediented_card_list_include_this_fusion:
			return new SkillTargetFusionIngredientedCardListIncludeThisFusion(ownerCard);
		case ContentKeyword.game_fusion_ingrediented_cards:
			return new SkillTargetGameFusionIngredientedCards();
		case ContentKeyword.game_fusion_ingrediented_and_discard_cards:
			return new SkillTargetGameFusionIngredientedAndDiscardCards();
		case ContentKeyword.fusion_ingrediented_this_turn_card_list:
			return new SkillTargetFusionIngredientedThisTurnCardList();
		case ContentKeyword.fusion_this_turn_card_list:
			return new SkillTargetFusionThisTurnCardList();
		case ContentKeyword.evolved_card_list:
			return new SkillTargetEvolvedCardListFilter();
		case ContentKeyword.played_card:
			return new SkillTargetPlayedCardFilter();
		case ContentKeyword.enhance_card:
			return new SkillTargetEnhanceCardfilter();
		case ContentKeyword.accelerated_card:
			return new SkillTargetAcceleratedCardfilter();
		case ContentKeyword.crystallized_card:
			return new SkillTargetCrystallizedCardfilter();
		case ContentKeyword.attacker:
			return new SkillTargetAttackerFilter();
		case ContentKeyword.be_attacked:
			return new SkillTargetBeAttackedFilter();
		case ContentKeyword.other_than_be_attacked:
			return new SkillTargetOtherThanBeAttackedFilter();
		case ContentKeyword.summoned_card:
			return new SkillTargetSummonedCardFilter();
		case ContentKeyword.fight_target:
			return new SkillTargetFightTargetFilter(ownerCard);
		case ContentKeyword.selected_cards:
			return new SkillTargetSelectedCardsFilter();
		case ContentKeyword.chosen_cards:
			return new SkillTargetChosenCardsFilter();
		case ContentKeyword.turn_play_cards:
			return new SkillTargetTurnPlayCardsFilter();
		case ContentKeyword.turn_draw_cards:
			return new SkillTargetTurnDrawCardsFilter();
		case ContentKeyword.turn_token_draw_skill_id:
			return new SkillTargetTurnTokenDrawSkillIdFilter(info.ValueStr);
		case ContentKeyword.turn_summon_cards:
			return new SkillTargetTurnSummonCardsFilter();
		case ContentKeyword.past_summon_cards:
			return new SkillTargetPastSummonCardsFilter(ownerCard, info.ValueStr);
		case ContentKeyword.turn_play_cards_other_self:
			return new SkillTargetTurnPlayCardsOtherSelfFilter(ownerCard, info.ValueStr);
		case ContentKeyword.game_play_cards_other_self:
			return new SkillTargetGamePlayCardsOtherSelfFilter(ownerCard);
		case ContentKeyword.game_play_cards:
			return new SkillTargetGamePlayCardsFilter();
		case ContentKeyword.game_summon_cards:
			return new SkillTargetGameSummonCardsFilter();
		case ContentKeyword.game_summon_cards_other:
			return new SkillTargetGameSummonCardsOtherFilter(ownerCard);
		case ContentKeyword.inplay_self_and_class:
			return new SkillTargetInplaySelfAndClassFilter(ownerCard);
		case ContentKeyword.inplay_and_hand:
			return new SkillTargetInplayAndHandFilter();
		case ContentKeyword.token_draw_card:
			return new SkillTargetTokenDrawCardFilter();
		case ContentKeyword.deck_draw_card:
			return new SkillTargetDeckDrawCardFilter();
		case ContentKeyword.drew_over_hand_limit:
			return new SkillTargetDrewOverHandLimitFilter();
		case ContentKeyword.load_target:
			return new SkillTargetLoadTargetFilter(ownerCard);
		case ContentKeyword.load_burial_rite_target:
			return new SkillTargetLoadBurialRiteTargetFilter(ownerCard);
		case ContentKeyword.reanimated_card:
			return new SkillTargetReanimatedCardFilter();
		case ContentKeyword.main_place:
			return new SkillTargetMainPlaceFilter();
		case ContentKeyword.skill_summoned_card:
			return new SkillTargetSkillSummonedCardFilter();
		case ContentKeyword.inplay_metamorphosed_cards:
			return new SkillTargetInplayMetamorphosedCardsFilter();
		case ContentKeyword.game_accelerated_cards:
			return new SkillTargetGameAcceleratedCardsFilter();
		case ContentKeyword.game_accelerated_cards_other_self:
			return new SkillTargetGameAcceleratedCardsOtherSelfFilter(ownerCard);
		case ContentKeyword.game_crystallized_cards:
			return new SkillTargetGameCrystallizedCardsFilter();
		case ContentKeyword.game_skill_activated:
			return new SkillTargetGameSkillActivatedFilter();
		case ContentKeyword.left_cards:
			return new SkillTargetLeftCardsFilter();
		case ContentKeyword.attaching_ability:
			return new SkillAttachingAbilityFilter(info.Value);
		case ContentKeyword.inplay_buffing_cards:
			return new SkillTargetInplayBuffingCardsFilter();
		case ContentKeyword.inplay_debuffing_cards:
			return new SkillTargetInplayDebuffingCardsFilter();
		case ContentKeyword.game_deck_draw_cards:
			return new SkillTargetGameDeckDrawCardsFilter();
		case ContentKeyword.game_draw_cards:
			return new SkillTargetGameDrawCardsFilter();
		case ContentKeyword.game_add_update_deck_cards:
			return new SkillTargetGameAddUpdateDeckCardsFilter();
		case ContentKeyword.game_left_cards:
			return new SkillTargetGameLeftCardsFilter();
		case ContentKeyword.left_this_turn_card_list:
			return new SkillTargetLeftThisTurnCardListFilter();
		case ContentKeyword.before_transform_card:
			return new SkillTargetBeforeTransformCardFilter(ownerCard);
		case ContentKeyword.game_quick_attack_cards:
			return new SkillTargetGameQuickAttackCardsFilter();
		case ContentKeyword.discard_card_list:
			return new SkillTargetDiscardCardListFilter();
		case ContentKeyword.chant_count_change_cards:
			return new SkillTargetChantCountChangeCardsFilter();
		case ContentKeyword.shortage_deck_win_cards:
			return new SkillTargetShortageDeckWinCards();
		default:
			return null;
		}
	}

	private static ISkillTargetFilter CreateSingleTargetFilter(ContentKeyword keyword, IReadOnlyBattleCardInfo ownerCard)
	{
		return keyword switch
		{
			ContentKeyword.self => new SkillTargetSelfFilter(ownerCard), 
			ContentKeyword.skill_drew_card => new SkillTargetSkillDrewCardFilter(), 
			ContentKeyword.skill_update_deck_card => new SkillTargetSkillUpdateDeckCardFilter(), 
			ContentKeyword.unique_base_card_id_card => new SkillTargetUniqueBaseCardIDCardFilter(), 
			_ => null, 
		};
	}

	public static ISkillCardFilter CreateCardTypeFilter(ContentKeyword keyword, IReadOnlyBattleCardInfo ownerCard = null, string valueStr = "")
	{
		return keyword switch
		{
			ContentKeyword.unit => new SkillUnitFilter(), 
			ContentKeyword._class => new SkillClassFilter(), 
			ContentKeyword.field => new SkillFieldFilter(), 
			ContentKeyword.chant_field => new SkillChantFieldFilter(), 
			ContentKeyword.not_chant_field => new SkillNotChantFieldFilter(), 
			ContentKeyword.spell => new SkillSpellFilter(), 
			ContentKeyword.unit_and_class => new SkillUnitAndClassFilter(), 
			ContentKeyword.unit_and_allfield => new SkillUnitAndAllFieldFilter(), 
			ContentKeyword.spell_and_field => new SkillSpellAndFieldFilter(), 
			ContentKeyword.unit_and_chant_field => new SkillUnitAndChantFieldFilter(), 
			ContentKeyword.unit_and_not_chant_field => new SkillUnitAndNotChantField(), 
			ContentKeyword.play_card_type => new SkillPlayCardTypeFilter(ownerCard, valueStr), 
			ContentKeyword.all => new SkillAllCardFilter(), 
			ContentKeyword.last => new SkillLastFilter(), 
			_ => null, 
		};
	}

	public static ISkillCardFilter CreateTribeFilter(ContentInfo info)
	{
		if (info.ValueStr == ContentKeyword.last_target.ToString())
		{
			return new SkillLastTargetTribeFilter(info.Operator);
		}
		if (info.ValueStr == ContentKeyword.any_tribe.ToString())
		{
			string op = ((info.Operator == "=") ? "!=" : "=");
			return new SkillTribeFilter(CardBasePrm.TribeType.ALL, op);
		}
		return new SkillTribeFilter(ParseEnum(info.ValueStr, CardBasePrm.TribeType.MAX), info.Operator);
	}

	public static ISkillCardFilter CreateClanFilter(ContentInfo info, IReadOnlyBattleCardInfo ownerCard)
	{
		if (info.ValueStr == ContentKeyword.all_and_self_both_clan.ToString())
		{
			List<CardBasePrm.ClanType> list = new List<CardBasePrm.ClanType>();
			CardBasePrm.ClanType clan = ownerCard.SelfBattlePlayer.Class.Clan;
			CardBasePrm.ClanType item = (CardBasePrm.ClanType)(ownerCard.SelfBattlePlayer.Class.IsPlayer ? ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().GetPlayerSubClassId() : ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().GetEnemySubClassId());
			list.Add(CardBasePrm.ClanType.ALL);
			list.Add(clan);
			list.Add(item);
			return new SkillMultiClanFilter(list, info.Operator);
		}
		return new SkillClanFilter(ParseEnum(info.ValueStr, CardBasePrm.ClanType.ALL), info.Operator);
	}

	public static ISkillCardFilter CreateCardConditionFilter(string text, IReadOnlyBattleCardInfo ownerCard)
	{
		ParseContentInfo(text, out var retParsedInfo);
		switch (retParsedInfo.Name)
		{
		case ContentKeyword.tribe:
			return CreateTribeFilter(retParsedInfo);
		case ContentKeyword.summon_moment_tribe:
			return new SkillSummonMomentTribeFilter(ParseEnum(retParsedInfo.ValueStr, CardBasePrm.TribeType.MAX), retParsedInfo.Operator);
		case ContentKeyword.play_moment_tribe:
			return new SkillPlayMomentTribeFilter(ParseEnum(retParsedInfo.ValueStr, CardBasePrm.TribeType.MAX), retParsedInfo.Operator);
		case ContentKeyword.play_moment_spell_charge:
			return new SkillPlayMomentSpellChargeFilter();
		case ContentKeyword.update_deck_moment_tribe:
			return new SkillUpdateDeckMomentTribeFilter(ParseEnum(retParsedInfo.ValueStr, CardBasePrm.TribeType.MAX), retParsedInfo.Operator);
		case ContentKeyword.clan:
			return CreateClanFilter(retParsedInfo, ownerCard);
		case ContentKeyword.both_clan:
			return new SkillBothClanFilter(ParseEnum(retParsedInfo.ValueStr, CardBasePrm.ClanType.ALL), retParsedInfo.Operator);
		case ContentKeyword.leader_skin_id:
			return new SkillLeaderSkinIdFilter(int.Parse(retParsedInfo.ValueStr), retParsedInfo.Operator);
		case ContentKeyword.evolution:
			return new SkillEvolutionCardFilter(ParseBoolean(retParsedInfo.ValueStr));
		case ContentKeyword.is_inplay:
			return new SkillConditionIsInplayCardFilter(ParseBoolean(retParsedInfo.ValueStr));
		case ContentKeyword.is_reanimate:
			return new SkillConditionIsReanimateCardFilter(ParseBoolean(retParsedInfo.ValueStr));
		case ContentKeyword.ability:
			return CreateAbilityFilter(retParsedInfo.Value, retParsedInfo.Operator);
		case ContentKeyword.selectable:
			if (ownerCard.IsSpell)
			{
				return new SpellSelectableCardFilter(ownerCard);
			}
			return new SkillSelectableCardFilter(ownerCard);
		case ContentKeyword.unique_base_card_id_card:
			return new SkillUniqueBaseCardIDCardFilter();
		case ContentKeyword.include_self:
			return new SkillIncludeSelfCardFilter(ownerCard, retParsedInfo.ValueStr);
		case ContentKeyword.life_is_larger:
			return new SkillLifeIsLarger();
		case ContentKeyword.attack_is_larger:
			return new SkillAttackIsLarger();
		case ContentKeyword.fusion_ingrediented_card_list:
			return new SkillFusionIngredientCardListFilter();
		case ContentKeyword.geton_card:
			return new SkillGetOnCardListFilter();
		case ContentKeyword.getoff_card:
			return new SkillGetOffCardListFilter();
		case ContentKeyword.avarice:
			return new SkillAvariceCardFilter(ParseBoolean(retParsedInfo.ValueStr));
		case ContentKeyword.is_watch:
			return new SkillIsWatchFilter(ParseBoolean(retParsedInfo.ValueStr));
		case ContentKeyword.is_buff_detail:
			return new SkillIsBuffDetailFilter(ParseBoolean(retParsedInfo.ValueStr));
		case ContentKeyword.is_fusion_info_or_is_not_sidelog:
			return new SkillIsFusionInfoOrIsNotSideLogFilter(ParseBoolean(retParsedInfo.ValueStr));
		case ContentKeyword.is_sidelog_by_when_destroy_and_buff_detail:
			return new SkillIsInvokeSideLogorBuffDetailFilter(ownerCard, ParseBoolean(retParsedInfo.ValueStr));
		case ContentKeyword.is_owner:
			return new SkillIsOwnerFilter(ParseBoolean(retParsedInfo.ValueStr));
		case ContentKeyword.self_skill_drew_card:
			return new SkillTargetDrewSkillFilter(ownerCard, retParsedInfo.ValueStr);
		case ContentKeyword.self_skill_return_card:
			return new SkillTargetReturnSkillFilter(ownerCard, retParsedInfo.ValueStr);
		case ContentKeyword.skill_summoned_card_list:
			return new SkillTargetSkillSummonedCardIdFilter();
		case ContentKeyword.skill_drew_card_list:
			return new SkillTargetSkillDrawCardListFilter();
		case ContentKeyword.not_unique_base_card_id_card:
			return new SkillTargetNotUniqueBaseCardIdFilter();
		case ContentKeyword.last_burial_rite_card_list:
			return new SkillLastBurialRiteCardFilter();
		case ContentKeyword.attacked:
			if (ownerCard.SelfBattlePlayer.BattleMgr is SingleBattleMgr)
			{
				return new SkillAttackedCardFilter(retParsedInfo.ValueStr);
			}
			break;
		}
		return null;
	}

	public static ISkillSelectFilter CreateCardSelectFilter(string text)
	{
		ParseContentInfo(text, out var retParsedInfo);
		return retParsedInfo.Name switch
		{
			ContentKeyword.select_index => new SkillSelectIndexFilter(int.Parse(retParsedInfo.ValueStr)), 
			ContentKeyword.select_cardlist => new SkillOnlyOneFilter(retParsedInfo.ValueStr), 
			ContentKeyword.limit_upper_count_from_oldest => new SkillLimitUpperCountFromOldestFilter(int.Parse(retParsedInfo.ValueStr)), 
			ContentKeyword.limit_upper_count_from_newest => new SkillLimitUpperCountFromNewestFilter(int.Parse(retParsedInfo.ValueStr)), 
			_ => null, 
		};
	}

	public static ISkillCardFilter CreateCardParameterCompareFilter(string text, IReadOnlyBattleCardInfo ownerCard)
	{
		ParseContentInfo(text, out var retParsedInfo);
		return retParsedInfo.Name switch
		{
			ContentKeyword.base_cost => new SkillParameterBaseCostFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.cost => new SkillParameterCostFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.base_offense => new SkillParameterBaseOffenseFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.offense => new SkillParameterOffenseFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.base_life => new SkillParameterBaseLifeFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.chant_count => new SkillParameterChantCountFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.max_life => new SkillParameterMaxLifeFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.life => new SkillParameterLifeFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.id => new SkillParameterIdFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.base_card_id => new SkillParameterBaseCardIdFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.attack_count => new SkillParameterAttackCountFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.max_attack_count => new SkillParameterMaxAttackCountFilter(retParsedInfo.ValueStr, retParsedInfo.Operator), 
			ContentKeyword.destroy => new SkillParameterDestroyFilter(retParsedInfo.ValueStr), 
			ContentKeyword.strict_destroy => new SkillParameterStrictDestroyFilter(retParsedInfo.ValueStr), 
			ContentKeyword.buff_count => new SkillParameterBuffCountFilter(int.Parse(retParsedInfo.ValueStr), retParsedInfo.Operator), 
			ContentKeyword.buff_life_count => new SkillParameterBuffLifeCountFilter(int.Parse(retParsedInfo.ValueStr), retParsedInfo.Operator), 
			ContentKeyword.previous_turn_attacked => new SkillParameterPreviousTurnAttackedFilter(retParsedInfo.ValueStr), 
			ContentKeyword.last_life => new SkillParameterLastLifeFilter(int.Parse(retParsedInfo.ValueStr), retParsedInfo.Operator), 
			ContentKeyword.has_skill => new SkillParameterHasSkillFilter(retParsedInfo.ValueStr), 
			ContentKeyword.destroyed_by => new SkillParameterDestroyedByFilter(retParsedInfo.Value), 
			ContentKeyword.returned_by => new SkillParameterReturnedByFilter(ownerCard, retParsedInfo.Value), 
			ContentKeyword.destroyed_by_ability => new SkillParameterDestroyedByAbilityFilter(retParsedInfo.Value), 
			ContentKeyword.destroyed_by_card_id => new SkillParameterDestroyedByCardIdFilter(retParsedInfo.ValueStr), 
			ContentKeyword.destroyed_by_card_id_and_ability => new SkillParameterDestroyedByCardIdAndAbilityFilter(retParsedInfo.ValueStr), 
			ContentKeyword.game_changed_max_life_count => new SkillParameterChangeMaxLifeCountFilter(int.Parse(retParsedInfo.ValueStr), retParsedInfo.Operator), 
			ContentKeyword.is_chaos => new SkillParameterIsChaosFilter(retParsedInfo.ValueStr), 
			ContentKeyword.is_unlimited => new SkillParameterIsUnlimitedFilter(ParseBoolean(retParsedInfo.ValueStr)), 
			ContentKeyword.rarity => new SkillParameterRarityFilter(int.Parse(retParsedInfo.ValueStr), retParsedInfo.Operator), 
			_ => null, 
		};
	}

	public static bool ParseContentInfo(string text, out ContentInfo retParsedInfo)
	{
		retParsedInfo = new ContentInfo(ContentKeyword.none, "", ContentKeyword.none, "");
		Match match = Regex.Match(text, "^<?[\\w_-]+");
		if (match == Match.Empty)
		{
			return false;
		}
		if (match == null)
		{
			LocalLog.AccumulateTraceLog("Match Result is null.  text : " + text);
		}
		if (match.Value.Contains("fixed_generic_value_initial_"))
		{
			string valueStr = string.Empty;
			if (match.Value != text)
			{
				string[] array = text.Split('=');
				text = array[0];
				valueStr = ((array.Count() > 2) ? (array[1] + "=" + array[2]) : array[1]);
			}
			int customValue = 0;
			ParseSuffixX(match.Value, out customValue);
			retParsedInfo = new ContentInfo(ContentKeyword.fixed_generic_value_initial, "", ContentKeyword.none, valueStr, customValue.ToString());
			return true;
		}
		if (match.Value == text)
		{
			if (match.Value.Contains("destroyed_last_target_"))
			{
				int customValue2 = 0;
				string valueStr2 = ParseSuffixX(match.Value, out customValue2);
				retParsedInfo = new ContentInfo(ContentKeyword.destroyed_last_target, "", ContentKeyword.none, valueStr2, customValue2.ToString());
			}
			else if (match.Value.Contains("inplay_last_target_"))
			{
				int customValue3 = 0;
				string valueStr3 = ParseSuffixX(match.Value, out customValue3);
				retParsedInfo = new ContentInfo(ContentKeyword.inplay_last_target, "", ContentKeyword.none, valueStr3, customValue3.ToString());
			}
			else if (match.Value.Contains("banished_last_target_"))
			{
				int customValue4 = 0;
				string valueStr4 = ParseSuffixX(match.Value, out customValue4);
				retParsedInfo = new ContentInfo(ContentKeyword.banished_last_target, "", ContentKeyword.none, valueStr4, customValue4.ToString());
			}
			else if (match.Value.Contains("last_target_"))
			{
				int customValue5 = 0;
				string valueStr5 = ParseSuffixX(match.Value, out customValue5);
				retParsedInfo = new ContentInfo(ContentKeyword.last_target, "", ContentKeyword.none, valueStr5, customValue5.ToString());
			}
			else
			{
				retParsedInfo = new ContentInfo(Str2ContentKeyword(match.Value), "", ContentKeyword.none, "");
			}
			return true;
		}
		text = text.Substring(match.Value.Length);
		Match match2 = Regex.Match(text, "[<>!:=]+");
		if (match2 == Match.Empty)
		{
			return false;
		}
		if (match2 == null)
		{
			LocalLog.AccumulateTraceLog("Match Result is null.  text : " + text);
		}
		string text2 = text.Substring(match2.Value.Length);
		int customValue6 = 0;
		if (!text2.Contains('{'))
		{
			text2 = ParseSuffixX(text2, out customValue6);
		}
		retParsedInfo = new ContentInfo(Str2ContentKeyword(match.Value), match2.Value, Str2ContentKeyword(text2), text2, customValue6.ToString());
		return true;
	}

	public static string ParseSuffixX(string target, out int customValue)
	{
		if (target.Contains(destroyed_last_target) && target.Count() > destroyed_last_target.Count())
		{
			if (int.TryParse(target.Substring(destroyed_last_target.Count() + 1), out customValue))
			{
				return destroyed_last_target;
			}
		}
		else if (target.Contains(inplay_last_target) && target.Count() > inplay_last_target.Count())
		{
			if (int.TryParse(target.Substring(inplay_last_target.Count() + 1), out customValue))
			{
				return inplay_last_target;
			}
		}
		else if (target.Contains(banished_last_target) && target.Count() > banished_last_target.Count())
		{
			if (int.TryParse(target.Substring(banished_last_target.Count() + 1), out customValue))
			{
				return banished_last_target;
			}
		}
		else if (target.Contains(equal_or_less_cost_from_last_target) && target.Count() > equal_or_less_cost_from_last_target.Count())
		{
			if (int.TryParse(target.Substring(equal_or_less_cost_from_last_target.Count() + 1), out customValue))
			{
				return equal_or_less_cost_from_last_target;
			}
		}
		else if (target.Contains(last_target) && target.Count() > last_target.Count())
		{
			if (int.TryParse(target.Substring(last_target.Count() + 1), out customValue))
			{
				return last_target;
			}
		}
		else if (target.Contains(fixed_generic_value_initial) && target.Count() > fixed_generic_value_initial.Count() && int.TryParse(target.Substring(fixed_generic_value_initial.Count() + 1), out customValue))
		{
			return fixed_generic_value_initial;
		}
		customValue = 0;
		return target;
	}

	public static T ParseEnum<T>(string strEnum, T errorVal)
	{
		if (strEnum == string.Empty)
		{
			return errorVal;
		}
		try
		{
			return (T)Enum.Parse(typeof(T), strEnum.ToUpper());
		}
		catch (Exception)
		{
			return errorVal;
		}
	}

	public static bool ParseBoolean(string boolean)
	{
		if (boolean == "true")
		{
			return true;
		}
		return false;
	}

	public static ISkillCardFilter CreateAbilityFilter(ContentKeyword keyword, string optionText)
	{
		return keyword switch
		{
			ContentKeyword.spell_charge => new SkillAbilitySpellChargeFilter(optionText), 
			ContentKeyword.sneak => new SkillAbilitySneakFilter(optionText), 
			ContentKeyword.guard => new SkillAbilityGuardFilter(optionText), 
			ContentKeyword.killer => new SkillAbilityKillerFilter(optionText), 
			ContentKeyword.rush => new SkillAbilityRushFilter(optionText), 
			ContentKeyword.quick => new SkillAbilityQuickFilter(optionText), 
			ContentKeyword.drain => new SkillAbilityDrainFilter(optionText), 
			ContentKeyword.cant_attack_all => new SkillAbilityCantAttackAllFilter(optionText), 
			ContentKeyword.white_ritual => new SkillAbilityWhiteRitualFilter(), 
			ContentKeyword.stack_white_ritual => new SkillAbilityStackWhiteRitualFilter(), 
			ContentKeyword.destroy_white_ritual => new SkillAbilityDestroyWhiteRitualFilter(), 
			ContentKeyword.when_destroy => new SkillAbilityWhenDestroyFilter(optionText), 
			ContentKeyword.when_attack => new SkillAbilityWhenAttackFilter(), 
			ContentKeyword.when_fight => new SkillAbilityWhenFightFilter(), 
			ContentKeyword.union_burst => new SkillAbilityUnionBurstFilter(optionText), 
			ContentKeyword.super_skybound_art => new SkillAbilitySuperSkyboundArtFilter(optionText), 
			ContentKeyword.enhance => new SkillAbilityEnhanceFilter(optionText), 
			ContentKeyword.when_play => new SkillAbilityWhenPlayFilter(optionText, isOnlyNoSelect: false), 
			ContentKeyword.when_play_no_select => new SkillAbilityWhenPlayFilter(optionText, isOnlyNoSelect: true), 
			ContentKeyword.when_accelerate => new SkillAbilityAccelerateFilter(optionText), 
			ContentKeyword.when_crystallize => new SkillAbilityCrystallizeFilter(optionText), 
			ContentKeyword.reanimate => new SkillAbilityReanimateFilter(optionText), 
			ContentKeyword.when_evolve => new SkillAbilityWhenEvolveFilter(optionText), 
			ContentKeyword.when_play_no_select_and_burial_rite => new SkillAbilityWhenPlayNoSelectAndBurialRiteFilter(optionText), 
			ContentKeyword.necromance => new SkillAbilityNecromanceFilter(optionText), 
			ContentKeyword.burial_rite => new SkillAbilityBurialRiteFilter(optionText), 
			ContentKeyword.fusion => new SkillAbilityFusionFilter(optionText), 
			_ => null, 
		};
	}
}
