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
        public double Value { get; set; }
        public DateTime EffectiveFrom { get; set; }

        public bool Equals(NormativeEntry? other)
        {
            if (other is null) return false;

            return Type == other.Type
                && Value.Equals(other.Value)
                && EffectiveFrom == other.EffectiveFrom;
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
