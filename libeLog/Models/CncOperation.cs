using libeLog.Base;
using libeLog.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace libeLog.Models
{
    /// <summary>
    /// Операция на станке ЧПУ
    /// </summary>
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

        private int _OrderIndex;
        /// <summary> Порядок отображения операции внутри детали </summary>
        public int OrderIndex
        {
            get => _OrderIndex;
            set => Set(ref _OrderIndex, value);
        }

        private ObservableCollection<CncSetup> _Setups = new();
        /// <summary> Список установок, входящих в операцию </summary>
        public ObservableCollection<CncSetup> Setups
        {
            get => _Setups;
            set
            {
                if (Set(ref _Setups, value))
                {
                    UnsubscribeFromSetups(_Setups);
                    SubscribeToSetups(_Setups);
                }
            }
        }

        public CncOperation(int id, string name, int orderIndex, IEnumerable<CncSetup> setups)
        {
            Id = id;
            Name = name;
            OrderIndex = orderIndex;
            _Setups = setups.ToObservableCollection();
            SubscribeToNestedChanges();
        }

        public CncOperation(string name)
        {
            Name = name;
            _Setups = new ObservableCollection<CncSetup>();
            SubscribeToNestedChanges();
        }

        public CncOperation()
        {
            _Setups = new ObservableCollection<CncSetup>();
            SubscribeToNestedChanges();
        }

        private void SubscribeToNestedChanges()
        {
            SubscribeToSetups(Setups);
            Setups.CollectionChanged += Setups_CollectionChanged;
        }

        private void UnsubscribeFromNestedChanges()
        {
            UnsubscribeFromSetups(Setups);
            Setups.CollectionChanged -= Setups_CollectionChanged;
        }

        private void Setups_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (CncSetup setup in e.NewItems)
                    SubscribeToSetup(setup);

            if (e.OldItems != null)
                foreach (CncSetup setup in e.OldItems)
                    UnsubscribeFromSetup(setup);
        }

        private void SubscribeToSetups(IEnumerable<CncSetup> setups)
        {
            foreach (var setup in setups)
                SubscribeToSetup(setup);
        }

        private void UnsubscribeFromSetups(IEnumerable<CncSetup> setups)
        {
            foreach (var setup in setups)
                UnsubscribeFromSetup(setup);
        }

        private void SubscribeToSetup(CncSetup setup)
        {
            setup.PropertyChanged += NestedPropertyChanged;
            setup.Normatives.CollectionChanged += Normatives_CollectionChanged;
        }

        private void UnsubscribeFromSetup(CncSetup setup)
        {
            setup.PropertyChanged -= NestedPropertyChanged;
            setup.Normatives.CollectionChanged -= Normatives_CollectionChanged;
        }

        private void Normatives_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Просто пробрасываем событие изменения коллекции вверх
            // (поскольку NormativeEntry внутри не меняется)
        }

        private void NestedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"CncOperation.NestedPropertyChanged: {e.PropertyName} от {sender?.GetType().Name}");
            // Пробрасываем уведомление вверх по иерархии
            OnPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// Создает глубокую копию операции
        /// </summary>
        public CncOperation Clone()
        {
            return new CncOperation(
                Id,
                Name,
                OrderIndex,
                Setups.Select(s => s.Clone())
            );
        }

        public bool Equals(CncOperation? other)
        {
            if (other is null) return false;
            return Id == other.Id
                && Name == other.Name
                && OrderIndex == other.OrderIndex
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
            return $"{Name} — установок: {Setups.Count}";
        }
    }
}
