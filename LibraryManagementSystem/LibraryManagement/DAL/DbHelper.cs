using System.Configuration;
using Microsoft.Data.SqlClient;

namespace LibraryManagement.DAL;

public static class DbHelper
{
    public static string ConnectionString =>
        ConfigurationManager.ConnectionStrings["LibraryDB"].ConnectionString;

    public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);
}
