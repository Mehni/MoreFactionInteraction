using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MoreFactionInteraction
{
    public class IncidentWorker_WeddingGuestsArrival : IncidentWorker_NeutralGroup
    {
        public override float AdjustedChance
        {
            get
            {
                return def.baseChance * 0;
            }
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Log.Warning(MarriageCeremonyUtility.AcceptableGameConditionsToStartCeremony(map).ToString());
            return base.CanFireNowSub(parms) && MarriageCeremonyUtility.AcceptableGameConditionsToStartCeremony(map);
        }

        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            return base.FactionCanBeGroupSource(f, map, desperate);
        }

        protected override void ResolveParmsPoints(IncidentParms parms)
        {
            parms.points = TraderCaravanUtility.GenerateGuardPoints();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Log.Message(parms.ToString());


            if (!base.TryResolveParms(parms) || !CanFireNowSub(parms) || parms.faction.HostileTo(Faction.OfPlayer)) { Log.Warning("Can't fire: CanFireNow "+ CanFireNowSub(parms).ToString() +" Try Resolve: "+ TryResolveParms(parms).ToString() + " "+ parms.faction.HostileTo(Faction.OfPlayer).ToString()); return false; }

            Map map = (Map)parms.target;
            List<Pawn> guests = base.SpawnPawns(parms);

            if (guests.Count == 0) return false;

            Log.Message(parms.spawnCenter.ToString());
            if (parms.spawnCenter == IntVec3.Invalid)
            {
                parms.spawnCenter = DropCellFinder.TradeDropSpot(map);
            }

            for (int i = 0; i < guests.Count; i++)
            {
                if (guests[i].needs?.food != null)
                {
                    guests[i].needs.food.CurLevel = guests[i].needs.food.MaxLevel;
                }
            }
            //Find.LetterStack.ReceiveLetter(def.letterLabel, "Wedding guests arrived", LetterDefOf.PositiveEvent, guests[0], parms.faction, null);
            RCellFinder.TryFindRandomSpotJustOutsideColony(guests[0], out IntVec3 chillSpot);

            Lord lord = map.lordManager.lords.Find(x => x.LordJob is LordJob_NonVoluntaryJoinable_MarriageCeremony);

            foreach (Pawn guest in guests)
            {
                if (lord !=null)
                {
                    lord.AddPawn(guest);
                }
                else
                {
                    Log.Warning("no lord");
                    break;
                }
            }

            //LordJob_TradeWithColony lordJob = new LordJob_TradeWithColony(parms.faction, chillSpot);
            //LordMaker.MakeNewLord(parms.faction, lordJob, map, guests);
            return true;
        }
    }
}
