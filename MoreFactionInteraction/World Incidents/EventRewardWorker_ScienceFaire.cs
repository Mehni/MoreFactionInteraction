using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreFactionInteraction.More_Flavour;
using RimWorld;
using RimWorld.Planet;
using Verse;
using MoreFactionInteraction.General;
using UnityEngine;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_ScienceFaire : EventRewardWorker
    {
        private readonly EventDef eventDef = MFI_DefOf.MFI_ScienceFaire;

        public override Predicate<ThingDef> ValidatorFirstPlace => base.ValidatorFirstPlace;

        public override Predicate<ThingDef> ValidatorFirstLoser => base.ValidatorFirstLoser;

        public override Predicate<ThingDef> ValidatorFirstOther => (ThingDef x) => x == ThingDefOf.TechprofSubpersonaCore;

        public override string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator, ThingSetMakerDef thingSetMakerDef)
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
                reward = buff.Description() + MaybeTheySuckAndDontHaveItYet(buff, pawn, caravan, thingSetMakerDef);

            else
                reward = base.GenerateRewards(pawn, caravan, globalValidator, thingSetMakerDef);

            return reward;
        }

        private string MaybeTheySuckAndDontHaveItYet(Buff buff, Pawn pawn, Caravan caravan, ThingSetMakerDef thingSetMakerDef)
        {
            if (thingSetMakerDef == eventDef.rewardFirstPlace && !MFI_Utilities.CaravanOrRichestColonyHasAnyOf(buff.RelevantThingDef(), caravan, out Thing thing))
            {
                thing = ThingMaker.MakeThing(buff.RelevantThingDef());
                thing.stackCount = Mathf.Min(thing.def.stackLimit, 75); //suck it, stackXXL users.
                CaravanInventoryUtility.GiveThing(caravan, thing);
                return "\n\n" + "MFI_SinceYouSuckAndDidntHaveIt".Translate(thing.Label);
            }
            return string.Empty;
        }
    }
}
