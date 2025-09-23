using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace Karandash.Authentication.API.Controllers
{
    [Route("api/[controller]"), ApiController, Obsolete]
    public class
        LanguageController : ControllerBase /* Bu controller istifadə məqsədi daşımır, test məqsədilə yaradılmışdır. Lakin məqsədli şəkildə saxlanılmışdır. */
    {
        [HttpGet("[action]")]
        public IActionResult GetCurrentLanguage()
        {
            return Ok(new
            {
                Culture = CultureInfo.CurrentCulture.Name,
                UICulture = CultureInfo.CurrentUICulture.Name
            });
        }
    }
}