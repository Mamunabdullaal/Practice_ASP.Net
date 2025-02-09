using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HRDashBoard.Controllers
{
    public class HomeController : Controller
    {
        private static string uploadedFilePath = "";
        private static string currentSheetName = "";

        // GET: Display the sheet data
        public IActionResult Index()
        {
            if (!string.IsNullOrEmpty(uploadedFilePath))
            {
                var sheetData = GetSheetCounts(uploadedFilePath);
                return View(sheetData);
            }
            return View(new Dictionary<string, int>());
        }

        // POST: Upload Excel file
        [HttpPost]
        public IActionResult UploadExcel(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", file.FileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                uploadedFilePath = path;
            }
            return RedirectToAction("Index");
        }

        // GET: View the data of a specific sheet
        public IActionResult ViewSheetData(string sheetName)
        {
            if (!string.IsNullOrEmpty(uploadedFilePath))
            {
                var sheetData = GetSheetData(uploadedFilePath, sheetName);
                currentSheetName = sheetName;
                ViewData["SheetName"] = sheetName;
                return View(sheetData);
            }
            return RedirectToAction("Index");
        }

        // POST: Save data back to Excel
        [HttpPost]
        public IActionResult SaveData(List<Dictionary<string, object>> updatedData)
        {
            string sheetName = currentSheetName;
            var filePath = uploadedFilePath;

            // Save data to the Excel file
            SaveDataToExcel(filePath, sheetName, updatedData);

            // Reload the updated sheet data
            return RedirectToAction("ViewSheetData", new { sheetName });
        }

        // GET: Delete a specific row (called via ajax from frontend)
        [HttpPost]
        public IActionResult DeleteRow(string sheetName, string rowId)
        {
            string filePath = uploadedFilePath;

            // Delete the row from the Excel file
            DeleteRowFromExcel(filePath, sheetName, rowId);

            // Reload the sheet data
            return RedirectToAction("ViewSheetData", new { sheetName });
        }

        // GET: Add new row (dynamically via frontend, not yet in Excel)
        [HttpPost]
        public IActionResult AddNewRow(List<string> newRowData)
        {
            string filePath = uploadedFilePath;

            // Add the new row to the Excel sheet
            AddRowToExcel(filePath, currentSheetName, newRowData);

            // Reload the sheet data
            return RedirectToAction("ViewSheetData", new { sheetName = currentSheetName });
        }

        // Helper method to get sheet data (as rows and columns)
        private List<Dictionary<string, object>> GetSheetData(string filePath, string sheetName)
        {
            var data = new List<Dictionary<string, object>>();

            // ✅ Set the license context to avoid LicenseException
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = package.Workbook.Worksheets[sheetName];

                // Get the header row
                var header = new List<string>();
                for (int col = 1; col <= sheet.Dimension.Columns; col++)
                {
                    header.Add(sheet.Cells[1, col].Text); // Assuming the first row is the header
                }

                // Get the data rows (skip header row)
                for (int row = 2; row <= sheet.Dimension.Rows; row++)
                {
                    var rowData = new Dictionary<string, object>();
                    for (int col = 1; col <= sheet.Dimension.Columns; col++)
                    {
                        rowData[header[col - 1]] = sheet.Cells[row, col].Text;
                    }
                    data.Add(rowData);
                }
            }

            return data;
        }

        // Helper method to get sheet counts (to display on the index page)
        private Dictionary<string, int> GetSheetCounts(string filePath)
        {
            var sheetCounts = new Dictionary<string, int>();

            // ✅ Set the license context to avoid LicenseException
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                foreach (var sheet in package.Workbook.Worksheets)
                {
                    sheetCounts[sheet.Name] = sheet.Dimension.Rows - 1; // Exclude title row
                }
            }

            return sheetCounts;
        }

        // Helper method to save data back to Excel
        private void SaveDataToExcel(string filePath, string sheetName, List<Dictionary<string, object>> updatedData)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = package.Workbook.Worksheets[sheetName];

                // Save data (for simplicity, we are not updating the header row)
                for (int i = 0; i < updatedData.Count; i++)
                {
                    var row = updatedData[i];
                    for (int j = 0; j < row.Count; j++)
                    {
                        sheet.Cells[i + 2, j + 1].Value = row.Values.ElementAt(j); // Start from row 2
                    }
                }

                package.Save();
            }
        }

        // Helper method to delete a specific row
        private void DeleteRowFromExcel(string filePath, string sheetName, string rowId)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = package.Workbook.Worksheets[sheetName];

                int rowToDelete = -1;
                for (int row = 2; row <= sheet.Dimension.Rows; row++)
                {
                    var currentRowId = sheet.Cells[row, 1].Text; // Assuming the first column contains the ID
                    if (currentRowId == rowId)
                    {
                        rowToDelete = row;
                        break; // Exit the loop once we find the row to delete
                    }
                }

                if (rowToDelete != -1)
                {
                    sheet.DeleteRow(rowToDelete);

                    // Re-sequence the SL number after deletion
                    for (int row = 2; row <= sheet.Dimension.Rows; row++) // Start from 2 to skip header
                    {
                        sheet.Cells[row, 1].Value = row - 1; // Update SL number (assumed in the first column)
                    }

                    package.Save();
                }
            }
        }

        // Helper method to add a new row
        private void AddRowToExcel(string filePath, string sheetName, List<string> newRowData)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = package.Workbook.Worksheets[sheetName];

                // Find the next available row
                int nextRow = sheet.Dimension.Rows + 1;

                // Add the data to the next available row
                for (int i = 0; i < newRowData.Count; i++)
                {
                    sheet.Cells[nextRow, i + 1].Value = newRowData[i];
                }

                package.Save();
            }
        }
    }
}
