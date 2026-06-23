using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace Friday.Backend.Api.Controllers;

[ApiController]
[Route("api/bot")]
[Route("api/v1/bot")]
public sealed class BotController : ControllerBase
{
  private readonly IBotService _service;

  public BotController(IBotService service)
  {
    _service = service;
  }

  [HttpGet("servers")]
  public async Task<IActionResult> GetEnabledGuilds()
  {
    var result = await _service.GetEnabledGuilds();
    return result.Send();
  }

  [HttpGet("servers/{guildId:long}/profile")]
  public async Task<IActionResult> GetGuildProfile([FromRoute] long guildId)
  {
    var result = await _service.GetGuildProfile(guildId);
    return result.Send();
  }

  [HttpGet("servers/{guildId:long}/commands/{commandName}")]
  public async Task<IActionResult> GetCommandResponse([FromRoute] long guildId, [FromRoute] string commandName)
  {
    var result = await _service.GetCommandResponse(guildId, commandName);
    return result.Send();
  }

  [HttpPost("servers/{guildId:long}/members/verify")]
  public async Task<IActionResult> VerifyMember([FromRoute] long guildId, [FromBody] VerifyMemberRequest request)
  {
    var result = await _service.VerifyMember(guildId, request);
    return result.Send();
  }

  [HttpPost("servers/{guildId:long}/members/xp")]
  public async Task<IActionResult> AddXp([FromRoute] long guildId, [FromBody] XpRequest request)
  {
    var result = await _service.AddXp(guildId, request);
    return result.Send();
  }

  [HttpPost("servers/{guildId:long}/sync")]
  public async Task<IActionResult> SyncGuild([FromRoute] long guildId, [FromBody] BotSyncRequest request)
  {
    var normalized = new BotSyncRequest
    {
      GuildId = guildId,
      GuildName = request.GuildName,
      SyncedByDiscordId = request.SyncedByDiscordId,
      Roles = request.Roles,
      Channels = request.Channels
    };

    var result = await _service.SyncGuild(normalized);
    return result.Send();
  }
}
