using UnityEngine;
using Wizard;

public static class CardShaderDefine
{

	public static void ReplaceShader(Material mat)
	{
		if (!(mat == null))
		{
			if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_FOIL_CARD_ANIMATION))
			{
				mat.shader = Shader.Find(mat.shader.name);
			}
			else
			{
				mat.shader = Shader.Find("Wizard/Card/Basic_Front0_Back0_Normal");
			}
		}
	}

	public static void ReplaceBaseShader(Material mat, bool isFoil)
	{
		if (!(mat == null))
		{
			if (isFoil && mat.shader.name != "Wizard/VariantCardShader")
			{
				mat.shader = Shader.Find("Wizard/VariantCardShader");
			}
			else
			{
				mat.shader = Shader.Find(mat.shader.name);
			}
		}
	}
}
