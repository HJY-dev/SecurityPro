using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Order.ViewModels;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Polly.Order.Controllers
{
    /// <summary>
    /// Polly
    /// </summary>
    /// <remarks>
    /// Build Resilient Microservices (Web API) using Polly in ASP.NET Core
    /// https://procodeguide.com/programming/polly-in-aspnet-core/
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private HttpClient _httpClient;
        private string apiurl = @"http://localhost:5001/";

        private OrderDetails _orderDetails = null;

        private readonly RetryPolicy _retryPolicy;
        private static TimeoutPolicy _timeoutPolicy;
        private readonly FallbackPolicy<string> _fallbackPolicy;
        private static CircuitBreakerPolicy _circuitBreakerPolicy;
        private static BulkheadPolicy _bulkheadPolicy;

        public OrderController(ILogger<OrderController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            if (_orderDetails == null)
            {
                _orderDetails = new OrderDetails
                {
                    Id = 7261,
                    SetupDate = DateTime.Now.AddDays(-10),
                    Items = new List<Item>()
                };
                _orderDetails.Items.Add(new Item
                {
                    Id = 6514,
                    Name = ".NET Core Book"
                });
            }

            _retryPolicy = Policy
                .Handle<Exception>()
                .Retry(2);

            _timeoutPolicy = Policy.Timeout(20, TimeoutStrategy.Pessimistic);

            _fallbackPolicy = Policy<string>
                                .Handle<Exception>()
                                .Fallback("Customer Name Not Available - Please retry later");

            if (_circuitBreakerPolicy == null)
            {
                _circuitBreakerPolicy = Policy.Handle<Exception>()
                                              .CircuitBreaker(2, TimeSpan.FromMinutes(1));
            }

            _bulkheadPolicy = Policy.Bulkhead(3, 6);
        }

        [HttpGet]
        [Route("GetOrderByCustomer/{customerCode}")]
        public OrderDetails GetOrderByCustomer(int customerCode)
        {
            _httpClient = _httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(apiurl);
            var uri = "/api/Customer/GetCustomerName/" + customerCode;
            var result = _httpClient.GetStringAsync(uri).Result;

            _orderDetails.CustomerName = result;

            return _orderDetails;
        }

        [HttpGet]
        [Route("GetOrderByCustomerWithRetry/{customerCode}")]
        public OrderDetails GetOrderByCustomerWithRetry(int customerCode)
        {
            _httpClient = _httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(apiurl);
            var uri = "/api/Customer/GetCustomerNameWithTempFailure/" + customerCode;
            var result = _retryPolicy.Execute(() => _httpClient.GetStringAsync(uri).Result);

            _orderDetails.CustomerName = result;

            return _orderDetails;
        }

        [HttpGet]
        [Route("GetOrderByCustomerWithTimeout/{customerCode}")]
        public OrderDetails GetOrderByCustomerWithTimeout(int customerCode)
        {
            try
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.BaseAddress = new Uri(apiurl);
                var uri = "/api/Customer/GetCustomerNameWithDelay/" + customerCode;
                var result = _timeoutPolicy.Execute(() => _httpClient.GetStringAsync(uri).Result);

                _orderDetails.CustomerName = result;

                return _orderDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excpetion Occurred");
                _orderDetails.CustomerName = "Customer Name Not Available as of Now";
                return _orderDetails;
            }
        }

        [HttpGet]
        [Route("GetOrderByCustomerWithFallback/{customerCode}")]
        public OrderDetails GetOrderByCustomerWithFallback(int customerCode)
        {
            try
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.BaseAddress = new Uri(apiurl);
                var uri = "/api/Customer/GetCustomerNameWithPermFailure/" + customerCode;
                var result = _fallbackPolicy.Execute(() => _httpClient.GetStringAsync(uri).Result);

                _orderDetails.CustomerName = result;
                return _orderDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excpetion Occurred");
                _orderDetails.CustomerName = "Customer Name Not Available as of Now";
                return _orderDetails;
            }
        }

        [HttpGet]
        [Route("GetOrderByCustomerWithCircuitBreaker/{customerCode}")]
        public OrderDetails GetOrderByCustomerWithCircuitBreaker(int customerCode)
        {
            try
            {
                _httpClient = _httpClientFactory.CreateClient();
                _httpClient.BaseAddress = new Uri(apiurl);
                var uri = "/api/Customer/GetCustomerNameWithPermFailure/" + customerCode;
                var result = _circuitBreakerPolicy.Execute(() => _httpClient.GetStringAsync(uri).Result);

                _orderDetails.CustomerName = result;
                return _orderDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excpetion Occurred");
                _orderDetails.CustomerName = "Customer Name Not Available as of Now";
                return _orderDetails;
            }
        }

        [HttpGet]
        [Route("GetOrderByCustomerWithBulkHead/{customerCode}")]
        public OrderDetails GetOrderByCustomerWithBulkHead(int customerCode)
        {
            _httpClient = _httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(apiurl);
            var uri = "/api/Customer/GetCustomerName/" + customerCode;
            var result = _bulkheadPolicy.Execute(() => _httpClient.GetStringAsync(uri).Result);

            _orderDetails.CustomerName = result;

            return _orderDetails;
        }
    }
}
