using QRCoder;

namespace EventAccessControl.API.Services
{
    public class QRService
    {
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