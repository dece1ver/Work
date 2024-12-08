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
        public int Length => _columns.Count;

        /// <summary>
        /// GUID
        /// </summary>
        public const string Guid = "guid";

        /// <summary>
        /// Станок
        /// </summary>
        public const string Machine = "machine";

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
        /// Деталь
        /// </summary>
        public const string Part = "part";

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
        /// Наладка
        /// </summary>
        public const string SetupRatioTitle = "setupRatioTitle";

        /// <summary>
        /// Невыполнение норматива наладки
        /// </summary>
        public const string MasterSetupComment = "masterSetupComment";

        /// <summary>
        /// Изготовление
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

        private static readonly Dictionary<string, string> _descriptions = new()
        {
            { Guid, "GUID" },
            { Machine, "Станок" },
            { Date, "Дата" },
            { Shift, "Смена" },
            { Operator, "Оператор" },
            { Part, "Деталь" },
            { Order, "М/Л" },
            { TotalByOrder, "Всего по М/Л" },
            { Finished, "Выполнено" },
            { Setup, "Установка" },
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
            { SetupDowntimes, $"Простои{Environment.NewLine}в наладке" },
            { MachiningDowntimes, $"Простои{Environment.NewLine}в изготовлении" },
            { PartialSetupTime, "Частичная наладка" },
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
            { EngineerComment, "Комментарий техотдела" }
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
    }
}
