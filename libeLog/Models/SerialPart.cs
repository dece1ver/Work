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
    public class SerialPart : ViewModel, IEquatable<SerialPart>, IDataErrorInfo
    {
        private SerialPart? _originalPart;

        public SerialPart(int id, string partName, ObservableCollection<CncOperation> operations, int yearCount)
        {
            Id = id;
            PartName = partName;
            YearCount = yearCount;
            Operations = operations;
        }

        public SerialPart()
        {
        }

        private SerialPart(int id, string partName, ObservableCollection<CncOperation> operations, int yearCount, bool _forClone)
        {
            Id = id;
            PartName = partName;
            YearCount = yearCount;
            Operations = operations;

        }

        private ObservableCollection<CncOperation> _Operations = new();
        public ObservableCollection<CncOperation> Operations
        {
            get => _Operations;
            set
            {
                if (Set(ref _Operations, value))
                {
                    UnsubscribeFromOperations(_Operations);
                    SubscribeToOperations(_Operations);

                    foreach (var operation in _Operations)
                        foreach (var setup in operation.Setups)
                            setup.UpdateDependentProperties();

                    OnPropertyChanged(nameof(IsModified));
                }
            }
        }

        /// <summary> В оригинальном ли состоянии деталь </summary>
        public bool IsModified => !IsOriginalState();


        private int _Id;
        /// <summary> ID </summary>
        public int Id
        {
            get => _Id;
            set => Set(ref _Id, value);
        }

        private string _PartName = string.Empty;
        /// <summary> Имя детали </summary>
        public string PartName
        {
            get => _PartName;
            set
            {
                if (Set(ref _PartName, value))
                    OnPropertyChanged(nameof(IsModified));
            }
        }

        private int _YearCount;
        /// <summary> Годовая потребность </summary>
        public int YearCount
        {
            get => _YearCount;
            set
            {
                if (Set(ref _YearCount, value))
                    OnPropertyChanged(nameof(IsModified));
            }
        }

        public string Error => null!;

        public string this[string propertyName]
        {
            get
            {
                return propertyName switch
                {
                    nameof(PartName) => ValidatePartName(),
                    nameof(YearCount) => ValidateYearCount(),
                    _ => string.Empty
                };
            }
        }

        private string ValidatePartName()
        {
            if (string.IsNullOrWhiteSpace(PartName))
                return "Имя не может быть пустым";

            return string.Empty;
        }

        private string ValidateYearCount()
        {
            if (YearCount < 0)
                return "Годовой план не может быть отрицательным";
            return string.Empty;
        }

        private void SubscribeToNestedChanges()
        {
            SubscribeToOperations(Operations);
            Operations.CollectionChanged += Operations_CollectionChanged;
        }

        private void Operations_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (CncOperation op in e.NewItems)
                    SubscribeToOperation(op);
            if (e.OldItems != null)
                foreach (CncOperation op in e.OldItems)
                    UnsubscribeFromOperation(op);

            OnPropertyChanged(nameof(IsModified));
        }

        private void SubscribeToOperations(IEnumerable<CncOperation> ops)
        {
            foreach (var op in ops)
                SubscribeToOperation(op);
        }

        private void UnsubscribeFromOperations(IEnumerable<CncOperation> ops)
        {
            foreach (var op in ops)
                UnsubscribeFromOperation(op);
        }

        private void SubscribeToOperation(CncOperation op)
        {
            op.PropertyChanged += NestedPropertyChanged; 
            op.Setups.CollectionChanged += Setups_CollectionChanged;
            foreach (var setup in op.Setups)
                SubscribeToSetup(setup);
        }

        private void UnsubscribeFromOperation(CncOperation op)
        {
            op.PropertyChanged -= NestedPropertyChanged;
            op.Setups.CollectionChanged -= Setups_CollectionChanged;
            foreach (var setup in op.Setups)
                UnsubscribeFromSetup(setup);
        }

        private void Setups_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (CncSetup setup in e.NewItems)
                    SubscribeToSetup(setup);
            if (e.OldItems != null)
                foreach (CncSetup setup in e.OldItems)
                    UnsubscribeFromSetup(setup);

            OnPropertyChanged(nameof(IsModified));
        }

        private void SubscribeToSetup(CncSetup setup)
        {
            setup.PropertyChanged += NestedPropertyChanged;
            setup.Normatives.CollectionChanged += Normatives_CollectionChanged;
            foreach (var norm in setup.Normatives)
                norm.PropertyChanged += NestedPropertyChanged;
        }

        private void UnsubscribeFromSetup(CncSetup setup)
        {
            setup.PropertyChanged -= NestedPropertyChanged;
            setup.Normatives.CollectionChanged -= Normatives_CollectionChanged;
            foreach (var norm in setup.Normatives)
                norm.PropertyChanged -= NestedPropertyChanged;
        }

        private void Normatives_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (NormativeEntry n in e.NewItems)
                    n.PropertyChanged += NestedPropertyChanged;
            if (e.OldItems != null)
                foreach (NormativeEntry n in e.OldItems)
                    n.PropertyChanged -= NestedPropertyChanged;

            OnPropertyChanged(nameof(IsModified));
        }

        private void NestedPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"SerialPart.NestedPropertyChanged: {e.PropertyName} от {sender?.GetType().Name}");
            OnPropertyChanged(nameof(IsModified));
        }

        /// <summary>
        /// Помечает текущее состояние «оригинальным» (сбрасывает флаг IsModified).
        /// </summary>
        public void AcceptChanges()
        {
            SubscribeToNestedChanges();
            _originalPart = this.CloneInternal();
            OnPropertyChanged(nameof(IsModified));
        }

        private bool IsOriginalState()
        {
            return _originalPart != null && Equals(_originalPart);
        }

        // Новый приватный метод для клонирования без рекурсии
        private SerialPart CloneInternal()
        {
            return new SerialPart(
                Id,
                PartName,
                Operations.Select(op => op.Clone())
                          .ToObservableCollection(),
                YearCount,
                _forClone: true
            );
        }

        public SerialPart Clone() => CloneInternal();

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
