using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using remeLog.Infrastructure.Winnum.Data;
using static remeLog.Infrastructure.Winnum.Types;

namespace remeLog.Infrastructure.Winnum
{
    internal class Parser
    {
        public static List<Dictionary<string, string>> ParseXmlItems(string xml)
        {
            var xDoc = XDocument.Parse(xml);
            var items = xDoc.Root?.Elements("item");
            var result = new List<Dictionary<string, string>>();

            if (items == null)
                return result;

            foreach (var item in items)
            {
                var dict = item.Attributes()
                               .ToDictionary(a => Uri.UnescapeDataString(a.Name.LocalName), a => Uri.UnescapeDataString(a.Value));
                result.Add(dict);
            }

            return result;
        }

        public static List<PriorityTagDuration> ParsePriorityTagDurations(string xml, DateTime start, DateTime end)
        {
            var xDoc = XDocument.Parse(xml);
            var items = xDoc.Root?.Elements("item") ?? Enumerable.Empty<XElement>();
            var result = new List<PriorityTagDuration>();

            foreach (var el in items)
            {
                var dto = new PriorityTagDuration
                {
                    SerialNumber = el.Attribute("SERIAL_NUMBER")?.Value ?? "",
                    Name = el.Attribute("NAME")?.Value ?? "",
                    Model = el.Attribute("MODEL")?.Value ?? "",
                    Tag = el.Attribute("TAG")?.Value ?? "",
                    Program = el.Attribute("PROGRAM")?.Value,
                    //TagOid = TryParseTagOid(el.Attribute("tagOid")?.Value, out var tagOid) ? tagOid : TagId.NONE,
                    TimeDataRaw = el.Attribute("timeData")?.Value ?? "",
                    Start = DateTime.ParseExact(
                        el.Attribute("START")?.Value ?? "",
                        "dd.MM.yyyy HH:mm:ss.fff",
                        CultureInfo.InvariantCulture),
                    End = DateTime.ParseExact(
                        el.Attribute("END")?.Value ?? "",
                        "dd.MM.yyyy HH:mm:ss.fff",
                        CultureInfo.InvariantCulture),
                    Duration = double.TryParse(el.Attribute("DURATION")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0
                };

                if (dto.Start < end && dto.End > start)
                {
                    result.Add(dto);
                }
            }

            return result;
        }

        private static bool TryParseTagOid(string? rawValue, out TagId result)
        {
            const string prefix = "winnum.org.tag.WNTag:";
            result = TagId.NONE;

            if (string.IsNullOrWhiteSpace(rawValue) || !rawValue.StartsWith(prefix))
                return false;

            var numericPart = rawValue.Substring(prefix.Length);

            if (int.TryParse(numericPart, out var id))
            {
                result = (TagId)id;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Возвращает элементы, у которых интервалы между ключами startKey и endKey
        /// хотя бы частично пересекаются с указанным диапазоном времени.
        /// </summary>
        /// <param name="items">Исходная коллекция словарей.</param>
        /// <param name="startKey">Ключ начала интервала в словаре.</param>
        /// <param name="start">Начало интересующего диапазона.</param>
        /// <param name="endKey">Ключ конца интервала в словаре.</param>
        /// <param name="end">Конец интересующего диапазона.</param>
        /// <param name="format">Формат дат в словаре.</param>
        /// <returns>Список словарей, чьи интервалы пересекаются с указанным диапазоном.</returns>
        public static List<Dictionary<string, string>> FilterByDateRange(
            List<Dictionary<string, string>> items,
            string startKey,
            DateTime start,
            string endKey,
            DateTime end,
            string format = "dd.MM.yyyy HH:mm:ss.fff")
        {
            var filtered = new List<Dictionary<string, string>>();

            foreach (var item in items)
            {
                if (!item.TryGetValue(startKey, out var startStr) ||
                    !item.TryGetValue(endKey, out var endStr))
                    continue;

                if (!DateTime.TryParseExact(startStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var itemStart) ||
                    !DateTime.TryParseExact(endStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var itemEnd))
                    continue;

                if (itemStart < end && itemEnd > start)
                    filtered.Add(item);
            }

            return filtered;
        }
    }
}
