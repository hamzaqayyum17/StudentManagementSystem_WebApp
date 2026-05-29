using Microsoft.AspNetCore.Mvc;

namespace StudentManagementSystem.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsAdmin()
        {
            return HttpContext.Session.GetString("role") == "admin";
        }

        protected bool IsTeacher()
        {
            return HttpContext.Session.GetString("role") == "teacher";
        }

        protected bool IsStudent()
        {
            return HttpContext.Session.GetString("role") == "student";
        }

        protected bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("sid") != null;
        }
    }
}