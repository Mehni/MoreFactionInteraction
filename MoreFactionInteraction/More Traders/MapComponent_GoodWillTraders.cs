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
        public MapComponent_GoodWillTrader(Map map) : base(map)
        {
        }

        private Dictionary<Faction, int> nextFactionInteraction = new Dictionary<Faction, int>();

        public Dictionary<Faction, int> NextFactionInteraction
        {
            get
            {
                if (this.nextFactionInteraction.Count == 0)
                {
                    IEnumerable<Faction> friendlyFactions = from faction in Find.FactionManager.AllFactionsVisible
                                                     where !faction.HostileTo(Faction.OfPlayer)
                                                     select faction;

                    foreach (Faction faction in friendlyFactions)
                    {
                        nextFactionInteraction.Add(faction, Find.TickManager.TicksGame + Rand.RangeInclusive(GenDate.TicksPerDay * 5, GenDate.TicksPerDay * 8));
                    }
                }
                return nextFactionInteraction;
            }
        }

        public override void MapComponentTick()
        {
            foreach (KeyValuePair<Faction, int> kvp in nextFactionInteraction)
            {
                if (kvp.Value >= Find.TickManager.TicksGame)
                {

                }
            }
        }
    }
}
