// PASS-9 STUB: 1,430-line NGUI static text-rendering utility reduced to compile-time
// surface. All callers (UIInput, Wizard/RubyText) are UI-side. No receive-path callers.
// See Task 5 of PASS8-PLAN.md.
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Wizard;

public static class NGUIText
{
    public enum Alignment { Left, Right, Justified }
    public enum SymbolStyle { None, Normal}

    public class GlyphInfo
    {
        public Vector2 v1;
    }
}
