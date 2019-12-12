using GiatDo.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GiatDo.Data
{
    public class GiatDoDbContext : DbContext
    {
        public GiatDoDbContext() : base((new DbContextOptionsBuilder())
        .UseLazyLoadingProxies()
       .UseSqlServer(@"Server=laundry.database.windows.net
;Database=LaundryProject;user id=laundryAdmin
;password=admin@admin1
;Trusted_Connection=True;Integrated Security=false;")
           .Options)
        {
        }
        public DbSet<Account> Account { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderService> OrderService { get; set; }
        public DbSet<Shipper> Shipper { get; set; }
        public DbSet<Services> Service { get; set; }
        public DbSet<ServiceType> ServiceType { get; set; }
        public DbSet<Slot> Slot { get; set; }
        public DbSet<Store> Store { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            #region Oder

            builder.Entity<Order>(Order =>
            {
                Order.HasOne(r => r.ShipperDeliver).WithMany(u => u.OrderDelivery);
                Order.HasOne(r => r.ShipperTake).WithMany(u => u.OrderTakes);
                Order.HasOne(c => c.SlotTake).WithMany(u => u.OrderTake);
                Order.HasOne(d => d.SlotTake).WithMany(u => u.OrderTake);
            });
            #endregion
            base.OnModelCreating(builder);
        }

        public void Commit()
        {
            base.SaveChanges();
        }
    }
}
