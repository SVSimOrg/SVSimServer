using System.Collections;
using UnityEngine;
using Wizard.Battle.View.Vfx;

public class TurnPanelControl : MonoBehaviour, ITurnPanelControl
{
	[SerializeField]
	private UIPanel TurnPanel;

	[SerializeField]
	private UISprite TurnTitle;

	[SerializeField]
	private UISprite SubTurnTitle;

	[SerializeField]
	private UILabel SubTurnLabel;

	[SerializeField]
	private UITable SubEvoTable;

	[SerializeField]
	private UISprite SubEvoTitle;

	[SerializeField]
	private UISprite SubEvoTurn;

	[SerializeField]
	private UILabel SubEvoLabel;

	[SerializeField]
	private UIPanel EvoPanel;

	[SerializeField]
	private UIPanel ArcanePanel;

	[SerializeField]
	private UISprite ArcaneOut;

	[SerializeField]
	private UISprite ArcaneIn;

	[SerializeField]
	private UISprite Bg;

	private int evoCnt;

	private bool isPlayer;

	[HideInInspector]
	public bool isEvoEnableP;

	[HideInInspector]
	public bool isEvoEnableE;

	private IEnumerator _turnStartSequenceEnumerator;

	public GameObject GameObject => base.gameObject;

	public void Initialize(bool isEvoEnableP = true, bool isEvoEnableE = true)
	{
		this.isEvoEnableP = isEvoEnableP;
		this.isEvoEnableE = isEvoEnableE;
		TurnPanel.alpha = 0f;
		EvoPanel.alpha = 0f;
		ArcanePanel.alpha = 0f;
		Bg.alpha = 0f;
		_turnStartSequenceEnumerator = RunUI();
	}

	public VfxBase LoadResource()
	{
		return NullVfx.GetInstance();
	}

	public void StartUI(int turn, int evo, bool isP)
	{
		base.gameObject.SetActive(value: true);
		evoCnt = evo;
		isPlayer = isP;
		if (isPlayer)
		{
			TurnTitle.spriteName = "img_your_turn_panel_01";
			SubTurnTitle.spriteName = "img_your_turn_count_panel_01";
			SubEvoTitle.spriteName = "text_battle_evo_left_01";
			if (evoCnt + 1 == 1)
			{
				SubEvoTurn.spriteName = "text_battle_evo_right_01_1";
			}
			else
			{
				SubEvoTurn.spriteName = "text_battle_evo_right_01_2";
			}
			ArcaneIn.spriteName = "arcane_01_ring";
			ArcaneOut.spriteName = "arcane_01_ring";
			SubTurnLabel.effectColor = new Color(0f, 0.5f, 0.9f, 0.5f);
			SubEvoLabel.effectColor = new Color(0f, 0.5f, 0.9f, 0.5f);
			if (isEvoEnableP)
			{
				SubTurnTitle.gameObject.SetActive(value: false);
				SubEvoTable.gameObject.SetActive(value: true);
				SubEvoLabel.text = (evoCnt + 1).ToString();
			}
			else
			{
				SubTurnTitle.gameObject.SetActive(value: true);
				SubEvoTable.gameObject.SetActive(value: false);
				SubTurnLabel.text = turn.ToString();
			}
		}
		else
		{
			TurnTitle.spriteName = "img_your_turn_panel_02";
			SubTurnTitle.spriteName = "img_your_turn_count_panel_02";
			SubEvoTitle.spriteName = "text_battle_evo_left_02";
			if (evoCnt + 1 == 1)
			{
				SubEvoTurn.spriteName = "text_battle_evo_right_02_1";
			}
			else
			{
				SubEvoTurn.spriteName = "text_battle_evo_right_02_2";
			}
			ArcaneIn.spriteName = "arcane_02_ring";
			ArcaneOut.spriteName = "arcane_02_ring";
			SubTurnLabel.effectColor = new Color(0.75f, 0f, 0.25f, 0.5f);
			SubEvoLabel.effectColor = new Color(0.75f, 0f, 0.25f, 0.5f);
			if (isEvoEnableE)
			{
				SubTurnTitle.gameObject.SetActive(value: false);
				SubEvoTable.gameObject.SetActive(value: true);
				SubEvoLabel.text = (evoCnt + 1).ToString();
			}
			else
			{
				SubTurnTitle.gameObject.SetActive(value: true);
				SubEvoTable.gameObject.SetActive(value: false);
				SubTurnLabel.text = turn.ToString();
			}
		}
		SnapSprites();
		TurnPanel.alpha = 0f;
		EvoPanel.alpha = 0f;
		ArcanePanel.alpha = 0f;
		Bg.alpha = 0f;
		StopCoroutine(_turnStartSequenceEnumerator);
		_turnStartSequenceEnumerator = RunUI();
		StartCoroutine(_turnStartSequenceEnumerator);
	}

	private IEnumerator RunUI()
	{
		TweenAlpha.Begin(Bg.gameObject, 0.2f, 0.5f);
		TweenAlpha.Begin(TurnPanel.gameObject, 0.2f, 1f);
		TurnPanel.transform.localScale = Vector3.one * 0.01f;
		iTween.ScaleTo(TurnPanel.gameObject, iTween.Hash("scale", Vector3.one * 0.95f, "time", 0.2f, "islocal", true, "easetype", iTween.EaseType.easeOutQuad));
		TweenAlpha.Begin(ArcanePanel.gameObject, 0.2f, 1f);
		ArcanePanel.transform.localScale = Vector3.one * 10f;
		iTween.ScaleTo(ArcanePanel.gameObject, iTween.Hash("scale", Vector3.one * 1.05f, "time", 0.2f, "islocal", true, "easetype", iTween.EaseType.easeOutQuad));
		yield return new WaitForSeconds(0.2f);
		TurnPanel.transform.localScale = Vector3.one * 0.95f;
		iTween.ScaleTo(TurnPanel.gameObject, iTween.Hash("scale", Vector3.one, "time", 1f, "islocal", true, "easetype", iTween.EaseType.easeOutQuad));
		ArcanePanel.transform.localScale = Vector3.one * 1.05f;
		iTween.ScaleTo(ArcanePanel.gameObject, iTween.Hash("scale", Vector3.one, "time", 1f, "islocal", true, "easetype", iTween.EaseType.easeOutQuad));
		yield return new WaitForSeconds(0.5f);
		if ((isPlayer && isEvoEnableP) || (!isPlayer && isEvoEnableE))
		{
			SubEvoLabel.transform.localScale = Vector3.one;
			if (isPlayer)
			{
				if (evoCnt == 1)
				{
					SubEvoTurn.spriteName = "text_battle_evo_right_01_1";
				}
				else
				{
					SubEvoTurn.spriteName = "text_battle_evo_right_01_2";
				}
			}
			else if (evoCnt == 1)
			{
				SubEvoTurn.spriteName = "text_battle_evo_right_02_1";
			}
			else
			{
				SubEvoTurn.spriteName = "text_battle_evo_right_02_2";
			}
			SnapSprites();
			yield return new WaitForSeconds(0.05f);
			SubEvoLabel.text = evoCnt.ToString();
			SubEvoLabel.transform.localScale = Vector3.one * 1.5f;
			iTween.ScaleTo(SubEvoLabel.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.2f, "islocal", true, "easetype", iTween.EaseType.easeOutQuad));
			yield return new WaitForSeconds(0.45f);
		}
		else
		{
			yield return new WaitForSeconds(0.5f);
		}
		iTween.ScaleTo(TurnPanel.gameObject, iTween.Hash("scale", Vector3.one * 0.01f, "time", 0.4f, "islocal", true, "easetype", iTween.EaseType.easeInQuad));
		iTween.ScaleTo(ArcanePanel.gameObject, iTween.Hash("scale", Vector3.one * 10f, "time", 0.4f, "islocal", true, "easetype", iTween.EaseType.easeInExpo));
		TweenAlpha.Begin(TurnPanel.gameObject, 0.4f, 0f);
		TweenAlpha.Begin(ArcanePanel.gameObject, 0.4f, 0f);
		if (evoCnt == 0 && ((isPlayer && isEvoEnableP) || (!isPlayer && isEvoEnableE)))
		{
			yield return new WaitForSeconds(0.2f);
			// Pre-Phase-5b: registered a NullVfx on the mgr's VfxMgr — a no-op VFX slot.
			// Dropping the ambient reach; the yield-based pacing above/below is preserved.
			yield return new WaitForSeconds(0.2f);
			EvoPanel.alpha = 1f;
			EvoPanel.transform.localScale = Vector3.one * 1.05f;
			iTween.ScaleTo(EvoPanel.gameObject, iTween.Hash("scale", Vector3.one, "time", 1.5f, "islocal", true, "easetype", iTween.EaseType.linear));
			yield return new WaitForSeconds(1.2f);
			TweenAlpha.Begin(EvoPanel.gameObject, 0.3f, 0f);
			if (isPlayer)
			{
				isEvoEnableP = false;
			}
			else
			{
				isEvoEnableE = false;
			}
		}
		TweenAlpha.Begin(Bg.gameObject, 0.2f, 0f);
		yield return new WaitForSeconds(0.3f);
		base.gameObject.SetActive(value: false);
	}

	private void SnapSprites()
	{
		TurnTitle.MakePixelPerfect();
		SubEvoTitle.MakePixelPerfect();
		SubEvoTurn.MakePixelPerfect();
		SubEvoTable.Reposition();
	}
}
