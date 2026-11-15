using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MOM.Models;
using System.Data;
using ClosedXML.Excel;

namespace MOM.Controllers
{
    public class MeetingController : Controller
    {
        private readonly IConfiguration _configuration;

        public MeetingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

      

        [HttpGet]
        public IActionResult MeetingList()
        {
            List<MeetingModel> list = new List<MeetingModel>();

            using (SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("PR_MOM_Meetings_SelectAll", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@SearchText", DBNull.Value);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new MeetingModel
                    {
                        MeetingID = Convert.ToInt32(reader["MeetingID"]),
                        MeetingDate = Convert.ToDateTime(reader["MeetingDate"]),
                        MeetingVenueName = reader["MeetingVenueName"].ToString(),
                        MeetingTypeName = reader["MeetingTypeName"].ToString(),
                        DepartmentName = reader["DepartmentName"].ToString(),
                        IsCancelled = Convert.ToBoolean(reader["IsCancelled"])
                    });
                }
            }

            return View(list);
        }


      

        [HttpPost]
        public IActionResult MeetingList(IFormCollection formData)
        {
            string searchText = formData["SearchText"].ToString();

            if (string.IsNullOrWhiteSpace(searchText))
                searchText = null;

            ViewBag.SearchText = searchText;

            List<MeetingModel> list = new List<MeetingModel>();

            using (SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("PR_MOM_Meetings_SelectAll", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@SearchText", searchText ?? (object)DBNull.Value);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new MeetingModel
                    {
                        MeetingID = Convert.ToInt32(reader["MeetingID"]),
                        MeetingDate = Convert.ToDateTime(reader["MeetingDate"]),
                        MeetingVenueName = reader["MeetingVenueName"].ToString(),
                        MeetingTypeName = reader["MeetingTypeName"].ToString(),
                        DepartmentName = reader["DepartmentName"].ToString(),
                        IsCancelled = Convert.ToBoolean(reader["IsCancelled"])
                    });
                }
            }

            return View(list);
        }


       

        public IActionResult MeetingAddEdit(int? id)
        {
            MeetingModel model = new MeetingModel();

            using (SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();

        

                SqlCommand deptCmd = new SqlCommand("PR_MOM_Department_SelectAll", con);
                deptCmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader deptReader = deptCmd.ExecuteReader();

                List<SelectListItem> deptList = new List<SelectListItem>();

                while (deptReader.Read())
                {
                    deptList.Add(new SelectListItem
                    {
                        Value = deptReader["DepartmentID"].ToString(),
                        Text = deptReader["DepartmentName"].ToString()
                    });
                }

                deptReader.Close();
                ViewBag.DepartmentList = deptList;


                SqlCommand typeCmd = new SqlCommand("PR_MOM_MeetingType_SelectAll", con);
                typeCmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader typeReader = typeCmd.ExecuteReader();

                List<SelectListItem> typeList = new List<SelectListItem>();

                while (typeReader.Read())
                {
                    typeList.Add(new SelectListItem
                    {
                        Value = typeReader["MeetingTypeID"].ToString(),
                        Text = typeReader["MeetingTypeName"].ToString()
                    });
                }

                typeReader.Close();
                ViewBag.MeetingTypeList = typeList;



                SqlCommand venueCmd = new SqlCommand("PR_MOM_MeetingVenue_SelectAll", con);
                venueCmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader venueReader = venueCmd.ExecuteReader();

                List<SelectListItem> venueList = new List<SelectListItem>();

                while (venueReader.Read())
                {
                    venueList.Add(new SelectListItem
                    {
                        Value = venueReader["MeetingVenueID"].ToString(),
                        Text = venueReader["MeetingVenueName"].ToString()
                    });
                }

                venueReader.Close();
                ViewBag.MeetingVenueList = venueList;



                if (id != null && id > 0)
                {
                    SqlCommand cmd = new SqlCommand("PR_MOM_Meetings_SelectByPK", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MeetingID", SqlDbType.Int).Value = id;

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        model.MeetingID = Convert.ToInt32(reader["MeetingID"]);
                        model.MeetingDate = Convert.ToDateTime(reader["MeetingDate"]);
                        model.DepartmentID = Convert.ToInt32(reader["DepartmentID"]);
                        model.MeetingTypeID = Convert.ToInt32(reader["MeetingTypeID"]);
                        model.MeetingVenueID = Convert.ToInt32(reader["MeetingVenueID"]);
                        model.MeetingDescription = reader["MeetingDescription"].ToString();
                        model.DocumentPath = reader["DocumentPath"].ToString();
                    }

                    reader.Close();
                }
            }

            return View(model);
        }


        public IActionResult Save(MeetingModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con =
                    new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (model.MeetingID == 0)
                    {
                        cmd.CommandText = "PR_MOM_Meetings_Insert";
                    }
                    else
                    {
                        cmd.CommandText = "PR_MOM_Meetings_UpdateByPK";
                        cmd.Parameters.AddWithValue("@MeetingID", model.MeetingID);
                    }

                    cmd.Parameters.AddWithValue("@MeetingDate", model.MeetingDate);
                    cmd.Parameters.AddWithValue("@MeetingVenueID", model.MeetingVenueID);
                    cmd.Parameters.AddWithValue("@MeetingTypeID", model.MeetingTypeID);
                    cmd.Parameters.AddWithValue("@DepartmentID", model.DepartmentID);
                    cmd.Parameters.AddWithValue("@MeetingDescription", model.MeetingDescription ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DocumentPath", model.DocumentPath ?? (object)DBNull.Value);

                    con.Open();

                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Meeting saved successfully";

                return RedirectToAction("MeetingList");
            }

            return View("MeetingAddEdit", model);
        }
    

        public IActionResult Delete(int MeetingID)
        {
            using (SqlConnection con =
                new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("PR_MOM_Meetings_DeleteByPK", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@MeetingID", MeetingID);

                con.Open();

                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Meeting deleted successfully";

            return RedirectToAction("MeetingList");
        }

        public IActionResult ExportToExcel()
        {
            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MOM_Meetings_SelectAll", conn))
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
                    var worksheet = workbook.Worksheets.Add("Meetings");

                    // Header row 
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = dt.Columns[i].ColumnName;
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    // Data row
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
                            "MeetingList.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error exporting meeting data: " + ex.Message;
                return RedirectToAction("MeetingList");
            }
        }
    }
}