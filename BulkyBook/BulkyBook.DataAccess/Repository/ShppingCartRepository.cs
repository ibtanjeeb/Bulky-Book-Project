using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class ShppingCartRepository : Repository<ShoppingCart>, IShppingCartRepository
    {
        private readonly ApplicationDbContext _db;

        public ShppingCartRepository(ApplicationDbContext db):base(db) 
        {
            _db = db;
        }
        public void Update(ShoppingCart obj)
        {
            _db.Update(obj);
        }
    }
}
