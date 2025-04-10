namespace InfonetCSCR.Core.Models;

  public class PurchaseHeader
  {
    public long PurchaseNo { get; set; } = default!;
    public string? AccountNo { get; set; }
    public string? CustAccount { get; set; }
    public string? VendorName { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public bool? Paid { get; set; }
    public int? Lines { get; set; }
    public int? ShiftID { get; set; }
    public int? ShiftNum { get; set; }
    public double? SurChargeTotalDiscount { get; set; }
    public string? SurChargeTotalDiscountType { get; set; }
    public bool? Emailed { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? SendDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? PostedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? POStatus { get; set; }
    public string? SendBy { get; set; }
    public string? PostedBy { get; set; }
    public double? SubTotal { get; set; }
    public double? SurChargeTotal { get; set; }
    public double? TotalGST { get; set; }
    public double? PurchaseTotal { get; set; }
    public string? PayType { get; set; }
    public string? Terms { get; set; }
    public string? Buyer { get; set; }
    public string? ShipVia { get; set; }
    public string? FOB { get; set; }
    public string? Freight { get; set; }
    public string? Notes { get; set; }
    public string? StoreID { get; set; }
    public string? HeaderFileName { get; set; }
    public bool? Reposted { get; set; }
    public string? RepostedBy { get; set; }
    public DateTime? RepostedDate { get; set; }
    public bool? TaxexemptPO { get; set; }
    public double? Charges { get; set; }
    public string? InvoiceNo { get; set; }
    public string? TransStatus { get; set; }
    public double? CalculateDiscount { get; set; }
    public int SupplierAmount { get; set; }
  }
