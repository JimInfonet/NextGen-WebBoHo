namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICheckInCheckOutService
{
  Task<IEnumerable<CheckInCheckOutUserDto>> GetCurrentUsers();
  Task<DetermineInOrOutDto> DetermineInOrOut(bool policyHOUR24STORE, string userCode);
  Task<bool> AddComment(AddCommentDto model);
  Task<PerformCheckInOrOutResponseDto> PerformCheckInOrOut(PerformCheckInOrOutDto infos);
  Task CalculateCommision(string userCode, DateTime shiftDate);
}
