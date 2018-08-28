//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using Verse;

//namespace MoreFactionInteraction.MoreFactionWar
//{
//    public class IncidentWorker_SellingWarBonds : IncidentWorker
//    {
//        public override float AdjustedChance => base.AdjustedChance;

//        protected override bool CanFireNowSub(IncidentParms parms)
//        {
//            return base.CanFireNowSub(parms) && Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing
//                                             && FindAlliedWarringFaction(faction: out Faction faction)
//                                             && CommsConsoleUtility.PlayerHasPoweredCommsConsole(map: (Map) parms.target);
//        }

//        protected override bool TryExecuteWorker(IncidentParms parms)
//        {
//            if (!FindAlliedWarringFaction(faction: out Faction faction)) return false;
//            if (faction == null) return false;

//            return false;

//        }

//        /// <summary>
//        /// Find warring allied faction that can send drop pods.
//        /// </summary>
//        /// <param name="faction"></param>
//        /// <returns></returns>
//        protected bool FindAlliedWarringFaction(out Faction faction)
//        {
//            faction = null;

//            if (!Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing)
//                return false;

//            if (Find.World.GetComponent<WorldComponent_MFI_FactionWar>().AllFactionsInVolvedInWar
//                    .Where(predicate: f => f.RelationWith(other: Faction.OfPlayer).kind == FactionRelationKind.Ally
//                                        && f.def.techLevel >= TechLevel.Industrial).TryRandomElementByWeight(weightSelector: f => f.def.RaidCommonalityFromPoints(points: 600f), result: out faction))
//                return true;

//            return false;
//        }
//    }
//}
