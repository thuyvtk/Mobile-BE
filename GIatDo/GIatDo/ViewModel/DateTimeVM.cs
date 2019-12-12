using System;
using System.Collections.Generic;

namespace GIatDo.ViewModel
{
    public class DateTimeVM
    {
        public DateTime Date { get; set; }
        public List<OrderCM> ListOrder { get; set; }
    }
    public class DateTimeVN
    {
        public DateTime Date { get; set; }
        public List<OrderVN> ListOrder { get; set; }
    }

    public class MonthVM
    {
        public DateTime Month { get; set; }
        public List<OrderVN> ListOrder { get; set; }
    }

    public class OrderVN
    {
        public Guid Id { get; set; }
        public float TotalPrice { get; set; }
        public string Status { get; set; }
        public CustomerCM customer { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public DateTime? TakeTime { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Address { get; set; }
        public List<ListOrderServiceCM> OrderServices { get; set; }
    }

    public class OrderTH
    {
        public Guid Id { get; set; }
        public DateTime DeliveryTime { get; set; }
        public float TotalPrice { get; set; }
        public string Status { get; set; }
        public bool IsDelete { get; set; }
    }
}
