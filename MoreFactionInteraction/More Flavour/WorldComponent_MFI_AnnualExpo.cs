//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using Verse;
//using RimWorld.Planet;
//using Harmony;
//using System.Reflection;

//namespace MoreFactionInteraction.More_Flavour
//{
//    public class WorldComponent_MFI_AnnualExpo : WorldComponent
//    {
//        private readonly IncidentDef incident = MFI_DefOf.MFI_QuestSpreadingPirateCamp; //MFI_DefOf.MFI_AnnualExpo;
//        private readonly float intervalDays = 60f;
//        private float occuringTick;
//        public Buff_Tracker buff = new Buff_Tracker();

//        public bool BuffedEmanator => this.buff.AllBuffsForReading.Find(x => x is Buff_Emanator).active; //used by patches.


//        public WorldComponent_MFI_AnnualExpo(World world) : base(world)
//        {
//        }

//        public override void FinalizeInit()
//        {
//            base.FinalizeInit();
//            //buff.AllBuffsForReading.Find(x => x is Buff_Emanator).Apply();
//            ApplyAllBuffs();
//            if (this.occuringTick < 4f) // I picked 4 in case of extraordinarily large values of 0.
//                this.occuringTick = new FloatRange(GenDate.TicksPerDay * 45, GenDate.TicksPerYear).RandomInRange;
//        }

//        public override void WorldComponentTick()
//        {
//            base.WorldComponentTick();
//            if (Find.AnyPlayerHomeMap == null) return;

//            if (Find.TickManager.TicksGame >= this.occuringTick)
//            {
//                IncidentParms parms = StorytellerUtility.DefaultParmsNow(this.incident.category, (from x in Find.Maps
//                                                                                                  where x.IsPlayerHome
//                                                                                                  select x).RandomElement());
//                if (this.incident.Worker.TryExecute(parms))
//                    this.occuringTick += this.IntervalTicks;

//                else
//                    this.occuringTick += GenDate.TicksPerDay;
//            }
//        }

//        public override void ExposeData()
//        {
//            base.ExposeData();
//            Scribe_Values.Look(ref this.occuringTick, "MFI_occuringTick");
//        }

//        private float IntervalTicks => GenDate.TicksPerDay * this.intervalDays;

//        private void ApplyAllBuffs()
//        {
//            foreach (Buff currentBuff in buff.AllBuffsForReading)
//            {
//                if (currentBuff.active)
//                    currentBuff.Apply();
//            }
//        }
//    }
//}
