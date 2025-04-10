namespace InfonetCSCR.Web.Models;

public class TenderViewModel
{
  [Required(ErrorMessage = "thisFieldIsRequired")]
  [MaxLength(10, ErrorMessage = "tendCodeMaxLength")]
  public string Code { get; set; } = default!;
  [Required(ErrorMessage = "thisFieldIsRequired")]
  [MaxLength(8, ErrorMessage = "shortNameMaxLength")]
  public string? ShortName { get; set; }
  [Required(ErrorMessage = "thisFieldIsRequired")]
  [MaxLength(20, ErrorMessage = "fullNameMaxLength")]
  public string? FullName { get; set; }
  [Required(ErrorMessage = "thisFieldIsRequired")]
  [MaxLength(10, ErrorMessage = "classMaxLength")]
  public string? Class { get; set; }
  [Required(ErrorMessage = "thisFieldIsRequired")]
  [Range(0, 214748, ErrorMessage = "exchangeRateRange")]
  public decimal? ExchangeRate { get; set; }
  [Required(ErrorMessage = "thisFieldIsRequired")]
  [Range(0, 255, ErrorMessage = "sequenceRange")]
  public byte? Sequence { get; set; }
  [Range(-32767, 32767, ErrorMessage = "cardCodeRange")]
  public int? CardCode { get; set; }
  public bool? ExactChange { get; set; }
  public bool? GiveChange { get; set; }
  public bool? AcceptAsPayment { get; set; }
  public bool? GiveAsRefund { get; set; }
  public bool? CanAdjust { get; set; }
  public bool? Bankable { get; set; }
  public bool? IncludeInClose { get; set; }
  public bool? PutInDrawer { get; set; }
  public bool? Signature { get; set; }
  public bool? Inactive { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public decimal? MinAmount { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public decimal? MaxAmount { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public decimal? SmallestUnit { get; set; }
  [Range(0, 255, ErrorMessage = "printCopiesRange")]
  public double? PrintCopies { get; set; }
  public byte? Instances { get; set; }
  public string? PayoutCode { get; set; }
  public string? AcctCode { get; set; }
  public int? PreviousCardCode { get; set; }

  #region checkboxes
  public bool ExactChangeCheckBox
  {
    get => this.ExactChange.GetValueOrDefault();
    set { this.ExactChange = value; }
  }
  public bool GiveChangeCheckBox
  {
    get => this.GiveChange.GetValueOrDefault();
    set { this.GiveChange = value; }
  }
  public bool AcceptAsPaymentCheckBox
  {
    get => this.AcceptAsPayment.GetValueOrDefault();
    set { this.AcceptAsPayment = value; }
  }
  public bool GiveAsRefundCheckBox
  {
    get => this.GiveAsRefund.GetValueOrDefault();
    set { this.GiveAsRefund = value; }
  }
  public bool CanAdjustCheckBox
  {
    get => this.CanAdjust.GetValueOrDefault();
    set { this.CanAdjust = value; }
  }
  public bool BankableCheckBox
  {
    get => this.Bankable.GetValueOrDefault();
    set { this.Bankable = value; }
  }
  public bool IncludeInCloseCheckBox
  {
    get => this.IncludeInClose.GetValueOrDefault();
    set { this.IncludeInClose = value; }
  }
  public bool PutInDrawerCheckBox
  {
    get => this.PutInDrawer.GetValueOrDefault();
    set { this.PutInDrawer = value; }
  }
  public bool SignatureCheckBox
  {
    get => this.Signature.GetValueOrDefault();
    set { this.Signature = value; }
  }
  public bool InactiveCheckBox
  {
    get => this.Inactive.GetValueOrDefault();
    set { this.Inactive = value; }
  }
  #endregion
}
