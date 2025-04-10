namespace InfonetCSCR.Core.Abstractions.Services;

public interface IVendorSortService
{
  Task<Dictionary<string, string>> GetVendorSortConditionDictionaryByAccountNoList(List<string> accountNoList);
}
