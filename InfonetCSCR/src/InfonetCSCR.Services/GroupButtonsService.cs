namespace InfonetCSCR.Services;

public class GroupButtonsService : IGroupButtonsService
{
  private readonly IUnitOfWork _unitOfWork;

  public GroupButtonsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<GroupButton>> GetAllGroupButtonsData()
  {
    IList<GroupButton> groupButtons = await _unitOfWork.GroupButtons.GetAllGroupButtonsDataAsync();
    _unitOfWork.Commit();

    return groupButtons;
  }


  public async Task<bool> SwapGroupButtonData(int sourceButtonId, int targetButtonId)
  {
    GroupButton sourceGroupButton = await _unitOfWork.GroupButtons.GetGroupButtonByIdAsync(sourceButtonId);
    GroupButton targetGroupButton = await _unitOfWork.GroupButtons.GetGroupButtonByIdAsync(targetButtonId);

    if (sourceGroupButton != null)
    {
      sourceGroupButton.ButtonId = targetButtonId;
      if (targetGroupButton != null)
      {
        await _unitOfWork.GroupButtons.UpdateGroupButtonAsync(sourceGroupButton);
      }
      else
      {
        await _unitOfWork.GroupButtons.InsertGroupButtonAsync(sourceGroupButton);
        await _unitOfWork.GroupButtons.DeleteGroupButtonAsync(sourceButtonId);
      }
    }

    if (targetGroupButton != null)
    {
      targetGroupButton.ButtonId = sourceButtonId;
      if (sourceGroupButton != null)
      {
        await _unitOfWork.GroupButtons.UpdateGroupButtonAsync(targetGroupButton);
      }
      else
      {
        await _unitOfWork.GroupButtons.InsertGroupButtonAsync(targetGroupButton);
        await _unitOfWork.GroupButtons.DeleteGroupButtonAsync(targetButtonId);
      }
    }

    bool isUpdated = _unitOfWork.Commit();

    return isUpdated;
  }

  public async Task<bool> InsertGroupButton(GroupButton groupButton)
  {
    bool isAdded = await _unitOfWork.GroupButtons.InsertGroupButtonAsync(groupButton);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> UpdateGroupButton(GroupButton groupButton)
  {
    bool isUpdated = await _unitOfWork.GroupButtons.UpdateGroupButtonAsync(groupButton);
    _unitOfWork.Commit();

    return isUpdated;
  }

  public async Task<bool> DeleteGroupButton(int buttonId)
  {
    bool isDeleted = await _unitOfWork.GroupButtons.DeleteGroupButtonAsync(buttonId);
    _unitOfWork.Commit();

    return isDeleted;
  }
}
