using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar
{
	public class CustomFaction_HealthUtility
	{
		public static void DamageUntilDownedWithSpecialOptions(Pawn p, bool allowBleedingWounds = true, DamageDef damageDef = null, ThingDef weapon = null)
		{
			if (!p.health.Downed)
			{
				if (p.apparel != null)
				{
					foreach (Apparel apparel in p.apparel.WornApparel)
					{
						if (apparel.def.StatBaseDefined(StatDefOf.SmokepopBeltRadius))
						{ p.apparel.Remove(apparel);
							break;
						}
					}
				}

				HediffSet hediffSet = p.health.hediffSet;
				p.health.forceIncap = true;
				IEnumerable<BodyPartRecord> source = (from x in HittablePartsViolence(hediffSet)
													 where !p.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
													 select x).ToList();
				int num = 0;
				while (num < 300 && !p.Downed && source.Any<BodyPartRecord>())
				{
					num++;
					BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
					int num2 = Mathf.RoundToInt(hediffSet.GetPartHealth(bodyPartRecord)) - 3;
					if (num2 >= 8)
					{
						if (bodyPartRecord.depth == BodyPartDepth.Outside)
						{
							if (!allowBleedingWounds && bodyPartRecord.def.bleedRate > 0f)
							{
								damageDef = DamageDefOf.Blunt;
							}
							else
							{
								if (damageDef == null || damageDef == DamageDefOf.Flame)
									damageDef = HealthUtility.RandomViolenceDamageType();
							}
						}
						else
						{
							damageDef = DamageDefOf.Blunt;
						}
						int num3 = Rand.RangeInclusive(Mathf.RoundToInt((float)num2 * 0.65f), num2);
						HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
						if (!p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, (float)num3))
						{
							DamageDef def = damageDef;
							float amount = (float)num3;
							float armorPenetration = 999f;
							BodyPartRecord hitPart = bodyPartRecord;
							DamageInfo dinfo = new DamageInfo(def, amount, armorPenetration, -1f, null, hitPart, weapon);
							dinfo.SetAllowDamagePropagation(false);
							p.TakeDamage(dinfo);
						}
					}
				}
				if (p.Dead)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(p + " died during GiveInjuriesToForceDowned");
					for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
					{
						stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i].ToString());
					}
					Log.Error(stringBuilder.ToString());
				}
				p.health.forceIncap = false;
			}
		}

		private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
		{
			return from x in bodyModel.GetNotMissingParts()
					where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
					select x;
		}
	}
}
