using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.DataAccess
{
    /// <summary>
    /// Repository for managing Item data access operations
    /// </summary>
    public class ItemRepository : IDisposable
    {
        private readonly string connectionString;
        private MySqlConnection connection;

        public ItemRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private async Task<MySqlConnection> GetConnectionAsync()
        {
            if (connection == null || connection.State != ConnectionState.Open)
            {
                connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();
            }
            return connection;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Item>> GetAllItemsAsync()
        {
            var items = new List<Item>();
            
            try
            {
                var conn = await GetConnectionAsync();
                string query = "SELECT ID, isim, price, category, description, created_at, updated_at FROM table1";
                
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        items.Add(new Item
                        {
                            ID = reader.GetInt32("ID"),
                            Name = reader.GetString("isim"),
                            Price = reader.GetDecimal("price"),
                            Category = reader.IsDBNull(reader.GetOrdinal("category")) ? "General" : reader.GetString("category"),
                            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                            CreatedAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ? DateTime.Now : reader.GetDateTime("created_at"),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? DateTime.Now : reader.GetDateTime("updated_at")
                        });
                    }
                }
            }
            catch (MySqlException ex)
            {
                // If columns don't exist, try legacy schema
                if (ex.Number == 1054) // Unknown column error
                {
                    return await GetAllItemsLegacyAsync();
                }
                throw new Exception("Database error: " + ex.Message, ex);
            }
            
            return items;
        }

        private async Task<List<Item>> GetAllItemsLegacyAsync()
        {
            var items = new List<Item>();
            var conn = await GetConnectionAsync();
            string query = "SELECT ID, isim, price FROM table1";
            
            using (var cmd = new MySqlCommand(query, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    items.Add(new Item
                    {
                        ID = reader.GetInt32("ID"),
                        Name = reader.GetString("isim"),
                        Price = Convert.ToDecimal(reader.GetFloat("price"))
                    });
                }
            }
            
            return items;
        }

        public async Task<Item> GetItemByIdAsync(int id)
        {
            try
            {
                var conn = await GetConnectionAsync();
                string query = "SELECT ID, isim, price, category, description, created_at, updated_at FROM table1 WHERE ID = @id";
                
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Item
                            {
                                ID = reader.GetInt32("ID"),
                                Name = reader.GetString("isim"),
                                Price = reader.GetDecimal("price"),
                                Category = reader.IsDBNull(reader.GetOrdinal("category")) ? "General" : reader.GetString("category"),
                                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                                CreatedAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ? DateTime.Now : reader.GetDateTime("created_at"),
                                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? DateTime.Now : reader.GetDateTime("updated_at")
                            };
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1054)
                {
                    return await GetItemByIdLegacyAsync(id);
                }
                throw new Exception("Database error: " + ex.Message, ex);
            }
            
            return null;
        }

        private async Task<Item> GetItemByIdLegacyAsync(int id)
        {
            var conn = await GetConnectionAsync();
            string query = "SELECT ID, isim, price FROM table1 WHERE ID = @id";
            
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Item
                        {
                            ID = reader.GetInt32("ID"),
                            Name = reader.GetString("isim"),
                            Price = Convert.ToDecimal(reader.GetFloat("price"))
                        };
                    }
                }
            }
            
            return null;
        }

        public async Task<int> AddItemAsync(Item item)
        {
            try
            {
                var conn = await GetConnectionAsync();
                string query = @"INSERT INTO table1 (isim, price, category, description, created_at, updated_at) 
                               VALUES (@name, @price, @category, @description, @created, @updated);
                               SELECT LAST_INSERT_ID();";
                
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", item.Name);
                    cmd.Parameters.AddWithValue("@price", item.Price);
                    cmd.Parameters.AddWithValue("@category", item.Category);
                    cmd.Parameters.AddWithValue("@description", item.Description);
                    cmd.Parameters.AddWithValue("@created", item.CreatedAt);
                    cmd.Parameters.AddWithValue("@updated", item.UpdatedAt);
                    
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1054)
                {
                    return await AddItemLegacyAsync(item);
                }
                throw new Exception("Failed to add item: " + ex.Message, ex);
            }
        }

        private async Task<int> AddItemLegacyAsync(Item item)
        {
            var conn = await GetConnectionAsync();
            
            // Get next ID
            string getMaxIdQuery = "SELECT IFNULL(MAX(ID), 0) + 1 FROM table1";
            int nextId;
            using (var cmd = new MySqlCommand(getMaxIdQuery, conn))
            {
                nextId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            
            string query = "INSERT INTO table1 (ID, isim, price) VALUES (@id, @name, @price)";
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", nextId);
                cmd.Parameters.AddWithValue("@name", item.Name);
                cmd.Parameters.AddWithValue("@price", item.Price);
                
                await cmd.ExecuteNonQueryAsync();
                return nextId;
            }
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            try
            {
                var conn = await GetConnectionAsync();
                item.UpdatedAt = DateTime.Now;
                
                string query = @"UPDATE table1 
                               SET isim = @name, price = @price, category = @category, 
                                   description = @description, updated_at = @updated 
                               WHERE ID = @id";
                
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", item.ID);
                    cmd.Parameters.AddWithValue("@name", item.Name);
                    cmd.Parameters.AddWithValue("@price", item.Price);
                    cmd.Parameters.AddWithValue("@category", item.Category);
                    cmd.Parameters.AddWithValue("@description", item.Description);
                    cmd.Parameters.AddWithValue("@updated", item.UpdatedAt);
                    
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1054)
                {
                    return await UpdateItemLegacyAsync(item);
                }
                throw new Exception("Failed to update item: " + ex.Message, ex);
            }
        }

        private async Task<bool> UpdateItemLegacyAsync(Item item)
        {
            var conn = await GetConnectionAsync();
            string query = "UPDATE table1 SET isim = @name, price = @price WHERE ID = @id";
            
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", item.ID);
                cmd.Parameters.AddWithValue("@name", item.Name);
                cmd.Parameters.AddWithValue("@price", item.Price);
                
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            try
            {
                var conn = await GetConnectionAsync();
                string query = "DELETE FROM table1 WHERE ID = @id";
                
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete item: " + ex.Message, ex);
            }
        }

        public async Task<Dictionary<string, decimal>> GetStatisticsAsync()
        {
            var stats = new Dictionary<string, decimal>();
            
            try
            {
                var conn = await GetConnectionAsync();
                string query = @"SELECT 
                                COUNT(*) as Total,
                                IFNULL(SUM(price), 0) as Sum,
                                IFNULL(AVG(price), 0) as Average,
                                IFNULL(MIN(price), 0) as Minimum,
                                IFNULL(MAX(price), 0) as Maximum
                                FROM table1";
                
                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        stats["Total"] = reader.GetInt32("Total");
                        stats["Sum"] = Convert.ToDecimal(reader.GetDouble("Sum"));
                        stats["Average"] = Convert.ToDecimal(reader.GetDouble("Average"));
                        stats["Minimum"] = Convert.ToDecimal(reader.GetDouble("Minimum"));
                        stats["Maximum"] = Convert.ToDecimal(reader.GetDouble("Maximum"));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get statistics: " + ex.Message, ex);
            }
            
            return stats;
        }

        public void Dispose()
        {
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Dispose();
                connection = null;
            }
        }
    }
}
