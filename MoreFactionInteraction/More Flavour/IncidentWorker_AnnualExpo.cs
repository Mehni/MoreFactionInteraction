using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using MoreFactionInteraction.General;


namespace MoreFactionInteraction.More_Flavour
{

    public class IncidentWorker_AnnualExpo : IncidentWorker
    {
        private static readonly List<Map> tmpAvailableMaps = new List<Map>();
        private const int MinDistance = 12;
        private const int MaxDistance = 26;
        private static readonly IntRange TimeoutDaysRange = new IntRange(min: 21, max: 23);

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null
                                             && TryGetRandomAvailableTargetMap(out Map map)
                                             && TryFindTile(tile: out int num);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryGetRandomAvailableTargetMap(map: out Map map))
                return false;

            if (map == null)
                return false;

            if (!TryFindTile(tile: out int tile))
                return false;

            if (!TryGetFactionHost(out Faction faction))
                return false;


            AnnualExpo annualExpo = (AnnualExpo)WorldObjectMaker.MakeWorldObject(def: MFI_DefOf.MFI_AnnualExpoObject);
            annualExpo.Tile = tile;
            annualExpo.GetComponent<TimeoutComp>().StartTimeout(TimeoutDaysRange.RandomInRange * GenDate.TicksPerDay);
            Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().Events.InRandomOrder().TryMinBy(kvp => kvp.Value, out KeyValuePair<EventDef, int> result);
            annualExpo.eventDef = result.Key;
            annualExpo.host = faction;

            Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().timesHeld++;

            Find.WorldObjects.Add(o: annualExpo);
            Find.LetterStack.ReceiveLetter(label: this.def.letterLabel,
                                            text: "MFI_AnnualExpoLetterText".Translate(
                                                Find.ActiveLanguageWorker.OrdinalNumber(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().TimesHeld), 
                                                Find.World.info.name,
                                                annualExpo.host.Name,
                                                annualExpo.eventDef.theme.Translate(),
                                                annualExpo.eventDef.themeDesc.Translate() ),
                                            textLetterDef: this.def.letterDef,
                                            lookTargets: annualExpo);

            return true;
        }

        private static bool TryGetFactionHost(out Faction faction) => Find
                                                     .FactionManager.AllFactionsVisible
                                                     .Where(x => !x.defeated && !x.def.permanentEnemy).TryRandomElement(out faction);

        private static bool TryGetRandomAvailableTargetMap(out Map map)
        {
            tmpAvailableMaps.Clear();
            List<Map> maps = Find.Maps;
            foreach (Map potentialTargetMap in maps)
            {
                if (potentialTargetMap.IsPlayerHome)
                {
                    tmpAvailableMaps.Add(item: potentialTargetMap);
                }
            }
            bool result = tmpAvailableMaps.TryRandomElement(result: out map);
            tmpAvailableMaps.Clear();
            return result;
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(tile: out tile, minDist: MinDistance, maxDist: MaxDistance, allowCaravans: true, preferCloserTiles: false);
        }
    }
}
