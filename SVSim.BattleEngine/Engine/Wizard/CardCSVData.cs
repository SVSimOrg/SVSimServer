using System.Collections.Generic;
// TODO(engine-cleanup-pass2): 3 of 25 methods unrun in baseline
//   Type: Wizard.CardCSVData
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public class CardCSVData
{
	public string card_id;

	public string resource_card_id;

	public string card_set_id;

	public string is_foil;

	public string path;

	public string clan;

	public string tribe;

	public string skill;

	public string skill_timing;

	public string skill_condition;

	public string skill_target;

	public string skill_option;

	public string skill_preprocess;

	public string skill_effect_condition;

	public string skill_icon;

	public string atk_effect_path;

	public string atk_se;

	public string atk_move_type;

	public string atk_effect_engin_type;

	public string atk_time;

	public string skill_effect_path;

	public string skill_se;

	public string skill_move_type;

	public string skill_effect_engin_type;

	public string skill_effect_time;

	public string skill_effect_target_type = "";

	public string char_type;

	public string evo_skill_effect_path;

	public string evo_skill_se;

	public string evo_skill_move_type;

	public string evo_skill_effect_engin_type;

	public string evo_skill_effect_time;

	public string cost;

	public string atk;

	public string life;

	public string evo_atk;

	public string evo_life;

	public string chant_count;

	public string rarity;

	public string get_red_ether;

	public string use_red_ether;

	public string summon_effect_path;

	public string summon_se_path;

	public string summon_move_type;

	public string summon_effect_type;

	public string summon_time;

	public string evol_effect_path;

	public string evol_se_path;

	public string evo_effect_type;

	public string evol_time;

	public string destroy_effect_path;

	public string play_voice;

	public string evo_voice;

	public string atk_voice;

	public string destroy_voice;

	public string skill_voice;

	public string base_card_id;

	public string normal_card_id;

	public string foil_card_id;

	public string card_frame_type;

	public string TwoPickFoilCardId;

	public string tilling_normal_x;

	public string tilling_normal_y;

	public string offset_normal_x;

	public string offset_normal_y;

	public string tilling_evol_x;

	public string tilling_evol_y;

	public string offset_evol_x;

	public string offset_evol_y;

	public int SortIndex { get; private set; }

	public string CardHashId { get; private set; }

	public string CardNameId { get; private set; }

	public string TribeNameId { get; private set; }

	public string SkillDescriptionId { get; private set; }

	public string EvoSkillDescriptionId { get; private set; }

	public string DescriptionId { get; private set; }

	public string EvoDescriptionId { get; private set; }

	public string CardVoiceId { get; private set; }

	public string IsOverrideSkillDescription { get; private set; }

	public string IsResurgentCard { get; private set; }

	public CardCSVData(string[] columns, int sortIndex)
	{
		SortIndex = sortIndex;
		CsvColumns csvColumns = new CsvColumns(columns, "card_master.csv");
		card_id = csvColumns.NextValue;
		foil_card_id = csvColumns.NextValue;
		card_set_id = csvColumns.NextValue;
		CardNameId = csvColumns.NextValue;
		is_foil = csvColumns.NextValue;
		path = csvColumns.NextValue;
		char_type = csvColumns.NextValue;
		clan = csvColumns.NextValue;
		tribe = csvColumns.NextValue;
		TribeNameId = csvColumns.NextValue;
		cost = csvColumns.NextValue;
		atk = csvColumns.NextValue;
		life = csvColumns.NextValue;
		evo_atk = csvColumns.NextValue;
		evo_life = csvColumns.NextValue;
		chant_count = csvColumns.NextValue;
		rarity = csvColumns.NextValue;
		skill = csvColumns.NextValue;
		skill_timing = csvColumns.NextValue;
		skill_condition = csvColumns.NextValue;
		skill_target = csvColumns.NextValue;
		skill_option = csvColumns.NextValue;
		skill_preprocess = csvColumns.NextValue;
		skill_effect_condition = csvColumns.NextValue;
		skill_icon = csvColumns.NextValue;
		SkillDescriptionId = csvColumns.NextValue;
		EvoSkillDescriptionId = csvColumns.NextValue;
		summon_effect_path = csvColumns.NextValue;
		summon_se_path = csvColumns.NextValue;
		summon_move_type = csvColumns.NextValue;
		summon_effect_type = csvColumns.NextValue;
		summon_time = csvColumns.NextValue;
		atk_effect_path = csvColumns.NextValue;
		atk_se = csvColumns.NextValue;
		atk_move_type = csvColumns.NextValue;
		atk_effect_engin_type = csvColumns.NextValue;
		atk_time = csvColumns.NextValue;
		skill_effect_path = csvColumns.NextValue;
		skill_se = csvColumns.NextValue;
		skill_move_type = csvColumns.NextValue;
		skill_effect_engin_type = csvColumns.NextValue;
		skill_effect_time = csvColumns.NextValue;
		skill_effect_target_type = csvColumns.NextValue;
		evo_skill_effect_path = csvColumns.NextValue;
		evo_skill_se = csvColumns.NextValue;
		evo_skill_move_type = csvColumns.NextValue;
		evo_skill_effect_engin_type = csvColumns.NextValue;
		evo_skill_effect_time = csvColumns.NextValue;
		get_red_ether = ConvOverwriteGetRedEther(card_id, csvColumns.NextValue);
		use_red_ether = ConvOverwriteUseRedEther(card_id, csvColumns.NextValue);
		evol_effect_path = csvColumns.NextValue;
		evol_se_path = csvColumns.NextValue;
		evo_effect_type = csvColumns.NextValue;
		evol_time = csvColumns.NextValue;
		destroy_effect_path = csvColumns.NextValue;
		play_voice = csvColumns.NextValue;
		evo_voice = csvColumns.NextValue;
		atk_voice = csvColumns.NextValue;
		destroy_voice = csvColumns.NextValue;
		skill_voice = csvColumns.NextValue;
		DescriptionId = csvColumns.NextValue;
		EvoDescriptionId = csvColumns.NextValue;
		card_frame_type = csvColumns.NextValue;
		base_card_id = csvColumns.NextValue;
		normal_card_id = csvColumns.NextValue;
		resource_card_id = csvColumns.NextValue;
		tilling_normal_x = csvColumns.NextValue;
		tilling_normal_y = csvColumns.NextValue;
		offset_normal_x = csvColumns.NextValue;
		offset_normal_y = csvColumns.NextValue;
		tilling_evol_x = csvColumns.NextValue;
		tilling_evol_y = csvColumns.NextValue;
		offset_evol_x = csvColumns.NextValue;
		offset_evol_y = csvColumns.NextValue;
		CardVoiceId = csvColumns.NextValue;
		IsOverrideSkillDescription = csvColumns.NextValue;
		IsResurgentCard = csvColumns.NextValue;
		TwoPickFoilCardId = csvColumns.NextValue;
		if (!CardMaster.IsUseLocalCardMaster())
		{
			CardHashId = csvColumns.NextValue;
		}
	}

	private string ConvOverwriteGetRedEther(string cardId, string getRedEther)
	{
		if (Data.Load.data.OverwriteGetRedEtherDict.ContainsKey(cardId))
		{
			return Data.Load.data.OverwriteGetRedEtherDict[cardId].ToString();
		}
		return getRedEther;
	}

	private string ConvOverwriteUseRedEther(string cardId, string useRedEther)
	{
		if (Data.Load.data.OverwriteUseRedEtherDict.ContainsKey(cardId))
		{
			return Data.Load.data.OverwriteUseRedEtherDict[cardId].ToString();
		}
		return useRedEther;
	}
}
