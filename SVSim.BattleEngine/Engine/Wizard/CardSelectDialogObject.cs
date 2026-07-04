using UnityEngine;

namespace Wizard;

public class CardSelectDialogObject : MonoBehaviour
{
	[SerializeField]
	private GameObject _cardObjectRoot;

	[SerializeField]
	private UILabel _possessionNumLabel;

	[SerializeField]
	private UIButton _decisionButton;

	public GameObject CardObjectRoot => _cardObjectRoot;

	public UILabel PossessionNumLabel => _possessionNumLabel;

	public UIButton DecisionButton => _decisionButton;
}
