// PASS-9 STUB: 1,152-line client-side NGUI modal dialog base reduced to the
// compile-time surface external callers touch. Held alive by
// BattleFinishToOpponentDisConnectChecker (network-battle disconnect UI — display-only,
// not receive-path mutations) and scattered UI-side callers.
// See Task 5 of PASS8-PLAN.md.
using System;
using UnityEngine;
using Wizard;
using Wizard.UI.Dialog.ImageSelection;

public class DialogBase : MonoBehaviour
{
    public enum DialogScene { }

    public enum Size { S, M, L, XL }

    public enum ButtonLayout
    {
        NONE, OkBtn, DecisionBtn, CloseBtn, GrayBtn, BlueBtn_CancelBtn, RedBtn_CancelBtn, BlueBtn_GrayBtn, BlueBtn_RedBtn_GrayBtn,
        GrayBtn_CancelBtn_BlueBtn,         BlueButton    }

    public enum ButtonType
    {
OK, Close,         Retry, BackToTitle, BackToHome, QuitApplication, VersionUp, RecommendedList
    }

    // Callback fields
    public Action onPushButton1;
    public Action onPushButton2;
    public Action onPushButton3;
    public Action OnClose;
    public Action OnCloseStart;
    public Action onCloseWithoutSelect;

    // Property/data fields
    public bool Button2Grey;
    public bool isNotCloseWindowButton2;
    public int ClickSe_Btn1;
    public int ClickSe_Btn2;
    public int ClickSe_Btn3;
    public int OpenSe;
    public GameObject InsideObject;
    public NguiObjs InputAreaObjs;
    public InputDialog InputDialog;
    public UIScrollView ScrollView;
    public UIButton button1;

    // Instance methods — all no-ops
    public void SetActive(bool inActive) { }
    public void SetText(string text) { }
    public void SetText(string text, bool isWrapText) { }
    public void Close() { }
    public void CloseOnOff(bool flag) { }
    public void CloseWithoutSelect() { }
    public bool IsOpen() => false;
    public void SetTitleLabel(string text) { }
    public void SetButtonLayout(ButtonLayout layout) { }
    public void SetFadeButtonEnabled(bool flag) { }
    public void SetPanelDepth(int depth) { }
    public void SetSize(Size size) { }
    public void AddButton(ButtonType type, Action callback = null) { }
    public void AddButton(ButtonType type, bool isReflect) { }
    public void SetScrollViewActive(bool b) { }
    public void SetReturnMsg(string msg, string s) { }
    public void SetReturnMsg(GameObject go, string a, string b) { }
    public void SetReturnMsg(GameObject go, string a, string b, string c, string d) { }
    public void SetButtonText(string text) { }
    public void SetButtonText(string text1, string text2) { }
    public void SetButtonText(string text1, string text2, string text3) { }
    public void SetButtonDelegate(Action callback) { }
    public void SetButtonDelegate(EventDelegate del) { }
    public void SetButtonDisable(bool isEnableOK, bool isEnableCancel = false) { }
    public void SetDialogNoClose() { }
    public void SetLayer(string layerName) { }
    public void SetObj(GameObject obj) { }
    public void SetBackViewLayer(int layer) { }
    public void SetBackViewToNotCloseDialog() { }
    public void SetPanelSortingOrder(int order) { }
    public void SetVisibleContactButton(bool isVisible, string errorId) { }
    public void AttachToScrollView(Transform t) { }

    // Static factory methods — return null (UI-side only)
    public static DialogBase CreateImageSelectionDialog(ImageSelection imageSelection, string titleTextId, Size dialogSize = Size.M) => null;
    public static DialogBase CreateFilteringImageSelectionDialog(FilteringImageSelection selection, string titleTextId, bool isOpenSelection = true) => null;
}
