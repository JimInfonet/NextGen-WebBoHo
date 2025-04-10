namespace InfonetCSCR.Web.Abstractions.Services;

public interface IFunctionListService
{
  Task<IList<FunctionListViewModel>?> GetAllFunctionListData();
  Task<IList<FunctionListViewModel>?> GetFunctionListByButtonType(int buttonType);
}
