using QRCoder;

namespace EventAccessControl.API.Services
{   
    /// <summary>
    /// Servicio para generar códigos QR. Utiliza la biblioteca QRCoder para crear códigos QR
    /// </summary>
    public class QRService
    {   
        /// <summary>
        /// Genera un código QR en formato PNG a partir del texto proporcionado.
        /// </summary> 
        /// <param name="text"></param>
        /// <returns></returns>
        public byte[] GenerateQRCodePng(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("El contenido del QR no puede ser vacío.");

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);

            return qrCode.GetGraphic(20);
        }

        /// <summary>
        /// Genera un código QR en formato PNG a partir del texto proporcionado y devuelve el resultado en formato base64. 
        /// </summary>
        public string GenerateQRCodeBase64(string text)
        {
            var qrBytes = GenerateQRCodePng(text);
            return Convert.ToBase64String(qrBytes);
        }
    }
}