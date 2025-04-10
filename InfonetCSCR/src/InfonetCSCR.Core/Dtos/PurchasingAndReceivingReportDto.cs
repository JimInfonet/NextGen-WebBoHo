namespace InfonetCSCR.Core.Dtos;

public class PurchasingAndReceivingReportDto
{
  public int PurchaseNo { get; set; }
  public string? CustAccount { get; set; }
  public string? AccountNo { get; set; }
  public decimal? SubTotal { get; set; }
  public decimal? SurchargeTotal { get; set; }
  public decimal? TotalGST { get; set; }
  public decimal? PurchaseTotal { get; set; }
  public string? Terms { get; set; }
  public string? Buyer { get; set; }
  public string? ShipVia { get; set; }
  public string? FOB { get; set; }
  public string? Freight { get; set; }
  public string? Notes { get; set; }
  public DateTime? PurchaseDate { get; set; }
  public DateTime? DeliveryDate { get; set; }
  public DateTime? ReceivedDate { get; set; }
  public int LineNum { get; set; }
  public string? PartNum { get; set; }
  public decimal? OrderedQty { get; set; }
  public string? UOM { get; set; }
  public decimal? UnitQty { get; set; }
  public decimal? Cost { get; set; }
  public string? Description { get; set; }
  public decimal? ExtCost { get; set; }
  public bool? ApplyGST { get; set; }
  public decimal? GST { get; set; }
  public decimal? CalcDiscount { get; set; }
  public string? Address { get; set; }
  public string? VendorsCity { get; set; }
  public string? VendorsProvince { get; set; }
  public string? Postal { get; set; }
  public string? Phone { get; set; }
  public string? Fax { get; set; }
  public string Name { get; set; } = default!;
  public string? CompanyName { get; set; }
  public string? Address1 { get; set; }
  public string? Address2 { get; set; }
  public string? SetupCity { get; set; }
  public string? SetupProvince { get; set; }
  public string? PostalCode { get; set; }
  public string? TelNum { get; set; }
  public string? FaxNum { get; set; }
  public string? PSet { get; set; }
}
