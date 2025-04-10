namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICardMessageRepository
{
  Task<IList<CardMessage>> GetAllCardMessagesDataAsync();
  Task<bool> InsertCardMessageAsync(CardMessage cardMessage);
  Task<bool> DeleteCardMessageAsync(CardMessage cardMessage);
}
