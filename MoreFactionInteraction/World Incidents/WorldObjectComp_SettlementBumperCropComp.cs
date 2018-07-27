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
        //attentive readers may find similarities between this and the Peacetalks quest. 
        public ThingDef bumperCrop;

        public int expiration = -1;
        private const int basereward = 20;
        public int workLeft;
        private bool workStarted;
        private const int workAmount = GenDate.TicksPerDay;
        private const float expGain = 6000f;
        private static readonly IntRange FactionRelationOffset = new IntRange(min: 3, max: 8);

        public bool CaravanIsWorking => this.workStarted && Find.TickManager.TicksGame < this.workLeft;

        public bool ActiveRequest => this.expiration > Find.TickManager.TicksGame;

        public WorldObjectComp_SettlementBumperCropComp()
        {
        }

        public override string CompInspectStringExtra()
        {
            if (this.ActiveRequest)
            {
                return "MFI_HarvestRequestInfo".Translate(args: new object[]
                {
                    (this.expiration - Find.TickManager.TicksGame).ToStringTicksToDays()
                });
            }
            return null;
        }

        public void Disable()
        {
            this.expiration = -1;
        }

        public void NotifyCaravanArrived(Caravan caravan)
        {
            this.workStarted = true;
            this.workLeft = Find.TickManager.TicksGame + workAmount;
            caravan.GetComponent<WorldObjectComp_CaravanComp>().workWillBeDoneAtTick = this.workLeft;
            caravan.GetComponent<WorldObjectComp_CaravanComp>().caravanIsWorking = true;
            this.Disable();
        }

        public void DoOutcome(Caravan caravan)
        {
            this.workStarted = false;
            caravan.GetComponent<WorldObjectComp_CaravanComp>().caravanIsWorking = false;
            this.Outcome_Triumph(caravan: caravan);
        }

        private void Outcome_Triumph(Caravan caravan)
        {
            int randomInRange = FactionRelationOffset.RandomInRange;
            this.parent.Faction.TryAffectGoodwillWith(other: Faction.OfPlayer, goodwillChange: randomInRange);

            List<Pawn> allMembersCapableOfGrowing = AllCaravanMembersCapableOfGrowing(caravan: caravan);
            float totalYieldPowerForCaravan = CalculateYieldForCaravan(caravanMembersCapableOfGrowing: allMembersCapableOfGrowing);

            //TODO: Calculate a good amount
            float totalreward = basereward * totalYieldPowerForCaravan * allMembersCapableOfGrowing.Count 
                              * Mathf.Max(a: 1, b: (float)allMembersCapableOfGrowing.Average(selector: pawn => pawn.skills.GetSkill(skillDef: SkillDefOf.Plants).Level));

            Thing reward = ThingMaker.MakeThing(def: this.bumperCrop);
            reward.stackCount = Mathf.RoundToInt(f: totalreward);
            CaravanInventoryUtility.GiveThing(caravan: caravan, thing: reward);

            Find.LetterStack.ReceiveLetter(label: "MFI_LetterLabelHarvest_Triumph".Translate(), text: GetLetterText(baseText: "MFI_Harvest_Triumph".Translate(
                this.parent.Faction.def.pawnsPlural, this.parent.Faction.Name,
                Mathf.RoundToInt(f: randomInRange),
                reward.LabelCap
            ), caravan: caravan), textLetterDef: LetterDefOf.PositiveEvent, lookTargets: caravan, relatedFaction: this.parent.Faction);

            allMembersCapableOfGrowing.ForEach(action: pawn => pawn.skills.Learn(sDef: SkillDefOf.Plants, xp: expGain, direct: true));
        }

        private static float CalculateYieldForCaravan(IEnumerable<Pawn> caravanMembersCapableOfGrowing)
        {           
            return caravanMembersCapableOfGrowing.Select(selector: x => x.GetStatValue(stat: StatDefOf.PlantHarvestYield)).Sum();
        }

        private static string GetLetterText(string baseText, Caravan caravan)
        {
            StringBuilder text = new StringBuilder();
            text.Append(value: baseText + "\n");
            foreach (Pawn pawn in AllCaravanMembersCapableOfGrowing(caravan: caravan))
            {
                text.Append(value: "\n" + "MFI_BumperCropXPGain".Translate(args: new object[]
                {
                    pawn.LabelShort,
                    expGain
                }));
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
            Scribe_Values.Look<int>(value: ref this.expiration, label: "MFI_BumperCropExpiration");
            Scribe_Values.Look<int>(value: ref this.workLeft, label: "MFI_BumperCropWorkLeft");
            Scribe_Values.Look<bool>(value: ref this.workStarted, label: "MFI_BumperCropWorkStarted");
        }
    }
}