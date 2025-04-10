namespace InfonetCSCR.Web.Models;

public class SaleHeadAndSaleLineViewModel
{
  public DateTime SaleDate { get; set; }
  public DateTime SaleTime { get; set; }
  public string StockCode { get; set; }
  public DateTime SD { get; set; }
  public Decimal Quantity { get; set; }
  public Decimal Amount { get; set; }
  public Decimal Price { get; set; }
  public int? GradeID { get; set; }
}
