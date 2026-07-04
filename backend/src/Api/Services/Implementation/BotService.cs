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
          faculty => faculty.Name),

        "ls_projects" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Projects and Research",
          "No project records found.",
          (conn, query) => _inelicomRepository.GetProjects(conn, query),
          project => $"{project.Name} - {project.Description}"),

        "ls_student_orgs" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Student Organizations",
          "No student organization records found.",
          (conn, query) => _inelicomRepository.GetOrganizations(conn, query),
          organization => $"{organization.Name} - {organization.Description}"),

        "salon" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Rooms",
          "No room records found.",
          (conn, query) => _inelicomRepository.GetRooms(conn, query),
          room => $"{room.Code} - {room.Name}"),

        "lab" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Labs",
          "No room records found.",
          (conn, query) => _inelicomRepository.GetRooms(conn, query),
          room => $"{room.Code} - {room.Name}"),

        "contact-department" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Departments",
          "No department records found.",
          (conn, query) => _inelicomRepository.GetDepartments(conn, query),
          department => department.Name),

        "contact-dcsp" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Contact Information",
          "No matching contact records found.",
          (conn, query) => _inelicomRepository.GetContacts(conn, query),
          contact => $"{contact.Name} - {contact.Email} - {contact.Phone} - {contact.Website}",
          "DCSP"),

        "contact-decanato-estudiantes" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Contact Information",
          "No matching contact records found.",
          (conn, query) => _inelicomRepository.GetContacts(conn, query),
          contact => $"{contact.Name} - {contact.Email} - {contact.Phone} - {contact.Website}",
          "Decanato"),

        "contact-guardia-univ" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Contact Information",
          "No matching contact records found.",
          (conn, query) => _inelicomRepository.GetContacts(conn, query),
          contact => $"{contact.Name} - {contact.Email} - {contact.Phone} - {contact.Website}",
          "Guardia"),

        "contact-asesoria-academica" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Contact Information",
          "No matching contact records found.",
          (conn, query) => _inelicomRepository.GetContacts(conn, query),
          contact => $"{contact.Name} - {contact.Email} - {contact.Phone} - {contact.Website}",
          "Asesoria"),

        "contact-asistencia-economica" => await GetInelicomCommandResponse(
          connection,
          commandName,
          "Contact Information",
          "No matching contact records found.",
          (conn, query) => _inelicomRepository.GetContacts(conn, query),
          contact => $"{contact.Name} - {contact.Email} - {contact.Phone} - {contact.Website}",
          "Asistencia"),

        "curriculo" => await GetCurriculumCommandResponse(connection, commandName),

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

  private static readonly string[] CurriculumPrograms = ["INEL", "ICOM"];

  private async Task<Result<BotCommandResponse, AppError>> GetCurriculumCommandResponse(IDbConnection connection, string commandName)
  {
    var result = await _inelicomRepository.GetCurriculums(connection);
    return result.Transform(page =>
    {
      var byProgram = page.Items.ToDictionary(item => item.Program, item => item);

      var lines = CurriculumPrograms.Select(program => byProgram.TryGetValue(program, out var curriculum)
        ? $"- **{program}**: uploaded {curriculum.UploadedAt:yyyy-MM-dd}"
        : $"- **{program}**: not uploaded yet");

      var buttons = CurriculumPrograms
        .Where(byProgram.ContainsKey)
        .Select(program => new BotButtonDefinition
        {
          Id = $"curriculum-{program.ToLowerInvariant()}",
          Label = program,
          Style = "link",
          Url = $"{PublicBaseUrl}/api/v1/inelicom/curriculums/{program}/file"
        })
        .ToArray();

      return new BotCommandResponse
      {
        CommandName = commandName,
        Title = "Curriculums",
        Description = string.Join("\n", lines),
        Ephemeral = true,
        Buttons = buttons
      };
    });
  }

  private static readonly string PublicBaseUrl = ResolvePublicBaseUrl();

  private static string ResolvePublicBaseUrl()
  {
    var value = Environment.GetEnvironmentVariable("BACKEND_PUBLIC_URL");
    return string.IsNullOrWhiteSpace(value) ? "http://localhost:8080" : value.TrimEnd('/');
  }

}
