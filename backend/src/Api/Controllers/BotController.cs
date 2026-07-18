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

  [HttpGet("profanities")]
  public async Task<IActionResult> GetProfanities()
  {
    var result = await _service.GetProfanities();
    return result.Send();
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

  [HttpPut("servers/{guildId:long}/profile")]
  public async Task<IActionResult> UpdateGuildProfile([FromRoute] long guildId, [FromBody] GuildProfileRequest request)
  {
    var result = await _service.UpdateGuildProfile(guildId, request);
    return result.Send();
  }

  [HttpGet("users")]
  public async Task<IActionResult> GetUsers()
  {
    var result = await _service.GetUsers();
    return result.Send();
  }

  [HttpGet("servers/{guildId:long}/members")]
  public async Task<IActionResult> GetGuildMembers([FromRoute] long guildId)
  {
    var result = await _service.GetGuildMembers(guildId);
    return result.Send();
  }

  [HttpGet("servers/{guildId:long}/roles")]
  public async Task<IActionResult> GetGuildRoles([FromRoute] long guildId)
  {
    var result = await _service.GetGuildRoles(guildId);
    return result.Send();
  }



  [HttpGet("servers/{guildId:long}/commands/{commandName}")]
  public async Task<IActionResult> GetCommandResponse([FromRoute] long guildId, [FromRoute] string commandName)
  {
    var result = await _service.GetCommandResponse(guildId, commandName);
    return result.Send();
  }

  [HttpGet("servers/{guildId:long}/teams")]
  public async Task<IActionResult> GetGuildTeams([FromRoute] long guildId)
  {
    var result = await _service.GetGuildTeams(guildId);
    return result.Send();
  }

  [HttpGet("servers/{guildId:long}/teams/prepas-export")]
  public async Task<IActionResult> GetGuildPrepaTeamExport(
    [FromRoute] long guildId)
  {
    var result = await _service.GetGuildPrepaTeamExport(guildId);
    return result.Send();
  }

  [HttpPut("servers/{guildId:long}/roster")]
  public async Task<IActionResult> SaveGuildRoster(
    [FromRoute] long guildId,
    [FromBody] SaveGuildRosterRequest request)
  {
    var result = await _service.SaveGuildRoster(guildId, request);
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
      Roles = request.Roles
    };

    var result = await _service.SyncGuild(normalized);
    return result.Send();
  }
}
