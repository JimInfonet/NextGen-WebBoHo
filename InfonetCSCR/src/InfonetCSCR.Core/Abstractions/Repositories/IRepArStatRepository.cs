using InfonetCSCR.Core.Dtos;

namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IRepArStatRepository
{
    public Task<IEnumerable<CustomersDto>> GetAllCustomersAsync(bool orderByCode);
    public Task <ArStatementDto> GetRepArStat(String client, DateTime dDate);
    public Task<bool> GenMonthClosing(String client, DateTime dDate);
}

