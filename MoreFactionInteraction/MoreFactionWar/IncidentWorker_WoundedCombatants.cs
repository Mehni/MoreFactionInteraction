using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using MoreFactionInteraction.General;

namespace MoreFactionInteraction.MoreFactionWar
{
	public class IncidentWorker_WoundedCombatants : IncidentWorker
	{
		private readonly IntRange pawnstoSpawn = new IntRange(min: 4, max: 6);

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			return base.CanFireNowSub(parms: parms) && Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing
													&& FindAlliedWarringFaction(faction: out Faction faction)
													&& CommsConsoleUtility.PlayerHasPoweredCommsConsole(map: (Map)parms.target)
													&& DropCellFinder.TryFindRaidDropCenterClose(spot: out IntVec3 dropSpot, map: (Map)parms.target);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!DropCellFinder.TryFindRaidDropCenterClose(spot: out IntVec3 dropSpot, map: (Map)parms.target)) return false;
			if (!FindAlliedWarringFaction(faction: out Faction faction)) return false;
			if (faction == null) return false;

			bool bamboozle = false;
			string arrivalText = string.Empty;
			int factionGoodWillLoss = FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
										.GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange / 2;

			IncidentParms raidParms =
				StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.ThreatBig, target: (Map)parms.target);
			raidParms.forced = true;
			raidParms.faction = faction.EnemyInFactionWar();
			raidParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
			raidParms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
			raidParms.spawnCenter = dropSpot;

			if (faction.EnemyInFactionWar().def.techLevel >= TechLevel.Industrial
				&& faction.EnemyInFactionWar().RelationKindWith(other: Faction.OfPlayer) == FactionRelationKind.Hostile)
					bamboozle = Rand.Chance(chance: 0.25f);

			if (bamboozle)
			{
				arrivalText = string.Format(format: raidParms.raidArrivalMode.textEnemy, arg0: raidParms.faction.def.pawnsPlural, arg1: raidParms.faction.Name);
			}

			//get combat-pawns to spawn.
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(groupKind: PawnGroupKindDefOf.Combat, parms: raidParms);
			defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(points: defaultPawnGroupMakerParms.points, raidArrivalMode: raidParms.raidArrivalMode, raidStrategy: raidParms.raidStrategy, faction: defaultPawnGroupMakerParms.faction, groupKind: PawnGroupKindDefOf.Combat);
			IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(parms: defaultPawnGroupMakerParms).ToList();
			List<Thing> pawnlist = new List<Thing>();

			for (int i = 0; i < this.pawnstoSpawn.RandomInRange; i++)
			{
				PawnGenerationRequest request = new PawnGenerationRequest(kind: pawnKinds.RandomElement(), faction: faction, allowDowned: true, allowDead: true, mustBeCapableOfViolence: true);
				Pawn woundedCombatant = PawnGenerator.GeneratePawn(request: request);
				woundedCombatant.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;
				ThingDef weapon = Rand.Bool ? DefDatabase<ThingDef>.AllDefsListForReading.Where(predicate: x => x.IsWeaponUsingProjectiles).RandomElement() : null;

				ThingDef usedWeaponDef = weapon;
				DamageDef damageDef = usedWeaponDef?.Verbs?.First()?.defaultProjectile?.projectile?.damageDef; //null? check? All? THE? THINGS!!!!?
				if (usedWeaponDef != null && damageDef == null)
				{
					usedWeaponDef = null;
				}
				CustomFaction_HealthUtility.DamageUntilDownedWithSpecialOptions(p: woundedCombatant, allowBleedingWounds: true, damageDef: damageDef, weapon: usedWeaponDef);
				//todo: maybe add some storylogging.
				pawnlist.Add(item: woundedCombatant);
			}

			string initialMessage = "MFI_WoundedCombatant".Translate(faction.Name);
			DiaNode diaNode = new DiaNode(text: initialMessage);

			DiaOption diaOptionOk = new DiaOption(text: "OK".Translate()) { resolveTree = true };

			DiaOption diaOptionAccept = new DiaOption(text: "RansomDemand_Accept".Translate())
			{
				action = () =>
				{
					if (bamboozle)
					{
						Find.TickManager.slower.SignalForceNormalSpeedShort();
						IncidentDefOf.RaidEnemy.Worker.TryExecute(parms: raidParms);
					}
					else
					{
						IntVec3 intVec = IntVec3.Invalid;

						List<Building> allBuildingsColonist = ((Map)parms.target).listerBuildings.allBuildingsColonist.Where(predicate: x => x.def.thingClass == typeof(Building_Bed)).ToList();
						for (int i = 0; i < allBuildingsColonist.Count; i++)
						{
							if (DropCellFinder.TryFindDropSpotNear(center: allBuildingsColonist[index: i].Position, map: (Map)parms.target, result: out intVec, allowFogged: false, canRoofPunch: false))
							{
								break;
							}
						}
						if (intVec == IntVec3.Invalid) intVec = DropCellFinder.RandomDropSpot(map: (Map)parms.target);
						DropPodUtility.DropThingsNear(dropCenter: intVec, map: (Map)parms.target, things: pawnlist, openDelay: 180, leaveSlag: true, canRoofPunch: false);
						Find.World.GetComponent<WorldComponent_MFI_FactionWar>().NotifyBattleWon(faction: faction);
					}
				}
			};
			string bamboozledAndAmbushed = "MFI_WoundedCombatantAmbush".Translate(faction, arrivalText);
			string commanderGreatful = "MFI_WoundedCombatantGratitude".Translate();
			DiaNode acceptDiaNode = new DiaNode(text: bamboozle ? bamboozledAndAmbushed : commanderGreatful);
			diaOptionAccept.link = acceptDiaNode;
			diaNode.options.Add(item: diaOptionAccept);
			acceptDiaNode.options.Add(item: diaOptionOk);

			DiaOption diaOptionRejection = new DiaOption(text: "RansomDemand_Reject".Translate())
			{
				action = () =>
				{
					if (bamboozle)
					{
						Find.World.GetComponent<WorldComponent_MFI_FactionWar>().NotifyBattleWon(faction: faction);
					}
					else
					{
						faction.TryAffectGoodwillWith(other: Faction.OfPlayer, goodwillChange: factionGoodWillLoss, canSendMessage: false);
					}
				}
			};
			string rejectionResponse = "MFI_WoundedCombatantRejected".Translate(faction.Name, factionGoodWillLoss);
			string bamboozlingTheBamboozler = "MFI_WoundedCombatantAmbushAvoided".Translate();
			DiaNode rejectionDiaNode = new DiaNode(text: bamboozle ? bamboozlingTheBamboozler : rejectionResponse);
			diaOptionRejection.link = rejectionDiaNode;
			diaNode.options.Add(item: diaOptionRejection);
			rejectionDiaNode.options.Add(item: diaOptionOk);

			string title = "MFI_WoundedCombatantTitle".Translate(((Map)parms.target).Parent.Label);
			Find.WindowStack.Add(window: new Dialog_NodeTreeWithFactionInfo(nodeRoot: diaNode, faction: faction, delayInteractivity: true, radioMode: true, title: title));
			Find.Archive.Add(archivable: new ArchivedDialog(text: diaNode.text, title: title, relatedFaction: faction));
			return true;
		}

		/// <summary>
		/// Find warring allied faction that can send drop pods.
		/// </summary>
		/// <param name="faction"></param>
		/// <returns></returns>
		protected bool FindAlliedWarringFaction(out Faction faction)
		{
			faction = null;

			if (!Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing)
				return false;

			if (Find.World.GetComponent<WorldComponent_MFI_FactionWar>().AllFactionsInVolvedInWar
					.Where(predicate: f => f.RelationWith(other: Faction.OfPlayer).kind == FactionRelationKind.Ally
							 && f.def.techLevel >= TechLevel.Industrial).TryRandomElementByWeight(weightSelector: f => f.def.RaidCommonalityFromPoints(points: 600f), result: out faction))
				return true;

			return false;
		}
	}
}
