using InfonetCSCR.Reporting.Models;
using InfonetCSCR.Reporting.Service;
using System.Web.Mvc;
using System.Web.Security;

namespace InfonetCSCR.Reporting.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserService userService;
        public AuthController()
        {
            userService = new UserService();
        }

        // GET: Auth
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User loginUser)
        {
            if (ModelState.IsValid)
            {
                bool isValidUser = userService.VerifyUser(loginUser);

                if (isValidUser)
                {
                    bool hasAccessRight = userService.HasAccessRight("U_RUNREPORT", loginUser);
                    if (hasAccessRight)
                    {
                        FormsAuthentication.SetAuthCookie(loginUser.Code, false);
                        Session["CompanyName"] = userService.GetCompnayName();
                        return RedirectToAction("Index", "Category");
                    }
                    else
                    {
                        ModelState.AddModelError("UserValidation", "You have entered wrong username/password or you do not have rights to access the application.");
                    }
                }
                else
                {
                    ModelState.AddModelError("UserValidation", "You have entered wrong username/password or you do not have rights to access the application.");
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}