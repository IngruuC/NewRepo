using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;



namespace ENTIDADES.SEGURIDAD
{
    public class EmailService
    {
        private readonly string smtpServer;
        private readonly int smtpPort;
        private readonly string smtpUsername;
        private readonly string smtpPassword;
        private readonly bool enableSsl;

        public EmailService(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword, bool enableSsl = true)
        {
            this.smtpServer = smtpServer;
            this.smtpPort = smtpPort;
            this.smtpUsername = smtpUsername;
            this.smtpPassword = smtpPassword;
            this.enableSsl = enableSsl;
        }

        public async Task<bool> EnviarEmailAsync(string destinatario, string asunto, string cuerpo, bool esHtml = true)
        {
            try
            {
                var mensaje = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = asunto,
                    Body = cuerpo,
                    IsBodyHtml = esHtml
                };

                mensaje.To.Add(new MailAddress(destinatario));

                using (var cliente = new SmtpClient(smtpServer, smtpPort))
                {
                    cliente.EnableSsl = enableSsl;
                    cliente.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    await cliente.SendMailAsync(mensaje);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar email: {ex.Message}");
                return false;
            }
        }

        public bool EnviarEmail(string destinatario, string asunto, string cuerpo, bool esHtml = true)
        {
            try
            {
                var mensaje = new MailMessage
                {
                    From = new MailAddress(smtpUsername),
                    Subject = asunto,
                    Body = cuerpo,
                    IsBodyHtml = esHtml
                };

                mensaje.To.Add(new MailAddress(destinatario));

                using (var cliente = new SmtpClient(smtpServer, smtpPort))
                {
                    cliente.EnableSsl = enableSsl;
                    cliente.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    cliente.Send(mensaje);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar email: {ex.Message}");
                return false;
            }
        }
    }
}