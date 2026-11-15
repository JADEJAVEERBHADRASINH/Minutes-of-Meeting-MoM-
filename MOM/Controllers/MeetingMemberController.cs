using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using MOM.Models;
using System.Data;
using ClosedXML.Excel;

namespace MOM.Controllers
{
    public class MeetingMemberController : Controller
    {
        private readonly IConfiguration _configuration;

        public MeetingMemberController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //get
        [HttpGet]
        public IActionResult MeetingMemberList()
        {
            List<MeetingMemberModel> list = new List<MeetingMemberModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("PR_MOM_MeetingMember_SelectAll", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SearchText", DBNull.Value);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        list.Add(new MeetingMemberModel
                        {
                            MeetingMemberID = Convert.ToInt32(reader["MeetingMemberID"]),
                            MeetingDate = Convert.ToDateTime(reader["MeetingDate"]),
                            StaffName = reader["StaffName"].ToString(),
                            IsPresent = Convert.ToBoolean(reader["IsPresent"]),
                            Remarks = reader["Remarks"].ToString()
                        });
                    }
                }
            }

            return View(list);
        }

       //post
        [HttpPost]
        public IActionResult MeetingMemberList(IFormCollection formData)
        {
            string searchText = formData["SearchText"].ToString();

            if (string.IsNullOrWhiteSpace(searchText))
                searchText = null;

            ViewBag.SearchText = searchText;

            List<MeetingMemberModel> list = new List<MeetingMemberModel>();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("PR_MOM_MeetingMember_SelectAll", con))
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
                        list.Add(new MeetingMemberModel
                        {
                            MeetingMemberID = Convert.ToInt32(reader["MeetingMemberID"]),
                            MeetingDate = Convert.ToDateTime(reader["MeetingDate"]),
                            StaffName = reader["StaffName"].ToString(),
                            IsPresent = Convert.ToBoolean(reader["IsPresent"]),
                            Remarks = reader["Remarks"].ToString()
                        });
                    }
                }
            }

            return View(list);
        }

        public IActionResult MeetingMemberAddEdit(int? id)
        {
            MeetingMemberModel model = new MeetingMemberModel();

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();

                // Meeting Dropdown
                SqlCommand meetingCmd = new SqlCommand(
                    "SELECT MeetingID, MeetingDate FROM MOM_Meetings", con);

                SqlDataReader meetingReader = meetingCmd.ExecuteReader();

                List<SelectListItem> meetingList = new List<SelectListItem>();

                while (meetingReader.Read())
                {
                    meetingList.Add(new SelectListItem
                    {
                        Value = meetingReader["MeetingID"].ToString(),
                        Text = Convert.ToDateTime(meetingReader["MeetingDate"]).ToString("dd-MM-yyyy")
                    });
                }

                meetingReader.Close();
                ViewBag.MeetingList = meetingList;


                //Staff dd
                SqlCommand staffCmd = new SqlCommand(
                    "SELECT StaffID, StaffName FROM MOM_Staff", con);

                SqlDataReader staffReader = staffCmd.ExecuteReader();

                List<SelectListItem> staffList = new List<SelectListItem>();

                while (staffReader.Read())
                {
                    staffList.Add(new SelectListItem
                    {
                        Value = staffReader["StaffID"].ToString(),
                        Text = staffReader["StaffName"].ToString()
                    });
                }

                staffReader.Close();
                ViewBag.StaffList = staffList;


                // edit//
                if (id != null && id > 0)
                {
                    SqlCommand cmd = new SqlCommand(
                        "PR_MOM_MeetingMember_SelectByPK", con);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@MeetingMemberID", SqlDbType.Int).Value = id;

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        model.MeetingMemberID = Convert.ToInt32(reader["MeetingMemberID"]);
                        model.MeetingID = Convert.ToInt32(reader["MeetingID"]);
                        model.StaffID = Convert.ToInt32(reader["StaffID"]);
                        model.IsPresent = Convert.ToBoolean(reader["IsPresent"]);
                        model.Remarks = reader["Remarks"].ToString();
                    }

                    reader.Close();
                }
            }

            return View(model);
        }

       
        [HttpPost]
        public IActionResult Save(MeetingMemberModel model)
        {
            if (!ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    con.Open();

                    // reload Meeting dropdown
                    SqlCommand meetingCmd = new SqlCommand("SELECT MeetingID, MeetingDate FROM MOM_Meetings", con);
                    SqlDataReader meetingReader = meetingCmd.ExecuteReader();

                    List<SelectListItem> meetingList = new List<SelectListItem>();

                    while (meetingReader.Read())
                    {
                        meetingList.Add(new SelectListItem
                        {
                            Value = meetingReader["MeetingID"].ToString(),
                            Text = Convert.ToDateTime(meetingReader["MeetingDate"]).ToString("dd-MM-yyyy")
                        });
                    }

                    meetingReader.Close();
                    ViewBag.MeetingList = meetingList;

                    
                    SqlCommand staffCmd = new SqlCommand("SELECT StaffID, StaffName FROM MOM_Staff", con);
                    SqlDataReader staffReader = staffCmd.ExecuteReader();

                    List<SelectListItem> staffList = new List<SelectListItem>();

                    while (staffReader.Read())
                    {
                        staffList.Add(new SelectListItem
                        {
                            Value = staffReader["StaffID"].ToString(),
                            Text = staffReader["StaffName"].ToString()
                        });
                    }

                    staffReader.Close();
                    ViewBag.StaffList = staffList;
                }

                return View("MeetingMemberAddEdit", model);
            }

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (model.MeetingMemberID == 0)
                    {
                        cmd.CommandText = "PR_MOM_MeetingMember_Insert";
                    }
                    else
                    {
                        cmd.CommandText = "PR_MOM_MeetingMember_UpdateByPK";
                        cmd.Parameters.Add("@MeetingMemberID", SqlDbType.Int).Value = model.MeetingMemberID;
                    }

                    cmd.Parameters.AddWithValue("@MeetingID", model.MeetingID);
                    cmd.Parameters.AddWithValue("@StaffID", model.StaffID);
                    cmd.Parameters.AddWithValue("@IsPresent", model.IsPresent);
                    cmd.Parameters.AddWithValue("@Remarks", model.Remarks ?? (object)DBNull.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Meeting Member saved successfully.";
            return RedirectToAction("MeetingMemberList");
        }

      
        public IActionResult Delete(int MeetingMemberID)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand("PR_MOM_MeetingMember_DeleteByPK", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MeetingMemberID", MeetingMemberID);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Meeting Member deleted successfully.";
            return RedirectToAction("MeetingMemberList");
        }

        public IActionResult ExportToExcel()
        {
            try
            {
                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    using (SqlCommand cmd = new SqlCommand("PR_MOM_MeetingMember_SelectAll", conn))
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
                    var worksheet = workbook.Worksheets.Add("Meeting Members");

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
                            "MeetingMemberList.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error exporting meeting member data: " + ex.Message;
                return RedirectToAction("MeetingMemberList");
            }
        }
    }
}