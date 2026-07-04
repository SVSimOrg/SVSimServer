using System.Collections.Generic;
using DeckBuilder;
using LitJson;
using UnityEngine;
using Wizard.Scripts.Network.Data.TableData.Ranking;
using Wizard.Scripts.Network.Data.TaskData;
using Wizard.Scripts.Network.Data.TaskData.Arena;
using Wizard.Scripts.Network.Data.TaskData.Arena.TwoPick;
using Wizard.Scripts.Network.Data.TaskData.Battle;
using Wizard.Scripts.Network.Data.TaskData.BuildDeckPurchase;
using Wizard.Scripts.Network.Data.TaskData.ItemPurchase;
using Wizard.Scripts.Network.Data.TaskData.Ranking;
using Wizard.Scripts.Network.Data.TaskData.SkinPurchase;
using Wizard.Scripts.Network.Data.TaskData.SleevePurchase;
using Wizard.Story;
// TODO(engine-cleanup-pass2): 156 of 165 methods unrun in baseline
//   Type: Wizard.Data
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public static class Data
{
	private static Format _currentFormat;

	public static Master Master { get; set; }

	public static Mastershop ShopMaster { get; set; }

	public static SystemText SystemText { get; set; }

	public static Load Load { get; set; }

	public static MyPage MyPage { get; set; }

	public static MyPageNotifications MyPageNotifications { get; set; }

	public static RankMatchFinish RankMatchFinish { get; set; }

	public static FreeMatchFinish FreeMatchFinish { get; set; }

	public static ColosseumBattleFinish ColosseumBattleFinish { get; set; }

	public static CompetitionBattleFinish CompetitionBattleFinish { get; set; }

	public static RedEtherCampaignResultData RedEtherCampaignResultData { get; set; }

	public static DoMatchingData DoMatchingDetail { get; set; }

	public static AIBattleStartData AIBattleStartData { get; set; }

	public static SelectedStoryInfo SelectedStoryInfo { get; set; }

	public static StoryWorldDataManager StoryWorldDataManager { get; set; }

	public static StoryInfo StoryInfo { get; set; }

	public static StoryFinish StoryFinish { get; set; }

	public static StoryLeaderSelect StoryLeaderSelect { get; set; }

	public static QuestMissionInfo QuestMissionInfo { get; set; }

	public static QuestFinish QuestFinish { get; set; }

	public static ArenaData ArenaData { get; set; }

	public static MailTop MailTop { get; set; }

	public static ReadMail ReadMail { get; set; }

	public static PackInfo PackInfo { get; set; }

	public static PackOpen PackOpen { get; set; }

	public static SleevePurchaseInfo SleevePurchaseInfo { get; set; }

	public static SkinPurchaseInfo SkinPurchaseInfo { get; set; }

	public static BuildDeckPurchaseInfo BuildDeckPurchaseInfo { get; set; }

	public static ItemPurchaseInfo ItemPurchaseInfo { get; set; }

	public static EmptyDeckInfo EmptyDeckInfo { get; set; }

	public static MissionInfo MissionInfo { get; set; }

	public static AchievementInfo AchievementInfo { get; set; }

	public static User User { get; set; }

	public static UserConfig UserConfig { get; set; }

	public static UserTutorial UserTutorial { get; set; }

	public static FriendInfo FriendInfo { get; set; }

	public static PlayedTogetherInfo PlayedTogetherInfo { get; set; }

	public static ReceiveFriendApplyInfo ReceiveFriendApplyInfo { get; set; }

	public static SendFriendApplyInfo SendFriendApplyInfo { get; set; }

	public static EmblemInfo EmblemInfo { get; set; }

	public static DegreeInfo DegreeInfo { get; set; }

	public static SearchUserInfo SearchUserInfo { get; set; }

	public static RankingPeriodList RankingPeriodList { get; set; }

	public static Dictionary<Format, MonthlyRanking> RankingRankMatchClassInfo { get; set; }

	public static Dictionary<Format, MonthlyRanking> RankingMasterInfo { get; set; }

	public static MonthlyRanking RankingTwoPickInfo { get; set; }

	public static MonthlyRanking RankingSealedInfo { get; set; }

	public static Dictionary<int, MyMasterPointHistories> RankingMasterMyHistories { get; set; }

	public static TwoPickInfo TwoPickInfo { get; set; }

	public static Entry TwoPickEntry { get; set; }

	public static RoomTwoPickInfo RoomTwoPickInfo { get; set; }

	public static RoomTwoPickBeforeBattleInfo RoomTwoPickBeforeBattleInfo { get; set; }

	public static RoomTwoPickMultiDeckInfo RoomTwoPickMultiDeckInfo { get; set; }

	public static DoMatchingResponse TwoPickDoMatching { get; set; }

	public static Finish ArenaBattleFinish { get; set; }

	public static ItemAcquireHistoryInfo ItemAcquireHistoryInfo { get; set; }

	public static PracticeDataMgr PracticeDataMgr { get; set; }

	public static PracticeFinish PracticeFinish { get; set; }

	public static PracticePuzzleFinishData PracticePuzzleFinishData { get; set; }

	public static GenerateDeckCode GenerateDeckCode { get; set; }

	public static InitializeRoomBattle InitializeRoomBattle { get; set; }

	public static ReplayInfo ReplayInfo { get; set; }

	public static ReplayDetailInfo ReplayBattleInfo { get; set; }

	public static BattleRecoveryInfo BattleRecoveryInfo
	{
		// Instance-backed via BattleManagerBase.InstanceRecoveryInfo (Phase 5, chunk 40 — ambient
		// fallback dropped after chunk 39 converted the last direct consumer).
		get => BattleManagerBase.GetIns()?.InstanceRecoveryInfo;
		set {
			var m = BattleManagerBase.GetIns();
			if (m is not null) m.InstanceRecoveryInfo = value;
		}
	}

	public static List<DeckGroup> DeckGroupDataBase { get; set; } = new List<DeckGroup>();

	public static List<NetworkDefine.MAINTENANCE_TYPE> MaintenanceCodeList { get; private set; } = null;

	public static Crossover Crossover { get; private set; } = null;

	public static MyRotationAllInfo MyRotationAllInfo { get; set; } = null;

	public static AvatarBattleAllInfo AvatarBattleAllInfo { get; set; } = null;

	public static TreasureBoxCp TreasureBoxCp { get; set; } = null;

	public static Format CurrentFormat
	{
		get
		{
			return _currentFormat;
		}
		set
		{
			if (value == Format.Hof)
			{
				_currentFormat = Format.Max;
			}
			else
			{
				_currentFormat = value;
			}
		}
	}

	public static void Clear()
	{
		Master = null;
		ShopMaster = null;
		SystemText = null;
		Load = null;
		MyPage = null;
		MyPageNotifications = null;
		RankMatchFinish = null;
		FreeMatchFinish = null;
		AIBattleStartData = null;
		DoMatchingDetail = null;
		SelectedStoryInfo = null;
		StoryWorldDataManager = null;
		StoryInfo = null;
		StoryFinish = null;
		QuestMissionInfo = null;
		QuestFinish = null;
		ArenaData = null;
		MailTop = null;
		ReadMail = null;
		PackInfo = null;
		PackOpen = null;
		SleevePurchaseInfo = null;
		SkinPurchaseInfo = null;
		BuildDeckPurchaseInfo = null;
		ItemPurchaseInfo = null;
		EmptyDeckInfo = null;
		FriendInfo = null;
		PlayedTogetherInfo = null;
		ReceiveFriendApplyInfo = null;
		SendFriendApplyInfo = null;
		User = null;
		UserConfig = null;
		UserTutorial = null;
		MissionInfo = null;
		AchievementInfo = null;
		EmblemInfo = null;
		DegreeInfo = null;
		SearchUserInfo = null;
		RankingPeriodList = null;
		RankingRankMatchClassInfo = null;
		RankingMasterInfo = null;
		RankingTwoPickInfo = null;
		RankingSealedInfo = null;
		RankingMasterMyHistories = null;
		TwoPickInfo = null;
		TwoPickEntry = null;
		TwoPickDoMatching = null;
		ArenaBattleFinish = null;
		RoomTwoPickInfo = null;
		RoomTwoPickBeforeBattleInfo = null;
		RoomTwoPickMultiDeckInfo = null;
		ItemAcquireHistoryInfo = null;
		PracticeDataMgr = null;
		PracticeFinish = null;
		PracticePuzzleFinishData = null;
		InitializeRoomBattle = null;
		ReplayInfo = null;
		CurrentFormat = Format.Max;
		BattleRecoveryInfo = null;
		MaintenanceCodeList = null;
		DeckGroupDataBase = new List<DeckGroup>();
		Crossover = null;
		MyRotationAllInfo = null;
		TreasureBoxCp = null;
		CardMaster.DeleteAllInstance();
		PlayerStaticData.Clear();
		Prerelease.Clear();
		DeckGroupDataBase.Clear();
	}

	public static int FormatConvertApi(Format format)
	{
		switch (format)
		{
		case Format.Rotation:
		case Format.Max:
			return 1;
		case Format.Unlimited:
			return 2;
		case Format.PreRotation:
			return 3;
		case Format.Crossover:
			return 4;
		case Format.Sealed:
			return 20;
		case Format.TwoPick:
			return 10;
		case Format.Hof:
			return 31;
		case Format.All:
			return 0;
		case Format.Windfall:
			return 33;
		case Format.MyRotation:
		case Format.Avatar:
			return (int)format;
		default:
			UnityEngine.Debug.LogWarning("不明なフォーマットが指定されています：" + format);
			return 1;
		}
	}

	public static Format ParseApiFormat(int format)
	{
		switch (format)
		{
		case 1:
			return Format.Rotation;
		case 2:
			return Format.Unlimited;
		case 3:
			return Format.PreRotation;
		case 4:
			return Format.Crossover;
		case 10:
			return Format.TwoPick;
		case 20:
			return Format.Sealed;
		case 31:
			return Format.Hof;
		case 33:
			return Format.Windfall;
		case 5:
		case 39:
			return (Format)format;
		case 0:
			return Format.Max;
		default:
			UnityEngine.Debug.LogWarning("不明なフォーマットが指定されています：" + format);
			return Format.Max;
		}
	}

	public static void UpdateMaintenance(List<NetworkDefine.MAINTENANCE_TYPE> checkList, List<NetworkDefine.MAINTENANCE_TYPE> maintenanceList)
	{
		for (int i = 0; i < checkList.Count; i++)
		{
			MaintenanceCodeList.Remove(checkList[i]);
			if (maintenanceList.Contains(checkList[i]))
			{
				MaintenanceCodeList.Add(checkList[i]);
			}
		}
	}
}
