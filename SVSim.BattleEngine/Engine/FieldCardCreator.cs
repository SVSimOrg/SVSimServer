using UnityEngine;
using Wizard;

public class FieldCardCreator : CardCreatorBase
{
	public FieldCardCreator(GameObject rootObject)
		: base(rootObject)
	{
	}

	public static void SetupFieldCardMaterialToCardMesh(Transform rootCardTransform, CardParameter cardBasePrm, Material normalCardArtMaterial)
	{
		MeshRenderer component = rootCardTransform.transform.Find("NormalField").GetComponent<MeshRenderer>();
		Transform transform = rootCardTransform.Find("Normal");
		LOD[] lODs = transform.GetComponent<LODGroup>().GetLODs();
		UIManager.GetInstance().SetLayerRecursive(transform, 10);
		Material[] sharedMaterials = lODs[0].renderers[0].sharedMaterials;
		Material[] sharedMaterials2 = component.sharedMaterials;
		int rarity = cardBasePrm.Rarity;
		Material handFieldFrame = SBattleLoad.CardFrameMaterialHolder.GetHandFieldFrame(rarity);
		Material inPlayNormalUnitFrame = SBattleLoad.CardFrameMaterialHolder.GetInPlayNormalUnitFrame(rarity);
		handFieldFrame.shader = Shader.Find(handFieldFrame.shader.name);
		inPlayNormalUnitFrame.shader = Shader.Find(inPlayNormalUnitFrame.shader.name);
		sharedMaterials[0] = handFieldFrame;
		sharedMaterials2[0] = inPlayNormalUnitFrame;
		sharedMaterials[1] = (sharedMaterials2[1] = normalCardArtMaterial);
		sharedMaterials[2] = CardCreatorBase.GetSharedClassIconMaterial(cardBasePrm.Clan);
		for (int i = 0; i < lODs.Length; i++)
		{
			lODs[i].renderers[0].sharedMaterials = sharedMaterials;
		}
		component.sharedMaterials = sharedMaterials2;
		component.materials[1].mainTextureScale = cardBasePrm.NormalTilling;
		component.materials[1].mainTextureOffset = cardBasePrm.NormalOffset;
	}
}
