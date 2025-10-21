using System.Security.Cryptography;

namespace code.Models;

public interface IRsaCipher
{
    void GenerateKeys(int keySizeInBits = 4096);

    void SavePublicKey(string filePath);
    void SavePrivateKey(string filePath);

    void LoadPublicKey(string filePath);
    void LoadPrivateKey(string filePath);

    void EncryptFile(string inputFilePath, string outputFilePath);
    void DecryptFile(string inputFilePath, string outputFilePath);
}