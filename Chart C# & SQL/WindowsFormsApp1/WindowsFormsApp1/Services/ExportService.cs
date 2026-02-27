using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    /// <summary>
    /// Service for exporting data to various formats
    /// </summary>
    public class ExportService
    {
        public bool ExportToCSV(List<Item> items, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // Write header
                    writer.WriteLine("ID,Name,Price,Category,Description,Created At,Updated At");

                    // Write data
                    foreach (var item in items)
                    {
                        writer.WriteLine($"{item.ID}," +
                                       $"\"{EscapeCSV(item.Name)}\"," +
                                       $"{item.Price:F2}," +
                                       $"\"{EscapeCSV(item.Category)}\"," +
                                       $"\"{EscapeCSV(item.Description)}\"," +
                                       $"{item.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                                       $"{item.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to export to CSV: " + ex.Message, ex);
            }
        }

        public bool ExportToHTML(List<Item> items, string filePath, Dictionary<string, decimal> statistics = null)
        {
            try
            {
                StringBuilder html = new StringBuilder();
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html>");
                html.AppendLine("<head>");
                html.AppendLine("<meta charset='UTF-8'>");
                html.AppendLine("<title>Items Report</title>");
                html.AppendLine("<style>");
                html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }");
                html.AppendLine("h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }");
                html.AppendLine("h2 { color: #34495e; margin-top: 30px; }");
                html.AppendLine(".stats { background: white; padding: 20px; border-radius: 8px; margin-bottom: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
                html.AppendLine(".stat-item { display: inline-block; margin: 10px 20px; }");
                html.AppendLine(".stat-label { font-weight: bold; color: #7f8c8d; }");
                html.AppendLine(".stat-value { font-size: 24px; color: #2c3e50; }");
                html.AppendLine("table { width: 100%; border-collapse: collapse; background: white; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
                html.AppendLine("th { background-color: #3498db; color: white; padding: 12px; text-align: left; }");
                html.AppendLine("td { padding: 10px; border-bottom: 1px solid #ecf0f1; }");
                html.AppendLine("tr:hover { background-color: #f8f9fa; }");
                html.AppendLine(".price { color: #27ae60; font-weight: bold; }");
                html.AppendLine(".footer { margin-top: 30px; text-align: center; color: #95a5a6; font-size: 12px; }");
                html.AppendLine("</style>");
                html.AppendLine("</head>");
                html.AppendLine("<body>");
                html.AppendLine("<h1>Items Price Report</h1>");
                html.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");

                // Statistics section
                if (statistics != null && statistics.Count > 0)
                {
                    html.AppendLine("<div class='stats'>");
                    html.AppendLine("<h2>Statistics</h2>");
                    foreach (var stat in statistics)
                    {
                        html.AppendLine("<div class='stat-item'>");
                        html.AppendLine($"<div class='stat-label'>{stat.Key}</div>");
                        html.AppendLine($"<div class='stat-value'>{(stat.Key == "Total" ? stat.Value.ToString("F0") : "$" + stat.Value.ToString("F2"))}</div>");
                        html.AppendLine("</div>");
                    }
                    html.AppendLine("</div>");
                }

                // Items table
                html.AppendLine("<h2>Items List</h2>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>ID</th><th>Name</th><th>Price</th><th>Category</th><th>Description</th><th>Created</th></tr>");

                foreach (var item in items)
                {
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{item.ID}</td>");
                    html.AppendLine($"<td>{EscapeHTML(item.Name)}</td>");
                    html.AppendLine($"<td class='price'>${item.Price:F2}</td>");
                    html.AppendLine($"<td>{EscapeHTML(item.Category)}</td>");
                    html.AppendLine($"<td>{EscapeHTML(item.Description)}</td>");
                    html.AppendLine($"<td>{item.CreatedAt:yyyy-MM-dd}</td>");
                    html.AppendLine("</tr>");
                }

                html.AppendLine("</table>");
                html.AppendLine("<div class='footer'>Chart Application - Professional Data Management System</div>");
                html.AppendLine("</body>");
                html.AppendLine("</html>");

                File.WriteAllText(filePath, html.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to export to HTML: " + ex.Message, ex);
            }
        }

        public bool ExportToJSON(List<Item> items, string filePath)
        {
            try
            {
                StringBuilder json = new StringBuilder();
                json.AppendLine("{");
                json.AppendLine($"  \"exportDate\": \"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\",");
                json.AppendLine($"  \"totalItems\": {items.Count},");
                json.AppendLine("  \"items\": [");

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    json.AppendLine("    {");
                    json.AppendLine($"      \"id\": {item.ID},");
                    json.AppendLine($"      \"name\": \"{EscapeJSON(item.Name)}\",");
                    json.AppendLine($"      \"price\": {item.Price:F2},");
                    json.AppendLine($"      \"category\": \"{EscapeJSON(item.Category)}\",");
                    json.AppendLine($"      \"description\": \"{EscapeJSON(item.Description)}\",");
                    json.AppendLine($"      \"createdAt\": \"{item.CreatedAt:yyyy-MM-dd HH:mm:ss}\",");
                    json.AppendLine($"      \"updatedAt\": \"{item.UpdatedAt:yyyy-MM-dd HH:mm:ss}\"");
                    json.AppendLine(i < items.Count - 1 ? "    }," : "    }");
                }

                json.AppendLine("  ]");
                json.AppendLine("}");

                File.WriteAllText(filePath, json.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to export to JSON: " + ex.Message, ex);
            }
        }

        private string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\"", "\"\"");
        }

        private string EscapeHTML(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#39;");
        }

        private string EscapeJSON(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }
    }
}
