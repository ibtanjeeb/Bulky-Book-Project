using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        public IUnitOfWork _unitOfWork;
        public IEmailSender _emailSender;
        public UserManager<IdentityUser> _userManager;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork,IEmailSender emailSender,UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                CartList = _unitOfWork.shppingCart.GetAll(u =>u.ApplicationUserId == claims.Value,includeProperties:"Product")
                
                

            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUser
                .GetFirstOrDefault(u => u.Id == claims.Value, includeProperties: "Company");

            foreach(var list in ShoppingCartVM.CartList)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                list.Product.Description = SD.ConvertToRawHtml(list.Product.Description);
                if(list.Product.Description.Length>100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99)+"...";
                }

            }

            return View(ShoppingCartVM);

        }

        public IActionResult Plus(int CartId)
        {
            var cart = _unitOfWork.shppingCart.GetFirstOrDefault(u => u.Id == CartId,includeProperties:"Product");
            cart.Count += 1;
            cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
               
        }
        public IActionResult Minus(int CartId)
        {
            var cart = _unitOfWork.shppingCart.GetFirstOrDefault(u => u.Id == CartId, includeProperties: "Product");
            if (cart.Count == 1)
            {
                var cnt = _unitOfWork.shppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId.ToString()).Count();
                _unitOfWork.shppingCart.Remove(cart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SsShoppinCart, cnt - 1);
            }
            else
            {
                cart.Count -= 1;
                cart.Price = SD.GetPriceBasedOnQuantity(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);
                _unitOfWork.Save();
            }
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Remove(int CartId)
        {
            var cart = _unitOfWork.shppingCart.GetFirstOrDefault(u => u.Id == CartId, includeProperties: "Product");
            
                var cnt = _unitOfWork.shppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId.ToString()).Count();
                _unitOfWork.shppingCart.Remove(cart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SsShoppinCart, cnt - 1);
            
           
                
            
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Summery()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new Models.OrderHeader(),
                CartList = _unitOfWork.shppingCart.GetAll(c => c.ApplicationUserId == claim.Value,includeProperties:"Product")
                
            };
            
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUser.GetFirstOrDefault(u => u.Id == claim.Value, includeProperties: "Company");


            foreach (var list in ShoppingCartVM.CartList)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
               

            }

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summery")]
        [ValidateAntiForgeryToken]
        public IActionResult SummeyPost(string stripeToken)
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var claims = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUser.GetFirstOrDefault(c => c.Id == claims.Value,includeProperties:"Company");

            ShoppingCartVM.CartList = _unitOfWork.shppingCart.GetAll(c => c.ApplicationUserId == claims.Value,includeProperties:"Product");

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);

            _unitOfWork.Save();
            List<OrderDetail> orderDetailsList = new List<OrderDetail>();
            
                foreach( var item in ShoppingCartVM.CartList)
            {
                item.Price = SD.GetPriceBasedOnQuantity(item.Count, item.Product.Price
                    , item.Product.Price50, item.Product.Price100);
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    Product = item.Product,
                    OrderId = ShoppingCartVM.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count


                };
                ShoppingCartVM.OrderHeader.OrderTotal += orderDetail.Price *orderDetail.Count;
                _unitOfWork.orderDeatail.Add(orderDetail);

               
                
            }

            _unitOfWork.shppingCart.RemoveRange(ShoppingCartVM.CartList);
            _unitOfWork.Save();
            

            HttpContext.Session.SetInt32(SD.SsShoppinCart,0);

            if(stripeToken==null)
            {
                //Order Will Payment  Delayed For Authorization Company
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.PaymentStatusApproved;

            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal*100),
                    Currency = "BDT",
                    Description = "Order ID" + ShoppingCartVM.OrderHeader.Id,
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if(charge.BalanceTransactionId==null)
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                }
                if (charge.Status.ToLower() =="Succeeded")
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
                }

            }
            _unitOfWork.Save();

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
            

        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }


    }
}
