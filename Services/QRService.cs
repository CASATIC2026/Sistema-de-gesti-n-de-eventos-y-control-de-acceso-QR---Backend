using QRCoder;

namespace EventAccessControl.API.Services
{   
    /// <summary>
    /// Servicio para generar códigos QR en formato base64.
    /// </summary>
    public class QRService
    {
        /// <summary>
        /// Método para generar un código QR a partir de un texto dado y devolverlo en formato base64. Este método utiliza la biblioteca QRCoder para crear el código QR, lo 
        /// convierte a un arreglo de bytes, y luego lo codifica en base64 para que pueda ser fácilmente enviado por correo electrónico o utilizado en otras partes de la aplicación.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string GenerateQRCodeBase64(string text)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new BitmapByteQRCode(qrCodeData))
                {
                    byte[] qrBytes = qrCode.GetGraphic(20);
                    return Convert.ToBase64String(qrBytes);
                }
            }
        }
    }
}