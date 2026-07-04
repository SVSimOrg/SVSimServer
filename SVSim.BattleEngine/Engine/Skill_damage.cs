using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_damage : SkillBase
{
	private struct BattleLogInfo
	{
		public int _damage;

		public BattleCardBase _damageBefore;

		public BattleCardBase _damageAfter;

		public BattleCardBase _refrectionDamageBefore;

		public BattleCardBase _refrectionDamageAfter;
	}

	public class PerTargetDamageInfo
	{
		public int Damage { get; private set; }

		public ApplySkillTargetFilterCollection Filter { get; private set; }

		public List<BattleCardBase> ApplyCards { get; private set; }

		private PerTargetDamageInfo(BattleCardBase owner, int damage, string filterString, SkillBase skill)
		{
			Damage = damage;
			Filter = new ApplySkillTargetFilterCollection();
			SkillFilterCreator.SetupTarget(Filter, filterString, owner, skill);
		}

		public void SetResult(BattlePlayerReadOnlyInfoPair pair, SkillConditionCheckerOption checker, SkillOptionValue optionValue)
		{
			ApplyCards = Filter.Filtering(pair, checker, optionValue).Cast<BattleCardBase>().ToList();
		}

		protected static string KeyBracketValue(string text, string key)
		{
			int num = text.IndexOf(key);
			if (num == -1)
			{
				return string.Empty;
			}
			text = text.Substring(num);
			num = text.IndexOf(")");
			if (num == -1)
			{
				return string.Empty;
			}
			return text.Substring(key.Length, num - key.Length);
		}

		public static List<PerTargetDamageInfo> MakeList(BattleCardBase owner, string text, SkillBase skill)
		{
			List<string> list = new List<string>();
			string text2 = string.Empty;
			int num = 0;
			while (text.Length > 0)
			{
				switch (text[0])
				{
				case ':':
					if (num == 0)
					{
						list.Add(text2);
						text2 = string.Empty;
					}
					else
					{
						text2 += text[0];
					}
					break;
				case '(':
					num++;
					text2 += text[0];
					break;
				case ')':
					num--;
					text2 += text[0];
					break;
				default:
					text2 += text[0];
					break;
				}
				text = text.Substring(1, text.Length - 1);
			}
			if (text2 != string.Empty)
			{
				list.Add(text2);
			}
			List<PerTargetDamageInfo> list2 = new List<PerTargetDamageInfo>();
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(new PerTargetDamageInfo(owner, int.Parse(KeyBracketValue(list[i], "damage:")), KeyBracketValue(list[i], "target:"), skill));
			}
			return list2;
		}
	}

	public override bool IsTargetIndicate => false;

	public Skill_damage(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter callParameter)
	{
		List<BattleCardBase> targets = callParameter.targetCards.Where((BattleCardBase t) => !t.IsDead).ToList();
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "_OPT_NULL_");
		string text2 = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.per_target_damage, string.Empty);
		if (text2 != string.Empty)
		{
			return TakeDamagePerTarget(targets, callParameter.skillProcessor, text2);
		}
		if (text == "each_target")
		{
			return TakeRandomDamageToEachTarget(targets, callParameter.skillProcessor);
		}
		if (text == "oldest_each_target")
		{
			return TakeDamageToOldestEachTarget(targets, callParameter.skillProcessor);
		}
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.damage, -1);
		if (num == -1)
		{
			if (base.SkillPrm.selfBattlePlayer.AddDamageByClassUseCard(GetCardType()) == 0)
			{
				return NullVfxWithLoading.GetInstance();
			}
			num = 0;
		}
		if (text == "oldest")
		{
			return TakeUnfixedDamage(num, targets, callParameter.skillProcessor);
		}
		return TakeFixedDamage(num, targets, callParameter.skillProcessor);
	}

	private VfxWithLoading MakeSkillVfx(List<BattleCardBase> targets, List<BattleCardBase.DamageResult> damageResultList, SkillProcessor skillProcessor)
	{
		VfxWithLoading vfxWithLoading = CreateSkillEffect(base.SkillPrm.resourceMgr, targets);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer3 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer4 = ParallelVfxPlayer.Create();
		parallelVfxPlayer4.Register(base.SkillPrm.ownerCard.SkillApplyInformation.IsSneak ? base.SkillPrm.ownerCard.AfterAddDamage() : NullVfx.GetInstance());
		for (int i = 0; i < damageResultList.Count; i++)
		{
			parallelVfxPlayer.Register(damageResultList[i].PreDamageVfx);
			parallelVfxPlayer2.Register(damageResultList[i].Vfx);
			parallelVfxPlayer3.Register(damageResultList[i].PostDamageVfx);
		}
		ParallelVfxPlayer parallelVfxPlayer5 = ParallelVfxPlayer.Create();
		foreach (BattleCardBase target in targets)
		{
			BattleCardBase damageReflectionTarget = target.GetDamageReflectionTarget(isSkillDamage: true);
			if (damageReflectionTarget.IsDead)
			{
				parallelVfxPlayer5.Register(damageReflectionTarget.SelfBattlePlayer.CardManagement(damageReflectionTarget, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, base.UsedRandom, null, null, this));
			}
			else if (base.UsedRandom)
			{
				damageReflectionTarget.SelfBattlePlayer.PredictionDamageRandomCards.Add(damageReflectionTarget);
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create(parallelVfxPlayer, vfxWithLoading.MainVfx, parallelVfxPlayer4, parallelVfxPlayer2, parallelVfxPlayer3, parallelVfxPlayer5);
		vfxWithLoadingSequential.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
		return vfxWithLoadingSequential;
	}

	private void LoggingDamageResult(List<BattleLogInfo> battleLogInfo)
	{
		if (!IsBattleLog)
		{
			return;
		}
		int i = 0;
		for (int count = battleLogInfo.Count; i < count; i++)
		{
			if (battleLogInfo[i]._damageAfter != null)
			{
				BattleLogManager.GetInstance().AddLogSkillDamage(battleLogInfo[i]._damageBefore, battleLogInfo[i]._damageAfter, battleLogInfo[i]._refrectionDamageBefore, battleLogInfo[i]._refrectionDamageAfter, this);
			}
		}
	}

	private void TakeDamageSingle(BattleCardBase.DamageParam damageParam, BattleCardBase target, List<BattleCardBase> damageTargets, List<BattleLogInfo> battleLogInfo, List<BattleCardBase.DamageResult> damageResultList, SkillProcessor skillProcessor)
	{
		BattleCardBase refrectionDamageBefore = null;
		BattleCardBase battleCardBase = null;
		BattleCardBase battleCardBase2 = null;
		BattleCardBase battleCardBase3 = null;
		BattleCardBase damageReflectionTarget = target.GetDamageReflectionTarget(isSkillDamage: true);
		if (damageReflectionTarget != target)
		{
			damageTargets.Add(damageReflectionTarget);
			refrectionDamageBefore = damageReflectionTarget.VirtualClone(damageReflectionTarget.SelfBattlePlayer, damageReflectionTarget.OpponentBattlePlayer);
		}
		else
		{
			damageTargets.Add(target);
		}
		battleCardBase = target.VirtualClone(target.SelfBattlePlayer, target.OpponentBattlePlayer);
		BattleCardBase.DamageResult damageResult = target.ApplyDamage(this, damageParam, doesAttackerPossessKiller: false, isReflectedDamage: false, skillProcessor, (damageReflectionTarget != target) ? target : null);
		if (base.SkillPrm.ownerCard.IsUnit)
		{
			BattleManagerBase ins = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr;
			base.SkillPrm.ownerCard.SelfBattlePlayer.Class.SkillApplyInformation.CausedDamageLife(damageResult.DamageApplied, ins.CurrentTurn, ins.BattlePlayer.IsSelfTurn);
		}
		damageResultList.Add(damageResult);
		battleCardBase3 = target.VirtualClone(target.SelfBattlePlayer, target.OpponentBattlePlayer);
		battleCardBase2 = ((damageReflectionTarget == target) ? battleCardBase3 : damageReflectionTarget.VirtualClone(target.SelfBattlePlayer, target.OpponentBattlePlayer));
		if (IsBattleLog)
		{
			battleLogInfo.Add(new BattleLogInfo
			{
				_damageBefore = battleCardBase,
				_damageAfter = battleCardBase3,
				_refrectionDamageBefore = refrectionDamageBefore,
				_refrectionDamageAfter = battleCardBase2,
				_damage = damageParam.Damage
			});
		}
	}

	public string GetCardType()
	{
		if (base.SkillPrm.ownerCard is UnitBattleCard)
		{
			return SkillFilterCreator.ContentKeyword.unit.ToString();
		}
		if (base.SkillPrm.ownerCard is SpellBattleCard)
		{
			return SkillFilterCreator.ContentKeyword.spell.ToString();
		}
		if (base.SkillPrm.ownerCard is FieldBattleCard)
		{
			return SkillFilterCreator.ContentKeyword.field.ToString();
		}
		if (base.SkillPrm.ownerCard.IsClass)
		{
			return SkillFilterCreator.ContentKeyword._class.ToStringCustom();
		}
		return "";
	}

	private VfxWithLoading TakeFixedDamage(int damage, List<BattleCardBase> targets, SkillProcessor skillProcessor)
	{
		BattleCardBase.DamageParam damageParam = new BattleCardBase.DamageParam(damage, base.SkillPrm.ownerCard, GetCardType(), base.SkillPrm.ownerCard.Clan);
		int damage2 = damageParam.Damage;
		List<BattleCardBase> list = new List<BattleCardBase>();
		List<BattleLogInfo> battleLogInfo = new List<BattleLogInfo>();
		List<BattleCardBase.DamageResult> list2 = new List<BattleCardBase.DamageResult>();
		for (int i = 0; i < targets.Count(); i++)
		{
			TakeDamageSingle(damageParam, targets[i], list, battleLogInfo, list2, skillProcessor);
		}
		for (int j = 0; j < targets.Count(); j++)
		{
			RegisterDamageTriggerSkill(skillProcessor, new List<BattleCardBase> { list[j] }, damage2, list2[j]);
		}
		VfxWithLoading result = MakeSkillVfx(targets, list2, skillProcessor);
		LoggingDamageResult(battleLogInfo);
		return result;
	}

	private VfxWithLoading TakeUnfixedDamage(int damage, List<BattleCardBase> targets, SkillProcessor skillProcessor)
	{
		BattleCardBase.DamageParam damageParam = new BattleCardBase.DamageParam(damage, base.SkillPrm.ownerCard, GetCardType(), base.SkillPrm.ownerCard.Clan);
		List<BattleLogInfo> battleLogInfo = new List<BattleLogInfo>();
		List<BattleCardBase.DamageResult> list = new List<BattleCardBase.DamageResult>();
		int num = damageParam.Damage;
		if (targets.Count >= 2 && targets[0].IsClass)
		{
			BattleCardBase item = targets[0];
			targets.RemoveAt(0);
			targets.Add(item);
		}
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		for (int i = 0; i < targets.Count(); i++)
		{
			List<BattleCardBase> list3 = new List<BattleCardBase>();
			int num2 = ((i == targets.Count - 1) ? num : Math.Min(num, targets[i].Life));
			num -= num2;
			damageParam.Damage = num2;
			TakeDamageSingle(damageParam, targets[i], list3, battleLogInfo, list, skillProcessor);
			RegisterDamageTriggerSkill(skillProcessor, list3, num2, list[i]);
			list2.Add(targets[i]);
			if (num <= 0)
			{
				break;
			}
		}
		VfxWithLoading result = MakeSkillVfx(list2, list, skillProcessor);
		LoggingDamageResult(battleLogInfo);
		return result;
	}

	public virtual void RegisterDamageTriggerSkill(SkillProcessor skillProcessor, IEnumerable<BattleCardBase> target, int defDamage, BattleCardBase.DamageResult damageResult)
	{
		base.SkillPrm.selfBattlePlayer.StartSkillWhenDamageSelfAndOther(this, target.ToList(), skillProcessor, defDamage, damageResult.DamageApplied);
	}

	private VfxWithLoading TakeDamagePerTarget(List<BattleCardBase> targets, SkillProcessor skillProcessor, string perTargetDamageText)
	{
		List<BattleLogInfo> battleLogInfo = new List<BattleLogInfo>();
		List<BattleCardBase.DamageResult> list = new List<BattleCardBase.DamageResult>();
		List<PerTargetDamageInfo> list2 = PerTargetDamageInfo.MakeList(base.SkillPrm.ownerCard, perTargetDamageText, this);
		BattlePlayerReadOnlyInfoPair pair = new BattlePlayerReadOnlyInfoPair(base.SkillPrm.selfBattlePlayer, base.SkillPrm.opponentBattlePlayer);
		SkillConditionCheckerOption checker = new SkillConditionCheckerOption();
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].SetResult(pair, checker, base.OptionValue);
		}
		int num = base.SkillPrm.selfBattlePlayer.AddDamageByClassUseCard(GetCardType());
		bool flag = false;
		int j;
		for (j = 0; j < targets.Count; j++)
		{
			PerTargetDamageInfo perTargetDamageInfo = list2.FirstOrDefault((PerTargetDamageInfo d) => d.ApplyCards.Contains(targets[j]));
			if (perTargetDamageInfo == null)
			{
				continue;
			}
			List<BattleCardBase> list3 = new List<BattleCardBase>();
			int num2 = perTargetDamageInfo.Damage;
			if (num2 == -1)
			{
				if (num <= 0)
				{
					continue;
				}
				num2 = 0;
			}
			BattleCardBase.DamageParam damageParam = new BattleCardBase.DamageParam(num2, base.SkillPrm.ownerCard, GetCardType(), base.SkillPrm.ownerCard.Clan);
			TakeDamageSingle(damageParam, targets[j], list3, battleLogInfo, list, skillProcessor);
			int damage = damageParam.Damage;
			RegisterDamageTriggerSkill(skillProcessor, list3, damage, list[j]);
			flag = true;
		}
		if (!flag)
		{
			return NullVfxWithLoading.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(MakeSkillVfx(targets, list, skillProcessor));
		LoggingDamageResult(battleLogInfo);
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	private List<int> GetDamageList(SkillFilterCreator.ContentKeyword damageType)
	{
		string stringAllParse = base.OptionValue.GetStringAllParse(damageType, ':');
		if (string.IsNullOrEmpty(stringAllParse))
		{
			return null;
		}
		string[] array = stringAllParse.Split(':');
		List<int> list = new List<int>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			int item;
			try
			{
				item = base.OptionValue.ParseInt(array[i]);
			}
			catch
			{
				item = -1;
			}
			list.Add(item);
		}
		return list;
	}

	private VfxWithLoading TakeRandomDamageToEachTarget(List<BattleCardBase> targets, SkillProcessor skillProcessor)
	{
		List<int> damageList = GetDamageList(SkillFilterCreator.ContentKeyword.random_damage);
		if (damageList == null)
		{
			return NullVfxWithLoading.GetInstance();
		}
		List<int> list = new List<int>();
		for (int i = 0; i < targets.Count; i++)
		{
			int index = base.SkillPrm.selfBattlePlayer.BattleMgr.StableRandom(damageList.Count);
			list.Add(damageList[index]);
		}
		return TakeDamageToEachTarget(targets, skillProcessor, list);
	}

	private VfxWithLoading TakeDamageToOldestEachTarget(List<BattleCardBase> targets, SkillProcessor skillProcessor)
	{
		List<int> damageList = GetDamageList(SkillFilterCreator.ContentKeyword.damage);
		if (damageList == null)
		{
			return NullVfxWithLoading.GetInstance();
		}
		List<BattleCardBase> list = targets;
		if (targets.Count > damageList.Count)
		{
			list = list.GetRange(0, damageList.Count);
		}
		return TakeDamageToEachTarget(list, skillProcessor, damageList);
	}

	private VfxWithLoading TakeDamageToEachTarget(List<BattleCardBase> targets, SkillProcessor skillProcessor, List<int> damageList)
	{
		List<BattleLogInfo> battleLogInfo = new List<BattleLogInfo>();
		List<BattleCardBase.DamageResult> list = new List<BattleCardBase.DamageResult>();
		string cardType = GetCardType();
		int num = base.SkillPrm.selfBattlePlayer.AddDamageByClassUseCard(cardType);
		List<BattleCardBase.DamageParam?> list2 = new List<BattleCardBase.DamageParam?>();
		for (int i = 0; i < targets.Count; i++)
		{
			if (damageList[i] == -1)
			{
				if (num <= 0)
				{
					list2.Add(null);
					continue;
				}
				damageList[i] = 0;
			}
			list2.Add(new BattleCardBase.DamageParam(damageList[i], base.SkillPrm.ownerCard, GetCardType(), base.SkillPrm.ownerCard.Clan));
		}
		for (int j = 0; j < targets.Count; j++)
		{
			if (list2[j].HasValue)
			{
				BattleCardBase.DamageParam value = list2[j].Value;
				List<BattleCardBase> list3 = new List<BattleCardBase>();
				TakeDamageSingle(value, targets[j], list3, battleLogInfo, list, skillProcessor);
				int damage = value.Damage;
				RegisterDamageTriggerSkill(skillProcessor, list3, damage, list[j]);
			}
		}
		if (list2.Any((BattleCardBase.DamageParam? d) => d.HasValue))
		{
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			parallelVfxPlayer.Register(MakeSkillVfx(targets, list, skillProcessor));
			LoggingDamageResult(battleLogInfo);
			return VfxWithLoading.Create(parallelVfxPlayer);
		}
		return NullVfxWithLoading.GetInstance();
	}
}
