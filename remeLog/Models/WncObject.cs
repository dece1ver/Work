using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Models
{
    public class WncObject
    {
        public WncObject(string name, string id, string link)
        {
            Name = name;
            Id = id;
            Link = link;
        }

        public string Name { get; set; }
        public string Id { get; set; }
        public string Link { get; set; }

        public override string ToString()
        {
            return $"Наименование: {Name}\n" +
                   $"Обозначение: {Id}\n" +
                   $"Ссылка: {Link}\n";
        }
    }
}
