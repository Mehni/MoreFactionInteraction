using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreFactionInteraction.General;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_CulturalSwap : EventRewardWorker
    {
        private static readonly float OVERPAYINGBY = 3f;
        private readonly EventDef eventDef = MFI_DefOf.MFI_CulturalSwap;

        public override Predicate<ThingDef> ValidatorFirstPlace => (x) => /*x.stuffCategories.Contains(StuffCategoryDefOf.Metallic) &&*/ (x.BaseMarketValue > 6 || x.smallVolume) && base.ValidatorFirstPlace(x);

        public override Predicate<ThingDef> ValidatorFirstLoser => base.ValidatorFirstLoser;

        public override Predicate<ThingDef> ValidatorFirstOther => base.ValidatorFirstOther;

        public override string GenerateRewards(Pawn pawn, Caravan caravan, Predicate<ThingDef> globalValidator, ThingSetMakerDef thingSetMakerDef)
        {
            string rewards = string.Empty;

            if (thingSetMakerDef == eventDef.rewardFirstLoser)
            {
                foreach (Pawn performer in caravan.PlayerPawnsForStoryteller)
                {
                    if (!performer.story?.WorkTagIsDisabled(WorkTags.Artistic) ?? false)
                    {
                        pawn.skills.Learn(sDef: SkillDefOf.Artistic, eventDef.xPGainFirstLoser, direct: true);
                        TryAppendExpGainInfo(ref rewards, performer, SkillDefOf.Artistic, eventDef.xPGainFirstLoser);
                    }
                }
                return rewards + (Rand.Bool ? string.Empty : 
                                  Rand.Bool ? "\n\n---\n\n" + "MFI_AnnualExpoMedicalEmergency".Translate() : "\n\n---\n\n" + "MFI_AnnualExpoMedicalEmergencySerious".Translate());
            }
            return base.GenerateRewards(pawn, caravan, globalValidator, thingSetMakerDef);
        }

        public static DiaNode DialogueResolverArtOffer(string textResult, Thing broughtSculpture, Caravan caravan)
        {
            float marketValue = broughtSculpture.GetStatValue(StatDefOf.MarketValue);
            DiaNode resolver = new DiaNode(text: textResult.Translate(broughtSculpture, marketValue * OVERPAYINGBY, marketValue));
            DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
            {
                resolveTree = true,
                action = () =>
                {
                    broughtSculpture.Destroy();
                    Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                    silver.stackCount = (int)(marketValue * OVERPAYINGBY);
                    CaravanInventoryUtility.GiveThing(caravan, silver);
                }
            };
            DiaOption reject = new DiaOption("RansomDemand_Reject".Translate())
            {
                resolveTree = true,
            };
            resolver.options.Add(accept);
            resolver.options.Add(reject);
            return resolver;
        }
    }
}
