﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderDetailVM  OrderDetailVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Detail(int id)
        {
            OrderDetailVM = new OrderDetailVM()
            {
                orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser"),
                orderDetails = _unitOfWork.orderDeatail.GetAll(o => o.OrderId == id, includeProperties: "Product")
            };
            return View(OrderDetailVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Detail")]
        public IActionResult Details(string stripeToken)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderDetailVM.orderHeader.Id,
                                                includeProperties: "ApplicationUser");
            if (stripeToken != null)
            {
                //process the payment
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                    Currency = "usd",
                    Description = "Order ID : " + orderHeader.Id,
                    Source = stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.Id == null)
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
                else
                {
                    orderHeader.TransactionId = charge.Id;
                }
                if (charge.Status.ToLower() == "succeeded")
                {
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;

                    orderHeader.PaymentDate = DateTime.Now;
                }

                _unitOfWork.Save();

            }
            return RedirectToAction("Detail", "Order", new { id = orderHeader.Id });
        }


        [Authorize(Roles = SD.Role_Admin+ "," +SD.Role_Employee )]
        public IActionResult StartProcessing(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
           
                orderHeader.OrderStatus = SD.StatusInProcess;
            _unitOfWork.Save();

            return RedirectToAction("Index");

               
           
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderDetailVM.orderHeader.Id);
            orderHeader.TrackingNumber = OrderDetailVM.orderHeader.TrackingNumber;
            orderHeader.Carrier = OrderDetailVM.orderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            if (orderHeader.PaymentStatus == SD.StatusApproved)
            {
                var option = new RefundCreateOptions()
                {
                    Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),

                    Reason = RefundReasons.RequestedByCustomer,

                    Charge = orderHeader.TransactionId

                };

                var service = new RefundService();

                Refund refund = service.Create(option);
                orderHeader.OrderStatus = SD.StatusRefunded;
                orderHeader.PaymentStatus = SD.StatusRefunded;


            }

            else
            {
                orderHeader.OrderStatus = SD.StatusCancelled;
                orderHeader.PaymentStatus = SD.StatusCancelled;
            }
           
            _unitOfWork.Save();

            return RedirectToAction("Index");



        }


        #region API Call

        [HttpGet]
        public IActionResult GetOrderList(string status)
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var claims = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> orderHeadersList;

            if(User.IsInRole(SD.Role_Admin)||User.IsInRole(SD.Role_Employee))

            {
                orderHeadersList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                 orderHeadersList = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId==claims.Value,includeProperties:"ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeadersList= orderHeadersList.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    orderHeadersList = orderHeadersList.Where(o => o.OrderStatus == SD.StatusApproved || o.OrderStatus == SD.StatusInProcess || o.OrderStatus == SD.StatusPending);
                    break;
                case "completed":
                    orderHeadersList = orderHeadersList.Where(o => o.OrderStatus == SD.StatusShipped);
                    
                    break;
                case "rejected":
                    orderHeadersList = orderHeadersList.Where(o => o.OrderStatus == SD.StatusCancelled || o.OrderStatus == SD.StatusRefunded || o.OrderStatus == SD.PaymentStatusRejected);


                    break;
                default:
                    
                    break;
            }


            return Json(new { data = orderHeadersList });
        }

        #endregion

    }
}
