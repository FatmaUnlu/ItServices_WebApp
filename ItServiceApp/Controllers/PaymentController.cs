using AutoMapper;
using ItServiceApp.Data;
using ItServiceApp.Extensions;
using ItServiceApp.Models;
using ItServiceApp.Models.Identity;
using ItServiceApp.Models.Payment;
using ItServiceApp.Services;
using ItServiceApp.ViewModels;
using Iyzipay.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ItServiceApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly MyContext _dbContext;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public PaymentController(IPaymentService paymentService, MyContext dbContext, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _paymentService = paymentService;
            _dbContext = dbContext;
            _mapper = mapper;
            _userManager = userManager;

            var cultureInfo = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult CheckInstallment(string binNumber, decimal price)
        {
            if (binNumber.Length < 6 || binNumber.Length > 16) return BadRequest(new
            {
                Message = "Bad Req."
            });

            var result = _paymentService.CheckInstallments(binNumber, price);
            return Ok(result); //istek geçerliyse ajaxa geri döner.
        }

        [Authorize]
        [HttpPost]
        public IActionResult Index(PaymentViewModel model)
        {
            var paymentModel = new PaymentModel()
            {
                Installment = model.Installment,
                AddressModel = new AddressModel(),
                BasketModel = new List<BasketModel>(),
                CustomerModel = new CustomerModel(),
                CardModel = model.CardModel,
                Price = 1000,
                UserId = HttpContext.GetUserId(),
                Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            };

            var installmentInfo = _paymentService.CheckInstallments(paymentModel.CardModel.CardNumber.Substring(0, 6), paymentModel.Price);

            var installmentNumber = installmentInfo.InstallmentPrices.FirstOrDefault(x => x.InstallmentNumber == model.Installment);

            paymentModel.PaidPrice = decimal.Parse(installmentNumber != null ? installmentNumber.TotalPrice.Replace('.', ',') : installmentInfo.InstallmentPrices[0].TotalPrice.Replace('.', ','));

            //if (installmetNumber!=null)
            //{
            //    paymentModel.PaidPrice = decimal.Parse(installmentNumber.TotalPrice);
            //}
            //else
            //{
            //    paymentModel.PaidPrice = decimal.Parse(installmenntInfo.InstallmentPrices[0].TotalPrice);
            //}

            //legacy code

            var result = _paymentService.Pay(paymentModel);
            return View();
        }

        [Authorize]
        public IActionResult Purchase(Guid id)
        {
            var data = _dbContext.SubscriptionTypes.Find(id);

            if (data == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = _mapper.Map<SubscriptionTypeViewModel>(data);
            ViewBag.Subs = model;

            var addresses = _dbContext.Addresses.Where(x => x.UserId == HttpContext.GetUserId())
                .ToList() //önce listeye çevirip ram a atılmalı
                .Select(x=>_mapper.Map<AddressViewModel>(x))
                .ToList();

            ViewBag.Addresses = addresses;

            var model2 = new PaymentViewModel()
            {
                BasketModel = new BasketModel()
                {
                    Category1 = data.Name,
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Id = data.Id.ToString(),
                    Name = data.Name,
                    Price = data.Price.ToString(new CultureInfo("en-us")) //
                }
            };
            return View(model2);
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(PaymentViewModel model)
        {
            var type = await _dbContext.SubscriptionTypes.FindAsync(Guid.Parse(model.BasketModel.Id));
            
            var basketModel = new BasketModel()
            {
                Category1 = type.Name,
                ItemType = BasketItemType.VIRTUAL.ToString(),
                Id = type.Id.ToString(),
                Name = type.Name,
                Price = type.Price.ToString(new CultureInfo("en-us")) //
            };

            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());

            var address= _dbContext.Addresses
                .Include(x=>x.State.City)
                .First(x=>x.Id == Guid.Parse(model.AddressModel.Id));

            var addressModel = new AddressModel()
            {
                City = address.State.City.Name,
                ContactName = ",",
                Country = "Türkiye",
                Description = address.Line,
                ZipCode = address.PasCode
            };

            var customerModel = new CustomerModel()
            {
                City = address.State.City.Name,
                Country = "Türkiye",
                Email = user.Email,
                GsmNumber = user.PhoneNumber,
                Id = user.Id,
                IdentityNumber = user.Id,
                Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                Name=user.Name,
                Surname = user.Surname,
                ZipCode = addressModel.ZipCode,
                LastLoginDate=$"{DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                RegistirationDate = $"{user.CreatedDate:yyyy-MM-dd HH:mm:ss}",
                RegistrationAddress = address.Line
            };

            var paymentModel = new PaymentModel()
            {
                Installment = model.Installment,
                AddressModel = addressModel,
                BasketModel = new List<BasketModel>() { basketModel },
                CustomerModel = customerModel,
                CardModel = model.CardModel,
                Price = model.Amount,
                UserId = HttpContext.GetUserId(),
                Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            };

            var installmentInfo = _paymentService.CheckInstallments(paymentModel.CardModel.CardNumber.Substring(0, 6), paymentModel.Price);

            var installmentNumber = installmentInfo.InstallmentPrices.FirstOrDefault(x => x.InstallmentNumber == model.Installment);

            paymentModel.PaidPrice = decimal.Parse(installmentNumber != null ? installmentNumber.TotalPrice : installmentInfo.InstallmentPrices[0].TotalPrice);

            //legacy code

            var result = _paymentService.Pay(paymentModel);

            var addresses = _dbContext.Addresses.Where(x => x.UserId == HttpContext.GetUserId())
                .ToList() //önce listeye çevirip ram a atılmalı
                .Select(x => _mapper.Map<AddressViewModel>(x))
                .ToList();

            ViewBag.Addresses = addresses;
            var subs = _mapper.Map<SubscriptionTypeViewModel>(type);
            ViewBag.Subs = subs;

            return View();
        }

    }
}
