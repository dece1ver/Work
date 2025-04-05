using System;
using System.Data;

namespace eLog.Models
{
    /// <summary>
    /// Представляет запись о поиске инструмента.
    /// </summary>
    public class ToolSearchCase
    {
        /// <summary>
        /// Конструктор для создания экземпляра вручную.
        /// </summary>
        public ToolSearchCase(int id, Guid partGuid, string toolType, string value, DateTime? startTime, DateTime? endTime, bool? isSuccess)
        {
            Id = id;
            PartGuid = partGuid;
            ToolType = toolType;
            Value = value;
            StartTime = startTime;
            EndTime = endTime;
            IsSuccess = isSuccess;
        }

        /// <summary>
        /// Уникальный идентификатор записи.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Уникальный идентификатор детали.
        /// </summary>
        public Guid PartGuid { get; private set; }

        /// <summary>
        /// Тип инструмента.
        /// </summary>
        public string ToolType { get; private set; }

        /// <summary>
        /// Значение параметра инструмента.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Время начала поиска.
        /// </summary>
        public DateTime? StartTime { get; private set; }

        /// <summary>
        /// Время окончания поиска.
        /// </summary>
        public DateTime? EndTime { get; private set; }

        /// <summary>
        /// Нашел ли
        /// </summary>
        public bool? IsSuccess { get; set; }

        /// <summary>
        /// Создаёт экземпляр ToolSearchCase из DataRow.
        /// </summary>
        /// <param name="row">Строка данных DataRow.</param>
        /// <returns>Объект ToolSearchCase.</returns>
        public static ToolSearchCase FromDataRow(DataRow row)
        {
            int id = row.Field<int>("Id");
            Guid partGuid = row.IsNull("PartGuid") ? Guid.Empty : row.Field<Guid>("PartGuid");
            string toolType = row.Field<string?>("ToolType") ?? "Н/Д";
            string value = row.Field<string?>("Value") ?? "Н/Д";
            DateTime? startTime = row.Field<DateTime?>("StartTime");
            DateTime? endTime = row.Field<DateTime?>("EndTime");
            bool? isSuccess = row.Field<bool?>("IsSuccess");

            return new ToolSearchCase(id, partGuid, toolType, value, startTime, endTime, isSuccess);
        }
    }
}
