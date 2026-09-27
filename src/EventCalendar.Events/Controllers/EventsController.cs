using EventCalendar.Events.Application.Services;
using EventCalendar.Events.Controllers.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventCalendar.Events.Controllers;

/// <summary>
/// Позволяет управлять событиями
/// </summary>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    /// <summary>
    /// Возвращает полный список событий.
    /// </summary>
    /// <param name="title">Фильтр по названию</param>
    /// <param name="from">Фильтр по дате начала</param>
    /// <param name="to">Фильтр по дате окончания</param>
    /// <param name="page">Страница, которую необходимо вернуть</param>
    /// <param name="pageSize">Количество элементов на странице</param>
    /// <response code="200">Список событий найден</response>
    /// <response code="400">Некорректный запрос</response>
    [ProducesResponseType(typeof(ActionResult<IEnumerable<GetEventDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetEventDto>>> GetEvents([FromQuery] string? title, [FromQuery] DateTime? from,
        [FromQuery] DateTime? to, [FromQuery] int page = EventService.DefaultPage,
        [FromQuery] int pageSize = EventService.DefaultPageSize)
    {
        var events = await eventService.GetEventsAsync(title, from, to, page, pageSize);
        return Ok(events.ToGetEventDto());
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
    public async Task<ActionResult<GetEventDto>> GetEvent([FromRoute] Guid id)
    {
        var @event = await eventService.GetEventAsync(id);
        return Ok(@event.ToGetEventDto());
    }

    /// <summary>
    /// Создаёт событие из Json-объекта.
    /// Поддерживает два варианта: с идентификатором (если событие уже создано и используется на клиенте) и без.
    /// В случае если событие с указанным идентификатором существует, возвращает ошибку не создавая события.
    /// </summary>
    /// <param name="eventDto">Событие в виде Json-объекта</param>
    /// <response code="201">Событие успешно создано</response>
    /// <response code="409">Событие не создано, т.к. событие с указанным идентификатором уже существует на сервере.
    /// Требуется предпринять действие на клиенте</response>
    [ProducesResponseType(typeof(GetEventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] EventDto eventDto)
    {
        var @event = eventDto.ToEntity();

        while (!await eventService.AddEventAsync(@event))
        {
            // повторяем пока не сгенерируем уникальный идентификатор
            @event = eventDto.ToEntity();
        }

        return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, @event.ToGetEventDto());
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] EventDto eventDto)
    {
        var @event = eventDto.ToEntity(id);
        await eventService.ChangeEventAsync(@event);
        return Ok();
    }

    /// <summary>
    /// Удаляет событие
    /// </summary>
    /// <param name="id">GUID события</param>
    /// <response code="204">Событие удалено</response>
    /// <response code="404">Событие с указанным идентификатором не найдено</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await eventService.RemoveEventAsync(id);
        return NoContent();
    }
}
