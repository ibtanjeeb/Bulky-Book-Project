using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
   
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            Category category = new Category();

            if(id==null)
            {
                return View(category);
            }

            category = _unitOfWork.Category.Get(id.GetValueOrDefault());
            
            if(category==null)
            {
                return NotFound();
            }
            return View(category);
            return View();
        }


        #region API CAlls

        [HttpGet]
        public IActionResult GetAll()
        {

            var allobj = _unitOfWork.Category.GetAll();

            return Json(new { data = allobj });
        }


        #endregion
    }
}
