using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillIsFusionInfoOrIsNotSideLogFilter : ISkillCardFilter
{
	private bool _flag;

	public SkillIsFusionInfoOrIsNotSideLogFilter(bool flag)
	{
		_flag = flag;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < cards.Count(); i++)
		{
			BattleCardBase battleCardBase = cards.ElementAt(i) as BattleCardBase;
			if (battleCardBase.SelfBattlePlayer != null)
			{
				IDetailPanelControl detailPanelControl = battleCardBase.SelfBattlePlayer.BattleMgr.DetailMgr.DetailPanelControl;
				bool flag = ((detailPanelControl._card.EquelsID(battleCardBase) && detailPanelControl.CurrentShowRequest == DetailPanelControl.ShowRequest.FUSION_INFO_CARD_LIST) || battleCardBase.IsRecordingFusionInfo || battleCardBase.IsInHand) && battleCardBase.SelfBattlePlayer.SideLogSkill == null;
				if (_flag == flag)
				{
					list.Add(battleCardBase);
				}
			}
		}
		return list;
	}
}
