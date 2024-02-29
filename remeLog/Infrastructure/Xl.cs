using ClosedXML.Excel;
using remeLog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace remeLog.Infrastructure
{
    public static class Xl
    {
        public static ICollection<Part> ReadParts()
        {
            List<Part> parts = new List<Part>();
            if (!Directory.Exists(AppSettings.Instance.SourcePath)) { return parts; }

            using (var fs = new FileStream(AppSettings.Instance.SourcePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var wb = new XLWorkbook(fs, new LoadOptions() { RecalculateAllFormulas = false });

                foreach (var xlRow in wb.Worksheet(1).Rows().Skip(1))
                {
                    if (!xlRow.Cell(1).Value.IsNumber) break;
                    parts.Add(new Part(
                        Guid.Parse(xlRow.Cell(36).Value.GetText()),
                        xlRow.Cell(7).Value.GetText(),
                        xlRow.Cell(6).Value.GetText(),
                        xlRow.Cell(8).Value.GetDateTime(),
                        xlRow.Cell(9).Value.GetText(),
                        xlRow.Cell(10).Value.GetText(),
                        xlRow.Cell(11).Value.GetText(),
                        (int)xlRow.Cell(13).Value.GetNumber(),
                        (int)xlRow.Cell(12).Value.GetNumber(),
                        0,
                        xlRow.Cell(14).Value.GetDateTime(),
                        xlRow.Cell(15).Value.GetDateTime(),
                        xlRow.Cell(16).Value.GetNumber(),
                        xlRow.Cell(21).Value.GetDateTime(),
                        xlRow.Cell(17).Value.GetNumber(),
                        xlRow.Cell(44).Value.GetNumber(),
                        xlRow.Cell(23).Value.GetNumber(),
                        xlRow.Cell(22).Value.GetNumber(),
                        TimeSpan.FromMinutes(xlRow.Cell(24).Value.GetNumber()),
                        xlRow.Cell(34).Value.GetNumber(),
                        xlRow.Cell(35).Value.GetNumber(),
                        xlRow.Cell(37).Value.GetNumber(),
                        xlRow.Cell(38).Value.GetNumber(),
                        xlRow.Cell(39).Value.GetNumber(),
                        xlRow.Cell(40).Value.GetNumber(),
                        xlRow.Cell(41).Value.GetNumber(),
                        xlRow.Cell(42).Value.GetNumber(),
                        xlRow.Cell(43).Value.GetNumber(),
                        xlRow.Cell(5).Value.GetText()
                        ));
                }
            }

            return parts;
        }
    }
}
