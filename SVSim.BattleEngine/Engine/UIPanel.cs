// PASS-9 STUB: 1,396-line NGUI panel container reduced to the compile-time surface
// external callers touch. All callers are UI-side (ActiveAnimation, DeckSortDragDrop,
// TurnPanelControl, NGUITools, UIDrawCall, UIWidget). No receive-path callers.
// See Task 5 of PASS8-PLAN.md.
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Panel")]
public class UIPanel : UIRect
{
    public enum RenderQueue { }

    public delegate void OnGeometryUpdated();
    public delegate void OnClippingMoved(UIPanel panel);
    public int depth;
    public UIDrawCall.Clipping clipping;
    public Vector4 baseClipRegion;
    public Vector2 clipSoftness;
    public Vector2 clipOffset;

    // UIRect abstract implementations
    public override float alpha { get; set; } = 1f;
    public override Vector3[] localCorners => new Vector3[4];
    public override Vector3[] worldCorners => new Vector3[4];
    public override float CalculateFinalAlpha(int frameID) => alpha;
    public override void SetRect(float x, float y, float width, float height) { }
    protected override void OnAnchor() { }
    protected override void OnStart() { }

    // UIRect virtual overrides
    public override bool canBeAnchored => false;
    public override void Invalidate(bool includeChildren) { }
    public bool hasCumulativeClipping => false;
    public Vector4 finalClipRegion => Vector4.zero;
    public int sortingOrder;
    public float height;

    public Vector2 GetViewSize() => Vector2.zero;
    public void Refresh() { }
    public void SortWidgets() { }
    public void RebuildAllDrawCalls() { }
    public bool IsVisible(UIWidget w) => true;
    public Vector3 CalculateConstrainOffset(Vector3 min, Vector3 max) => Vector3.zero;
    public bool ConstrainTargetToBounds(Transform target, bool immediate) => false;
    public static UIPanel Find(Transform trans, bool createIfMissing, int layer) => null;
    public void AddWidget(UIWidget w) { }
    public void RemoveWidget(UIWidget w) { }
}
