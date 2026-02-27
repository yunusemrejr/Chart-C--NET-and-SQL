using System;

namespace WindowsFormsApp1.Models
{
    /// <summary>
    /// Represents an item with pricing information
    /// </summary>
    public class Item
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Item()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            Category = "General";
            Description = string.Empty;
        }

        public override string ToString()
        {
            return $"{Name} - ${Price:F2}";
        }
    }
}
