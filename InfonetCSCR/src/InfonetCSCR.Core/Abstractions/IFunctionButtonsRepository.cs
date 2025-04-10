namespace InfonetCSCR.Core.Abstractions;

public interface IFunctionButtonsRepository
{
  Task<IList<FunctionButton>> GetAllFunctionButtonsDataAsync();
  Task<FunctionButton> GetFunctionButtonsDataByButtonIdAsync(int buttonId);
  Task<bool> InsertFunctionButtonAsync(FunctionButton functionButton);
  Task<bool> UpdateFunctionButtonAsync(FunctionButton functionButton);
  Task<bool> DeleteFunctionButtonAsync(int buttonId);
}
