using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using RimWorld;
using Verse;
using UnityEngine;

namespace MoreFactionInteraction
{
    class WorldComponent_OutpostGrower : WorldComponent
    {
        public WorldComponent_OutpostGrower(World world) : base(world)
        {
        }

        public override void WorldComponentUpdate()
        {
            base.WorldComponentUpdate();
            if (Find.TickManager.TicksGame % 100 == 0)
            {
                //get settlements to upgrade. These shouldn't include temp generated or event maps -- preferably only the outposts this spawned by this mod
                IEnumerable<Site> sites = from site in Find.WorldObjects.Sites
                                          where site.Faction.HostileTo(Faction.OfPlayer) && !site.Faction.def.appreciative && !site.Faction.def.hidden && !site.Faction.defeated
                                          && site.KnownDanger && site.parts.Contains<SitePartDef>(SitePartDefOf.Outpost) && !site.GetComponent<TimeoutComp>().Active
                                          select site;

                Site toUpgrade = null;

                foreach (Site current in sites)
                {
                    if (current.creationGameTicks + MoreFactionInteraction_Settings.ticksToUpgrade <= Find.TickManager.TicksGame)
                    {
                        toUpgrade = current;
                    }
                }

                if (toUpgrade != null)
                {
                    FactionBase factionBase = (FactionBase)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.FactionBase);
                    factionBase.SetFaction(toUpgrade.Faction);
                    factionBase.Tile = toUpgrade.Tile;
                    factionBase.Name = FactionBaseNameGenerator.GenerateFactionBaseName(factionBase);
                    Find.WorldObjects.Remove(toUpgrade);
                    Find.WorldObjects.Add(factionBase);
                    Find.LetterStack.ReceiveLetter("LetterLabelBanditOutpostUpgraded".Translate(), "LetterBanditOutpostUpgraded".Translate(new object[]
                    {
                            factionBase.Faction.Name,
                    }), LetterDefOf.NeutralEvent, factionBase, null);
                }
            }
        }
    }
}
