using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using Harmony;
using System.Reflection;

namespace MoreFactionInteraction.More_Flavour
{
    public class WorldComponent_MFI_AnnualExpo : WorldComponent
    {
        private readonly IncidentDef incident = MFI_DefOf.MFI_AnnualExpo;
        private readonly float intervalDays = 60f;
        private float occuringTick;
        public int timesHeld = 0;
        private List<Buff> activeBuffList = new List<Buff>();

        public int TimesHeld => timesHeld + Math.Abs((int)Rand.ValueSeeded(Find.World.ConstantRandSeed)) % 1000;

        public bool BuffedEmanator => (this.activeBuffList.Find(x => x is Buff_Emanator)?.Active) ?? false; //used by patches.

        public void RegisterBuff(Buff buff)
        {
            if (!this.activeBuffList.Contains(buff))
                this.activeBuffList.Add(buff);
        }

        public readonly Dictionary<string, StatDef> relevantXpForEvent = new Dictionary<string, StatDef>
        {
            { "gameOfUrComp", StatDefOf.PsychicSensitivity },
            { "shootingComp", StatDefOf.ShootingAccuracyPawn },
            { "culturalSwap", StatDefOf.SocialImpact },
            { "scienceFaire", StatDefOf.ResearchSpeed },
            { "acousticShow", StatDefOf.TradePriceImprovement },
        };

        public readonly Dictionary<string, int> Events = new Dictionary<string, int>
        {
            //{ "buffChemfuel", 0 },
            //{ "buffPemmican", 0 },
            //{ "buffEmanator", 0 },
            //{ "buffPsychite", 0 },
            { "gameOfUrComp", 0 },
            { "shootingComp", 0 },
            { "culturalSwap", 0 },
            { "scienceFaire", 0 },
            { "acousticShow", 0 },
        };

        public WorldComponent_MFI_AnnualExpo(World world) : base(world)
        {
        }

        public Buff ApplyRandomBuff(Predicate<Buff> validator = null)
        {
            if (this.activeBuffList.Where(x => validator(x)).TryRandomElement(out Buff result))
                result.Apply();

            return result;
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            Buff_Chemfuel.Register();
            Buff_Emanator.Register();
            Buff_Pemmican.Register();
            Buff_PsychTea.Register();
            
            foreach (Buff item in activeBuffList.Where(x => x.Active))
            {
                item.Apply();
            }

            if (this.occuringTick < 4f) // I picked 4 in case of extraordinarily large values of 0.
                this.occuringTick = new FloatRange(GenDate.TicksPerDay * 45, GenDate.TicksPerYear).RandomInRange;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.AnyPlayerHomeMap == null) return;

            if (Find.TickManager.TicksGame >= this.occuringTick)
            {
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(this.incident.category, (from x in Find.Maps
                                                                                                  where x.IsPlayerHome
                                                                                                  select x).RandomElement());

                if (this.incident.Worker.TryExecute(parms))
                    this.occuringTick += this.IntervalTicks;

                else
                    this.occuringTick += GenDate.TicksPerDay;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.occuringTick, "MFI_occuringTick");
            Scribe_Collections.Look(ref this.activeBuffList, "MFI_buffList");
            Scribe_Values.Look(ref this.timesHeld, "MFI_AnnualExpoTimesHeld");
        }

        private float IntervalTicks => GenDate.TicksPerDay * this.intervalDays;
    }
}
