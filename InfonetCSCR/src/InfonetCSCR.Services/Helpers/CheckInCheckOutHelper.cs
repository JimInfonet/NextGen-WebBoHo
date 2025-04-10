namespace InfonetCSCR.Services.Helpers;

public static class CheckInCheckOutHelper
{
  public static double CalculateHours(TimeSpan timeIn, TimeSpan timeOut, int policyROUNDTIME)
  {
    TimeSpan timeDiff;

    if (timeOut >= timeIn)
    {
      // Regular span
      timeDiff = timeOut - timeIn;
    }
    else
    {
      TimeSpan partialHr = TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59) - timeIn;
      partialHr += TimeSpan.FromMinutes(1);
      timeDiff = partialHr + timeOut;
    }

    int interval = policyROUNDTIME;
    if (interval == 0)
    {
      // No rounding
      return timeDiff.TotalHours.Round(2);
    }
    else
    {
      // Rounding
      double roundedMin = ((double)timeDiff.Minutes / interval).Round() * interval / 60.0;
      roundedMin = roundedMin.Round(2);

      if (roundedMin == 1.0)
      {
        timeDiff += TimeSpan.FromHours(1);
        roundedMin = 0.00;
      }
      timeDiff -= TimeSpan.FromMinutes(timeDiff.Minutes);

      return (timeDiff.Hours + roundedMin).Round(2);
    }
  }
}
