using UnityEngine;

namespace Wizard;

public static class MaterialDefine
{
	public static Material CreateNguiStencilMaskMaterial(int stencil = 1)
	{
		Material material = new Material(Shader.Find("/Unlit/UIMask"));
		material.SetInt("_StencilMask", stencil);
		return material;
	}
}
