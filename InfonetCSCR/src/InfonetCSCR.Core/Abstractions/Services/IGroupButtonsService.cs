namespace InfonetCSCR.Core.Abstractions.Services;

public interface IGroupButtonsService
{
  Task<IList<GroupButton>> GetAllGroupButtonsData();
  Task<bool> SwapGroupButtonData(int sourceButtonId, int targetButtonId);
  Task<bool> DeleteGroupButton(int buttonId);
  Task<bool> InsertGroupButton(GroupButton groupButton);
  Task<bool> UpdateGroupButton(GroupButton groupButton);
}
