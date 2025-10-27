using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static MyLoginApp.Data.UserModel;
using System.Windows;


public class AccountController : Controller
{
    private readonly IUserRepository _repo;
    public AccountController(IUserRepository repo) => _repo = repo;

    // ----- SIGNUP -----
    [HttpGet]
    public IActionResult Signup()
    {
        return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "signup.html"), "text/html");
    }

    [HttpPost]
    public async Task<IActionResult> Signup([FromForm] SignupModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Fill required fields.";
            return Redirect("/signup.html");
        }

        var ok = await _repo.RegisterUserAsync(model.FullName, model.Username, model.Password, model.Email, model.PhoneNumber);
        if (!ok)
        {
            TempData["Error"] = "Username already exists.";
            return Redirect("/signup.html");
        }
        HttpContext.Session.SetString("Username", model.Username);
        return RedirectToAction("Index", "Home");
    }

    // ----- LOGIN -----
    [HttpGet]
    public IActionResult Login()
    {
        return View("Login");
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromForm] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Fill required fields.";
            return Redirect("/login.html");
        }

        var user = await _repo.GetUserByUsernameAsync(model.username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.password, user.Password))
        {
            TempData["Error"] = "Invalid username or password.";
            return Redirect("/signup.html");
        }
        TempData["Success"] = "Login successful!";
        HttpContext.Session.SetString("Username", model.username);
        return RedirectToAction("Index", "Home");
    }

    // ----- LOGOUT -----
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("Username");
        return Redirect("/login.html");
    }
}


