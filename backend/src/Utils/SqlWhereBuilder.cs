using System.Reflection;
using Dapper;

namespace Utils;

public enum SqlJoinType
{
  And,
  Or
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class SqlJoinAttribute : Attribute
{
  public SqlJoinType JoinType { get; }

  public SqlJoinAttribute(SqlJoinType joinType)
  {
    JoinType = joinType;
  }
}

public static class SqlWhereBuilder
{
  public static (string Clause, DynamicParameters Parameters) BuildWhere<T>(
    DynamicParameters parameters,
    T query,
    Dictionary<string, string>? columnMap = null,
    IEnumerable<string>? searchColumns = null)
    where T : BaseUrlQuery
  {
    var andConditions = new List<string>();
    var orConditions = new List<string>();

    if (query is null)
    {
      return ("", parameters);
    }

    var props = typeof(T)
      .GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(prop => prop.DeclaringType != typeof(BaseUrlQuery));

    foreach (var prop in props)
    {
      var value = prop.GetValue(query);

      if (value is null)
      {
        continue;
      }

      var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

      if (!IsSimpleType(propType))
      {
        continue;
      }

      if (prop.Name == "Search")
      {
        if (value is string search &&
            !string.IsNullOrWhiteSpace(search) &&
            searchColumns is not null &&
            searchColumns.Any())
        {
          var searchConditions = searchColumns.Select(column => $"{column} ILIKE @Search");
          andConditions.Add("(" + string.Join(" OR ", searchConditions) + ")");
          parameters.Add("Search", $"%{search.Trim()}%");
        }

        continue;
      }

      var columnName = columnMap != null && columnMap.TryGetValue(prop.Name, out var mapped)
        ? mapped
        : ToSnakeCase(prop.Name);

      var parameterName = prop.Name;

      if (propType.IsEnum)
      {
        value = value.ToString();
      }

      var condition = $"{columnName} = @{parameterName}";

      var joinType = prop.GetCustomAttribute<SqlJoinAttribute>()?.JoinType ?? SqlJoinType.And;

      if (joinType == SqlJoinType.Or)
      {
        orConditions.Add(condition);
      }
      else
      {
        andConditions.Add(condition);
      }

      parameters.Add(parameterName, value);
    }

    var finalConditions = new List<string>();
    finalConditions.AddRange(andConditions);

    if (orConditions.Count > 0)
    {
      finalConditions.Add("(" + string.Join(" OR ", orConditions) + ")");
    }

    return (
      finalConditions.Count > 0 ? " AND " + string.Join(" AND ", finalConditions) : "",
      parameters);
  }

  private static bool IsSimpleType(Type type)
  {
    return type.IsPrimitive
      || type.IsEnum
      || type == typeof(string)
      || type == typeof(decimal)
      || type == typeof(DateTime)
      || type == typeof(DateTimeOffset)
      || type == typeof(Guid);
  }

  private static string ToSnakeCase(string name)
  {
    return string.Concat(name.Select((c, i) =>
      i > 0 && char.IsUpper(c)
        ? "_" + char.ToLowerInvariant(c)
        : char.ToLowerInvariant(c).ToString()));
  }
}
