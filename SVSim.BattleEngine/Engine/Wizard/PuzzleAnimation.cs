using System.Collections;
using System.Linq;
using UnityEngine;

namespace Wizard;

public class PuzzleAnimation : MonoBehaviour
{
	[SerializeField]
	private UISprite TitleFailed;

	[SerializeField]
	private UISprite TitleReset;

	[SerializeField]
	private UITexture Bg;

	[SerializeField]
	private UITexture FieldHideBg;

	[SerializeField]
	private UIPanel MainPanel;

	[SerializeField]
	private UISprite ArcaneIn;

	[SerializeField]
	private UISprite ArcaneOut;

	private bool _isEndFadeOut;

	public float FadeOutDuration { get; private set; } = 0.7f;

	public float FadeInDuration => 0.475f;

	public void Run(bool isReset)
	{
		StartCoroutine(FadeOut(isReset));
	}

	public void End()
	{
		StartCoroutine(FadeIn());
	}

	public void SetUp()
	{
		ArcaneIn.transform.localScale = Vector3.one * 0.01f;
		ArcaneOut.transform.localScale = Vector3.one * 0.01f;
		ArcaneIn.alpha = 0f;
		ArcaneOut.alpha = 0f;
		Bg.alpha = 0f;
		FieldHideBg.alpha = 0f;
		UIAtlas atlas = UIManager.GetInstance().GetAtlasList().FirstOrDefault((UIAtlas s) => s.name == "BattleLang");
		TitleFailed.atlas = atlas;
		TitleFailed.spriteName = "result_text_failed";
		TitleReset.atlas = atlas;
		TitleReset.spriteName = "result_text_resetting";
		/* Pre-Phase-5b: InitCommonEffect dropped */
		base.gameObject.SetLayer(26, isSetChildren: true);
		Bg.gameObject.SetActive(value: false);
		FieldHideBg.gameObject.SetActive(value: false);
	}

	public IEnumerator FadeOut(bool isReset)
	{
		_isEndFadeOut = false;
		Bg.gameObject.SetActive(value: true);
		FieldHideBg.gameObject.SetActive(value: true);
		FieldHideBg.alpha = 0f;
		if (isReset)
		{
			TitleFailed.gameObject.SetActive(value: false);
			TitleReset.gameObject.SetActive(value: true);
			TitleReset.transform.localScale = Vector3.one * 10f;
			TitleReset.alpha = 0f;
			Bg.color = new Color32(0, 48, 16, 0);
		}
		else
		{
			TitleFailed.gameObject.SetActive(value: true);
			TitleReset.gameObject.SetActive(value: false);
			TitleFailed.transform.localScale = Vector3.one * 10f;
			TitleFailed.alpha = 0f;
			Bg.color = new Color32(0, 24, 48, 0);
		}
		MainPanel.alpha = 1f;
		yield return new WaitForSeconds(0.1f);
		if (isReset)
		{
			TweenAlpha.Begin(TitleReset.gameObject, 0.2f, 1f);
			iTween.ScaleTo(TitleReset.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.2f, "islocal", true, "easetype", iTween.EaseType.easeInQuad));

		}
		else
		{
			TweenAlpha.Begin(TitleFailed.gameObject, 0.2f, 1f);
			iTween.ScaleTo(TitleFailed.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.2f, "islocal", true, "easetype", iTween.EaseType.easeInQuad));

		}
		TweenAlpha.Begin(Bg.gameObject, 0.5f, 0.75f);
		TweenAlpha.Begin(FieldHideBg.gameObject, 0.5f, 1f);
		yield return new WaitForSeconds(0.2f);
		TweenAlpha.Begin(ArcaneIn.gameObject, 0.5f, 1f);
		TweenAlpha.Begin(ArcaneOut.gameObject, 0.5f, 1f);
		iTween.ScaleTo(ArcaneIn.gameObject, iTween.Hash("scale", Vector3.one, "time", 2f, "islocal", true, "easetype", iTween.EaseType.easeOutExpo));
		iTween.ScaleTo(ArcaneOut.gameObject, iTween.Hash("scale", Vector3.one, "time", 2f, "islocal", true, "easetype", iTween.EaseType.easeOutExpo));
		if (isReset)
		{
			/* Pre-Phase-5b: Start CMN_RESULT_TITLE_3 dropped */
			TitleReset.transform.localScale = Vector3.one;
			iTween.ScaleTo(TitleReset.gameObject, iTween.Hash("scale", Vector3.one * 1.1f, "time", 2f, "islocal", true, "easetype", iTween.EaseType.linear));
		}
		else
		{
			/* Pre-Phase-5b: Start CMN_RESULT_TITLE_2 dropped */
			TitleFailed.transform.localScale = Vector3.one;
			iTween.ScaleTo(TitleFailed.gameObject, iTween.Hash("scale", Vector3.one * 1.1f, "time", 2f, "islocal", true, "easetype", iTween.EaseType.linear));
		}
		yield return new WaitForSeconds(2f);
		_isEndFadeOut = true;
	}

	public IEnumerator FadeIn()
	{
		while (!_isEndFadeOut)
		{
			yield return null;
		}
		TweenAlpha.Begin(TitleFailed.gameObject, 0.15f, 0f);
		TweenAlpha.Begin(TitleReset.gameObject, 0.15f, 0f);
		TweenAlpha.Begin(ArcaneIn.gameObject, 0.15f, 0f);
		TweenAlpha.Begin(ArcaneOut.gameObject, 0.15f, 0f);
		yield return new WaitForSeconds(0.22500001f);
		TweenAlpha.Begin(FieldHideBg.gameObject, 0.15f, 0f);
		TweenAlpha.Begin(Bg.gameObject, 0.15f, 0f);
		yield return new WaitForSeconds(0.15f);
		Bg.alpha = 0f;
		FieldHideBg.alpha = 0f;
		TweenAlpha.Begin(FieldHideBg.gameObject, 0f, 0f);
		TweenAlpha.Begin(Bg.gameObject, 0f, 0f);
		Bg.gameObject.SetActive(value: false);
		FieldHideBg.gameObject.SetActive(value: false);
	}
}
