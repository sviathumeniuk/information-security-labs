using System;
using System.Data;
using System.IO;
using System.Text;
using Avalonia.Media;

namespace code.Models;

public class MD5Hasher : IMD5Hasher
{
    private uint A, B, C, D;
    private readonly byte[] _buffer = new byte[64];
    private int _bufferLength;
    private ulong _bitCount;

    private static readonly uint[] K = new uint[64] {
        0xd76aa478,0xe8c7b756,0x242070db,0xc1bdceee,0xf57c0faf,0x4787c62a,0xa8304613,0xfd469501,
        0x698098d8,0x8b44f7af,0xffff5bb1,0x895cd7be,0x6b901122,0xfd987193,0xa679438e,0x49b40821,
        0xf61e2562,0xc040b340,0x265e5a51,0xe9b6c7aa,0xd62f105d,0x02441453,0xd8a1e681,0xe7d3fbc8,
        0x21e1cde6,0xc33707d6,0xf4d50d87,0x455a14ed,0xa9e3e905,0xfcefa3f8,0x676f02d9,0x8d2a4c8a,
        0xfffa3942,0x8771f681,0x6d9d6122,0xfde5380c,0xa4beea44,0x4bdecfa9,0xf6bb4b60,0xbebfbc70,
        0x289b7ec6,0xeaa127fa,0xd4ef3085,0x04881d05,0xd9d4d039,0xe6db99e5,0x1fa27cf8,0xc4ac5665,
        0xf4292244,0x432aff97,0xab9423a7,0xfc93a039,0x655b59c3,0x8f0ccc92,0xffeff47d,0x85845dd1,
        0x6fa87e4f,0xfe2ce6e0,0xa3014314,0x4e0811a1,0xf7537e82,0xbd3af235,0x2ad7d2bb,0xeb86d391
    };

    private static readonly int[] S = new int[64] {
         7,12,17,22, 7,12,17,22, 7,12,17,22, 7,12,17,22,
         5, 9,14,20, 5, 9,14,20, 5, 9,14,20, 5, 9,14,20,
         4,11,16,23, 4,11,16,23, 4,11,16,23, 4,11,16,23,
         6,10,15,21, 6,10,15,21, 6,10,15,21, 6,10,15,21
    };

    public MD5Hasher()
    {
        Initialize();
    }

    private void Initialize()
    {
        A = 0x67452301;
        B = 0xefcdab89;
        C = 0x98badcfe;
        D = 0x10325476;
        _bufferLength = 0;
        _bitCount = 0;
    }

    public string ComputeHash(string input)
    {
        Initialize();
        var bytes = Encoding.UTF8.GetBytes(input);
        Update(bytes, 0, bytes.Length);
        var digest = FinalizeHash();

        return BytesToHex(digest);
    }

    public string ComputeFileHash(string filePath)
    {
        Initialize();
        const int chunkSize = 8192;
        var buffer = new byte[chunkSize];
        using var fs = File.OpenRead(filePath);
        int read;
        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
        {
            Update(buffer, 0, read);
        }
        var digest = FinalizeHash();

        return BytesToHex(digest);
    }

    public bool VerifyFileHash(string filePath, string expectedHash)
    {
        var actual = ComputeFileHash(filePath);
        return string.Equals(actual.Trim(), expectedHash.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private void Update(byte[] input, int offset, int length)
    {
        _bitCount += (ulong)length * 8UL;

        if (_bufferLength > 0)
        {
            int need = 64 - _bufferLength;
            if (length < need)
            {
                Array.Copy(input, offset, _buffer, _bufferLength, length);
                _bufferLength += length;
                return;
            }
            Array.Copy(input, offset, _buffer, _bufferLength, need);
            Transform(_buffer, 0);
            offset += need;
            length -= need;
            _bufferLength = 0;
        }

        while (length >= 64)
        {
            Transform(input, offset);
            offset += 64;
            length -= 64;
        }

        if (length > 0)
        {
            Array.Copy(input, offset, _buffer, 0, length);
            _bufferLength = length;
        }
    }

    private byte[] FinalizeHash()
    {
        ulong originalBitCount = _bitCount;

        byte[] padding = new byte[64];
        padding[0] = 0x80;

        int padLength = (_bufferLength < 56) ? (56 - _bufferLength) : (120 - _bufferLength);
        Update(padding, 0, padLength);

        byte[] lengthBytes = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            lengthBytes[i] = (byte)((originalBitCount >> (i * 8)) & 0xff);
        }
        Update(lengthBytes, 0, 8);

        var digest = new byte[16];
        WriteUIntToBytes(A, digest, 0);
        WriteUIntToBytes(B, digest, 4);
        WriteUIntToBytes(C, digest, 8);
        WriteUIntToBytes(D, digest, 12);
        return digest;
    }

    private static void WriteUIntToBytes(uint value, byte[] bytes, int offset)
    {
        bytes[offset] = (byte)(value & 0xff);
        bytes[offset + 1] = (byte)((value >> 8) & 0xff);
        bytes[offset + 2] = (byte)((value >> 16) & 0xff);
        bytes[offset + 3] = (byte)((value >> 24) & 0xff);
    }

    private static string BytesToHex(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }

    private void Transform(byte[] block, int offset)
    {
        uint a = A, b = B, c = C, d = D;
        uint[] M = new uint[16];

        for (int i = 0; i < 16; i++)
        {
            M[i] = (uint)(block[offset + i * 4] |
                         (block[offset + i * 4 + 1] << 8) |
                         (block[offset + i * 4 + 2] << 16) |
                         (block[offset + i * 4 + 3] << 24));
        }

        for (int i = 0; i < 64; i++)
        {
            uint f, g;
            if (i < 16)
            {
                f = (b & c) | (~b & d);
                g = (uint)i;
            }
            else if (i < 32)
            {
                f = (d & b) | (~d & c);
                g = (uint)(5 * i + 1) % 16;
            }
            else if (i < 48)
            {
                f = b ^ c ^ d;
                g = (uint)(3 * i + 5) % 16;
            }
            else
            {
                f = c ^ (b | ~d);
                g = (uint)(7 * i) % 16;
            }

            uint temp = d;
            d = c;
            c = b;
            b += LeftRotate(a + f + K[i] + M[g], S[i]);
            a = temp;
        }

        A += a;
        B += b;
        C += c;
        D += d;
    }

    private static uint LeftRotate(uint x, int n) => (x << n) | (x >> (32 - n));
}