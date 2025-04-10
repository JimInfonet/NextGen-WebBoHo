namespace InfonetCSCR.Services;

public class CustomerService : ICustomerService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;

  public CustomerService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
    _config = applicationConfig;
  }

  public async Task<IEnumerable<CustomersDto>> GetAllCustomers(bool orderByCode)
  {
    IEnumerable<CustomersDto> customers = await _unitOfWork.Customer.GetAllCustomersAsync(orderByCode);
    _unitOfWork.Commit();
    return customers;
  }

  public async Task<bool> DeleteClientCard(string code)
  {
    bool isDeleted = await _unitOfWork.Customer.DeleteClientCardByCodeAsync(code);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> DeleteClient(CustomerFormDto customerForm)
  {
    Customer? customer = await _unitOfWork.Customer.GetCustomerAsync(customerForm.Code);
    bool isDeleted = await _unitOfWork.Customer.DeleteClientAsync(customerForm.Code);
    bool isArCustomerDeleted = true;

    if (isDeleted)
    {
      if (_config.IsBackOffice == false)
      {
        ArCustomer? existArCustomer = await GetArCustomer(customerForm.Code);
        if (existArCustomer != null)
        {
          isArCustomerDeleted = await DeleteArCustomer(customerForm.Code);
        }

        await _unitOfWork.Customer.DeleteClientCardByCodeAsync(customerForm.Code);

        if (customerForm.IsSyncCustInfo == true && customer?.ARCustomer == true)
        {
          ClientChanges clientChange = new ClientChanges
          {
            Code = customerForm.Code,
            TransFlag = "N",
            OPStatus = "N"
          };

          bool success = await DeleteClientChange(clientChange);
          if (!success)
          {
            clientChange.OPStatus = "D";
            await AddClientChange(clientChange);
          }
        }
      }
    }

    _unitOfWork.Commit();
    return isDeleted && isArCustomerDeleted;
  }

  public async Task<CustomerFormDto> GetCustomerByCode(string code, string? loyaltyNum)
  {
    Customer? customer = null;
    ArCustomer? arCustomer = null;
    if (loyaltyNum != null && loyaltyNum != "")
    {
      customer = await GetCustomerByLoyaltyNum(loyaltyNum);
    }
    else if (code != null)
    {
      customer = await _unitOfWork.Customer.GetCustomerAsync(code);
    }

    if (_config.IsBackOffice == false && code != null)
    {
      arCustomer = await GetArCustomer(code);
    }

    CustomerFormDto? customerForm = null;
    if (customer != null)
    {
      customerForm = CustomerHelper.CreateCustomerFormDto(customer, arCustomer);
    }

    _unitOfWork.Commit();
    return customerForm;
  }

  private async Task<Customer> GetCustomerByLoyaltyNum(string loyaltyNum)
  {
    Customer customer = await _unitOfWork.Customer.GetCustomerByLoyaltyNumAsync(loyaltyNum);
    return customer;
  }

  public async Task<bool> AddCustomer(CustomerFormDto customerForm)
  {
    Customer customer = CustomerHelper.GetCustomerFromCustomerForm(customerForm);
    ArCustomer arCustomer = CustomerHelper.GetArCustomerFromCustomerForm(customerForm);
    bool isAdded = await _unitOfWork.Customer.AddCustomerAsync(customer);
    bool isArCustomerAdded = true;

    if (_config.IsBackOffice == false)
    {
      ArCustomer? existingArCustomer = await GetArCustomer(customer.Code);
      isArCustomerAdded = (existingArCustomer == null) ? await AddArCustomer(arCustomer) : await UpdateArCustomer(arCustomer);
    }

    if (isAdded && customerForm.IsSyncCustInfo == true && customer.ARCustomer == true && !_config.IsBackOffice)
    {
      ClientChanges clientChange = new ClientChanges()
      {
        Code = customer.Code,
        TransFlag = "N",
        OPStatus = "N" // OPStatus = U when adding
      };

      await AddClientChange(clientChange);
    }
    _unitOfWork.Commit();
    return isAdded && isArCustomerAdded;
  }

  public async Task<bool> UpdateCustomer(CustomerFormDto customerForm)
  {

    Customer customer = CustomerHelper.GetCustomerFromCustomerForm(customerForm);
    ArCustomer arCustomer = CustomerHelper.GetArCustomerFromCustomerForm(customerForm);
    bool isCustomerUpdated = await _unitOfWork.Customer.UpdateCustomerAsync(customer);

    bool isArCustomerUpdated = true;
    if (!_config.IsBackOffice)
    {
      isArCustomerUpdated = await UpdateArCustomer(arCustomer);
    }

    if (isCustomerUpdated && customerForm.IsSyncCustInfo == true && customer.ARCustomer == true && !_config.IsBackOffice)
    {
      ClientChanges clientChange = new ClientChanges()
      {
        Code = customer.Code,
        TransFlag = "N",
        OPStatus = "U" // OPStatus = U when editing
      };

      await AddClientChange(clientChange);
    }
    _unitOfWork.Commit();
    return isCustomerUpdated && isArCustomerUpdated;
  }

  public async Task<IEnumerable<CustomerGroup>> GetAllCustomerGroups()
  {
    IEnumerable<CustomerGroup> groups = await _unitOfWork.Customer.GetAllCustomerGroupsAsync();
    _unitOfWork.Commit();
    return groups;
  }

  private async Task<bool> AddClientChange(ClientChanges clientChange)
  {
    bool isAdded = await _unitOfWork.Customer.AddClientChangeAsync(clientChange);
    return isAdded;
  }

  private async Task<bool> DeleteClientChange(ClientChanges clientChange)
  {
    bool success = await _unitOfWork.Customer.DeleteClientChangeAsync(clientChange);
    return success;
  }

  private async Task<bool> AddArCustomer(ArCustomer arCustomer)
  {
    bool isAdded = await _unitOfWork.Customer.AddArCustomerAsync(arCustomer);
    return isAdded;
  }

  private async Task<ArCustomer> GetArCustomer(string code)
  {
    ArCustomer arCustomer = await _unitOfWork.Customer.GetArCustomerAsync(code);
    return arCustomer;
  }

  private async Task<bool> UpdateArCustomer(ArCustomer arCustomer)
  {
    bool isUpdated = await _unitOfWork.Customer.UpdateArCustomerAsync(arCustomer);
    return isUpdated;
  }

  private async Task<bool> DeleteArCustomer(string code)
  {
    bool isDeleted = await _unitOfWork.Customer.DeleteArCustomerAsync(code);
    return isDeleted;
  }

  public async Task<bool> GetSecurityAR()
  {
    string posBOFeatures = await _unitOfWork.Customer.GetSecurityARAsync();
    bool securityAR = posBOFeatures.StartsWith("1");
    _unitOfWork.Commit();
    return securityAR;
  }
}
