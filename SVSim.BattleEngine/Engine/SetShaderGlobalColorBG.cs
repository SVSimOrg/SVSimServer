using System.Collections;
using UnityEngine;

public class SetShaderGlobalColorBG : MonoBehaviour
{
	private readonly Color FROM_COLOR = new Color(1f, 1f, 1f, 1f);

	private readonly Color TO_COLOR = new Color(0.3529412f, 0.3529412f, 0.3529412f, 1f);

	private readonly float fadeTime = 0.3f;

	private IEnumerator interpolateGlobalShaderColor;

	public bool IsFadeIn { get; private set; }

	public void ChangeGlobalShaderColorFadeIn()
	{
		ChangeGlobalShaderColor(FROM_COLOR, TO_COLOR);
		IsFadeIn = true;
	}

	private void ChangeGlobalShaderColor(Color baseColor, Color changeColor)
	{
		StopCoroutine(interpolateGlobalShaderColor);
		interpolateGlobalShaderColor = InterpolateGlobalShaderColor(baseColor, changeColor, Time.time);
		StartCoroutine(interpolateGlobalShaderColor);
	}

	private IEnumerator InterpolateGlobalShaderColor(Color fromColor, Color toColor, float startTime)
	{
		float t = 0f;
		while (t < 1f)
		{
			t = (Time.time - startTime) / fadeTime;
			Color value = Color.Lerp(fromColor, toColor, t);
			Shader.SetGlobalColor("_ColorBG", value);
			yield return null;
		}
	}
}
