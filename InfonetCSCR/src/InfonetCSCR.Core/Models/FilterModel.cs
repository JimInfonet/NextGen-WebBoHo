using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Models
{
    public class FilterModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    public class DeptFilterModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? FuelDept { get; set; }
    }

    public class StockMovementFilterModel
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? DeptId { get; set; }
    }

    public class CategoryFilterModel
    {
        public int GID { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
    public class ReportFilterModel
    {
        public string? Report_Name { get; set; }
        public int Filter_Seq { get; set; }
        public string? Field { get; set; }
        public string? Type { get; set; }
        public string? Field_Type { get; set; }
    }
    //public class ReportResModel
    //{
    //    public string? Report_Name { get; set; }
    //    public string? Field_Name { get; set; }
    //    public int Res_Number { get; set; }
    //}
    public class ReportFilter
    {
        public List<DeptFilterModel>? Dept { get; set; }
        public List<DeptFilterModel>? DeptWithFuel { get; set; }
        public List<StockMovementFilterModel>? SubDept { get; set; }
        public List<StockMovementFilterModel>? SubDetail { get; set; }
        public List<StockMovementFilterModel>? StockCode { get; set; }
        public List<FilterModel>? Vendor { get; set; }
        public List<CategoryFilterModel>? Reports { get; set; }
        public List<ReportFilterModel>? Filters { get; set; }
        //public List<ReportResModel>? ReportRes { get; set; }
    }

}
