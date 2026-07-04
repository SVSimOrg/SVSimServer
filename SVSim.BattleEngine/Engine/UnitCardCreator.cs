using UnityEngine;
using Wizard;

public class UnitCardCreator : CardCreatorBase
{
	public UnitCardCreator(GameObject rootObject)
		: base(rootObject)
	{
	}

	public static void SetupUnitCardMaterialToCardMesh(Transform rootCardTransform, CardParameter cardBasePrm, Material normalCardArtMaterial)
	{
		Transform transform = rootCardTransform.Find("Normal");
		UIManager.GetInstance().SetLayerRecursive(transform, 10);
		MeshRenderer component = rootCardTransform.Find("NormalField").GetComponent<MeshRenderer>();
		LOD[] lODs = transform.GetComponent<LODGroup>().GetLODs();
		Material[] sharedMaterials = lODs[0].renderers[0].sharedMaterials;
		Material[] sharedMaterials2 = component.sharedMaterials;
		int rarity = cardBasePrm.Rarity;
		Material handUnitFrame = SBattleLoad.CardFrameMaterialHolder.GetHandUnitFrame(rarity);
		Material inPlayNormalUnitFrame = SBattleLoad.CardFrameMaterialHolder.GetInPlayNormalUnitFrame(rarity);
		sharedMaterials[0] = handUnitFrame;
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
