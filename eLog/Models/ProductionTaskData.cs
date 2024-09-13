using libeLog.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    public class ProductionTaskData: ViewModel
    {
        public string PartName { get; set; }

        public string Order { get; set; }

        public string PartsCount { get; set; }

        public string Date { get; set; }

        public string PlantComment { get; set; }

        public string Priority { get; set; }

        public string EngeneersComment { get; set; }

        public string LaborInput { get; set; }

        public string PdComment { get; set;}


        private bool _IsSelected;
        public bool IsSelected
        {
            get => _IsSelected;
            set => Set(ref _IsSelected, value);
        }

        public ProductionTaskData(string partName, string order, string partsCount, string date, string plantComment, string priority, string engeneersComment, string laborInput, string pdComment)
        {
            PartName = partName;
            Order = order;
            PartsCount = partsCount;
            Date = date;
            PlantComment = plantComment;
            Priority = priority;
            EngeneersComment = engeneersComment;
            LaborInput = laborInput;
            PdComment = pdComment;
        }
    }
}
