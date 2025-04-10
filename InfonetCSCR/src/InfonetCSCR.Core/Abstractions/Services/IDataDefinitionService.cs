namespace InfonetCSCR.Core.Abstractions.Services;

public interface IDataDefinitionService
{
  Task<IEnumerable<DataDefinition>> GetDataDefinitionsByCriteria(GetDataDefinitionsByCriteriaDto model);
  Task<IEnumerable<Store>> GetStoresFromStockTransfer();
  Task<IEnumerable<Region>> GetRegionsFromStockTransfer();
  Task<IEnumerable<PriceGroup>> GetPriceGroupsFromStockTransfer();
  Task<IEnumerable<Vendor>> GetVendorsFromStockTransfer();
  Task<bool> SaveDataDefinition(DataDefinitionDto dataDefinitionDto);
}
