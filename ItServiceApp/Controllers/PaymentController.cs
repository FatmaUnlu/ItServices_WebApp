using ItServiceApp.Extensions;
using ItServiceApp.Models;
using ItServiceApp.Models.Payment;
using ItServiceApp.Services;
using ItServiceApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ItServiceApp.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

         [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult CheckInstallment(string binNumber)
        {
            if (binNumber.Length != 6) return BadRequest(new
            {
                Message = "Bad Req."
            });

            var result = _paymentService.CheckInstallments(binNumber, 1000);
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
                UserId=HttpContext.GetUserId(),
                Ip=Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            };

            var installmentInfo = _paymentService.CheckInstallments(paymentModel.CardModel.CardNumber.Substring(0, 6), paymentModel.Price);

          var installmentNumber=installmentInfo.InstallmentPrices.FirstOrDefault(x => x.InstallmentNumber == model.Installment);

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
            return View(model);
        }

    }
}
