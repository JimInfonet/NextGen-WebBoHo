namespace InfonetCSCR.Web;

public static class Extentions
{
  public static string PadLeftFixed(this string? str, int length, char fillChar = ' ') // PadL
  {
    str ??= "";
    str = str.TrimStart();
    if (str.Length >= length)
    {
      return str.Substring(str.Length - length);
    }
    else
    {
      return str.PadLeft(length, fillChar);
    }
  }

  public static string PadRightFixed(this string? str, int length, char fillChar = ' ') // PadR
  {
    str ??= "";
    str = str.TrimStart();

    if (str.Length >= length)
    {
      str = str[..length]; // cut from right
    }
    else
    {
      str = str.PadRight(length, fillChar);
    }

    return str;
  }

  public static string FormatSigned(this string? str1, int num1, int num2 = 0)
  {
    int len1, n1, n2, mypos, mysign, Diff, diffn1 = 0;
    string npart, dpart;

    str1 ??= "";
    str1 = str1.Trim();
    mysign = str1.IndexOf("-"); // Checking -ve sign
    string Sign = "-";
    if (mysign == -1)
    {
      str1 = "+" + str1;
      mysign = str1.IndexOf("+"); // Checking +ve sign
      Sign = "+";
    }

    str1 = str1[1..]; // Removing sign

    len1 = str1.Length;
    mypos = str1.IndexOf("."); // Checking decimal point
                               // this if() wont be reachable as mysign wont be -1
    if (mysign == -1)
    {
      if (mypos == -1)  // For Integers
      {
        str1 = str1.PadLeftFixed(num1, '0');
        if (num2 > 0)
        {
          str1 = str1.PadRight(str1.Length + num2, '0');
        }

      }
      else // For Decimal Numbers
      {
        len1--;
        npart = str1[..mypos]; // Cutting only numeric part
        dpart = str1.Substring(mypos + 1, len1);

        npart = npart.PadLeftFixed(num1, '0');
        dpart = dpart.PadRightFixed(num2, '0');

        if (num2 > 0)
        {
          str1 = npart + dpart;
        }
        else
        {
          str1 = npart;
        }
      }
    }
    else // mysign = 0
    {
      if (mypos == -1)  // For Integers
      {
        if (num2 == 0) // No decimal format
        {
          if (num1 > len1)
          {
            str1 = str1.PadLeft(num1, '0');
          }
          else
          {
            Diff = len1 - num1;
            if (mysign == -1)
            {
              str1 = str1[Diff..];
            }
            else
            {
              str1 = str1[(Diff + 1)..];
            }
          }
        }
        else // Decimal format is needed
        {
          str1 = str1.PadLeftFixed(num1, '0');

          if (num2 > 0)
          {
            str1 = str1.PadRight(str1.Length + num2, '0');
          }
        }
      }
      else // For Decimal Numbers
      {
        len1--;
        npart = str1[..mypos]; // Cutting only numeric part
        dpart = str1[(mypos+1)..]; // mismatch with vba code
        n1 = npart.Length;
        n2 = dpart.Length;

        if (n1 < num1)
        {
          npart = npart.PadLeft(num1, '0');
        }
        else
        {
          if (mysign == -1)
          {
            diffn1 = n1 - num1;
            npart = npart[diffn1..];
          }
          else
          {
            npart = npart[(diffn1 + 1)..];
          }
        }

        if (n2 < num2)
        {
          dpart = dpart.PadRight(num2, '0');
        }
        else
        {
          dpart = dpart[..num2]; // same as substring
        }

        str1 = npart + (num2 > 0 ? dpart : "");
      }

      if (mysign > -1)
      {
        str1 = Sign + str1;
      }
    }

    return str1;
  }

  public static string FormatNumeric(this string? str1, int num1, int num2 = 0)
  {
    str1 ??= "";
    // If there is a negative sign, remove it
    int mypos = str1.IndexOf("-");
    if (mypos >= 0)
    {
      str1 = str1[1..];
    }

    str1 = str1.Trim();
    int len1 = str1.Length;
    mypos = str1.IndexOf(".");
    if (mypos == -1)  // For Integers
    {
      if (num2 == 0) // No decimal format
      {
        str1 = str1.PadLeftFixed(num1, '0');
      }
      else // Decimal format is needed
      {

        str1.PadLeftFixed(num1, '0');
        if (num2 > 0)
        {
          str1 = str1.PadRight(num1 + num2, '0');
        }
      }
    }
    else // For Decimal Numbers
    {
      len1--;
      string npart = str1[..mypos];
      string dpart = str1.Substring(mypos + 1, len1);
      npart.PadLeftFixed(num1, '0');
      dpart.PadRightFixed(num2, '0');

      if (num2 > 0)
      {
        str1 = npart + dpart;
      }
      else
      {
        str1 = npart;
      }
    }

    return str1;
  }

  public static string PadC(this string str, int length, char paddingChar = ' ') // Pads both left and right
  {
    if (length <= 0)
    {
      return string.Empty;
    }

    str = str.Trim();

    if (str.Length >= length)
    {
      return str.Substring(0, length);
    }

    int paddingLeft = (length - str.Length) / 2;
    str = str.PadLeft(paddingLeft + str.Length, paddingChar);
    str = str.PadRight(length, paddingChar);

    return str;
  }
}