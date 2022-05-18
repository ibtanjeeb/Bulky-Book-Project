using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class OrderDeatailRepository : Repository<OrderDetail>, IOrderDeatailRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderDeatailRepository(ApplicationDbContext db):base(db) 
        {
            _db = db;
        }
        public void Update(OrderDetail obj)
        {
            _db.Update(obj);
        }
    }
}
