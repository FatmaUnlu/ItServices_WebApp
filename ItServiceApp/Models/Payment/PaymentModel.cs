using System.Collections.Generic;

namespace ItServiceApp.Models.Payment
{
    public class PaymentModel
    {
        public string PaymentId { get; set; }
        public decimal Price { get; set; }
        public decimal PaidPrice { get; set; }
        public int Installment { get; set; }
        public CardModel CardModel { get; set; }
        public List<BasketModel> BasketModel { get; set; }
        public CustomerModel CustomerModel { get; set; }
        public AddressModel AddressModel { get; set; }
        public string Ip { get; set; }
        public string UserId { get; set; }


    }
}
