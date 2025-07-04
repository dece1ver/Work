using System.Collections.Generic;
using libeLog.Base;

namespace libeLog.Models
{
    public class SerialPart : ViewModel
    {
        private readonly string _originalPartName;
        private readonly int _originalYearCount;
        public SerialPart(int id, string partName, int yearCount)
        {
            Id = id;
            _PartName = partName;
            _YearCount = yearCount;
            _originalPartName = _PartName;
            _originalYearCount = _YearCount;
        }

        public SerialPart(string partName, int yearCount)
        {
            Id = 0;
            _PartName = partName;
            _YearCount = yearCount;
            _originalPartName = _PartName;
            _originalYearCount = _YearCount;
        }

        public SerialPart()
        {
            Id = 0;
            _PartName = "";
            _YearCount = 0;
            _originalPartName = _PartName;
            _originalYearCount = 0;
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

        private int _YearCount;
        public int YearCount
        {
            get => _YearCount;
            set
            {
                if (Set(ref _YearCount, value))
                {
                    IsModified = !IsOriginalState();
                }
            }
        }

        private bool IsOriginalState()
        {
            return PartName == _originalPartName && YearCount == _originalYearCount;
        }
    }
}
