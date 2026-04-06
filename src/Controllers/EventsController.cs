using EventCalendar.Controllers.Dtos;
using EventCalendar.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendar.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<EventDto>> GetEvents()
    {
        return Ok(eventService.GetEvents().Select(Mapper.ToGetEventDto));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<GetEventDto> GetEvent([FromRoute] Guid id)
    {
        var @event = eventService.GetEvent(id);
        return @event != null ? Ok(@event.ToGetEventDto()) : NotFound();
    }

    [HttpPost]
    public IActionResult Post([FromBody] CreateEventDto eventDto)
    {
        while (true)
        {
            var @event = eventDto.ToEntity();

            if (eventService.AddEvent(@event))
                return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, @event.ToGetEventDto());

            if (eventDto.Id.HasValue) // если событие уже используется на клиенте с созданным офлайн Id
                return Conflict(@event.Id);
        }
    }

    [HttpPut("{id:guid}")]
    public IActionResult Put([FromRoute] Guid id, [FromBody] EventDto eventDto)
    {
        var @event = eventDto.ToEntity(id);
        return eventService.ChangeEvent(@event) ? Ok() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        return eventService.RemoveEvent(id) ? Ok() : NotFound();
    }
}