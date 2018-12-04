using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction.More_Flavour
{
    public class WorldComponent_MFI_AnnualExpo : WorldComponent
    {
        private readonly IncidentDef incident = MFI_DefOf.MFI_AnnualExpo;
        private readonly float intervalDays = 60f;
        private float occuringTick;
        public int timesHeld = 0;
        private List<Buff> activeBuffList = new List<Buff>();

        public List<Buff> ActiveBuffsList => this.activeBuffList;

        public int TimesHeld => timesHeld + Rand.RangeInclusiveSeeded(
            (int)PawnKindDefOf.Muffalo.race.race.lifeExpectancy,
            (int)PawnKindDefOf.Thrumbo.race.race.lifeExpectancy,
            (int)(Rand.ValueSeeded(Find.World.ConstantRandSeed) * 1000));

        public bool BuffedEmanator => (this.ActiveBuffsList.Find(x => x is Buff_Emanator)?.Active) ?? false; //used by patches. 

        public void RegisterBuff(Buff buff)
        {
            if (!this.ActiveBuffsList.Contains(buff))
                this.ActiveBuffsList.Add(buff);
        }

        public Dictionary<EventDef, int> events = new Dictionary<EventDef, int>
        {
            { MFI_DefOf.MFI_GameOfUrComp, 0 },
            { MFI_DefOf.MFI_ShootingComp, 0 },
            { MFI_DefOf.MFI_CulturalSwap, 0 },
            { MFI_DefOf.MFI_ScienceFaire, 0 },
            { MFI_DefOf.MFI_AcousticShow, 0 },
        };

        public WorldComponent_MFI_AnnualExpo(World world) : base(world)
        {
        }

        public Buff ApplyRandomBuff(Predicate<Buff> validator)
        {
            if (this.ActiveBuffsList.Where(x => validator(x)).TryRandomElement(out Buff result))
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

            foreach (Buff item in this.ActiveBuffsList.Where(x => x.Active))
            {
                item.Apply();
            }

            if (this.occuringTick < 4f & this.timesHeld == 0) // I picked 4 in case of extraordinarily large values of 0.
                this.occuringTick = new FloatRange(GenDate.TicksPerDay * 45, GenDate.TicksPerYear).RandomInRange;
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.AnyPlayerHomeMap == null) return;

            if (Find.TickManager.TicksGame >= this.occuringTick)
            {
                IncidentParms parms = StorytellerUtility.DefaultParmsNow(this.incident.category, Find.Maps.Where(x => x.IsPlayerHome).RandomElement());

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
            Scribe_Collections.Look(ref this.events, "MFI_Events");
            Scribe_Collections.Look(ref this.activeBuffList, "MFI_buffList");
            Scribe_Values.Look(ref this.timesHeld, "MFI_AnnualExpoTimesHeld");
        }

        private float IntervalTicks => GenDate.TicksPerDay * this.intervalDays;
    }
}
