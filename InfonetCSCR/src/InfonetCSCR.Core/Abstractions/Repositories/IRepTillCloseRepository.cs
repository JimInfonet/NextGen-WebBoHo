using InfonetCSCR.Core.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfonetCSCR.Core.Abstractions.Repositories
{
    public interface IRepTillCloseRepository
    {
        //public Task<IEnumerable<RepTillClose>> GetRepTillCloses();
        public Task<IEnumerable<RepTillClose>> GetRepTillCloses(DateTime date1, DateTime date2);
        public Task<string[]> GetRepByShift(GetRepTillCloseDto model);
        public Task<IEnumerable<int>> GetShiftNos(DateTime shiftdate);
        public Task<string[]> GetRepByDates(GetRepTillCloseDto model);
        public Task<string[]> GetRepByTillNo(GetRepTillCloseDto model);
        Task<bool> AddTillClose(TillClose tc);
    }
}
