// PASS-9 STUB: 1,525-line NGUI text label reduced to the compile-time surface external
// callers touch. BattleManagerBase.AlertDialogueLabel field type is UILabel — field
// declaration compiles fine against this stub; UILabel methods are called only from
// UI-side code. See Task 5 of PASS8-PLAN.md.
using System;
using UnityEngine;
using Wizard;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Label")]
public class UILabel : UIWidget
{
    public enum Effect { None, Outline8 }
    public enum Overflow { ClampContent}
    public enum Crispness { }

    // Backing field populated by the shim's Materialize() reflection path so material is non-null
    // when SetNumberLabelStyle reads inLabel.material.name.
    [NonSerialized] private Material mMaterial;
    public override Material material { get => mMaterial; set => mMaterial = value; }

    // Core display fields
    public string text;
    public string processedText;
    public Effect effectStyle;
    public Color effectColor;
    public Vector2 effectDistance;
    public bool multiLine;
    public Overflow overflowMethod;
    public bool overflowEllipsis;
    public int maxLineCount;
    public int fontSize;
    public int spacingY;
    public NGUIText.Alignment alignment;
    public bool applyGradient;
    public Color gradientBottom;
    public Color gradientTop;
    public bool supportEncoding;
    public UIFont bitmapFont;
    public UnityEngine.Font trueTypeFont;

    // Methods
    public void ProcessText() { }
    public void SetWrapText(string text) { }
    public bool Wrap(string text, out string finalText) { finalText = text; return false; }
    public int CalculateOffsetToFit(string text) => 0;
    public int GetCharacterIndexAtPosition(Vector3 worldPos, bool precise) => 0;
    public void PrintOverlay(int start, int end, UIGeometry caret, UIGeometry highlight, Color caretColor, Color selectionColor) { }
}
