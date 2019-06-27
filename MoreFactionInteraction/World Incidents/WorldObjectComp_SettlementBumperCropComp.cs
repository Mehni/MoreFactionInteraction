using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class WorldObjectComp_SettlementBumperCropComp : WorldObjectComp
    {
        public int expiration = -1;
        private const int basereward = 50;
        public int workLeft;
        private bool workStarted;
        private const int workAmount = GenDate.TicksPerDay;
        private const float expGain = 6000f;
        private static readonly IntRange FactionRelationOffset = new IntRange(min: 3, max: 8);

        private readonly Texture2D setPlantToGrowTex = HarmonyPatches.setPlantToGrowTex;

        public bool CaravanIsWorking => workStarted && Find.TickManager.TicksGame < workLeft;

        public bool ActiveRequest => expiration > Find.TickManager.TicksGame;

        public WorldObjectComp_SettlementBumperCropComp()
        {
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            if (ActiveRequest)
            {
                Command_Action commandAction = new Command_Action
                {
                    defaultLabel = "MFI_CommandHelpOutHarvesting".Translate(),
                    defaultDesc = "MFI_CommandHelpOutHarvesting".Translate(),
                    icon = setPlantToGrowTex,
                    action = () =>
                    {
                        {
                            if (!ActiveRequest)
                            {
                                Log.Error(text: "Attempted to fulfill an unavailable request");
                                return;
                            }
                            if (BestCaravanPawnUtility.FindPawnWithBestStat(caravan: caravan, stat: StatDefOf.PlantHarvestYield) == null)
                            {
                                Messages.Message(text: "MFI_MessageBumperCropNoGrower".Translate(), lookTargets: caravan, def: MessageTypeDefOf.NegativeEvent);
                                return;
                            }
                            Find.WindowStack.Add(window: Dialog_MessageBox.CreateConfirmation(text: "MFI_CommandFulfillBumperCropHarvestConfirm".Translate(caravan.LabelCap),

                            confirmedAct: () => NotifyCaravanArrived(caravan: caravan)));
                        }
                    }
                };


                if (BestCaravanPawnUtility.FindPawnWithBestStat(caravan: caravan, stat: StatDefOf.PlantHarvestYield) == null)
                {
                    commandAction.Disable(reason: "MFI_MessageBumperCropNoGrower".Translate());
                }
                yield return commandAction;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (ActiveRequest)
            {
                return "MFI_HarvestRequestInfo".Translate((expiration - Find.TickManager.TicksGame).ToStringTicksToDays());
            }
            return null;
        }

        public void Disable()
        {
            this.expiration = -1;
        }

        public void NotifyCaravanArrived(Caravan caravan)
        {
            workStarted = true;
            workLeft = Find.TickManager.TicksGame + workAmount;
            caravan.GetComponent<WorldObjectComp_CaravanComp>().workWillBeDoneAtTick = this.workLeft;
            caravan.GetComponent<WorldObjectComp_CaravanComp>().caravanIsWorking = true;
            Disable();
        }

        public void DoOutcome(Caravan caravan)
        {
            workStarted = false;
            caravan.GetComponent<WorldObjectComp_CaravanComp>().caravanIsWorking = false;
            Outcome_Triumph(caravan: caravan);
        }

        private void Outcome_Triumph(Caravan caravan)
        {
            int randomInRange = FactionRelationOffset.RandomInRange;
            parent.Faction?.TryAffectGoodwillWith(other: Faction.OfPlayer, goodwillChange: randomInRange);

            List<Pawn> allMembersCapableOfGrowing = AllCaravanMembersCapableOfGrowing(caravan: caravan);
            float totalYieldPowerForCaravan = CalculateYieldForCaravan(caravanMembersCapableOfGrowing: allMembersCapableOfGrowing);

            //TODO: Calculate a good amount
            //v1 (first vers): base of 20 * Sum of plant harvest yield * count * avg grow skill (20 * 2.96 * 3 * 14.5) = ~2579 or 20*5.96*6*20 = 14400
            //v2 (2018/08/03): base of 50 * avg of plant harvest yield * count * avg grow skill (50 * 0.99 * 3 * 14.5) = ~2153 or 40*0.99*6*20 = 4752 ((5940 for 50))
            //v3 (2018/12/18): base of 50 * avg of plant harvest yield * (count*0.75) * avg grow skill = (50 * 0.99 * (2.25) * 14.5 = ~1615 or (50 * 0.99 * (4.5) * 20 = 4455
            float totalreward = basereward * totalYieldPowerForCaravan * (allMembersCapableOfGrowing.Count * 0.75f)
                              * Mathf.Max(a: 1, b: (float)allMembersCapableOfGrowing.Average(selector: pawn => pawn.skills.GetSkill(skillDef: SkillDefOf.Plants).Level));

            Thing reward = ThingMaker.MakeThing(def: RandomRawFood());
            reward.stackCount = Mathf.RoundToInt(f: totalreward);
            CaravanInventoryUtility.GiveThing(caravan: caravan, thing: reward);

            Find.LetterStack.ReceiveLetter(label: "MFI_LetterLabelHarvest_Triumph".Translate(), text: GetLetterText(baseText: "MFI_Harvest_Triumph".Translate(
                parent.Faction?.def.pawnsPlural, parent.Faction?.Name,
                Mathf.RoundToInt(f: randomInRange),
                reward.LabelCap
            ), caravan: caravan), textLetterDef: LetterDefOf.PositiveEvent, lookTargets: caravan, relatedFaction: parent.Faction);

            allMembersCapableOfGrowing.ForEach(action: pawn => pawn.skills.Learn(sDef: SkillDefOf.Plants, xp: expGain, direct: true));
        }

        //a long list of things to excluse stuff like milk and kibble. In retrospect, it may have been easier to get all plants and get their harvestables.
        private static ThingDef RandomRawFood() => (from x in ThingSetMakerUtility.allGeneratableItems
                                                    where x.IsNutritionGivingIngestible && !x.IsCorpse && x.ingestible.HumanEdible && !x.IsMeat
                                                        && !x.IsDrug && !x.HasComp(compType: typeof(CompHatcher)) && !x.HasComp(compType: typeof(CompIngredients))
                                                        && x.BaseMarketValue < 3 && (x.ingestible.preferability == FoodPreferability.RawBad || x.ingestible.preferability == FoodPreferability.RawTasty)
                                                    select x).RandomElementWithFallback(ThingDefOf.RawPotatoes);

        private static float CalculateYieldForCaravan(IEnumerable<Pawn> caravanMembersCapableOfGrowing)
        {
            return caravanMembersCapableOfGrowing.Select(selector: x => x.GetStatValue(stat: StatDefOf.PlantHarvestYield)).Average();
        }

        private static string GetLetterText(string baseText, Caravan caravan)
        {
            StringBuilder text = new StringBuilder();
            text.Append(baseText).Append('\n');
            foreach (Pawn pawn in AllCaravanMembersCapableOfGrowing(caravan: caravan))
            {
                text.Append('\n').Append("MFI_BumperCropXPGain".Translate(pawn.LabelShort, expGain));
            }
            return text.ToString();
        }

        private static List<Pawn> AllCaravanMembersCapableOfGrowing(Caravan caravan)
        {
            return caravan.PawnsListForReading.Where(predicate: pawn => !pawn.Dead && !pawn.Downed && !pawn.InMentalState
                                                                     && caravan.IsOwner(p: pawn) && pawn.health.capacities.CanBeAwake
                                                                     && !StatDefOf.PlantHarvestYield.Worker.IsDisabledFor(thing: pawn)).ToList();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(value: ref expiration, label: "MFI_BumperCropExpiration");
            Scribe_Values.Look(value: ref workLeft, label: "MFI_BumperCropWorkLeft");
            Scribe_Values.Look(value: ref workStarted, label: "MFI_BumperCropWorkStarted");
        }
    }
}