using Microsoft.AspNetCore.Mvc;

namespace OrderManagementApi.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult GetStatus()
    {
        var resposta = new
        {
            aplicacao = "OrderManagementApi",
            status = "online",
            dataUtc = DateTime.UtcNow
        };

        return Ok(resposta);
    }
}
