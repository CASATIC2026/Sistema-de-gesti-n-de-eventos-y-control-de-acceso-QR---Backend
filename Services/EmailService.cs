using System.Net;
using System.Net.Mail;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace EventAccessControl.API.Services
{
    /// <summary>
    /// Servicio para enviar correos electrónicos con códigos QR adjuntos.
    /// </summary>
    public class EmailService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructor que recibe la configuración de la aplicación para obtener los parámetros necesarios para enviar correos electrónicos, como el servidor SMTP, el puerto, el correo electrónico de origen, y la contraseña.
        /// </summary>
        /// <param name="config"></param>
        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Método para enviar un correo electrónico con un código QR adjunto. Recibe el correo electrónico de destino y el código QR en formato base64 como parámetros. El método construye un mensaje de correo electrónico con el código QR adjunto y lo envía utilizando el servidor SMTP configurado.
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="qrBase64"></param>
        /// <param name="eventName"></param>
        /// <param name="eventDate"></param>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task SendQrEmail(string toEmail, string qrBase64, string eventName, DateTimeOffset startDateTime, DateTimeOffset endDateTime, Guid ticketId)
        {
            try
            {
                var smtpServer = _config["EmailSettings:SmtpServer"]
            ?? throw new InvalidOperationException("SmtpServer no configurado.");

                if (!int.TryParse(_config["EmailSettings:Port"], out int port))
                    throw new InvalidOperationException("Puerto SMTP inválido.");

                var senderEmail = _config["EmailSettings:SenderEmail"]
                    ?? throw new InvalidOperationException("SenderEmail no configurado.");

                var senderName = _config["EmailSettings:SenderName"] ?? "Event Access";
                var password = _config["EmailSettings:Password"]
                    ?? throw new InvalidOperationException("Password no configurado.");

                var enableSsl = bool.Parse(_config["EmailSettings:EnableSsl"] ?? "true");

                // Convertir QR
                byte[] qrBytes = Convert.FromBase64String(qrBase64);

                using var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Tu ticket de acceso al evento",
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                // HTML con fallback
                //                string html = $@"
                //                <h2>Tu entrada al evento</h2>
                //                <p>
                //                <strong>{WebUtility.HtmlEncode(eventName)}</strong><br/>
                //                Inicio: {startDateTime:dd/MM/yyyy HH:mm}<br/>
                //                Fin: {endDateTime:dd/MM/yyyy HH:mm}
                //                </p>
                //                <p>Ticket: {ticketId}</p>
                //                <p>Presenta este código QR en la entrada.</p>
                //                <p><b>Si no ves la imagen, revisa el PDF adjunto.</b></p>
                //<img src='cid:QrCodeImage' width='200' height='200' style='max-width:200px; height:auto;' />
                //<br/>
                //<p>
                //Software con fines educativos <br/>
                //<strong>Powered By: Eileen Alessia Dimas  eilale3710@gmail.com</strong>
                //</p>
                //            ";
                string html = $@"
                    <div style='font-family: Arial, sans-serif; color:#333;'>
                        <h2 style='color:#2c3e50; margin-bottom:10px;'>Tu entrada al evento</h2>
        
                        <p style='margin:0 0 10px 0;'>
                            <strong style='font-size:16px;'>{WebUtility.HtmlEncode(eventName)}</strong><br/>
                            <span style='color:#555;'>Inicio:</span> {startDateTime:dd/MM/yyyy HH:mm}<br/>
                            <span style='color:#555;'>Fin:</span> {endDateTime:dd/MM/yyyy HH:mm}
                        </p>
        
                        <p style='margin:0 0 10px 0;'>
                            <strong>Ticket:</strong> {ticketId}
                        </p>
        
                        <p style='margin:0 0 15px 0;'>
                            Presenta este código QR en la entrada.
                        </p>
        
                        <p style='margin:0 0 10px 0; font-style:italic; color:#777;'>
                            <b>Si no ves la imagen, revisa el PDF adjunto.</b>
                        </p>
        
                        <div style='text-align:center; margin:20px 0;'>
                            <img src='cid:QrCodeImage' width='200' height='200' 
                                 style='max-width:200px; height:auto; border:1px solid #ccc; padding:5px;' 
                                 alt='Código QR' />
                        </div>
        
                        <hr style='border:none; border-top:1px solid #ddd; margin:20px 0;'/>
        
                        <p style='font-size:12px; color:#555; text-align:center;'>
                            Software con fines educativos <br/>
                            <strong>Powered By: Eileen Alessia Dimas – eilale3710@gmail.com</strong>
                        </p>
                    </div>
                ";


                // Imagen embebida
                var qrStream = new MemoryStream(qrBytes);
                var qrImage = new LinkedResource(qrStream, "image/png")
                {
                    ContentId = "QrCodeImage"
                };

                var htmlView = AlternateView.CreateAlternateViewFromString(html, null, "text/html");
                htmlView.LinkedResources.Add(qrImage);
                message.AlternateViews.Add(htmlView);

                //  PDF seguro (NO rompe flujo)
                Attachment? pdfAttachment = null;

                try
                {
                    var pdfStream = new MemoryStream();

                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(20);

                            page.Content().Padding(10).Column(col =>
                            {
                                col.Spacing(10);

                                col.Item().Text("Ticket de Evento")
                                    .FontSize(20).Bold();

                                col.Item().LineHorizontal(1);

                                col.Item().Row(row =>
                                {
                                    row.ConstantColumn(150).Image(qrBytes); //  FIX

                                    row.RelativeColumn().Column(c2 =>
                                    {
                                        c2.Spacing(5);
                                        c2.Item().Text(eventName).FontSize(16).Bold();
                                        //c2.Item().Text($"Fecha: {eventDate}");
                                        c2.Item().Text($"Inicio: {startDateTime:dd/MM/yyyy HH:mm}");
                                        c2.Item().Text($"Fin: {endDateTime:dd/MM/yyyy HH:mm}");
                                        c2.Item().Text($"Ticket: {ticketId}");
                                    });
                                });
                            });
                        });
                    }).GeneratePdf(pdfStream);

                    pdfStream.Position = 0;
                    pdfAttachment = new Attachment(pdfStream, $"ticket_{ticketId}.pdf", "application/pdf");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generando PDF: {ex.Message}");
                    // NO rompe el envío
                }

                if (pdfAttachment != null)
                    message.Attachments.Add(pdfAttachment);

                using var smtp = new SmtpClient(smtpServer, port)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = enableSsl
                };

                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Enviando el Email: {ex.Message}");
                throw;
            }
        }

        public async Task SendPasswordResetEmail(
    string toEmail,
    string resetLink)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"]
                ?? throw new InvalidOperationException("SmtpServer no configurado.");

            if (!int.TryParse(_config["EmailSettings:Port"], out int port))
                throw new InvalidOperationException("Puerto SMTP inválido.");

            var senderEmail = _config["EmailSettings:SenderEmail"]
                ?? throw new InvalidOperationException("SenderEmail no configurado.");

            var senderName = _config["EmailSettings:SenderName"] ?? "Event Access";

            var password = _config["EmailSettings:Password"]
                ?? throw new InvalidOperationException("Password no configurado.");

            var enableSsl = bool.Parse(
                _config["EmailSettings:EnableSsl"] ?? "true"
            );

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "Recuperación de contraseña",
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            string html = $@"
        <h2>Recuperación de contraseña</h2>

        <p>
            Haz clic en el siguiente enlace para
            restablecer tu contraseña:
        </p>

        <p>
            <a href='{resetLink}'>
                Restablecer contraseña
            </a>
        </p>

        <br/>

        <p>
            Si no solicitaste este cambio,
            puedes ignorar este correo.
        </p>
    ";

            message.Body = html;

            using var smtp = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(
                    senderEmail,
                    password
                ),
                EnableSsl = enableSsl
            };

            await smtp.SendMailAsync(message);
        }

    }
}