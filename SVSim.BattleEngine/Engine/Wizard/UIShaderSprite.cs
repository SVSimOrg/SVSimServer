using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class UIShaderSprite : UISprite
{
	[SerializeField]
	private Shader _shader;

	private Material _material;

	private bool _isSharedMaterial = true;

	private static Dictionary<int, Dictionary<int, Material>> _materialDic = new Dictionary<int, Dictionary<int, Material>>();

	public override Material material
	{
		get
		{
			if (_shader != null && base.atlas != null)
			{
				if (_material == null || mChanged)
				{
					if (!_materialDic.TryGetValue(base.atlas.GetInstanceID(), out var value))
					{
						value = (_materialDic[base.atlas.GetInstanceID()] = new Dictionary<int, Material>());
					}
					int key = (_isSharedMaterial ? _shader.GetInstanceID() : GetInstanceID());
					if ((!value.TryGetValue(key, out _material) || _material == null) && base.atlas != null)
					{
						Dictionary<int, Material> dictionary2 = value;
						Material obj = new Material(base.atlas.spriteMaterial)
						{
							shader = _shader
						};
						Material value2 = obj;
						_material = obj;
						dictionary2[key] = value2;
					}
				}
				return _material;
			}
			return base.atlas?.spriteMaterial;
		}
	}

	public void SetShader(Shader shader, bool isSharedMaterial = true)
	{
		_shader = shader;
		_isSharedMaterial = isSharedMaterial;
		mChanged = true;
		if (drawCall != null)
		{
			drawCall.shader = shader;
		}
	}
}
