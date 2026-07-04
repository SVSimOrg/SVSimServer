using System.Collections.Generic;
using Wizard;
using Wizard.Scripts.Network.Data.TaskData.Arena;

public class PackOpenDetail
{
	public List<CardPack> pack_list;

	public List<Wizard.Scripts.Network.Data.TaskData.Arena.Reward> reward_list { get; set; }

	public List<NotificatonAnimation.Param> NotificatonAnimationParams { get; set; }

	public AchievedInfo AchievedInfo { get; set; }
}
