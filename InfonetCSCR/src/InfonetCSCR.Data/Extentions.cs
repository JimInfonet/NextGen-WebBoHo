namespace InfonetCSCR.Data;

public static class Extentions
{
  public static string? ToEncodedSqlString(this string? str)
  {
    if (str == null) return str;
    str = str.Replace("'", "''");
    return str;
  }
}
