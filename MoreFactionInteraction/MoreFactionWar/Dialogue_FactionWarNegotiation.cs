using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;

namespace MoreFactionInteraction
{
    public class Dialogue_FactionWarNegotiation : Dialog_NodeTree
    {
        private const float TitleHeight = 70f;
        private const float InfoHeight = 60f;
        private Faction factionOne;
        private Faction factionInstigator;

        public override Vector2 InitialSize => new Vector2(x: 720f, y: 600f);

        public Dialogue_FactionWarNegotiation(Faction factionOne, Faction factionInstigator, DiaNode nodeRoot, bool delayInteractivity = false, bool radioMode = false, string title = null) : base(nodeRoot: nodeRoot, delayInteractivity: delayInteractivity, radioMode: radioMode, title: title)
        {
            this.factionOne = factionOne;
            this.factionInstigator = factionInstigator;
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(position: inRect);
            Rect leftFactionLabel = new Rect(x: 0f, y: 0f, width: inRect.width / 2f, height: TitleHeight);
            Rect leftBox = new Rect(x: 0f, y: leftFactionLabel.yMax, width: leftFactionLabel.width, height: InfoHeight);
            Rect rightFactionLabel = new Rect(x: inRect.width / 2f, y: 0f, width: inRect.width / 2f, height: TitleHeight);
            Rect rightBox = new Rect(x: inRect.width / 2f, y: leftFactionLabel.yMax, width: leftFactionLabel.width, height: InfoHeight);

            Text.Font = GameFont.Medium;
            Widgets.Label(rect: leftFactionLabel, label: this.factionOne.GetCallLabel());
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect: rightFactionLabel, label: this.factionInstigator.GetCallLabel());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = new Color(r: 1f, g: 1f, b: 1f, a: 0.7f);
            Widgets.Label(rect: leftBox, label: this.factionOne.GetInfoText());
            if (this.factionOne != null)
            {
                FactionRelationKind playerRelationKind = this.factionOne.PlayerRelationKind;
                GUI.color = playerRelationKind.GetColor();
                Rect factionOneRelactionBox = new Rect(x: leftBox.x, y: leftBox.y + Text.CalcHeight(text: this.factionOne.GetInfoText(), width: leftBox.width) + Text.SpaceBetweenLines, width: leftBox.width, height: 30f);
                Widgets.Label(rect: factionOneRelactionBox, label: playerRelationKind.GetLabel());
            }
            GUI.color = new Color(r: 1f, g: 1f, b: 1f, a: 0.7f);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect: rightBox, label: this.factionInstigator.GetInfoText());

            if (this.factionInstigator != null)
            {
                FactionRelationKind playerRelationKind = this.factionInstigator.PlayerRelationKind;
                GUI.color = playerRelationKind.GetColor();
                Rect factionInstigatorRelactionBox = new Rect(x: rightBox.x, y: rightBox.y + Text.CalcHeight(text: this.factionInstigator.GetInfoText(), width: rightBox.width) + Text.SpaceBetweenLines, width: rightBox.width, height: 30f);
                Widgets.Label(rect: factionInstigatorRelactionBox, label: playerRelationKind.GetLabel());
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            GUI.EndGroup();
            const float magicalNum = 147f;
            Rect middleRemainingRectForDialog = new Rect(x: 0f, y: magicalNum, width: inRect.width, height: inRect.height - magicalNum);
            this.DrawNode(rect: middleRemainingRectForDialog);
        }
    }
}
