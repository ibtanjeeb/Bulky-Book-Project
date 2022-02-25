using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
   
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            
            return View();
        }
        public IActionResult Upsert(int? id)
        {
           Product product  = new Product();

            if(id==null)
            {
                return View(product);
            }

           product = _unitOfWork.product.Get(id.GetValueOrDefault());
            
            if(product ==null)
            {
                return NotFound();
            }
            return View(product);
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Product product)
        {
            if(ModelState.IsValid)
            {
                if(product.Id==0)
                {
                    _unitOfWork.product.Add(product);
                }
                else
                {
                    _unitOfWork.product.Update(product);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }


        #region API CAlls

        [HttpGet]
        public IActionResult GetAll()
        {

            var allobj = _unitOfWork.product.GetAll();

            return Json(new { data = allobj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.product.Get(id);
            if(objFromDb==null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            _unitOfWork.product.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }


        #endregion
    }
}
