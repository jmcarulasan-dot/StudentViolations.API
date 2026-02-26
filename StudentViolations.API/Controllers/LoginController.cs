using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Add logging
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Threading.Tasks;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : Controller
    {
        ILoginRepository loginRepository;
        

        public LoginController(ILoginRepository login)
        {
            loginRepository = login;   
        }

        [HttpPost]
        public async Task<IActionResult> LoginStudent(@object login)
        {
            @object model = new @object();
            var response = loginRepository.GetLogin(login.Username, login.Password);
            return BadRequest();
           
        }
    }
}