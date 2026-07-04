using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public abstract class InPlayViewBase
{
	protected readonly InPlayCardControl inPlayCardControl;

	protected readonly List<IBattleCardView> battleCardViewList;

	public InPlayViewBase()
	{
	}

	public InPlayViewBase(GameObject inPlayGameObject)
	{
		inPlayCardControl = new InPlayCardControl(inPlayGameObject);
		battleCardViewList = new List<IBattleCardView>();
	}

	public virtual void RemoveCardFromView(IBattleCardView cardViewToRemove)
	{
		battleCardViewList.Remove(cardViewToRemove);
	}
}
