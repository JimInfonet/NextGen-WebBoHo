namespace InfonetCSCR.Api.Helpers;

public static class ProductHelper
{
  public static StockMaster GetStockMasterFromProduct(Product product)
  {
    return (StockMaster)product;
  }

  public static StockBranch GetStockBranchFromProduct(Product product)
  {
    StockBranch stockBranchFromProduct = new StockBranch();
    stockBranchFromProduct.StockCode = product.StockCode;
    stockBranchFromProduct.MaxQty = product.MaxQty;
    stockBranchFromProduct.ReOrderQty = product.ReOrderQty;

    return stockBranchFromProduct;
  }

  public static PLUMast GetPLUMastFromProduct(Product product)
  {
    PLUMast plumastFromProduct = new PLUMast();
    plumastFromProduct.PLUCode = product.StockCode;
    plumastFromProduct.PLUPrim = product.StockCode;
    plumastFromProduct.PLUType = "S"; // Hard coded in old codebase

    return plumastFromProduct;
  }

  public static ProductTaxExempt GetTaxExemptfromProduct(Product product)
  {
    ProductTaxExempt taxExemptFromProduct = new ProductTaxExempt();
    taxExemptFromProduct.ProductCode = product.StockCode;
    taxExemptFromProduct.MaxQty = product.MaxQtyTE;
    taxExemptFromProduct.ReorderQuantity = product.ReOrderQtyTE;

    return taxExemptFromProduct;
  }

  public static Product GetProductFromStockMasterAndStockBranch(StockMaster stockMaster, StockBranch stockBranch, ProductTaxExempt? productTaxExempt)
  {
    Product product = new Product();
    product.StockCode = stockMaster.StockCode;
    product.Description = stockMaster.Description;
    product.StockType = stockMaster.StockType;
    product.ProductDiscount = stockMaster.ProductDiscount;
    product.StandardCost = stockMaster.StandardCost;
    product.AverageCost = stockMaster.AverageCost;
    product.AverageUnits = stockMaster.AverageUnits;
    product.LateCost = stockMaster.LateCost;
    product.Department = stockMaster.Department;
    product.SubDepartment = stockMaster.SubDepartment;
    product.SubDetail = stockMaster.SubDetail;
    product.Serial = stockMaster.Serial;
    product.SBYWGHT = stockMaster.SBYWGHT;
    product.Vendor = stockMaster.Vendor;
    product.PriceType = stockMaster.PriceType;
    product.PriceUnits = stockMaster.PriceUnits;
    product.PriceFrom = stockMaster.PriceFrom;
    product.PriceTo = stockMaster.PriceTo;
    product.VendorNumber = stockMaster.VendorNumber;
    product.Availability = stockMaster.Availability;
    product.Brand = stockMaster.Brand;
    product.Characteristic = stockMaster.Characteristic;
    product.CreationDate = stockMaster.CreationDate;
    product.Family = stockMaster.Family;
    product.Format = stockMaster.Format;
    product.Generic = stockMaster.Generic;
    product.Label = stockMaster.Label;
    product.UnitOfMeasure = stockMaster.UnitOfMeasure;
    product.UpdateDate = stockMaster.UpdateDate;
    product.OrderUnit = stockMaster.OrderUnit;
    product.Packing = stockMaster.Packing;
    product.Transfer = stockMaster.Transfer;
    product.EligibleLoyalty = stockMaster.EligibleLoyalty;
    product.EligibleFuelRebate = stockMaster.EligibleFuelRebate;
    product.FuelRebate = stockMaster.FuelRebate;
    product.QualTaxRebate = stockMaster.QualTaxRebate;
    product.EligibleTaxRebate = stockMaster.EligibleTaxRebate;
    product.EligibleTaxExemption = stockMaster.EligibleTaxExemption;

    product.MaxQty = stockBranch.MaxQty;
    product.ReOrderQty = stockBranch.ReOrderQty;
    product.Branch = stockBranch.Branch;
    product.AvailableQty = stockBranch.Avail;

    product.MaxQtyTE = productTaxExempt?.MaxQty;
    product.ReOrderQtyTE = productTaxExempt?.ReorderQuantity;
    product.AvailableQtyTE = productTaxExempt?.Available;

    return product;
  }
}
