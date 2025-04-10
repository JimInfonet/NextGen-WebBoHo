namespace InfonetCSCR.Core.Abstractions.Services;

public interface IFunctionListService
{
  Task<IList<FunctionList>> GetAllFunctionListData();
  Task<IList<FunctionList>> GetFunctionListByButtonType(int buttonType);
}
