using Microsoft.AspNetCore.Mvc;
using SharedClass;
using SharedClass.Model;
using System;
using System.Threading.Tasks;

namespace AWS.Register.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        [HttpPost]
        public async Task PostAsync([FromBody] Order objOrder)
        {
            objOrder.Id = Guid.NewGuid().ToString();
            objOrder.CreationDate = DateTime.UtcNow;

            await objOrder.SaveAsync();

            Console.WriteLine($"Pedido salvo com sucesso: id {objOrder.Id}");
        }
    }
}