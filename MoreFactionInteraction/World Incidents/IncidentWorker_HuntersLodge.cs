using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimWorld.BaseGen;
using Verse;
//using Kitchen.Sink;
using Verse.AI;
using Verse.AI.Group;

namespace MoreFactionInteraction
{
    public class IncidentWorker_HuntersLodge : IncidentWorker
    {
        private const float NoSitePartChance = 0.3f;
        private const int MinDistance = 2;
        private const int MaxDistance = 15;
        private static readonly string DownedRefugeeQuestThreatTag = "DownedRefugeeQuestThreat";
        private static readonly IntRange TimeoutDaysRange = new IntRange(5, 10);



        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            return base.CanFireNowSub(target) && this.TryFindTile(out int num) && SiteMakerHelper.TryFindRandomFactionFor(MFI_DefOf.HuntersLodge, null, out Faction faction, true, null);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!this.TryFindTile(out int tile))
            {
                return false;
            }
            Site site = SiteMaker.TryMakeSite_SingleSitePart(MFI_DefOf.HuntersLodge, (!Rand.Chance(NoSitePartChance)) ? DownedRefugeeQuestThreatTag : null, null, true, null);
            if (site == null)
            {
                return false;
            }
            site.Tile = tile;
            Pawn pawn = DownedRefugeeQuestUtility.GenerateRefugee(tile);
            site.GetComponent<DownedRefugeeComp>().pawn.TryAdd(pawn, true);
            int randomInRange = IncidentWorker_HuntersLodge.TimeoutDaysRange.RandomInRange;
            site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * GenDate.TicksPerDay);
            Find.WorldObjects.Add(site);
            string text = string.Format(this.def.letterText.AdjustedFor(pawn), pawn.Label, randomInRange).CapitalizeFirst();


            Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterDef, site, null);
            return true;
        }

        private bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(out tile, MinDistance, MaxDistance, true, false);
        }
    }
    
}
