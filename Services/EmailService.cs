using System.Net;
using System.Net.Mail;

namespace EventAccessControl.API.Services
{   
    /// <summary>
    /// Servicio para enviar correos electrónicos con códigos QR adjuntos.  
    /// /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor que recibe la configuración de la aplicación para obtener los parámetros necesarios para enviar correos electrónicos, como el servidor SMTP, puerto, 
        /// credenciales, etc.
        /// </summary>
        /// <param name="config"></param>
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Método para enviar un correo electrónico con un código QR adjunto. Recibe el correo electrónico del destinatario y el código QR en formato base64. 
        /// El método construye el mensaje de correo, adjunta la imagen del código QR, y utiliza el servidor SMTP configurado para enviar el correo al destinatario.
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="qrBase64"></param>
        /// <returns></returns>
        public async Task SendQrEmail(string toEmail, string qrBase64)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var port = int.Parse(_config["EmailSettings:Port"]);
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var senderName = _config["EmailSettings:SenderName"];
            var password = _config["EmailSettings:Password"];

            var message = new MailMessage();
            message.From = new MailAddress(senderEmail, senderName);
            message.To.Add(toEmail);
            message.Subject = "Tu ticket de acceso al evento";

            // Convertir base64 a bytes
            byte[] qrBytes = Convert.FromBase64String(qrBase64);
            using var ms = new MemoryStream(qrBytes);
            var qrImage = new LinkedResource(ms, "image/png")
            {
                ContentId = "QrCodeImage"
            };

            string html = $@"
                <h2>Tu entrada al evento</h2>
                <p>Presenta este código QR en la entrada.</p>
                <img src='cid:QrCodeImage' />
            ";

            var htmlView = AlternateView.CreateAlternateViewFromString(html, null, "text/html");
            htmlView.LinkedResources.Add(qrImage);

            message.AlternateViews.Add(htmlView);
            // message.Body = html;
            // message.IsBodyHtml = true;

            var smtp = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }
    }
}
