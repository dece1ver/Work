using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using remeLog.Infrastructure.Winnum.Data;

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
                               .ToDictionary(a => a.Name.LocalName, a => a.Value);
                result.Add(dict);
            }

            return result;
        }

        public static List<PriorityTagDuration> ParsePriorityTagDurations(string xml)
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
                    TagOid = el.Attribute("tagOid")?.Value ?? "",
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

                result.Add(dto);
            }

            return result;
        }

    }
}
