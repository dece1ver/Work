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
        public ToolSearchCase(int id, Guid partGuid, string toolType, string value, DateTime? startTime, DateTime? endTime)
        {
            Id = id;
            PartGuid = partGuid;
            ToolType = toolType;
            Value = value;
            StartTime = startTime;
            EndTime = endTime;
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
        /// Создаёт экземпляр ToolSearchCase из DataRow.
        /// </summary>
        /// <param name="row">Строка данных DataRow.</param>
        /// <returns>Объект ToolSearchCase.</returns>
        public static ToolSearchCase FromDataRow(DataRow row)
        {
            return new ToolSearchCase(
                Convert.ToInt32(row["Id"]),
                Guid.Parse(row["PartGuid"].ToString() ?? "Н/Д"),
                row["ToolType"].ToString() ?? "Н/Д",
                row["Value"].ToString() ?? "Н/Д",
                row["StartTime"] == DBNull.Value ? null : Convert.ToDateTime(row["StartTime"]),
                row["EndTime"] == DBNull.Value ? null : Convert.ToDateTime(row["EndTime"])
            );
        }
    }
}
