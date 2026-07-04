// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\UIManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BestHTTP;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.View.Vfx;
using Wizard.Dialog.Setting;
using Wizard.UI.ReportToManagement;
public partial class UIManager
{
        public partial class ChangeViewSceneParam { }
        public enum ViewScene
        {
        None,
        MyPage,
        Battle,
        Room,
        RankMatch,
        ClassSelectionPage,
        StorySelectPage,
        StorySelectionWorld,
        QuestSelectionPage,
        DeckList,
        DeckCardEdit,
        CardAllList,
        Sealed,
        SealedCardPackOpen,
        SealedDeckEdit,
        Gacha,
        BuildDeckPurchasePage,
        CardSleevePurchasePage,
        ClassSkinPurchasePage,
        Profile,
        Mission,
        BattlePass,
        Bingo,
        CrossoverPortal,
        NeutralPopularityVote,
        LeaderPopularityVote        }
        public UIRoot UIManagerRoot;
        public Camera UIRootLoadingCamera;
        public DeckCreateMenuUI _deckCreateMenuOriginal;
        public bool isErrorProc;
        public bool isRetryProc;
        public bool isNoAvailMemory;
        public Footer _Footer { get; set; }
        public GameObject MyPageUICameraObj { get; set; }
        public NguiObjs TextInputDialogPrefab { get; set; }
        public DrumrollDialog DrumrollDialogPrefab { get; set; }
        public DialogBase NowOpenDialog { get; set; }
        public UIRoot UIRootSystem { get; set; }
        public bool isBattleRecovery { get; set; }
        public WebViewHelper WebViewHelper { get; set; }
        public bool IsTouchable { get; set; }
        public Camera getCamera() => default!;
        public string GetSceneAssetPath(UIAtlasManager.AssetBundleNames assetname, string singlebundlename = "", bool isload = false) => default!;
        public void AddResidentAtlas(UIAtlasManager.AssetBundleNames atlasName) { }
        public void RemoveResidentAtlas(UIAtlasManager.AssetBundleNames atlasName) { }
        public List<UIAtlas> GetAtlasList() => default!;
        public void DestroyView(ViewScene scene) { }
        public UIBase GetUIBase(ViewScene scene) => default!;
        public void OverrideSceneParam(ViewScene scene, object sceneParam) { }
        public T GetSceneParam<T>(ViewScene scene) where T : class => default!;
        public void ChangeViewScene(ViewScene nextScene, ChangeViewSceneParam param = null, object sceneParam = null) { }
        public void OnReadyViewScene(bool isFadein, Action onFinishChangeView = null, Action onFinishFadeIn = null) { }
        public void Force_Increment_LockCountChangeView() { }
        public void Force_Decrement_LockCountChangeView() { }
        public void UpdateFooterMenuTexture(ViewScene scene) { }
        public ViewScene GetCurrentScene() => default!;
        public bool IsCurrentScene(ViewScene scene) => default!;
        public UIBase GetUiBaseOfCurrentScene() => default!;
        public void OpenNotTouch() { }
        public void offNotTouch() { }
        public DialogBase CreateDialogClose(bool isSystem = false, bool dontChangeLabelColor = false) => default!;
        public DialogBase CreateConfirmationDialog(string message) => default!;
        public void dialogAllClear() { }
        public bool isOpenDialog() => default!;
        public VfxBase CreateNowLoadingVfx(VfxBase loadResourcesVfx) => global::Wizard.Battle.View.Vfx.NullVfx.GetInstance();
        public void createInSceneCenterLoading(bool notBlack = false, bool notCollider = false, bool force = true, string overrideText = null) { }
        public void closeInSceneCenterLoading(bool force = true, bool disableCollider = false) { }
        public LoadingInScene createInSceneLoadingMatching(bool notBlack = false, bool notCollider = false) => default!;
        public void createInSceneNotNetwork() { }
        public void closeInSceneNotNetwork() { }
        public void CreatFadeClose(Action onFinishCallback = null) { }
        public bool isFading() => default!;
        public void CardLoadSelect(GameObject returnObj, IList<int> CardNums, int layer, bool is2D, Action onFinish = null, bool isDefaultSleeve = false, CardMaster.CardMasterId cardMasterId = CardMaster.CardMasterId.Default) { }
        public List<UIBase_CardManager.CardObjData> getCardList2DObjs() => default!;
        // Shim fix (M5): return a non-null, field-wired no-op so the copied cosmetic helpers it
        // exposes (UIBase_CardManager.SetNumberLabelStyle/SetNameLabelStyle, read by
        // CardCreatorBase.CreateCard on the createNullView:false path) resolve headless instead of
        // NRE-ing on a null manager. Was `default!`.
        public UIBase_CardManager getUIBase_CardManager() => UnityEngine.ShimView.Create<UIBase_CardManager>();
        public void setBackScene(GameObject obj, ViewScene backScene) { }
        public TopBar CreateTopBar(GameObject obj, string titleMsg, ViewScene backScene = ViewScene.None, bool MoneyDraw = true, ChangeViewSceneParam Param = null, bool isWideMode = false) => default!;
        public void RemoveNowSceneBackButtonParameter() { }
        public void ShowFooterMenu(bool isShow) { }
        public static void SetObjectToGrey(GameObject o, bool b, Color? enableTextColor = null, Dictionary<Color32, AllLabelColorChanger.ColorSet> changeColorDict = null) { }
        public void CommonRetry() { }
        public FirstTips CheckFirstTips(FirstTips.TipsType TipsType, Action onFinish = null, int startPage = 0) => default!;
        public FirstTips StartFirstTips(IEnumerable<FirstTips.TipsType> tipsTypes, Action onFinish = null, int startPage = 0, int seasonId = 0) => default!;
        public void SetLayerRecursive(Transform parentObj, int layer) { }
        public void AttachAtlas(GameObject obj, bool isTargetChildren = true) { }
        public void AttachAtlas(List<GameObject> obj_list, bool isTargetChildren = true) { }
        public bool IsQuitDialog() => default!;
        public static void ShowDialogUrl(string title, string url, Action<DialogBase> onDialogOpening = null) { }
        public void CreateAssetFileErrorDialog() { }
}
