namespace code.Models;

public interface IMD5Hasher
{
    string ComputeHash(string input);
    string ComputeFileHash(string filePath);
    bool VerifyFileHash(string filePath, string expectedHash);

}