namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IDataDefinitionRepository
{
  Task<bool> InsertIntoDataDefinitionBatch(List<DataDefinition> dataDefinitions);
  Task<IEnumerable<DataDefinition>> GetDataDefinitionsAsync();
  Task<IEnumerable<DataDefinition>> GetDataDefinitionsByCriteriaAsync(GetDataDefinitionsByCriteriaDto model);
  Task<bool> DeleteFromDataDefinitionBatch(List<DataDefinition> dataDefinitions);
  Task<bool> UpdateDataDefinitionTransFlagAsync();
}
