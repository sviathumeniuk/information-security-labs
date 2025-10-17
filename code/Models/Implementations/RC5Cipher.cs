using System;

namespace code.Models;

public class RC5Cipher : IBlockCipher
{
    private readonly uint[] S;
    private readonly int r;
    private readonly int w;
    private readonly uint mask;
    private readonly int u;
    private readonly uint P;
    private readonly uint Q;

    public RC5Cipher(byte[] key, int rounds = 12, int wordSize = 32)
    {
        r = rounds;
        w = wordSize;

        u = w / 8;
        mask = (w == 32) ? 0xFFFFFFFFu : 0xFFFFu;

        if (w == 32)
        {
            P = 0xB7E15163u;
            Q = 0x9E3779B9u;
        }
        else
        {
            P = 0xB7E1u;
            Q = 0x9E37u;
        }

        int b = key.Length;
        uint[] L = new uint[(b + u - 1) / u];
        for (int i = 0; i < b; i++)
        {
            L[i / u] |= (uint)key[i] << (8 * (i % u));
        }

        S = new uint[2 * (r + 1)];
        S[0] = P & mask;
        for (int i = 1; i < S.Length; i++)
        {
            S[i] = (S[i - 1] + Q) & mask;
        }

        int i_idx = 0, j = 0;
        uint A = 0, B = 0;
        int v = 3 * Math.Max(L.Length, S.Length);
        for (int k = 0; k < v; k++)
        {
            A = S[i_idx] = RotateLeft((S[i_idx] + A + B) & mask, 3);
            B = L[j] = RotateLeft((L[j] + A + B) & mask, (int)((A + B) & ((uint)w - 1)));
            i_idx = (i_idx + 1) % S.Length;
            j = (j + 1) % L.Length;
        }
    }

    private uint RotateLeft(uint value, int shift)
    {
        if (w == 32)
        {
            int s = shift & 31;
            return (value << s) | (value >> (32 - s));
        }
        else
        {
            int s = shift & 15;
            uint v = (value & mask);
            return ((v << s) | (v >> (16 - s))) & mask;
        }
    }

    private uint RotateRight(uint value, int shift)
    {
        if (w == 32)
        {
            int s = shift & 31;
            return (value >> s) | (value << (32 - s));
        }
        else
        {
            int s = shift & 15;
            uint v = (value & mask);
            return ((v >> s) | (v << (16 - s))) & mask;
        }
    }

    private uint ReadWord(ReadOnlySpan<byte> span, int offset)
    {
        uint val = 0;
        for (int i = 0; i < u; i++)
            val |= (uint)span[offset + i] << (8 * i);
        return val & mask;
    }

    private void WriteWord(Span<byte> span, int offset, uint val)
    {
        uint v = val & mask;
        for (int i = 0; i < u; i++)
            span[offset + i] = (byte)((v >> (8 * i)) & 0xFF);
    }

    public void EncryptBlock(ReadOnlySpan<byte> plain, Span<byte> cipher)
    {
        uint A = ReadWord(plain, 0);
        uint B = ReadWord(plain, u);

        A = (A + S[0]) & mask;
        B = (B + S[1]) & mask;

        for (int i = 1; i <= r; i++)
        {
            A = (RotateLeft(A ^ B, (int)B) + S[2 * i]) & mask;
            B = (RotateLeft(B ^ A, (int)A) + S[2 * i + 1]) & mask;
        }

        WriteWord(cipher, 0, A);
        WriteWord(cipher, u, B);
    }

    public void DecryptBlock(ReadOnlySpan<byte> cipher, Span<byte> plain)
    {
        uint A = ReadWord(cipher, 0);
        uint B = ReadWord(cipher, u);

        for (int i = r; i >= 1; i--)
        {
            B = RotateRight((B - S[2 * i + 1]) & mask, (int)A) ^ A;
            A = RotateRight((A - S[2 * i]) & mask, (int)B) ^ B;
        }

        B = (B - S[1]) & mask;
        A = (A - S[0]) & mask;

        WriteWord(plain, 0, A);
        WriteWord(plain, u, B);
    }

    public byte[] EncryptCBC(byte[] plain, byte[] iv)
    {
        int blockSize = 2 * u;
        byte[] padded = AddPadding(plain, blockSize);
        byte[] result = new byte[padded.Length];
        byte[] previous = iv;

        for (int i = 0; i < padded.Length; i += blockSize)
        {
            byte[] block = new byte[blockSize];
            
            for (int j = 0; j < blockSize; j++)
            {
                block[j] = (byte)(padded[i + j] ^ previous[j]);
            }

            EncryptBlock(block, result.AsSpan(i, blockSize));
            previous = result.AsSpan(i, blockSize).ToArray();
        }

        return result;
    }

    public byte[] DecryptCBC(byte[] cipher, byte[] iv)
    {
        int blockSize = 2 * u;
        byte[] result = new byte[cipher.Length];
        byte[] previous = iv;

        for (int i = 0; i < cipher.Length; i += blockSize)
        {
            byte[] temp = new byte[blockSize];
            DecryptBlock(cipher.AsSpan(i, blockSize), temp);
            
            for (int j = 0; j < blockSize; j++)
            {
                result[i + j] = (byte)(temp[j] ^ previous[j]);
            }

            previous = cipher.AsSpan(i, blockSize).ToArray();
        }

        return RemovePadding(result, blockSize);
    }

    private static byte[] AddPadding(byte[] data, int blockSize)
    {
        int padding = blockSize - (data.Length % blockSize);

        if (padding == 0)
        {
            padding = blockSize;
        }

        byte[] padded = new byte[data.Length + padding];
       
        data.CopyTo(padded, 0);
        for (int i = data.Length; i < padded.Length; i++)
        {
            padded[i] = (byte)padding;
        }

        return padded;
    }

    private static byte[] RemovePadding(byte[] data, int blockSize)
    {
        if (data.Length == 0)
        {
            return data;
        }

        int padding = data[data.Length - 1];
        if (padding <= 0 || padding > blockSize)
        {
            return data;
        }

        return data.AsSpan(0, data.Length - padding).ToArray();
    }
}