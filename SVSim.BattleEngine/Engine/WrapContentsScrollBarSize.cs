using UnityEngine;

public class WrapContentsScrollBarSize : MonoBehaviour
{
	public UIWrapContent wrapContent;

	public void ContentUpdate()
	{
		UIWidget uIWidget = base.gameObject.GetComponent<UIWidget>();
		if (uIWidget == null)
		{
			uIWidget = base.gameObject.AddComponent<UIWidget>();
		}
		uIWidget.height = wrapContent.itemSize * (Mathf.Abs(wrapContent.minIndex - wrapContent.maxIndex) + 1);
		uIWidget.pivot = UIWidget.Pivot.Top;
		base.transform.localPosition = new Vector3(0f, (float)wrapContent.itemSize / 2f, 0f);
	}
}
