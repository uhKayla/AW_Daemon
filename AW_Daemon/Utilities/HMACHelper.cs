namespace AW_Daemon.Utilities;

/// <summary>
/// Tools for generating HMAC signatures.
/// </summary>
public class HMACHelper
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Generates our HMAC signature.
    /// </summary>
    public static string ComputeHMACSHA256Signature(string message, string secretKey)
    {
        var encoding = new System.Text.UTF8Encoding();
        var keyByte = encoding.GetBytes(secretKey);
        var messageBytes = encoding.GetBytes(message);
        using var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyByte);
        var hashmessage = hmacsha256.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashmessage);
    }
}