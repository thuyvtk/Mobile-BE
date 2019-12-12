using System;
using System.Collections.Generic;
using GiatDo.Model;
using GiatDo.Service.Service;
using GIatDo.ViewModel;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace GIatDo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlotController : ControllerBase
    {
        private readonly IShipperService _shipperService;
        private readonly ISlotService _slotService;
        private readonly IOrderService _orderService;

        public SlotController(IShipperService shipperService, ISlotService slotService, IOrderService orderService)
        {
            _shipperService = shipperService;
            _slotService = slotService;
            _orderService = orderService;
        }

        [HttpGet("GetById")]
        public ActionResult GetSlotById(Guid Id)
        {
            return Ok(_slotService.GetSlot(Id).Adapt<SlotVM>());
        }

        [HttpGet("GetAll")]
        public ActionResult GetSlotAll()
        {
            return Ok(_slotService.GetSlots(s=> !s.IsDelete).Adapt<List<SlotVM>>());
        }
        //[HttpGet("GetByDay")]
        //public ActionResult GetSlotByDay(DateTime date)
        //{
        //    return Ok(_slotService.GetSlots(s => s.TimeEnd.Date == date.Date).Adapt<List<SlotVM>>());
        //}
        [HttpPost]
        public ActionResult CreateSlot([FromBody]SlotCM model)
        {
            Slot slot = model.Adapt<Slot>();
            slot.IsDelete = false;
            _slotService.CreateSlot(slot);
            _slotService.Save();
            return Ok(201);
        }
        [HttpDelete]
        public ActionResult DeleteSlot(Guid Id)
        {
            var model = _slotService.GetSlot(Id);
            if(model == null)
            {
                return NotFound();
            }
            _slotService.DeleteSlot(model);
            _slotService.Save();
            return Ok(201);
        }
        [HttpPut]
        public ActionResult UpdateSlot([FromBody] SlotVM model)
        {
            var test = _slotService.GetSlot(model.Id);
            if (test == null) { return NotFound(401); }
            _slotService.UpdateSlot(model.Adapt(test));
            _slotService.Save();
            return Ok(201);
        }
    }
}