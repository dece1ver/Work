using DocumentFormat.OpenXml.Wordprocessing;
using libeLog.Base;
using System;
using System.ComponentModel;

namespace remeLog.Models
{
    public class OperatorInfo : ViewModel, IDataErrorInfo
    {
        private string _originalName;
        private int _originalQualification;
        private bool _originalIsActive;

        public OperatorInfo(int id, string name, int qualification, bool isActive)
        {
            Id = id;
            _Name = name;
            _Qualification = qualification;
            IsActive = isActive;
            _originalName = _Name;
            _originalQualification = _Qualification;
            _originalIsActive = IsActive;
            IsModified = !IsOriginalState();
        }

        public OperatorInfo()
        {
            Id = -1;
            _Name = "";
            _Qualification = 0;
            IsActive = false;
            _originalName = _Name;
            _originalQualification = _Qualification;
            _originalIsActive = IsActive;
        }

        private bool _IsModified;
        public bool IsModified
        {
            get => _IsModified;
            set => Set(ref _IsModified, value);
        }

        public int Id { get; set; }

        private string _Name;
        /// <summary> ФИО оператора </summary>
        public string Name
        {
            get => _Name;
            set
            {
                if (Set(ref _Name, value))
                {
                    IsModified = !IsOriginalState();
                }
            }
        }

        private int _Qualification;
        /// <summary> Разряд </summary>
        public int Qualification
        {
            get => _Qualification;
            set
            {
                if (Set(ref _Qualification, value))
                {
                    IsModified = !IsOriginalState();
                }
            }
        }

        private bool _IsActive;
        public bool IsActive
        {
            get => _IsActive;
            set
            {
                if (Set(ref _IsActive, value))
                {
                    IsModified = !IsOriginalState();
                }
            }
        }

        public string this[string columnName]
        {
            get
            {
                string error = null!;
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                            error = "Имя оператора не может быть пустым.";
                        break;
                    case nameof(Qualification):
                        if (Qualification < 0 || Qualification > 6)
                            error = "Разряд должен быть от 0 до 6.";
                        break;
                }
                return error;
            }
        }

        public string Error => null!;

        private bool IsOriginalState()
        {
            return Name == _originalName && Qualification == _originalQualification && IsActive == _originalIsActive;
        }
    }
}
