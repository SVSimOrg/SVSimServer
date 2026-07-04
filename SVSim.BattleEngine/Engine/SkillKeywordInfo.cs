using System;
using System.Collections.Generic;

public static class SkillKeywordInfo
{
	public enum SkillKeyword
	{
		damage_modifier,
		attach_skill,
		attack_by_life,
		attack_count,
		banish,
		cant_activate_fanfare,
		cant_activate_shortage_deck_win,
		cant_attack,
		cant_play,
		cant_summon,
		change_affiliation,
		change_cemetery,
		change_rally_count,
		update_deck,
		chant_count_change,
		consume_ep_modifier,
		cost_change,
		damage,
		damage_cut,
		destroy,
		discard,
		drain,
		draw,
		evolve,
		extra_turn,
		force_berserk,
		force_skill_target,
		attract_skill_target,
		force_avarice,
		force_wrath,
		guard,
		heal,
		ignore_guard,
		independent,
		not_be_debuffed,
		indestructible,
		killer,
		lose,
		metamorphose,
		fusion_metamorphose,
		transform,
		none,
		not_be_attacked,
		play_count_change,
		possess_ep_modifier,
		power_down,
		powerup,
		power_modifier,
		pp_fixeduse,
		pp_modifier,
		bp_modifier,
		quick,
		reflection,
		remove_by_banish,
		remove_by_destroy,
		return_card,
		rush,
		select,
		shield,
		sneak,
		special_win,
		special_lose,
		spell_charge,
		summon_card,
		summon_token,
		token_draw,
		special_token_draw,
		trigger,
		turn_start_fixed_pp,
		unite,
		untouchable,
		repeat_skill,
		choice,
		shortage_deck_win,
		generic_value_modifier,
		change_union_burst_count,
		change_skybound_art_count,
		change_super_skybound_art_count,
		change_white_ritual_stack,
		stack_white_ritual,
		rob_skill,
		random_array,
		no_duplication_random_array,
		cant_evolution,
		invoke_skill,
		token_draw_modifier,
		fusion,
		random_attack,
		clear_destroyed_and_discarded_card_list,
		clear_summoned_card_list,
		geton,
		getoff,
		not_decrease_pp,
		heal_modifier,
		life_zero_activate_leon_skill,
		copy_skill,
		loop_skill,
		evolve_to_other,
		not_attached_resident_chant_count_change,
		invoke_emote,
		invoke_voice,
		can_play_self
	}

	public static Dictionary<string, SkillKeyword> Keyword2StringDictionary;

	static SkillKeywordInfo()
	{
		SetupStatic();
	}

	private static void SetupStatic()
	{
		if (Keyword2StringDictionary != null)
		{
			return;
		}
		Keyword2StringDictionary = new Dictionary<string, SkillKeyword>(Enum.GetValues(typeof(SkillKeyword)).Length);
		foreach (SkillKeyword value in Enum.GetValues(typeof(SkillKeyword)))
		{
			Keyword2StringDictionary.Add(value.ToString(), value);
		}
	}

	public static SkillKeyword GetKeyword(string info)
	{
		if (!Keyword2StringDictionary.TryGetValue(info, out var value))
		{
			return SkillKeyword.none;
		}
		return value;
	}
}
