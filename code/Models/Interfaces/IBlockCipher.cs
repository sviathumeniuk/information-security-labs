using System;

namespace code.Models;

public interface IBlockCipher
{
    void EncryptBlock(ReadOnlySpan<byte> plain, Span<byte> cipher);
    void DecryptBlock(ReadOnlySpan<byte> cipher, Span<byte> plain);
    byte[] EncryptCBC(byte[] plain, byte[] iv);
    byte[] DecryptCBC(byte[] cipher, byte[] iv);
}