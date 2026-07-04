using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetLastTargetFilter : ISkillTargetFilter
{
	private readonly int _lastTargetIndex;

	public int LastTargetIndex => _lastTargetIndex;

	public SkillTargetLastTargetFilter(string option)
	{
		int.TryParse(option, out _lastTargetIndex);
	}

	public virtual IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		List<IReadOnlyBattleCardInfo> list2 = new List<IReadOnlyBattleCardInfo>();
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoClassAndInPlayCards));
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoHandCards));
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoDeckCards));
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoCemeterys));
		list2.AddRange(battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoBanishCards));
		List<List<IReadOnlyBattleCardInfo>> list3 = new List<List<IReadOnlyBattleCardInfo>>();
		if (battlePlayerInfos.Count() > 1)
		{
			List<List<IReadOnlyBattleCardInfo>> list4 = battlePlayerInfos.ToList()[0].SkillInfoLastTargets.ToList().ConvertAll((IEnumerable<IReadOnlyBattleCardInfo> c) => c.ToList());
			List<List<IReadOnlyBattleCardInfo>> list5 = battlePlayerInfos.ToList()[1].SkillInfoLastTargets.ToList().ConvertAll((IEnumerable<IReadOnlyBattleCardInfo> c) => c.ToList());
			for (int num = 0; num < list4.Count(); num++)
			{
				list4[num].AddRange(list5[num]);
			}
			list3 = list4;
		}
		else
		{
			list3 = battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo p) => p.SkillInfoLastTargets.ToList().ConvertAll((IEnumerable<IReadOnlyBattleCardInfo> c) => c.ToList())).ToList();
		}
		foreach (IReadOnlyBattleCardInfo card in (list3.Count > _lastTargetIndex) ? list3[_lastTargetIndex] : new List<IReadOnlyBattleCardInfo>())
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = list2.FirstOrDefault((IReadOnlyBattleCardInfo c) => c.EquelsID(card));
			if (readOnlyBattleCardInfo != null)
			{
				list.Add(readOnlyBattleCardInfo);
			}
		}
		return list;
	}
}
