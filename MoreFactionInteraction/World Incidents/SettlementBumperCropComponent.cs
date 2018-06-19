using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class SettlementBumperCropComponent : WorldObjectComp
    {
        //attentive readers may find similarities between this and the Peacetalks quest. 
        public ThingDef bumperCrop;

        public int expiration = -1;
        private const int basereward = 20;
        public int workLeft;
        private bool workStarted;
        private const int workAmount = GenDate.TicksPerDay;
        private const float expGain = 6000f;
        private static readonly IntRange FactionRelationOffset = new IntRange(3, 8);

        //private static readonly SimpleCurve BadOutcomeFactorAtHarvestingPower = new SimpleCurve
        //{
        //    {
        //        new CurvePoint(0f, 4f),
        //        true
        //    },
        //    {
        //        new CurvePoint(1f, 1f),
        //        true
        //    },
        //    {
        //        new CurvePoint(1.5f, 0.4f),
        //        true
        //    }
        //};

        public bool CaravanIsWorking
        {
            get
            {
                return workStarted && Find.TickManager.TicksGame < workLeft;
            }
        }

        public bool ActiveRequest
        {
            get
            {
                return this.expiration > Find.TickManager.TicksGame;
            }
        }

        public SettlementBumperCropComponent()
        {
        }

        public override string CompInspectStringExtra()
        {
            if (this.ActiveRequest)
            {
                return "MFI_HarvestRequestInfo".Translate(new object[]
                {
                    (this.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F1")
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
            workStarted = true;
            workLeft = Find.TickManager.TicksGame + workAmount;
            caravan.GetComponent<CaravanComp>().workWillBeDoneAtTick = workLeft;
            caravan.GetComponent<CaravanComp>().caravanIsWorking = true;
            Disable();
        }

        public void DoOutcome(Caravan caravan)
        {
            workStarted = false;
            caravan.GetComponent<CaravanComp>().caravanIsWorking = false;
            Outcome_Triumph(caravan);
        }

        private void Outcome_Triumph(Caravan caravan)
        {
            int randomInRange = FactionRelationOffset.RandomInRange;
            parent.Faction.TryAffectGoodwillWith(Faction.OfPlayer, randomInRange);

            List<Pawn> allMembersCapableOfGrowing = AllCaravanMembersCapableOfGrowing(caravan);
            float totalYieldPowerForCaravan = CalculateYieldForCaravan(allMembersCapableOfGrowing);

            //TODO: Calculate a good amount
            float totalreward = basereward * totalYieldPowerForCaravan * allMembersCapableOfGrowing.Count * Mathf.Max(1, (float)allMembersCapableOfGrowing.Average(pawn => pawn.skills.GetSkill(SkillDefOf.Growing).Level));

            Thing reward = ThingMaker.MakeThing(bumperCrop);
            reward.stackCount = Mathf.RoundToInt(totalreward);
            CaravanInventoryUtility.GiveThing(caravan, reward);

            Find.LetterStack.ReceiveLetter("MFI_LetterLabelHarvest_Triumph".Translate(), this.GetLetterText("MFI_Harvest_Triumph".Translate(new object[]
            {
                parent.Faction.def.pawnsPlural,
                parent.Faction.Name,
                Mathf.RoundToInt(randomInRange),
                reward.LabelCap
            }), caravan), LetterDefOf.PositiveEvent, caravan, null);

            allMembersCapableOfGrowing.ForEach(pawn => pawn.skills.Learn(SkillDefOf.Growing, expGain, true));
        }

        private float CalculateYieldForCaravan(List<Pawn> caravanMembersCapableOfGrowing)
        {           
            return caravanMembersCapableOfGrowing.Select(x => x.GetStatValue(StatDefOf.PlantHarvestYield, true)).Sum();
        }

        private string GetLetterText(string baseText, Caravan caravan)
        {
            StringBuilder text = new StringBuilder();
            text.Append(baseText + "\n");
            foreach (Pawn pawn in AllCaravanMembersCapableOfGrowing(caravan))
            {
                text.Append("\n" + "MFI_BumperCropXPGain".Translate(new object[]
                {
                    pawn.LabelShort,
                    expGain
                }));
            }
            return text.ToString();
        }

        private static List<Pawn> AllCaravanMembersCapableOfGrowing(Caravan caravan)
        {
            return caravan.PawnsListForReading.Where(pawn => !pawn.Dead && !pawn.Downed && !pawn.InMentalState && caravan.IsOwner(pawn) && pawn.health.capacities.CanBeAwake
               && !StatDefOf.PlantHarvestYield.Worker.IsDisabledFor(pawn)).ToList();
        }
    }
}