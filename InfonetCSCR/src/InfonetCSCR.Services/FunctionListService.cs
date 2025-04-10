namespace InfonetCSCR.Services;

public class FunctionListService : IFunctionListService
{
  private readonly IUnitOfWork _unitOfWork;

  public FunctionListService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<FunctionList>> GetAllFunctionListData()
  {
    IList<FunctionList> functionLists = await _unitOfWork.FunctionLists.GetAllFunctionListDataAsync();
    _unitOfWork.Commit();

    return functionLists;
  }

  public async Task<IList<FunctionList>> GetFunctionListByButtonType(int buttonType)
  {
    IList<FunctionList> functionLists = await _unitOfWork.FunctionLists.GetFunctionListByButtonTypeAsync(buttonType);
    _unitOfWork.Commit();

    return functionLists;
  }
}
