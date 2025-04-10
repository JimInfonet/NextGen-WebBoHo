namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICheckInCheckOutService
{
  CheckInCheckOutPoliciesDtoViewModel GetCheckInCheckOutPolicies(UserDetailsViewModel? userDetails = null);
}
