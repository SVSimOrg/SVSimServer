using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

// PASS-8/Phase-1 STUB: 1,909-line client-side card-detail-panel UI. Held on `DetailMgr.
// SubDetailPanelControl` (nullable field) but never constructed anywhere — NullDetailPanelControl
// is the headless-live implementation carried into the receive path. The class exists purely
// as a compile-time surface: the ShowRequest enum + LoadCardHeaderTexture static + interface
// contract via IDetailPanelControl (mirroring NullDetailPanelControl's shape). Every instance
// member is a no-op / default because a headless `SubDetailPanelControl` is either null (NRE
// on read — never reached) or is actually a NullDetailPanelControl instance.
public class DetailPanelControl : CardDetailBase, IDetailPanelControl
{
    public enum ShowRequest
    {
        NORMAL,
        MULLIGAN,
        EVOLUTION_SELECT,
        FUSION_INFO_CARD_LIST,
        CHOICE_BRAVE,
        CHOICE_BRAVE_AND_BUFF
    }

    public bool IsShow => false;
    public BattleCardBase _card => null;
    public bool forceEvolutionConfirm { get; set; }
    public UIButton EvolveButton => null;
    public ShowRequest CurrentShowRequest => ShowRequest.NORMAL;
    public GameObject EvoTargetPanelColliderGameObject => null;
    public EvolutionConfirmation _evolutionConfirmation => null;

    public event Action OnHideOneTime { add { } remove { } }

    public void UpdateCardDescriptionOnEvent() { }
    public void UpdateCardDescriptionOnEvolutionEvent() { }
    public void Show(BattleManagerBase battleMgrBase, OperateMgr operateMgr, BattleCardBase card, ShowRequest showRequest) { }
    public void ShowList(BattleManagerBase battleMgrBase, OperateMgr operateMgr, List<BattleCardBase> cards, ShowRequest showRequest,
        BuffInfo buff, BattleLogItem.CardTextureOption textureOption = BattleLogItem.CardTextureOption.Null,
        string divergenceId = "", int logTextureId = 0) { }
    public void Hide() { }
    public void SetSize(float percent) { }
    public void UpdateBuffInfo(BattleCardBase targetCard, List<BattlePlayerBase.MyRotationBonusCondition> myRotationBonusList) { }
    public void UpdateLogItemBuffInfo(BattleCardBase targetCard) { }
    public void SetScreenPosition(bool right) { }
    public VfxBase ShowEvolutionButton(BattleCardBase card) => NullVfx.GetInstance();
    public void CreateNextPanel() { }
    public void SetKeyBtnActive(List<bool> hasKeyword) { }
    public void ShowKeySubPanel(int page) { }
    public void HideKeySubPanel() { }
    public bool IsDisplayedRight() => false;
    public List<BuffInfo> GetDistinctBuffList(List<BuffInfo> buffInfoList) => new List<BuffInfo>();
    public List<NetworkBattleReceiver.ReplayBuffInfoLabel> GetBuffDetailLabel(BattleCardBase targetCard) => new List<NetworkBattleReceiver.ReplayBuffInfoLabel>();
}
