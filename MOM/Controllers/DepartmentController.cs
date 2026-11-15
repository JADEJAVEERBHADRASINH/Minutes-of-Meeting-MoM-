using Microsoft.AspNetCore.Mvc;
using MOM.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using ClosedXML.Excel;
using MOM.Filters;

namespace MOM.Controllers
{
    [CheckAccess]
    public class DepartmentController : Controller
    {
        private readonly IConfiguration _configuration;

        public DepartmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

     
        [HttpGet]
        public IActionResult DepartmentList()
        {
            List<DepartmentModel> list = new List<DepartmentModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("PR_MOM_Department_SelectAll", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SearchText", DBNull.Value);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        list.Add(new DepartmentModel
                        {
                            DepartmentID = Convert.ToInt32(reader["DepartmentID"]),
                            DepartmentName = reader["DepartmentName"].ToString(),
                            Created = reader["Created"] == DBNull.Value ? null : Convert.ToDateTime(reader["Created"]),
                            Modified = reader["Modified"] == DBNull.Value ? null : Convert.ToDateTime(reader["Modified"])
                        });
                    }
                }
            }

            return View(list);
        }

        [HttpPost]
        public IActionResult DepartmentList(IFormCollection formData)
        {
            string searchText = formData["SearchText"].ToString();

            if (string.IsNullOrWhiteSpace(searchText))
                searchText = null;

            ViewBag.SearchText = searchText;

            List<DepartmentModel> list = new List<DepartmentModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("PR_MOM_Department_SelectAll", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (searchText != null)
                        cmd.Parameters.AddWithValue("@SearchText", searchText);
                    else
                        cmd.Parameters.AddWithValue("@SearchText", DBNull.Value);

                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        list.Add(new DepartmentModel
                        {
                            DepartmentID = Convert.ToInt32(reader["DepartmentID"]),
                            DepartmentName = reader["DepartmentName"].ToString(),
                            Created = reader["Created"] == DBNull.Value ? null : Convert.ToDateTime(reader["Created"]),
                            Modified = reader["Modified"] == DBNull.Value ? null : Convert.ToDateTime(reader["Modified"])
                        });
                    }
                }
            }

            return View(list);
        }

        
        public IActionResult DepartmentAddEdit(int? id)
        {
            if (id != null && id > 0)
            {
                DepartmentModel model = new DepartmentModel();

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MOM_Department_SelectByPK", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = id;

                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            model.DepartmentID = Convert.ToInt32(reader["DepartmentID"]);
                            model.DepartmentName = reader["DepartmentName"].ToString();
                        }
                    }
                }

                return View(model);
            }

            return View(new DepartmentModel());
        }

     
        public IActionResult Save(DepartmentModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (model.DepartmentID == 0)
                        {
                            cmd.CommandText = "PR_MOM_Department_Insert";
                        }
                        else
                        {
                            cmd.CommandText = "PR_MOM_Department_UpdateByPK";
                            cmd.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = model.DepartmentID;
                        }

                        cmd.Parameters.Add("@DepartmentName", SqlDbType.NVarChar).Value = model.DepartmentName;

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Department saved successfully.";
                return RedirectToAction("DepartmentList");
            }

            return View("DepartmentAddEdit", model);
        }

        
        public IActionResult Delete(int DepartmentID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MOM_Department_DeleteByPK", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DepartmentID", DepartmentID);

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Department deleted successfully.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Cannot delete this department because staff members are assigned to it.";
            }

            return RedirectToAction("DepartmentList");

        }

        public IActionResult ExportToExcel()
        {
            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MOM_Department_SelectAll", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Same as staff → no search
                        cmd.Parameters.AddWithValue("@SearchText", DBNull.Value);

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            dt.Load(dr);
                        }
                    }
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Departments");

                    // Header row
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = dt.Columns[i].ColumnName;
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    // Data rows 
                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        for (int col = 0; col < dt.Columns.Count; col++)
                        {
                            worksheet.Cell(row + 2, col + 1).Value = dt.Rows[row][col]?.ToString();
                        }
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();

                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "DepartmentList.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error exporting data: " + ex.Message;
                return RedirectToAction("DepartmentList");
            }
        }
    }
}