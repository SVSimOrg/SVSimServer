using LitJson;

namespace Wizard;

public class ShopNotification
{
	public class ShopAppealInfo
	{
		public RemainTime RemainTime { get; private set; }

		public bool IsNew { get; private set; }

		public bool IsCollaborationPanel { get; private set; }

		public bool NeedsCampaignDisplay { get; set; }

		public bool NeedsFooterBadgeIcon { get; set; }

		public ShopAppealInfo(JsonData data, double serverTime)
		{
			if (data.Count != 0)
			{
				RemainTime = (data.Keys.Contains("sale_end_time") ? new RemainTime(data["sale_end_time"].ToString(), serverTime) : null);
				IsNew = data.Keys.Contains("is_new") && data["is_new"].ToBoolean();
				IsCollaborationPanel = data.Keys.Contains("is_collaboration_term") && data["is_collaboration_term"].ToBoolean();
				if (data.TryGetValue("is_open_free_gacha_campaign", out var value))
				{
					NeedsCampaignDisplay = value.ToBoolean();
				}
				if (data.TryGetValue("can_free_gacha", out var value2))
				{
					NeedsFooterBadgeIcon = value2.ToBoolean();
				}
			}
		}

		public ShopAppealInfo()
		{
			RemainTime = null;
			IsNew = false;
			IsCollaborationPanel = false;
		}
	}

	public ShopAppealInfo AppealCardPack { get; private set; }

	public ShopAppealInfo AppealBuildDeck { get; private set; }

	public ShopAppealInfo AppealSleeve { get; private set; }

	public ShopAppealInfo AppealLeaderSkin { get; private set; }

	public void SetShopBadgeEnable(JsonData data)
	{
		JsonData jsonData = data["data"]["shop_notification"];
		AppealCardPack = ((AppealCardPack != null) ? AppealCardPack : new ShopAppealInfo());
		AppealBuildDeck = ((AppealBuildDeck != null) ? AppealBuildDeck : new ShopAppealInfo());
		AppealSleeve = ((AppealSleeve != null) ? AppealSleeve : new ShopAppealInfo());
		AppealLeaderSkin = ((AppealLeaderSkin != null) ? AppealLeaderSkin : new ShopAppealInfo());
		AppealCardPack.NeedsFooterBadgeIcon = jsonData["card_pack"].ToBoolean();
		AppealBuildDeck.NeedsFooterBadgeIcon = jsonData["build_deck"].ToBoolean();
		AppealSleeve.NeedsFooterBadgeIcon = jsonData["sleeve"].ToBoolean();
		AppealLeaderSkin.NeedsFooterBadgeIcon = jsonData["leader_skin"].ToBoolean();
	}
}
