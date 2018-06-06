using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace MoreFactionInteraction
{
    public class IncidentWorker_ReverseTradeRequest : IncidentWorker
    {
        private const int TimeoutTicks = GenDate.TicksPerDay;

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            return IncidentWorker_CaravanRequest.RandomNearbyTradeableSettlement(map.Tile) != null && base.CanFireNowSub(target);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Settlement settlement = IncidentWorker_CaravanRequest.RandomNearbyTradeableSettlement(parms.target.Tile);
            if (settlement != null)
            {
                //TODO: look into making the below dynamic based on requester's biome, faction, pirate outpost vicinity and other stuff.
                ThingCategoryDef thingCategoryDef;
                if (Rand.Range(0, 100) < 33) thingCategoryDef = ThingCategoryDefOf.Apparel;
                else if (Rand.Range(0, 100) > 33 && Rand.Range(0, 100) < 66) thingCategoryDef = ThingCategoryDefOf.PlantFoodRaw;
                else if (Rand.Range(0, 100) > 66 && Rand.Range(0, 100) < 90) thingCategoryDef = ThingCategoryDefOf.Weapons;
                else thingCategoryDef = ThingCategoryDefOf.Medicine;

                Map map = (Map)parms.target;
                int feeRequest = Math.Max(Rand.Range(150, 300), (int)parms.points);
                string categorylabel = (thingCategoryDef == ThingCategoryDefOf.PlantFoodRaw) ? thingCategoryDef.label + " items" : thingCategoryDef.label;
                ChoiceLetter_ReverseTradeRequest choiceLetter_ReverseTradingRequest = (ChoiceLetter_ReverseTradeRequest)LetterMaker.MakeLetter(this.def.letterLabel, "MFI_ReverseTradeRequest".Translate(new object[]
                {
                    settlement.Faction.leader.LabelShort,
                    settlement.Faction.def.leaderTitle,
                    settlement.Faction.Name,
                    settlement.Label,
                    categorylabel,
                    feeRequest,
                }).AdjustedFor(settlement.Faction.leader), this.def.letterDef);
                choiceLetter_ReverseTradingRequest.title = "MFI_ReverseTradeRequestTitle".Translate(new object[]
                {
                    map.info.parent.Label
                }).CapitalizeFirst();

                choiceLetter_ReverseTradingRequest.thingCategoryDef = thingCategoryDef;
                choiceLetter_ReverseTradingRequest.map = map;
                choiceLetter_ReverseTradingRequest.incidentParms = parms;
                choiceLetter_ReverseTradingRequest.faction = settlement.Faction;
                choiceLetter_ReverseTradingRequest.fee = feeRequest;
                choiceLetter_ReverseTradingRequest.StartTimeout(TimeoutTicks);
                Find.LetterStack.ReceiveLetter(choiceLetter_ReverseTradingRequest);
                return true;
            }
            return false;
        }
    }
}