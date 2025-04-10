namespace InfonetCSCR.Web.Helpers;

public static class IniFileReader
{
  [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
  public static extern int GetPrivateProfileString(
      string lpSectionName,
      string lpKeyName,
      string lpDefault,
      StringBuilder lpReturnedString,
      int nSize,
      string lpFileName);

  public static string ReadFromFile(string section, string key, string fileName)
  {
    StringBuilder strReturn = new StringBuilder(255);
    int strLen = GetPrivateProfileString(section, key, "", strReturn, 255, fileName);
    return strReturn.ToString().Substring(0, strLen);
  }
}

