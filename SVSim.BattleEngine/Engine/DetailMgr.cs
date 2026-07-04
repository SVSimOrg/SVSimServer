using UnityEngine;

public class DetailMgr
{
	public GameObject DetailNormal;

	public LODGroup DetailNormalLodGroup;

	public MeshRenderer DetailNormalBaseMesh;

	public UILabel DetailNormalCostLabel;

	public UILabel DetailNormalAtkLabel;

	public UILabel DetailNormalLifeLabel;

	public UILabel DetailNormalNameLabel;

	public GameObject DetailSkill;

	public MeshRenderer DetailSkillBaseMesh;

	public LODGroup DetailSkillLodGroup;

	public UILabel DetailSkillCostLabel;

	public UILabel DetailSkillNameLabel;

	public GameObject DetailField;

	public MeshRenderer DetailFieldBaseMesh;

	public LODGroup DetailFieldLodGroup;

	public UILabel DetailFieldCostLabel;

	public UILabel DetailFieldNameLabel;

	public GameObject DetailPanel;

	public IDetailPanelControl DetailPanelControl;

	public GameObject SubDetailPanel;

	public DetailPanelControl SubDetailPanelControl;

	public DetailMgr()
	{
		DetailNormal = null;
		DetailNormalLodGroup = null;
		DetailNormalBaseMesh = null;
		DetailNormalCostLabel = null;
		DetailNormalAtkLabel = null;
		DetailNormalLifeLabel = null;
		DetailNormalNameLabel = null;
		DetailSkill = null;
		DetailSkillBaseMesh = null;
		DetailSkillLodGroup = null;
		DetailSkillCostLabel = null;
		DetailSkillNameLabel = null;
		DetailField = null;
		DetailFieldBaseMesh = null;
		DetailFieldLodGroup = null;
		DetailFieldCostLabel = null;
		DetailFieldNameLabel = null;
		DetailPanel = null;
		DetailPanelControl = null;
	}

	public void Dispose()
	{
		DetailPanel = null;
		DetailPanelControl = null;
	}

	public static Camera GetCamera()
	{
		return UIManager.GetInstance().UIRootLoadingCamera;
	}
}
