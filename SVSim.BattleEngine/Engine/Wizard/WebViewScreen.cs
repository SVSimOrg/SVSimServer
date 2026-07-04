using UnityEngine;

namespace Wizard;

[RequireComponent(typeof(BoxCollider))]
public class WebViewScreen : MonoBehaviour
{
	[SerializeField]
	private BoxCollider curBoxCollider;

	public BoxCollider CurBoxCollider => curBoxCollider;
}
