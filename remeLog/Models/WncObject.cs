using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class WncObject
    {
        public WncObject(string name, string id, string link, string version, string state, string container, string type, string modifyDate, string createDate)
        {
            Name = name;
            Id = id;
            Link = link;
            Version = version;
            State = state;
            Container = container;
            Type = type;
            ModifyDate = modifyDate;
            CreateDate = createDate;
        }

        public string Name { get; set; }
        public string Id { get; set; }
        public string Link { get; set; }
        public string Version { get; set; }
        public string State { get; set; }
        public string PrettyState
        {
            get
            {
                return State switch
                {
                    "LITERA_A" => "Литера А",
                    "In Work" => "В работе",
                    "Released" => "Выпущено",
                    _ => State,
                };
            }
            
        }
        public string Container { get; set; }
        public string Type { get; set; }

        public string PrettyType
        {
            get
            {
                return Type switch
                {
                    "CAD Part" => "Модель",
                    "CAD Part Generic" => "Модель с семейством",
                    "CAD Part Instance" => "Модель из семейства",
                    "Assembly" => "Сборка",
                    "Assembly Generic" => "Сборка с семейством",
                    "Assembly Instance" => "Сборка из семейства",
                    "Drawing" => "Чертеж",
                    "NC Assembly" => "Сборка ЧПУ",
                    "Machine control data file" => "УП",
                    _ => Type,
                };
            }
        }
        public string ModifyDate { get; set; }
        public string CreateDate { get; set; }

        public override string ToString()
        {
            return $"Наименование: {Name}\n" +
                   $"Обозначение: {Id}\n" +
                   $"Версия: {Version}\n" +
                   $"Состояние: {State}\n" +
                   $"Контекст: {Container}\n" +
                   $"Тип: {Type}\n" +
                   $"Изменен: {ModifyDate}\n" +
                   $"Создан: {CreateDate}\n" +
                   $"Ссылка: {Link}\n";
        }
    }

}
