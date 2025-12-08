using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Abstractions.Services
{
    public interface IRepArStatService
    {
        public Task<IEnumerable<CustomersDto>> GetAllCustomers(bool orderByCode);
        public Task<ArStatementDto> GetRepArStat(String client, DateTime dDate);
        public Task<bool> GenMonthClosing(String client, DateTime dDate);
        
        //public Task<string[]> GetRepByShift(GetRepArStatDto model);
        //public Task<IEnumerable<int>> GetShiftNos(DateTime shiftdate);
        //public Task<string[]> GetRepByDates(GetRepTillCloseDto model);
        //public Task<string[]> GetRepByTillNo(GetRepTillCloseDto model);
    }
}
