using Microsoft.AspNetCore.Mvc;
using MOM.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using ClosedXML.Excel;
using Microsoft.Identity.Client;

namespace MOM.Controllers
{
    public class StaffController : Controller
    {
        private readonly IConfiguration _configuration;

        public StaffController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // get //

        [HttpGet]
        public IActionResult StaffList()
        {
            List<StaffModel> stafflist = new List<StaffModel>();

            using (SqlConnection sqlConnection =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand sqlCommand = new SqlCommand("PR_MOM_Staff_SelectAll", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@SearchText", DBNull.Value);

                sqlConnection.Open();

                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    stafflist.Add(new StaffModel
                    {
                        StaffId = Convert.ToInt32(reader["StaffID"]),
                        StaffName = reader["StaffName"].ToString(),
                        MobileNo = reader["MobileNo"].ToString(),
                        EmailAddress = reader["EmailAddress"].ToString(),
                        Remarks = reader["Remarks"].ToString(),
                        Created = Convert.ToDateTime(reader["Created"]),
                        DepartmentName = reader["DepartmentName"].ToString()
                    });
                }
            }

            return View(stafflist);
        }

        // post //

        [HttpPost]
        public IActionResult StaffList(IFormCollection formData)
        {
            string searchText = formData["SearchText"].ToString();

            if (string.IsNullOrWhiteSpace(searchText))
                searchText = null;

            ViewBag.SearchText = searchText;

            List<StaffModel> stafflist = new List<StaffModel>();

            using (SqlConnection sqlConnection =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand sqlCommand = new SqlCommand("PR_MOM_Staff_SelectAll", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.AddWithValue("@SearchText", searchText ?? (object)DBNull.Value);

                sqlConnection.Open();

                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    stafflist.Add(new StaffModel
                    {
                        StaffId = Convert.ToInt32(reader["StaffID"]),
                        StaffName = reader["StaffName"].ToString(),
                        MobileNo = reader["MobileNo"].ToString(),
                        EmailAddress = reader["EmailAddress"].ToString(),
                        Remarks = reader["Remarks"].ToString(),
                        Created = Convert.ToDateTime(reader["Created"]),
                        DepartmentName = reader["DepartmentName"].ToString()
                    });
                }
            }

            return View(stafflist);
        }


        public IActionResult Delete(int StaffId)
        {
            using (SqlConnection sqlConnection =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand sqlCommand = new SqlCommand("PR_MOM_Staff_DeleteByPK", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@StaffID", StaffId);

                    sqlConnection.Open();

                    sqlCommand.ExecuteNonQuery();
                }
            }

            return RedirectToAction("StaffList");
        }

        
        public IActionResult StaffAddEdit(int? id)
        {
            ViewBag.DepartmentList = FillDepartmentDropDown();

            if (id != null)
            {
                StaffModel model = new StaffModel();

                using (SqlConnection con =
                    new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MOM_Staff_SelectByPK", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@StaffID", id);

                        con.Open();

                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            model.StaffId = Convert.ToInt32(reader["StaffId"]);
                            model.StaffName = reader["StaffName"].ToString();
                            model.MobileNo = reader["MobileNo"].ToString();
                            model.EmailAddress = reader["EmailAddress"].ToString();
                            model.DepartmentID = Convert.ToInt32(reader["DepartmentID"]);
                            model.Remarks = reader["Remarks"].ToString();
                        }
                    }
                }

                return View(model);
            }

            return View(new StaffModel());
        }

       
        public IActionResult Save(StaffModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con =
                    new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (model.StaffId == 0)
                        {
                            cmd.CommandText = "PR_MOM_Staff_Insert";
                        }
                        else
                        {
                            cmd.CommandText = "PR_MOM_Staff_UpdateByPK";
                            cmd.Parameters.Add("@StaffID", SqlDbType.Int).Value = model.StaffId;
                        }

                        cmd.Parameters.Add("@StaffName", SqlDbType.NVarChar).Value = model.StaffName;
                        cmd.Parameters.Add("@MobileNo", SqlDbType.NVarChar).Value = model.MobileNo;
                        cmd.Parameters.Add("@EmailAddress", SqlDbType.NVarChar).Value = model.EmailAddress;
                        cmd.Parameters.Add("@DepartmentID", SqlDbType.Int).Value = model.DepartmentID;
                        cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar).Value = model.Remarks ?? (object)DBNull.Value;

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("StaffList");
            }

            ViewBag.DepartmentList = FillDepartmentDropDown();
            return View("StaffAddEdit", model);
        }


        // Dept dd //

        public List<SelectListItem> FillDepartmentDropDown()
        {
            List<SelectListItem> departmentList = new List<SelectListItem>();

            using (SqlConnection sqlConnection =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand sqlCommand = new SqlCommand("PR_MOM_Department_DDL", sqlConnection))
                {
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlConnection.Open();

                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        departmentList.Add(new SelectListItem
                        {
                            Text = reader["DepartmentName"].ToString(),
                            Value = reader["DepartmentID"].ToString()
                        });
                    }
                }
            }

            return departmentList;

        }

            public IActionResult ExportToExcel()
            {
            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MOM_Staff_SelectAll", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

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
                    var worksheet = workbook.Worksheets.Add("Staff List");

                    // Header
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = dt.Columns[i].ColumnName;
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    // Data
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
                            "StaffList.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error exporting staff data: " + ex.Message;
                return RedirectToAction("StaffList");
            }
        }
    }
}
