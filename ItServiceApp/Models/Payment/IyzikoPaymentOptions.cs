using Iyzipay;

namespace ItServiceApp.Models
{
    public class IyzikoPaymentOptions :Options
    {
        public const string Key = "IyzikoOptions"; 
        public string  ThreedsCallbackUrl { get; set; }
    }
}
