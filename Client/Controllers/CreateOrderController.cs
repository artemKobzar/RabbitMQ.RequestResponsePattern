using Client.Model;
using Client.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Web;

namespace Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateOrderController : ControllerBase
    {
        private readonly ILogger<Order> _logger;
        private readonly IRequestProducer _requestProducer;
        private readonly IRequestSender _requestSender;
        public static readonly List<Order> Orders = new();

        public CreateOrderController (ILogger<Order> logger, IRequestProducer requestProducer, IRequestSender requestSender)
        {
            _logger = logger;
            _requestProducer = requestProducer;
            _requestSender = requestSender;
        }
        [HttpPost(Name = "CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            var jsonOrder = JsonSerializer.Serialize(order);
            _requestSender.SendRequest(jsonOrder);
            var response = await _requestSender.GetResponse();
            if (response != null)
            {
                return Ok(response);
            }
            else
                return BadRequest();
        }
        //[HttpPost(Name = "CreateAnotherOrder")]
        //public async Task<IActionResult> CreateAnotherOrder([FromBody] Order order)
        //{
        //    var jsonOrder = JsonSerializer.Serialize(order);

        //    Orders.Add(order);
        //    var response = await _requestProducer.SendRequest(jsonOrder);
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }
        //    return Content("response");

        //}
        //[HttpGet(Name = "GetOrders")]
        //public async Task<IEnumerable<Order>> GetOrders()
        //{

        //    await _requestProducer.SendGetRequest();

        //    return Orders;
        //}
    }
}
