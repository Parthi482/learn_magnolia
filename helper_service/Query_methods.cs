using Dapper;
using Magnolia_cares.database;

namespace Magnolia_cares.helper_service
{
    public static class QueryMethods
    {


        // FindOne 
        public static async Task<dynamic> ExecuteQueryFirstAsync(string query, object? parameters = null)
        {
            try
            {
                using (var connection = SqlConnections.GetConnection())
                {
                    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(query, parameters);
                    SqlConnections.Dispose();
                    return result;
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }


        // FindAll 
        public static async Task<dynamic> ExecuteQueryAsync(string query, object? parameters = null)
        {
            try
            {
                using (var connection = SqlConnections.GetConnection())
                {
                    var result = await connection.QueryAsync<dynamic>(query, parameters);
                    SqlConnections.Dispose();
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
    }
}
