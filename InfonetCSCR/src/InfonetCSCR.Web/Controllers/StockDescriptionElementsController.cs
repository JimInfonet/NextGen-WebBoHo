namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class StockDescriptionElementsController : Controller
{
  private readonly IBrandService _brandService;
  private readonly IGenericService _genericService;
  private readonly ICharacteristicService _characteristicService;
  private readonly IFamilyService _familyService;
  private readonly IUnitOfMeasureService _unitOfMeasureService;
  private readonly IOrderUnitService _orderUnitService;
  private readonly IGroupService _groupService;

  public StockDescriptionElementsController(
    IBrandService brandService,
    IGenericService genericService,
    ICharacteristicService characteristicService, 
    IFamilyService familyService,
    IUnitOfMeasureService unitOfMeasureService,
    IOrderUnitService orderUnitService,
    IGroupService groupService)
  {
    _brandService = brandService;
    _genericService = genericService;
    _characteristicService = characteristicService;
    _familyService = familyService;
    _unitOfMeasureService = unitOfMeasureService;
    _orderUnitService = orderUnitService;
    _groupService = groupService;
  }

  public IActionResult Index()
  {
    return View();
  }
  public async Task<IActionResult> GetAllBrands()
  {
    IList<BrandViewModel>? brands = await _brandService.GetAllBrands();
    return Ok(brands);
  }

  public async Task<IActionResult> SaveBrand([FromBody] BrandViewModel brand, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _brandService.UpdateBrand(brand) : _brandService.AddBrand(brand));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteBrand(string brandId)
  {
    var isDeleted = await _brandService.DeleteBrand(brandId);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllGenerics()
  {
    IList<GenericViewModel>? generics = await _genericService.GetAllGenerics();
    return Ok(generics);
  }

  public async Task<IActionResult> SaveGeneric([FromBody] GenericViewModel generic, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _genericService.UpdateGeneric(generic) : _genericService.AddGeneric(generic));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteGeneric(string genericId)
  {
    var isDeleted = await _genericService.DeleteGeneric(genericId);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllCharacteristics()
  {
    IList<CharacteristicViewModel>? characteristics = await _characteristicService.GetAllCharacteristics();
    return Ok(characteristics);
  }

  public async Task<IActionResult> SaveCharacteristic([FromBody] CharacteristicViewModel characteristic, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _characteristicService.UpdateCharacteristic(characteristic) : _characteristicService.AddCharacteristic(characteristic));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteCharacteristic(string characteristicId)
  {
    var isDeleted = await _characteristicService.DeleteCharacteristic(characteristicId);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllFamily()
  {
    IList<FamilyViewModel>? families = await _familyService.GetAllFamily();
    return Ok(families);
  }

  public async Task<IActionResult> SaveFamily([FromBody] FamilyViewModel family, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _familyService.UpdateFamily(family) : _familyService.AddFamily(family));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteFamily(string familyId)
  {
    var isDeleted = await _familyService.DeleteFamily(familyId);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllUnitOfMeasures()
  {
    IList<UnitOfMeasureViewModel>? unitOfMeasures = await _unitOfMeasureService.GetAllUnitOfMeasures();
    return Ok(unitOfMeasures);
  }

  public async Task<IActionResult> SaveUnitOfMeasure([FromBody] UnitOfMeasureViewModel unitOfMeasure, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _unitOfMeasureService.UpdateUnitOfMeasure(unitOfMeasure) : _unitOfMeasureService.AddUnitOfMeasure(unitOfMeasure));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteUnitOfMeasure(string unitOfMeasureId)
  {
    var isDeleted = await _unitOfMeasureService.DeleteUnitOfMeasure(unitOfMeasureId);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllOrderUnits()
  {
    IList<OrderUnitViewModel>? orderUnits = await _orderUnitService.GetAllOrderUnits();
    return Ok(orderUnits);
  }

  public async Task<IActionResult> SaveOrderUnit([FromBody] OrderUnitViewModel orderUnit, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _orderUnitService.UpdateOrderUnit(orderUnit) : _orderUnitService.AddOrderUnit(orderUnit));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteOrderUnit(string orderUnitId)
  {
    var isDeleted = await _orderUnitService.DeleteOrderUnit(orderUnitId);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllGroups()
  {
    IList<FamilyGroupJoinFamilyViewModel>? groups = await _groupService.GetAllGroups();
    return Ok(groups);
  }

  public async Task<IActionResult> AddGroup([FromBody] FamilyGroupRecordViewModel group)
  {
    var isAdded = await _groupService.AddGroup(group);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeleteGroup([FromBody] FamilyGroupJoinFamilyViewModel group)
  {
    var isDeleted = await _groupService.DeleteGroup(group);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllFamilyGroups()
  {
    IList<FamilyGroupViewModel>? groups = await _groupService.GetAllFamilyGroups();
    return Ok(groups);
  }
}
