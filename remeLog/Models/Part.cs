using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class Part
    {
        public int Id { get; set; }
        public string SetupProductivity { get; set; }
        public string MachiningProductivity { get; set; }
        public string OperatorComment { get; set; }
        public DateTime Date { get; set; }
        public string Machine { get; set; }
        public string Shift { get; set; }
        public string Operator { get; set; }
        public string Name { get; set; }
        public string Order { get; set; }
        public int FinishedCount { get; set; }
        public int Setup { get; set; }
        public DateTime StartSetup { get; set; }
        public DateTime StartMachining { get; set; }
        public DateTime EndMachining { get; set; }
        public double SetupFact { get; set; }
        public double SetupPlan { get; set; }
        public double ProductionFact { get; set; }
        public double ProductionPlan { get; set; }
        public double SetupDowntimes { get; set; }
        public double MachiningDowntimes { get; set; }


        public Part(
            int id, 
            string setupProductivity, 
            string machiningProductivity, 
            string operatorComment, 
            DateTime date, 
            string machine, 
            string shift, 
            string @operator, 
            string name, 
            string order, 
            int finishedCount, 
            int setup, 
            DateTime startSetup, 
            DateTime startMachining, 
            DateTime endMachining, 
            double setupFact, 
            double setupPlan, 
            double productionFact, 
            double productionPlan, 
            double setupDowntimes, 
            double machiningDowntimes)
        {
            Id = id;
            SetupProductivity = setupProductivity;
            MachiningProductivity = machiningProductivity;
            OperatorComment = operatorComment;
            Date = date;
            Machine = machine;
            Shift = shift;
            Operator = @operator;
            Name = name;
            Order = order;
            FinishedCount = finishedCount;
            Setup = setup;
            StartSetup = startSetup;
            StartMachining = startMachining;
            EndMachining = endMachining;
            SetupFact = setupFact;
            SetupPlan = setupPlan;
            ProductionFact = productionFact;
            ProductionPlan = productionPlan;
            SetupDowntimes = setupDowntimes;
            MachiningDowntimes = machiningDowntimes;
        }
    }
    
    
}
