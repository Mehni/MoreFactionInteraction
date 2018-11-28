using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_AcousticShow : EventRewardWorker
    {
        private readonly EventDef eventDef = MFI_DefOf.MFI_AcousticShow;

        public override Predicate<ThingDef> ValidatorFirstPlace => base.ValidatorFirstPlace;

        public override Predicate<ThingDef> ValidatorFirstLoser => base.ValidatorFirstLoser;

        public override Predicate<ThingDef> ValidatorFirstOther => base.ValidatorFirstOther;

        public override string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator, ThingSetMakerDef thingSetMakerDef)
        {
            if (thingSetMakerDef == eventDef.rewardFirstPlace)
                GiveHappyThoughtsToCaravan(caravan, 20);

            if (thingSetMakerDef == eventDef.rewardFirstLoser)
                GiveHappyThoughtsToCaravan(caravan, 15);

            if (thingSetMakerDef == eventDef.rewardFirstOther)
                GiveHappyThoughtsToCaravan(caravan, 10);

            return base.GenerateRewards(pawn, caravan, globalValidator, thingSetMakerDef);
        }

        private static void GiveHappyThoughtsToCaravan(Caravan caravan, int amount)
        {
            foreach (Pawn pawn in caravan.PlayerPawnsForStoryteller)
            {
                for (int i = 0; i < amount; i++)
                {
                    if (pawn.needs?.mood?.thoughts?.memories != null)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.AttendedParty, null);
                }
            }
        }
    }
}
