using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wizard;

public class UISpriteAtlasOverwriter : MonoBehaviour
{
	private class SpriteAtlasPair
	{
		public UISprite Sprite { get; }

		public UIAtlas Atlas { get; }

		public SpriteAtlasPair(UISprite sprite)
		{
			Sprite = sprite;
			Atlas = sprite.atlas;
		}
	}

	public class TargetObject
	{
		private readonly GameObject _rootObject;

		private readonly bool _includeChildren;

		public TargetObject(GameObject rootObject, bool includeChildren)
		{
			_rootObject = rootObject;
			_includeChildren = includeChildren;
		}
	}

	private UIAtlas _atlas;

	private TargetObject[] _targetObjects;

	private List<TargetObject> _exceptionObjects;

	private readonly List<SpriteAtlasPair> _originalSpriteAtlasPairs = new List<SpriteAtlasPair>();

	public void Init(UIAtlas atlas, TargetObject[] targetObjects)
	{
		UndoParameters();
		_atlas = atlas;
		_targetObjects = targetObjects;
	}

	public void AddExceptionObjects(List<TargetObject> exceptionObjects)
	{
		if (_exceptionObjects == null)
		{
			_exceptionObjects = new List<TargetObject>();
		}
		_exceptionObjects.AddRange(exceptionObjects);
	}

	private void UndoParameters()
	{
		foreach (SpriteAtlasPair originalSpriteAtlasPair in _originalSpriteAtlasPairs)
		{
			if (originalSpriteAtlasPair.Sprite != null)
			{
				originalSpriteAtlasPair.Sprite.atlas = originalSpriteAtlasPair.Atlas;
			}
		}
		_originalSpriteAtlasPairs.Clear();
	}
}
