using libeLog.Base;
using libeLog.Extensions;
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
    /// Операция на станке ЧПУ
    /// </summary>
    public class CncOperation : ViewModel, IEquatable<CncOperation>
    {
        private int _Id;
        /// <summary> Уникальный идентификатор операции </summary>
        public int Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        private string _Name = string.Empty;
        /// <summary> Название операции </summary>
        public string Name
        {
            get => _Name;
            set => Set(ref _Name, value);
        }

        private ObservableCollection<CncSetup> _Setups = new();
        /// <summary> Список установок, входящих в операцию </summary>
        public ObservableCollection<CncSetup> Setups
        {
            get => _Setups;
            set => Set(ref _Setups, value);
        }

        public CncOperation(int id, string name, IEnumerable<CncSetup> setups)
        {
            Id = id;
            Name = name;
            _Setups = setups.ToObservableCollection();
        }

        public CncOperation(string name) 
        {
            Name = name;
        }

        /// <summary>
        /// Создает глубокую копию операции
        /// </summary>
        public CncOperation Clone()
        {
            return new CncOperation(
                Id,
                Name,
                Setups.Select(s => s.Clone())
            );
        }

        public bool Equals(CncOperation? other)
        {
            if (other is null) return false;

            return Id == other.Id
                && Name == other.Name
                && Setups.SequenceEqual(other.Setups);
        }

        public override bool Equals(object? obj) => Equals(obj as CncOperation);

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Id,
                Name,
                Setups.Aggregate(0, (acc, s) => acc ^ s.GetHashCode())
            );
        }

        public override string ToString()
        {
            return $"{Name} (ID {Id}) — {Setups.Count} установок";
        }
    }

}
