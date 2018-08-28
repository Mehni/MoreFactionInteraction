using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Harmony;
using System.Reflection;

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
                        Rand.PushState(replacementSeed: faction.loadID ^ faction.GetHashCode());
                        this.nextFactionInteraction.Add(key: faction, value: Find.TickManager.TicksGame + Rand.RangeInclusive(min: GenDate.TicksPerDay * 2, max: GenDate.TicksPerDay * 8));
                        Rand.PopState();
                    }
                }

                if (nextFactionInteraction.RemoveAll(x => x.Key.HostileTo(Faction.OfPlayerSilentFail)) > 0)
                {
                    List<QueuedIncident> queued = Traverse.Create(Find.Storyteller.incidentQueue).Field("queuedIncidents").GetValue<List<QueuedIncident>>();
                    queued.RemoveAll(qi => qi.FiringIncident.parms.faction.HostileTo(Faction.OfPlayer) && this.allowedIncidentDefs.Contains(qi.FiringIncident.def));
                }

                //and the opposite
                while ((from faction in Find.FactionManager.AllFactionsVisible
                        where !faction.IsPlayer && faction != Faction.OfPlayerSilentFail && !faction.HostileTo(other: Faction.OfPlayerSilentFail) && !this.nextFactionInteraction.ContainsKey(key: faction)
                        select faction).Any())
                {
                    this.nextFactionInteraction.Add(
                        key: (from faction in Find.FactionManager.AllFactionsVisible
                              where !faction.HostileTo(other: Faction.OfPlayerSilentFail) && !faction.IsPlayer && !this.nextFactionInteraction.ContainsKey(key: faction)
                              select faction).First(),
                        value: Find.TickManager.TicksGame + Rand.RangeInclusive(min: GenDate.TicksPerDay * 2, max: GenDate.TicksPerDay * 8));
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
                        IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.FactionArrival, target: this.map);
                        incidentParms.faction = faction;
                        //forced, because half the time game doesn't feel like firing events.
                        incidentParms.forced = true;

                        //trigger incident somewhere between half a day and 3 days from now
                        Find.Storyteller.incidentQueue.Add(def: IncomingIncidentDef() ?? IncomingIncidentDef(), // overly-cautious "try again" null-check after strange bugreport.
                                                           fireTick: Find.TickManager.TicksGame + Rand.Range(min: GenDate.TicksPerDay / 2, max: GenDate.TicksPerDay * 3),
                                                           parms: incidentParms);

                        this.NextFactionInteraction[key: faction] =
                            Find.TickManager.TicksGame
                              + (int)(FactionInteractionTimeSeperator.TimeBetweenInteraction.Evaluate(faction.PlayerGoodwill)
                                    * MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction);


                        //kids, you shouldn't change values you iterate over.
                        break;
                    }
                }
            }
        }

        private static IncidentDef IncomingIncidentDef()
        {
            switch (Rand.RangeInclusive(min: 0, max: 50))
            {
                //kinda dirty hack that also means if you set the modifier to 2 you get fewer of the other quests. Oh well.
                case int n when n <= 6 * MoreFactionInteraction_Settings.pirateBaseUpgraderModifier:
                    return MFI_DefOf.MFI_QuestSpreadingPirateCamp;

                case int n when n <= 7:
                    return IncidentDef.Named("Quest_BanditCamp");

                case int n when n <= 9:
                    return MFI_DefOf.MFI_DiplomaticMarriage;

                case int n when n <= 17:
                    return MFI_DefOf.MFI_ReverseTradeRequest;

                case int n when n <= 25:
                    return MFI_DefOf.MFI_BumperCropRequest;

                case int n when n <= 29:
                    return MFI_DefOf.MFI_HuntersLodge;

                case int n when n <= 31:
                    return IncidentDef.Named("MFI_MysticalShaman");

                case int n when n <= 35:
                    return IncidentDef.Named("Quest_ItemStash");

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

        //working lists for ExposeData.
        private List<Faction> factionsListforInteraction = new List<Faction>();
        private List<Faction> factionsListforTimesTraded = new List<Faction>();
        private List<int> intListForInteraction = new List<int>();
        private List<int> intListforTimesTraded = new List<int>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Faction, int>(dict: ref this.nextFactionInteraction, label: "MFI_nextFactionInteraction", keyLookMode: LookMode.Reference, valueLookMode: LookMode.Value, keysWorkingList: ref this.factionsListforInteraction, valuesWorkingList: ref this.intListForInteraction);
            Scribe_Collections.Look<Faction, int>(dict: ref this.timesTraded, label: "MFI_timesTraded", keyLookMode: LookMode.Reference, valueLookMode: LookMode.Value, keysWorkingList: ref this.factionsListforTimesTraded, valuesWorkingList: ref this.intListforTimesTraded);
        }
    }
}
