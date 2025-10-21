using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace code.Models;

public sealed class RsaCipher : IRsaCipher
{
    private readonly RSA _rsa = RSA.Create();

    public void GenerateKeys(int keySizeInBits = 4096)
    {
        _rsa.KeySize = keySizeInBits;
    }

    public void SavePublicKey(string filePath)
    {
        var publicKey = _rsa.ExportSubjectPublicKeyInfo();
        File.WriteAllText(filePath, PemEncoding.Write("PUBLIC KEY", publicKey));
    }

    public void SavePrivateKey(string filePath)
    {
        var privateKey = _rsa.ExportPkcs8PrivateKey();
        File.WriteAllText(filePath, PemEncoding.Write("PRIVATE KEY", privateKey));
    }

    public void LoadPublicKey(string filePath)
    {
        var pem = File.ReadAllText(filePath);
        using var tempRsa = RSA.Create();
        tempRsa.ImportFromPem(pem);
        var publicParams = tempRsa.ExportParameters(false);
        _rsa.ImportParameters(publicParams);
    }

    public void LoadPrivateKey(string filePath)
    {
        var pem = File.ReadAllText(filePath);
        _rsa.ImportFromPem(pem);
    }

    public void EncryptFile(string inputFilePath, string outputFilePath)
    {
        var keySizeInBytes = (_rsa.KeySize + 7) / 8;
        const int hashSizeInBytes = 32;
        var maxBlockSize = keySizeInBytes - (2 * hashSizeInBytes) - 2;

        using var inputStream = File.OpenRead(inputFilePath);
        using var outputStream = File.Create(outputFilePath);
        using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);

        var buffer = new byte[maxBlockSize];
        int bytesRead;

        while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            var block = new byte[bytesRead];
            Array.Copy(buffer, block, bytesRead);

            var encryptedBlock = _rsa.Encrypt(block, RSAEncryptionPadding.OaepSHA256);
            writer.Write(encryptedBlock.Length);
            writer.Write(encryptedBlock);
        }

        writer.Flush();
    }

    public void DecryptFile(string inputFilePath, string outputFilePath)
    {
        using var inputStream = File.OpenRead(inputFilePath);
        using var reader = new BinaryReader(inputStream, Encoding.UTF8, leaveOpen: true);
        using var outputStream = File.Create(outputFilePath);

        while (inputStream.Position < inputStream.Length)
        {
            var blockLength = reader.ReadInt32();
            var encryptedBlock = reader.ReadBytes(blockLength);

            var decryptedBlock = _rsa.Decrypt(encryptedBlock, RSAEncryptionPadding.OaepSHA256);
            outputStream.Write(decryptedBlock, 0, decryptedBlock.Length);
        }
    }
}