namespace code.Models;

public interface IDigitalSignature
{
    (byte[] publicKey, byte[] privateKey) GenerateKeys();
    byte[] SignData(byte[] data, byte[] privateKey);
    bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey);
    string ExportPublicKey(byte[] publicKey);
    string ExportPrivateKey(byte[] privateKey);
    byte[] ImportPublicKey(string keyXml);
    byte[] ImportPrivateKey(string keyXml);
}