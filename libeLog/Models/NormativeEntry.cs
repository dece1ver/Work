using libeLog.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libeLog.Models
{
    public class NormativeEntry : ViewModel, IEquatable<NormativeEntry>
    {
        public enum NormativeType : byte
        {
            Setup = 0,
            Production = 1
        }

        private int _Id;
        /// <summary> ID </summary>
        public int Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        public NormativeType Type { get; set; }

        public string NormativeTypeDisplay => Type switch
        {
            NormativeType.Setup => "Наладка",
            NormativeType.Production => "Изготовление",
            _ => "Неизвестно"
        };

        private double _Value;
        /// <summary> Описание </summary>
        public double Value
        {
            get => _Value;
            set => Set(ref _Value, value);
        }


        private DateTime _EffectiveFrom;
        /// <summary> Описание </summary>
        public DateTime EffectiveFrom
        {
            get => _EffectiveFrom;
            set => Set(ref _EffectiveFrom, value);
        }


        private bool _IsApproved;
        /// <summary> Утверждённый </summary>
        public bool IsApproved
        {
            get => _IsApproved;
            set => Set(ref _IsApproved, value);
        }


        public bool Equals(NormativeEntry? other)
        {
            if (other is null) return false;

            return Id == other.Id
                && Type == other.Type
                && Value.Equals(other.Value)
                && EffectiveFrom == other.EffectiveFrom;
        }

        public NormativeEntry Clone()
        {
            return new NormativeEntry
            {
                Id = this.Id,
                Type = this.Type,
                Value = this.Value,
                EffectiveFrom = this.EffectiveFrom
            };
        }

        public override bool Equals(object? obj) => Equals(obj as NormativeEntry);

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value, EffectiveFrom);
        }

        public override string ToString()
        {
            return $"{Value} @ {EffectiveFrom:yy-MM-dd HH:mm} ({Type})";
        }
    }
}
