using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
   public class UnitOfWork:IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;

            Category = new CategoryRepository(_db);
            Company = new CompanyRepository(_db);
            coverType = new CoverTypeRepository(_db);
            product = new ProductRepository(_db);
            applicationUser = new ApplicationUserRepository(_db);
            SP_Call = new SP_Call(_db);

            shppingCart = new ShppingCartRepository(_db);
            orderDeatail = new OrderDeatailRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);

        }

        public ICategoryRepository Category { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IApplicationUserRepository applicationUser { get; private set; }
        public ICoverTypeRepository coverType { get; private set; }

        public IProductRepository product { get; private set; }
        public IShppingCartRepository shppingCart { get; private set; }
        

        public IOrderHeaderRepository OrderHeader { get; private set; }

        public IOrderDeatailRepository  orderDeatail { get; private set; }
        public ISP_Call SP_Call { get; private set; }

        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
