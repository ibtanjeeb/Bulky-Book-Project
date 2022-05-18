using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        public ICategoryRepository Category { get; }
        public ICompanyRepository Company { get; }
        public IApplicationUserRepository applicationUser { get; }
        public ICoverTypeRepository coverType { get; }
        public IProductRepository product { get; }
        public ISP_Call SP_Call { get; }
        public IShppingCartRepository shppingCart {get;}
        public IOrderHeaderRepository OrderHeader { get; }
        public IOrderDeatailRepository orderDeatail { get; }

        void Save();

    }
}
