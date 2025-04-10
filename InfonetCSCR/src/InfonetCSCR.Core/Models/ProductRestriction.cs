namespace InfonetCSCR.Core.Models;
public class ProductRestriction
{
  public string BankCardId { get; set; } = default!;
  public string CardProductCode { get; set; } = default!;
  public string? CardDescription { get; set; }
  public string? CardProductDescription { get; set; }
  public string? Dept { get; set; }
  public string? SubDept { get; set; }
  public string? SubDetail { get; set; }
  public string? StockCode { get; set; }
  public string? Description { get; set; }
  public string? CardNumber { get; set; }
  public string? ProfileId { get; set; }
  public decimal? Amount { get; set; }
  public string? PreBankCardId { get; set; } = default!;
  public string? PreCardProductCode { get; set; } = default!;
  public string? PreCardDescription { get; set; }
  public string? PreCardProductDescription { get; set; }
  public string? PreDept { get; set; }
  public string? PreSubDept { get; set; }
  public string? PreSubDetail { get; set; }
  public string? PreStockCode { get; set; }
  public string? PreDescription { get; set; }
  public string? PreCardNumber { get; set; }
  public string? PreProfileId { get; set; }
  public decimal? PreAmount { get; set; }
}
