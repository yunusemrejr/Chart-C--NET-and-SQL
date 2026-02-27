using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using WindowsFormsApp1.Models;

namespace WindowsFormsApp1.Services
{
    /// <summary>
    /// Service for managing chart generation and styling
    /// </summary>
    public class ChartService
    {
        public enum ChartType
        {
            Column,
            Bar,
            Line,
            Pie,
            Area,
            Spline
        }

        public void ConfigureChart(Chart chart, List<Item> items, ChartType chartType, string title = "Price Analysis")
        {
            if (chart == null || items == null || items.Count == 0)
                return;

            chart.Series.Clear();
            chart.Titles.Clear();
            chart.Legends.Clear();
            chart.ChartAreas.Clear();

            // Add chart area
            ChartArea chartArea = new ChartArea("MainArea");
            ConfigureChartArea(chartArea, chartType);
            chart.ChartAreas.Add(chartArea);

            // Add title
            Title chartTitle = new Title(title)
            {
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64)
            };
            chart.Titles.Add(chartTitle);

            // Add legend
            Legend legend = new Legend("MainLegend")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.Transparent
            };
            chart.Legends.Add(legend);

            // Create series based on chart type
            if (chartType == ChartType.Pie)
            {
                CreatePieSeries(chart, items);
            }
            else
            {
                CreateStandardSeries(chart, items, chartType);
            }

            // Apply modern styling
            ApplyModernStyling(chart);
        }

        private void ConfigureChartArea(ChartArea chartArea, ChartType chartType)
        {
            chartArea.BackColor = Color.White;
            chartArea.BorderColor = Color.LightGray;
            chartArea.BorderWidth = 1;
            chartArea.BorderDashStyle = ChartDashStyle.Solid;

            if (chartType != ChartType.Pie)
            {
                // X Axis
                chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
                chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartArea.AxisX.LabelStyle.Font = new Font("Segoe UI", 9);
                chartArea.AxisX.LineColor = Color.DarkGray;
                chartArea.AxisX.TitleFont = new Font("Segoe UI", 10, FontStyle.Bold);

                // Y Axis
                chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
                chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                chartArea.AxisY.LabelStyle.Font = new Font("Segoe UI", 9);
                chartArea.AxisY.LabelStyle.Format = "${0:F2}";
                chartArea.AxisY.LineColor = Color.DarkGray;
                chartArea.AxisY.Title = "Price ($)";
                chartArea.AxisY.TitleFont = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        private void CreateStandardSeries(Chart chart, List<Item> items, ChartType chartType)
        {
            Series series = new Series("Price")
            {
                ChartArea = "MainArea",
                Legend = "MainLegend",
                IsValueShownAsLabel = true,
                LabelFormat = "${0:F2}",
                Font = new Font("Segoe UI", 8)
            };

            // Set chart type
            switch (chartType)
            {
                case ChartType.Column:
                    series.ChartType = SeriesChartType.Column;
                    break;
                case ChartType.Bar:
                    series.ChartType = SeriesChartType.Bar;
                    break;
                case ChartType.Line:
                    series.ChartType = SeriesChartType.Line;
                    series.BorderWidth = 3;
                    series.MarkerStyle = MarkerStyle.Circle;
                    series.MarkerSize = 8;
                    break;
                case ChartType.Area:
                    series.ChartType = SeriesChartType.Area;
                    break;
                case ChartType.Spline:
                    series.ChartType = SeriesChartType.Spline;
                    series.BorderWidth = 3;
                    series.MarkerStyle = MarkerStyle.Circle;
                    series.MarkerSize = 8;
                    break;
            }

            // Add data points with colors
            Color[] colors = GetModernColorPalette();
            int colorIndex = 0;

            foreach (var item in items)
            {
                int pointIndex = series.Points.AddXY(item.Name, item.Price);
                series.Points[pointIndex].Color = colors[colorIndex % colors.Length];
                series.Points[pointIndex].ToolTip = $"{item.Name}\nPrice: ${item.Price:F2}\nCategory: {item.Category}";
                colorIndex++;
            }

            chart.Series.Add(series);
        }

        private void CreatePieSeries(Chart chart, List<Item> items)
        {
            Series series = new Series("Price")
            {
                ChartArea = "MainArea",
                Legend = "MainLegend",
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelFormat = "${0:F2}",
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            series["PieLabelStyle"] = "Outside";
            series["PieLineColor"] = "Black";

            // Add data points with colors
            Color[] colors = GetModernColorPalette();
            int colorIndex = 0;

            foreach (var item in items)
            {
                int pointIndex = series.Points.AddXY(item.Name, item.Price);
                series.Points[pointIndex].Color = colors[colorIndex % colors.Length];
                series.Points[pointIndex].LegendText = item.Name;
                series.Points[pointIndex].Label = $"${item.Price:F2}";
                series.Points[pointIndex].ToolTip = $"{item.Name}\nPrice: ${item.Price:F2}\nPercentage: #PERCENT{P1}";
                colorIndex++;
            }

            chart.Series.Add(series);
        }

        private void ApplyModernStyling(Chart chart)
        {
            chart.BackColor = Color.WhiteSmoke;
            chart.BorderlineColor = Color.DarkGray;
            chart.BorderlineWidth = 2;
            chart.BorderlineDashStyle = ChartDashStyle.Solid;
            chart.AntiAliasing = AntiAliasingStyles.All;
            chart.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
        }

        private Color[] GetModernColorPalette()
        {
            return new Color[]
            {
                Color.FromArgb(52, 152, 219),   // Blue
                Color.FromArgb(46, 204, 113),   // Green
                Color.FromArgb(155, 89, 182),   // Purple
                Color.FromArgb(241, 196, 15),   // Yellow
                Color.FromArgb(231, 76, 60),    // Red
                Color.FromArgb(26, 188, 156),   // Turquoise
                Color.FromArgb(230, 126, 34),   // Orange
                Color.FromArgb(149, 165, 166),  // Gray
                Color.FromArgb(52, 73, 94),     // Dark Blue
                Color.FromArgb(192, 57, 43)     // Dark Red
            };
        }

        public void ConfigureChartByCategory(Chart chart, List<Item> items, ChartType chartType)
        {
            if (chart == null || items == null || items.Count == 0)
                return;

            var groupedItems = items.GroupBy(i => i.Category)
                                   .Select(g => new { Category = g.Key, TotalPrice = g.Sum(i => i.Price), Count = g.Count() })
                                   .ToList();

            chart.Series.Clear();
            chart.Titles.Clear();
            chart.Legends.Clear();
            chart.ChartAreas.Clear();

            ChartArea chartArea = new ChartArea("MainArea");
            ConfigureChartArea(chartArea, chartType);
            chart.ChartAreas.Add(chartArea);

            Title chartTitle = new Title("Price by Category")
            {
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64)
            };
            chart.Titles.Add(chartTitle);

            Legend legend = new Legend("MainLegend")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center,
                Font = new Font("Segoe UI", 9)
            };
            chart.Legends.Add(legend);

            Series series = new Series("Total Price")
            {
                ChartArea = "MainArea",
                Legend = "MainLegend",
                IsValueShownAsLabel = true,
                LabelFormat = "${0:F2}",
                Font = new Font("Segoe UI", 8)
            };

            switch (chartType)
            {
                case ChartType.Column:
                    series.ChartType = SeriesChartType.Column;
                    break;
                case ChartType.Bar:
                    series.ChartType = SeriesChartType.Bar;
                    break;
                case ChartType.Pie:
                    series.ChartType = SeriesChartType.Pie;
                    series["PieLabelStyle"] = "Outside";
                    break;
                default:
                    series.ChartType = SeriesChartType.Column;
                    break;
            }

            Color[] colors = GetModernColorPalette();
            int colorIndex = 0;

            foreach (var group in groupedItems)
            {
                int pointIndex = series.Points.AddXY(group.Category, group.TotalPrice);
                series.Points[pointIndex].Color = colors[colorIndex % colors.Length];
                series.Points[pointIndex].ToolTip = $"{group.Category}\nTotal: ${group.TotalPrice:F2}\nItems: {group.Count}";
                colorIndex++;
            }

            chart.Series.Add(series);
            ApplyModernStyling(chart);
        }
    }
}
