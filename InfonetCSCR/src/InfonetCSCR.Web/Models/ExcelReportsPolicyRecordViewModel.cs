namespace InfonetCSCR.Web.Models;

public class ExcelReportsPolicyRecordViewModel
{
  public bool IsTaxExempt { get; set; } = default!;
  public string TaxExemptType { get; set; } = default!;
  public string MemberNo { get; set; } = default!;
  public bool TEByRate { get; set; } = default!;
  public string Version { get; set; } = default!;
}
