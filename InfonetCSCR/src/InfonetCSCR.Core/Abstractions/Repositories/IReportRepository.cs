using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Abstractions.Repositories
{
    public interface IReportRepository
    {
        Task <ReportFilter> ReportFilter();
    }
}
