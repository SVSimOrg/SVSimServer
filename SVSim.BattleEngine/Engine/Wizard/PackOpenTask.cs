using System.Collections.Generic;
using LitJson;
using Wizard.Scripts.Network.Data.TaskData.Arena;

namespace Wizard;

public class PackOpenTask : BaseTask
{
	public class PackOpenTaskParam : BaseParam
	{
	}

	public class PackOpenStarterPackTaskParam : BaseParam
	{

		public int class_id;
	}

	public class PackOpenAcquireSkinCardPackTaskParam : BaseParam
	{
	}

	private PackConfig _packConfig;

	public bool IsSpecialEffect;

	public bool IsExistFreePackLeaderSkin { get; private set; }

	public PackOpenTask(bool isTutorial, PackConfig packConfig)
	{
		base.type = (isTutorial ? ApiType.Type.PackOpenTutorial : ApiType.Type.PackOpen);
		_packConfig = packConfig;
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		if (Data.PackOpen.data == null)
		{
			Data.PackOpen.data = new PackOpenDetail();
			Data.PackOpen.data.pack_list = new List<CardPack>();
			Data.PackOpen.data.reward_list = new List<Wizard.Scripts.Network.Data.TaskData.Arena.Reward>();
			Data.PackOpen.data.NotificatonAnimationParams = new List<NotificatonAnimation.Param>();
			Data.PackOpen.data.AchievedInfo = new AchievedInfo();
		}
		else
		{
			Data.PackOpen.data.pack_list.Clear();
			Data.PackOpen.data.reward_list.Clear();
			if (Data.PackOpen.data.NotificatonAnimationParams != null)
			{
				Data.PackOpen.data.NotificatonAnimationParams.Clear();
			}
			Data.PackOpen.data.AchievedInfo = new AchievedInfo();
		}
		if (base.ResponseData["data"].Keys.Contains("mission_result"))
		{
			Data.PackOpen.data.AchievedInfo = new AchievedInfo(base.ResponseData["data"]["mission_result"]);
			Data.PackOpen.data.NotificatonAnimationParams = new List<NotificatonAnimation.Param>();
			for (int i = 0; i < Data.PackOpen.data.AchievedInfo._missions.Count; i++)
			{
				string achieved_message = Data.PackOpen.data.AchievedInfo._missions[i].achieved_message;
				Data.PackOpen.data.NotificatonAnimationParams.Add(new NotificatonAnimation.Param(NotificatonAnimation.Param.Type.GachaResult, achieved_message));
			}
		}
		PlayerStaticData.UpdateHaveUserGoodsNumByJsonData(base.ResponseData["data"]["reward_list"]);
		JsonData jsonData = base.ResponseData["data"]["pack_list"];
		int count = jsonData.Count;
		for (int j = 0; j < count; j++)
		{
			JsonData jsonData2 = jsonData[j];
			CardPack cardPack = new CardPack();
			cardPack.card_id = (int)jsonData2["card_id"];
			cardPack.rarity = (int)jsonData2["rarity"];
			cardPack.SleeveId = _packConfig.SleeveId;
			cardPack.IsFreePackLeaderSkin = jsonData2.GetValueOrDefault("special_effect", 0) == 1;
			if (cardPack.IsFreePackLeaderSkin)
			{
				IsExistFreePackLeaderSkin = true;
			}
			Data.PackOpen.data.pack_list.Add(cardPack);
		}
		if (_packConfig.IsSpecialCardPack && _packConfig.PackId >= 90001)
		{
			CardPack cardPack2 = Data.PackOpen.data.pack_list[count - 1];
			cardPack2.SleeveId = _packConfig.SpecialSleeveId;
			cardPack2.IsSpecialCard = true;
		}
		JsonData jsonData3 = base.ResponseData["data"]["rewards"];
		for (int k = 0; k < jsonData3.Count; k++)
		{
			Wizard.Scripts.Network.Data.TaskData.Arena.Reward item = new Wizard.Scripts.Network.Data.TaskData.Arena.Reward(jsonData3[k]);
			Data.PackOpen.data.reward_list.Add(item);
		}
		IsSpecialEffect = base.ResponseData["data"].GetValueOrDefault("is_special_effect", defaultValue: false);
		Data.Load.data._userTutorial.Update(base.ResponseData["data"]);
		return num;
	}
}
