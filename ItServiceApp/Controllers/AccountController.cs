using AutoMapper;
using ItServiceApp.Extensions;
using ItServiceApp.Models;
using ItServiceApp.Models.Identity;
using ItServiceApp.Services;
using ItServiceApp.ViewModel;
using ItServiceApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ItServiceApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        //servislerin kullanımı için fieldlar oluşturuldu
        private readonly UserManager<ApplicationUser> _userManager; //sadece okunabilir (readonly) ve tek bir atama yapılabilir.Sonradan değişiklik yapılamaz. Tanımlandığı anda ya da constructorında değeri verilebilir.
        private readonly SignInManager<ApplicationUser> _signInManager;//giriş yapılacaksa SignInManager(hazır geliyor identityden)
        private readonly RoleManager<ApplictionRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;
        //fieldlar için referans atama yapıldı.
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplictionRole> roleManager, IEmailSender emailSender, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _mapper = mapper;
            CheckRoles(); //sistemde rol yoksa rolleri ekler           
        }

        private void CheckRoles()
        {
            // verilere rol ataması yapılır.
            foreach (var roleName in RoleNames.Roles)
            {
                if (!_roleManager.RoleExistsAsync(roleName).Result)
                {
                    var result = _roleManager.CreateAsync(new ApplictionRole()
                    {
                        Name = roleName
                    }).Result;
                }
            }
        }

        [AllowAnonymous]
        [HttpGet] //kullanıcıya bilgi sunmak
        public IActionResult Register()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost] //veri tabanına veri sunmak
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            //post kısmında öncelikle kontrolleri yapmalısın kayıttan önce
            if (!ModelState.IsValid)
            {
                model.Password = string.Empty;
                model.ConfirmPassword = string.Empty;
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null) //girilen kullanıcı adı veri tabanında kayıtlı ise
            {
                ModelState.AddModelError(nameof(model.UserName), "Bu kullanıcı adı daha önce sisteme kaydedilmiştir");
                return View(model);
            }
            user = await _userManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Bu email daha önce sisteme kaydedilmiştir");
                return View(model);
            }

            //kullanıcı daha önce sisteme girmediyse
            user = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname
            };

            var result = await _userManager.CreateAsync(user, model.Password); //user manager girilenden farklı bir şifre oluşturuyor sizin için güvenlik nedeniyle (şifreler güvende olsun kimse bilmesin yazılımcı da)

            if (result.Succeeded)
            {
                //TODO:kullanıcıya rol atama
                var count = _userManager.Users.Count();
                result = await _userManager.AddToRoleAsync(user, count == 1 ? RoleNames.Admin : RoleNames.Passive);

                //if (count==1)//admin
                //{
                //    _userManager.AddToRoleAsync(user, RoleNames.Admin);
                //}
                //else
                //{
                //    result = await _userManager.AddToRoleAsync(user,RoleNames.User);
                //}

                //TODO:kullanıcıya mail dogrulaması atma

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);//kullanıcı için token oluşturma.(aktivasyon kodu)
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)); //query stringde hata çıkmasın diye
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                var emailMessage = new EmailMessage()
                {
                    Contacts = new string[] { user.Email },
                    Body = $"Please confirm your account by <a href ='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here </a>", //ilgili like tıklandığında (alttaki) confirm email metoduna yönlendirir.
                    Subject = "Confirm your email"
                };

                await _emailSender.SendAsync(emailMessage);
                //TODO:giriş sayfasına yönlendirme
            }
            else
            {
                ModelState.AddModelError(string.Empty, ModelState.ToFullErrorString());
                return View(model);
            }

            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        //confirm yapılmazsa pasif olarak kalır kullanıcı.
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("index", "Home");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);//
            ViewBag.StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email ";
            if (result.Succeeded && _userManager.IsInRoleAsync(user, RoleNames.Passive).Result)
            {
                await _userManager.RemoveFromRoleAsync(user, RoleNames.Passive);
                await _userManager.AddToRoleAsync(user, RoleNames.User);

            }
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, true); //veri tabanındaki bilgiler ve modeldeki bilgiler örtüşüyor mu kontrolü (SignInManager sayesinde)

            if (result.Succeeded)
            {
                await _emailSender.SendAsync(new EmailMessage()
                {
                    //gönderen bilgileri ayarları appsetting.json klasöründe
                    Contacts = new string[] { "ftmunlu7@gmail.com" },
                    Body = $"{HttpContext.User.Identity.Name} Sisteme giriş yaptı",
                    Subject = $"Merhaba Fatma mail geldi mi"
                });
                return RedirectToAction("Index", "Home"); //action gerçekleşince Home sayfasına yönlendir. 
            }
            else
            {
                ModelState.AddModelError(string.Empty, "kullanıcı adı veya şifre hatalı");
                return View(model);
            }
        }

        [Authorize] //sisteme girmemiş birine logout yetkisi veremezsin bu yetki için giriş yapmış olmalı
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());

            //var model = new UserProfileViewModel()
            //{
            //    Email = user.Email,
            //    Name = user.Name,
            //    Surname = user.Surname,
            //};

            //automapper ile daha kısa yolu:
            var model = _mapper.Map<UserProfileViewModel>(user);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            //  var userModel = _mapper.Map<ApplicationUser>(model);üsttekinin tersine dönüştürme işlemi.

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            //kullanıcı bilgilerini güncelleme
            user.Name = model.Name;
            user.Surname = model.Surname;

            if (user.Email != model.Email)//mail adresini değiştirmiş ise yeni emaile aktivasyon linki gönder.
            {
                await _userManager.RemoveFromRoleAsync(user, RoleNames.User);
                await _userManager.AddToRoleAsync(user, RoleNames.Passive);

                user.Email = model.Email;
                user.EmailConfirmed = false;

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)); //query stringde hata çıkmasın diye
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                var emailMessage = new EmailMessage()
                {
                    Contacts = new string[] { user.Email },
                    Body = $"Please confirm your account by <a href ='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here </a>",
                    Subject = "Confirm your email"
                };

                await _emailSender.SendAsync(emailMessage);
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, ModelState.ToFullErrorString());
            }

            return View(model);
        }
        public IActionResult PasswordUpdate()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> PasswordUpdate(PasswordUpdateView model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                //email gönder.
                TempData["Message"] = "Şifre değiştirme işleminiz başarılı";
                return View();
            }
            else
            {
                var message = string.Join("<br", result.Errors.Select(x => x.Description));
                TempData["Message"] = message;
                return View();
            }


            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task <IActionResult> ResetPassword()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user==null)
            {
                ViewBag.Message = "Girdiğiniz email sistemimizde bulunamadı";
            }
            else
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);//kullanıcı için token oluşturma.(aktivasyon kodu)
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code)); //query stringde hata çıkmasın diye
                var callbackUrl = Url.Action("ConfirmResetPassword", "Account", new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                var emailMessage = new EmailMessage()
                {
                    Contacts = new string[] { user.Email },
                    Body = $"Please confirm your password by <a href ='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here </a>", //ilgili like tıklandığında (alttaki) confirm email metoduna yönlendirir.
                    Subject = "ResetPassword"
                };

                await _emailSender.SendAsync(emailMessage);
                ViewBag.Message = "Mailinize şifre güncelleme yönergeniz gönderilmiştir.";
            }
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        //confirm yapılmazsa pasif olarak kalır kullanıcı.
        public async Task<IActionResult> ConfirmResetPassword(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId)|| string.IsNullOrEmpty(code))
            {
                return BadRequest("Hatalı İstek");
            }
            ViewBag.Code = code;
            ViewBag.UserId = userId;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ConfirmResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user==null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı");
                return View();
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ResetPasswordAsync(user, code, model.NewPassword);

            if (result.Succeeded)
            {
                //email gönder
                TempData["Message"] = "Şifre değişikliğiniz gerçekleştirilmiştir";
                return View();
            }
            else
            {
                var message = string.Join("<br",result.Errors.Select(x=>x.Description));
                TempData["Message"] = message;
                return View();
            }
        }

    }
}
