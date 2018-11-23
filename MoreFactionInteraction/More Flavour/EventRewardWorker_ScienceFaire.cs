using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreFactionInteraction.More_Flavour;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_ScienceFaire : EventRewardWorker
    {
        public override Predicate<ThingDef> ValidatorFirstPlace => base.ValidatorFirstPlace;

        public override Predicate<ThingDef> ValidatorFirstLoser => base.ValidatorFirstLoser;

        public override Predicate<ThingDef> ValidatorFirstOther => (ThingDef x) => x == ThingDefOf.TechprofSubpersonaCore;

        public override string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator = null, ThingSetMakerDef thingSetMakerDef = null)
        {
            return GenerateBuff(thingSetMakerDef.root.fixedParams.techLevel.GetValueOrDefault(), pawn, caravan, globalValidator, thingSetMakerDef);
        }

        private string GenerateBuff(TechLevel desiredTechLevel, Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator, ThingSetMakerDef thingSetMakerDef)
        {
            string reward;

            Buff buff = Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().ApplyRandomBuff((Buff x) => x.MinTechLevel() >= desiredTechLevel && !x.Active);

            if (buff == null)
                buff = Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().ApplyRandomBuff((Buff x) => !x.Active);

            if (buff != null)
                reward = buff.Description();

            else
                reward = base.GenerateRewards(pawn, caravan, globalValidator, thingSetMakerDef);

            return reward;
        }
    }
}
