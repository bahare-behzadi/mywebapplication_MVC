using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mywebapplication.DataAccess.Repository.IRepository;
using mywebapplication.Models.Models;
using mywebapplication.Models.ViewModels;
using mywebapplication.Utility;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace MyWebApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVm OrederVm { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrederVm = new()
            {
                Header = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == orderId, includeproperties: "ApplicationUser"),
                Details = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeproperties: "Product")
            };

            return View(OrederVm);
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin +","+ SD.Role_Employee)]
        public IActionResult DetailsUpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == OrederVm.Header.OrderHeaderId);
            orderHeaderFromDb.Name = OrederVm.Header.Name;
            orderHeaderFromDb.streetAdress = OrederVm.Header.streetAdress;
            orderHeaderFromDb.city = OrederVm.Header.city;
            orderHeaderFromDb.state = OrederVm.Header.state;
            orderHeaderFromDb.postalCode = OrederVm.Header.postalCode;
            if(!string.IsNullOrEmpty(OrederVm.Header.Carrier))
                orderHeaderFromDb.Carrier = OrederVm.Header.Carrier;
            if (!string.IsNullOrEmpty(OrederVm.Header.TrackingNymber))
                orderHeaderFromDb.TrackingNymber = OrederVm.Header.TrackingNymber;
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order updated successfyly";
            return RedirectToAction(nameof(Details), new {orderId = orderHeaderFromDb.OrderHeaderId});
              
        }

        [HttpPost]
        [Authorize (Roles = SD.Role_Admin+ "," + SD.Role_Employee )]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrederVm.Header.OrderHeaderId, SD.statusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order updated successfyly";
            return RedirectToAction(nameof(Details), new { orderId = OrederVm.Header.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == OrederVm.Header.OrderHeaderId);
            orderHeaderFromDb.TrackingNymber = OrederVm.Header.TrackingNymber;
            orderHeaderFromDb.Carrier = OrederVm.Header.Carrier;
            orderHeaderFromDb.OrderStatus = SD.statusShiped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentstatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDue = DateOnly.FromDateTime( DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order shipped successfyly";
            return RedirectToAction(nameof(Details), new { orderId = OrederVm.Header.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == OrederVm.Header.OrderHeaderId);
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentstatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId,
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.OrderHeaderId, SD.statusCanceled, SD.statusRefund);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.OrderHeaderId, SD.statusCanceled, SD.statusCanceled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order canceled successfyly";
            return RedirectToAction(nameof(Details), new { orderId = OrederVm.Header.OrderHeaderId });
        }


        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrederVm.Header = _unitOfWork.OrderHeader
                .Get(u => u.OrderHeaderId == OrederVm.Header.OrderHeaderId, includeproperties: "ApplicationUser");
            OrederVm.Details = _unitOfWork.OrderDetail
                .GetAll(u => u.OrderHeaderId == OrederVm.Header.OrderHeaderId, includeproperties: "Product");

            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrederVm.Header.OrderHeaderId}",
                CancelUrl = domain + $"admin/order/details?orderId={OrederVm.Header.OrderHeaderId}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrederVm.Details)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentId(OrederVm.Header.OrderHeaderId, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {

            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.OrderHeaderId == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentstatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentstatusApproved);
                    _unitOfWork.Save();
                }


            }


            return View(orderHeaderId);
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaderList;
            
            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaderList = _unitOfWork.OrderHeader.GetAll(includeproperties: "ApplicationUser").ToList();

            }
            else
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeaderList = _unitOfWork.OrderHeader.GetAll(user => user.ApplicationUserId == claim, includeproperties: "ApplicationUser");

            }

            switch (status)
            {
                case "pending":
                    objOrderHeaderList = objOrderHeaderList.Where(u=>u.PaymentStatus == SD.PaymentstatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaderList = objOrderHeaderList.Where(u => u.OrderStatus == SD.statusInProcess);
                    break;
                case "completed":
                    objOrderHeaderList = objOrderHeaderList.Where(u => u.OrderStatus == SD.statusShiped);
                    break;
                case "approved":
                    objOrderHeaderList = objOrderHeaderList.Where(u => u.OrderStatus == SD.statusApproved);
                    break;
                default:
                    break;


            }


            return Json(new { data = objOrderHeaderList });
        }
    }
}
