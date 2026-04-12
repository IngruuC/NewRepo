using ENTIDADES;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextDocument = iTextSharp.text.Document;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CONTROLADORA
{
    public class ControladoraReporteCompra
    {
        private static ControladoraReporteCompra instancia;
        private ControladoraCompra controladoraCompra;

        private ControladoraReporteCompra()
        {
            controladoraCompra = ControladoraCompra.ObtenerInstancia();
        }

        public static ControladoraReporteCompra ObtenerInstancia()
        {
            if (instancia == null)
                instancia = new ControladoraReporteCompra();
            return instancia;
        }

        public void GenerarReporteCompras(DateTime fechaInicio, DateTime fechaFin, string rutaGuardado, byte[] imagenGrafico)
        {
            var compras = controladoraCompra.ObtenerComprasPorFecha(fechaInicio, fechaFin);

            using (FileStream fs = new FileStream(rutaGuardado, FileMode.Create))
            {
                iTextDocument document = new iTextDocument(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);

                document.Open();

                // Título
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                document.Add(new Paragraph("FRESCO MARKET - Reporte de Compras", titleFont));
                document.Add(new Paragraph($"Período: {fechaInicio.ToShortDateString()} - {fechaFin.ToShortDateString()}\n\n", normalFont));

                // Resumen general
                decimal totalCompras = compras.Sum(c => c.Total);
                int totalTransacciones = compras.Count;

                document.Add(new Paragraph("Resumen General", subtitleFont));
                document.Add(new Paragraph($"Total de Compras: ${totalCompras:N2}", normalFont));
                document.Add(new Paragraph($"Número de Transacciones: {totalTransacciones}", normalFont));
                if (totalTransacciones > 0)
                    document.Add(new Paragraph($"Promedio por Compra: ${(totalCompras / totalTransacciones):N2}\n\n", normalFont));

                // Compras por forma de pago
                document.Add(new Paragraph("Compras por Forma de Pago", subtitleFont));
                var comprasPorFormaPago = compras.GroupBy(c => c.FormaPago)
                    .Select(g => new { FormaPago = g.Key, Total = g.Sum(c => c.Total) });
                foreach (var grupo in comprasPorFormaPago)
                {
                    document.Add(new Paragraph($"{grupo.FormaPago}: ${grupo.Total:N2}", normalFont));
                }
                document.Add(new Paragraph("\n"));

                // Compras por proveedor
                document.Add(new Paragraph("Compras por Proveedor", subtitleFont));
                var comprasPorProveedor = compras.GroupBy(c => c.Proveedor.RazonSocial)
                    .Select(g => new { Proveedor = g.Key, Total = g.Sum(c => c.Total) })
                    .OrderByDescending(x => x.Total);
                foreach (var grupo in comprasPorProveedor)
                {
                    document.Add(new Paragraph($"{grupo.Proveedor}: ${grupo.Total:N2}", normalFont));
                }
                document.Add(new Paragraph("\n"));

                // Tabla de detalles de compras
                document.Add(new Paragraph("Detalle de Compras", subtitleFont));
                PdfPTable table = new PdfPTable(6);
                float[] widths = new float[] { 20f, 25f, 20f, 15f, 20f, 20f };
                table.SetWidths(widths);
                table.WidthPercentage = 100;

                // Estilo
                var cellStyle = new PdfPCell
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    Padding = 5
                };

                // Encabezado
                table.AddCell(new PdfPCell(new Phrase("Fecha", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Proveedor", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("N° Factura", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Forma Pago", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Items", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Total", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                // Datos
                foreach (var compra in compras.OrderByDescending(c => c.FechaCompra))
                {
                    table.AddCell(compra.FechaCompra.ToString("dd/MM/yyyy HH:mm"));
                    table.AddCell(compra.Proveedor.RazonSocial);
                    table.AddCell(compra.NumeroFactura);
                    table.AddCell(compra.FormaPago);
                    table.AddCell(compra.Detalles.Sum(d => d.Cantidad).ToString());
                    table.AddCell($"${compra.Total:N2}");
                }
                document.Add(table);

                // Agregar gráfico
                if (imagenGrafico != null && imagenGrafico.Length > 0)
                {
                    document.NewPage();
                    document.Add(new Paragraph("Gráfico de Compras", subtitleFont));
                    document.Add(new Paragraph("\n"));

                    var chartImage = iTextSharp.text.Image.GetInstance(imagenGrafico);
                    chartImage.ScalePercent(75);
                    chartImage.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
                    document.Add(chartImage);
                }

                document.Close();
            }
        }
    }
}