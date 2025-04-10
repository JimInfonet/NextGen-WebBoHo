namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IFunctionListRepository
{
  Task<IList<FunctionList>> GetAllFunctionListDataAsync();
  Task<IList<FunctionList>> GetFunctionListByButtonTypeAsync(int buttonType);
}
