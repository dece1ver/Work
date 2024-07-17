using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    public class ProductionTasksData
    {
        public string Machine { get; set; }

        public List<ProductionTaskData> ProductionTasks { get; set; }

        public ProductionTasksData(string machine)
        {
            Machine = machine;
            ProductionTasks = new List<ProductionTaskData>();
        }
    }
}
