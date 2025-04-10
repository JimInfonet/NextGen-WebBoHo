namespace InfonetCSCR.Web.Abstractions.Services;

public interface IGroupButtonsService
{
  Task<IList<GroupButtonViewModel>?> GetAllGroupButtonsData();
  Task<bool> SwapGroupButtonData(int sourceButtonId, int targetButtonId);
  Task<bool> InsertGroupButton(GroupButtonViewModel groupButton);
  Task<bool> UpdateGroupButton(GroupButtonViewModel groupButton);
  Task<bool> DeleteGroupButton(int buttonId);
}
