using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db):base(db) 
        {
            _db = db;
        }
        public void Update(Product product)
        {
            var objFromdb = _db.Products.FirstOrDefault(s => s.Id == product.Id);
            if(objFromdb!=null)
            {
                if(objFromdb.ImageUrl!=null)
                {
                    objFromdb.ImageUrl = product.ImageUrl;

                }
                objFromdb.ISBN = product.ISBN;
                objFromdb.ListPrice = product.ListPrice;
                objFromdb.Price = product.Price;
                objFromdb.Price50 = product.Price50;
                objFromdb.Price100 = product.Price100;
                objFromdb.Title = product.Title;
                objFromdb.Author = product.Author;
                objFromdb.Description = product.Description;
                objFromdb.CategoryId = product.CategoryId;
                objFromdb.CoverTypeId = product.CoverTypeId;

                
            }
        }
    }
}
