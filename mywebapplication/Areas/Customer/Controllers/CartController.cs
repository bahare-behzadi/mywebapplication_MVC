using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using mywebapplication.DataAccess.Repository.IRepository;
using mywebapplication.Models;
using mywebapplication.Models.Models;
using mywebapplication.Models.ViewModels;
using mywebapplication.Utility;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace MyWebApplication.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM
            {
                ItemsShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeproperties: "Product"
                ),
                OrderHeader = new()
            };
            foreach (var cart in ShoppingCartVM.ItemsShoppingCartList)
            {
                cart.Price = GetCartPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM
            {
                ItemsShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeproperties: "Product"
                ),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            //ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.streetAdress = ShoppingCartVM.OrderHeader.ApplicationUser.streetAdress;
            ShoppingCartVM.OrderHeader.city = ShoppingCartVM.OrderHeader.ApplicationUser.city;
            ShoppingCartVM.OrderHeader.state = ShoppingCartVM.OrderHeader.ApplicationUser.state;
            ShoppingCartVM.OrderHeader.postalCode = ShoppingCartVM.OrderHeader.ApplicationUser.postalCode;
            foreach (var cart in ShoppingCartVM.ItemsShoppingCartList)
            {
                cart.Price = GetCartPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ItemsShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeproperties: "Product");
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            foreach (var cart in ShoppingCartVM.ItemsShoppingCartList)
            {
                cart.Price = GetCartPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //user is an ordinary customer
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentstatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.PaymentstatusPending;
            }
            else
            {
                //user is a company
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentstatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.statusApproved;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ItemsShoppingCartList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    Count = cart.Count,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.OrderHeaderId,
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            { //order confirmation
                var domain = "https://localhost:7103/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.OrderHeaderId}",
                    CancelUrl = domain + "Customer/Cart/Index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                     Mode = "payment",
                };
                foreach (var option in ShoppingCartVM.ItemsShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(option.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = option.Product.Title
                            }
                        }, 
                        Quantity = option.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new Stripe.Checkout.SessionService();
                Session session=service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);            }

            return RedirectToAction(nameof(OrderConfirmation), new { orderId = ShoppingCartVM.OrderHeader.OrderHeaderId });
        }

        public IActionResult OrderConfirmation(int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u=>u.OrderHeaderId == orderId, includeproperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentstatusDelayedPayment)
            {
                //A customer's order
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if(session.PaymentStatus.ToLower()=="paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.statusApproved, SD.PaymentstatusApproved);
                    _unitOfWork.Save();
                }
                List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==orderHeader.ApplicationUserId).ToList();
                _unitOfWork.ShoppingCart.DeleteRange(shoppingCarts);
                _unitOfWork.Save();
            }
            return View(orderId);
        }
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ProductId == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ProductId == cartId, tracked:true);
            if (cartFromDb.Count <= 1)
            {
                //remove that from cart
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
                    .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
                _unitOfWork.ShoppingCart.Delete(cartFromDb);
                
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.Save();
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ProductId == cartId, tracked:true);

            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
              .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

            _unitOfWork.ShoppingCart.Delete(cartFromDb);

            
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        private double GetCartPrice(ShoppingCart shoppingcart)
        {
            if (shoppingcart.Count <= 50)
                return shoppingcart.Product.Price;
            else
            {
                if (shoppingcart.Count <= 100)
                {
                    return shoppingcart.Product.Price50;
                }
                else
                    return shoppingcart.Product.Price100;
            }
        }
    }
}
