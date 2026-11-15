using Microsoft.AspNetCore.Mvc;
using MOM.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using ClosedXML.Excel;

namespace MOM.Controllers
{
    public class MeetingVenueController : Controller
    {
        private readonly IConfiguration _configuration;

        public MeetingVenueController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

    //get//
        [HttpGet]
        public IActionResult MeetingVenueList()
        {
            List<MeetingVenueModel> venueList = new List<MeetingVenueModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("PR_MOM_MeetingVenue_SelectAll", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SearchText", DBNull.Value);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    venueList.Add(new MeetingVenueModel
                    {
                        MeetingVenueID = Convert.ToInt32(reader["MeetingVenueID"]),
                        MeetingVenueName = reader["MeetingVenueName"].ToString() ?? "",
                        Created = Convert.ToDateTime(reader["Created"]),
                        Modified = reader["Modified"] == DBNull.Value ? null : Convert.ToDateTime(reader["Modified"])
                    });
                }
            }

            return View(venueList);
        }

        //post//
        [HttpPost]
        public IActionResult MeetingVenueList(IFormCollection formData)
        {
            string searchText = formData["SearchText"].ToString();

            if (string.IsNullOrWhiteSpace(searchText))
                searchText = null;

            ViewBag.SearchText = searchText;

            List<MeetingVenueModel> venueList = new List<MeetingVenueModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("PR_MOM_MeetingVenue_SelectAll", con);
                cmd.CommandType = CommandType.StoredProcedure;

                if (searchText != null)
                    cmd.Parameters.AddWithValue("@SearchText", searchText);
                else
                    cmd.Parameters.AddWithValue("@SearchText", DBNull.Value);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    venueList.Add(new MeetingVenueModel
                    {
                        MeetingVenueID = Convert.ToInt32(reader["MeetingVenueID"]),
                        MeetingVenueName = reader["MeetingVenueName"].ToString() ?? "",
                        Created = Convert.ToDateTime(reader["Created"]),
                        Modified = reader["Modified"] == DBNull.Value ? null : Convert.ToDateTime(reader["Modified"])
                    });
                }
            }

            return View(venueList);
        }

        
        public IActionResult MeetingVenueAddEdit(int? id)
        {
            if (id != null && id > 0)
            {
                MeetingVenueModel model = new MeetingVenueModel();

                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    SqlCommand cmd = new SqlCommand("PR_MOM_MeetingVenue_SelectByPK", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MeetingVenueID", id);

                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        model.MeetingVenueID = Convert.ToInt32(reader["MeetingVenueID"]);
                        model.MeetingVenueName = reader["MeetingVenueName"].ToString();
                    }
                }

                return View(model);
            }

            return View(new MeetingVenueModel());
        }

      
        public IActionResult Save(MeetingVenueModel model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (model.MeetingVenueID == 0)
                    {
                        cmd.CommandText = "PR_MOM_MeetingVenue_Insert";
                    }
                    else
                    {
                        cmd.CommandText = "PR_MOM_MeetingVenue_UpdateByPK";
                        cmd.Parameters.AddWithValue("@MeetingVenueID", model.MeetingVenueID);
                    }

                    cmd.Parameters.AddWithValue("@MeetingVenueName", model.MeetingVenueName);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Meeting Venue saved successfully";

                return RedirectToAction("MeetingVenueList");
            }

            return View("MeetingVenueAddEdit", model);
        }

        public IActionResult Delete(int MeetingVenueID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    SqlCommand cmd = new SqlCommand("PR_MOM_MeetingVenue_DeleteByPK", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MeetingVenueID", MeetingVenueID);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Meeting Venue deleted successfully.";
            }
            catch
            {
                TempData["ErrorMessage"] = "Cannot delete this venue because it is used in meetings.";
            }

            return RedirectToAction("MeetingVenueList");
        }

        public IActionResult ExportToExcel()
        {
            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MOM_MeetingVenue_SelectAll", conn))
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
                    var worksheet = workbook.Worksheets.Add("Meeting Venues");

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
                            "MeetingVenueList.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error exporting meeting venue data: " + ex.Message;
                return RedirectToAction("MeetingVenueList");
            }
        }
    }
}