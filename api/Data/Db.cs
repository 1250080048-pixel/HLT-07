using Microsoft.Data.SqlClient;

namespace TuVan.Api.Data;

/// <summary>Tạo kết nối SQL Server cho mỗi thao tác.</summary>
public sealed class Db(IConfiguration cfg)
{
    private readonly string _cs = cfg.GetConnectionString("SqlServer")
        ?? throw new InvalidOperationException("Thiếu ConnectionStrings:SqlServer");

    public async Task<SqlConnection> OpenAsync(CancellationToken ct = default)
    {
        var c = new SqlConnection(_cs);
        await c.OpenAsync(ct);
        return c;
    }
}

/// <summary>Lỗi vi phạm quy tắc nghiệp vụ (BR02, BR04, ...). Trả về HTTP 422.</summary>
public sealed class NghiepVuException(string message) : Exception(message);
