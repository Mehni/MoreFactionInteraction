using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    public class IncidentWorker_Extortion : IncidentWorker
    {
        private const int TimeoutTicks = GenDate.TicksPerDay;

        private Faction faction;
        private WorldObject worldObject;

        public override float AdjustedChance
        {
            get
            {
                float modifier = NearbyHostileEncampments().Count() / 10;
                return this.def.baseChance * 1 + modifier;
            }
        }

        private static bool RandomNearbyHostileWorldObject(int originTile, out WorldObject encampment, out Faction faction)
        {
            encampment = NearbyHostileEncampments(originTile).RandomElementWithFallback(null);

            faction = encampment?.Faction;
            if (faction != null) return true;

            return false;
        }

        private static IEnumerable<WorldObject> NearbyHostileEncampments(int forTile = -1)
        {
        	if (Find.AnyPlayerHomeMap != null)
                forTile = Find.AnyPlayerHomeMap.Tile;
            else if (Find.CurrentMap != null)
                forTile = Find.CurrentMap.Tile;

            return from worldObject in Find.WorldObjects.AllWorldObjects
                                    where (worldObject is Settlement || worldObject is Site)
                                    && worldObject.Faction.HostileTo(Faction.OfPlayer)
                                    && Find.WorldGrid.ApproxDistanceInTiles(forTile, worldObject.Tile) < 15f
                                    && (Find.WorldReachability.CanReach(forTile, worldObject.Tile) || forTile == -1)
                                    select worldObject;
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            return CommsConsoleUtility.PlayerHasPoweredCommsConsole(map) && base.CanFireNowSub(parms) && RandomNearbyHostileWorldObject(parms.target.Tile, out worldObject, out faction);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            
            if (RandomNearbyHostileWorldObject(map.Tile, out worldObject, out faction))
            {
                //technically the math.max is nonsense since this incident uses Misc category, and points don't get calculated for that. Left in for future expansion.
                int extorsionDemand = Math.Max(Rand.Range(150, 300), (int)parms.points) * NearbyHostileEncampments(map.Tile).Count();

                ChoiceLetter_ExtortionDemand choiceLetter_ExtortionDemand = (ChoiceLetter_ExtortionDemand)LetterMaker.MakeLetter(this.def.letterLabel, "MFI_ExtortionDemand".Translate(new object[]
                {
                    faction.leader.LabelShort,
                    faction.def.leaderTitle,
                    faction.Name,
                    worldObject.def.label,
                    worldObject.Label,
                    extorsionDemand,
                }).AdjustedFor(faction.leader), this.def.letterDef);
                choiceLetter_ExtortionDemand.title = "MFI_ExtortionDemandTitle".Translate(new object[]
                {
                    map.info.parent.Label
                }).CapitalizeFirst();
                if (worldObject is Site) choiceLetter_ExtortionDemand.outpost = true;
                choiceLetter_ExtortionDemand.radioMode = true;
                choiceLetter_ExtortionDemand.faction = faction;
                choiceLetter_ExtortionDemand.map = map;
                choiceLetter_ExtortionDemand.fee = extorsionDemand;
                choiceLetter_ExtortionDemand.StartTimeout(TimeoutTicks);
                Find.LetterStack.ReceiveLetter(choiceLetter_ExtortionDemand);
                return true;
            }
            return false;
        }
    }
}