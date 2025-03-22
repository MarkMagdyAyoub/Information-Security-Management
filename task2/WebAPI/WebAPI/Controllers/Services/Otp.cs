using OtpNet;
using QRCoder;
using WebAPI.Model.Entities;

namespace WebAPI.Controllers.Services;
public static class Otp
{
  /// <summary>
  /// The Number Of Digits In The Generated OTP (Default Is 6 Digits).
  /// </summary>
  private const int _size = 6;

  /// <summary>
  /// Regenerate New OTP Every 30 Sec
  /// </summary>
  private const int _step = 30;

  /// <summary>
  /// The hash algorithm used to generate the OTP (default is SHA1).
  /// </summary>
  private const OtpHashMode _hashMode = OtpHashMode.Sha1;

  /// <summary>
  /// Generate Random Secret Key Using KeyGeneration Class (Implemented In OtpNet).
  /// GenerateRandomKey() Function Takes The Hash Mode To Hash The Random Key
  /// And Return It As A Array Of Bytes
  /// So We Need To Convert It To String To Save It In The Database 
  /// (Base32Encoding.ToString() Resposible For This Task)
  /// </summary>
  /// <returns>
  ///  Base32 Encoded String 
  /// </returns>
  public static string SecretKey() => Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(_hashMode));

  /// <summary>
  /// Verifies a time-based one-time password (TOTP) against a given secret key.
  /// Allows a small time window before and after the current OTP for clock drift tolerance.
  /// </summary>
  /// <param name="key"> The Secret Key Base32 Encoded String</param>
  /// <param name="code">The OTP code to validate</param>
  /// <returns>
  /// Returns <c>true</c> if the OTP code is valid within the allowed time window, otherwise <c>false</c>.
  /// </returns>
  /// <remarks>
  /// new(1, 1): A time window for tolerance:
  /// The first 1 allows one code before the current time step (30s earlier).
  /// The second 1 allows one code after the current step (30s later).
  /// This helps prevent failures due to slight clock drift or user delay.
  /// </remarks>
  public static bool Verified(string key, string code) =>
    new OtpNet.Totp(Base32Encoding.ToBytes(key), _step, _hashMode, _size)
    .VerifyTotp(code , out _ , new(1,1));

  /// <summary>
  /// Generates an OTP URI compatible with authentication apps like Google Authenticator.
  /// This URI can be converted into a QR Code for easy setup.
  /// </summary>
  /// <param name="key">The Base32-encoded secret key.</param>
  /// <param name="identity">The user’s identity (email or username).</param>
  /// <param name="issuer">The name of the service</param>
  /// <returns>An OTP URI string that authentication apps can read</returns>
  public static string Uri(string key, string identity, string issuer) =>
    new OtpUri(OtpType.Totp, key, identity, issuer, _hashMode, _size, _size, _step).ToString();

  /// <summary>
  /// Generates a QR code representing an OTP URI and encodes it as a Base64 string.
  /// </summary>
  /// <param name="key">The Base32-encoded secret key.</param>
  /// <param name="identity">The user’s identity (e.g., email or username).</param>
  /// <param name="issuer">The name of the app or service issuing the OTP.</param>
  /// <returns>A Base64-encoded QR code image as a Data URL (JPEG format).</returns>
  /// <remarks>
  /// This function performs the following steps:
  /// 1. Generates the OTP URI based on the provided key, identity, and issuer.
  /// 2. Creates QR code data from the URI using error correction level Q (25% recovery).
  /// 3. Converts the QR code data into a PNG byte array with 20 pixels per module.
  /// 4. Encodes the PNG byte array into a Base64 string, suitable for embedding in HTML.
  /// </remarks>

  public static string QrCodeAsBase64(string key, string identity, string issuer) {
    string uri = Uri(key, identity, issuer);
    var qrGen = new QRCodeGenerator();
    var qrData = qrGen.CreateQrCode(uri , QRCodeGenerator.ECCLevel.Q);
    var pngByte = new PngByteQRCode(qrData).GetGraphic(20);
    string base64 = Convert.ToBase64String(pngByte);
    return $"base64:{base64}";
  }
}
