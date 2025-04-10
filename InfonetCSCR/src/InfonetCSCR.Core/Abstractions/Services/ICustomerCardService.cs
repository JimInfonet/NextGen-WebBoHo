namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICustomerCardService
{
  Task<IEnumerable<CustomerCard>> GetCustomerCardsByCode(string code);
  Task<bool> AddCustomerCard(CustomerCardDto customerCard);
  Task<bool> UpdateCustomerCard(CustomerCardDto customerCard);
  Task<IEnumerable<RewardCard>> GetRewardCardsByCardNumber(string number);
  Task<bool> IsRewardCardExistByCustomerCardAndRewardCardNumber(string custCardNumber, string rewardCardNumber);
  Task<bool> IsRewardCardExistByCustomerCardAndPhone(string custCardNumber, string phoneNumber);
  Task<CustomerCard> GetCustomerCardByNumber(string custCardNumber);
  Task<bool> IsCustomerCardExist(string custCardNumber);
  Task<bool> AddRewardCard(RewardCard rewardCard);
  Task<bool> UpdateRewardCard(RewardCard rewardCard);
  Task<bool> IsArCustomer(string code);
  Task<bool> DeleteCustomerCardByNumber(CustomerCardDeleteDto customerCardDeleteDto);
  Task<bool> DeleteRewardCard(string customerCardNumber, string rewardCardNumber);
  Task<int> GetNextSequenceNumber(string customerCode);
}
