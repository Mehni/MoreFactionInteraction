using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;

namespace MoreFactionInteraction
{
    public class MapComponent_GoodWillTrader : MapComponent
    {
        //empty constructor
        public MapComponent_GoodWillTrader(Map map) : base(map: map)
        {
        }

        //private IncidentQueue incidentQueue;
        private Dictionary<Faction, int> nextFactionInteraction = new Dictionary<Faction, int>();
        private Dictionary<Faction, int> timesTraded = new Dictionary<Faction, int>();

        /// <summary>
        /// Used to keep track of how many times the player traded with the faction and increase trader stock based on that.
        /// </summary>
        public Dictionary<Faction, int> TimesTraded
        {
            get
            {
                //intermingled :D
                foreach (Faction faction in this.NextFactionInteraction.Keys)
                {
                    if (!this.timesTraded.Keys.Contains(value: faction)) this.timesTraded.Add(key: faction, value: 0);
                }
                //trust betrayed, reset count.
                this.timesTraded.RemoveAll(predicate: x => x.Key.HostileTo(other: Faction.OfPlayerSilentFail));
                return this.timesTraded;
            }
        }

        public Dictionary<Faction, int> NextFactionInteraction
        {
            get
            {
                //initialise values
                if (this.nextFactionInteraction.Count == 0)
                {
                    IEnumerable<Faction> friendlyFactions = from faction in Find.FactionManager.AllFactionsVisible
                                                        where !faction.IsPlayer && faction != Faction.OfPlayerSilentFail && !Faction.OfPlayer.HostileTo(other: faction)
                                                        select faction;


                    foreach (Faction faction in friendlyFactions)
                    {
                        this.nextFactionInteraction.Add(key: faction, value: Find.TickManager.TicksGame + Rand.RangeInclusive(min: GenDate.TicksPerDay * 2, max: GenDate.TicksPerDay * 8));
                    }
                }
                //if a faction became hostile, remove
                //TODO: remove priorly scheduled incidents involving said faction
                //nextFactionInteraction.RemoveAll(x => x.Key.HostileTo(Faction.OfPlayerSilentFail));

                //and the opposite
                while ((from faction in Find.FactionManager.AllFactionsVisible
                        where !faction.IsPlayer && faction != Faction.OfPlayerSilentFail && !faction.HostileTo(other: Faction.OfPlayerSilentFail) && !this.nextFactionInteraction.ContainsKey(key: faction)
                        select faction).Any())
                {
                    this.nextFactionInteraction.Add(
                        key: (from faction in Find.FactionManager.AllFactionsVisible
                              where !faction.HostileTo(other: Faction.OfPlayerSilentFail) && !faction.IsPlayer && !this.nextFactionInteraction.ContainsKey(key: faction)
                              select faction).First(),
                        value: Find.TickManager.TicksGame + Rand.RangeInclusive(min: GenDate.TicksPerDay * 2, max: GenDate.TicksPerDay * 4));
                }
                return this.nextFactionInteraction;
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            //We don't need to run all that often
            if (Find.TickManager.TicksGame % 531 == 0 && GenDate.DaysPassed > 8)
            {
                foreach (KeyValuePair<Faction, int> kvp in this.NextFactionInteraction)
                {
                    if (Find.TickManager.TicksGame >= kvp.Value)
                    {
                        Faction faction = kvp.Key;
                        Log.Message("faction: "+ faction.Name + " value: " + kvp.Value);
                        IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.FactionArrival, target: this.map);
                        incidentParms.forced = true;
                        incidentParms.faction = faction;
                        //trigger incident somewhere between half a day and 3 days from now
                        Find.Storyteller.incidentQueue.Add(def: IncidentDef(), fireTick: Rand.Range(min: GenDate.TicksPerDay / 2, max: GenDate.TicksPerDay * 3), parms: incidentParms);

                        this.NextFactionInteraction[key: faction] =
                                    Find.TickManager.TicksGame + (int)FactionInteractionTimeSeperator.TimeBetweenInteraction.Evaluate(x: faction.GoodwillWith(other: Faction.OfPlayer));

                        //kids, you shouldn't change values you iterate over.
                        break;
                    }
                }
            }
        }

        private static IncidentDef IncidentDef()
        {
            switch (Rand.RangeInclusive(min: 0,max: 4))
            {
                case 0: 
                case 1: return MFI_DefOf.MFI_ReverseTradeRequest;
                case 2: return IncidentDefOf.Quest_TradeRequest;
                case 3:
                case 4: return IncidentDefOf.TraderCaravanArrival;

                default: return IncidentDefOf.TraderCaravanArrival;
            }
        }


        //working lists for ExposeData.
        List<Faction> factionsListforInteraction = new List<Faction>();
        List<Faction> factionsListforTimesTraded = new List<Faction>();
        List<int> intListForInteraction = new List<int>();
        List<int> intListforTimesTraded = new List<int>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Faction, int>(dict: ref this.nextFactionInteraction, label: "MFI_nextFactionInteraction", keyLookMode: LookMode.Reference, valueLookMode: LookMode.Value, keysWorkingList: ref this.factionsListforInteraction, valuesWorkingList: ref this.intListForInteraction);
            Scribe_Collections.Look<Faction, int>(dict: ref this.timesTraded, label: "MFI_timesTraded", keyLookMode: LookMode.Reference, valueLookMode: LookMode.Value, keysWorkingList: ref this.factionsListforTimesTraded, valuesWorkingList: ref this.intListforTimesTraded);
        }
    }
}
