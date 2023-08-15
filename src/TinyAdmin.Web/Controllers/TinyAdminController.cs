using Microsoft.AspNetCore.Mvc;

namespace TinyAdmin.Web.Controllers;

public class TinyAdminController : Controller
{

    [HttpGet("")] 
    public async Task<IActionResult> GetMainPage()
    {
        return View("Views/MainPage.cshtml");
    }
    
}