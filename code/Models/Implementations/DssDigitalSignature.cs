using System;
using System.Security.Cryptography;

namespace code.Models;

public class DssDigitalSignature : IDigitalSignature
{
    private const int KeySize = 2048;

    public (byte[] publicKey, byte[] privateKey) GenerateKeys()
    {
        using var dsa = DSA.Create(KeySize);
        
        var privateKeyData = dsa.ExportParameters(true);
        var publicKeyData = dsa.ExportParameters(false);
        
        var privateKeyXml = ParametersToXml(privateKeyData, true);
        var publicKeyXml = ParametersToXml(publicKeyData, false);
        
        return (System.Text.Encoding.UTF8.GetBytes(publicKeyXml), 
                System.Text.Encoding.UTF8.GetBytes(privateKeyXml));
    }

    public byte[] SignData(byte[] data, byte[] privateKey)
    {
        using var dsa = DSA.Create();
        var keyXml = System.Text.Encoding.UTF8.GetString(privateKey);
        var parameters = XmlToParameters(keyXml);
        dsa.ImportParameters(parameters);
        
        return dsa.SignData(data, HashAlgorithmName.SHA256);
    }

    public bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey)
    {
        using var dsa = DSA.Create();
        var keyXml = System.Text.Encoding.UTF8.GetString(publicKey);
        var parameters = XmlToParameters(keyXml);
        dsa.ImportParameters(parameters);
        
        return dsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
    }

    public string ExportPublicKey(byte[] publicKey)
    {
        return System.Text.Encoding.UTF8.GetString(publicKey);
    }

    public string ExportPrivateKey(byte[] privateKey)
    {
        return System.Text.Encoding.UTF8.GetString(privateKey);
    }

    public byte[] ImportPublicKey(string keyXml)
    {
        return System.Text.Encoding.UTF8.GetBytes(keyXml);
    }

    public byte[] ImportPrivateKey(string keyXml)
    {
        return System.Text.Encoding.UTF8.GetBytes(keyXml);
    }

    private static string ParametersToXml(DSAParameters parameters, bool includePrivate)
    {
        var xml = "<DSAKeyValue>";
        xml += $"<P>{Convert.ToBase64String(parameters.P!)}</P>";
        xml += $"<Q>{Convert.ToBase64String(parameters.Q!)}</Q>";
        xml += $"<G>{Convert.ToBase64String(parameters.G!)}</G>";
        xml += $"<Y>{Convert.ToBase64String(parameters.Y!)}</Y>";
        
        if (includePrivate && parameters.X != null)
        {
            xml += $"<X>{Convert.ToBase64String(parameters.X)}</X>";
        }
        
        xml += "</DSAKeyValue>";
        return xml;
    }

    private static DSAParameters XmlToParameters(string xml)
    {
        var parameters = new DSAParameters();
        
        parameters.P = ExtractBase64Value(xml, "P");
        parameters.Q = ExtractBase64Value(xml, "Q");
        parameters.G = ExtractBase64Value(xml, "G");
        parameters.Y = ExtractBase64Value(xml, "Y");
        
        if (xml.Contains("<X>"))
        {
            parameters.X = ExtractBase64Value(xml, "X");
        }
        
        return parameters;
    }

    private static byte[] ExtractBase64Value(string xml, string tagName)
    {
        var startTag = $"<{tagName}>";
        var endTag = $"</{tagName}>";
        
        var startIndex = xml.IndexOf(startTag) + startTag.Length;
        var endIndex = xml.IndexOf(endTag);
        
        var base64Value = xml.Substring(startIndex, endIndex - startIndex);
        return Convert.FromBase64String(base64Value);
    }
}