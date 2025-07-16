using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using libeLog.Base;
using libeLog.Extensions;

namespace libeLog.Models
{
    public class SerialPart : ViewModel, IEquatable<SerialPart>
    {
        //private readonly SerialPart _originalPart;

        public SerialPart(int id, string partName, int yearCount, ObservableCollection<CncOperation> operations)
        {
            Id = id;
            PartName = partName;
            YearCount = yearCount;
            Operations = operations;
        }

        public SerialPart()
        {
        }

        private ObservableCollection<CncOperation> _Operations = new();
        /// <summary> Операции </summary>
        public ObservableCollection<CncOperation> Operations
        {
            get => _Operations;
            set => Set(ref _Operations, value);
        }

        private bool _IsModified;
        public bool IsModified
        {
            get => _IsModified;
            set => Set(ref _IsModified, value);
        }

        public int Id { get; set; }

        private string _PartName = string.Empty;
        public string PartName
        {
            get => _PartName;
            set
            {
                if (Set(ref _PartName, value))
                    IsModified = !IsOriginalState();
            }
        }

        private int _YearCount;
        public int YearCount
        {
            get => _YearCount;
            set
            {
                if (Set(ref _YearCount, value))
                    IsModified = !IsOriginalState();
            }
        }

        private SerialPart Clone()
        {
            return new SerialPart(
                Id,
                PartName,
                YearCount,
                Operations.Select(op => op.Clone()).ToObservableCollection()
            );
        }

        private bool IsOriginalState()
        {
            return true;
            //return Equals(_originalPart);
        }

        public bool Equals(SerialPart? other)
        {
            if (other is null) return false;

            return PartName == other.PartName
                && YearCount == other.YearCount
                && Operations.SequenceEqual(other.Operations);
        }

        public override bool Equals(object? obj) => Equals(obj as SerialPart);

        public override int GetHashCode()
        {
            return HashCode.Combine(
                PartName,
                YearCount,
                Operations.Aggregate(0, (acc, op) => acc ^ op.GetHashCode())
            );
        }

        public override string ToString()
        {
            return $"{PartName} ({YearCount} шт.) — {Operations.Count} операций";
        }
    }
}
