using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class CategoryRepository : RepositoryAsync<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db):base(db) 
        {
            _db = db;
        }
        public void Update(Category category)
        {
            var objFromdb = _db.Categories.FirstOrDefault(s => s.Id == category.Id);
            if(objFromdb!=null)
            {
                objFromdb.Name = category.Name;
                _db.SaveChanges();
            }
        }

        
    }
}
