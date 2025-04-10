namespace InfonetCSCR.Services;

public class HotButtonsService : IHotButtonsService
{
  private readonly IUnitOfWork _unitOfWork;

  public HotButtonsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<HotButtonsWithProductName>> GetHotButtonsWithProductName()
  {
    IEnumerable<HotButtonsWithProductName> hotButtons = await _unitOfWork.HotButtons.GetHotButtonsWithProductNameAsync();
    _unitOfWork.Commit();
    return hotButtons;
  }

  public async Task<IList<HotButtons>> GetHotButtonsByButtonIdRange(HotButtonsIdRangeRecord hotButtonsIdRangeRecord)
  {
    IList<HotButtons> hotButtons = await _unitOfWork.HotButtons.GetHotButtonsByButtonIdRangeAsync(hotButtonsIdRangeRecord);
    _unitOfWork.Commit();
    return hotButtons;
  }

  public async Task<bool> InsertHotButtons(HotButtons hotButtons)
  {
    bool isAdded = await _unitOfWork.HotButtons.InsertHotButtonsAsync(hotButtons);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateHotButtonsColor(HotButtons hotButtons)
  {
    bool isColorUpdated = await _unitOfWork.HotButtons.UpdateHotButtonsColorAsync(hotButtons);
    _unitOfWork.Commit();
    return isColorUpdated;
  }

  public async Task<bool> DeleteHotButton(HotButtons hotButtons)
  {
    bool isDeleted = await _unitOfWork.HotButtons.DeleteHotButtonAsync(hotButtons);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<HotButtons> GetBottleReturnDataByProductCode(string productCode)
  {
    HotButtons hotButton = await _unitOfWork.HotButtons.GetBottleReturnDataByProductCodeAsync(productCode);
    _unitOfWork.Commit();
    return hotButton;
  }
}
