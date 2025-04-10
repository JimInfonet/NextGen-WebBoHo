namespace InfonetCSCR.Web.Models;

public class FuelSaleVarianceViewModel
{
  public int PumpID { get; set; }
  public string? GradeName { get; set; }
  public decimal SumOfQty { get; set; }
  public decimal SumOfExt { get; set; }
  public decimal SumOfSalesLiters { get; set; }
  public decimal SumOfSalesDollars { get; set; }
}
