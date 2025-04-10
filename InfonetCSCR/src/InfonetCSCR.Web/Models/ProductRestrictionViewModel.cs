namespace InfonetCSCR.Web.Models;

public class ProductRestrictionViewModel
{
  public string? BankCardId { get; set; }
  [Required(ErrorMessage = "thisFieldIsRequired")]
  [MaxLength(16)]
  public string? CardProductCode { get; set; }
  [MaxLength(50)]
  public string? CardDescription { get; set; }
  [MaxLength(16)]
  public string? CardProductDescription { get; set; }
  public string? Dept { get; set; }
  public string? SubDept { get; set; }
  public string? SubDetail { get; set; }
  public string? StockCode { get; set; }
  [MaxLength(50)]
  public string? Description { get; set; }
  public string? CardNumber { get; set; }
  public string? ProfileId { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public decimal? Amount { get; set; }

  //Previous Data are used to track which data to update as there are no primary key
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
