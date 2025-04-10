namespace InfonetCSCR.Api.Controllers;

[Route("api/CustomerCard")]
[ApiController]
[Authorize]

public class CustomersCardController : ControllerBase
{
  public readonly ICustomerCardService _customerCardService;

  public CustomersCardController(ICustomerCardService customerCardService)
  {
    _customerCardService = customerCardService;
  }

  // Get api/CustomerCard/GetCustomerCardsByCode
  [HttpGet]
  [Route("GetCustomerCardsByCode")]
  public async Task<IActionResult> GetCustomerCardsByCode([FromQuery] string code)
  {
    IEnumerable<CustomerCard> customers = await _customerCardService.GetCustomerCardsByCode(code);
    return Ok(customers);
  }

  // POST api/CustomerCard/AddCustomerCard
  [HttpPost]
  [Route("AddCustomerCard")]
  public async Task<IActionResult> AddCustomerCard([FromBody] CustomerCardDto customer)
  {
    var result = await _customerCardService.AddCustomerCard(customer);
    return Ok(result);
  }

  // POST api/CustomerCard/UpdateCustomerCard
  [HttpPost]
  [Route("UpdateCustomerCard")]
  public async Task<IActionResult> UpdateCustomerCard([FromBody] CustomerCardDto customer)
  {
    bool isUpdated = await _customerCardService.UpdateCustomerCard(customer);
    return Ok(isUpdated);
  }

  // POST api/CustomerCard/DeleteCustomerCardByNumber
  [HttpPost]
  [Route("DeleteCustomerCardByNumber")]
  public async Task<IActionResult> DeleteCustomerCardByNumber([FromBody] CustomerCardDeleteDto customerCardDeleteDto)
  {
    bool isDeleted = await _customerCardService.DeleteCustomerCardByNumber(customerCardDeleteDto);
    return Ok(isDeleted);
  }

  // POST api/CustomerCard/DeleteRewardCard
  [HttpPost]
  [Route("DeleteRewardCard")]
  public async Task<IActionResult> DeleteRewardCard(string customerCardNumber, string rewardCardNumber)
  {
    bool isDeleted = await _customerCardService.DeleteRewardCard(customerCardNumber, rewardCardNumber);
    return Ok(isDeleted);
  }

  // GET api/CustomerCard/GetRewardCardsByCardNumber
  [HttpGet]
  [Route("GetRewardCardsByCardNumber")]
  public async Task<IActionResult> GetRewardCardsByCardNumber(string cardNumber)
  {
    IEnumerable<RewardCard> rewardCards = await _customerCardService.GetRewardCardsByCardNumber(cardNumber);
    return Ok(rewardCards);
  }

  // GET api/CustomerCard/IsCustomerCardExist
  [HttpGet]
  [Route("IsCustomerCardExist")]
  public async Task<IActionResult> IsCustomerCardExist(string custCardNumber)
  {
    bool rewardCard = await _customerCardService.IsCustomerCardExist(custCardNumber);
    return Ok(rewardCard);
  }

  // GET api/CustomerCard/IsRewardCardExistByCustomerCardAndRewardCardNumber
  [HttpGet]
  [Route("IsRewardCardExistByCustomerCardAndRewardCardNumber")]
  public async Task<IActionResult> IsRewardCardExistByCustomerCardAndRewardCardNumber(string custCardNumber, string rewardCardNumber)
  {
    bool rewardCard = await _customerCardService.IsRewardCardExistByCustomerCardAndRewardCardNumber(custCardNumber, rewardCardNumber);
    return Ok(rewardCard);
  }

  // GET api/CustomerCard/IsRewardCardExistByCustomerCardAndPhone
  [HttpGet]
  [Route("IsRewardCardExistByCustomerCardAndPhone")]
  public async Task<IActionResult> IsRewardCardExistByCustomerCardAndPhone(string custCardNumber, string phoneNumber)
  {
    bool rewardCard = await _customerCardService.IsRewardCardExistByCustomerCardAndPhone(custCardNumber, phoneNumber);
    return Ok(rewardCard);
  }

  // Post api/CustomerCard/AddRewardCard
  [HttpPost]
  [Route("AddRewardCard")]
  public async Task<IActionResult> AddRewardCard([FromBody] RewardCard rewardCard)
  {
    bool isAdded = await _customerCardService.AddRewardCard(rewardCard);
    return Ok(isAdded);
  }

  // POST api/CustomerCard/UpdateRewardCard
  [HttpPost]
  [Route("UpdateRewardCard")]
  public async Task<IActionResult> UpdateRewardCard([FromBody] RewardCard rewardCard)
  {
    bool isUpdated = await _customerCardService.UpdateRewardCard(rewardCard);
    return Ok(isUpdated);
  }

  // GET api/CustomerCard/GetCustomerCardByNumber
  [HttpGet]
  [Route("GetCustomerCardByNumber")]
  public async Task<IActionResult> GetCustomerCardByNumber(string cardNumber)
  {
    CustomerCard customerCard = await _customerCardService.GetCustomerCardByNumber(cardNumber);
    return Ok(customerCard);
  }

  // GET api/CustomerCard/IsArCustomer
  [HttpGet]
  [Route("IsArCustomer")]
  public async Task<IActionResult> IsArCustomer(string code)
  {
    bool isArCsutomer = await _customerCardService.IsArCustomer(code);
    return Ok(isArCsutomer);
  }

  // GET api/CustomerCard/GetNextSequenceNumber
  [HttpGet]
  [Route("GetNextSequenceNumber")]
  public async Task<IActionResult> GetNextSequenceNumber(string customerCode)
  {
    int nextSeqNumber = await _customerCardService.GetNextSequenceNumber(customerCode);
    return Ok(nextSeqNumber);
  }
}
