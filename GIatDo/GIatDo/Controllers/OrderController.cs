using System;
using System.Collections.Generic;
using System.Linq;
using GiatDo.Model;
using GiatDo.Service.Service;
using GIatDo.Hubs;
using GIatDo.ViewModel;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GIatDo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly IOrderService _orderService;
        private readonly ISlotService _slotService;
        private readonly ICustomerService _customerService;
        private readonly IShipperService _shipperService;
        private readonly IOrderSService _orderServiceService;
        private readonly IServiceService _serviceService;
        private readonly IStoreService _storeService;
        private readonly IHubContext<CenterHub> _hub;

        public OrderController(IOrderService orderService, ISlotService slotService, ICustomerService customerService, IShipperService shipperService, IOrderSService orderServiceService, IServiceService serviceService, IStoreService storeService, IHubContext<CenterHub> hub)
        {
            _orderService = orderService;
            _slotService = slotService;
            _customerService = customerService;
            _shipperService = shipperService;
            _orderServiceService = orderServiceService;
            _serviceService = serviceService;
            _storeService = storeService;
            _hub = hub;
        }

        [HttpPost("CreateOrder")]
        public ActionResult CreateOrder([FromBody] OrderPO model)
        {
            var timeCreate = DateTime.Now;
            DateTime? timeDelivery = model.DeliveryTime;
            DateTime? takeTime = model.TakeTime;
            var deliver = _slotService.GetSlots().Where(t1 => t1.TimeStart <= timeDelivery.Value.TimeOfDay).Where(t2 => t2.TimeEnd >= timeDelivery.Value.TimeOfDay).ToList();
            var take = _slotService.GetSlots().Where(t1 => t1.TimeStart <= takeTime.Value.TimeOfDay).Where(t2 => t2.TimeEnd >= takeTime.Value.TimeOfDay).ToList();
                var checkCus = _customerService.GetCustomer(model.CustomerId.Value);
                if (checkCus == null)
                {
                    return NotFound();
                }

                Order newOrder = model.Adapt<Order>();
                newOrder.IsDelete = false;
                newOrder.SlotTakeId = take[0].Id;
                newOrder.SlotDeliveryId = deliver[0].Id;
                newOrder.CreateTime = timeCreate;
                _orderService.CreateOrder(newOrder);
                _orderService.Save();
                 _hub.Clients.All.SendAsync("transferchartdata", "Có Đơn hàng được tạo");
                return Ok(201);

        }

        [HttpGet("GetByCustomerId")]
        public ActionResult GetOrder(Guid Id, DateTime DateCreateStart, DateTime DateCreateEnd, string Status)
        {
            var checkCus = _customerService.GetCustomer(Id);
            if (checkCus == null)
            {
                return NotFound("Not Found Customer");
            }
            List<DateTimeVM> listDateTime = new List<DateTimeVM>();
            //get orders
            var result = _orderService.GetOrders(o => o.CustomerId == Id && o.DateCreate >= DateCreateStart.AddDays(-1) && o.DateCreate <= DateCreateEnd.AddDays(1) && !o.Status.ToLower().Equals("done") && !o.Status.ToLower().Equals("cancel"));
            foreach (Order item in result)
            {
                DateTimeVM date = new DateTimeVM();
                date.Date = item.DateCreate;
                OrderCM orderCM = item.Adapt<OrderCM>();
                bool flag = false;
                foreach (DateTimeVM dates in listDateTime)
                {
                    if (date.Date.ToString("dd/MM/yyyy").Equals(dates.Date.ToString("dd/MM/yyyy")))
                    {
                        flag = true;
                        dates.ListOrder.Add(orderCM);
                    }
                }
                if (!flag)
                {
                    date.ListOrder = new List<OrderCM>();
                    date.ListOrder.Add(orderCM);
                    listDateTime.Add(date);
                }
            }
            return Ok(listDateTime);
        }

        [HttpGet("GetOrderHistory")]
        public ActionResult GetOrderHistory(Guid Id, DateTime DateCreateStart, DateTime DateCreateEnd)
        {
            var checkCus = _customerService.GetCustomer(Id);
            if (checkCus == null)
            {
                return NotFound("Not Found Customer");
            }
            List<DateTimeVM> listDateTime = new List<DateTimeVM>();
            //get orders
            var result = _orderService.GetOrders(o => o.CustomerId == Id && o.DateCreate >= DateCreateStart.AddDays(-1) && o.DateCreate <= DateCreateEnd.AddDays(1) && (o.Status.ToLower().Equals("done") || o.Status.ToLower().Equals("cancel")));
            foreach (Order item in result)
            {
                DateTimeVM date = new DateTimeVM();
                date.Date = item.DateCreate;
                OrderCM orderCM = item.Adapt<OrderCM>();
                bool flag = false;
                foreach (DateTimeVM dates in listDateTime)
                {
                    if (date.Date.ToString("dd/MM/yyyy").Equals(dates.Date.ToString("dd/MM/yyyy")))
                    {
                        flag = true;
                        dates.ListOrder.Add(orderCM);
                    }
                }
                if (!flag)
                {
                    date.ListOrder = new List<OrderCM>();
                    date.ListOrder.Add(orderCM);
                    listDateTime.Add(date);
                }
            }
            return Ok(listDateTime);
        }


        [HttpPut("UpdateShipperTake")]
        public ActionResult UpdateShipperTake([FromBody]UpdateShipperTake model)
        {
            var CheckShipper = _shipperService.GetShipper(model.ShipperId);
            if (CheckShipper == null)
            {
                return NotFound("Shipper Not Found");
            }
            var _order = _orderService.GetOrders(o => o.Id == model.OrderId).AsEnumerable().ElementAt(0);
            _order.ShipperTakeId = CheckShipper.Id;
            _orderService.UpdateOrder(_order);
            _orderService.Save();
            return Ok(201);
        }
        [HttpPut("UpdateSlotDelivery")]
        public ActionResult UpdateSlotDelivery([FromBody]UpdateSlotDelivery model)
        {
            var checkSlot = _slotService.GetSlot(model.SlotId);
            if (checkSlot == null)
            {
                return NotFound("Slot Not Found");
            }
            var _order = _orderService.GetOrders(o => o.Id == model.OrderId).AsEnumerable().ElementAt(0);
            _order.SlotDeliveryId = checkSlot.Id;
            _orderService.UpdateOrder(_order);
            _orderService.Save();
            return Ok(201);
        }
        [HttpPut("UpdateShipperDelivery")]
        public ActionResult UpdateShipperDelivery([FromBody]UpdateShipperDelivery model)
        {
            var CheckShipper = _shipperService.GetShipper(model.ShipperId);
            if (CheckShipper == null)
            {
                return NotFound("Shipper Not Found");
            }
            var _order = _orderService.GetOrders(o => o.Id == model.OrderId).AsEnumerable().ElementAt(0);
            _order.ShipperDeliverId = CheckShipper.Id;
            _orderService.UpdateOrder(_order);
            _orderService.Save();
            return Ok(201);
        }
        [HttpDelete("DeleteOrder")]
        public ActionResult Delete(Guid Id)
        {
            var checkOrder = _orderService.GetOrder(Id);
            if (checkOrder == null)
            {
                return NotFound("Not Found Order");
            }
            var listOrderService = _orderServiceService.GetOrderServices(s => s.OrderId == Id).ToList();
            foreach (var item in listOrderService)
            {
                _orderServiceService.DeleteOrderService(item);
            }
            _orderService.DeleteOrder(checkOrder);
            _orderService.Save();
            _orderServiceService.Save();
            return Ok(201);
        }

        [HttpGet("GetOrderByStatus")]
        public ActionResult GetOrderByStatus(String status, Guid storeId)
        {
            try
            {
                var order = _orderService.GetOrders(t => !t.IsDelete);
                var orderService = _orderServiceService.GetOrderServices(t => !t.IsDelete);
                var service = _serviceService.GetServices(t => !t.IsDelete);
                var store = _storeService.GetStores(t => !t.IsDelete);
                var temp = from t in order
                           join o in orderService on t.Id equals (o.OrderId)
                           join s in service on o.ServiceId equals (s.Id)
                           join st in store on s.StoreId equals (st.Id)
                           where t.Status.Equals(status)
                           where st.Id.Equals(storeId)
                           select t;
                List<Order> list = temp.ToList();
                return Ok(list.Adapt<List<OrderVM>>());
            }
            catch (Exception e)
            {
                e.ToString();
            }
            return Ok();
        }

        [HttpPut("RateStore")]
        public ActionResult RateStore(Guid Id, float Rate)
        {
            var store = _storeService.GetStore(Id);
            float rateNumber = store.Rate;
            float newRate = 0;
            if (rateNumber == 0)
            {
                newRate = Rate;
            }
            else
            {
                newRate = ((rateNumber * 5) + Rate) / 6;
            }

            store.Rate = float.Parse(newRate.ToString("0.0"));
            _storeService.UpdateStore(store);
            _storeService.Save();
            return Ok(201);
        }

        [HttpGet("GetByStoreId")]
        public ActionResult GetOrderByStore(Guid Id, DateTime DateCreateStart, DateTime DateCreateEnd, String Status)
        {
            var checkCus = _storeService.GetStore(Id);
            if (checkCus == null)
            {
                return NotFound("Not Found Customer");
            }
            List<DateTimeVN> listDateTime = new List<DateTimeVN>();
            //get orders
            var order = _orderService.GetOrders(t => !t.IsDelete);
            var orderService = _orderServiceService.GetOrderServices(t => !t.IsDelete);
            var service = _serviceService.GetServices(t => !t.IsDelete);
            var store = _storeService.GetStores(t => !t.IsDelete);
            var result = from st in store
                         join s in service on st.Id equals s.StoreId
                         join os in orderService on s.Id equals os.ServiceId
                         join o in order on os.OrderId equals o.Id
                         where st.Id == Id && o.DateCreate >= DateCreateStart.AddDays(-1)
                         && o.DateCreate <= DateCreateEnd.AddDays(1)
                         && o.Status.ToLower().Equals(Status)
                         select o;
            foreach (Order item in result.Distinct())
            {
                DateTimeVN date = new DateTimeVN();
                date.Date = item.DateCreate;
                OrderVN orderVN = item.Adapt<OrderVN>();
                orderVN.customer = item.Customer.Adapt<CustomerCM>();
                bool flag = false;
                foreach (DateTimeVN dates in listDateTime)
                {
                    if (date.Date.ToString("dd/MM/yyyy").Equals(dates.Date.ToString("dd/MM/yyyy")))
                    {
                        flag = true;
                        dates.ListOrder.Add(orderVN);
                    }
                }
                if (!flag)
                {
                    date.ListOrder = new List<OrderVN>();
                    date.ListOrder.Add(orderVN);
                    listDateTime.Add(date);
                }
            }
            return Ok(listDateTime);
        }


        [HttpGet("GetStoreOrderHistory")]
        public ActionResult GetStoreOrderHistory(Guid Id, DateTime DateCreateStart, DateTime DateCreateEnd)
        {
            var checkCus = _storeService.GetStore(Id);
            if (checkCus == null)
            {
                return NotFound("Not Found Customer");
            }
            List<DateTimeVN> listDateTime = new List<DateTimeVN>();
            //get orders
            var order = _orderService.GetOrders(t => !t.IsDelete);
            var orderService = _orderServiceService.GetOrderServices(t => !t.IsDelete);
            var service = _serviceService.GetServices(t => !t.IsDelete);
            var store = _storeService.GetStores(t => !t.IsDelete);
            var result = from st in store
                         join s in service on st.Id equals s.StoreId
                         join os in orderService on s.Id equals os.ServiceId
                         join o in order on os.OrderId equals o.Id
                         where st.Id == Id && o.DateCreate >= DateCreateStart.AddDays(-1)
                         && o.DateCreate <= DateCreateEnd.AddDays(1)
                         && (o.Status.ToLower().Equals("done")
                         || o.Status.ToLower().Equals("cancel"))
                         select o;
            foreach (Order item in result.Distinct())
            {
                DateTimeVN date = new DateTimeVN();
                date.Date = item.DateCreate;
                OrderVN orderVN = item.Adapt<OrderVN>();
                orderVN.customer = item.Customer.Adapt<CustomerCM>();
                bool flag = false;
                foreach (DateTimeVN dates in listDateTime)
                {
                    if (date.Date.ToString("dd/MM/yyyy").Equals(dates.Date.ToString("dd/MM/yyyy")))
                    {
                        flag = true;
                        dates.ListOrder.Add(orderVN);
                    }
                }
                if (!flag)
                {
                    date.ListOrder = new List<OrderVN>();
                    date.ListOrder.Add(orderVN);
                    listDateTime.Add(date);
                }
            }
            return Ok(listDateTime);
        }

        [HttpPut("SetOrderStatus")]
        public ActionResult SetOrderStatus(Guid Id,string Status)
        {
            string[] statues = { "ongoing","taken","onwarehousetake","onstore","washed",
            "onwarehousedelivery","ondelivery","done","cancel"};
            if (statues.Contains(Status))
            {
                var order = _orderService.GetOrder(Id);
                order.Status = Status;
                _orderService.UpdateOrder(order);
                _orderService.Save();
                _hub.Clients.All.SendAsync("transferchartdata", order.Status);
                return Ok(201);
            }         
            return NotFound();
        }
    }
}