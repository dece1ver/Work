using DocumentFormat.OpenXml.Wordprocessing;
using libeLog.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class SerialPart : ViewModel
    {
        private readonly string _originalPartName;
        public SerialPart(int id, string partName)
        {
            Id = id;
            _PartName = partName;
            _originalPartName = _PartName;
        }

        public SerialPart(string partName)
        {
            Id = 0;
            _PartName = partName;
            _originalPartName = _PartName;
        }

        public SerialPart()
        {
            Id = 0;
            _PartName = "";
            _originalPartName = _PartName;
        }

        private bool _IsModified;
        public bool IsModified
        {
            get => _IsModified;
            set => Set(ref _IsModified, value);
        }

        public int Id { get; init; }

        private string _PartName;
        public string PartName
        {
            get => _PartName;
            set
            {
                if (Set(ref _PartName, value))
                {
                    IsModified = !IsOriginalState();
                }
            }
        }

        private bool IsOriginalState()
        {
            return PartName == _originalPartName;
        }
    }
}
