using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;

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

        [HttpGet]

        public async Task<ActionResult> LoginStudent(@object login)
        {
            @object model = new @object();
            var response = loginRepository.GetLogin(login.Username, login.Password);
            return BadRequest();
        }
    }
}
