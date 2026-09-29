using QRCoder;

namespace SzApp.Domain.Billing.IpsQr;

/// <summary>
/// Renders the IPS QR payload to a PNG image. Uses QRCoder's pure-managed PNG renderer
/// (no System.Drawing/GDI, no external exe — replaces the legacy shell-out to a QR generator exe).
/// </summary>
public static class IpsQrCodeGenerator
{
    public static byte[] GeneratePng(IpsQrInput input, int pixelsPerModule = 6)
    {
        var payload = IpsQrPayloadBuilder.Build(input);
        using var generator = new QRCodeGenerator();
        // NBS IPS QR uses UTF-8 payloads with ECC level M; ISO-8859-1 byte mode is a legacy artefact
        // of some encoders and not required here.
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(pixelsPerModule);
    }
}
