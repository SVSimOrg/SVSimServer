using System.Collections.Generic;
using System.Linq;

public class NetworkSkill_random_array : Skill_random_array
{
	public NetworkSkill_random_array(NetworkBattleManagerBase battleManager, SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		NetworkSkill_random_array networkSkill_random_array = this;
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch || base.SkillPrm.ownerCard.IsPlayer)
		{
			return;
		}
		base.OnSkillStart += delegate(SkillBase skill, List<BattleCardBase> targetsCard, SkillConditionCheckerOption checkeroption)
		{
			if (!targetsCard.Any((BattleCardBase c) => !c.IsInHand && !c.IsInDeck))
			{
				List<CardDataModel> receiveCardList = battleManager.networkBattleData.GetReceiveData().GetReceiveCardList();
				int i;
				for (i = 0; i < targetsCard.Count; i++)
				{
					CardDataModel cardDataModel = receiveCardList.SingleOrDefault((CardDataModel c) => c.Index == targetsCard[i].Index);
					if (cardDataModel != null && !(cardDataModel.AttachTarget == string.Empty))
					{
						string[] array = cardDataModel.AttachTarget.Split(',');
						for (int num = 0; num < array.Length; num++)
						{
							if (int.TryParse(array[num], out var targetSkillCount))
							{
								SkillBase skillBase = battleManager.PublishedSkillList.SingleOrDefault((SkillBase s) => s.PublishedActiveSkillCount == targetSkillCount);
								if (skillBase != null && skillBase.PreprocessList.SingleOrDefault((SkillPreprocessBase p) => p is SkillPreprocessRandomArrayIndex) is SkillPreprocessRandomArrayIndex skillPreprocessRandomArrayIndex)
								{
									networkSkill_random_array.AddSelectedIndex(targetsCard[i], skillPreprocessRandomArrayIndex.RandomIndexes.ToList());
								}
							}
						}
					}
				}
			}
		};
	}

	private void AddSelectedIndex(BattleCardBase key, List<int> values)
	{
		if (!base.SelectedIndex.ContainsKey(key))
		{
			base.SelectedIndex[key] = new List<int>();
		}
		for (int i = 0; i < values.Count; i++)
		{
			if (!base.SelectedIndex[key].Contains(values[i]))
			{
				base.SelectedIndex[key].Add(values[i]);
			}
		}
	}
}
