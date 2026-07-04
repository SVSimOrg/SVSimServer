using UnityEngine;

namespace Wizard;

public class MyRotationAbilityDetailDialogItem : MonoBehaviour
{
	[SerializeField]
	private UILabel _headerLabel;

	[SerializeField]
	private GameObject _abilityOriginal;

	[SerializeField]
	private UIGrid _grid;

	public void Initialize(MyRotationAbilityGroup group)
	{
		_headerLabel.text = Data.SystemText.Get("MyRotation_ID_12", group.StartPackNumber.ToString(), group.LastPackNumber.ToString(), group.StartPackShortName, group.LastPackShortName);
		_abilityOriginal.SetActive(value: false);
		foreach (MyRotationInfo.MyRotationBonus ability in group.AbilityList)
		{
			GameObject obj = NGUITools.AddChild(_grid.gameObject, _abilityOriginal);
			obj.GetComponentInChildren<UISprite>().spriteName = ability.IconName;
			obj.GetComponentInChildren<UILabel>().text = ability.AbilityDesc;
			obj.SetActive(value: true);
		}
		_grid.Reposition();
	}
}
