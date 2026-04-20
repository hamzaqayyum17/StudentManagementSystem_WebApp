using Microsoft.AspNetCore.Mvc;

namespace StudentManagementSystem.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsAdmin()
        {
            return HttpContext.Session.GetString("role") == "admin";
        }

        protected bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("sid") != null;
        }
    }
}
