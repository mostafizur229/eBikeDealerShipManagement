using System.Security.Cryptography;
using System.Text;
using eBike.Application.Common.Interfaces;

namespace eBike.Infrastructure.Services;

public class CryptographyService : ICryptographyService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Pbkdf2Iterations = 100_000;

    /// <summary>
    /// Hashes password in modern ASP.NET Identity compatible format
    /// Format: [0x01] [4-byte iteration count] [4-byte salt size] [salt] [hash]
    /// </summary>
    public (string Hash, string Salt) CreatePasswordHash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: Pbkdf2Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: HashSize);

        // Version 0x01 (modern Identity format)
        byte[] output = new byte[13 + SaltSize + HashSize];
        output[0] = 0x01; // Format marker
        WriteNetworkByteOrder(output, 1, (uint)HashAlgorithmName.SHA256.Name!.GetHashCode());
        WriteNetworkByteOrder(output, 5, (uint)Pbkdf2Iterations);
        WriteNetworkByteOrder(output, 9, (uint)SaltSize);
        Buffer.BlockCopy(salt, 0, output, 13, SaltSize);
        Buffer.BlockCopy(hash, 0, output, 13 + SaltSize, HashSize);

        return (Convert.ToBase64String(output), Convert.ToBase64String(salt));
    }

    /// <summary>
    /// Verifies password against either:
    /// 1. Legacy ASP.NET Identity v2 format (0x00, 16 salt, 32 subkey, 1000 iter HMAC-SHA1) - used by existing DB users!
    /// 2. Modern ASP.NET Identity v3 format (0x01)
    /// 3. Standard Base64 salt+hash format
    /// </summary>
    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        try
        {
            byte[] decoded = Convert.FromBase64String(storedHash);

            // Format check
            if (decoded.Length == 49 && decoded[0] == 0x00)
            {
                // ASP.NET Identity v2 (Existing DB Users in ebikeimsweb)
                return VerifyIdentityV2(decoded, password);
            }
            else if (decoded.Length > 13 && decoded[0] == 0x01)
            {
                // ASP.NET Identity v3
                return VerifyIdentityV3(decoded, password);
            }
            else if (!string.IsNullOrEmpty(storedSalt))
            {
                // Fallback custom salt+hash
                byte[] saltBytes = Convert.FromBase64String(storedSalt);
                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password: Encoding.UTF8.GetBytes(password),
                    salt: saltBytes,
                    iterations: 100_000,
                    hashAlgorithm: HashAlgorithmName.SHA256,
                    outputLength: HashSize);

                return CryptographicOperations.FixedTimeEquals(actualHash, decoded);
            }
        }
        catch
        {
            // If base64 decode fails or plain text match
            return password == storedHash;
        }

        return false;
    }

    private static bool VerifyIdentityV2(byte[] decoded, string password)
    {
        byte[] salt = new byte[16];
        Buffer.BlockCopy(decoded, 1, salt, 0, 16);

        byte[] subkey = new byte[32];
        Buffer.BlockCopy(decoded, 17, subkey, 0, 32);

        byte[] generatedSubkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, 1000, HashAlgorithmName.SHA1, 32);

        return CryptographicOperations.FixedTimeEquals(subkey, generatedSubkey);
    }

    private static bool VerifyIdentityV3(byte[] decoded, string password)
    {
        int iterCount = (int)ReadNetworkByteOrder(decoded, 5);
        int saltLength = (int)ReadNetworkByteOrder(decoded, 9);

        if (saltLength < 16) return false;

        byte[] salt = new byte[saltLength];
        Buffer.BlockCopy(decoded, 13, salt, 0, saltLength);

        int subkeyLength = decoded.Length - 13 - saltLength;
        byte[] expectedSubkey = new byte[subkeyLength];
        Buffer.BlockCopy(decoded, 13 + saltLength, expectedSubkey, 0, subkeyLength);

        byte[] actualSubkey = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: iterCount,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: subkeyLength);

        return CryptographicOperations.FixedTimeEquals(expectedSubkey, actualSubkey);
    }

    private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }

    private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
    {
        return ((uint)(buffer[offset + 0]) << 24)
            | ((uint)(buffer[offset + 1]) << 16)
            | ((uint)(buffer[offset + 2]) << 8)
            | ((uint)(buffer[offset + 3]));
    }
}
