using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
   
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();

            if(id==null)
            {
                return View(coverType);
            }

            coverType = _unitOfWork.coverType.Get(id.GetValueOrDefault());
            
            if(coverType ==null)
            {
                return NotFound();
            }
            return View(coverType);
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if(ModelState.IsValid)
            {
                if(coverType.Id==0)
                {
                    _unitOfWork.coverType.Add(coverType);
                }
                else
                {
                    _unitOfWork.coverType.Update(coverType);
                }

                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }

            return View(coverType);
        }


        #region API CAlls

        [HttpGet]
        public IActionResult GetAll()
        {

            var allobj = _unitOfWork.coverType.GetAll();

            return Json(new { data = allobj });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.coverType.Get(id);
            if(objFromDb==null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            _unitOfWork.coverType.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }


        #endregion
    }
}
