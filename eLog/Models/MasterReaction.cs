using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eLog.Models
{
    public class MasterReaction
    {
        /// <summary>
        /// Инициализирует новый экземпляр реакции мастера.
        /// </summary>
        /// <param name="master">Имя мастера, к которому относится реакция.</param>
        public MasterReaction(ReactionType reactionType, string master)
        {
            Type = reactionType;
            Master = master;
            StartTime = DateTime.Now;
            Comment = string.Empty;
        }

        public MasterReaction(MasterReaction masterReaction)
        {
            Type = masterReaction.Type;
            Master = masterReaction.Master;
            StartTime = masterReaction.StartTime;
            EndDate = masterReaction.EndDate;
            Comment = masterReaction.Comment;
        }

        /// <summary>
        /// Имя мастера, к которому относится реакция.
        /// </summary>
        public string Master { get; set; }

        /// <summary>
        /// Время начала реакции.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Время окончания реакции.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Комментарий мастера.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Тип реакции мастера.
        /// </summary>
        public ReactionType Type { get; set; }

        /// <summary>
        /// Перечисление типов реакций мастера.
        /// </summary>
        public enum ReactionType
        {
            /// <summary>
            /// Долгая наладка.
            /// </summary>
            LongSetup,

            /// <summary>
            /// Поиск инструмента.
            /// </summary>
            ToolSearching
        }
    }
}
