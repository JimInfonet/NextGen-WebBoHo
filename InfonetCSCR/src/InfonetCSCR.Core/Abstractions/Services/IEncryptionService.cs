namespace InfonetCSCR.Core.Abstractions.Services;

public interface IEncryptionService
{
    public string EncryptText(string text);
    public string DecryptText(string text);
}
