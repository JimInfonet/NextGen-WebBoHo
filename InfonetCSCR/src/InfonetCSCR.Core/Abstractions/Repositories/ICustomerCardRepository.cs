namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICustomerCardRepository
{
  Task<IEnumerable<CustomerCard>> GetCustomerCardsByCodeAsync(string code);
  Task<bool> AddCustomerCardAsync(CustomerCard customerCard);
  Task<bool> UpdateCustomerCardAsync(CustomerCard customerCard);
  Task<bool> DeleteCustomerCardByNumberAsync(string customerCardNumber);
  Task<bool> DeleteRewardCardAsync(string customerCardNumber, string rewardCardNumber);
  Task<IEnumerable<RewardCard>> GetRewardCardByNumberAsync(string number);
  Task<bool> IsRewardCardExistByCustomerCardAndRewardCardNumberAsync(string custCardNumber, string rewardCardNumber);
  Task<bool> IsRewardCardExistByCustomerCardAndPhoneAsync(string custCardNumber, string phoneNumber);
  Task<bool> IsCustomerCardExistAsync(string custCardNumber);
  Task<CustomerCard> GetCustomerCardByNumberAsync(string custCardNumber);
  Task<bool> AddRewardCardAsync(RewardCard rewardCard);
  Task<bool> UpdateRewardCardAsync(RewardCard rewardCard);
  Task<bool> IsArCustomerAsync(string code);
  Task<int> GetNextSequenceNumberAsync(string customerCode);
  Task<bool> AddCustomerCardChangeAsync(ClientCardChange clientCardChange);
  Task<bool> DeleteCustomerCardChangeAsync(ClientCardChange clientCardChange);
}
