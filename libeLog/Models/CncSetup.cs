using libeLog.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static libeLog.Models.NormativeEntry;

namespace libeLog.Models
{
    /// <summary>
    /// Установка в рамках операции
    /// </summary>
    public class CncSetup : ViewModel, IEquatable<CncSetup>
    {
        private int _Id;
        /// <summary> Уникальный ID установки </summary>
        public int Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        private byte _Number;
        /// <summary> Порядковый номер установки </summary>
        public byte Number
        {
            get => _Number;
            set => Set(ref _Number, value);
        }

        private ObservableCollection<NormativeEntry> _Normatives = new();
        /// <summary> Нормативы, относящиеся к установке </summary>
        public ObservableCollection<NormativeEntry> Normatives
        {
            get => _Normatives;
            set => Set(ref _Normatives, value);
        }

        /// <summary> Нормативы наладки </summary>
        public IEnumerable<NormativeEntry> SetupNormatives =>
            Normatives.Where(n => n.Type == NormativeEntry.NormativeType.Setup);

        /// <summary> Нормативы изготовления </summary>
        public IEnumerable<NormativeEntry> ProductionNormatives =>
            Normatives.Where(n => n.Type == NormativeEntry.NormativeType.Production);

        /// <summary>
        /// История нормативов
        /// </summary>
        public IEnumerable<NormativeEntry> NormativesHistory => Normatives.OrderByDescending(n => n.EffectiveFrom);

        /// <summary>
        /// История нормативов наладки
        /// </summary>
        public IEnumerable<NormativeEntry> SetupHistory => SetupNormatives.OrderByDescending(n => n.EffectiveFrom);
        /// <summary>
        /// История нормативов изготовления
        /// </summary>
        public IEnumerable<NormativeEntry> ProductionHistory => ProductionNormatives.OrderByDescending(n => n.EffectiveFrom);

        /// <summary> Последний норматив наладки </summary>
        public NormativeEntry? LastSetupNormative =>
            SetupNormatives.OrderByDescending(n => n.EffectiveFrom).FirstOrDefault();

        /// <summary> Последний норматив изготовления </summary>
        public NormativeEntry? LastProductionNormative =>
            ProductionNormatives.OrderByDescending(n => n.EffectiveFrom).FirstOrDefault();

        /// <summary>
        /// Создает глубокую копию установки
        /// </summary>
        public CncSetup Clone()
        {
            return new CncSetup
            {
                Id = this.Id,
                Number = this.Number,
                Normatives = new ObservableCollection<NormativeEntry>(
                    this.Normatives.Select(n => new NormativeEntry
                    {
                        Type = n.Type,
                        Value = n.Value,
                        EffectiveFrom = n.EffectiveFrom
                    })
                )
            };
        }

        public bool Equals(CncSetup? other)
        {
            if (other is null) return false;

            return Id == other.Id
                && Number == other.Number
                && Normatives.SequenceEqual(other.Normatives);
        }

        public override bool Equals(object? obj) => Equals(obj as CncSetup);

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id,
                Number,
                Normatives.Aggregate(0, (acc, n) => acc ^ n.GetHashCode())
            );
        }

        public override string ToString()
        {
            return $"Установка #{Number} (ID {Id}) — {Normatives.Count} нормативов";
        }
    }

}
