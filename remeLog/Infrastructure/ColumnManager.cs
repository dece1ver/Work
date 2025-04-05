﻿using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Infrastructure
{
    /// <summary>
    /// Класс для управления колонками в таблице.
    /// </summary>
    public class ColumnManager
    {
        private readonly List<(string Key, string Header)> _columns = new();

        /// <summary>
        /// Добавляет новую колонку в таблицу с заголовком по-умолчанию.
        /// </summary>
        /// <param name="key">Ключ колонки (идентификатор).</param>
        public void Add(string key)
        {
            var description = GetDescription(key);
            _columns.Add((key, description));
        }

        /// <summary>
        /// Добавляет новую колонку в таблицу.
        /// </summary>
        /// <param name="key">Ключ колонки (идентификатор).</param>
        /// <param name="header">Заголовок колонки.</param>
        public void Add(string key, string header)
        {
            _columns.Add((key, header));
        }

        /// <summary>
        /// Возвращает словарь, где ключи — идентификаторы колонок, а значения — индексы (1-based).
        /// </summary>
        /// <returns>Словарь с индексами колонок.</returns>
        public Dictionary<string, int> GetIndexes()
        {
            return _columns.Select((col, index) => new { col.Key, Index = index + 1 })
                          .ToDictionary(c => c.Key, c => c.Index);
        }

        /// <summary>
        /// Возвращает список пар (индекс, заголовок) для построения таблицы.
        /// </summary>
        /// <returns>Список пар (индекс, заголовок).</returns>
        public IEnumerable<(int Index, string Header)> GetIndexedHeaders()
        {
            return _columns.Select((col, index) => (index + 1, col.Header));
        }

        /// <summary>
        /// Возвращает количество колонок.
        /// </summary>
        public int Count => _columns.Count;

        /// <summary>
        /// GUID
        /// </summary>
        public const string Guid = "guid";

        /// <summary>
        /// Станок
        /// </summary>
        public const string Machine = "machine";

        /// <summary>
        /// Станок закрепленный
        /// </summary>
        public const string MachineAssigned = "machineAssigned";

        /// <summary>
        /// Дата
        /// </summary>
        public const string Date = "date";

        /// <summary>
        /// Смена
        /// </summary>
        public const string Shift = "shift";

        /// <summary>
        /// Оператор
        /// </summary>
        public const string Operator = "operator";

        /// <summary>
        /// Разряд
        /// </summary>
        public const string Qualification = "qualification";

        /// <summary>
        /// Деталь
        /// </summary>
        public const string Part = "part";

        // <summary>
        /// Деталь закрепленная
        /// </summary>
        public const string PartAssigned = "partAssigned";

        /// <summary>
        /// М/Л
        /// </summary>
        public const string Order = "order";

        /// <summary>
        /// Всего по М/Л
        /// </summary>
        public const string TotalByOrder = "totalByOrder";

        /// <summary>
        /// Выполнено
        /// </summary>
        public const string Finished = "finished";

        /// <summary>
        /// Установка
        /// </summary>
        public const string Setup = "setup";

        /// <summary>
        /// Количество установок
        /// </summary>
        public const string SetupsCount = "setups";

        /// <summary>
        /// Количество изготовлений
        /// </summary>
        public const string ProductionsCount = "productions";

        /// <summary>
        /// Начало наладки
        /// </summary>
        public const string StartSetupTime = "startSetupTime";

        /// <summary>
        /// Начало изготовления
        /// </summary>
        public const string StartMachiningTime = "startMachiningTime";

        /// <summary>
        /// Конец изготовления
        /// </summary>
        public const string EndMachiningTime = "endMachiningTime";

        /// <summary>
        /// Норматив наладки
        /// </summary>
        public const string SetupTimePlan = "setupTimePlan";

        /// <summary>
        /// Фактическая наладка
        /// </summary>
        public const string SetupTimeFact = "setupTimeFact";

        /// <summary>
        /// Лимит наладки
        /// </summary>
        public const string SetupLimit = "setupLimit";

        /// <summary>
        /// Норматив штучный
        /// </summary>
        public const string SingleProductionTimePlan = "singleProductionTimePlan";

        /// <summary>
        /// Машинное время
        /// </summary>
        public const string MachiningTime = "machiningTime";

        /// <summary>
        /// Штучное фактическое
        /// </summary>
        public const string SingleProductionTime = "singleProductionTime";

        /// <summary>
        /// Время замены
        /// </summary>
        public const string PartReplacementTime = "partReplacementTime";

        /// <summary>
        /// Фактическое изготовление
        /// </summary>
        public const string ProductionTimeFact = "productionTimeFact";

        /// <summary>
        /// Норматив на партию
        /// </summary>
        public const string PlanForBatch = "planForBatch";

        /// <summary>
        /// Комментарий оператора
        /// </summary>
        public const string OperatorComment = "operatorComment";

        /// <summary>
        /// Типовые проблемы
        /// </summary>
        public const string Problems = "problems";

        /// <summary>
        /// Простои в наладке
        /// </summary>
        public const string SetupDowntimes = "setupDowntimes";

        /// <summary>
        /// Простои в изготовлении
        /// </summary>
        public const string MachiningDowntimes = "machiningDowntimes";

        /// <summary>
        /// Частичная наладка
        /// </summary>
        public const string PartialSetupTime = "partialSetupTime";

        /// <summary>
        /// Написание УП
        /// </summary>
        public const string CreateNcProgramTime = "createNcProgramTime";

        /// <summary>
        /// Обслуживание
        /// </summary>
        public const string MaintenanceTime = "maintenanceTime";

        /// <summary>
        /// Поиск инструмента
        /// </summary>
        public const string ToolSearchingTime = "toolSearchingTime";

        /// <summary>
        /// Замена инструмента
        /// </summary>
        public const string ToolChangingTime = "toolChangingTime";

        /// <summary>
        /// Обучение
        /// </summary>
        public const string MentoringTime = "mentoringTime";

        /// <summary>
        /// Другие службы
        /// </summary>
        public const string ContactingDepartmentsTime = "contactingDepartmentsTime";

        /// <summary>
        /// Изготовление оснастки
        /// </summary>
        public const string FixtureMakingTime = "fixtureMakingTime";

        /// <summary>
        /// Отказ оборудования
        /// </summary>
        public const string HardwareFailureTime = "hardwareFailureTime";

        /// <summary>
        /// Отмеченные простои
        /// </summary>
        public const string SpecifiedDowntimesRatio = "specifiedDowntimesRatio";

        /// <summary>
        /// Комментарий к простоям
        /// </summary>
        public const string SpecifiedDowntimesComment = "specifiedDowntimesComment";

        /// <summary>
        /// Наладка или Б/Н
        /// </summary>
        public const string SetupRatioTitle = "setupRatioTitle";

        /// <summary>
        /// Невыполнение норматива наладки
        /// </summary>
        public const string MasterSetupComment = "masterSetupComment";

        /// <summary>
        /// Изготовление или Б/И
        /// </summary>
        public const string ProductionRatioTitle = "productionRatioTitle";

        /// <summary>
        /// Невыполнение норматива изготовления
        /// </summary>
        public const string MasterProductionComment = "masterProductionComment";

        /// <summary>
        /// Комментарий мастера
        /// </summary>
        public const string MasterComment = "masterComment";

        /// <summary>
        /// Норматив наладки (И)
        /// </summary>
        public const string FixedSetupTimePlan = "fixedSetupTimePlan";

        /// <summary>
        /// Норматив изготовления (И)
        /// </summary>
        public const string FixedProductionTimePlan = "fixedProductionTimePlan";

        /// <summary>
        /// Комментарий техотдела
        /// </summary>
        public const string EngineerComment = "engineerComment";

        /// <summary>
        /// Средний размер партии
        /// </summary>
        public const string AveragePartsCount = "averagePartsCount";

        /// <summary>
        /// Среднее количество изготовленных деталей
        /// </summary>
        public const string AverageFinishedCount = "averageFinishedCount";

        /// <summary>
        /// Доля штучных изготовлений
        /// </summary>
        public const string SmallProductionsRatio = "smallProductionsRatio";

        /// <summary>
        /// Доля штучных партий
        /// </summary>
        public const string SmallSeriesRatio = "smallSeriesRatio";

        /// <summary>
        /// Отработано смен
        /// </summary>
        public const string WorkedShifts = "workedShifts";

        /// <summary>
        /// Смены без операторов
        /// </summary>
        public const string NoOperatorShifts = "noOperatorShifts";

        /// <summary>
        /// Смены с ремонтом оборудования
        /// </summary>
        public const string HardwareRepairShifts = "hardwareRepairShifts";

        /// <summary>
        /// Смены без электропитания
        /// </summary>
        public const string NoPowerShifts = "noPowerShifts";

        /// <summary>
        /// Организационные потери
        /// </summary>
        public const string ProcessRelatedLossShifts = "processRelatedLossShifts";

        /// <summary>
        /// Смены без работы по другим причинам
        /// </summary>
        public const string UnspecifiedOtherShifts = "unspecifiedOtherShifts";

        /// <summary>
        /// Коэффициент наладки
        /// </summary>
        public const string SetupRatio = "setupRatio";

        /// <summary>
        /// Коэффициент наладки не исключая простоев
        /// </summary>
        public const string SetupRatioIncludeDowntimes = "setupRatioIncludeDowntimes";

        /// <summary>
        /// Коэффициент изготовления
        /// </summary>
        public const string ProductionRatio = "productionRatio";

        /// <summary>
        /// Коэффициент изготовления не исключая простоев
        /// </summary>
        public const string ProductionRatioIncludeDowntimes = "productionRatioIncludeDowntimes";

        /// <summary>
        /// Общая эффективность
        /// </summary>
        public const string GeneralRatio = "generalRatio";

        /// <summary>
        /// Коэффициент наладки на штучке
        /// </summary>
        public const string SetupRatioUnder = "setupRatioUnder";

        /// <summary>
        /// Коэффициент изготовления на штучке
        /// </summary>
        public const string ProductionRatioUnder = "productionRatioUnder";

        /// <summary>
        /// Коэффициент наладки на серийке
        /// </summary>
        public const string SetupRatioOver = "setupRatioOver";

        /// <summary>
        /// Коэффициент изготовления на серийке
        /// </summary>
        public const string ProductionRatioOver = "productionRatioOver";

        /// <summary>
        /// Соотношение штучки к серийке при наладке
        /// </summary>
        public const string SetupUnderOverRatio = "setupUnderOverRatio";

        /// <summary>
        /// Соотношение штучки к серийке при изготовлении
        /// </summary>
        public const string ProductionUnderOverRatio = "productionUnderOverRatio";

        /// <summary>
        /// Отношение наладки к общему времени
        /// </summary>
        public const string SetupToTotalRatio = "setupToTotalRatio";

        /// <summary>
        /// Отношение изготовления к общему времени
        /// </summary>
        public const string ProductionToTotalRatio = "productionToTotalRatio";

        /// <summary>
        /// Отношение нормативов к общему времени
        /// </summary>
        public const string ProductionEfficiencyToTotalRatio = "productionEfficiencyToTotalRatio";

        /// <summary>
        /// Среднее время замены детали
        /// </summary>
        public const string AverageReplacementTime = "averageReplacementTime";

        /// <summary>
        /// Отмеченные простои
        /// </summary>
        public const string SpecifiedDowntimes = "specifiedDowntimes";

        /// <summary>
        /// Отмеченные простои (для К1)
        /// </summary>
        public const string SpecifiedDowntimesEx = "specifiedDowntimesEx";

        /// <summary>
        /// Неуказанные простои
        /// </summary>
        public const string UnspecifiedDowntimes = "unspecifiedDowntimes";

        /// <summary>
        /// Количество по станку
        /// </summary>
        public const string CountPerMachine = "countPerMachine";

        /// <summary>
        /// Среднее время наладки
        /// </summary>
        public const string AverageSetupTime = "averageSetupTime";

        /// <summary>
        /// Время в наладке
        /// </summary>
        public const string TotalSetupTime = "setupTime";

        /// <summary>
        /// Вермя изготовления
        /// </summary>
        public const string TotalProductionTime = "productionTime";

        /// <summary>
        /// Вермя простоев
        /// </summary>
        public const string TotalDowntimesTime = "downtimesTime";

        /// <summary>
        /// Вермя общее
        /// </summary>
        public const string TotalTime = "totalTime";

        /// <summary>
        /// Совпадает
        /// </summary>
        public const string IsEqual = "isEqual";

        /// <summary>
        /// Коэффициент
        /// </summary>
        public const string Coefficient = "coefficient";

        /// <summary>
        /// Тип
        /// </summary>
        public const string Type = "type";

        /// <summary>
        /// Описание
        /// </summary>
        public const string Description = "description";

        /// <summary>
        /// Успешно ли
        /// </summary>
        public const string IsSuccess = "isSuccess";

        private static readonly Dictionary<string, string> _descriptions = new()
        {
            { Guid, "GUID" },
            { Machine, "Станок" },
            { MachineAssigned, "Станок закрепленный" },
            { Date, "Дата" },
            { Shift, "Смена" },
            { Operator, "Оператор" },
            { Qualification, "Разряд" },
            { Part, "Деталь" },
            { PartAssigned, "Деталь закрепленная" },
            { Order, "М/Л" },
            { TotalByOrder, "Всего по М/Л" },
            { Finished, "Выполнено" },
            { Setup, "Установка" },
            { SetupsCount, $"Количество{Environment.NewLine}наладок" },
            { ProductionsCount, $"Количество{Environment.NewLine}изготовлений" },
            { StartSetupTime, "Начало наладки" },
            { StartMachiningTime, $"Начало{Environment.NewLine}изготовления" },
            { EndMachiningTime, $"Конец{Environment.NewLine}изготовления" },
            { SetupTimePlan, $"Норматив{Environment.NewLine}наладки" },
            { SetupLimit, $"Лимит{Environment.NewLine}наладки" },
            { SetupTimeFact, $"Фактическая{Environment.NewLine}наладка" },
            { SingleProductionTimePlan, $"Норматив{Environment.NewLine}штучный" },
            { MachiningTime, "Машинное время" },
            { SingleProductionTime, $"Штучное{Environment.NewLine}фактическое" },
            { PartReplacementTime, "Время замены" },
            { ProductionTimeFact, $"Фактическое{Environment.NewLine}изготовление" },
            { PlanForBatch, $"Норматив{Environment.NewLine}на партию" },
            { OperatorComment, $"Комментарий{Environment.NewLine}оператора" },
            { Problems, $"Типовые{Environment.NewLine}проблемы" },
            { SetupDowntimes, $"Простои{Environment.NewLine}в наладке" },
            { MachiningDowntimes, $"Простои{Environment.NewLine}в изготовлении" },
            { PartialSetupTime, "Частичная наладка" },
            { CreateNcProgramTime, "Написание УП" },
            { MaintenanceTime, "Обслуживание" },
            { ToolSearchingTime, "Поиск инструмента" },
            { ToolChangingTime, "Замена инструмента" },
            { MentoringTime, "Обучение" },
            { ContactingDepartmentsTime, "Другие службы" },
            { FixtureMakingTime, "Изготовление оснастки" },
            { HardwareFailureTime, "Отказ оборудования" },
            { SpecifiedDowntimesRatio, "Отмеченные простои" },
            { SpecifiedDowntimesComment, $"Комментарий{Environment.NewLine}к простоям" },
            { SetupRatioTitle, "Наладка" },
            { MasterSetupComment, $"Отклонение от{Environment.NewLine}норматива наладки" },
            { ProductionRatioTitle, "Изготовление" },
            { MasterProductionComment, $"Отклонение от{Environment.NewLine}норматива изготовления" },
            { MasterComment, "Комментарий мастера" },
            { FixedSetupTimePlan, "Норматив наладки (И)" },
            { FixedProductionTimePlan, "Норматив изготовления (И)" },
            { EngineerComment, "Комментарий техотдела" },
            { AveragePartsCount, "Средняя партия" },
            { AverageFinishedCount, "Среднее изготовление" },
            { SmallProductionsRatio, $"Доля штучных{Environment.NewLine}изготовлений" },
            { SmallSeriesRatio, $"Доля штучных{Environment.NewLine}партий" },
            { WorkedShifts, "Отработанные смены" },
            { NoOperatorShifts, "Смены без операторов" },
            { HardwareRepairShifts, $"Смены с ремонтом{Environment.NewLine}оборудования" },
            { NoPowerShifts, "Смены без электропитания" },
            { ProcessRelatedLossShifts, "Организационные потери" },
            { UnspecifiedOtherShifts, $"Смены без работы{Environment.NewLine}по другим причинам" },
            { GeneralRatio, $"Эффективность" },
            { SetupRatio, $"Выполнение норматива{Environment.NewLine}наладки" },
            { SetupRatioIncludeDowntimes, $"Выполнение норматива{Environment.NewLine}наладки{Environment.NewLine}(с простоями)" },
            { ProductionRatio, $"Выполнение норматива{Environment.NewLine}изготовления" },
            { ProductionRatioIncludeDowntimes, $"Выполнение норматива{Environment.NewLine}изготовления{Environment.NewLine}(с простоями)" },
            { SetupRatioUnder, $"Выполнение норматива{Environment.NewLine}наладки на штучке" },
            { ProductionRatioUnder, $"Выполнение норматива{Environment.NewLine}изготовления на штучке" },
            { SetupRatioOver, $"Выполнение норматива{Environment.NewLine}наладки на серийке" },
            { ProductionRatioOver, $"Выполнение норматива{Environment.NewLine}изготовления на серийке" },
            { SetupUnderOverRatio, $"Соотношение штучки{Environment.NewLine}к серийке при наладке" },
            { ProductionUnderOverRatio, $"Соотношение штучки{Environment.NewLine}к серийке при изготовлении" },
            { SetupToTotalRatio, $"Доля наладок" },
            { ProductionToTotalRatio, $"Доля изготовлений" },
            { ProductionEfficiencyToTotalRatio, $"Отношение нормативов{Environment.NewLine}к общему времени" },
            { AverageReplacementTime, $"Среднее время{Environment.NewLine}замены детали" },
            { SpecifiedDowntimes, "Отмеченные простои" },
            { SpecifiedDowntimesEx, $"Отмеченные простои{Environment.NewLine}(для К1)" },
            { UnspecifiedDowntimes, "Неуказанные простои" },
            { CountPerMachine, $"Количество{Environment.NewLine}по станку" },
            { AverageSetupTime, $"Среднее{Environment.NewLine}время наладки" },
            { TotalSetupTime, $"Время наладки" },
            { TotalProductionTime, $"Время изготовления" },
            { TotalDowntimesTime, $"Время простоев" },
            { TotalTime, $"Отмеченное общее время" },
            { IsEqual, $"Совпадает" },
            { Coefficient, $"Коэффициент" },
            { Type, $"Тип" },
            { Description, $"Описание" },
            { IsSuccess, $"Успешно ли" }
        };

        /// <summary>
        /// Возвращает описание для ключа.
        /// </summary>
        /// <param name="key">Ключ колонки.</param>
        /// <returns>Описание или "Н/Д", если описание не найдено.</returns>
        static string GetDescription(string key)
        {
            return _descriptions.TryGetValue(key, out var description) ? description : "Н/Д";
        }


        /// <summary>
        /// Билдер для создания экземпляров ColumnManager.
        /// </summary>
        public class Builder
        {
            private readonly List<(string Key, string Header)> _columns = new();

            public Builder Add(string key)
            {
                var description = GetDescription(key);
                _columns.Add((key, description));
                return this;
            }

            public Builder Add(string key, string header)
            {
                _columns.Add((key, header));
                return this;
            }

            public Builder AddRange(IEnumerable<string> keys)
            {
                foreach (var key in keys)
                {
                    Add(key);
                }
                return this;
            }

            public Builder AddRange(IEnumerable<(string Key, string Header)> columns)
            {
                foreach (var column in columns)
                {
                    Add(column.Key, column.Header);
                }
                return this;
            }

            public ColumnManager Build()
            {
                var manager = new ColumnManager();
                foreach (var column in _columns)
                {
                    manager.Add(column.Key, column.Header);
                }
                return manager;
            }
        }
    }
}
