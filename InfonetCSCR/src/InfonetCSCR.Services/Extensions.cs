namespace InfonetCSCR.Services;

public static class Extensions
{
  public static double Round(this double number, int digits = 0)
  {
    return Math.Round(number, digits, MidpointRounding.AwayFromZero);
  }

  public static decimal Round(this decimal number, int digits = 0)
  {
    return Math.Round(number, digits, MidpointRounding.AwayFromZero);
  }

  public static int ConvertToIntOrZero(this string input)
  {
    input = input.Trim();

    if (int.TryParse(input, out int result))
    {
      return result;
    }
    else
    {
      return 0;
    }
  }
}
