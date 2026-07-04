using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Root")]
public class UIRoot : MonoBehaviour
{
	public enum Scaling
	{
		Flexible,
		Constrained,
		ConstrainedOnMobiles
	}

	public enum Constraint
	{
		Fit,
		Fill,
		FitWidth,
		FitHeight
	}

	public Scaling scalingStyle;

	public int manualWidth = 1280;

	public int manualHeight = 720;

	public int minimumHeight = 320;

	public int maximumHeight = 1536;

	public bool fitWidth;

	public bool fitHeight = true;

	public bool adjustByDPI;

	public bool shrinkPortraitUI;

	private Transform mTrans;

	public Constraint constraint
	{
		get
		{
			if (fitWidth)
			{
				if (fitHeight)
				{
					return Constraint.Fit;
				}
				return Constraint.FitWidth;
			}
			if (fitHeight)
			{
				return Constraint.FitHeight;
			}
			return Constraint.Fill;
		}
	}

	public Scaling activeScaling
	{
		get
		{
			Scaling scaling = scalingStyle;
			if (scaling == Scaling.ConstrainedOnMobiles)
			{
				return Scaling.Constrained;
			}
			return scaling;
		}
	}

	public int activeHeight
	{
		get
		{
			if (activeScaling == Scaling.Flexible)
			{
				Vector2 screenSize = NGUITools.screenSize;
				float num = screenSize.x / screenSize.y;
				if (screenSize.y < (float)minimumHeight)
				{
					screenSize.y = minimumHeight;
					screenSize.x = screenSize.y * num;
				}
				else if (screenSize.y > (float)maximumHeight)
				{
					screenSize.y = maximumHeight;
					screenSize.x = screenSize.y * num;
				}
				int num2 = Mathf.RoundToInt((shrinkPortraitUI && screenSize.y > screenSize.x) ? (screenSize.y / num) : screenSize.y);
				if (!adjustByDPI)
				{
					return num2;
				}
				return NGUIMath.AdjustByDPI(num2);
			}
			Constraint constraint = this.constraint;
			if (constraint == Constraint.FitHeight)
			{
				return manualHeight;
			}
			Vector2 screenSize2 = NGUITools.screenSize;
			float num3 = screenSize2.x / screenSize2.y;
			float num4 = (float)manualWidth / (float)manualHeight;
			switch (constraint)
			{
			case Constraint.FitWidth:
				return Mathf.RoundToInt((float)manualWidth / num3);
			case Constraint.Fit:
				if (!(num4 > num3))
				{
					return manualHeight;
				}
				return Mathf.RoundToInt((float)manualWidth / num3);
			case Constraint.Fill:
				if (!(num4 < num3))
				{
					return manualHeight;
				}
				return Mathf.RoundToInt((float)manualWidth / num3);
			default:
				return manualHeight;
			}
		}
	}

	protected virtual void Awake()
	{
		mTrans = base.transform;
	}
}
