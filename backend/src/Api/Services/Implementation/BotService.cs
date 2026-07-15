using System.Data;
using Friday.Backend.Api.Domain;
using Friday.Backend.Api.Repositories;
using Npgsql;
using Utils;

namespace Friday.Backend.Api.Services;

public sealed partial class BotService : IBotService
{
  private readonly IDbConnectionFactory _dbFactory;
  private readonly IBotRepository _repository;
  private readonly IInelicomRepository _inelicomRepository;

  public BotService(
    IDbConnectionFactory dbFactory,
    IBotRepository repository,
    IInelicomRepository inelicomRepository)
  {
    _dbFactory = dbFactory;
    _repository = repository;
    _inelicomRepository = inelicomRepository;
  }

  public async Task<Result<IReadOnlyCollection<GuildSummary>, AppError>> GetEnabledGuilds()
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetEnabledGuilds(connection);
  }

  public async Task<Result<GuildProfile, AppError>> GetGuildProfile(long guildId)
  {
    using var connection = _dbFactory.Create();
    return await _repository.GetGuildProfile(connection, guildId);
  }

  public Task<Result<IReadOnlyCollection<string>, AppError>> GetProfanities()
  {
    IReadOnlyCollection<string> words = ["cabron", "carajo", "cojones", "puta", "puto"];
    return Task.FromResult(Result<IReadOnlyCollection<string>, AppError>.Ok(words));
  }

  public async Task<Result<BotCommandResponse, AppError>> GetCommandResponse(long guildId, string commandName)
  {
    _ = guildId;
    using var connection = _dbFactory.Create();

    try
    {
      return commandName switch
      {
        "faculty" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Faculty",
          "No faculty records found.",
          (conn, query) => _inelicomRepository.GetFaculties(conn, query),
          faculty => faculty.Name ?? "N/A"),

        "ls_projects" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Projects and Research",
          "No project records found.",
          (conn, query) => _inelicomRepository.GetProjects(conn, query),
          project => $"{project.Name ?? "N/A"} - {project.Description ?? "N/A"}"),

        "ls_organizations" or "ls_student_orgs" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Student Organizations",
          "No student organization records found.",
          (conn, query) => _inelicomRepository.GetOrganizations(conn, query),
          organization => $"{organization.Name ?? "N/A"} - {organization.Description ?? "N/A"}"),

        "salon" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Buildings",
          "No building records found.",
          (conn, query) => _inelicomRepository.GetBuildings(conn, query),
          building => $"{building.Code ?? building.Name ?? "N/A"} - {building.Name ?? "N/A"}"),


        _ => Result<BotCommandResponse, AppError>.Ok(new BotCommandResponse
        {
          CommandName = commandName,
          Title = $"/{commandName}",
          Description = $"/{commandName} is wired. Add backend data for a richer response.",
          Ephemeral = true
        })
      };
    }
    catch (Exception ex)
    {
      return Result<BotCommandResponse, AppError>.Fail(AppError.BadRequest(ex.Message));
    }
  }

  public async Task<Result<MemberVerification, AppError>> VerifyMember(long guildId, VerifyMemberRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.VerifyMember(connection, guildId, request);
  }

  public async Task<Result<MemberXp, AppError>> AddXp(long guildId, XpRequest request)
  {
    using var connection = _dbFactory.Create();
    return await _repository.AddXp(connection, guildId, request);
  }

  public async Task<Result<BotSyncResult, AppError>> SyncGuild(BotSyncRequest request)
  {
    var connection = (NpgsqlConnection)_dbFactory.Create();
    return await TransactionResult<BotSyncResult, AppError>
      .Begin(connection, ex => AppError.BadRequest(ex.Message))
      .AndThen((conn, tran) => _repository.SyncGuild(conn, tran, request))
      .Complete();
  }


  private static async Task<Result<BotCommandResponse, AppError>> GetInelicomCommandResponse<T>(
    IDbConnection connection,
    string commandName,
    string title,
    string emptyMessage,
    Func<IDbConnection, InelicomQuery, Task<Result<Paged<T>, AppError>>> getItems,
    Func<T, string> formatItem,
    string? search = null)
  {
    var query = new InelicomQuery
    {
      Limit = 25,
      PageIndex = 0,
      IncludeTotal = false,
      Search = search
    };

    var result = await getItems(connection, query);
    return result.Transform(page =>
    {
      var lines = page.Items.Select(item => $"- {formatItem(item)}").ToArray();
      var description = lines.Length == 0 ? emptyMessage : string.Join("\n", lines);

      return new BotCommandResponse
      {
        CommandName = commandName,
        Title = title,
        Description = description,
        Ephemeral = true
      };
    });
  }

}
