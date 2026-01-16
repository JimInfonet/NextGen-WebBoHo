using InfonetCSCR.Core.Abstractions.Repositories;
using InfonetCSCR.Core.Abstractions.Services;

namespace InfonetCSCR.Data;

public class UnitOfWork : IUnitOfWork
{
    #region private fields
    private IUserRepository? _userRepository;
    private ICheckInCheckOutRepository? _checkInCheckOutRepository;
    private ITimesheetReportsRepository? _timesheetReportsRepository;
    private IStockItemRepository? _stockItemRepository;
    private IStockItemReportsRepository? _stockItemReportsRepository;
    private IStockDescriptionElementsRepository? _stockItemFormDataRepository;
    private ICategoryRepository? _categoryRepository;
    private IVendorRepository? _vendorRepository;
    private IPLUMastRepository? _plumastRepository;
    private IStockBranchRepository? _stockBranchRepository;
    private IStockCostRepository? _stockCostRepository;
    private IStockPriceRepository? _stockPriceRepository;
    private IPricebookTransferRepository? _pricebookTransferRepository;
    private IPriceLRepository? _priceLRepository;
    private IStockRebateRepository? _stockRebateRepository;
    private IStockTaxRepository? _stockTaxRepository;
    private IStockAcRepository? _stockAcRepository;
    private ICompanySetupRepository? _companySetupRepository;
    private IKitMasterRepository? _kitMasterRepository;
    private IKitItemRepository? _kitItemRepository;
    private IVendorPartRepository? _vendorPartRepository;
    private ISaleRepository? _saleRepository;
    private IProductTaxExemptRepository? _productTaxExemptRepository;
    private IStockTransferRepository? _stockTransferRepository;
    private ITaxExemptCategoryRepository? _taxExemptCategoryRepository;
    private ITaxExemptVendorsRepository? _taxExemptVendorsRepository;
    private ITaxExemptRateRepository? _taxExemptRateRepository;
    private IFuelRepository? _fuelRepository;
    private ICardRepository? _cardRepository;
    private ITenderCardRepository? _tenderCardRepository;
    private ICardCodesRepository? _cardCodesRepository;
    private ICardProductRestrictionRepository? _cardProductRestrictionRepository;
    private IOptionalDataRepository? _optionalDataRepository;
    private ITillsSaleTendRepository? _tillsSaleTendRepository;
    private ITransSaleTendRepository? _transSaleTendRepository;
    private IPriceGroupRepository? _priceGroupRepository;
    private ITaxMasterRepository? _taxMasterRepository;
    private IAssociationRepository? _associationRepository;
    private IStoreLinkRepository? _storeLinkRepository;
    private IRegionRepository? _region;
    private IStoreRepository? _store;
    private IPolicyRepository? _policyRepository;
    private IStockItemDetailsRepository? _stockItemDetailsRepository;
    private ITokenRepository? _tokenRepository;
    private ITendersRepository? _tendersRepository;
    private IHotButtonsRepository? _hotButtonsRepository;
    private IDataDefinitionRepository? _dataDefinitionRepository;
    private readonly IApplicationConfig _applicationConfig;
    private IDataTransferReportsRepository? _dataTransferReportsRepository;
    private IEODGroupRepository? _eodGroupRepository;
    private ISubDepartmentRepository _subDepartmentRepository;
    private ISubDetailRepository _subDetailRepository;
    private IPromotionalItemsRepository? _promotionalItemsRepository;
    private ICustomerRepository? _customerRepository;
    private IStockTakesAndReceivingRepository? _stockTakesAndReceivingRepository;
    private IDbConnection _connection;
    private IDbTransaction _transaction;
    private ICardRestrictionProfileRepository _cardRestrictionProfileRepository;
    private IPurchaseHeaderRepository _purchaseHeaderRepository;
    private ICustomerCardRepository? _customerCardRepository;
    private IPurchaseDetailRepository _purchaseDetailRepository;
    private IAddItemsToThePORepository _addItemsToThePORepository;
    private IVendorSortRepository _vendorSortRepository;
    private IAdminRepository _adminRepository;
    private IDashboardChartRepository? _dashboardChartRepository;
    private IDiscountTableRepository _discountTableRepository;
    private IHotButtonPagesRepository _hotButtonPagesRepository;
    private IFunctionButtonsRepository _functionButtonsRepository;
    private IFunctionListRepository _functionListRepository;
    private ICustomerGroupRepository? _customerGroupRepository;
    private ICashBonusRepository? _cashBonusRepository;
    private IDiscountChartRepository? _discountChartRepository;
    private IGroupButtonsRepository _groupButtonsRepository;
    private ICashDrawsRepository _cashDrawsRepository;
    private IRestrictionsRepository? _restrictionsRepository;
    private IReasonsRepository? _reasonsRepository;
    private bool _disposed;
    private ITaxRateRepository? _taxRepository;
    private IPositiveCardRepository? _positiveCardRepository;
    private INegativeCardRepository? _negativeCardRepository;
    private ICardMessageRepository? _cardMessageRepository;
    private IBrandRepository? _brandRepository;
    private ITimesheetPayPeriodRepository? _timesheetPayPeriodRepository;
    private IUserGroupRepository? _userGroupRepository;
    private IUigRepository? _uig;
    private IGenericRepository? _genericRepository;
    private ISecurityLevelRepository? _userGroupSecurityLevel;
    private ICharacteristicRepository? _characteristicRepository;
    private IFamilyRepository? _familyRepository;
    private ICommissionRepository? _commissionRepository;
    private IUnitOfMeasureRepository? _unitOfMeasureRepository;
    private IRegistersRepository? _registersRepository;
    private IOrderUnitRepository? _orderUnitRepository;
    private IShiftRepository _shiftRepository;
    private IGroupRepository? _groupRepository;
    private IGradeRepository? _gradeRepository;
    private ITierRepository? _tierRepository;
    private IServiceLevelRepository? _serviceLevelRepository;
    private ITierLevelPriceDifferenceRepository? _tierLevelPriceDifferenceRepository;
    private ISaleHeadRepository? _saleHeadRepository;
    private IFuelPriceHistoryRepository? _fuelPriceHistoryRepository;
    private ITillsRepository? _tillsRepository;
    private IPolicyByClassRepository? _policyByClassRepository;
    private ITextReportsRepository? _textReportsRepository;
    private IElectronicJournalRepository? _electronicJournalRepository;
    private IPolicyByEntityRepository? _policyByEntityRepository;
    private IStoreShiftsRepository? _storeShiftsRepository;
    private IFuelPriceChangeRepository? _fuelPriceChangeRepository;
    private IPricebookManagementRepository? _pricebookManagementRepository;
    private ISendDataToStoreRepository? _sendDataToStoreRepository;
    private IExcelReportsRepository? _excelReportsRepository;
    private IReportRepository? _reportRepository;
    private IRepTillCloseRepository? _repTillCloseRepository;
    private IRepCloseBatchRepository? _repCloseBatchRepository;
    private IRepArStatRepository? _repArStatementRepository;
    private IUserService? _userService;
    #endregion private fields

    public UnitOfWork(IApplicationConfig _applicationConfig)
    {
        this._applicationConfig = _applicationConfig;
    }

    public UnitOfWork(IApplicationConfig _applicationConfig, string? connectionString)
    {
        this._applicationConfig = _applicationConfig;
        if (_applicationConfig.IsBackOffice == false && connectionString == _applicationConfig.CSCTillsConnectionString)
        {
            return;
        }
        _connection = new SqlConnection(connectionString);
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    public void BeginTransaction(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    public bool Commit()
    {
        bool isSuccess = false;
        try
        {
            _transaction.Commit();
            isSuccess = true;
        }
        catch
        {
            _transaction.Rollback();
            throw;
        }
        finally
        {
            _transaction.Dispose();
            _transaction = _connection.BeginTransaction();
            GC.Collect();
        }

        return isSuccess;
    }

    public void Dispose()
    {
        dispose(true);
        GC.SuppressFinalize(this);
        GC.Collect();
    }

    private void dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
            _disposed = true;
        }
    }
    ~UnitOfWork()
    {
        dispose(false);
    }

    #region properties
    public IUserRepository Users => _userRepository ??= new UserRepository(_transaction);
    public ICheckInCheckOutRepository CheckInCheckOut => _checkInCheckOutRepository ??= new CheckInCheckOutRepository(_applicationConfig, _transaction);
    public ITimesheetReportsRepository TimesheetReports => _timesheetReportsRepository ??= new TimesheetReportsRepository(_transaction);
    public IStockItemRepository StockItems => _stockItemRepository ??= new StockItemRepository(_applicationConfig, _transaction);
    public IStockItemReportsRepository StockItemReports => _stockItemReportsRepository ??= new StockItemReportsRepository(_transaction);
    public IStockDescriptionElementsRepository StockItemFormData => _stockItemFormDataRepository ??= new StockDescriptionElementsRepository(_transaction);
    public IVendorRepository Vendors => _vendorRepository ??= new VendorRepository(_transaction);
    public ICategoryRepository Category => _categoryRepository ??= new CategoryRepository(_transaction);
    public IPLUMastRepository PLUMasts => _plumastRepository ??= new PLUMastRepository(_transaction);
    public IStockBranchRepository StockBranches => _stockBranchRepository ??= new StockBranchRepository(_transaction);
    public IStockCostRepository StockCosts => _stockCostRepository ??= new StockCostRepository(_applicationConfig, _transaction);
    public IStockPriceRepository StockPrices => _stockPriceRepository ??= new StockPriceRepository(_applicationConfig, _transaction);
    public IPricebookTransferRepository PricebookTransfer => _pricebookTransferRepository ??= new PricebookTransferRepository(_applicationConfig, _transaction);
    public IPriceLRepository PriceLs => _priceLRepository ??= new PriceLRepository(_applicationConfig, _transaction);
    public IStockRebateRepository StockRebates => _stockRebateRepository ??= new StockRebateRepository(_applicationConfig, _transaction);
    public IStockTaxRepository StockTaxes => _stockTaxRepository ??= new StockTaxRepository(_applicationConfig, _transaction);
    public IStockAcRepository StockAcs => _stockAcRepository ??= new StockAcRepository(_applicationConfig, _transaction);
    public ICompanySetupRepository CompanySetups => _companySetupRepository ??= new CompanySetupRepository(_applicationConfig, _transaction);
    public IKitMasterRepository KitMasters => _kitMasterRepository ??= new KitMasterRepository(_applicationConfig, _transaction);
    public IKitItemRepository KitItems => _kitItemRepository ??= new KitItemRepository(_transaction);
    public IVendorPartRepository VendorParts => _vendorPartRepository ??= new VendorPartRepository(_transaction);
    public ISaleRepository Sales => _saleRepository ??= new SaleRepository(_transaction);
    public IProductTaxExemptRepository ProductTaxExempts => _productTaxExemptRepository ??= new ProductTaxExemptRepository(_applicationConfig, _transaction);
    public IStockTransferRepository StockTransfers => _stockTransferRepository ??= new StockTransferRepository(_applicationConfig, _transaction);
    public IStoreRepository Store => _store ??= new StoreRepository(_transaction);
    public IRegionRepository Region => _region ??= new RegionRepository(_transaction);
    public IPolicyRepository Policy => _policyRepository ??= new PolicyRepository(_transaction);
    public IStockItemDetailsRepository StockItemDetails => _stockItemDetailsRepository ??= new StockItemDetailsRepository(_applicationConfig, _transaction);
    public ITaxExemptCategoryRepository TaxExemptCategories => _taxExemptCategoryRepository ??= new TaxExemptCategoryRepository(_transaction);
    public ITaxExemptVendorsRepository TaxExemptVendors => _taxExemptVendorsRepository ??= new TaxExemptVendorsRepository(_transaction);
    public ITaxExemptRateRepository TaxExemptRates => _taxExemptRateRepository ??= new TaxExemptRateRepository(_transaction);
    public IFuelRepository Fuels => _fuelRepository ??= new FuelRepository(_transaction);
    public ITokenRepository Token => _tokenRepository ??= new TokenRepository(_transaction);
    public ICardRepository Cards => _cardRepository ??= new CardRepository(_transaction);
    public ITenderCardRepository TenderCards => _tenderCardRepository ??= new TenderCardRepository(_transaction);
    public ITendersRepository Tenders => _tendersRepository ??= new TendersRepository(_transaction);
    public ICardCodesRepository CardCodes => _cardCodesRepository ??= new CardCodesRepository(_transaction);
    public ICardProductRestrictionRepository CardProductRestrictions => _cardProductRestrictionRepository ??= new CardProductRestrictionRepository(_transaction);
    public IOptionalDataRepository OptionalDatas => _optionalDataRepository ??= new OptionalDataRepository(_transaction);
    public ITillsSaleTendRepository TilllsSaleTends => _tillsSaleTendRepository ??= new TillsSaleTendRepository(_transaction);
    public ITransSaleTendRepository TransSaleTends => _transSaleTendRepository ??= new TransSaleTendRepository(_transaction);
    public IHotButtonsRepository HotButtons => _hotButtonsRepository ??= new HotButtonsRepository(_transaction);
    public IPriceGroupRepository PriceGroups => _priceGroupRepository ??= new PriceGroupRepository(_transaction);
    public ITaxMasterRepository TaxMasters => _taxMasterRepository ??= new TaxMasterRepository(_transaction);
    public IAssociationRepository Associations => _associationRepository ??= new AssociationRepository(_transaction);
    public IStoreLinkRepository StoreLinks => _storeLinkRepository ??= new StoreLinkRepository(_transaction);
    public IDataDefinitionRepository DataDefinitions => _dataDefinitionRepository ??= new DataDefinitionRepository(_transaction);
    public IDataTransferReportsRepository DataTransferReports => _dataTransferReportsRepository ??= new DataTransferReportsRepository(_applicationConfig, _transaction);
    public IEODGroupRepository EODGroups => _eodGroupRepository ??= new EODGroupRepository(_transaction);
    public ISubDepartmentRepository SubDepartments => _subDepartmentRepository ??= new SubDepartmentRepository(_transaction);
    public ISubDetailRepository SubDetails => _subDetailRepository ??= new SubDetailRepository(_transaction);
    public IPromotionalItemsRepository PromotionalItems => _promotionalItemsRepository ??= new PromotionalItemsRepository(_applicationConfig, _transaction);
    public ICustomerRepository Customer => _customerRepository ??= new CustomerRepository(_transaction);
    public IStockTakesAndReceivingRepository StockTakesAndReceiving => _stockTakesAndReceivingRepository ??= new StockTakesAndReceivingRepository(_transaction);
    public ICardRestrictionProfileRepository CardRestrictionProfileRepository => _cardRestrictionProfileRepository ??= new CardRestrictionProfileRepository(_transaction);
    public IPurchaseHeaderRepository PurchaseHeaders => _purchaseHeaderRepository ??= new PurchaseHeaderRepository(_transaction);
    public ICustomerCardRepository CustomerCard => _customerCardRepository ??= new CustomerCardRepository(_transaction);
    public IPurchaseDetailRepository PurchaseDetails => _purchaseDetailRepository ??= new PurchaseDetailRepository(_transaction);
    public IAddItemsToThePORepository AddItemsToThePO => _addItemsToThePORepository ??= new AddItemsToThePORepository(_applicationConfig, _transaction);
    public IVendorSortRepository VendorSorts => _vendorSortRepository ??= new VendorSortRepository(_transaction);
    public IAdminRepository Admins => _adminRepository ??= new AdminRepository(_transaction);
    public IDiscountTableRepository DiscountTables => _discountTableRepository ??= new DiscountTableRepository(_transaction);
    public IHotButtonPagesRepository HotButtonPages => _hotButtonPagesRepository ??= new HotButtonPagesRepository(_transaction);
    public IFunctionButtonsRepository FunctionButtons => _functionButtonsRepository ??= new FunctionButtonsRepository(_transaction);
    public IFunctionListRepository FunctionLists => _functionListRepository ??= new FunctionListRepository(_transaction);
    public ICustomerGroupRepository CustomerGroup => _customerGroupRepository ??= new CustomerGroupRepository(_transaction);
    public ICashBonusRepository CashBonus => _cashBonusRepository ??= new CashBonusRepository(_transaction);
    public IDiscountChartRepository DiscountChart => _discountChartRepository ??= new DiscountChartRepository(_transaction);
    public IGroupButtonsRepository GroupButtons => _groupButtonsRepository ??= new GroupButtonsRepository(_transaction);
    public IDashboardChartRepository DashboardCharts => _dashboardChartRepository ??= new DashboardChartRepository(_transaction);
    public ICashDrawsRepository CashDraws => _cashDrawsRepository ??= new CashDrawsRepository(_transaction);
    public ITaxRateRepository TaxRate => _taxRepository ??= new TaxRateRepository(_transaction);
    public IRestrictionsRepository Restrictions => _restrictionsRepository ??= new RestrictionsRepository(_transaction);
    public IPositiveCardRepository PositiveCards => _positiveCardRepository ??= new PositiveCardRepository(_transaction);
    public INegativeCardRepository NegativeCards => _negativeCardRepository ??= new NegativeCardRepository(_transaction);
    public ICardMessageRepository CardMessages => _cardMessageRepository ??= new CardMessageRepository(_transaction);
    public IBrandRepository Brand => _brandRepository ??= new BrandRepository(_transaction);
    public ITimesheetPayPeriodRepository TimesheetPayPeriodRepository => _timesheetPayPeriodRepository ??= new TimesheetPayPeriodRepository(_transaction);
    public IUserGroupRepository UserGroup => _userGroupRepository ??= new UserGroupRepository(_transaction);
    public IUigRepository Uig => _uig ??= new UigRepository(_transaction);
    public IGenericRepository Generic => _genericRepository ??= new GenericRepository(_transaction);
    public ISecurityLevelRepository UserGroupSecurityLevel => _userGroupSecurityLevel ??= new SecurityLevelRepository(_transaction);
    public ICharacteristicRepository Characteristic => _characteristicRepository ??= new CharacteristicRepository(_transaction);
    public IReasonsRepository Reasons => _reasonsRepository ??= new ReasonsRepository(_transaction);
    public IFamilyRepository Family => _familyRepository ??= new FamilyRepository(_transaction);
    public ICommissionRepository Commission => _commissionRepository ??= new CommissionRepository(_transaction);
    public IUnitOfMeasureRepository UnitOfMeasure => _unitOfMeasureRepository ??= new UnitOfMeasureRepository(_transaction);
    public IRegistersRepository Registers => _registersRepository ??= new RegistersRepository(_transaction);
    public IOrderUnitRepository OrderUnit => _orderUnitRepository ??= new OrderUnitRepository(_transaction);
    public IShiftRepository Shift => _shiftRepository ??= new ShiftRepository(_transaction);
    public IGroupRepository Group => _groupRepository ??= new GroupRepository(_transaction);
    public ISaleHeadRepository SaleHeads => _saleHeadRepository ??= new SaleHeadRepository(_transaction);
    public IGradeRepository Grades => _gradeRepository ??= new GradeRepository(_transaction);
    public IFuelPriceHistoryRepository FuelPriceHistory => _fuelPriceHistoryRepository ??= new FuelPriceHistoryRepository(_transaction);
    public ITierRepository Tiers => _tierRepository ??= new TierRepository(_transaction);
    public IServiceLevelRepository ServiceLevels => _serviceLevelRepository ??= new ServiceLevelRepository(_transaction);
    public ITierLevelPriceDifferenceRepository TierLevelPriceDifferences => _tierLevelPriceDifferenceRepository ??= new TierLevelPriceDifferenceRepository(_transaction);
    public ITillsRepository Tills => _tillsRepository ??= new TillsRepository(_transaction);
    public IPolicyByClassRepository PolicyByClass => _policyByClassRepository ??= new PolicyByClassRepository(_transaction);
    public ITextReportsRepository TextReports => _textReportsRepository ??= new TextReportsRepository(_transaction, _applicationConfig);
    public IElectronicJournalRepository ElectronicJournal => _electronicJournalRepository ??= new ElectronicJournalRepository(_applicationConfig, _transaction);
    public IPolicyByEntityRepository PolicyByEntity => _policyByEntityRepository ??= new PolicyByEntityRepository(_transaction);
    public IStoreShiftsRepository StoreShifts => _storeShiftsRepository ??= new StoreShiftsRepository(_transaction);
    public IFuelPriceChangeRepository FuelPriceChange => _fuelPriceChangeRepository ??= new FuelPriceChangeRepository(_transaction);
    public IPricebookManagementRepository PricebookManagement => _pricebookManagementRepository ??= new PricebookManagementRepository(_transaction);
    public ISendDataToStoreRepository SendDataToStore => _sendDataToStoreRepository ??= new SendDataToStoreRepository(_transaction);
    public IExcelReportsRepository ExcelReports => _excelReportsRepository ??= new ExcelReportsRepository(_transaction);
    public IReportRepository Reports => _reportRepository ??= new ReportRepository(_transaction);
    public IRepTillCloseRepository RepTillCloses => _repTillCloseRepository ??= new RepTillCloseRepository(_applicationConfig, _policyRepository, _transaction, _userService);
    //public IRepTillCloseRepository RepTillCloses { get; }
    public IRepCloseBatchRepository RepCloseBatch => _repCloseBatchRepository ??= new RepCloseBatchRepository(_applicationConfig, _transaction);
    public IRepArStatRepository RepArStat => _repArStatementRepository ??= new RepArStatRepository(_transaction);
    #endregion properties
}
