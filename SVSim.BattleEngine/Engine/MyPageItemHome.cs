using System.Collections.Generic;
using UnityEngine;
using Wizard;

// PASS-8/Phase-2 STUB: 471-line client-side home-tab MyPage panel reduced to its
// compile-time surface. MyPageMenu.Instance returns null so instance-member chains
// are never reachable at runtime on the headless node. Four members preserved:
// TUTORIAL_OFFSET_FOOTER (Mail.cs static ref), ContentsRoot (MyPageBannerBase),
// OnDecideMyPageBG (SceneTransition delegate), HideAndRepositionSubBanner
// (BattlePassPurchaseDialog). Held alive by MyPageMenu.HomeMenu return-type reference.
// See Task 2a of PASS8-PLAN.md.
public class MyPageItemHome : MyPageItem
{
    public static readonly Vector3 TUTORIAL_OFFSET_FOOTER = new Vector3(0f, 19f, 0f);

    public void OnDecideMyPageBG(MyPageDetail.BGType bgType, string selectId, List<string> randomIdList) { }
}
