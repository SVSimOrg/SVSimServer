using System;
using UnityEngine;
using Wizard;

public class CostCurveUI : MonoBehaviour
{
	[Serializable]
	private class CostStruct
	{
		[SerializeField]
		public GameObject Parent;

		[SerializeField]
		public UILabel LabelCost;

		[SerializeField]
		public UILabel LabelCount;

		[SerializeField]
		public UIProgressBar Bar;

		public int Count;
	}

	[SerializeField]
	private int m_maxBarValue = 15;

	[SerializeField]
	private CostStruct m_original;

	[SerializeField]
	private UIProgressBar m_barAdditiveEffect;

	[SerializeField]
	private int m_barNum = 8;

	[SerializeField]
	private int m_minCost = 1;

	[SerializeField]
	private float m_barWidth = 25f;

	private CostStruct[] m_costArray;

	private CardMaster.CardMasterId _cardMasterId;

	public void Initialize(CardMaster.CardMasterId cardMasterId)
	{
		_cardMasterId = cardMasterId;
	}

	public void Refresh()
	{
		if (m_costArray != null)
		{
			for (int i = 0; i < m_costArray.Length; i++)
			{
				m_costArray[i].Count = 0;
				m_costArray[i].LabelCount.text = "0";
				m_costArray[i].Bar.value = 0f;
			}
		}
	}

	public void Refresh(int[] array)
	{
		Refresh();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				Add(array[i], withAnim: false);
			}
		}
	}

	public void Add(int cardId, bool withAnim)
	{
		ChangeValue(1, cardId, withAnim);
	}

	public void Sub(int cardId, bool withAnim)
	{
		ChangeValue(-1, cardId, withAnim);
	}

	private void ChangeValue(int addnum, int cardId, bool withAnim)
	{
		int num = Mathf.Min(Mathf.Max(CardMaster.GetInstance(_cardMasterId).GetCardParameterFromId(cardId).Cost, m_minCost), m_barNum) - m_minCost;
		CostStruct cost_this = m_costArray[num];
		cost_this.Count += addnum;
		if (withAnim && m_barAdditiveEffect != null)
		{
			m_barAdditiveEffect.gameObject.SetActive(value: true);
			UIProgressBar barAdditiveEffect = m_barAdditiveEffect;
			float value = (cost_this.Bar.value = (float)(cost_this.Count + addnum) / (float)m_maxBarValue);
			barAdditiveEffect.value = value;
			m_barAdditiveEffect.transform.position = cost_this.Bar.transform.position;
			m_barAdditiveEffect.alpha = 0.8f;
			TweenAlpha.Begin(m_barAdditiveEffect.gameObject, 0.4f, 0f);
			TweenScale anim = TweenScale.Begin(cost_this.LabelCount.gameObject, 0.2f, Vector3.one * 1.2f);
			TweenAlpha.Begin(cost_this.LabelCount.gameObject, 0.2f, 0f);
			EventDelegate ev = null;
			ev = new EventDelegate(delegate
			{
				anim.RemoveOnFinished(ev);
				cost_this.Bar.value = (float)cost_this.Count / (float)m_maxBarValue;
				cost_this.LabelCount.text = cost_this.Count.ToString() ?? "";
				cost_this.LabelCount.transform.localScale = Vector3.one * 0.8f;
				TweenScale.Begin(cost_this.LabelCount.gameObject, 0.2f, Vector3.one);
				TweenAlpha.Begin(cost_this.LabelCount.gameObject, 0.2f, 1f);
			});
			anim.onFinished.Add(ev);
		}
		else
		{
			cost_this.LabelCount.text = cost_this.Count.ToString() ?? "";
			cost_this.Bar.value = (float)cost_this.Count / (float)m_maxBarValue;
		}
	}

	private void Awake()
	{
		m_costArray = new CostStruct[m_barNum];
		m_original.LabelCost.text = m_minCost.ToString() ?? "";
		m_original.LabelCount.text = 0.ToString() ?? "";
		m_original.Bar.value = 0f;
		m_costArray[0] = m_original;
		for (int i = 1; i < m_barNum; i++)
		{
			m_costArray[i] = new CostStruct();
			GameObject obj = (m_costArray[i].Parent = UnityEngine.Object.Instantiate(m_original.Parent));
			obj.transform.parent = m_original.Parent.transform.parent;
			obj.transform.localScale = m_original.Parent.transform.localScale;
			Vector3 localPosition = m_original.Parent.transform.localPosition;
			localPosition.x += m_barWidth * (float)i;
			obj.transform.localPosition = localPosition;
			UIWidget[] componentsInChildren = obj.GetComponentsInChildren<UIWidget>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (componentsInChildren[j].name == m_original.LabelCost.name)
				{
					m_costArray[i].LabelCost = componentsInChildren[j].GetComponent<UILabel>();
					m_costArray[i].LabelCost.text = (i + m_minCost).ToString() ?? "";
					if (i == m_barNum - 1)
					{
						m_costArray[i].LabelCost.text += "⁺";
					}
				}
				else if (componentsInChildren[j].name == m_original.LabelCount.name)
				{
					m_costArray[i].LabelCount = componentsInChildren[j].GetComponent<UILabel>();
					m_costArray[i].LabelCount.text = 0.ToString() ?? "";
				}
				else if (componentsInChildren[j].name == m_original.Bar.name)
				{
					m_costArray[i].Bar = componentsInChildren[j].GetComponent<UIProgressBar>();
					m_costArray[i].Bar.value = 0f;
				}
				m_costArray[i].Count = 0;
			}
		}
		if (m_barAdditiveEffect != null)
		{
			m_barAdditiveEffect.gameObject.SetActive(value: false);
		}
	}
}
