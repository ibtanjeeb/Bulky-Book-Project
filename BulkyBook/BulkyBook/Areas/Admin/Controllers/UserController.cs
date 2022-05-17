using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {

        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            
            return View();
        }
        
          


        #region API CAlls

        [HttpGet]
        public IActionResult GetAll()
        {

            var UserList = _db.ApplicationUsers.Include(u => u.Company).ToList();
            var UserRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach(var user in UserList)
            {
                var RoleId = UserRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == RoleId).Name;
                if(user.Company==null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };

                }
            }

            return Json(new { data = UserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objfromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if(objfromDb==null)
            {
                return Json(new { success = false, message = "Error Occur While Lock/Unlock" });
            }
            if(objfromDb.LockoutEnd!=null&&objfromDb.LockoutEnd>DateTime.Now)
            {
                objfromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objfromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successfully" });
        }


        #endregion
    }
}
