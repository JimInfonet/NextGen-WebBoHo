using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfonetCSCR.Reporting.Models
{
    public class SalesReportViewModel
    {
        public List<FuelSalesModel> FuelSales { get; set; }
        public List<CStoreSalesModel> CStoreSales { get; set; }
        public List<SalesTax> SalesTaxes { get; set; }
        public List<SaleCharge> SalesCharges { get; set; }
        public List<SaleReceipt> SalesReceipts { get; set; }
        public DateTime DateTime{ get; set; }
    }

    public class FuelSalesModel
    {
        public string DeptID { get; set; }
        public string StockCode { get; set; }
        public decimal QTY { get; set; }
        public decimal Sales { get; set; }
    }

    public class CStoreSalesModel
    {
        public string DeptID { get; set; }
        public string DeptName { get; set; }
        public decimal QTY { get; set; }
        public decimal Sales { get; set; }
    }

    public class SalesDetails
    {
        public int SaleNo { get; set; }
        public int TillNum { get; set; }
        public int LineNum { get; set; }
        public string StockCode { get; set; }
        public string Descr { get; set; }
        public decimal QTY { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
    }

    public class SalesTax
    {
        public string TaxName { get; set; }
        public decimal Added { get; set; }
        public decimal Included { get; set; }
        public decimal Total { get; set; }
    }

    public class SalesTaxDetails
    {
        public int SaleNo { get; set; }
        public int TillNum { get; set; }
        public string TaxName { get; set; }
        public decimal Added { get; set; }
        public decimal Included { get; set; }
    }

    public class SaleCharge
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }

    public class SaleChargeDetails
    {
        public int SaleNo { get; set; }
        public int TillNum { get; set; }
        public int LineNum { get; set; }
        public string Descr { get; set; }
        public decimal Amount { get; set; }
    }

    public class SaleReceipt
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public bool IsChartData { get; set; }
    }

    public class SaleReceiptDetails
    {
        public int SaleNo { get; set; }
        public int TillNum { get; set; }
        public int Sequence { get; set; }
        public string TendName { get; set; }
        public decimal Amount { get; set; }
    }

}