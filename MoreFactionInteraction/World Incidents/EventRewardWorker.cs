using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public abstract class EventRewardWorker
    {
        public virtual Predicate<ThingDef> ValidatorFirstPlace => (ThingDef _) => true;

        public virtual Predicate<ThingDef> ValidatorFirstLoser => (ThingDef _) => true;

        public virtual Predicate<ThingDef> ValidatorFirstOther => (ThingDef _) => true;

        public virtual string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator, ThingSetMakerDef thingSetMakerDef)
        {
            List<Thing> rewards = new List<Thing>();
            if (thingSetMakerDef != null)
            {
                ThingSetMakerParams parms = default;
                parms.validator = globalValidator;
                parms.qualityGenerator = QualityGenerator.Reward;
                rewards = thingSetMakerDef.root.Generate(parms);
            }

            string rewardsToCommaList = GenThing.ThingsToCommaList(rewards);
            GenThing.TryAppendSingleRewardInfo(ref rewardsToCommaList, rewards);

            foreach (Thing itemReward in rewards)
            {
                caravan.AddPawnOrItem(itemReward, true);
            }

            return rewardsToCommaList;
        }

        public virtual void TryAppendExpGainInfo(ref string rewardsOutcome, Pawn pawn, SkillDef skill, float amount)
        {
            if (amount > 0)
            {
                rewardsOutcome = rewardsOutcome + "\n\n" + "MFI_AnnualExpoXPGain"
                    .Translate(pawn.LabelShort, amount.ToString("F0"), skill.label);
            }
        }
    }
}
