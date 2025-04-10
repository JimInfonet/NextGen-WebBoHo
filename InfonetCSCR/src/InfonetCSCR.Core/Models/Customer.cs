namespace InfonetCSCR.Core.Models;

public class Customer
{
  public string Code { get; set; } = default!;
  public string? Name { get; set; }
  public string? Category { get; set; }
  public string? Address1 { get; set; }
  public string? Address2 { get; set; }
  public string? City { get; set; }
  public string? Province { get; set; }
  public string? Country { get; set; }
  public string? PostalCode { get; set; }
  public string? AreaCode { get; set; }
  public string? Phone { get; set; }
  public string? OfficePhone { get; set; }
  public string? CellPhone { get; set; }
  public string? Fax { get; set; }
  public string? Toll { get; set; }
  public string? Email { get; set; }
  public string? Contact1 { get; set; }
  public string? Contact2 { get; set; }
  public string? LoyaltyNumber { get; set; }
  public float? LoyaltyPoints { get; set; }
  public bool? ARCustomer { get; set; }
  public string? Type { get; set; }
  public float? Terms { get; set; }
  public int? DiscountDays { get; set; }
  public float? EarlyPayPercent { get; set; }
  public string? CustomerDiscount { get; set; }
  public string? PriceCode { get; set; }
  public decimal? HistoricalBalance { get; set; }
  public decimal? HistoricalPaid { get; set; }
  public decimal? CreditLimit { get; set; }
  public decimal? CurrentBalance { get; set; }
  public string? Status { get; set; }
  public string? ParentAccount { get; set; }
  public bool? UsePurchaseOrder { get; set; }
  public string? GroupID { get; set; }
  public bool? UseFuelRebate { get; set; }
  public bool? UseFuelRebateDiscount { get; set; }
  public string? Note { get; set; }
  public bool? TaxExempt { get; set; }
  public string? PlateNumber { get; set; }
  public bool? MultiUsePurchaseOrder { get; set; }
  public string? CardNumber { get; set; }
  public string? ProfileID { get; set; }
}
