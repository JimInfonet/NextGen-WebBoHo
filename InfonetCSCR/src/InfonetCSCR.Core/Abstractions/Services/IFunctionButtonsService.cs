namespace InfonetCSCR.Core.Abstractions.Services;

public interface IFunctionButtonsService
{
  Task<IList<FunctionButton>> GetAllFunctionButtonsData();
  Task<bool> SwapFunctionButtonData(int sourceButtonId, int targetButtonId);
  Task<bool> DeleteFunctionButton(int buttonId);
  Task<bool> InsertFunctionButton(FunctionButton functionButton);
  Task<bool> UpdateFunctionButton(FunctionButton functionButton);
}
