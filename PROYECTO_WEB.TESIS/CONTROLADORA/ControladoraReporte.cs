using ENTIDADES;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextDocument = iTextSharp.text.Document;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CONTROLADORA
{
    public class ControladoraReporte
    {
        private static ControladoraReporte instancia;
        private ControladoraVenta controladoraVenta;

        private ControladoraReporte()
        {
            controladoraVenta = ControladoraVenta.ObtenerInstancia();
        }

        public static ControladoraReporte ObtenerInstancia()
        {
            if (instancia == null)
                instancia = new ControladoraReporte();
            return instancia;
        }

        public void GenerarReporteVentas(DateTime fechaInicio, DateTime fechaFin, string rutaGuardado, byte[] imagenGrafico)
        {
            var ventas = controladoraVenta.ObtenerVentasPorFecha(fechaInicio, fechaFin);

            using (FileStream fs = new FileStream(rutaGuardado, FileMode.Create))
            {
                iTextDocument document = new iTextDocument(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);

                document.Open();

                // Titulo
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                document.Add(new Paragraph("FRESCO MARKET - Reporte de Ventas", titleFont));
                document.Add(new Paragraph($"Período: {fechaInicio.ToShortDateString()} - {fechaFin.ToShortDateString()}\n\n", normalFont));

                // Resumen general
                decimal totalVentas = ventas.Sum(v => v.Total);
                int totalTransacciones = ventas.Count;

                document.Add(new Paragraph("Resumen General", subtitleFont));
                document.Add(new Paragraph($"Total de Ventas: ${totalVentas:N2}", normalFont));
                document.Add(new Paragraph($"Número de Transacciones: {totalTransacciones}", normalFont));
                if (totalTransacciones > 0)
                    document.Add(new Paragraph($"Ticket Promedio: ${(totalVentas / totalTransacciones):N2}\n\n", normalFont));

                // Ventas por forma de pago
                document.Add(new Paragraph("Ventas por Forma de Pago", subtitleFont));
                var ventasPorFormaPago = ventas.GroupBy(v => v.FormaPago)
                    .Select(g => new { FormaPago = g.Key, Total = g.Sum(v => v.Total) });
                foreach (var grupo in ventasPorFormaPago)
                {
                    document.Add(new Paragraph($"{grupo.FormaPago}: ${grupo.Total:N2}", normalFont));
                }
                document.Add(new Paragraph("\n"));

                // Tabla de detalles de ventas
                document.Add(new Paragraph("Detalle de Ventas", subtitleFont));
                PdfPTable table = new PdfPTable(5);
                float[] widths = new float[] { 20f, 25f, 20f, 15f, 20f };
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
                table.AddCell(new PdfPCell(new Phrase("Cliente", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Forma de Pago", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Items", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                table.AddCell(new PdfPCell(new Phrase("Total", subtitleFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                // Datos
                foreach (var venta in ventas.OrderByDescending(v => v.FechaVenta))
                {
                    table.AddCell(venta.FechaVenta.ToString("dd/MM/yyyy HH:mm"));
                    table.AddCell($"{venta.Cliente.Nombre} {venta.Cliente.Apellido}");
                    table.AddCell(venta.FormaPago);
                    table.AddCell(venta.Detalles.Sum(d => d.Cantidad).ToString());
                    table.AddCell($"${venta.Total:N2}");
                }
                document.Add(table);

                // Agregar grafico
                if (imagenGrafico != null && imagenGrafico.Length > 0)
                {
                    document.NewPage(); // Nueva página para el gráfico
                    document.Add(new Paragraph("Gráfico de Ventas", subtitleFont));
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