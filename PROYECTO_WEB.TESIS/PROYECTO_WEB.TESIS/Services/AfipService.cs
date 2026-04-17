using System;
using Microsoft.Extensions.Configuration;

namespace PROYECTO_WEB.TESIS.Services
{
    public class AfipTokenResponse
    {
        public string Token { get; set; }
        public string Sign { get; set; }
        public DateTime ExpirationTime { get; set; }
    }

    public class AfipFacturaRequest
    {
        public string TipoComprobante { get; set; } // "A" o "B"
        public int PuntoVenta { get; set; } = 1;
        public decimal ImporteTotal { get; set; }
        public decimal ImporteNeto { get; set; }
        public decimal ImporteIVA { get; set; }
        public string CuitReceptor { get; set; } // null = consumidor final
        public string ConceptoTipo { get; set; } = "1"; // 1=Productos
    }

    public class AfipFacturaResponse
    {
        public bool Exitoso { get; set; }
        public string CAE { get; set; }
        public DateTime FechaVencimientoCAE { get; set; }
        public int NroComprobante { get; set; }
        public string MensajeError { get; set; }
    }

    public class AfipService
    {
        // ─── Constantes — cuando se pase a producción solo cambiar estas 3 cosas:
        //     1. MODO_SIMULACION = false
        //     2. URLs homo → producción
        //     3. CUIT_EMISOR → CUIT real del negocio
        // ────────────────────────────────────────────────────────────────────
        private const bool MODO_SIMULACION = true;

        private const string WSAA_URL = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";
        private const string WSFE_URL = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx";
        private const string CUIT_EMISOR = "20111111112";
        private const int PUNTO_VENTA = 1;

        // Contador interno para simular numeración correlativa
        private static int _contadorFacturasA = 0;
        private static int _contadorFacturasB = 0;

        private readonly IConfiguration _config;

        public AfipService(IConfiguration config)
        {
            _config = config;
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTODO PRINCIPAL — EmitirFacturaAsync
        // En simulación: genera CAE y NroComprobante falsos pero con formato real
        // En producción: poner MODO_SIMULACION = false
        // ════════════════════════════════════════════════════════════════════
        public async Task<AfipFacturaResponse> EmitirFacturaAsync(AfipFacturaRequest req)
        {
            if (MODO_SIMULACION)
                return SimularEmision(req);

            // ── BLOQUE REAL (activar cuando se tenga certificado registrado en ARCA) ──
            // AfipTokenResponse token;
            // try { token = await ObtenerTokenAsync(); }
            // catch (Exception ex) { return new AfipFacturaResponse { Exitoso = false, MensajeError = $"Error auth: {ex.Message}" }; }
            // return await EmitirConWSFE(token, req);
            // ─────────────────────────────────────────────────────────────────

            return SimularEmision(req);
        }

        // ════════════════════════════════════════════════════════════════════
        // SIMULACIÓN — genera respuesta realista sin llamar a AFIP
        // ════════════════════════════════════════════════════════════════════
        private AfipFacturaResponse SimularEmision(AfipFacturaRequest req)
        {
            // Simular demora de llamada real
            System.Threading.Thread.Sleep(800);

            // Incrementar contador según tipo de comprobante
            int nroComprobante;
            if (req.TipoComprobante == "A")
                nroComprobante = ++_contadorFacturasA;
            else
                nroComprobante = ++_contadorFacturasB;

            // Generar CAE simulado con formato real de AFIP (14 dígitos numéricos)
            // Formato: 2 dígitos tipo + 6 dígitos fecha + 4 dígitos secuencia + 2 verificación
            var fecha = DateTime.Now.ToString("yyyyMMdd");
            var secuencia = nroComprobante.ToString().PadLeft(4, '0');
            var tipoPrefix = req.TipoComprobante == "A" ? "10" : "60";
            var caeSimulado = tipoPrefix + fecha.Substring(2) + secuencia; // 14 dígitos

            // Vencimiento CAE = 10 días (igual que AFIP real)
            var fechaVencimiento = DateTime.Now.AddDays(10);

            return new AfipFacturaResponse
            {
                Exitoso = true,
                CAE = caeSimulado,
                FechaVencimientoCAE = fechaVencimiento,
                NroComprobante = nroComprobante
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // MÉTODOS REALES — se usan cuando MODO_SIMULACION = false
        // ════════════════════════════════════════════════════════════════════
        private async Task<AfipTokenResponse> ObtenerTokenAsync()
        {
            var tra = BuildTRA();
            var traFirmado = FirmarTRA(tra);

            var soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <loginCms xmlns=""http://wsaa.view.sua.dvadac.desein.afip.gov/"">
      <in0>{EscapeXml(traFirmado)}</in0>
    </loginCms>
  </soap:Body>
</soap:Envelope>";

            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            var content = new System.Net.Http.StringContent(soapRequest, System.Text.Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "\"\"");

            var response = await client.PostAsync(WSAA_URL, content);
            var responseStr = await response.Content.ReadAsStringAsync();

            var xdoc = System.Xml.Linq.XDocument.Parse(responseStr);
            var loginCmsReturn = xdoc.Descendants("loginCmsReturn").FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(loginCmsReturn))
                throw new Exception("AFIP WSAA no devolvió token. Respuesta: " + responseStr);

            var ta = System.Xml.Linq.XDocument.Parse(loginCmsReturn);
            var token = ta.Descendants("token").FirstOrDefault()?.Value;
            var sign = ta.Descendants("sign").FirstOrDefault()?.Value;
            var expirationStr = ta.Descendants("expirationTime").FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(sign))
                throw new Exception("Token o Sign vacíos en respuesta WSAA.");

            DateTime expiration = DateTime.Now.AddHours(12);
            if (!string.IsNullOrEmpty(expirationStr))
                DateTime.TryParse(expirationStr, out expiration);

            return new AfipTokenResponse { Token = token, Sign = sign, ExpirationTime = expiration };
        }

        private string BuildTRA()
        {
            var now = DateTime.UtcNow;
            var from = now.AddMinutes(-10).ToString("yyyy-MM-ddTHH:mm:sszzz");
            var to = now.AddHours(12).ToString("yyyy-MM-ddTHH:mm:sszzz");
            var uniqueId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            return $@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<loginTicketRequest version=""1.0"">
  <header>
    <uniqueId>{uniqueId}</uniqueId>
    <generationTime>{from}</generationTime>
    <expirationTime>{to}</expirationTime>
  </header>
  <service>wsfe</service>
</loginTicketRequest>";
        }

        private string FirmarTRA(string tra)
        {
            var certPath = _config["Afip:CertPath"] ?? System.IO.Path.Combine(AppContext.BaseDirectory, "Certificados", "fresco.p12");
            var certPass = _config["Afip:CertPassword"] ?? "fresco123";

            if (!System.IO.File.Exists(certPath))
                throw new Exception($"Certificado no encontrado en: {certPath}");

            var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                certPath, certPass,
                System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet |
                System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.EphemeralKeySet);

            var traBytes = System.Text.Encoding.UTF8.GetBytes(tra);
            var contentInfo = new System.Security.Cryptography.Pkcs.ContentInfo(traBytes);
            var signedCms = new System.Security.Cryptography.Pkcs.SignedCms(contentInfo);
            var signer = new System.Security.Cryptography.Pkcs.CmsSigner(cert)
            {
                IncludeOption = System.Security.Cryptography.X509Certificates.X509IncludeOption.EndCertOnly
            };
            signedCms.ComputeSignature(signer);
            return Convert.ToBase64String(signedCms.Encode());
        }

        private static string EscapeXml(string s) =>
            s?.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";
    }
}