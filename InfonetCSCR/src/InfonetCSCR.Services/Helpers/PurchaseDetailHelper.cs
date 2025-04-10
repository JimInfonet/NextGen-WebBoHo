namespace InfonetCSCR.Services.Helpers;

public static class PurchaseDetailHelper
{
  public static double CalculateDiscount(double poDiscount, double cost, double charges)
  {
    double discount = 0;
    if (poDiscount != 0)
    {
      discount = ((cost - charges) / (1 - poDiscount) * poDiscount).Round(4);
    }
    return discount;
  }

  public static double CalculateGST(IList<double> taxRates, string exptTaxPolicy, double extCost)
  {
    double calculatedGST = 0;
    foreach (double taxRate in taxRates)
    {
      calculatedGST += (extCost * taxRate / 100);
      calculatedGST = calculatedGST.Round(2);
    }

    return calculatedGST;
  }

  public static double CalculateItemsGstRate(IList<double> taxRates)
  {
    double itemsGstRate = 0;
    foreach (double taxRate in taxRates)
    {
      itemsGstRate += taxRate / 100;
    }

    itemsGstRate = itemsGstRate.Round(2);
    return itemsGstRate;
  }

  public static double CalculateItemsHstRate(IList<double> taxRates, int? costDecPolicy)
  {
    double itemsHstRate = 0;
    foreach (double taxRate in taxRates)
    {
      itemsHstRate += taxRate / 100;
    }

    itemsHstRate = itemsHstRate.Round(costDecPolicy ?? 0);
    return itemsHstRate;
  }

  public static double CalculateTax(double itemsTaxRate, double amount)
  {
    double calculatedTax = amount - amount / (1 + itemsTaxRate);
    calculatedTax = calculatedTax.Round(2);

    return calculatedTax;
  }
}
