// Controllers/ReporteController.cs
using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextDocument = iTextSharp.text.Document;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AuthFilter]
    public class ReporteController : Controller
    {
        private readonly ControladoraVenta _controladoraVenta;
        private readonly ControladoraCompra _controladoraCompra;

        public ReporteController()
        {
            _controladoraVenta = ControladoraVenta.ObtenerInstancia();
            _controladoraCompra = ControladoraCompra.ObtenerInstancia();
        }

        // ── REPORTE VENTAS ────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GenerarReporteVentas(DateTime? desde, DateTime? hasta)
        {
            var fechaDesde = desde ?? DateTime.Today.AddDays(-30);
            var fechaHasta = hasta ?? DateTime.Today;

            var ventas = _controladoraVenta.ObtenerVentasPorFecha(
                fechaDesde.Date,
                fechaHasta.Date.AddDays(1).AddSeconds(-1));

            using var ms = new MemoryStream();
            var document = new iTextDocument(PageSize.A4, 25, 25, 30, 30);
            PdfWriter.GetInstance(document, ms).CloseStream = false;
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

            // Título
            var title = new Paragraph("FRESCO MARKET — Reporte de Ventas", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph($"Período: {fechaDesde:dd/MM/yyyy} — {fechaHasta:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph(" "));

            // Resumen
            decimal totalVentas = ventas.Sum(v => v.Total);
            int cantTransacciones = ventas.Count;

            document.Add(new Paragraph("Resumen General", subtitleFont));
            document.Add(new Paragraph($"Total de Ventas: ${totalVentas:N2}", normalFont));
            document.Add(new Paragraph($"Número de Transacciones: {cantTransacciones}", normalFont));
            if (cantTransacciones > 0)
                document.Add(new Paragraph($"Ticket Promedio: ${(totalVentas / cantTransacciones):N2}", normalFont));
            document.Add(new Paragraph(" "));

            // Por forma de pago
            document.Add(new Paragraph("Ventas por Forma de Pago", subtitleFont));
            foreach (var grupo in ventas.GroupBy(v => v.FormaPago)
                                        .Select(g => new { fp = g.Key, total = g.Sum(v => v.Total) }))
            {
                document.Add(new Paragraph($"  {grupo.fp}: ${grupo.total:N2}", normalFont));
            }
            document.Add(new Paragraph(" "));

            // Tabla detalle
            document.Add(new Paragraph("Detalle de Ventas", subtitleFont));
            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 22f, 25f, 20f, 10f, 18f });

            void AddHeader(string txt)
            {
                var c = new PdfPCell(new Phrase(txt, boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 4 };
                table.AddCell(c);
            }
            AddHeader("Fecha"); AddHeader("Cliente"); AddHeader("Forma Pago");
            AddHeader("Items"); AddHeader("Total");

            foreach (var v in ventas.OrderByDescending(v => v.FechaVenta))
            {
                table.AddCell(new Phrase(v.FechaVenta.ToString("dd/MM/yyyy HH:mm"), normalFont));
                table.AddCell(new Phrase($"{v.Cliente?.Nombre} {v.Cliente?.Apellido}", normalFont));
                table.AddCell(new Phrase(v.FormaPago, normalFont));
                table.AddCell(new Phrase(v.Detalles.Sum(d => d.Cantidad).ToString(), normalFont));
                table.AddCell(new Phrase($"${v.Total:N2}", normalFont));
            }
            document.Add(table);

            // Estadísticas productos
            document.NewPage();
            document.Add(new Paragraph("Productos Más Vendidos", subtitleFont));
            var prodTop = ventas.SelectMany(v => v.Detalles)
                                .GroupBy(d => d.ProductoNombre)
                                .Select(g => new { nombre = g.Key, cant = g.Sum(d => d.Cantidad), tot = g.Sum(d => d.Subtotal) })
                                .OrderByDescending(x => x.cant).Take(10).ToList();

            var table2 = new PdfPTable(3) { WidthPercentage = 100 };
            table2.SetWidths(new float[] { 50f, 25f, 25f });
            AddHeader2(table2, "Producto", boldFont);
            AddHeader2(table2, "Cantidad", boldFont);
            AddHeader2(table2, "Total", boldFont);
            foreach (var p in prodTop)
            {
                table2.AddCell(new Phrase(p.nombre, normalFont));
                table2.AddCell(new Phrase(p.cant.ToString(), normalFont));
                table2.AddCell(new Phrase($"${p.tot:N2}", normalFont));
            }
            document.Add(table2);

            document.Close();
            ms.Position = 0;
            var fileName = $"ReporteVentas_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.pdf";
            return File(ms.ToArray(), "application/pdf", fileName);
        }

        // ── REPORTE COMPRAS ───────────────────────────────────────────────
        [HttpGet]
        public IActionResult GenerarReporteCompras(DateTime? desde, DateTime? hasta)
        {
            var fechaDesde = desde ?? DateTime.Today.AddDays(-30);
            var fechaHasta = hasta ?? DateTime.Today;

            var compras = _controladoraCompra.ObtenerComprasPorFecha(
                fechaDesde.Date,
                fechaHasta.Date.AddDays(1).AddSeconds(-1));

            using var ms = new MemoryStream();
            var document = new iTextDocument(PageSize.A4, 25, 25, 30, 30);
            PdfWriter.GetInstance(document, ms).CloseStream = false;
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

            var title = new Paragraph("FRESCO MARKET — Reporte de Compras", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);
            document.Add(new Paragraph($"Período: {fechaDesde:dd/MM/yyyy} — {fechaHasta:dd/MM/yyyy}", normalFont));
            document.Add(new Paragraph(" "));

            decimal totalCompras = compras.Sum(c => c.Total);
            int cantTransacciones = compras.Count;

            document.Add(new Paragraph("Resumen General", subtitleFont));
            document.Add(new Paragraph($"Total de Compras: ${totalCompras:N2}", normalFont));
            document.Add(new Paragraph($"Número de Transacciones: {cantTransacciones}", normalFont));
            if (cantTransacciones > 0)
                document.Add(new Paragraph($"Promedio por Compra: ${(totalCompras / cantTransacciones):N2}", normalFont));
            document.Add(new Paragraph(" "));

            document.Add(new Paragraph("Compras por Forma de Pago", subtitleFont));
            foreach (var g in compras.GroupBy(c => c.FormaPago)
                                     .Select(g => new { fp = g.Key, total = g.Sum(c => c.Total) }))
            {
                document.Add(new Paragraph($"  {g.fp}: ${g.total:N2}", normalFont));
            }
            document.Add(new Paragraph(" "));

            document.Add(new Paragraph("Compras por Proveedor", subtitleFont));
            foreach (var g in compras.GroupBy(c => c.Proveedor?.RazonSocial ?? "—")
                                     .Select(g => new { prov = g.Key, total = g.Sum(c => c.Total) })
                                     .OrderByDescending(x => x.total))
            {
                document.Add(new Paragraph($"  {g.prov}: ${g.total:N2}", normalFont));
            }
            document.Add(new Paragraph(" "));

            document.Add(new Paragraph("Detalle de Compras", subtitleFont));
            var table = new PdfPTable(6) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 18f, 22f, 15f, 15f, 10f, 15f });

            void AH(string txt)
            {
                var c = new PdfPCell(new Phrase(txt, boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 4 };
                table.AddCell(c);
            }
            AH("Fecha"); AH("Proveedor"); AH("N° Factura");
            AH("Forma Pago"); AH("Items"); AH("Total");

            foreach (var c in compras.OrderByDescending(c => c.FechaCompra))
            {
                table.AddCell(new Phrase(c.FechaCompra.ToString("dd/MM/yyyy HH:mm"), normalFont));
                table.AddCell(new Phrase(c.Proveedor?.RazonSocial ?? "—", normalFont));
                table.AddCell(new Phrase(c.NumeroFactura, normalFont));
                table.AddCell(new Phrase(c.FormaPago, normalFont));
                table.AddCell(new Phrase(c.Detalles.Sum(d => d.Cantidad).ToString(), normalFont));
                table.AddCell(new Phrase($"${c.Total:N2}", normalFont));
            }
            document.Add(table);
            document.Close();

            ms.Position = 0;
            var fileName = $"ReporteCompras_{fechaDesde:yyyyMMdd}_{fechaHasta:yyyyMMdd}.pdf";
            return File(ms.ToArray(), "application/pdf", fileName);
        }

        private static void AddHeader2(PdfPTable t, string txt, iTextSharp.text.Font f)
        {
            t.AddCell(new PdfPCell(new Phrase(txt, f)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 4 });
        }
    }
}