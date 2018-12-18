using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace MoreFactionInteraction
{
    class WorldComponent_OutpostGrower : WorldComponent
    {
        private List<ChoiceLetter> choiceLetters = new List<ChoiceLetter>();

        public WorldComponent_OutpostGrower(World world) : base(world: world)
        {
        }

        public override void WorldComponentUpdate()
        {
            base.WorldComponentUpdate();
            if (Find.TickManager.TicksGame % 100 == 0)
            {
                //get settlements to upgrade. These shouldn't include temp generated or event maps -- preferably only the outposts this spawned by this mod
                //ideally I'd add some specific Component to each outpost (as a unique identifier and maybe even as the thing that makes em upgrade), but for the moment that's not needed.

                IEnumerable<Site> sites = from site in Find.WorldObjects.Sites
                                          where site.Faction.HostileTo(other: Faction.OfPlayer)
                                              && site.Faction.def.permanentEnemy && !site.Faction.def.hidden
                                              && !site.Faction.defeated
                                              && (site.HasMap ? site.ShouldRemoveMapNow(out bool alsoRemoveWorldObject) : true)
                                              && site.parts.Any(predicate: (SitePart x) => x.Def == SitePartDefOf.Outpost)
                                              && !site.GetComponent<TimeoutComp>().Active
                                          select site;

                Site toUpgrade = null;

                foreach (Site current in sites)
                {
                    if (current.creationGameTicks + MoreFactionInteraction_Settings.ticksToUpgrade <= Find.TickManager.TicksGame)
                    {
                        toUpgrade = current;
                        break;
                    }
                }

                if (toUpgrade != null)
                {
                    UpgradeSiteToSettlement(toUpgrade);
                }
                TickLetters();
            }
        }

        private static void UpgradeSiteToSettlement(Site toUpgrade)
        {
            Settlement factionBase = (Settlement)WorldObjectMaker.MakeWorldObject(def: WorldObjectDefOf.Settlement);
            factionBase.SetFaction(newFaction: toUpgrade.Faction);
            factionBase.Tile = toUpgrade.Tile;
            factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase: factionBase);
            Find.WorldObjects.Remove(o: toUpgrade);
            Find.WorldObjects.Add(o: factionBase);
            Find.LetterStack.ReceiveLetter(label: "MFI_LetterLabelBanditOutpostUpgraded".Translate(), text: "MFI_LetterBanditOutpostUpgraded".Translate(
                    factionBase.Faction.Name
            ), textLetterDef: LetterDefOf.NeutralEvent, lookTargets: factionBase, relatedFaction: toUpgrade.Faction);
        }

        private void TickLetters()
        {
            foreach (ChoiceLetter letter in choiceLetters)
            {
                if (letter == null)
                {
                    choiceLetters.Remove(letter);
                    break;
                }
                if (Find.TickManager.TicksGame > letter.disappearAtTick)
                {
                    letter.OpenLetter();
                    choiceLetters.Remove(letter);
                    break;
                }
            }
        }

        public void Registerletter(ChoiceLetter choiceLetter)
        {
            choiceLetters.Add(choiceLetter);
        }

        public override void ExposeData()
        {
            //this is where I store letters, because RimWorld just goes and deletes them.
            base.ExposeData();
            Scribe_Collections.Look<ChoiceLetter>(ref choiceLetters, "MFI_ChoiceLetters", LookMode.Deep);
        }
    }
}
