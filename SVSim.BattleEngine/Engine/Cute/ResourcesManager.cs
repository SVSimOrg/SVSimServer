using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

namespace Cute;

public class ResourcesManager : MonoBehaviour, IManager
{
	public enum AssetLoadPathType
	{
		Master,
		MasterEtc,
		CharaMaster,
		Tutorial,
		StoryTutorial,
		StoryParam,
		StoryParamDiff,
		StoryText,
		StoryMovieSubtitles,
		StoryCharacter,
		StoryBackground,
		StoryEndingStill,
		StorySubChapterBtnTexture,
		StoryChapterHeader,
		StorySelectWorldPanel,
		HandCard,
		HandCardSpecular,
		CardFrame,
		CardFrameClassIcon,
		CardDeco,
		SleeveTexture,
		SleeveTextureMask,
		SleeveTextureBasePath,
		SleeveTextureAORefSpec,
		SleeveTextureNormalMap,
		SleeveMaterial,
		SleeveMesh,
		SleeveSpecular,
		CardFrameMesh,
		CardFrameMaterial,
		CardFrameMaterialPlus,
		CardFrameTextureCommon,
		FoilTextures,
		UnitCardMaterial,
		UnitCardTextures,
		SpellCardMaterial,
		SpellCardTextures,
		Effect2D,
		Effect2DMaterials,
		Effect3D,
		Effect3DAnim,
		Effect3DMat,
		Emblem_S,
		Emblem_M,
		Degree_S,
		Degree_M,
		DegreeMaterial_S,
		DegreeMaterial_M,
		DegreeMask,
		RankIcon_S,
		RankIcon_L,
		Country_S,
		Country_M,
		Font,
		DeckListTexture,
		DeckEditBGTexture,
		ClassCharaSpine,
		ClassCharaBase,
		ClassCharaBaseWin,
		ClassCharaBaseLose,
		ClassCharaEvolve,
		ClassCharaEvolveWin,
		ClassCharaEvolveLose,
		ClassCharaHeader,
		ClassCharaSkinThumbnail,
		ClassCharaWideThumbnail,
		ClassCharaButton,
		ClassCharaIconLevel,
		ClassCharaEncampment,
		ClassCharaMesh,
		ClassCharaMaterial,
		ClassCharaMaterialMain,
		ClassCharaTexture,
		ClassCharaFrameTexture,
		ClassCharaProfile,
		ClassCharaStory,
		ClassChara3D,
		AreaSelectTexture,
		GachaTexture,
		ShopCardPack,
		ShopBuildDeckThumbnail,
		ShopBuildDeck,
		ShopSleeve,
		ShopClassSkin,
		ShopItem,
		BattleTexture,
		BattleLangTexture,
		Field3D,
		Background,
		CardMenu,
		BossRushSpecialCard,
		FirstTips,
		Arena,
		Item,
		Lottery,
		UiOtherTexture,
		UilangOtherTexture,
		UiStory,
		UiStorySectionSummary,
		PackBox,
		UnitHeader,
		OtherHeader,
		UiDownLoad,
		UiDownLoadInfo,
		SpecialCrystal,
		GrandPrixSpecialAtlas,
		QuestAtlas,
		Stamp,
		Uilang3DField,
		BattlePass,
		BattleStage,
		Bingo,
		PracticePuzzleThumbnail,
		NeutralVote,
		NeutralVoteThumbnail_S,
		NeutralVoteThumbnail_M,
		LeaderVote,
		LeaderVoteThumbnail_S,
		UISpine,
		FlowChart,
		MyPageBackGround,
		MyPageCharacter,
		MyPageCharacterMask,
		MyPageBackGroundIcon,
		MyPageBackGroundRandomSelectIcon,
		Competition,
		FreeCardPack,
		SpecialTitle
	}

	private enum ProgressDebugType
	{
		Progress_NONE,
		Progress_LOAD
	}

	public enum BackgroundDownloadState
	{
		None,
		StopRequest
	}

	public enum StopResult
	{
	}

	public List<string> Card2DAssetPathList = new List<string>();

	public List<string> CardListAssetPathList = new List<string>();

	public List<string> BattleListAssetPathList = new List<string>();

	private bool loadSemaphore;

	private AssetRequestContext loadRequestContext = new AssetRequestContext(null, new Utility.LeanSemaphore(0), new AssetErrorState());

	public static int fixedParallelTasks;

	private int _preferredParallelLoadTasks = -1;

	public BackgroundDownloadState BgDownloadState;

	public string GetAssetTypePath(string path, AssetLoadPathType type, bool isfetch = false)
	{
		if (string.IsNullOrEmpty(path))
		{
			return "";
		}
		string result = "";
		int result2 = 0;
		if (isfetch)
		{
			switch (type)
			{
			case AssetLoadPathType.Master:
				result = "Master/" + path;
				break;
			case AssetLoadPathType.MasterEtc:
				result = "Master/etc/" + path;
				break;
			case AssetLoadPathType.CharaMaster:
				result = "Master/chara/" + path;
				break;
			case AssetLoadPathType.Tutorial:
				result = "Tutorial/TutorialTextureAtlas/" + path;
				break;
			case AssetLoadPathType.StoryTutorial:
				result = "Storylang/StoryTutorialTextureAtlas/" + path;
				break;
			case AssetLoadPathType.StoryParam:
				result = "Story/Param/" + path;
				break;
			case AssetLoadPathType.StoryParamDiff:
				result = "Storylang/ParamDiff/" + path;
				break;
			case AssetLoadPathType.StoryText:
				result = "Storylang/Text/" + path;
				break;
			case AssetLoadPathType.StoryMovieSubtitles:
				result = "Storylang/MovieSubtitles/" + path;
				break;
			case AssetLoadPathType.StoryCharacter:
				result = "Story/Character/" + path;
				break;
			case AssetLoadPathType.StoryBackground:
				result = "Story/Background/" + path;
				break;
			case AssetLoadPathType.StoryEndingStill:
				result = "Story/EndingStill/" + path;
				break;
			case AssetLoadPathType.HandCard:
				result = "Card/CardFrame/HandCard/" + path;
				break;
			case AssetLoadPathType.HandCardSpecular:
				result = "Card/CardFrame/HandCardSpecular/" + path;
				break;
			case AssetLoadPathType.CardFrame:
				result = "Card/CardFrame/" + path;
				break;
			case AssetLoadPathType.CardFrameClassIcon:
				result = "Card/CardFrame/ClassIcon/" + path;
				break;
			case AssetLoadPathType.CardFrameMesh:
				result = "Card/Common/Frame/Meshes/" + path;
				break;
			case AssetLoadPathType.CardFrameMaterial:
				result = "Card/Common/Frame/" + path + "/" + path;
				break;
			case AssetLoadPathType.CardFrameMaterialPlus:
				result = "Card/Common/Frame/Materials/" + path;
				break;
			case AssetLoadPathType.CardFrameTextureCommon:
				result = "Card/Common/Frame/Textures/" + path;
				break;
			case AssetLoadPathType.FoilTextures:
				result = "Card/Common/Foil/Textures/" + path;
				break;
			case AssetLoadPathType.UnitCardMaterial:
				result = "Card/Field/Materials/" + path;
				break;
			case AssetLoadPathType.UnitCardTextures:
				result = "Card/Field/Textures/" + path;
				break;
			case AssetLoadPathType.SpellCardMaterial:
				result = "Card/Spell/Materials/" + path;
				break;
			case AssetLoadPathType.SpellCardTextures:
				result = "Card/Spell/Textures/" + path;
				break;
			case AssetLoadPathType.UnitHeader:
				result = "Card/Field/Header/log_" + path;
				break;
			case AssetLoadPathType.OtherHeader:
				result = "Card/Spell/Header/log_" + path;
				break;
			case AssetLoadPathType.Effect2D:
				result = "Effect/Effects/" + path;
				break;
			case AssetLoadPathType.Effect2DMaterials:
				result = "Effect/Images/Materials/" + path;
				break;
			case AssetLoadPathType.Effect3D:
				result = "Effect/FBX/" + path;
				break;
			case AssetLoadPathType.Effect3DAnim:
				result = "Effect/FBX/" + path + "/Motions/";
				break;
			case AssetLoadPathType.Effect3DMat:
				result = "Effect/FBX/" + path + "/Materials/";
				break;
			case AssetLoadPathType.Emblem_S:
				result = "Ui/Emblem/S/" + Data.Master.EmblemMgr.GetResourcePath(long.Parse(path)) + "_s";
				break;
			case AssetLoadPathType.Emblem_M:
				result = "Ui/Emblem/M/" + Data.Master.EmblemMgr.GetResourcePath(long.Parse(path)) + "_m";
				break;
			case AssetLoadPathType.Degree_S:
				result = "Uilang/Degree/S/" + Data.Master.DegreeMgr.GetResourcePath(int.Parse(path)) + "_s";
				break;
			case AssetLoadPathType.DegreeMaterial_S:
				result = "Uilang/Degree/Materials/" + Data.Master.DegreeMgr.GetResourcePath(int.Parse(path)) + "_s_M";
				break;
			case AssetLoadPathType.Degree_M:
				result = "Uilang/Degree/M/" + Data.Master.DegreeMgr.GetResourcePath(int.Parse(path)) + "_m";
				break;
			case AssetLoadPathType.DegreeMaterial_M:
				result = "Uilang/Degree/Materials/" + Data.Master.DegreeMgr.GetResourcePath(int.Parse(path)) + "_m_M";
				break;
			case AssetLoadPathType.DegreeMask:
				result = "Uilang/Degree/Masks/" + Data.Master.DegreeMgr.GetMaskTexturePath(int.Parse(path));
				break;
			case AssetLoadPathType.RankIcon_S:
				result = "Uilang/RankIcon/S/icon_rank_" + path + "_s";
				break;
			case AssetLoadPathType.RankIcon_L:
				result = "Uilang/RankIcon/L/icon_rank_" + path + "_l";
				break;
			case AssetLoadPathType.Country_S:
				result = "Ui/Country/S/country_" + path + "_s";
				break;
			case AssetLoadPathType.Country_M:
				result = "Ui/Country/M/country_" + path + "_m";
				break;
			case AssetLoadPathType.DeckListTexture:
				result = $"Ui/TextureAtlas/DeckListTextureAtlas/btn_deck_{int.Parse(path):D2}";
				break;
			case AssetLoadPathType.DeckEditBGTexture:
				result = "Ui/TextureAtlas/DeckListTextureAtlas/" + path;
				break;
			case AssetLoadPathType.ClassCharaSpine:
				result = "Ui/ClassChar/Prefab/class_" + path;
				break;
			case AssetLoadPathType.ClassCharaBase:
				result = ((!int.TryParse(path, out result2)) ? $"Ui/ClassChar/Textures/Base/class_{path}_base" : $"Ui/ClassChar/Textures/Base/class_{result2:D2}_base");
				break;
			case AssetLoadPathType.ClassCharaBaseWin:
				result = "Ui/ClassChar/Textures/Base/class_" + path + "_base_win";
				break;
			case AssetLoadPathType.ClassCharaBaseLose:
				result = "Ui/ClassChar/Textures/Base/class_" + path + "_base_lose";
				break;
			case AssetLoadPathType.ClassCharaEvolve:
				result = ((!int.TryParse(path, out result2)) ? $"Ui/ClassChar/Textures/Base/class_{path}_base_evolve" : $"Ui/ClassChar/Textures/Base/class_{result2:D2}_base_evolve");
				break;
			case AssetLoadPathType.ClassCharaEvolveWin:
				result = "Ui/ClassChar/Textures/Base/class_" + path + "_base_evolve_win";
				break;
			case AssetLoadPathType.ClassCharaEvolveLose:
				result = "Ui/ClassChar/Textures/Base/class_" + path + "_base_evolve_lose";
				break;
			case AssetLoadPathType.ClassCharaHeader:
				result = "Ui/ClassChar/Textures/Header/" + path;
				break;
			case AssetLoadPathType.ClassCharaSkinThumbnail:
				result = $"Ui/ClassChar/Textures/SkinThumbnail/class_skin_{int.Parse(path):D2}";
				break;
			case AssetLoadPathType.ClassCharaWideThumbnail:
			{
				int result5 = 0;
				result = ((!int.TryParse(path, out result5)) ? $"Ui/ClassChar/Textures/WideThumbnail/chara_select_thumbnail_wide_class_{path}" : $"Ui/ClassChar/Textures/WideThumbnail/chara_select_thumbnail_wide_class_{result5:D2}");
				break;
			}
			case AssetLoadPathType.ClassCharaButton:
			{
				int result4 = 0;
				result = ((!int.TryParse(path, out result4)) ? $"Ui/ClassChar/Textures/Button/class_select_thumbnail_{path}" : $"Ui/ClassChar/Textures/Button/class_select_thumbnail_{result4:D2}");
				break;
			}
			case AssetLoadPathType.PracticePuzzleThumbnail:
			{
				result = ((!int.TryParse(path, out var result3)) ? $"Ui/Puzzle/Thumbnail/puzzle_thumbnail_{path}" : $"Ui/Puzzle/Thumbnail/puzzle_thumbnail_{result3:D2}");
				break;
			}
			case AssetLoadPathType.ClassCharaIconLevel:
				result = "Ui/ClassChar/Textures/Icon/class_" + path + "_level";
				break;
			case AssetLoadPathType.ClassCharaEncampment:
				result = "Ui/ClassChar/Textures/Encampment/class_" + path + "_encampment";
				break;
			case AssetLoadPathType.ClassCharaMesh:
				result = "Ui/ClassChar/Meshes/" + path;
				break;
			case AssetLoadPathType.ClassCharaMaterial:
				result = "Ui/ClassChar/Materials/" + path;
				break;
			case AssetLoadPathType.ClassCharaMaterialMain:
				result = "Ui/ClassChar/Materials/Main/" + path;
				break;
			case AssetLoadPathType.ClassCharaTexture:
				result = "Ui/ClassChar/Textures/" + path;
				break;
			case AssetLoadPathType.ClassCharaFrameTexture:
				result = "Ui/ClassChar/Textures/Frame/" + path;
				break;
			case AssetLoadPathType.ClassCharaProfile:
				result = "Ui/ClassChar/Textures/Profile/" + path;
				break;
			case AssetLoadPathType.ClassCharaStory:
				result = $"Ui/ClassChar/Textures/Story/class_{int.Parse(path):D2}_story";
				break;
			case AssetLoadPathType.ClassChara3D:
				result = "Ui/Class3dChar/Prehab/" + path + "/chr" + path;
				break;
			case AssetLoadPathType.AreaSelectTexture:
				result = "Ui/TextureAtlas/AreaSelectTextureAtlas/" + path;
				break;
			case AssetLoadPathType.GachaTexture:
				result = "Ui/TextureAtlas/GachaTextureAtlas/" + path;
				break;
			case AssetLoadPathType.ShopCardPack:
				result = "Uilang/Shop/CardPack/" + path;
				break;
			case AssetLoadPathType.ShopBuildDeck:
				result = "Uilang/Shop/BuildDeck/" + path;
				break;
			case AssetLoadPathType.ShopBuildDeckThumbnail:
				result = "Uilang/Shop/BuildDeck/build_deck_thumbnail_" + path;
				break;
			case AssetLoadPathType.ShopSleeve:
				result = "Uilang/Shop/Sleeve/" + path;
				break;
			case AssetLoadPathType.ShopClassSkin:
				result = "Uilang/Shop/ClassSkin/" + path;
				break;
			case AssetLoadPathType.ShopItem:
				result = "Uilang/Shop/Ticket/" + path;
				break;
			case AssetLoadPathType.CardDeco:
				result = "Sleeve/CardDeco/" + path;
				break;
			case AssetLoadPathType.SleeveTexture:
				result = "Sleeve/Textures/" + Data.Master.SleeveMgr.GetResourcePath(long.Parse(path)).ToString();
				break;
			case AssetLoadPathType.SleeveTextureMask:
			{
				Sleeve sleeve = Data.Master.SleeveMgr.Get(long.Parse(path));
				result = ((!sleeve.IsPremiumSleeve) ? string.Empty : ("Sleeve/Masks/sleeve_mask_" + sleeve.sleeve_id));
				break;
			}
			case AssetLoadPathType.SleeveTextureBasePath:
				result = "Sleeve/Textures/" + path;
				break;
			case AssetLoadPathType.SleeveTextureAORefSpec:
				result = "Sleeve/Textures/sleeve_" + DataMgr.GetAbleSleeveId(long.Parse(path)) + "_a";
				break;
			case AssetLoadPathType.SleeveTextureNormalMap:
				result = "Sleeve/Textures/sleeve_" + DataMgr.GetAbleSleeveId(long.Parse(path)) + "_n";
				break;
			case AssetLoadPathType.SleeveMaterial:
				result = "Sleeve/Materials/sleeve_" + DataMgr.GetAbleSleeveId(long.Parse(path)) + "_M";
				break;
			case AssetLoadPathType.SleeveMesh:
				result = "Sleeve/Meshes/" + path;
				break;
			case AssetLoadPathType.SleeveSpecular:
				result = "Sleeve/Meshes/Materials/" + path;
				break;
			case AssetLoadPathType.BattleTexture:
				result = "Ui/TextureAtlas/BattleTextureAtlas/" + path;
				break;
			case AssetLoadPathType.BattleLangTexture:
				result = "Uilang/TextureAtlas/BattleLangTextureAtlas/" + path;
				break;
			case AssetLoadPathType.Field3D:
				result = "Bg/" + path + "/_Battle" + path;
				break;
			case AssetLoadPathType.Font:
				result = "Ui/Fonts/" + path;
				break;
			case AssetLoadPathType.Background:
				result = "Ui/Background/" + path;
				break;
			case AssetLoadPathType.CardMenu:
				result = "Ui/CardMenu/" + path;
				break;
			case AssetLoadPathType.BossRushSpecialCard:
				result = "Ui/BossRush/boss_rush_" + path;
				break;
			case AssetLoadPathType.FirstTips:
				result = "Uilang/FirstTips/" + path;
				break;
			case AssetLoadPathType.Arena:
				result = "Ui/TextureAtlas/ArenaTextureAtlas/" + path;
				break;
			case AssetLoadPathType.Item:
				result = "Ui/Item/" + path;
				break;
			case AssetLoadPathType.Lottery:
				result = "Ui/Lottery/" + path;
				break;
			case AssetLoadPathType.UiOtherTexture:
				result = "Ui/Other/" + path;
				break;
			case AssetLoadPathType.UilangOtherTexture:
				result = "Uilang/Other/" + path;
				break;
			case AssetLoadPathType.UiStory:
				result = "Ui/Story/" + path;
				break;
			case AssetLoadPathType.UiStorySectionSummary:
				result = "Ui/Story/SectionSummary/" + path;
				break;
			case AssetLoadPathType.StorySubChapterBtnTexture:
				result = "Ui/Story/StorySelect/" + path;
				break;
			case AssetLoadPathType.StoryChapterHeader:
				result = "Ui/Story/ChapterHeader/" + path;
				break;
			case AssetLoadPathType.StorySelectWorldPanel:
				result = "Ui/Story/StorySelectWorldPanel/" + path;
				break;
			case AssetLoadPathType.BattlePass:
				result = "Ui/BattlePass/" + path;
				break;
			case AssetLoadPathType.BattleStage:
				result = "Ui/BattleStage/" + path;
				break;
			case AssetLoadPathType.PackBox:
				result = "PackBox/prefab/" + path;
				break;
			case AssetLoadPathType.UiDownLoad:
				result = "UiDownload/" + path;
				break;
			case AssetLoadPathType.UiDownLoadInfo:
				result = "UiDownload/Info/" + path;
				break;
			case AssetLoadPathType.SpecialCrystal:
				result = "Uilang/LegendCrystal/" + path;
				break;
			case AssetLoadPathType.GrandPrixSpecialAtlas:
				result = "Ui/TextureAtlas/GrandPrixSpecial/" + path;
				break;
			case AssetLoadPathType.QuestAtlas:
				result = "Ui/TextureAtlas/Quest/Quest";
				break;
			case AssetLoadPathType.Stamp:
				result = "Uilang/Stamp/stamp_" + path;
				break;
			case AssetLoadPathType.Uilang3DField:
				result = "Uilang/3DField/" + path;
				break;
			case AssetLoadPathType.Bingo:
				result = "Ui/Bingo/" + path;
				break;
			case AssetLoadPathType.NeutralVote:
				result = "Ui/PopularityVote/Neutral/neutral_vote_" + path;
				break;
			case AssetLoadPathType.NeutralVoteThumbnail_S:
				result = "Ui/PopularityVote/Neutral/Thumbnail_S/neutral_vote_thumb_" + path + "_s";
				break;
			case AssetLoadPathType.NeutralVoteThumbnail_M:
				result = "Ui/PopularityVote/Neutral/Thumbnail_M/neutral_vote_thumb_" + path + "_m";
				break;
			case AssetLoadPathType.LeaderVote:
				result = "Ui/PopularityVote/Leader/leader_vote_" + path;
				break;
			case AssetLoadPathType.LeaderVoteThumbnail_S:
				result = "Ui/PopularityVote/Leader/Thumbnail_S/leader_vote_thumb_" + path + "_s";
				break;
			case AssetLoadPathType.UISpine:
				result = "Ui/UISpine/Prefab/" + path.Replace("_body", "") + "/" + path;
				break;
			case AssetLoadPathType.FlowChart:
				result = "Ui/FlowChart/" + path;
				break;
			case AssetLoadPathType.MyPageBackGround:
				result = "Ui/MyPageBackGround/bg_mypage_" + path;
				break;
			case AssetLoadPathType.MyPageCharacter:
				result = "Ui/MyPageCharacter/" + path + "/" + path + "_mypage";
				break;
			case AssetLoadPathType.MyPageCharacterMask:
				result = "Ui/MyPageCharacter/" + path + "/" + path + "_mypage_mask";
				break;
			case AssetLoadPathType.MyPageBackGroundIcon:
				result = "Ui/MyPageBackGroundIcon/mypage_bg_icon_" + path;
				break;
			case AssetLoadPathType.MyPageBackGroundRandomSelectIcon:
				result = "Ui/MyPageBackGroundIcon/mypage_bg_random_icon_" + path;
				break;
			case AssetLoadPathType.Competition:
				result = "Ui/Competition/" + path;
				break;
			case AssetLoadPathType.FreeCardPack:
				result = "Ui/FreeCardPack/" + path;
				break;
			case AssetLoadPathType.SpecialTitle:
				result = "Ui/SpecialTitle/" + path + "/special_title_" + path;
				break;
			}
		}
		else
		{
			switch (type)
			{
			case AssetLoadPathType.Master:
			case AssetLoadPathType.MasterEtc:
			case AssetLoadPathType.CharaMaster:
				result = "master_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.StoryParam:
			case AssetLoadPathType.StoryCharacter:
			case AssetLoadPathType.StoryBackground:
			case AssetLoadPathType.StoryEndingStill:
				result = "story_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.StoryTutorial:
			case AssetLoadPathType.StoryParamDiff:
			case AssetLoadPathType.StoryText:
			case AssetLoadPathType.StoryMovieSubtitles:
				result = "storylang_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.HandCard:
			case AssetLoadPathType.HandCardSpecular:
			case AssetLoadPathType.CardFrame:
			case AssetLoadPathType.CardFrameClassIcon:
			case AssetLoadPathType.CardDeco:
			case AssetLoadPathType.SleeveTextureBasePath:
			case AssetLoadPathType.SleeveMesh:
			case AssetLoadPathType.SleeveSpecular:
			case AssetLoadPathType.CardFrameMesh:
			case AssetLoadPathType.CardFrameMaterial:
			case AssetLoadPathType.CardFrameMaterialPlus:
			case AssetLoadPathType.CardFrameTextureCommon:
			case AssetLoadPathType.FoilTextures:
			case AssetLoadPathType.UnitHeader:
			case AssetLoadPathType.OtherHeader:
				result = "card_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.SleeveMaterial:
				result = "card_sleeve_" + Data.Master.SleeveMgr.Get(long.Parse(path)).sleeve_id + "_m.unity3d";
				break;
			case AssetLoadPathType.SleeveTexture:
				result = "card_sleeve_" + Data.Master.SleeveMgr.Get(long.Parse(path)).sleeve_id + ".unity3d";
				break;
			case AssetLoadPathType.SleeveTextureMask:
			{
				Sleeve sleeve2 = Data.Master.SleeveMgr.Get(long.Parse(path));
				result = ((!sleeve2.IsPremiumSleeve) ? string.Empty : ("card_sleeve_mask_" + sleeve2.sleeve_id + ".unity3d"));
				break;
			}
			case AssetLoadPathType.SleeveTextureAORefSpec:
				result = "card_sleeve_" + DataMgr.GetAbleSleeveId(long.Parse(path)) + "_a.unity3d";
				break;
			case AssetLoadPathType.SleeveTextureNormalMap:
				result = "card_sleeve_" + DataMgr.GetAbleSleeveId(long.Parse(path)) + "_n.unity3d";
				break;
			case AssetLoadPathType.UnitCardMaterial:
			case AssetLoadPathType.UnitCardTextures:
			case AssetLoadPathType.SpellCardMaterial:
			case AssetLoadPathType.SpellCardTextures:
				result = "card_" + path.ToLower() + "0.unity3d";
				break;
			case AssetLoadPathType.Effect2D:
			case AssetLoadPathType.Effect2DMaterials:
			case AssetLoadPathType.Effect3D:
			case AssetLoadPathType.Effect3DAnim:
			case AssetLoadPathType.Effect3DMat:
				result = "effect_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.Emblem_S:
				result = "ui_" + Data.Master.EmblemMgr.GetResourcePath(long.Parse(path)) + "_s.unity3d";
				break;
			case AssetLoadPathType.Emblem_M:
				result = "ui_" + Data.Master.EmblemMgr.GetResourcePath(long.Parse(path)) + "_m.unity3d";
				break;
			case AssetLoadPathType.Degree_S:
				result = "uilang_" + Data.Master.DegreeMgr.GetResourcePath(int.Parse(path)) + "_s.unity3d";
				break;
			case AssetLoadPathType.DegreeMaterial_S:
				result = "uilang_" + Data.Master.DegreeMgr.GetResourcePath(int.Parse(path)) + "_s_m.unity3d";
				break;
			case AssetLoadPathType.Degree_M:
				result = "uilang_" + Data.Master.DegreeMgr.GetResourcePath(int.Parse(path)) + "_m.unity3d";
				break;
			case AssetLoadPathType.DegreeMaterial_M:
				result = "uilang_" + Data.Master.DegreeMgr.GetResourcePath(int.Parse(path)) + "_m_m.unity3d";
				break;
			case AssetLoadPathType.DegreeMask:
				result = "uilang_" + Data.Master.DegreeMgr.GetMaskTexturePath(int.Parse(path)) + ".unity3d";
				break;
			case AssetLoadPathType.RankIcon_S:
				result = "uilang_icon_rank_" + path + "_s.unity3d";
				break;
			case AssetLoadPathType.RankIcon_L:
				result = "uilang_icon_rank_" + path + "_l.unity3d";
				break;
			case AssetLoadPathType.Country_S:
				result = "ui_country_" + path.ToLower() + "_s.unity3d";
				break;
			case AssetLoadPathType.Country_M:
				result = "ui_country_" + path.ToLower() + "_m.unity3d";
				break;
			case AssetLoadPathType.DeckListTexture:
				result = $"ui_btn_deck_{int.Parse(path):D2}.unity3d";
				break;
			case AssetLoadPathType.DeckEditBGTexture:
				result = "ui_" + path + ".unity3d";
				break;
			case AssetLoadPathType.ClassCharaSpine:
				result = "ui_class_" + path + ".unity3d";
				break;
			case AssetLoadPathType.ClassCharaBase:
				result = ((!int.TryParse(path, out result2)) ? $"ui_class_{path}_base.unity3d" : $"ui_class_{result2:D2}_base.unity3d");
				break;
			case AssetLoadPathType.ClassCharaBaseWin:
				result = "ui_class_" + path + "_base_win.unity3d";
				break;
			case AssetLoadPathType.ClassCharaBaseLose:
				result = "ui_class_" + path + "_base_lose.unity3d";
				break;
			case AssetLoadPathType.ClassCharaEvolve:
				result = ((!int.TryParse(path, out result2)) ? $"ui_class_{path}_base_evolve.unity3d" : $"ui_class_{result2:D2}_base_evolve.unity3d");
				break;
			case AssetLoadPathType.ClassCharaEvolveWin:
				result = "ui_class_" + path + "_base_evolve_win.unity3d";
				break;
			case AssetLoadPathType.ClassCharaEvolveLose:
				result = "ui_class_" + path + "_base_evolve_lose.unity3d";
				break;
			case AssetLoadPathType.ClassCharaHeader:
				result = "ui_" + path + ".unity3d";
				break;
			case AssetLoadPathType.ClassCharaSkinThumbnail:
				result = $"ui_class_skin_{int.Parse(path):D2}.unity3d";
				break;
			case AssetLoadPathType.PracticePuzzleThumbnail:
				result = $"ui_puzzle_thumbnail_{int.Parse(path):D2}.unity3d";
				break;
			case AssetLoadPathType.ClassCharaWideThumbnail:
			{
				int result7 = 0;
				result = ((!int.TryParse(path, out result7)) ? $"ui_chara_select_thumbnail_wide_class_{path}.unity3d" : $"ui_chara_select_thumbnail_wide_class_{result7:D2}.unity3d");
				break;
			}
			case AssetLoadPathType.ClassCharaButton:
			{
				int result6 = 0;
				result = ((!int.TryParse(path, out result6)) ? $"ui_class_select_thumbnail_{path}.unity3d" : $"ui_class_select_thumbnail_{result6:D2}.unity3d");
				break;
			}
			case AssetLoadPathType.ClassCharaStory:
				result = $"ui_class_{int.Parse(path):D2}_story.unity3d";
				break;
			case AssetLoadPathType.ClassChara3D:
				result = $"ui_chr{path}.unity3d";
				break;
			case AssetLoadPathType.ClassCharaIconLevel:
				result = "ui_class_" + path + "_level.unity3d";
				break;
			case AssetLoadPathType.ClassCharaEncampment:
				result = "ui_class_" + path + "_encampment.unity3d";
				break;
			case AssetLoadPathType.Field3D:
				result = "bg_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.Tutorial:
				result = "tutorial_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.BossRushSpecialCard:
				result = "ui_boss_rush_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.StorySubChapterBtnTexture:
			case AssetLoadPathType.StoryChapterHeader:
			case AssetLoadPathType.StorySelectWorldPanel:
			case AssetLoadPathType.Font:
			case AssetLoadPathType.ClassCharaMesh:
			case AssetLoadPathType.ClassCharaMaterial:
			case AssetLoadPathType.ClassCharaMaterialMain:
			case AssetLoadPathType.ClassCharaTexture:
			case AssetLoadPathType.ClassCharaFrameTexture:
			case AssetLoadPathType.ClassCharaProfile:
			case AssetLoadPathType.AreaSelectTexture:
			case AssetLoadPathType.GachaTexture:
			case AssetLoadPathType.BattleTexture:
			case AssetLoadPathType.Background:
			case AssetLoadPathType.CardMenu:
			case AssetLoadPathType.Arena:
			case AssetLoadPathType.UiOtherTexture:
			case AssetLoadPathType.UiStory:
			case AssetLoadPathType.UiStorySectionSummary:
			case AssetLoadPathType.GrandPrixSpecialAtlas:
			case AssetLoadPathType.BattlePass:
			case AssetLoadPathType.BattleStage:
			case AssetLoadPathType.Bingo:
			case AssetLoadPathType.FlowChart:
			case AssetLoadPathType.Competition:
			case AssetLoadPathType.FreeCardPack:
				result = "ui_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.MyPageBackGround:
				result = "ui_bg_mypage_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.MyPageCharacter:
				result = "ui_" + path.ToLower() + "_mypage.unity3d";
				break;
			case AssetLoadPathType.MyPageCharacterMask:
				result = "ui_" + path.ToLower() + "_mypage_mask.unity3d";
				break;
			case AssetLoadPathType.MyPageBackGroundIcon:
				result = "ui_mypage_bg_icon_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.MyPageBackGroundRandomSelectIcon:
				result = "ui_mypage_bg_random_icon_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.QuestAtlas:
				result = "ui_quest.unity3d";
				break;
			case AssetLoadPathType.UilangOtherTexture:
				result = "uilang_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.ShopBuildDeckThumbnail:
				result = "uilang_build_deck_thumbnail_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.SpecialTitle:
				result = "ui_special_title_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.ShopCardPack:
			case AssetLoadPathType.ShopBuildDeck:
			case AssetLoadPathType.ShopSleeve:
			case AssetLoadPathType.ShopClassSkin:
			case AssetLoadPathType.ShopItem:
			case AssetLoadPathType.BattleLangTexture:
				result = "uilang_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.FirstTips:
				result = "uilang_" + path + ".unity3d";
				break;
			case AssetLoadPathType.SpecialCrystal:
				result = "uilang_" + path.ToLower() + ".unity3d";
				break;
			case AssetLoadPathType.Item:
			case AssetLoadPathType.Lottery:
				result = "ui_" + path + ".unity3d";
				break;
			case AssetLoadPathType.PackBox:
				result = "packbox_" + path + ".unity3d";
				break;
			case AssetLoadPathType.UiDownLoad:
			case AssetLoadPathType.UiDownLoadInfo:
				result = "uidownload_" + path + ".unity3d";
				break;
			case AssetLoadPathType.Stamp:
				result = "uilang_stamp_" + path + ".unity3d";
				break;
			case AssetLoadPathType.Uilang3DField:
				result = "uilang_" + path + ".unity3d";
				break;
			case AssetLoadPathType.NeutralVote:
				result = "ui_neutral_vote_" + path + ".unity3d";
				break;
			case AssetLoadPathType.NeutralVoteThumbnail_S:
				result = "ui_neutral_vote_thumb_" + path + "_s.unity3d";
				break;
			case AssetLoadPathType.NeutralVoteThumbnail_M:
				result = "ui_neutral_vote_thumb_" + path + "_m.unity3d";
				break;
			case AssetLoadPathType.LeaderVote:
				result = "ui_leader_vote_" + path + ".unity3d";
				break;
			case AssetLoadPathType.LeaderVoteThumbnail_S:
				result = "ui_leader_vote_thumb_" + path + "_s.unity3d";
				break;
			case AssetLoadPathType.UISpine:
				result = "ui_" + path + ".unity3d";
				break;
			}
		}
		return result;
	}

	public long GetExistingSleeveId(long sleeveId)
	{
		string path = sleeveId.ToString();
		string assetTypePath = GetAssetTypePath(path, AssetLoadPathType.SleeveMaterial);
		if (!ExistsAssetBundleManifest(assetTypePath))
		{
			AddAssetNotFoundLog(assetTypePath);
			return 3000011L;
		}
		string assetTypePath2 = GetAssetTypePath(path, AssetLoadPathType.SleeveTexture);
		if (!ExistsAssetBundleManifest(assetTypePath2))
		{
			AddAssetNotFoundLog(assetTypePath2);
			return 3000011L;
		}
		if (Data.Master.SleeveMgr.Get(sleeveId).IsPremiumSleeve)
		{
			string assetTypePath3 = GetAssetTypePath(path, AssetLoadPathType.SleeveTextureMask);
			if (!ExistsAssetBundleManifest(assetTypePath3))
			{
				AddAssetNotFoundLog(assetTypePath3);
				return 3000011L;
			}
		}
		return sleeveId;
	}

	private void AddAssetNotFoundLog(string path)
	{
		string text = "asset not found : " + path;
		Debug.LogError(text);
		LocalLog.AccumulateTraceLog(text);
	}

	public Material FindCardMaterial(int cardId, AssetLoadPathType type, bool isEvol = false, bool isMutation = false, CardBasePrm.CharaType originalType = CardBasePrm.CharaType.NORMAL, bool isChoiceBrave = false)
	{
		string text = ((cardId / 1000000000 == 1) ? GetAssetTypePath($"{cardId}_M", AssetLoadPathType.UnitCardMaterial, isfetch: true) : ((isEvol && type == AssetLoadPathType.UnitCardMaterial) ? GetAssetTypePath($"{cardId}1_M", type, isfetch: true) : ((!(isMutation || isChoiceBrave)) ? GetAssetTypePath($"{cardId}0_M", type, isfetch: true) : ((originalType != CardBasePrm.CharaType.NORMAL) ? GetAssetTypePath($"{cardId}0_M", AssetLoadPathType.SpellCardMaterial, isfetch: true) : GetAssetTypePath($"{cardId}0_M", AssetLoadPathType.UnitCardMaterial, isfetch: true)))));
		Material obj = LoadObject(text) as Material;
		if (obj == null)
		{
			AddAssetNotFoundLog(text);
		}
		return obj;
	}

	public int GetPreferredParallelLoadNum()
	{
		if (fixedParallelTasks > 0)
		{
			return fixedParallelTasks;
		}
		if (_preferredParallelLoadTasks < 0)
		{
			_preferredParallelLoadTasks = 8;
		}
		return _preferredParallelLoadTasks;
	}

	private void Awake()
	{
	}

	private List<string> FilterAssetList(List<string> assetList, ref List<string> duplicateList)
	{
		if (assetList.Count < 2)
		{
			return assetList;
		}
		List<string> list = new List<string>(new HashSet<string>(assetList));
		if (list.Count != assetList.Count)
		{
			duplicateList = assetList;
			for (int i = 0; i < list.Count; i++)
			{
				duplicateList.Remove(list[i]);
			}
		}
		return list;
	}

	private IEnumerator ParallelAssetListExec(List<string> assetList, int numParallelTasks, AssetRequestContext requestContext, Action<string, AssetRequestContext> exec, ProgressDebugType Progress = ProgressDebugType.Progress_NONE, bool preferSynchornousLoad = false)
	{
		if (BgDownloadState == BackgroundDownloadState.StopRequest)
		{
			yield break;
		}
		List<string> duplist = null;
		assetList = FilterAssetList(assetList, ref duplist);
		int totalJobs = assetList.Count;
		int finishedJobs = 0;
		if (requestContext == null)
		{
			requestContext = new AssetRequestContext(null, new Utility.LeanSemaphore(0), new AssetErrorState());
		}
		requestContext.callback = delegate(AssetHandle handle)
		{
			if (handle == null || string.IsNullOrEmpty(handle.AssetName) || assetList == null)
			{
				finishedJobs++;
			}
			else if (assetList.Contains(handle.AssetName))
			{
				finishedJobs++;
			}
		};
		requestContext.semaphore.Reset(numParallelTasks);
		requestContext.preferSynchronousLoad = preferSynchornousLoad;
		requestContext.errorState.Reset();
		AssetErrorState errorState = requestContext.errorState;
		LocalLog.UpdateLoadResourceLog("JustBeforeExec");
		do
		{
			if (BgDownloadState == BackgroundDownloadState.StopRequest)
			{
				yield break;
			}
			int count = assetList.Count;
			for (int num = 0; num < count; num++)
			{
				exec(assetList[num], requestContext);
			}
			while (finishedJobs < totalJobs && BgDownloadState != BackgroundDownloadState.StopRequest)
			{
				if (errorState.HasError())
				{
					errorState.SetCanceled();
				}
				yield return 0;
			}
			if (errorState.HasError())
			{
				if (errorState.lastDialogDecision != AssetErrorState.DialogDecision.RETRY)
				{
					LocalLog.RecordResouseLoadError(errorState.errorFlag);
					while (true)
					{
						yield return 0;
					}
				}
				assetList = errorState.GatherErrorFilenames();
				finishedJobs = totalJobs - assetList.Count;
			}
			if (BgDownloadState == BackgroundDownloadState.StopRequest)
			{
				Toolbox.AssetManager.CancelDownloadAsyncJob();
				break;
			}
		}
		while (errorState.HasError());
		requestContext.callback = null;
		if (duplist == null)
		{
			yield break;
		}
		for (int num2 = 0; num2 < duplist.Count; num2++)
		{
			AssetHandle assetHandle = Toolbox.AssetManager.GetAssetHandle(duplist[num2]);
			if (assetHandle != null && assetHandle.reference >= 1)
			{
				assetHandle.reference++;
			}
		}
	}

	public IEnumerator LoadAssetGroupAsync(List<string> rogueAssetList, Action callback, bool isProgress = true)
	{
		return LoadAssetGroup(rogueAssetList, callback, isProgress);
	}

	public IEnumerator LoadAssetGroupSync(List<string> rogueAssetList, Action callback, bool isProgress = true)
	{
		return LoadAssetGroup(rogueAssetList, callback, isProgress, preferSynchronousLoad: true);
	}

	public IEnumerator LoadAssetGroup(List<string> rogueAssetList, Action callback, bool isProgress = true, bool preferSynchronousLoad = false)
	{
		List<string> filteredAssetList = QuickLoadAssetGroup(rogueAssetList);
		if (filteredAssetList == null || filteredAssetList.Count == 0)
		{
			callback?.Invoke();
			yield break;
		}
		LocalLog.UpdateLoadResourceLog("BeforeSemaphore");
		while (loadSemaphore)
		{
			yield return null;
		}
		SetLoadProcessLock(enable_lock: true);
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;
		_ = isProgress;
		LocalLog.UpdateLoadResourceLog("BeforeParallelAssetListExec");
		Toolbox.AssetManager.AddLoadingMaxCount(rogueAssetList.Count);
		yield return StartCoroutine(ParallelAssetListExec(filteredAssetList, GetPreferredParallelLoadNum(), loadRequestContext, LoadAssetSub, isProgress ? ProgressDebugType.Progress_LOAD : ProgressDebugType.Progress_NONE, preferSynchronousLoad));
		LocalLog.UpdateLoadResourceLog("AfterParallelAssetListExec");
		yield return new WaitForFixedUpdate();
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = Toolbox.QualityManager.GetFrameRate();
		SetLoadProcessLock(enable_lock: false);
		callback?.Invoke();
		LocalLog.UpdateLoadResourceLog("FinishLoad");
	}

	private bool QuickLoadAsset(string assetName)
	{
		AssetHandle assetHandle = Toolbox.AssetManager.GetAssetHandle(assetName);
		bool result = false;
		if (assetHandle != null)
		{
			result = assetHandle.QuickLoadIfPossible();
		}
		return result;
	}

	private List<string> QuickLoadAssetGroup(List<string> assetList)
	{
		if (assetList == null || assetList.Count == 0)
		{
			return null;
		}
		List<string> list = null;
		int count = assetList.Count;
		if (count == 1)
		{
			string text = assetList[0];
			if (!QuickLoadAsset(text))
			{
				list = new List<string>(1);
				list.Add(text);
			}
		}
		else
		{
			HashSet<string> hashSet = null;
			for (int i = 0; i < count; i++)
			{
				string text2 = assetList[i];
				if (string.IsNullOrEmpty(text2))
				{
					continue;
				}
				if (hashSet != null && hashSet.Contains(text2))
				{
					AssetHandle assetHandle = Toolbox.AssetManager.GetAssetHandle(text2);
					if (assetHandle != null && assetHandle.reference >= 1)
					{
						assetHandle.reference++;
					}
				}
				else if (QuickLoadAsset(text2))
				{
					if (hashSet == null)
					{
						hashSet = new HashSet<string>();
					}
					hashSet.Add(text2);
				}
				else
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(text2);
				}
			}
		}
		return list;
	}

	public IEnumerator LoadAssetAsync(string assetName, Action callback)
	{
		List<string> rogueAssetList = new List<string> { assetName };
		yield return StartCoroutine(LoadAssetGroupAsync(rogueAssetList, null));
		callback?.Invoke();
	}

	private void LoadAssetSub(string assetName, AssetRequestContext requestContext)
	{
		Toolbox.AssetManager.CacheAsset(assetName, requestContext);
	}

	public void StartCoroutine_LoadAssetGroupAsync(string asset, Action callback, bool isProgress = true)
	{
		StartCoroutine(LoadAssetGroupAsync(new List<string> { asset }, callback, isProgress));
	}

	public void StartCoroutine_LoadAssetGroupSync(List<string> assetList, Action callback, bool isProgress = true)
	{
		if (assetList == null || assetList.Count == 0)
		{
			callback?.Invoke();
		}
		else
		{
			StartCoroutine(LoadAssetGroupSync(assetList, callback, isProgress));
		}
	}

	public void RemoveAssetGroup(List<string> assetList)
	{
		for (int i = 0; i < assetList.Count; i++)
		{
			RemoveAsset(assetList[i]);
		}
	}

	public void RemoveAsset(string assetName)
	{
		Toolbox.AssetManager.UnloadAsset(assetName);
	}

	public bool ExistsAssetBundleManifest(string assetName)
	{
		if (Toolbox.AssetManager.GetAssetHandle(assetName) != null)
		{
			return true;
		}
		return false;
	}

	public UnityEngine.Object LoadObject(string objectName, bool isServerResources = true, bool isIfFindLoad = false)
	{
		return LoadObject<UnityEngine.Object>(objectName, isServerResources);
	}

	public T LoadObject<T>(string objectName, bool isServerResources = true, bool isIfFindLoad = false) where T : UnityEngine.Object
	{
		if (!isServerResources)
		{
			return Resources.Load<T>(objectName);
		}
		AssetManager assetManager = Toolbox.AssetManager;
		if (assetManager != null)
		{
			UnityEngine.Object obj = assetManager.LoadObject(objectName, typeof(T), isIfFindLoad);
			if (obj != null)
			{
				return (T)obj;
			}
		}
		return Resources.Load<T>(objectName);
	}

	private void SetLoadProcessLock(bool enable_lock)
	{
		loadSemaphore = enable_lock;
	}
}
