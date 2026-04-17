using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using PROYECTO_WEB.TESIS.Helpers;
using PROYECTO_WEB.TESIS.Services;
using Microsoft.AspNetCore.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextDocument = iTextSharp.text.Document;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AdminOnlyFilter]
    public class FacturacionController : Controller
    {
        private readonly AfipService _afipService;
        private readonly ControladoraVenta _controladoraVenta;
        private readonly ControladoraFacturacion _controladoraFacturacion;

        public FacturacionController(AfipService afipService)
        {
            _afipService = afipService;
            _controladoraVenta = ControladoraVenta.ObtenerInstancia();
            _controladoraFacturacion = ControladoraFacturacion.ObtenerInstancia();
        }

        // ── Emitir factura para una venta ─────────────────────────────────
        [HttpPost]
        public async Task<JsonResult> EmitirFactura(int ventaId, string tipoComprobante, string cuitReceptor)
        {
            try
            {
                // Verificar que no tenga factura ya
                if (_controladoraFacturacion.TieneFactura(ventaId))
                    return Json(new { ok = false, error = "Esta venta ya tiene una factura emitida." });

                var venta = _controladoraVenta.ObtenerVentaPorId(ventaId);
                if (venta == null)
                    return Json(new { ok = false, error = "Venta no encontrada." });

                // Calcular importes
                decimal importeNeto = Math.Round(venta.Total / 1.21m, 2);
                decimal importeIVA = Math.Round(venta.Total - importeNeto, 2);

                var req = new AfipFacturaRequest
                {
                    TipoComprobante = tipoComprobante, // "A" o "B"
                    ImporteTotal = venta.Total,
                    ImporteNeto = importeNeto,
                    ImporteIVA = importeIVA,
                    CuitReceptor = tipoComprobante == "A" ? cuitReceptor : null,
                    ConceptoTipo = "1"
                };

                var resultado = await _afipService.EmitirFacturaAsync(req);

                if (!resultado.Exitoso)
                    return Json(new { ok = false, error = resultado.MensajeError });

                // Guardar en BD
                var factura = new FacturaAfip
                {
                    VentaId = ventaId,
                    TipoComprobante = tipoComprobante,
                    NroComprobante = resultado.NroComprobante,
                    CAE = resultado.CAE,
                    FechaVencimientoCAE = resultado.FechaVencimientoCAE,
                    FechaEmision = DateTime.Now,
                    ImporteTotal = venta.Total,
                    ImporteNeto = importeNeto,
                    ImporteIVA = importeIVA,
                    CuitEmisor = "20111111112",
                    CuitReceptor = tipoComprobante == "A" ? cuitReceptor : null,
                    PuntoVenta = 1,
                    Estado = "Emitida"
                };

                _controladoraFacturacion.GuardarFactura(factura);

                // Log
                var usuario = SessionHelper.GetUsuarioNombre(HttpContext.Session) ?? "Admin";
                ControladoraLog.ObtenerInstancia().Registrar(usuario, "Emitió Factura AFIP",
                    $"Venta #{ventaId} — Tipo: {tipoComprobante} — CAE: {resultado.CAE}",
                    HttpContext.Connection.RemoteIpAddress?.ToString());

                return Json(new
                {
                    ok = true,
                    cae = resultado.CAE,
                    nroComprobante = resultado.NroComprobante,
                    vencimientoCAE = resultado.FechaVencimientoCAE.ToString("dd/MM/yyyy"),
                    facturaId = factura.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        // ── Ver facturas emitidas (JSON para el panel) ────────────────────
        [HttpGet]
        public JsonResult ObtenerFacturas()
        {
            var facturas = _controladoraFacturacion.ObtenerTodas();
            return Json(facturas.Select(f => new
            {
                f.Id,
                f.VentaId,
                cliente = f.Venta?.Cliente?.ToString() ?? "—",
                tipo = f.TipoComprobante,
                nro = f.NroComprobante,
                f.CAE,
                fechaEmision = f.FechaEmision.ToString("dd/MM/yyyy HH:mm"),
                vencCAE = f.FechaVencimientoCAE.ToString("dd/MM/yyyy"),
                total = f.ImporteTotal.ToString("N2"),
                f.Estado
            }));
        }

        // ── Verificar si una venta tiene factura ─────────────────────────
        [HttpGet]
        public JsonResult TieneFactura(int ventaId)
        {
            var factura = _controladoraFacturacion.ObtenerPorVenta(ventaId);
            if (factura == null)
                return Json(new { tiene = false });

            return Json(new
            {
                tiene = true,
                cae = factura.CAE,
                nro = factura.NroComprobante,
                tipo = factura.TipoComprobante,
                vencCAE = factura.FechaVencimientoCAE.ToString("dd/MM/yyyy"),
                total = factura.ImporteTotal.ToString("N2")
            });
        }

        // ── Descargar PDF de la factura ──────────────────────────────────
        [HttpGet]
        public IActionResult DescargarFacturaPDF(int ventaId)
        {
            var factura = _controladoraFacturacion.ObtenerPorVenta(ventaId);
            if (factura == null)
                return NotFound("Factura no encontrada.");

            var venta = _controladoraVenta.ObtenerVentaPorId(ventaId);

            using var ms = new MemoryStream();
            var doc = new iTextDocument(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(doc, ms).CloseStream = false;
            doc.Open();

            // Fuentes
            var fTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var fSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13);
            var fNormal = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            var fBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            var fGrande = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22);
            var fPeq = FontFactory.GetFont(FontFactory.HELVETICA, 9);

            // ── ENCABEZADO ──────────────────────────────────────────────
            var tablaCab = new PdfPTable(3) { WidthPercentage = 100 };
            tablaCab.SetWidths(new float[] { 35f, 15f, 35f });

            // Celda izquierda — datos emisor
            var celdaEmisor = new PdfPCell { Border = Rectangle.BOX, Padding = 8 };
            celdaEmisor.AddElement(new Paragraph("FRESCO MARKET", fSubtitulo));
            celdaEmisor.AddElement(new Paragraph("CUIT: 20-11111111-2", fNormal));
            celdaEmisor.AddElement(new Paragraph("Ing. Bravo 1234, Rosario", fNormal));
            celdaEmisor.AddElement(new Paragraph("IVA Responsable Inscripto", fNormal));
            tablaCab.AddCell(celdaEmisor);

            // Celda central — tipo de comprobante (letra grande)
            var celdaTipo = new PdfPCell { Border = Rectangle.BOX, Padding = 8, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE };
            celdaTipo.AddElement(new Paragraph(factura.TipoComprobante, fGrande) { Alignment = Element.ALIGN_CENTER });
            celdaTipo.AddElement(new Paragraph("COD. " + (factura.TipoComprobante == "A" ? "001" : "006"), fPeq) { Alignment = Element.ALIGN_CENTER });
            tablaCab.AddCell(celdaTipo);

            // Celda derecha — datos comprobante
            var celdaComp = new PdfPCell { Border = Rectangle.BOX, Padding = 8 };
            celdaComp.AddElement(new Paragraph($"FACTURA {factura.TipoComprobante}", fSubtitulo));
            celdaComp.AddElement(new Paragraph($"Punto de Venta: {factura.PuntoVenta:D4}    Comp. Nro: {factura.NroComprobante:D8}", fNormal));
            celdaComp.AddElement(new Paragraph($"Fecha: {factura.FechaEmision:dd/MM/yyyy}", fNormal));
            tablaCab.AddCell(celdaComp);

            doc.Add(tablaCab);
            doc.Add(new Paragraph(" "));

            // ── DATOS RECEPTOR ──────────────────────────────────────────
            var tablaReceptor = new PdfPTable(2) { WidthPercentage = 100 };
            tablaReceptor.SetWidths(new float[] { 50f, 50f });

            void AddCeldaReceptor(string label, string value)
            {
                var c = new PdfPCell { Border = Rectangle.NO_BORDER, Padding = 3 };
                c.AddElement(new Paragraph(label + ": ", fBold) { SpacingAfter = 0 });
                c.AddElement(new Paragraph(value, fNormal));
                tablaReceptor.AddCell(c);
            }

            var clienteNombre = venta?.Cliente?.ToString() ?? "Consumidor Final";
            AddCeldaReceptor("Apellido y Nombre / Razón Social", clienteNombre);
            AddCeldaReceptor("Condición frente al IVA", factura.TipoComprobante == "A" ? "Responsable Inscripto" : "Consumidor Final");
            AddCeldaReceptor("CUIT / DNI", factura.CuitReceptor ?? "—");
            AddCeldaReceptor("Condición de venta", venta?.FormaPago ?? "—");

            doc.Add(tablaReceptor);
            doc.Add(new Paragraph(" "));

            // ── DETALLE ─────────────────────────────────────────────────
            doc.Add(new Paragraph("Detalle", fSubtitulo));
            var tablaDetalle = new PdfPTable(4) { WidthPercentage = 100 };
            tablaDetalle.SetWidths(new float[] { 40f, 20f, 20f, 20f });

            void AddHeader(string txt)
            {
                tablaDetalle.AddCell(new PdfPCell(new Phrase(txt, fBold)) { BackgroundColor = BaseColor.LIGHT_GRAY, Padding = 5 });
            }
            AddHeader("Descripción"); AddHeader("Cant."); AddHeader("P. Unit."); AddHeader("Subtotal");

            if (venta?.Detalles != null)
            {
                foreach (var d in venta.Detalles)
                {
                    tablaDetalle.AddCell(new Phrase(d.ProductoNombre, fNormal));
                    tablaDetalle.AddCell(new Phrase(d.Cantidad.ToString(), fNormal));
                    tablaDetalle.AddCell(new Phrase($"${d.PrecioUnitario:N2}", fNormal));
                    tablaDetalle.AddCell(new Phrase($"${d.Subtotal:N2}", fNormal));
                }
            }
            doc.Add(tablaDetalle);
            doc.Add(new Paragraph(" "));

            // ── TOTALES ─────────────────────────────────────────────────
            var tablaTotales = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_RIGHT };
            tablaTotales.SetWidths(new float[] { 50f, 50f });

            void AddFila(string label, string val, bool bold = false)
            {
                var f = bold ? fBold : fNormal;
                tablaTotales.AddCell(new PdfPCell(new Phrase(label, f)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                tablaTotales.AddCell(new PdfPCell(new Phrase(val, f)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
            }

            AddFila("Importe Neto:", $"${factura.ImporteNeto:N2}");
            AddFila("IVA 21%:", $"${factura.ImporteIVA:N2}");
            AddFila("TOTAL:", $"${factura.ImporteTotal:N2}", bold: true);
            doc.Add(tablaTotales);

            // ── CAE ──────────────────────────────────────────────────────
            doc.Add(new Paragraph(" "));
            var tablaCAE = new PdfPTable(1) { WidthPercentage = 100 };
            var celdaCAE = new PdfPCell { Border = Rectangle.BOX, Padding = 8, BackgroundColor = new BaseColor(255, 248, 220) };
            celdaCAE.AddElement(new Paragraph("CAE: " + factura.CAE, fBold));
            celdaCAE.AddElement(new Paragraph("Fecha de Vencimiento del CAE: " + factura.FechaVencimientoCAE.ToString("dd/MM/yyyy"), fNormal));
            celdaCAE.AddElement(new Paragraph("Comprobante autorizado por AFIP — Homologación", fPeq));
            tablaCAE.AddCell(celdaCAE);
            doc.Add(tablaCAE);

            doc.Close();
            ms.Position = 0;

            var fileName = $"Factura{factura.TipoComprobante}_{factura.NroComprobante:D8}.pdf";
            return File(ms.ToArray(), "application/pdf", fileName);
        }
    }
}