namespace InfonetCSCR.Core.Dtos;

public class PendingTransferDto
{
  public List<StockItemMinimal> Products { get; set; }
  public List<StockItemMinimal?>? DeselectedProducts { get; set; }
  public List<string?>? StoreIds { get; set; }
  public List<string>? SelectedOptionIds { get; set; }
  public List<PriceChangeInfoDto> PriceDetails { get; set; }
  public List<Department> Departments { get; set; }
  public List<SubDepartment?>? SubDepartments { get; set; }
  public List<SubDetail?>? SubDetails { get; set; }
  public bool ShowAllProducts { get; set; }
  public bool? IsSelectAllStoreOrOtherOptionChecked { get; set; }
  public bool IsSelectAllProductsChecked { get; set; }
  public bool? IsSelectAllProductsModeActivated { get; set; }
  public short OrderNo { get; set; }
  public string VendorId { get; set; }
  public string? PR_Units { get; set; }
  public string? IncreaseOrDecrease { get; set; }
  public string? DollarOrPercentage { get; set; }
  public string? MarginOrPrice { get; set; }
  public string SelectedPriceOption { get; set; }
  public string SelectedPriceType { get; set; }
  public string SelectedDataTransferType { get; set; }
  public DateTime? EffectiveDateFrom { get; set; }
  public DateTime? EffectiveDateTo { get; set; }
  public bool EffectiveDateDoesNotEnd { get; set; }
  public bool ChangePriceBasedOnCost { get; set; }
  public string? TransUser { get; set; }
  public PricebookTransferPoliciesDto Policies { get; set; }
  public bool? IsStockChangeSelected { get; set; }
  public string? UserCode { get; set; }
  public string? TransType { get; set; }
  public string? ShowByOption { get; set; }
  public bool? IsShowVendorProducts { get; set; }
}
