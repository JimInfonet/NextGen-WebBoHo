namespace InfonetCSCR.Services;

public class UserService : IUserService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUnitOfWork _unitOfWorkTrans;
  private readonly IEncryptionService _encryptionService;
  private readonly IUserGroupService _userGroupService;
  private readonly IUigService _uigService;

  public UserService(IApplicationConfig applicationConfig, IEncryptionService encryptionService,
    IUserGroupService userGroupService, IUigService uigService)
  {
    _unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCAdminConnectionString);
    _unitOfWorkTrans = new UnitOfWork(applicationConfig, applicationConfig.CSCTransConnectionString);
    _encryptionService = encryptionService;
    _userGroupService = userGroupService;
    _uigService = uigService;
  }

  public async Task<UserDetails> GetUserDetails(string userCode, bool shouldIncludeUserGroup = true)
  {
    UserDetails user = await _unitOfWork.Users.GetUserDetailsAsync(userCode);
    if (shouldIncludeUserGroup)
    {
      UserGroup userGroup = await _unitOfWork.Users.GetUserGroupDetailsAsync(userCode);
      user.UserGroup = userGroup;
    }

    _unitOfWork.Commit();
    return user;
  }

  public async Task<int> GetUserId(string userCode)
  {
    int userId = await _unitOfWork.Users.GetUserId(userCode);
    _unitOfWork.Commit();

    return userId;
  }

  public async Task<User?> GetUserByCode(string code)
  {
    User? user = await _unitOfWork.Users.GetByCodeAsync(code);
    _unitOfWork.Commit();
    return user;
  }

  public async Task<string> GetUserNameByCode(string code)
  {
    string userName = await _unitOfWork.Users.GetUserNameByCodeAsync(code);
    _unitOfWork.Commit();
    return userName;
  }

  public async Task<bool> HasAccessRight(string permission, User user)
  {
    bool hasAccessRight = await _unitOfWork.Users.GetUserAccessRightAsync(permission, user);
    _unitOfWork.Commit();
    return hasAccessRight;
  }

  public async Task<IEnumerable<UserDetails>> GetAllUserDetails(bool timeSheetsAndHaveUserIdPolicy)
  {
    IEnumerable<UserDetails> userDetails = await _unitOfWork.Users.GetAllUserDetailsAsync(timeSheetsAndHaveUserIdPolicy);
    _unitOfWork.Commit();
    return userDetails;
  }

  public async Task<bool> AddUserDetails(UserDetails userDetails, bool timeSheetsAndHaveUserIdPolicy)
  {
    userDetails.Password = _encryptionService.EncryptText(userDetails.Password);

    if (timeSheetsAndHaveUserIdPolicy)
    {
      await _unitOfWorkTrans.TimesheetPayPeriodRepository.IncrementInitialEmployeeId((int)userDetails.Id + 1);
    }

    bool isAdded = await _unitOfWork.Users.AddUserDetailsAsync(userDetails);

    _unitOfWorkTrans.Commit();
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> UpdateUserDetails(UserDetails userDetails)
  {
    bool isUpdated = await _unitOfWork.Users.UpdateUserDetailsAsync(userDetails);
    _unitOfWork.Commit();

    return isUpdated;
  }

  public async Task<bool> DeleteUserDetails(string groupID)
  {
    await _uigService.DeleteUigRecord(new Uig
    {
      User = groupID
    }, false);
    bool isDeleted = await _unitOfWork.Users.DeleteUserDetailsAsync(groupID);
    _unitOfWork.Commit();

    return isDeleted;
  }

  public async Task<int> GetInitialEmployeeId()
  {
    var initialEmployeeId = await _unitOfWorkTrans.TimesheetPayPeriodRepository.GetInitialEmployeeIdAsync();
    _unitOfWorkTrans.Commit();

    return initialEmployeeId;
  }

  public async Task<IEnumerable<GroupInfoDto>?> GetGroupInfo(string userCode)
  {
    IEnumerable<GroupInfoDto>? groupInfo = await _uigService.GetGroupInfo(userCode);
    return groupInfo;
  }

  public async Task<IEnumerable<string>> GetGroupCodesBySecurityLevel(int securityLevel)
  {
    IEnumerable<string> groupInfo = await _userGroupService.GetGroupCodesBySecurityLevel(securityLevel);
    return groupInfo;
  }

  public async Task<bool> AddUIGRecord(Uig uig)
  {
    bool isAdded = await _uigService.AddUigRecord(uig);
    return isAdded;
  }

  public async Task<bool> DeleteUIGRecord(Uig uig, int securityLevelForLoggedInUser)
  {
    int securityLevelForDeleteUIG = await _userGroupService.GetSecurityLevelForDeleteUIG(uig.Group);
    if (securityLevelForDeleteUIG > securityLevelForLoggedInUser)
      return false;

    bool isDeleted = await _uigService.DeleteUigRecord(uig);
    return isDeleted;
  }

  public async Task<IEnumerable<string>?> GetUserCodes(UserDetails userDetails)
  {
    int loggedInUserSecurityLevel = userDetails.UserGroup != null ?
      await _unitOfWork.UserGroup.GetSecurityLevelForDeleteUIGAsync(userDetails.UserGroup.Code) : 0;
    IEnumerable<string>? userCodes = await _unitOfWork.Users.GetUserCodesAsync(loggedInUserSecurityLevel);
    _unitOfWork.Commit();

    return userCodes;
  }

  public async Task ChangeUserPassword(UserDetails userDetails)
  {
    userDetails.Password = _encryptionService.EncryptText(userDetails.Password);
    await _unitOfWork.Users.ChangeUserPasswordAsync(userDetails);
    _unitOfWork.Commit();
  }

  public async Task<int> GetMaxUserIDAsync()
  {
    var maxUserID = await _unitOfWorkTrans.Users.GetMaxUserIDAsync();
    _unitOfWorkTrans.Commit();
    return maxUserID;
  }
}
