using EventCalendar.Controllers.Dtos;
using EventCalendar.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendar.Controllers;

/// <summary>
/// Позволяет управлять событиями
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    private const string EventNotFoundTitle = "Событие не найдено";
    private const string EventAlreadyExistsTitle = "Событие с таким идентификатором уже существует";

    /// <summary>
    /// Возвращает полный список событий.
    /// </summary>
    /// <response code="200">Список событий найден</response>
    [ProducesResponseType(typeof(ActionResult<IEnumerable<GetEventDto>>), StatusCodes.Status200OK)]
    [Produces("application/json")]
    [HttpGet]
    public ActionResult<IEnumerable<GetEventDto>> GetEvents()
    {
        return Ok(eventService.GetEvents().Select(Mapper.ToGetEventDto));
    }

    /// <summary>
    /// Возвращает событие по идентификатору.
    /// </summary>
    /// <param name="id">GUID события</param>
    /// <response code="200">Событие найдено</response>
    /// <response code="404">Событие с указанным идентификатором не найдено</response>
    [ProducesResponseType(typeof(ActionResult<GetEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [HttpGet("{id:guid}")]
    public ActionResult<GetEventDto> GetEvent([FromRoute] Guid id)
    {
        var @event = eventService.GetEvent(id);
        return @event != null
            ? Ok(@event.ToGetEventDto())
            : NotFound(new ProblemDetails { Title = EventNotFoundTitle, Status = StatusCodes.Status404NotFound });
    }

    /// <summary>
    /// Создаёт событие из Json-объекта.
    /// Поддерживает два варианта: с идентификатором (если событие уже создано и используется на клиенте) и без.
    /// В случае если событие с указанным идентификатором существует, возвращает ошибку не создавая события.
    /// </summary>
    /// <param name="eventDto">Событие в виде Json-объекта</param>
    /// <response code="200">Событие успешно создано</response>
    /// <response code="409">Событие не создано, т.к. событие с указанным идентификатором уже существует на сервере.
    /// Требуется предпринять действие на клиенте</response>
    [ProducesResponseType(typeof(GetEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPost]
    public IActionResult Post([FromBody] CreateEventDto eventDto)
    {
        while (true)
        {
            var @event = eventDto.ToEntity();

            if (eventService.AddEvent(@event))
                return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, @event.ToGetEventDto());

            if (eventDto.Id.HasValue) // если событие уже используется на клиенте с созданным офлайн Id
                return Conflict(new ProblemDetails
                    { Title = EventAlreadyExistsTitle, Status = StatusCodes.Status409Conflict });
        }
    }

    /// <summary>
    /// Полностью обновляет событие.
    /// </summary>
    /// <param name="id">GUID события</param>
    /// <param name="eventDto">Событие в виде Json-объекта</param>
    /// <response code="200">Событие обновлено</response>
    /// <response code="404">Событие с указанным идентификатором не найдено</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [Consumes("application/json")]
    [HttpPut("{id:guid}")]
    public IActionResult Put([FromRoute] Guid id, [FromBody] EventDto eventDto)
    {
        var @event = eventDto.ToEntity(id);
        return eventService.ChangeEvent(@event)
            ? Ok()
            : NotFound(new ProblemDetails { Title = EventNotFoundTitle, Status = StatusCodes.Status404NotFound });
    }

    /// <summary>
    /// Удаляет событие
    /// </summary>
    /// <param name="id">GUID события</param>
    /// <response code="200">Событие удалено</response>
    /// <response code="404">Событие с указанным идентификатором не найдено</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        return eventService.RemoveEvent(id)
            ? Ok()
            : NotFound(new ProblemDetails { Title = EventNotFoundTitle, Status = StatusCodes.Status404NotFound });
    }
}