using System.Collections.Generic;
using Wizard.Battle;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class NullBattleCard : BattleCardBase
{
	private static BuildInfo _dummyBuildInfo;

	private static NullSkillApplyInformation _dummySkillApplyInfo;

	private static BattleCardView.BuildInfo _dummyViewBuildInfo;

	private static NullBattleCardView _dummyView;

	public override int Index => -1;

	public override List<CardBasePrm.TribeType> Tribe => CardBasePrm.DefaultType;

	public override bool IsInHand => false;

	public override bool IsInplay => false;

	public override bool IsInDeck => false;

	public override bool IsInCemetery => true;

	public static void ReleaseSharedDummy()
	{
		_dummyBuildInfo = null;
		_dummySkillApplyInfo = null;
		_dummyViewBuildInfo = null;
		_dummyView = null;
	}

	public static NullBattleCard Create()
	{
		if (_dummyBuildInfo == null)
		{
			_dummyBuildInfo = new BuildInfo(null, -1, null, null, null, null, null, _isPlayer: true, 0, NullInnerOptionsBuilder.GetInstance().CreateCardOptions(), null, null);
		}
		NullBattleCard nullBattleCard = new NullBattleCard(_dummyBuildInfo);
		nullBattleCard.Setup(createNullView: true);
		return nullBattleCard;
	}

	protected override void InitSkillCollection()
	{
		_normalSkillCollection = null;
		_evolveSkillCollection = null;
	}

	private NullBattleCard(BuildInfo buildInfo)
		: base(buildInfo)
	{
	}


	public override BattleCardBase VirtualClone(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer)
	{
		return this;
	}

	protected override BattleCardView.BuildInfo CreateViewBuildInfo(BuildInfo baseBuildInfo)
	{
		if (_dummyViewBuildInfo == null)
		{
			_dummyViewBuildInfo = new BattleCardView.BuildInfo(this, null, null, null, null, null, null, null, null, null, null, null, null, null);
		}
		return _dummyViewBuildInfo;
	}

	protected override IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool isNullView)
	{
		if (_dummyView == null)
		{
			_dummyView = new NullBattleCardView(buildInfo);
		}
		return _dummyView;
	}

	protected override ISkillApplyInformation CreateSkillApplyInformation(BattleCardBase card)
	{
		if (_dummySkillApplyInfo == null)
		{
			_dummySkillApplyInfo = new NullSkillApplyInformation();
		}
		return _dummySkillApplyInfo;
	}
}
