//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using RimWorld;
//using Verse;

//namespace MoreFactionInteraction.More_Flavour
//{
//    public class Buff_Tracker : IExposable
//    {
//        private List<int> allBuffs = new List<int>();

//        //public List<Buff> AllBuffsForReading => this.allBuffs;

//        public Buff_Tracker()
//        {
//            if (!this.allBuffs.Any())
//            {
//                //Buff_Pemmican pemmican = new Buff_Pemmican();
//                //this.AllBuffsForReading.Add(pemmican);

//                //Buff_ChemFuel chemfuel = new Buff_ChemFuel();
//                //this.AllBuffsForReading.Add(chemfuel);

//                //Buff_Emanator emanator = new Buff_Emanator();
//                //this.AllBuffsForReading.Add(emanator);

//                //Buff_PsychTea psychTea = new Buff_PsychTea();
//                //this.AllBuffsForReading.Add(psychTea);
//            }
//        }


//        public virtual TechLevel MinTechLevel() => TechLevel.Undefined;
//        public bool active;


//        public void Buff_Pemmican()
//        {
//            ThingDefOf.Pemmican.comps.RemoveAll(x => x is CompProperties_Rottable);
//            this.active = true;
//            Scribe_Values.Look(ref this.active, "MFI_buffedPemmican");
//        }

//        public void Buff_Emanator()
//        {

//            ThoughtDef.Named("PsychicEmanatorSoothe").stages.First().baseMoodEffect = 6f;
//            ThingDefOf.PsychicEmanator.specialDisplayRadius = 20f;
//            CompProperties_Power power = (CompProperties_Power)ThingDefOf.PsychicEmanator.comps.First(x => x is CompProperties_Power);

//            if (power != null)
//                power.basePowerConsumption *= 1.5f;

//            this.active = true;

//            Scribe_Values.Look(ref this.active, "MFI_buffedEmanator");

//        }

//        public void Buff_PsychTea()
//        {

//            IngestionOutcomeDoer_GiveHediff giveHediff = (IngestionOutcomeDoer_GiveHediff)ThingDef.Named("PsychiteTea").ingestible.outcomeDoers.First(x => x is IngestionOutcomeDoer_GiveHediff);
//            if (giveHediff != null)
//                giveHediff.severity = 1f;

//            this.active = true;



//            Scribe_Values.Look(ref this.active, "MFI_buffedPsychTea");

//        }

//        public void Buff_ChemFuel()
//        {
//            CompProperties_Spawner spawner = (CompProperties_Spawner)ThingDefOf.InfiniteChemreactor.comps.First(x => x is CompProperties_Spawner);

//            if (spawner != null)
//                spawner.spawnIntervalRange.min = (int)(spawner.spawnIntervalRange.min * 0.9f);

//            this.active = true;


//            Scribe_Values.Look(ref this.active, "MFI_buffedChemFuel");

//        }

//        public void ExposeData()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
