using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace WindowsFormsApp1.Helpers
{
    /// <summary>
    /// Helper class for managing application configuration
    /// </summary>
    public static class ConfigHelper
    {
        private static readonly byte[] entropy = Encoding.UTF8.GetBytes("ChartApp2026SecureKey");

        public static string GetConnectionString()
        {
            try
            {
                string encryptedConnection = ConfigurationManager.AppSettings["EncryptedConnectionString"];
                if (!string.IsNullOrEmpty(encryptedConnection))
                {
                    return DecryptString(encryptedConnection);
                }

                // Fallback to default connection string
                return ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString 
                    ?? "host=localhost;user=root;password=;port=3306;database=demo1";
            }
            catch
            {
                return "host=localhost;user=root;password=;port=3306;database=demo1";
            }
        }

        public static void SaveConnectionString(string connectionString)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                string encrypted = EncryptString(connectionString);
                
                if (config.AppSettings.Settings["EncryptedConnectionString"] == null)
                {
                    config.AppSettings.Settings.Add("EncryptedConnectionString", encrypted);
                }
                else
                {
                    config.AppSettings.Settings["EncryptedConnectionString"].Value = encrypted;
                }
                
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save connection string: " + ex.Message);
            }
        }

        private static string EncryptString(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedBytes);
        }

        private static string DecryptString(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }

        public static string GetSetting(string key, string defaultValue = "")
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

        public static void SaveSetting(string key, string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                
                if (config.AppSettings.Settings[key] == null)
                {
                    config.AppSettings.Settings.Add(key, value);
                }
                else
                {
                    config.AppSettings.Settings[key].Value = value;
                }
                
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save setting {key}: " + ex.Message);
            }
        }
    }
}
