using UnityEngine;

namespace Wizard;

public class ArenaCommonLobbyBattleStateObject : MonoBehaviour
{
	public enum eState
	{
		None,
		Won,
		Lost,
		Next	}

	[SerializeField]
	private GameObject _nextBattleMark;

	[SerializeField]
	private UILabel _winLabel;

	[SerializeField]
	private UILabel _loseLabel;

	[SerializeField]
	private UISprite _stateIcon;

	[SerializeField]
	private UILabel _titleLabel;

	private readonly string[] STATE_ICON_SPRITE_NAME_TABLE = new string[4] { "orb_empty", "orb_win", "orb_lose", "orb_empty" };

	private readonly int[] STATE_ICON_SPRITE_SIZE_TABLE = new int[4] { 25, 70, 70, 70 };

	public eState State { get; private set; }

	public bool IsWon => State == eState.Won;

	public bool IsLost => State == eState.Lost;

	public void ChangeState(eState state)
	{
		State = state;
		_nextBattleMark.SetActive(state == eState.Next);
		_winLabel.gameObject.SetActive(state == eState.Won);
		_loseLabel.gameObject.SetActive(state == eState.Lost);
		_stateIcon.spriteName = STATE_ICON_SPRITE_NAME_TABLE[(int)state];
		UISprite stateIcon = _stateIcon;
		int width = (_stateIcon.height = STATE_ICON_SPRITE_SIZE_TABLE[(int)state]);
		stateIcon.width = width;
	}

	public void SetTitleText(string text)
	{
		_titleLabel.text = text;
	}
}
