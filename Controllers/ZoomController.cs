using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ZoomIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZoomController : ControllerBase
    {
        private readonly ZoomService _zoomService;

        public ZoomController(ZoomService zoomService)
        {
            _zoomService = zoomService;
        }
    [HttpPost("create-meeting")]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequest request)
    {
        try
        {
            var meetingUrl = await _zoomService.CreateMeetingAsync(
                request.UserId, request.Topic, request.StartTime, request.Duration
            );
            return Ok(new { MeetingUrl = meetingUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    }
}

public class CreateMeetingRequest
{
    public string UserId { get; set; }
    public string Topic { get; set; }
    public string StartTime { get; set; }
    public int Duration { get; set; }
}