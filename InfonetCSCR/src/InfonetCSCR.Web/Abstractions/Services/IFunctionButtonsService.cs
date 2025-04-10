namespace InfonetCSCR.Web.Abstractions.Services;

public interface IFunctionButtonsService
{
  Task<IList<FunctionButtonViewModel>?> GetAllFunctionButtonsData();
  Task<bool> SwapFunctionButtonData(int sourceButtonId, int targetButtonId);
  Task<bool> InsertFunctionButton([FromBody] FunctionButtonViewModel functionButton);
  Task<bool> UpdateFunctionButton([FromBody] FunctionButtonViewModel functionButton);
  Task<bool> DeleteFunctionButton(int buttonId);
}
