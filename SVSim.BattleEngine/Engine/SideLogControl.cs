using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle;
using Wizard.Battle.View.Vfx;

public class SideLogControl : MonoBehaviour
{
	private class SideLog
	{
		private List<SkillBase> _skills;

		private List<int> _skillHashCodes;

		private int _repeatCount;

		public BattleCardBase Card { get; private set; }

		public bool IsSelect { get; private set; }

		public SideLog(BattleCardBase card, SkillBase skill, bool isSelect)
		{
			Card = card;
			_skills = new List<SkillBase>();
			_skillHashCodes = new List<int>();
			IsSelect = isSelect;
			CheckShowSideLog(skill);
		}

		public bool CheckShowSideLog(SkillBase skill)
		{
			_skills.Add(skill);
			int num = _skills.Where((SkillBase s) => skill == s).Count();
			if (num > _repeatCount)
			{
				_repeatCount = num;
				return true;
			}
			if (skill != null && skill.SkillPrm.ownerCard.BaseParameter.BaseCardId == 900241110)
			{
				return true;
			}
			return false;
		}
	}

	private IList<NguiObjs> PanelList;

	private IList<NguiObjs> PanelOnList;

	private IList<bool> PanelOnFlagList;

	private List<SideLog> LogShowingList;

	private SideLog _lastSideLog;

	public bool IsOnSummonOrReturnTimingSkill(SkillBase skill)
	{
		if (skill == null)
		{
			return false;
		}
		if (skill.IsWhenPlaySkill || skill.OnWhenSummonStart != 0 || skill.OnWhenReturnStart != 0)
		{
			return true;
		}
		if (skill.OnWhenChangeInPlay != 0 || skill.OnWhenChangeInPlayImmediate != 0 || skill.OnWhenChangeClassLifeInplay != 0 || skill.OnWhenChangePPTotal != 0 || skill.OnWhenAddToHand != 0)
		{
			return true;
		}
		return false;
	}

	public VfxBase ClearLastShowLogCard()
	{
		return InstantVfx.Create(delegate
		{
			_lastSideLog = null;
		});
	}

	public void RemoveAllLog()
	{
		if (PanelOnList.Count >= 1)
		{
			for (int num = PanelOnList.Count - 1; num >= 0; num--)
			{
				int index = PanelList.IndexOf(PanelOnList[num]);
				PanelOnList[num].gameObject.SetActive(value: false);
				PanelOnList.RemoveAt(num);
				PanelOnFlagList.RemoveAt(num);
				LogShowingList[index] = null;
			}
		}
	}
}
