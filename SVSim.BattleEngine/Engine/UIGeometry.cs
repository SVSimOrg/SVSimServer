using UnityEngine;

public class UIGeometry
{
	public BetterList<Vector3> verts = new BetterList<Vector3>();

	public bool hasVertices => verts.size > 0;
}
