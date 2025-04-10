namespace InfonetCSCR.Web.Models;
public class CardRestrictionProfileViewModel
{
  [MaxLength(10)]
  [Required(ErrorMessage = "thisFieldIsRequired")]
  public string ProfileID { get; set; } = default!;
  [MaxLength(30)]
  [Required(ErrorMessage = "thisFieldIsRequired")]
  public string? ProfileName { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public decimal? SngleTransaction { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public decimal? DailyTransaction { get; set; }
  [Range(-214748.3648, 214748.3647, ErrorMessage = "smallMoneyRange")]
  public decimal? MonthlyTransaction { get; set; }
  [Range(-32768, 32767, ErrorMessage = "smallIntRange")]
  public int? TransactionsPerDay { get; set; }
  public bool? AskForPO { get; set; }
  public bool? PromptForFuel { get; set; }
  public bool? LimitTimeofPurchase { get; set; }
  public bool? RestrictProducts { get; set; }
  
  #region checkboxes
  public bool AskForPOCheckbox
  {
    get => this.AskForPO.GetValueOrDefault();
    set { this.AskForPO = value; }
  }
  public bool PromptForFuelCheckbox
  {
    get => this.PromptForFuel.GetValueOrDefault();
    set { this.PromptForFuel = value; }
  }
  public bool LimitTimeofPurchaseCheckbox
  {
    get => this.LimitTimeofPurchase.GetValueOrDefault();
    set { this.LimitTimeofPurchase = value; }
  }
  #endregion
}
