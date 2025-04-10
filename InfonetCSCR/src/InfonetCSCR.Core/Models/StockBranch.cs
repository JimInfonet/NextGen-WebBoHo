namespace InfonetCSCR.Core.Models;

public class StockBranch
{
  public string StockCode { get; set; } = default!;
  public string Branch { get; set; } = default!;
  public decimal? InStock { get; set; }
  public decimal? Avail { get; set; }
  public decimal? OnOrderIC { get; set; }
  public decimal? OrderCostIC { get; set; }
  public decimal? OnOrderPO { get; set; }
  public decimal? AllocLay { get; set; }
  public decimal? AllocOE { get; set; }
  public decimal? Returned { get; set; }
  public decimal? Hold { get; set; }
  public decimal? Waste { get; set; }
  public decimal? StockTake { get; set; }
  public bool? TakeEnter { get; set; }
  public decimal? MinQty { get; set; }
  public decimal? MaxQty { get; set; }
  public decimal? ReOrderQty { get; set; }
  public DateTime? StDate { get; set; }
  public DateTime? LastRecv { get; set; }
  public DateTime? LastOrder { get; set; }
  public DateTime? LastSale { get; set; } 
  public decimal? MaxReOrderQty { get; set; }
  public bool? ActiveStock { get; set; }
}
