namespace InfonetCSCR.Web.Models;

public class SideBarItemsPoliciesViewModel
{
  public bool ShowStockReceiving { get; set; }
  public bool ShowStockTakes { get; set; }
  public bool ShowPurchasingAndReceiving { get; set; }
  public bool ShowCheckInCheckOut { get; set; }
  public bool ShowPriceBookTransfer { get; set; }
  public bool ShowPriceBookManagement { get; set; }
  public bool IsCustomer { get; set; }
  public bool CustomerDiscount { get; set; }
  public bool ProductDiscount { get; set; }
  public bool ShowFunctionButtons { get; set; }
  public bool ShowFuelPriceChange { get; set; }
  public bool UserReports { get; set; }
  public bool ShowSendDataToStore { get; set; }
}
