namespace InfonetCSCR.Web.Helpers;

public static class StockItemReportsHelper
{
  public static (List<StockItemReportQuantityViewModel> quantityViewData, List<StockItemReportAmountViewModel> amountViewData) mapReportData(IEnumerable<StockItemReportsViewModel> reportData)
  {
    int maxYear, minYear = int.MaxValue;
    maxYear = DateTime.Now.Year;

    foreach (StockItemReportsViewModel dr in reportData)
    {
      minYear = Math.Min(minYear, Convert.ToInt32(dr.Year));
    }
    List<StockItemReportQuantityViewModel> quantityViewData = new List<StockItemReportQuantityViewModel>();
    List<StockItemReportAmountViewModel> amountViewData = new List<StockItemReportAmountViewModel>();
    for (int j = 0; j < maxYear - minYear + 1; j++)
    {
      quantityViewData.Add(new StockItemReportQuantityViewModel
      {
        Year = minYear + j
      });
      amountViewData.Add(new StockItemReportAmountViewModel
      {
        Year = minYear + j
      });
    }

    foreach (StockItemReportsViewModel dr in reportData)
    {
      int index = Convert.ToInt32(dr.Year) - minYear;
      dr.Amount = Math.Round(Convert.ToDouble(dr.Amount), 2);
      switch (Convert.ToInt32(dr.Month))
      {
        case 1:
          quantityViewData[index].QtyJan = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountJan = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 2:
          quantityViewData[index].QtyFeb = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountFeb = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 3:
          quantityViewData[index].QtyMar = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountMar = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 4:
          quantityViewData[index].QtyApr = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountApr = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 5:
          quantityViewData[index].QtyMay = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountMay = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 6:
          quantityViewData[index].QtyJun = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountJun = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 7:
          quantityViewData[index].QtyJul = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountJul = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 8:
          quantityViewData[index].QtyAug = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountAug = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 9:
          quantityViewData[index].QtySep = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountSep = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 10:
          quantityViewData[index].QtyOct = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountOct = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 11:
          quantityViewData[index].QtyNov = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountNov = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
        case 12:
          quantityViewData[index].QtyDec = Convert.ToInt32(dr.Quantity);
          quantityViewData[index].QtyTotal += Convert.ToInt32(dr.Quantity);
          amountViewData[index].AmountDec = Convert.ToDouble(dr.Amount);
          amountViewData[index].AmountTotal += Convert.ToDouble(dr.Amount);
          break;
      }
      amountViewData[index].AmountTotal = Math.Round(amountViewData[index].AmountTotal, 2);
    }

    return (quantityViewData, amountViewData);
  }
}
