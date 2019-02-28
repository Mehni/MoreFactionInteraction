using Harmony;
using MoreFactionInteraction.More_Flavour;
using MoreFactionInteraction.MoreFactionWar;
using MoreFactionInteraction.World_Incidents;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        //resources must be loaded in cctor
        public static Texture2D setPlantToGrowTex = ContentFinder<Texture2D>.Get(itemPath: "UI/Commands/SetPlantToGrow");

        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create(id: "mehni.rimworld.MFI.main");
            //HarmonyInstance.DEBUG = true;

            #region MoreTraders
            harmony.Patch(original: AccessTools.Method(type: typeof(TraderKindDef), name: nameof(TraderKindDef.PriceTypeFor)),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(PriceTypeSetter_PostFix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(StoryState), name: nameof(StoryState.Notify_IncidentFired)),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(IncidentFired_TradeCounter_Postfix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(CompQuality), name: nameof(CompQuality.PostPostGeneratedForTrader)),
                 prefix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(CompQuality_TradeQualityIncreaseDestructivePreFix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(ThingSetMaker), name: nameof(ThingSetMaker.Generate), parameters: new Type[] { typeof(ThingSetMakerParams) }),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(TraderStocker_OverStockerPostFix)));

            harmony.Patch(original: AccessTools.Method(type: typeof(Tradeable), name: "InitPriceDataIfNeeded"),
                transpiler: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(ErrorSuppressionSssh)));
            #endregion

            #region WorldIncidents
            harmony.Patch(original: AccessTools.Method(type: typeof(WorldReachabilityUtility), name: nameof(WorldReachabilityUtility.CanReach)),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(WorldReachUtility_PostFix)));
            #endregion

#if DEBUG
            harmony.Patch(original: AccessTools.Method(type: typeof(DebugWindowsOpener), name: "ToggleDebugActionsMenu"),
                transpiler: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(DebugWindowsOpener_ToggleDebugActionsMenu_Patch)));
#endif

            harmony.Patch(original: AccessTools.Method(type: typeof(ThoughtWorker_PsychicEmanatorSoothe), name: "CurrentStateInternal"),
                transpiler: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(PsychicEmanatorSoothe_Transpiler)));

            harmony.Patch(original: AccessTools.Method(type: typeof(Faction), name: nameof(Faction.Notify_RelationKindChanged)),
                postfix: new HarmonyMethod(type: typeof(HarmonyPatches), name: nameof(Notify_RelationKindChanged)));

            if (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Relations Tab"))
            {
                try
                {
                    ((Action)(() =>
                    {
                        float func(Faction faction, Vector2 pos, float width)
                        {
                            if (Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StuffIsGoingDown)
                            {
                                Rect canvas = new Rect(pos.x, pos.y, width, 125f);
                                MainTabWindow_FactionWar.DrawFactionWarBar(canvas);
                                return 125f;
                            }
                            return 0;
                        }
                        Fluffy_Relations.MainTabWindow_Relations.ExtraFactionDetailDrawers.Add(func);
                    }))();
                }
                catch (TypeLoadException) { }
            }
        }

        private static void Notify_RelationKindChanged(Faction __instance, Faction other)
        {
            if (other == Faction.OfPlayer && __instance.HostileTo(other))
            {
                foreach (WorldObject item in Find.WorldObjects.AllWorldObjects.Where(x => x is WorldObject_RoadConstruction && x.Faction == __instance))
                {
                    Find.WorldObjects.Remove(item);
                }
                foreach (Settlement stlmnt in Find.WorldObjects.Settlements.Where(x => x.Faction == __instance))
                {
                    stlmnt.GetComponent<WorldObjectComp_SettlementBumperCropComp>()?.Disable();
                }
            }
        }

        private static IEnumerable<CodeInstruction> PsychicEmanatorSoothe_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo helperMethod = AccessTools.Method(type: typeof(HarmonyPatches), name: nameof(HarmonyPatches.SootheTranspilerHelperMethod));

            foreach (CodeInstruction codeInstruction in instructions)
            {
                if (codeInstruction.opcode == OpCodes.Ldc_R4)
                {
                    codeInstruction.opcode = OpCodes.Call;
                    codeInstruction.operand = helperMethod;
                }
                yield return codeInstruction;
            }
        }

        private static float SootheTranspilerHelperMethod() => Find.World?.GetComponent<WorldComponent_MFI_AnnualExpo>()?.BuffedEmanator ?? false ? 20f : 15f;

        //thx Brrainz
        private static IEnumerable<CodeInstruction> DebugWindowsOpener_ToggleDebugActionsMenu_Patch(IEnumerable<CodeInstruction> instructions)
        {
            ConstructorInfo from = AccessTools.Constructor(type: typeof(Dialog_DebugActionsMenu));
            ConstructorInfo to = AccessTools.Constructor(type: typeof(Dialog_MFIDebugActionMenu));
            return instructions.MethodReplacer(from: from, to: to);
        }

#region MoreTraders
        private static void TraderStocker_OverStockerPostFix(ref List<Thing> __result, ThingSetMakerParams parms)
        {
            if (parms.traderDef != null)
            {
                Map map = null;

                if (parms.tile != null && parms.tile != -1 && (Current.Game.FindMap(tile: parms.tile.Value)?.IsPlayerHome ?? false))
                    map = Current.Game.FindMap(tile: parms.tile.Value);

                else if (Find.AnyPlayerHomeMap != null)
                    map = Find.AnyPlayerHomeMap;

                else if (Find.CurrentMap != null)
                    map = Find.CurrentMap;

                if (map != null && (parms.traderDef.orbital || parms.traderDef.defName.Contains(value: "Base_")))
                {
                    //nullable float because not all traders bring silver
                    float? silverCount = __result.FirstOrDefault(predicate: x => x.def == ThingDefOf.Silver)?.stackCount;
                    if (!silverCount.HasValue)
                        return;
                    silverCount *= WealthSilverIncreaseDeterminationCurve.Evaluate(x: map.PlayerWealthForStoryteller);
                    __result.First(predicate: x => x.def == ThingDefOf.Silver).stackCount = (int)silverCount;
                    return;
                }
                if (map != null && parms.traderFaction != null)
                {
                    //nullable float because not all traders bring silver
                    float? silverCount = __result.FirstOrDefault(predicate: x => x.def == ThingDefOf.Silver)?.stackCount;
                    if (!silverCount.HasValue)
                        return;
                    __result.First(predicate: x => x.def == ThingDefOf.Silver).stackCount += (int)(parms.traderFaction.GoodwillWith(other: Faction.OfPlayer) * (map.GetComponent<MapComponent_GoodWillTrader>().TimesTraded[key: parms.traderFaction] * MoreFactionInteraction_Settings.traderWealthOffsetFromTimesTraded));
                }
            }
        }

        private static readonly SimpleCurve WealthSilverIncreaseDeterminationCurve = new SimpleCurve
        {
            new CurvePoint(x: 0, y: 0.8f),
            new CurvePoint(x: 10000, y: 1),
            new CurvePoint(x: 75000, y: 2),
            new CurvePoint(x: 300000, y: 4),
            new CurvePoint(x: 1000000, y: 6f),
            new CurvePoint(x: 2000000, y: 7f)
        };

#region TradeQualityImprovements
        private static bool CompQuality_TradeQualityIncreaseDestructivePreFix(CompQuality __instance, TraderKindDef trader, int forTile, Faction forFaction)
        {
            //forTile is assigned in RimWorld.ThingSetMaker_TraderStock.Generate. It's either a best-effort map, or -1.
            Map map = null;
            if (forTile != -1)
                map = Current.Game.FindMap(tile: forTile);
            __instance.SetQuality(q: FactionAndGoodWillDependantQuality(faction: forFaction, map: map, trader: trader), source: ArtGenerationContext.Outsider);
            return false;
        }

        /// <summary>
        /// Change quality carried by traders depending on Faction/Goodwill/Wealth.
        /// </summary>
        /// <returns>QualityCategory depending on wealth or goodwill. Fallsback to vanilla when fails.</returns>
        private static QualityCategory FactionAndGoodWillDependantQuality(Faction faction, Map map, TraderKindDef trader)
        {
            if ((trader.orbital || trader.defName.Contains(value: "_Base")) && map != null)
            {
                if (Rand.Value < 0.25f)
                {
                    return QualityCategory.Normal;
                }
                float num = Rand.Gaussian(centerX: WealthQualityDeterminationCurve.Evaluate(x: map.wealthWatcher.WealthTotal), widthFactor: WealthQualitySpreadDeterminationCurve.Evaluate(x: map.wealthWatcher.WealthTotal));
                num = Mathf.Clamp(value: num, min: 0f, max: QualityUtility.AllQualityCategories.Count - 0.5f);
                return (QualityCategory)num;
            }
            if (map != null && faction != null)
            {
                float qualityIncreaseFromTimesTradedWithFaction = Mathf.Clamp01(value: (float)map.GetComponent<MapComponent_GoodWillTrader>().TimesTraded[key: faction] / 100);
                float qualityIncreaseFactorFromPlayerGoodWill = Mathf.Clamp01(value: (float)faction.GoodwillWith(other: Faction.OfPlayer) / 100);

                if (Rand.Value < 0.25f)
                {
                    return QualityCategory.Normal;
                }
                float num = Rand.Gaussian(centerX: 2.5f + qualityIncreaseFactorFromPlayerGoodWill, widthFactor: 0.84f + qualityIncreaseFromTimesTradedWithFaction);
                num = Mathf.Clamp(value: num, min: 0f, max: QualityUtility.AllQualityCategories.Count - 0.5f);
                return (QualityCategory)num;
            }
            return QualityUtility.GenerateQualityTraderItem();
        }

        private static IEnumerable<CodeInstruction> ErrorSuppressionSssh(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[index: i].opcode == OpCodes.Ldstr)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        instructionList[index: i + j].opcode = OpCodes.Nop;
                    }
                }
                yield return instructionList[index: i];
            }
        }

#region SimpleCurves
        private static readonly SimpleCurve WealthQualityDeterminationCurve = new SimpleCurve
        {
            new CurvePoint(x: 0, y: 1),
            new CurvePoint(x: 10000, y: 1.5f),
            new CurvePoint(x: 75000, y: 2.5f),
            new CurvePoint(x: 300000, y: 3),
            new CurvePoint(x: 1000000, y: 3.8f),
            new CurvePoint(x: 2000000, y: 4.3f)
        };

        private static readonly SimpleCurve WealthQualitySpreadDeterminationCurve = new SimpleCurve
        {
            new CurvePoint(x: 0, y: 4.2f),
            new CurvePoint(x: 10000, y: 4),
            new CurvePoint(x: 75000, y: 2.5f),
            new CurvePoint(x: 300000, y: 2.1f),
            new CurvePoint(x: 1000000, y: 1.5f),
            new CurvePoint(x: 2000000, y: 1.2f)
        };
#endregion SimpleCurves
#endregion TradeQualityImprovements

        /// <summary>
        /// Increment TimesTraded count of dictionary by one for this faction.
        /// </summary>
        private static void IncidentFired_TradeCounter_Postfix(ref FiringIncident fi)
        {
            if (fi.parms.target is Map map && fi.def == IncidentDefOf.TraderCaravanArrival && fi.parms.faction != null)
            {
                map.GetComponent<MapComponent_GoodWillTrader>().TimesTraded[key: fi.parms.faction] += 1;
            }
        }

        private static void PriceTypeSetter_PostFix(TraderKindDef __instance, ref PriceType __result, TradeAction action)
        {
            //PriceTypeSetter is more finicky than I'd like, part of the reason traders arrive without any sellable inventory.
            // had issues with pricetype undefined, pricetype normal and *all* traders having pricetype expensive for *all* goods. This works.
            PriceType priceType = __result;
            if (priceType == PriceType.Undefined)
            {
                return;
            }
            //if (__instance.stockGenerators[i] is StockGenerator_BuyCategory && action == TradeAction.PlayerSells)
            if (__instance.stockGenerators.Any(predicate: x => x is StockGenerator_BuyCategory) && action == TradeAction.PlayerSells)
            {
                __result = PriceType.Expensive;
            }
            else __result = priceType;
        }
#endregion

#region WorldIncidents
        private static void WorldReachUtility_PostFix(ref bool __result, Caravan c)
        {
            SettlementBase settlement = CaravanVisitUtility.SettlementVisitedNow(caravan: c);
            WorldObjectComp_SettlementBumperCropComp bumperCropComponent = settlement?.GetComponent<WorldObjectComp_SettlementBumperCropComp>();

            if (bumperCropComponent != null)
            {
                __result = !bumperCropComponent.CaravanIsWorking;
            }
        }
#endregion
    }
}
