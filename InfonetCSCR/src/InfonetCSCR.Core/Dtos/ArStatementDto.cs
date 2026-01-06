using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Dtos
{
    public class ArStatementDto
    {
        public CompanyDto? header { get; set; }
        public List<ArSalesDto>? details { get; set; }
    }
}
