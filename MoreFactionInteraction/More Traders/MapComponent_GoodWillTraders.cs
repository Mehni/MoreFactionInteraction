using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Harmony;
using System.Reflection;
using System;

namespace MoreFactionInteraction
{

    public class MapComponent_GoodWillTrader : MapComponent
    {
        private List<QueuedIncident> queued;
        private Dictionary<Faction, int> nextFactionInteraction = new Dictionary<Faction, int>();
        private Dictionary<Faction, int> timesTraded = new Dictionary<Faction, int>();

        //empty constructor
        public MapComponent_GoodWillTrader(Map map) : base(map: map)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            queued = Traverse.Create(Find.Storyteller.incidentQueue).Field("queuedIncidents").GetValue<List<QueuedIncident>>();

            if (queued == null)
                throw new NullReferenceException("MFI failed to initialise IncidentQueue in MapComponent.");
        }

        /// <summary>
        /// Used to keep track of how many times the player traded with the faction and increase trader stock based on that.
        /// </summary>
        public Dictionary<Faction, int> TimesTraded
        {
            get
            {
                //intermingled :D
                foreach (Faction faction in NextFactionInteraction.Keys)
                {
                    if (!timesTraded.Keys.Contains(value: faction)) timesTraded.Add(key: faction, value: 0);
                }
                //trust betrayed, reset count.
                timesTraded.RemoveAll(predicate: x => x.Key.HostileTo(other: Faction.OfPlayerSilentFail));
                return timesTraded;
            }
        }

        public Dictionary<Faction, int> NextFactionInteraction
        {
            get
            {
                //initialise values
                if (nextFactionInteraction.Count == 0)
                {
                    IEnumerable<Faction> friendlyFactions = from faction in Find.FactionManager.AllFactionsVisible
                                                            where !faction.IsPlayer && faction != Faction.OfPlayerSilentFail && !Faction.OfPlayer.HostileTo(other: faction)
                                                            select faction;

                    foreach (Faction faction in friendlyFactions)
                    {
                        Rand.PushState(replacementSeed: faction.loadID ^ faction.GetHashCode());
                        nextFactionInteraction.Add(key: faction, value: Find.TickManager.TicksGame + Rand.RangeInclusive(min: GenDate.TicksPerDay * 2, max: GenDate.TicksPerDay * 8));
                        Rand.PopState();
                    }
                }

                if (nextFactionInteraction.RemoveAll(x => x.Key.HostileTo(Faction.OfPlayerSilentFail)) > 0)
                {
                    CleanIncidentQueue(null, true);
                }

                //and the opposite
                while ((from faction in Find.FactionManager.AllFactionsVisible
                        where !faction.IsPlayer && faction != Faction.OfPlayerSilentFail && !faction.HostileTo(other: Faction.OfPlayerSilentFail) && !nextFactionInteraction.ContainsKey(key: faction)
                        select faction).Any())
                {
                    nextFactionInteraction.Add(
                        key: (from faction in Find.FactionManager.AllFactionsVisible
                              where !faction.HostileTo(other: Faction.OfPlayerSilentFail) && !faction.IsPlayer && !nextFactionInteraction.ContainsKey(key: faction)
                              select faction).First(),
                        value: Find.TickManager.TicksGame + Rand.RangeInclusive(min: GenDate.TicksPerDay * 2, max: GenDate.TicksPerDay * 8));
                }
                return nextFactionInteraction;
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            //We don't need to run all that often
            if (Find.TickManager.TicksGame % 531 == 0 && GenDate.DaysPassed > 8)
            {
                CleanIncidentQueue(null);

                foreach (KeyValuePair<Faction, int> kvp in NextFactionInteraction)
                {
                    if (Find.TickManager.TicksGame >= kvp.Value)
                    {
                        Faction faction = kvp.Key;
                        IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.FactionArrival, target: map);
                        incidentParms.faction = faction;
                        //forced, because half the time game doesn't feel like firing events.
                        incidentParms.forced = true;

                        //trigger incident somewhere between half a day and 3 days from now
                        Find.Storyteller.incidentQueue.Add(def: IncomingIncidentDef(faction) ?? IncomingIncidentDef(faction), // overly-cautious "try again" null-check after strange bugreport.
                                                           fireTick: Find.TickManager.TicksGame + Rand.Range(min: GenDate.TicksPerDay / 2, max: GenDate.TicksPerDay * 3),
                                                           parms: incidentParms,
                                                           retryDurationTicks: 2500);

                        NextFactionInteraction[key: faction] =
                            Find.TickManager.TicksGame
                              + (int)(FactionInteractionTimeSeperator.TimeBetweenInteraction.Evaluate(faction.PlayerGoodwill)
                                    * MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction);

                        //kids, you shouldn't change values you iterate over.
                        break;
                    }
                }
            }
        }

        public override void MapRemoved()
        {
            base.MapRemoved();
            CleanIncidentQueue(map);
        }

        private static IncidentDef IncomingIncidentDef(Faction faction)
        {
            switch (Rand.RangeInclusive(min: 0, max: 50))
            {
                //kinda dirty hack that also means if you set the modifier to 2 you get fewer of the other quests. Oh well.
                case int n when n <= 6 * MoreFactionInteraction_Settings.pirateBaseUpgraderModifier:
                    return MFI_DefOf.MFI_QuestSpreadingPirateCamp;

                case int n when n <= 8:
                    return IncidentDef.Named("Quest_BanditCamp");

                case int n when n <= 9:
                    return MFI_DefOf.MFI_DiplomaticMarriage;

                case int n when n <= 19:
                    return faction.leader != null ? MFI_DefOf.MFI_ReverseTradeRequest : IncomingIncidentDef(faction);

                case int n when n <= 25:
                    return MFI_DefOf.MFI_BumperCropRequest;

                case int n when n <= 29:
                    return faction.leader != null ? MFI_DefOf.MFI_HuntersLodge : IncomingIncidentDef(faction);

                case int n when n <= 31:
                    return IncidentDef.Named("MFI_MysticalShaman");

                case int n when n <= 35:
                    return faction.leader != null ? IncidentDef.Named("Quest_ItemStash") : IncomingIncidentDef(faction);

                case int n when n <= 40:
                    return IncidentDefOf.Quest_TradeRequest;

                case int n when n <= 50:
                    return IncidentDefOf.TraderCaravanArrival;

                default: return IncidentDefOf.TraderCaravanArrival;
            }
        }

        private readonly List<IncidentDef> allowedIncidentDefs =
            new List<IncidentDef>()
            {
                MFI_DefOf.MFI_QuestSpreadingPirateCamp,
                IncidentDef.Named("Quest_BanditCamp"),
                MFI_DefOf.MFI_DiplomaticMarriage,
                MFI_DefOf.MFI_ReverseTradeRequest,
                MFI_DefOf.MFI_BumperCropRequest,
                MFI_DefOf.MFI_HuntersLodge,
                IncidentDef.Named("MFI_MysticalShaman"),
                IncidentDef.Named("Quest_ItemStash"),
                IncidentDefOf.Quest_TradeRequest,
                IncidentDefOf.TraderCaravanArrival
            };

        private void CleanIncidentQueue(Map map, bool removeHostile = false)
        {
            if (removeHostile)
                queued.RemoveAll(qi => qi.FiringIncident.parms.faction.HostileTo(Faction.OfPlayer) && allowedIncidentDefs.Contains(qi.FiringIncident.def));

            queued.RemoveAll(qi => qi.FiringIncident.parms.target == map);

            if (queued.RemoveAll(qi => (qi.FireTick + GenDate.TicksPerTwelfth) < Find.TickManager.TicksGame) < 0)
            {
                Log.Warning($"MFI removed one or more stale incidents from the incidentQueue.");/*Please contact the mod author and provide your previous auto-save.*/
                //Log.TryOpenLogWindow();
            }
        }

        //working lists for ExposeData.
        private List<Faction> factionsListforInteraction = new List<Faction>();
        private List<Faction> factionsListforTimesTraded = new List<Faction>();
        private List<int> intListForInteraction = new List<int>();
        private List<int> intListforTimesTraded = new List<int>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(dict: ref nextFactionInteraction, label: "MFI_nextFactionInteraction", keyLookMode: LookMode.Reference, valueLookMode: LookMode.Value, keysWorkingList: ref factionsListforInteraction, valuesWorkingList: ref intListForInteraction);
            Scribe_Collections.Look(dict: ref timesTraded, label: "MFI_timesTraded", keyLookMode: LookMode.Reference, valueLookMode: LookMode.Value, keysWorkingList: ref factionsListforTimesTraded, valuesWorkingList: ref intListforTimesTraded);
        }
    }
}
