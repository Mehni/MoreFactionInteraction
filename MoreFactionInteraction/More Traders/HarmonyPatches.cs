using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;
using UnityEngine;

namespace MoreFactionInteraction
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Mehni.RimWorld.MFI.main");

            harmony.Patch(AccessTools.Method(typeof(TraderKindDef), nameof(TraderKindDef.PriceTypeFor)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(PriceTypeSetter_PostFix)), null);

            harmony.Patch(AccessTools.Method(typeof(StoryState), nameof(StoryState.Notify_IncidentFired)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(IncidentFired_TradeCounter_Postfix)), null);

            harmony.Patch(AccessTools.Method(typeof(CompQuality), nameof(CompQuality.PostPostGeneratedForTrader)),
                new HarmonyMethod(typeof(HarmonyPatches), nameof(CompQuality_TradeQualityIncreasePreFix)), null);
        }

        #region TradeQualityImprovements
        private static bool CompQuality_TradeQualityIncreasePreFix(CompQuality __instance, ref TraderKindDef trader, ref int forTile, ref Faction forFaction)
        {
            //forTile is assigned in RimWorld.ItemCollectionGenerator_TraderStock.Generate. It's either a best-effort map, or -1.
            Map map = null;
            if (forTile != -1) map = Current.Game.FindMap(forTile);
            __instance.SetQuality(FactionAndGoodWillDependantQuality(forFaction, map, trader), ArtGenerationContext.Outsider);
            return false;
        }

        /// <summary>
        /// Change quality carried by traders depending on Faction/Goodwill/Wealth.
        /// </summary>
        /// <returns>QualityCategory depending on wealth or goodwill. Fallsback to vanilla when fails.</returns>
        private static QualityCategory FactionAndGoodWillDependantQuality(Faction faction, Map map, TraderKindDef trader)
        {
            if (map != null && faction != null)
            {
                float qualityIncreaseFromTimesTradedWithFaction = Mathf.Clamp01(map.GetComponent<MapComponent_GoodWillTrader>().TimesTraded[faction] / 100);
                float qualityIncreaseFactorFromPlayerGoodWill = Mathf.Clamp01(faction.GoodwillWith(Faction.OfPlayer) / 100);

                if (Rand.Value < 0.25f)
                {
                    return QualityCategory.Normal;
                }
                float num = Rand.Gaussian(3.5f + qualityIncreaseFactorFromPlayerGoodWill, 1.13f + qualityIncreaseFromTimesTradedWithFaction);
                num = Mathf.Clamp(num, 0f, (float)QualityUtility.AllQualityCategories.Count - 0.5f);
                return (QualityCategory)((int)num);
            }
            if ((trader.orbital || trader.defName.Contains("_Base")) && map != null)
            {
                if (Rand.Value < 0.25f)
                {
                    return QualityCategory.Normal;
                }
                float num = Rand.Gaussian(WealthQualityDeterminationCurve.Evaluate(map.wealthWatcher.WealthTotal), WealthQualitySpreadDeterminationCurve.Evaluate(map.wealthWatcher.WealthTotal));
                num = Mathf.Clamp(num, 0f, (float)QualityUtility.AllQualityCategories.Count - 0.5f);
                return (QualityCategory)((int)num);
            }
            return QualityUtility.RandomTraderItemQuality();
        }

        #region SimpleCurves
        private static readonly SimpleCurve WealthQualityDeterminationCurve = new SimpleCurve
        {
            {
                new CurvePoint(0, 1),
                true
            },
            {
                new CurvePoint(10000, 2),
                true
            },
            {
                new CurvePoint(75000, 3),
                true
            },
            {
                new CurvePoint(300000, 4),
                true
            },
            {
                new CurvePoint(1000000, 5.5f),
                true
            },
            {
                new CurvePoint(2000000, 6.3f),
                true
            },
        };

        private static readonly SimpleCurve WealthQualitySpreadDeterminationCurve = new SimpleCurve
        {
            {
                new CurvePoint(0, 4.2f),
                true
            },
            {
                new CurvePoint(10000, 4),
                true
            },
            {
                new CurvePoint(75000, 3.5f),
                true
            },
            {
                new CurvePoint(300000, 2.5f),
                true
            },
            {
                new CurvePoint(1000000, 1.5f),
                true
            },
            {
                new CurvePoint(2000000, 1.2f),
                true
            },
        };
        #endregion SimpleCurves
        #endregion TradeQualityImprovements

        /// <summary>
        /// Increment TimesTraded count of dictionary by one for this faction.
        /// </summary>
        private static void IncidentFired_TradeCounter_Postfix(ref FiringIncident qi)
        {
            if(qi.parms.target is Map map && qi.def == IncidentDefOf.TraderCaravanArrival)
            {
                map.GetComponent<MapComponent_GoodWillTrader>().TimesTraded[qi.parms.faction] += 1;
            }
        }

        private static void PriceTypeSetter_PostFix(ref TraderKindDef __instance, ref PriceType __result, TradeAction action)
        {
            //PriceTypeSetter is more finicky than I'd like, part of the reason traders arrive without any sellable inventory.
            // had issues with pricetype undefined, pricetype normal and *all* traders having pricetype expensive for *all* goods. This works.
            PriceType priceType = __result;
            if (priceType == PriceType.Undefined)
            {
                return;
            }
            //if (__instance.stockGenerators[i] is StockGenerator_BuyCategory && action == TradeAction.PlayerSells)
            if (__instance.stockGenerators.Any(x => x is StockGenerator_BuyCategory) && action == TradeAction.PlayerSells)
            {
                __result = PriceType.Expensive;
            }
            else __result = priceType;
        }

    }
}
