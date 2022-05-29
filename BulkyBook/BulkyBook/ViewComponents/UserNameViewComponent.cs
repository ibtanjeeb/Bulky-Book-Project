using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.ViewComponents
{
    public class UserNameViewComponent:ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserNameViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var UserFromDb = _unitOfWork.applicationUser.GetFirstOrDefault(u => u.Id == claims.Value);

            return View(UserFromDb);

        }
    }
}
