using Hms.Services.Abstraction;
using HMS.Core.Contracts;
using HMS.Core.Entities.BookingModule;
using HMS.Shared.Respones;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HMS.Infrastructure.ExternalService
{
    public class PaymentService(IUnitOfWork _unitOfWork, IConfiguration _configuration, HttpClient _httpClient) : IPaymentService
    {

        public async Task<GenericResponse<string>> CreatePaymentUrlAsync(Guid bookingId)
        {
            var genericResponse = new GenericResponse<string>();

            var booking = await _unitOfWork.GetRepository<Booking, Guid>().GetByIdAsync(bookingId, null, [b => b.HotelUser]);

            if (booking is null)
            {
                genericResponse.StatusCode = StatusCodes.Status404NotFound;
                genericResponse.Message = "Booking is not found!";
                return genericResponse;
            }

            //Get Auth Token.
            var authToken = await AuthenticateAsync();

            //Create Order[Intent]
            var orderId = await CreateOrderAsync(authToken, booking.TotalAmount, booking.Currency);

            if (orderId is null)
            {
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "Failed to create intent on PayMob!";
                return genericResponse;
            }

            booking.PayMobOrderId = orderId;

            //Create PaymentKey
            var paymentKey = await CreatePaymentKeyAsync(authToken, orderId, booking.Currency, booking.TotalAmount,
                booking.HotelUser.Email!, booking.HotelUser.PhoneNumber!, booking.HotelUser.FullName);

            if (paymentKey is null)
            {
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "Failed to create intent on PayMob!";
                return genericResponse;
            }

            booking.PayMobPaymentKey = paymentKey;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.Status = BookingStatus.Paid;
            booking.PaidDate = DateTime.UtcNow;


            _unitOfWork.GetRepository<Booking, Guid>().Update(booking);

            var result = await _unitOfWork.SaveChangesAsync() > 0;

            if (result)
            {
                genericResponse.StatusCode = StatusCodes.Status200OK;
                genericResponse.Message = "Payment URL created successfully!";
                genericResponse.Data = $"{_configuration["PayMob:BaseUrl"]}/acceptance/iframes/{_configuration["PayMob:IFrameId"]}?payment_token={paymentKey}";

            }
            else
            {
                genericResponse.StatusCode = StatusCodes.Status500InternalServerError;
                genericResponse.Message = "Failed to create payment URL!";
            }
            return genericResponse;
        }

        #region Private methods

        private async Task<string> AuthenticateAsync()
        {
            var response = await _httpClient.PostAsJsonAsync($"{_configuration["PayMob:BaseUrl"]}/auth/tokens",
                 new { api_key = _configuration["PayMob:ApiKey"] });

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            return json.GetProperty("token").GetString()!;
        }

        private async Task<string> CreateOrderAsync(string authToken, decimal amount, string currency)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_configuration["PayMob:BaseUrl"]}/ecommerce/orders",
                  new
                  {
                      auth_token = authToken,
                      delivery_needed = false,
                      amount_cents = (int)(amount * 100),
                      currency,
                      items = Array.Empty<object>()
                  }
            );

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("id").GetInt32().ToString();
        }

        private async Task<string> CreatePaymentKeyAsync(string authToken, string orderId, string currency,
            decimal amount, string email, string phone, string fullName
            )
        {
            var response = await _httpClient.PostAsJsonAsync($"{_configuration["PayMob:BaseUrl"]}/acceptance/payment_keys",
                new
                {
                    auth_token = authToken,
                    amount_cents = (int)(amount * 100),
                    currency,
                    order_id = orderId,
                    expiration = 3600,
                    integration_id = int.Parse(_configuration["PayMob:IntegrationId"]!),
                    billing_data = new
                    {
                        email,
                        first_name = fullName.Split(' ')[0],
                        last_name = fullName.Split(' ').Length > 1 ? fullName.Split(' ')[1] : "N/A",
                        phone_number = phone,
                        apartment = "NA",
                        floor = "NA",
                        street = "NA",
                        building = "NA",
                        city = "Cairo",
                        country = "EG",
                        state = "Cairo"
                    }
                });
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("token").GetString()!;
        }

        #endregion    


    }
}
