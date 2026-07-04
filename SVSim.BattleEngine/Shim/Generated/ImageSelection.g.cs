// AUTO-GENERATED no-op stubs (m1_stub_gen) from Shadowverse_Code_2026-05-23\Wizard.UI.Dialog.ImageSelection\ImageSelection.cs
using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;
namespace Wizard.UI.Dialog.ImageSelection
{
public partial class ImageSelection
{
        public partial class PageItem { }
        public partial class Page { }
        public partial class PageDisplayFrameItem { }
        public partial class PageDisplayFrame { }
        public partial class CategoryInfo { }
        private delegate void OnClickPageItemDelegate(PageItem pageItem, PageDisplayFrame frame);
        public delegate bool IsNewItemDelegate();
        public enum SelectType
        {
        }
        public void Create(int allPanelDepthAddValue = 0, DialogBase dialog = null) { }
        public List<string> GetSelectedList() => default!;
        public void SelectAll() { }
        public void SelectCancelAll() { }
        public void AddItem(string key, string category, bool isSelectable, IsNewItemDelegate isNewItemMethod, string loadTexPath, string fetchTexPath, bool isDisplaySprite, string name, string[] texts, Action onDisplay, Action onClick = null, Action<GameObject> onPress = null) { }
        public void Open() { }
        public void Close() { }
        public void SetDisplayPage(int pageNo) { }
        public void LoadDisplayPage() { }
        public string GetSelectedItemKey() => default!;
        public int SelectItemWithKey(string key) => default!;
        public void SelectMultiItem(List<string> keyList) { }
}
}
