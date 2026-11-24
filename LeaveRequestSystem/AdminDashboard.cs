using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LeaveRequestSystem
{
    public partial class AdminDashboard : Form
    {
        private List<LeaveRequest> displayedRequests = new List<LeaveRequest>();

        public AdminDashboard()
        {
            InitializeComponent();
            EnsureSeededData();
            PopulateEmployeeDropdown();
            RefreshAdminView();
        }

        private void EnsureSeededData()
        {
            if (MainDashboard.requestList.Count == 0)
            {
                MainDashboard.requestList.Add(new LeaveRequest("Mahmoud", "Annual", 4, "Accepted", DateTime.Now.AddDays(-14)));
                MainDashboard.requestList.Add(new LeaveRequest("Ahmed", "Sick", 2, "Rejected", DateTime.Now.AddDays(-6)));
                MainDashboard.requestList.Add(new LeaveRequest("Ahmed", "Annual", 5, "Under Review", DateTime.Now.AddDays(-1)));
            }
        }

        private void PopulateEmployeeDropdown()
        {
            cmbEmployees.Items.Clear();
            cmbEmployees.Items.Add("All Employees");

            foreach (LeaveRequest req in MainDashboard.requestList)
            {
                if (!cmbEmployees.Items.Contains(req.EmployeeName))
                {
                    cmbEmployees.Items.Add(req.EmployeeName);
                }
            }

            cmbEmployees.SelectedIndex = 0;
        }

        private void RefreshAdminView(int preserveRowIndex = -1)
        {
            string selectedFilter = cmbEmployees.SelectedItem?.ToString() ?? "All Employees";

            dgvAdminRequests.Rows.Clear();
            displayedRequests.Clear();

            foreach (LeaveRequest req in MainDashboard.requestList)
            {
                if (selectedFilter == "All Employees" ||
                    req.EmployeeName.Equals(selectedFilter, StringComparison.OrdinalIgnoreCase))
                {
                    displayedRequests.Add(req);
                    dgvAdminRequests.Rows.Add(
                        req.Status.ToUpper(),
                        req.EmployeeName,
                        req.LeaveType,
                        $"{req.DurationDays} {(req.DurationDays == 1 ? "Day" : "Days")}",
                        req.RequestDate.ToString("MMM dd, yyyy")
                    );
                }
            }

            if (preserveRowIndex >= 0 && preserveRowIndex < dgvAdminRequests.Rows.Count)
            {
                dgvAdminRequests.ClearSelection();
                dgvAdminRequests.Rows[preserveRowIndex].Selected = true;
            }

            UpdateEmployeeHistorySummary(selectedFilter);
        }

        private void UpdateEmployeeHistorySummary(string selectedEmployee)
        {
            if (selectedEmployee == "All Employees")
            {
                lblLastRequestInfo.Text = $"Showing all {displayedRequests.Count} request(s) across the organization. Select an employee above to inspect their history and last leave details.";
                return;
            }

            if (displayedRequests.Count == 0)
            {
                lblLastRequestInfo.Text = $"Employee: {selectedEmployee}  |  No leave history found.";
                return;
            }

            LeaveRequest lastReq = displayedRequests[0];
            foreach (LeaveRequest req in displayedRequests)
            {
                if (req.RequestDate > lastReq.RequestDate)
                {
                    lastReq = req;
                }
            }

            TimeSpan elapsed = DateTime.Now - lastReq.RequestDate;
            string timeAgoText;

            if (elapsed.TotalMinutes < 1)
                timeAgoText = "Just now";
            else if (elapsed.TotalHours < 1)
                timeAgoText = $"{(int)elapsed.TotalMinutes} minute(s) ago";
            else if (elapsed.TotalDays < 1)
                timeAgoText = $"{(int)elapsed.TotalHours} hour(s) ago";
            else
                timeAgoText = $"{(int)elapsed.TotalDays} day(s) ago";

            lblLastRequestInfo.Text =
                $"Employee: {selectedEmployee}  •  Total History: {displayedRequests.Count} request(s)\r\n" +
                $"Last Request: {lastReq.LeaveType} ({lastReq.DurationDays} days)  •  Submitted: {timeAgoText} ({lastReq.RequestDate:MMM dd, yyyy})  •  Status: {lastReq.Status}";
        }

        private void dgvAdminRequests_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;

            // Color-code the Status partition
            if (e.ColumnIndex == 0 && e.CellStyle != null)
            {
                string status = e.Value.ToString() ?? string.Empty;
                e.CellStyle.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);

                if (status == "ACCEPTED")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(5, 150, 105);
                    e.CellStyle.BackColor = Color.FromArgb(236, 253, 245);
                    e.CellStyle.SelectionForeColor = Color.FromArgb(5, 150, 105);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(209, 250, 229);
                }
                else if (status == "REJECTED")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(225, 29, 72);
                    e.CellStyle.BackColor = Color.FromArgb(255, 241, 242);
                    e.CellStyle.SelectionForeColor = Color.FromArgb(225, 29, 72);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(255, 228, 230);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.FromArgb(217, 119, 6);
                    e.CellStyle.BackColor = Color.FromArgb(255, 251, 235);
                    e.CellStyle.SelectionForeColor = Color.FromArgb(217, 119, 6);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(254, 243, 199);
                }
            }
            // Bold the Employee Name partition
            else if (e.ColumnIndex == 1 && e.CellStyle != null)
            {
                e.CellStyle.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
            }
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            if (dgvAdminRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a request row to accept.",
                                "No Request Selected",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            int idx = dgvAdminRequests.SelectedRows[0].Index;
            displayedRequests[idx].Status = "Accepted";
            RefreshAdminView(idx);
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            if (dgvAdminRequests.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a request row to reject.",
                                "No Request Selected",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            int idx = dgvAdminRequests.SelectedRows[0].Index;
            displayedRequests[idx].Status = "Rejected";
            RefreshAdminView(idx);
        }

        private void cmbEmployees_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshAdminView();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            LoginForm login = new LoginForm();
            login.Show();
            this.Hide();
        }

        private void AdminDashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}