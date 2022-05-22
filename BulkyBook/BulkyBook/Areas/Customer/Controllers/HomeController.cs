using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfwork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfwork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productlist = _unitOfwork.product.GetAll(includeProperties:"Category,CoverType");
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim!=null)
            {
                
                var count = _unitOfwork.shppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count();
                //HttpContext.Session.SetObject(SD.SsShoppinCart, CartObject);
                HttpContext.Session.SetInt32(SD.SsShoppinCart, count);
                
            }
            return View(productlist);
        }

        public IActionResult Details(int id)
        {
            var objfromProduct = _unitOfwork.product.GetFirstOrDefault(u => u.Id == id, includeProperties:
                "Category,CoverType");
            ShoppingCart shoppingCart = new ShoppingCart()
            {
                ProductId = objfromProduct.Id,
                Product = objfromProduct
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //then we will add to cart
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _unitOfwork.shppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId == CartObject.ProductId
                    , includeProperties: "Product"
                    );

                if (cartFromDb == null)
                {
                    //no records exists in database for that product for that user
                    _unitOfwork.shppingCart.Add(CartObject);
                }
                else
                {
                    cartFromDb.Count += CartObject.Count;
                    //_unitOfWork.ShoppingCart.Update(cartFromDb);
                }
                _unitOfwork.Save();
                var count = _unitOfwork.shppingCart.GetAll(u => u.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();
                //HttpContext.Session.SetObject(SD.SsShoppinCart, CartObject);
                HttpContext.Session.SetInt32(SD.SsShoppinCart, count);
                return RedirectToAction(nameof(Index));

            }
            else
            {
                var objfromProduct = _unitOfwork.product.GetFirstOrDefault(u => u.Id == CartObject.ProductId, includeProperties:
                "Category,CoverType");
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    ProductId = objfromProduct.Id,
                    Product = objfromProduct
                };
                return View(shoppingCart);
            }
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
