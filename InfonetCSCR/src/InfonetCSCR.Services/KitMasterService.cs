namespace InfonetCSCR.Services;

public class KitMasterService : IKitMasterService
{
  private readonly IUnitOfWork _unitOfWork;

  public KitMasterService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<bool> MakeStockItemKit(KitMast KitMast)
  {
    await _unitOfWork.KitMasters.MakeStockItemKitAsync(KitMast);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<KitsDto>> GetKits()
  {
    IEnumerable<KitsDto> result = await _unitOfWork.KitMasters.GetKitsAsync();
    _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<KitItemsDto>> GetKitItemsByKitCodeWithDesc(string stockCode)
  {
    IEnumerable<KitItemsDto> result = await _unitOfWork.KitMasters.GetKitItemsByKitCodeWithDescAsync(stockCode);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<KitItemsDto>> GetKitItems(bool isSellInactive)
  {
    IEnumerable<KitItemsDto> result = await _unitOfWork.KitMasters.GetKitItemsAsync(isSellInactive);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> AddKitItem(KitItemsDto kitItem)
  {
    bool result = await _unitOfWork.KitMasters.AddKitItemAsync(kitItem);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> IsKitItem(string stockCode)
  {
    bool result = await _unitOfWork.KitMasters.IsKitItemAsync(stockCode);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> IsItemAlreadyInKit(KitItemsDto kitItem)
  {
    bool result = await _unitOfWork.KitMasters.IsItemAlreadyInKitAsync(kitItem);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> UpdateKitItem(KitItemsDto kitItem)
  {
    bool result = await _unitOfWork.KitMasters.UpdateKitItemAsync(kitItem);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> DeleteKitItem(KitItemsDto kitItem)
  {
    bool result = await _unitOfWork.KitMasters.DeleteKitItemAsync(kitItem);
    _unitOfWork.Commit();
    return result;
  }
}
