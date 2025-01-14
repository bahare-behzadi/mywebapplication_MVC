﻿using mywebapplication.DataAccess.Data;
using mywebapplication.DataAccess.Repository.IRepository;
using mywebapplication.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace mywebapplication.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(ShoppingCart shoppingCartObj)
        {
            _db.ShoppingCarts.Update(shoppingCartObj);
        }


    }
}
