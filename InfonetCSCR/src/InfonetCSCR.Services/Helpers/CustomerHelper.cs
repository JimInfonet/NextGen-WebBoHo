namespace InfonetCSCR.Services.Helpers;

public static class CustomerHelper
{
  public static Customer GetCustomerFromCustomerForm(CustomerFormDto customerForm)
  {
    Customer customer = new Customer
    {
      Code = customerForm.Code,
      Name = customerForm.Name,
      Category = customerForm.Category,
      Address1 = customerForm.Address1,
      Address2 = customerForm.Address2,
      City = customerForm.City,
      Province = customerForm.Province,
      Country = customerForm.Country,
      PostalCode = customerForm.PostalCode,
      AreaCode = customerForm.AreaCode,
      Phone = customerForm.Phone,
      OfficePhone = customerForm.OfficePhone,
      CellPhone = customerForm.CellPhone,
      Fax = customerForm.Fax,
      Toll = customerForm.Toll,
      Email = customerForm.Email,
      Contact1 = customerForm.Contact1,
      Contact2 = customerForm.Contact2,
      LoyaltyNumber = customerForm.LoyaltyNumber,
      LoyaltyPoints = customerForm.LoyaltyPoints,
      ARCustomer = customerForm.ARCustomer,
      Type = customerForm.Type,
      Terms = customerForm.Terms,
      DiscountDays = customerForm.DiscountDays,
      EarlyPayPercent = customerForm.EarlyPayPercent,
      CustomerDiscount = customerForm.CustomerDiscount,
      PriceCode = customerForm.PriceCode,
      HistoricalBalance = customerForm.HistoricalBalance,
      HistoricalPaid = customerForm.HistoricalPaid,
      CreditLimit = customerForm.CreditLimit,
      CurrentBalance = customerForm.CurrentBalance,
      Status = customerForm.Status,
      ParentAccount = customerForm.ParentAccount,
      UsePurchaseOrder = customerForm.UsePurchaseOrder,
      GroupID = customerForm.GroupID,
      UseFuelRebate = customerForm.UseFuelRebate,
      UseFuelRebateDiscount = customerForm.UseFuelRebateDiscount,
      Note = customerForm.Note,
      TaxExempt = customerForm.TaxExempt,
      PlateNumber = customerForm.PlateNumber,
      MultiUsePurchaseOrder = customerForm.MultiUsePurchaseOrder,
      CardNumber = customerForm.CardNumber,
      ProfileID = customerForm.ProfileID
    };

    return customer;
  }

  public static ArCustomer GetArCustomerFromCustomerForm(CustomerFormDto customerForm)
  {
    ArCustomer arCustomer = new ArCustomer
    {
      Code = customerForm.Code,
      StmtEmail = customerForm.StmtEmail,
      DateOpen = customerForm.DateOpen,
      LastStmtBalance = customerForm.LastStmtBalance,
      LastStmtDate = customerForm.LastStmtDate,
      LastSaleDate = customerForm.LastSaleDate,
      HighStmtBalance = customerForm.HighStmtBalance,
      HighSaleDate = customerForm.HighSaleDate,
      LastPaymentBalance = customerForm.LastPaymentBalance,
      LastPaymentDate = customerForm.LastPaymentDate
    };

    return arCustomer;
  }

  public static CustomerFormDto? CreateCustomerFormDto(Customer customer, ArCustomer? arCustomer)
  {
    if (customer == null)
      return null;

    CustomerFormDto customerForm = new CustomerFormDto
    {
      Code = customer.Code,
      Name = customer.Name,
      Category = customer.Category,
      Address1 = customer.Address1,
      Address2 = customer.Address2,
      City = customer.City,
      Province = customer.Province,
      Country = customer.Country,
      PostalCode = customer.PostalCode,
      AreaCode = customer.AreaCode,
      Phone = customer.Phone,
      OfficePhone = customer.OfficePhone,
      CellPhone = customer.CellPhone,
      Fax = customer.Fax,
      Toll = customer.Toll,
      Email = customer.Email,
      Contact1 = customer.Contact1,
      Contact2 = customer.Contact2,
      LoyaltyNumber = customer.LoyaltyNumber,
      LoyaltyPoints = customer.LoyaltyPoints,
      ARCustomer = customer.ARCustomer,
      Type = customer.Type,
      Terms = customer.Terms,
      DiscountDays = customer.DiscountDays,
      EarlyPayPercent = customer.EarlyPayPercent,
      CustomerDiscount = customer.CustomerDiscount,
      PriceCode = customer.PriceCode,
      HistoricalBalance = customer.HistoricalBalance,
      HistoricalPaid = customer.HistoricalPaid,
      CreditLimit = customer.CreditLimit,
      CurrentBalance = customer.CurrentBalance,
      Status = customer.Status,
      ParentAccount = customer.ParentAccount,
      UsePurchaseOrder = customer.UsePurchaseOrder,
      GroupID = customer.GroupID,
      UseFuelRebate = customer.UseFuelRebate,
      UseFuelRebateDiscount = customer.UseFuelRebateDiscount,
      Note = customer.Note,
      TaxExempt = customer.TaxExempt,
      PlateNumber = customer.PlateNumber,
      MultiUsePurchaseOrder = customer.MultiUsePurchaseOrder,
      CardNumber = customer.CardNumber,
      ProfileID = customer.ProfileID
    };

    if (arCustomer != null)
    {
      customerForm.StmtEmail = arCustomer.StmtEmail;
      customerForm.DateOpen = arCustomer.DateOpen;
      customerForm.LastStmtBalance = arCustomer.LastStmtBalance;
      customerForm.LastStmtDate = arCustomer.LastStmtDate;
      customerForm.LastSaleDate = arCustomer.LastSaleDate;
      customerForm.HighStmtBalance = arCustomer.HighStmtBalance;
      customerForm.HighSaleDate = arCustomer.HighSaleDate;
      customerForm.LastPaymentBalance = arCustomer.LastPaymentBalance;
      customerForm.LastPaymentDate = arCustomer.LastPaymentDate;
    }

    return customerForm;
  }
}
