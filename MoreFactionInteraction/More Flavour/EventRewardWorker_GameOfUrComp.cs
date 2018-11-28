using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_GameOfUrComp : EventRewardWorker
    {
        private readonly EventDef eventDef = MFI_DefOf.MFI_GameOfUrComp;

        public override Predicate<ThingDef> ValidatorFirstPlace => base.ValidatorFirstPlace;

        public override Predicate<ThingDef> ValidatorFirstLoser => base.ValidatorFirstLoser;

        public override Predicate<ThingDef> ValidatorFirstOther => base.ValidatorFirstOther;

        public override string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator, ThingSetMakerDef thingSetMakerDef)
        {
            if (thingSetMakerDef == eventDef.rewardFirstOther)
            {
                string rewards = "MFI_AnnualExpoMedicalEmergency".Translate();
                foreach (Pawn brawler in caravan.PlayerPawnsForStoryteller)
                {
                    if (!brawler.story?.WorkTagIsDisabled(WorkTags.Violent) ?? false)
                    {
                        brawler.skills.Learn(SkillDefOf.Melee, eventDef.xPGainFirstPlace, true);
                        TryAppendExpGainInfo(ref rewards, brawler, SkillDefOf.Melee, eventDef.xPGainFirstPlace);
                    }
                }
                return rewards;
            }
            return base.GenerateRewards(pawn, caravan, globalValidator, thingSetMakerDef);
        }
    }
}
