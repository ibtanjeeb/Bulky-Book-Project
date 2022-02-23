using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.DataAccess.Repository.IRepository
{
   public interface IUnitOfWork:IDisposable
    {
        public ICategoryRepository Category { get; }
        public ICoverTypeRepository coverType { get; }
        public ISP_Call SP_Call { get; }

        void Save();

    }
}
