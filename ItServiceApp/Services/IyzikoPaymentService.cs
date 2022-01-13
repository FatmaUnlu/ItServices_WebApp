﻿using AutoMapper;
using ItServiceApp.Models;
using ItServiceApp.Models.Payment;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Configuration;
using MUsefulMethods;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ItServiceApp.Services
{
    public class IyzikoPaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IyzikoPaymentOptions _options;
        private readonly IMapper _mapper;

        public IyzikoPaymentService(IConfiguration configuration, IMapper mapper)
        {
            _configuration = configuration;
            _mapper = mapper;

            var section = configuration.GetSection(IyzikoPaymentOptions.Key);
           
            _options = new IyzikoPaymentOptions()
            {
                ApiKey = section["ApiKey"],
                SecretKey = section["SecretKey"],
                BaseUrl = section["BaseUrl"],
                ThreedsCallbackUrl = section["ThreedsCallbackUrl"],
            };
        }

        private string GenerateConversationId()
        {
            return StringHelpers.GenerateUniqueCode();
        }
        public InstallmentModel CheckInstallments(string binNumber, decimal price)
        {
            var conversationId = GenerateConversationId();
            var request = new RetrieveInstallmentInfoRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = conversationId,
                BinNumber = binNumber,
                Price = price.ToString(new CultureInfo("en-US"))
            };
            var result = InstallmentInfo.Retrieve(request, _options);
            if (result.Status=="failure")
            {
                throw new Exception(result.ErrorMessage);
            }
            if (result.ConversationId != conversationId)
            {
                throw new Exception("Hatalı istek oluturuldu");

            }
            var resultModel = _mapper.Map<InstallmentModel>(result.InstallmentDetails[0]);
            Console.WriteLine();
            return resultModel;
        }

       

        public PaymentResponseModel Pay(PaymentModel model)
        {

            return null;
        }
    }
}