using Microsoft.AspNetCore.Mvc;
using MOM.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using ClosedXML.Excel;

namespace MOM.Controllers
{
    public class MeetingTypeController : Controller
    {
        private readonly IConfiguration _configuration;

        public MeetingTypeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //get//

        [HttpGet]
        public IActionResult MeetingTypeList()
        {
            List<MeetingTypeModel> meetingTypeList = new List<MeetingTypeModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("PR_MOM_MeetingType_SelectAll", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SearchText", DBNull.Value);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    meetingTypeList.Add(new MeetingTypeModel
                    {
                        MeetingTypeID = Convert.ToInt32(reader["MeetingTypeID"]),
                        MeetingTypeName = reader["MeetingTypeName"].ToString() ?? "",
                        Remarks = reader["Remarks"] == DBNull.Value ? null : reader["Remarks"].ToString(),
                        Created = Convert.ToDateTime(reader["Created"]),
                        Modified = reader["Modified"] == DBNull.Value ? null : Convert.ToDateTime(reader["Modified"])
                    });
                }
            }

            return View(meetingTypeList);
        }

        // post//
        
        [HttpPost]
        public IActionResult MeetingTypeList(IFormCollection formData)
        {
            string searchText = formData["SearchText"].ToString();

            if (string.IsNullOrWhiteSpace(searchText))
                searchText = null;

            ViewBag.SearchText = searchText;

            List<MeetingTypeModel> meetingTypeList = new List<MeetingTypeModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("PR_MOM_MeetingType_SelectAll", con);
                cmd.CommandType = CommandType.StoredProcedure;

                if (searchText != null)
                    cmd.Parameters.AddWithValue("@SearchText", searchText);
                else
                    cmd.Parameters.AddWithValue("@SearchText", DBNull.Value);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    meetingTypeList.Add(new MeetingTypeModel
                    {
                        MeetingTypeID = Convert.ToInt32(reader["MeetingTypeID"]),
                        MeetingTypeName = reader["MeetingTypeName"].ToString() ?? "",
                        Remarks = reader["Remarks"] == DBNull.Value ? null : reader["Remarks"].ToString(),
                        Created = Convert.ToDateTime(reader["Created"]),
                        Modified = reader["Modified"] == DBNull.Value ? null : Convert.ToDateTime(reader["Modified"])
                    });
                }
            }

            return View(meetingTypeList);
        }

  
        public IActionResult MeetingTypeAddEdit(int? id)
        {
            if (id != null && id > 0)
            {
                MeetingTypeModel model = new MeetingTypeModel();

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    SqlCommand cmd = new SqlCommand("PR_MOM_MeetingType_SelectByPK", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MeetingTypeID", id);

                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        model.MeetingTypeID = Convert.ToInt32(reader["MeetingTypeID"]);
                        model.MeetingTypeName = reader["MeetingTypeName"].ToString();
                        model.Remarks = reader["Remarks"] == DBNull.Value ? null : reader["Remarks"].ToString();
                    }
                }

                return View(model);
            }

            return View(new MeetingTypeModel());
        }

     
        public IActionResult Save(MeetingTypeModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (model.MeetingTypeID == 0)
                    {
                        cmd.CommandText = "PR_MOM_MeetingType_Insert";
                    }
                    else
                    {
                        cmd.CommandText = "PR_MOM_MeetingType_UpdateByPK";
                        cmd.Parameters.AddWithValue("@MeetingTypeID", model.MeetingTypeID);
                    }

                    cmd.Parameters.AddWithValue("@MeetingTypeName", model.MeetingTypeName);
                    cmd.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modified", DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Meeting Type saved successfully";
                return RedirectToAction("MeetingTypeList");
            }

            return View("MeetingTypeAddEdit", model);
        }

      
        public IActionResult Delete(int MeetingTypeID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    SqlCommand cmd = new SqlCommand("PR_MOM_MeetingType_DeleteByPK", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MeetingTypeID", MeetingTypeID);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Meeting Type deleted successfully";
            }
            catch
            {
                TempData["ErrorMessage"] = "Cannot delete this Meeting Type because it is used in meetings.";
            }

            return RedirectToAction("MeetingTypeList");
        }

        public IActionResult ExportToExcel()
        {
            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MOM_MeetingType_SelectAll", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Same logic → export all
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
                    var worksheet = workbook.Worksheets.Add("Meeting Types");

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
                            "MeetingTypeList.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error exporting meeting type data: " + ex.Message;
                return RedirectToAction("MeetingTypeList");
            }
        }
    }
}