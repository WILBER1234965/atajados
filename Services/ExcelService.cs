using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using atajados.Models;
using ClosedXML.Excel;

namespace atajados.Services
{
    public class ExcelService
    {
        // ------------------- EXPORTAR -------------------
        public Task ExportItemsAsync(string filePath, IEnumerable<Item> items)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Items");

            ws.Cell(1, 1).Value = "Numero";
            ws.Cell(1, 2).Value = "Descripcion";
            ws.Cell(1, 3).Value = "Unidad";
            ws.Cell(1, 4).Value = "Cantidad";
            ws.Cell(1, 5).Value = "PrecioUnitario";
            ws.Cell(1, 6).Value = "UsarEnSeguimiento";

            var r = 2;
            foreach (var it in items)
            {
                ws.Cell(r, 1).Value = it.Numero;
                ws.Cell(r, 2).Value = it.Descripcion;
                ws.Cell(r, 3).Value = it.Unidad;
                ws.Cell(r, 4).Value = it.Cantidad;
                ws.Cell(r, 5).Value = it.PrecioUnitario;
                ws.Cell(r, 6).Value = it.UsarEnSeguimiento;
                r++;
            }

            wb.SaveAs(filePath);
            return Task.CompletedTask;
        }

        // ------------------- IMPORTAR -------------------
        public Task<List<Item>> ImportItemsAsync(string filePath, string sheetName = "Items")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var list = new List<Item>();

            using var wb = new XLWorkbook(filePath);

            // Si la hoja no existe usa la primera
            var ws = wb.Worksheets.TryGetWorksheet(sheetName, out var tmp)
                       ? tmp
                       : wb.Worksheet(1);

            foreach (var row in ws.RangeUsed().RowsUsed().Skip(1))
            {
                // Manejo seguro del booleano (admite 0/1, true/false, Sí/No, etc.)
                var boolText = row.Cell(6).GetString().Trim().ToLowerInvariant();
                bool usarSeg = boolText == "1" || boolText == "true" ||
                               boolText == "sí" || boolText == "si";

                list.Add(new Item
                {
                    Numero = row.Cell(1).GetString(),
                    Descripcion = row.Cell(2).GetString(),
                    Unidad = row.Cell(3).GetString(),
                    Cantidad = (decimal)row.Cell(4).GetDouble(),
                    PrecioUnitario = (decimal)row.Cell(5).GetDouble(),
                    UsarEnSeguimiento = usarSeg
                });
            }

            return Task.FromResult(list);
        }
    }
}
