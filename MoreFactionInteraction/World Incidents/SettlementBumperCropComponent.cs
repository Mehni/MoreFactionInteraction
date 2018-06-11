using System;
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
        public int requestCount;
        public int expiration = -1;
        private const int basereward = 20;
        private int workLeft = GenDate.TicksPerDay;
        private bool caravanIsWorking;

        private static readonly FloatRange FactionRelationOffset = new FloatRange(5f, 12f);

        private static readonly SimpleCurve BadOutcomeFactorAtHarvestingPower = new SimpleCurve
        {
            {
                new CurvePoint(0f, 4f),
                true
            },
            {
                new CurvePoint(1f, 1f),
                true
            },
            {
                new CurvePoint(1.5f, 0.4f),
                true
            }
        };

        public bool CaravanCanMove
        {
            get
            {
                return !caravanIsWorking;
            }
        }


        //I can and should move this out of a tick and into a getter.
        public override void CompTick()
        {
            if (caravanIsWorking)
            {
                workLeft--;
            }
            if (workLeft <= 0)
            {
                Reset();
                caravanIsWorking = false;
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
                return "CaravanRequestInfo".Translate(new object[]
                {
                    GenLabel.ThingLabel(this.bumperCrop, null, this.requestCount).CapitalizeFirst(),
                    this.bumperCrop,
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
            caravanIsWorking = true;
            List<Pawn> allMembersCapableOfGrowing = AllCaravanMembersCapableOfGrowing(caravan);
            float totalYieldPowerForCaravan = CalculateYieldForCaravan(allMembersCapableOfGrowing);
            float badOutcomeWeightFactor = BadOutcomeFactorAtHarvestingPower.Evaluate(totalYieldPowerForCaravan);
            float num = 1f / badOutcomeWeightFactor;

            //TODO: Calculate a good amount

            float totalreward = basereward * totalYieldPowerForCaravan * allMembersCapableOfGrowing.Count;

            Thing reward = ThingMaker.MakeThing(bumperCrop);
            reward.stackCount = Mathf.RoundToInt(totalreward);
            
            CaravanInventoryUtility.GiveThing(caravan, reward);

            //todo: postpone rewards until work is done
            Outcome_Triumph(caravan);

            allMembersCapableOfGrowing.ForEach(pawn=> pawn.skills.Learn(SkillDefOf.Growing, 6000f, true));
        }

        private void Outcome_Triumph(Caravan caravan)
        {
            float randomInRange = FactionRelationOffset.RandomInRange;
            parent.Faction.AffectGoodwillWith(Faction.OfPlayer, randomInRange);

            Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_Triumph".Translate(), this.GetLetterText("Harvest_Triumph".Translate(new object[]
            {
                parent.Faction.Name,
                Mathf.RoundToInt(randomInRange),
                bumperCrop.label
            }), caravan), LetterDefOf.PositiveEvent, caravan, null);
        }

        private float CalculateYieldForCaravan(List<Pawn> caravanMembersCapableOfGrowing)
        {           
            return caravanMembersCapableOfGrowing.Select(x => x.GetStatValue(StatDefOf.PlantHarvestYield, true)).Sum();
        }

        private string GetLetterText(string baseText, Caravan caravan)
        {
            string text = baseText;
            Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
            if (pawn != null)
            {
                text = text + "\n\n" + "PeaceTalksSocialXPGain".Translate(new object[]
                {
                    pawn.LabelShort,
                    6000f
                });
            }
            return text;
        }

        private static List<Pawn> AllCaravanMembersCapableOfGrowing(Caravan caravan)
        {
            return caravan.PawnsListForReading.Where(pawn => !pawn.Dead && !pawn.Downed && !pawn.InMentalState && caravan.IsOwner(pawn) && pawn.health.capacities.CanBeAwake
               && !StatDefOf.PlantHarvestYield.Worker.IsDisabledFor(pawn)).ToList();
        }

        private void Reset()
        {
            this.workLeft = GenDate.TicksPerDay;
        }
    }
}