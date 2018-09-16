using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;
using System;

namespace MoreFactionInteraction.MoreFactionWar
{
    public class MainTabWindow_FactionWar : MainTabWindow_Factions
    {
        private const float TitleHeight = 70f;
        private const float InfoHeight = 60f;

        private static Texture2D factionOneColorTexture;
        private static Texture2D factionTwoColorTexture;

        public static Texture2D FactionOneColorTexture
        {
            get
            {
                Faction factionOne = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne;
                
                if (factionOneColorTexture == null && factionOne != null)
                {
                    factionOneColorTexture = SolidColorMaterials.NewSolidColorTexture(factionOne.Color);
                }
                return factionOneColorTexture;
            }
        }

        public static Texture2D FactionTwoColorTexture
        {
            get
            {
                Faction factionInstigator = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;

                if (factionTwoColorTexture == null && factionInstigator != null)
                {
                    factionTwoColorTexture = SolidColorMaterials.NewSolidColorTexture(factionInstigator.Color);
                }
                return factionTwoColorTexture;
            }
        }


        //[TweakValue("MainTabWindow_FactionWar", -100f, 150f)]
        private static float yMaxOffset = 0;

        //[TweakValue("MainTabWindow_FactionWar", -50f, 50f)]
        private static float yPositionBar = 33;

        //[TweakValue("MainTabWindow_FactionWar", -50f, 50f)]
        private static float barHeight = 32;


        public override void DoWindowContents(Rect fillRect)
        {
            //regular faction tab if no war/unrest. Fancy tab otherwise.
            if (!Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing)
            {
                base.DoWindowContents(fillRect);
            }
            else
            {
                DrawFactionWarBar(fillRect);

                // scooch down original. amount of offset depends on devmode or not (because of devmode "show all" button)
                Rect baseRect = fillRect;
                baseRect.y = (Prefs.DevMode) ? fillRect.y + 120f : fillRect.y + 75f;
                baseRect.yMax = fillRect.yMax + yMaxOffset;

                GUI.BeginGroup(baseRect);
                base.DoWindowContents(baseRect);
                GUI.EndGroup();
            }
        }

        public static void DrawFactionWarBar(Rect fillRect)
        {
            if (!Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StuffIsGoingDown)
                return;

            Faction factionOne = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne;
            Faction factionInstigator = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;

            Rect position = new Rect(fillRect.x, fillRect.y, fillRect.width, fillRect.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            //4 boxes: faction name and faction call label.
            Rect leftFactionLabel = new Rect(x: 0f, y: 0f, width: position.width / 2f, height: TitleHeight);
            Rect leftBox = new Rect(x: 0f, y: leftFactionLabel.yMax, width: leftFactionLabel.width, height: InfoHeight);
            Rect rightFactionLabel = new Rect(x: position.width / 2f, y: 0f, width: position.width / 2f, height: TitleHeight);
            Rect rightBox = new Rect(x: position.width / 2f, y: leftFactionLabel.yMax, width: leftFactionLabel.width, height: InfoHeight);

            //big central box
            Rect centreBoxForBigFactionwar = new Rect(0f, leftFactionLabel.yMax, width: position.width, height: TitleHeight);
            Text.Font = GameFont.Medium;
            GUI.color = Color.cyan;
            Text.Anchor = TextAnchor.MiddleCenter;
            string factionWarStatus = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing
                                          ? "MFI_FactionWarProgress".Translate()
                                          : "MFI_UnrestIsBrewing".Translate();
            Widgets.Label(centreBoxForBigFactionwar, factionWarStatus);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;

            //"score card" bar
            Rect leftFactionOneScoreBox = new Rect(0f, yPositionBar, position.width * Find.World.GetComponent<WorldComponent_MFI_FactionWar>().ScoreForFaction(factionOne), barHeight);
            GUI.DrawTexture(leftFactionOneScoreBox, FactionOneColorTexture);
            Rect rightFactionTwoScoreBox = new Rect(position.width * Find.World.GetComponent<WorldComponent_MFI_FactionWar>().ScoreForFaction(factionOne), yPositionBar, position.width * Find.World.GetComponent<WorldComponent_MFI_FactionWar>().ScoreForFaction(factionInstigator), barHeight);
            GUI.DrawTexture(rightFactionTwoScoreBox, FactionTwoColorTexture);

            //stuff that fills up and does the faction name and call label boxes.
            Text.Font = GameFont.Medium;
            Widgets.Label(rect: leftFactionLabel, label: factionOne.GetCallLabel());
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect: rightFactionLabel, label: factionInstigator.GetCallLabel());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = new Color(r: 1f, g: 1f, b: 1f, a: 0.7f);
            Widgets.Label(rect: leftBox, label: factionOne.GetInfoText());

            if (factionOne != null)
            {
                FactionRelationKind playerRelationKind = factionOne.PlayerRelationKind;
                GUI.color = playerRelationKind.GetColor();
                Rect factionOneRelactionBox = new Rect(x: leftBox.x, y: leftBox.y + Text.CalcHeight(text: factionOne.GetInfoText(), width: leftBox.width) + Text.SpaceBetweenLines, width: leftBox.width, height: 30f);
                Widgets.Label(rect: factionOneRelactionBox, label: playerRelationKind.GetLabel());
            }
            GUI.color = new Color(r: 1f, g: 1f, b: 1f, a: 0.7f);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect: rightBox, label: factionInstigator.GetInfoText());

            if (factionInstigator != null)
            {
                FactionRelationKind playerRelationKind = factionInstigator.PlayerRelationKind;
                GUI.color = playerRelationKind.GetColor();
                Rect factionInstigatorRelactionBox = new Rect(x: rightBox.x, y: rightBox.y + Text.CalcHeight(text: factionInstigator.GetInfoText(), width: rightBox.width) + Text.SpaceBetweenLines, width: rightBox.width, height: 30f);
                Widgets.Label(rect: factionInstigatorRelactionBox, label: playerRelationKind.GetLabel());
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            GUI.EndGroup();
        }
    }
}
