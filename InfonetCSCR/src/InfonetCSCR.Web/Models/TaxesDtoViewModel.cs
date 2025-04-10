namespace InfonetCSCR.Web.Models
{
  public class TaxesDtoViewModel
  {
    public string? TaxName { get; set; }
    public string? TaxCode { get; set; }
    public decimal? AddedTax { get; set; }
    public decimal? InclTax { get; set; }
    public string? TaxName_Desc { get; set; }
    public string? TaxCode_Desc { get; set; }
    public bool? Included { get; set; }
  }
}
