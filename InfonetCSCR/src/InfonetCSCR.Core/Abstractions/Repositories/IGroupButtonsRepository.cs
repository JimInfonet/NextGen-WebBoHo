namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IGroupButtonsRepository
{
  Task<IList<GroupButton>> GetAllGroupButtonsDataAsync();
  Task<GroupButton> GetGroupButtonByIdAsync(int buttonId);
  Task<bool> InsertGroupButtonAsync(GroupButton groupButton);
  Task<bool> UpdateGroupButtonAsync(GroupButton groupButton);
  Task<bool> DeleteGroupButtonAsync(int buttonId);
}
