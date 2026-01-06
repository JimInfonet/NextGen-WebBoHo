namespace InfonetCSCR.Services;

public class FunctionButtonsService : IFunctionButtonsService
{
  private readonly IUnitOfWork _unitOfWork;

  public FunctionButtonsService(IApplicationConfig _applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<FunctionButton>> GetAllFunctionButtonsData()
  {
    IList<FunctionButton> functionButtons = await _unitOfWork.FunctionButtons.GetAllFunctionButtonsDataAsync();
    _unitOfWork.Commit();

    return functionButtons;
  }

  public async Task<bool> SwapFunctionButtonData(int sourceButtonId, int targetButtonId)
  {
    FunctionButton sourceFunctionButton = await _unitOfWork.FunctionButtons.GetFunctionButtonsDataByButtonIdAsync(sourceButtonId);
    FunctionButton targetFunctionButton = await _unitOfWork.FunctionButtons.GetFunctionButtonsDataByButtonIdAsync(targetButtonId);

    await _unitOfWork.FunctionButtons.DeleteFunctionButtonAsync(sourceButtonId);
    await _unitOfWork.FunctionButtons.DeleteFunctionButtonAsync(targetButtonId);

    if(sourceFunctionButton != null)
    {
      sourceFunctionButton.ButtonID = targetButtonId;
      await _unitOfWork.FunctionButtons.InsertFunctionButtonAsync(sourceFunctionButton);
    }

    if(targetFunctionButton != null)
    {
      targetFunctionButton.ButtonID = sourceButtonId;
      await _unitOfWork.FunctionButtons.InsertFunctionButtonAsync(targetFunctionButton);
    }

    bool isUpdated = _unitOfWork.Commit();

    return isUpdated;
  }

  public async Task<bool> InsertFunctionButton(FunctionButton functionButton)
  {
    bool isAdded = await _unitOfWork.FunctionButtons.InsertFunctionButtonAsync(functionButton);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> UpdateFunctionButton(FunctionButton functionButton)
  {
    bool isUpdated = await _unitOfWork.FunctionButtons.UpdateFunctionButtonAsync(functionButton);
    _unitOfWork.Commit();

    return isUpdated;
  }

  public async Task<bool> DeleteFunctionButton(int buttonId)
  {
    bool isDeleted = await _unitOfWork.FunctionButtons.DeleteFunctionButtonAsync(buttonId);
    _unitOfWork.Commit();

    return isDeleted;
  }
}
