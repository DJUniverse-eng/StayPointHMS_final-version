Imports System
Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class ReportsControl
        Inherits UserControl

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly sidebarColor As Color = Color.FromArgb(32, 58, 95)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly pageColor As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)

        Private ReadOnly cboReport As New ComboBox()
        Private ReadOnly dtFrom As New DateTimePicker()
        Private ReadOnly dtTo As New DateTimePicker()
        Private ReadOnly btnGenerate As New Button()
        Private ReadOnly btnExport As New Button()
        Private ReadOnly lblStatus As New Label()
        Private ReadOnly lblRecordCount As New Label()
        Private ReadOnly lblTotal As New Label()
        Private ReadOnly grid As New DataGridView()

        Private ReadOnly reportPanel As New TableLayoutPanel()
        Private currentData As DataTable

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            SuspendLayout()

            Dock = DockStyle.Fill
            BackColor = pageColor
            MinimumSize = New Size(700, 450)
            Padding = New Padding(18)

            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 4,
                .BackColor = pageColor,
                .Margin = New Padding(0)
            }

            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim heading As New Label With {
                .Text = "Reports",
                .Font = New Font("Segoe UI", 21, FontStyle.Bold),
                .ForeColor = navy,
                .AutoSize = True,
                .Margin = New Padding(0, 0, 0, 3)
            }

            Dim subtitle As New Label With {
                .Text = "Review stay activity, charges, payments, and parking records.",
                .Font = New Font("Segoe UI", 9.5F),
                .ForeColor = muted,
                .AutoSize = True,
                .Margin = New Padding(0, 0, 0, 18)
            }

            Dim titlePanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .FlowDirection = FlowDirection.TopDown,
                .WrapContents = False,
                .Margin = New Padding(0)
            }
            titlePanel.Controls.Add(heading)
            titlePanel.Controls.Add(subtitle)

            Dim filters As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .ColumnCount = 6,
                .RowCount = 2,
                .BackColor = Color.White,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 0, 0, 12)
            }

            filters.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 34))
            filters.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 22))
            filters.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 22))
            filters.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 112))
            filters.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 112))
            filters.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 8))
            filters.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            filters.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            filters.Controls.Add(MakeLabel("Report type"), 0, 0)
            filters.Controls.Add(MakeLabel("From"), 1, 0)
            filters.Controls.Add(MakeLabel("To"), 2, 0)

            cboReport.Dock = DockStyle.Fill
            cboReport.DropDownStyle = ComboBoxStyle.DropDownList
            cboReport.Font = New Font("Segoe UI", 9.5F)
            cboReport.Items.AddRange(New Object() {
                "Stay Summary",
                "Revenue Summary",
                "Charge Details",
                "Payment Details",
                "Parking Tickets"
            })
            cboReport.SelectedIndex = 0
            cboReport.Margin = New Padding(3, 3, 8, 3)

            ConfigureDatePicker(dtFrom)
            ConfigureDatePicker(dtTo)
            dtFrom.Value = Date.Today.AddDays(-30)
            dtTo.Value = Date.Today

            btnGenerate.Text = "Generate"
            StyleButton(btnGenerate, teal)
            btnGenerate.Dock = DockStyle.Fill
            btnGenerate.Margin = New Padding(3, 3, 3, 3)

            btnExport.Text = "Export CSV"
            StyleButton(btnExport, navy)
            btnExport.Dock = DockStyle.Fill
            btnExport.Margin = New Padding(3, 3, 3, 3)

            filters.Controls.Add(cboReport, 0, 1)
            filters.Controls.Add(dtFrom, 1, 1)
            filters.Controls.Add(dtTo, 2, 1)
            filters.Controls.Add(btnGenerate, 3, 1)
            filters.Controls.Add(btnExport, 4, 1)

            Dim summary As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = Color.White,
                .Padding = New Padding(14),
                .Margin = New Padding(0, 0, 0, 12)
            }
            summary.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            summary.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))

            lblRecordCount.Text = "Records: 0"
            lblRecordCount.Dock = DockStyle.Fill
            lblRecordCount.ForeColor = navy
            lblRecordCount.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            lblRecordCount.TextAlign = ContentAlignment.MiddleLeft
            lblRecordCount.AutoSize = True

            lblTotal.Text = "Total: ₱0.00"
            lblTotal.Dock = DockStyle.Fill
            lblTotal.ForeColor = teal
            lblTotal.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            lblTotal.TextAlign = ContentAlignment.MiddleRight
            lblTotal.AutoSize = True

            summary.Controls.Add(lblRecordCount, 0, 0)
            summary.Controls.Add(lblTotal, 1, 0)

            grid.Dock = DockStyle.Fill
            grid.BackgroundColor = Color.White
            grid.BorderStyle = BorderStyle.FixedSingle
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.ReadOnly = True
            grid.MultiSelect = False
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            grid.RowHeadersVisible = False
            grid.AutoGenerateColumns = True
            grid.AllowUserToResizeRows = False
            grid.EnableHeadersVisualStyles = False
            grid.ColumnHeadersDefaultCellStyle.BackColor = navy
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 9, FontStyle.Bold)
            grid.DefaultCellStyle.Font = New Font("Segoe UI", 9)
            grid.DefaultCellStyle.ForeColor = Color.FromArgb(40, 50, 65)
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 242, 239)
            grid.DefaultCellStyle.SelectionForeColor = navy
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(247, 249, 252)
            grid.RowTemplate.Height = 30

            lblStatus.Text = "Select a report and click Generate."
            lblStatus.Dock = DockStyle.Fill
            lblStatus.ForeColor = muted
            lblStatus.Font = New Font("Segoe UI", 9)
            lblStatus.AutoSize = True
            lblStatus.Padding = New Padding(0, 8, 0, 0)

            Dim gridArea As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .Margin = New Padding(0)
            }
            gridArea.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            gridArea.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            gridArea.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            gridArea.Controls.Add(grid, 0, 0)
            gridArea.Controls.Add(lblStatus, 0, 1)

            root.Controls.Add(titlePanel, 0, 0)
            root.Controls.Add(filters, 0, 1)
            root.Controls.Add(summary, 0, 2)
            root.Controls.Add(gridArea, 0, 3)

            Controls.Add(root)

            AddHandler btnGenerate.Click, AddressOf GenerateReport
            AddHandler btnExport.Click, AddressOf ExportReport
            AddHandler Load, AddressOf ReportsControl_Load

            ResumeLayout(True)
        End Sub

        Private Function MakeLabel(caption As String) As Label
            Return New Label With {
                .Text = caption,
                .AutoSize = True,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .Margin = New Padding(3, 3, 3, 5)
            }
        End Function

        Private Sub ConfigureDatePicker(picker As DateTimePicker)
            picker.Dock = DockStyle.Fill
            picker.Format = DateTimePickerFormat.Custom
            picker.CustomFormat = "MMM dd, yyyy"
            picker.Font = New Font("Segoe UI", 9)
            picker.Margin = New Padding(3, 3, 8, 3)
        End Sub

        Private Sub StyleButton(button As Button, backColor As Color)
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderSize = 0
            button.BackColor = backColor
            button.ForeColor = Color.White
            button.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            button.Cursor = Cursors.Hand
            button.Height = 36
        End Sub

        Private Sub ReportsControl_Load(sender As Object, e As EventArgs)
            GenerateReport(Nothing, EventArgs.Empty)
        End Sub

        Private Sub GenerateReport(sender As Object, e As EventArgs)
            If dtFrom.Value.Date > dtTo.Value.Date Then
                MessageBox.Show(
                    "The From date cannot be later than the To date.",
                    "Invalid Date Range",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
                Return
            End If

            Try
                Cursor = Cursors.WaitCursor
                btnGenerate.Enabled = False
                lblStatus.Text = "Generating report..."

                Dim startDate As DateTime = dtFrom.Value.Date
                Dim endDateExclusive As DateTime = dtTo.Value.Date.AddDays(1)

                Dim sql As String = GetReportSql(cboReport.Text)

                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Using command As New MySqlCommand(sql, connection)
                        command.Parameters.Add("@fromDate", MySqlDbType.DateTime).Value = startDate
                        command.Parameters.Add("@toDate", MySqlDbType.DateTime).Value = endDateExclusive

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            Dim result As New DataTable()
                            result.Load(reader)
                            currentData = result
                        End Using
                    End Using
                End Using

                grid.DataSource = currentData
                FormatGrid()
                UpdateSummary()

                lblStatus.Text = String.Format(
                    CultureInfo.InvariantCulture,
                    "Report generated for {0:MMM dd, yyyy} to {1:MMM dd, yyyy}.",
                    startDate,
                    dtTo.Value.Date)

            Catch ex As Exception
                lblStatus.Text = "Unable to generate report."
                MessageBox.Show(
                    "The report could not be generated." &
                    Environment.NewLine & Environment.NewLine &
                    ex.Message,
                    "Reports",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            Finally
                btnGenerate.Enabled = True
                Cursor = Cursors.Default
            End Try
        End Sub

        Private Function GetReportSql(reportName As String) As String
            Select Case reportName

                Case "Stay Summary"
                    Return "
SELECT
    s.stay_reference AS `Stay Reference`,
    CONCAT(g.first_name, ' ', g.last_name) AS `Guest`,
    g.phone AS `Phone`,
    s.stay_status AS `Stay Status`,
    s.checked_in_at AS `Checked In`,
    s.expected_checkout_at AS `Expected Checkout`,
    s.checked_out_at AS `Checked Out`,
    COALESCE(room_data.room_numbers, 'No room assigned') AS `Rooms`,
    COALESCE(parking_data.active_tickets, 0) AS `Active Parking Tickets`
FROM stays s
INNER JOIN guests g ON g.guest_id = s.guest_id
LEFT JOIN (
    SELECT
        ra.stay_id,
        GROUP_CONCAT(DISTINCT r.room_number ORDER BY r.room_number SEPARATOR ', ') AS room_numbers
    FROM room_assignments ra
    INNER JOIN rooms r ON r.room_id = ra.room_id
    WHERE ra.released_at IS NULL
    GROUP BY ra.stay_id
) room_data ON room_data.stay_id = s.stay_id
LEFT JOIN (
    SELECT
        pt.stay_id,
        COUNT(*) AS active_tickets
    FROM parking_tickets pt
    WHERE pt.ticket_status = 'ACTIVE'
    GROUP BY pt.stay_id
) parking_data ON parking_data.stay_id = s.stay_id
WHERE s.checked_in_at >= @fromDate
  AND s.checked_in_at < @toDate
ORDER BY s.checked_in_at DESC;"

                Case "Revenue Summary"
                    Return "
SELECT
    summary_data.`Activity Date`,
    summary_data.`Category`,
    summary_data.`Transaction Count`,
    summary_data.`Amount (PHP)`
FROM (
    SELECT
        DATE(fc.charged_at) AS `Activity Date`,
        CONCAT('Charges - ', fc.charge_type) AS `Category`,
        COUNT(*) AS `Transaction Count`,
        SUM(fc.amount) AS `Amount (PHP)`
    FROM folio_charges fc
    INNER JOIN guest_folios gf ON gf.folio_id = fc.folio_id
    WHERE fc.charged_at >= @fromDate
      AND fc.charged_at < @toDate
      AND gf.folio_status <> 'VOID'
    GROUP BY DATE(fc.charged_at), fc.charge_type

    UNION ALL

    SELECT
        DATE(p.paid_at) AS `Activity Date`,
        'Cash Payments Received' AS `Category`,
        COUNT(*) AS `Transaction Count`,
        SUM(p.amount) AS `Amount (PHP)`
    FROM payments p
    WHERE p.paid_at >= @fromDate
      AND p.paid_at < @toDate
      AND p.payment_status = 'COMPLETED'
    GROUP BY DATE(p.paid_at)
) summary_data
ORDER BY summary_data.`Activity Date` DESC,
         summary_data.`Category`;"

                Case "Charge Details"
                    Return "
SELECT
    fc.folio_charge_id AS `Charge ID`,
    s.stay_reference AS `Stay Reference`,
    CONCAT(g.first_name, ' ', g.last_name) AS `Guest`,
    fc.charge_type AS `Charge Type`,
    fc.description AS `Description`,
    fc.quantity AS `Quantity`,
    fc.unit_price AS `Unit Price (PHP)`,
    fc.amount AS `Amount (PHP)`,
    fc.charged_at AS `Charged At`
FROM folio_charges fc
INNER JOIN guest_folios gf ON gf.folio_id = fc.folio_id
INNER JOIN stays s ON s.stay_id = gf.stay_id
INNER JOIN guests g ON g.guest_id = s.guest_id
WHERE fc.charged_at >= @fromDate
  AND fc.charged_at < @toDate
  AND gf.folio_status <> 'VOID'
ORDER BY fc.charged_at DESC;"

                Case "Payment Details"
                    Return "
SELECT
    p.receipt_number AS `Receipt Number`,
    s.stay_reference AS `Stay Reference`,
    CONCAT(g.first_name, ' ', g.last_name) AS `Guest`,
    p.amount AS `Amount (PHP)`,
    p.payment_method AS `Method`,
    p.payment_status AS `Status`,
    p.paid_at AS `Paid At`,
    p.notes AS `Notes`
FROM payments p
INNER JOIN guest_folios gf ON gf.folio_id = p.folio_id
INNER JOIN stays s ON s.stay_id = gf.stay_id
INNER JOIN guests g ON g.guest_id = s.guest_id
WHERE p.paid_at >= @fromDate
  AND p.paid_at < @toDate
ORDER BY p.paid_at DESC;"

                Case "Parking Tickets"
                    Return "
SELECT
    pt.ticket_reference AS `Ticket Reference`,
    pt.plate_number AS `Plate Number`,
    pt.vehicle_category AS `Vehicle Category`,
    pt.ticket_status AS `Status`,
    CONCAT(g.first_name, ' ', g.last_name) AS `Guest`,
    s.stay_reference AS `Stay Reference`,
    pt.entered_at AS `Entry Time`,
    pt.departed_at AS `Departure Time`,
    pt.hourly_rate_snapshot AS `Hourly Rate (PHP)`,
    pt.minimum_charge_snapshot AS `Minimum Charge (PHP)`
FROM parking_tickets pt
LEFT JOIN guests g ON g.guest_id = pt.guest_id
LEFT JOIN stays s ON s.stay_id = pt.stay_id
WHERE pt.entered_at >= @fromDate
  AND pt.entered_at < @toDate
ORDER BY pt.entered_at DESC;"

                Case Else
                    Throw New InvalidOperationException("Unknown report type.")
            End Select
        End Function

        Private Sub FormatGrid()
            If grid.Columns.Count = 0 Then Return

            For Each column As DataGridViewColumn In grid.Columns
                column.SortMode = DataGridViewColumnSortMode.Automatic

                Dim columnName As String = column.Name.ToLowerInvariant()

                If columnName.Contains("amount") OrElse
                   columnName.Contains("price") OrElse
                   columnName.Contains("rate") OrElse
                   columnName.Contains("charge") Then

                    column.DefaultCellStyle.Format = "N2"
                    column.DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight

                ElseIf columnName.Contains("date") OrElse
                       columnName.Contains("time") OrElse
                       columnName.Contains("checked") OrElse
                       columnName.Contains("paid") OrElse
                       columnName.Contains("entry") OrElse
                       columnName.Contains("departure") Then

                    column.DefaultCellStyle.Format = "MMM dd, yyyy hh:mm tt"
                End If
            Next
        End Sub

        Private Sub UpdateSummary()
            If currentData Is Nothing Then
                lblRecordCount.Text = "Records: 0"
                lblTotal.Text = "Total: ₱0.00"
                Return
            End If

            lblRecordCount.Text = "Records: " & currentData.Rows.Count.ToString("N0")

            Dim total As Decimal = 0D
            Dim totalColumn As String = Nothing

            If currentData.Columns.Contains("Amount (PHP)") Then
                totalColumn = "Amount (PHP)"
            ElseIf currentData.Columns.Contains("Amount") Then
                totalColumn = "Amount"
            End If

            If totalColumn IsNot Nothing Then
                For Each row As DataRow In currentData.Rows
                    If Not row.IsNull(totalColumn) Then
                        Dim value As Decimal
                        If Decimal.TryParse(
                            Convert.ToString(row(totalColumn), CultureInfo.InvariantCulture),
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            value) Then
                            total += value
                        Else
                            total += Convert.ToDecimal(row(totalColumn))
                        End If
                    End If
                Next
            End If

            lblTotal.Text = "Total: ₱" & total.ToString("N2")
        End Sub

        Private Sub ExportReport(sender As Object, e As EventArgs)
            If currentData Is Nothing OrElse currentData.Rows.Count = 0 Then
                MessageBox.Show(
                    "Generate a report containing data before exporting.",
                    "Export Report",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
                Return
            End If

            Using dialog As New SaveFileDialog()
                dialog.Title = "Export Report"
                dialog.Filter = "CSV files (*.csv)|*.csv"
                dialog.FileName = "StayPoint_" &
                    cboReport.Text.Replace(" ", "_") & "_" &
                    DateTime.Now.ToString("yyyyMMdd_HHmm") & ".csv"

                If dialog.ShowDialog() <> DialogResult.OK Then Return

                Try
                    Using writer As New StreamWriter(
                        dialog.FileName,
                        False,
                        New UTF8Encoding(True))

                        Dim headers As New List(Of String)()

                        For Each column As DataColumn In currentData.Columns
                            headers.Add(EscapeCsv(column.ColumnName))
                        Next

                        writer.WriteLine(String.Join(",", headers))

                        For Each row As DataRow In currentData.Rows
                            Dim values As New List(Of String)()

                            For Each column As DataColumn In currentData.Columns
                                Dim value As String =
                                    If(row.IsNull(column), "", Convert.ToString(
                                        row(column), CultureInfo.InvariantCulture))

                                values.Add(EscapeCsv(value))
                            Next

                            writer.WriteLine(String.Join(",", values))
                        Next
                    End Using

                    MessageBox.Show(
                        "Report exported successfully.",
                        "Export Report",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)

                Catch ex As Exception
                    MessageBox.Show(
                        "Could not export the report." &
                        Environment.NewLine & ex.Message,
                        "Export Report",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
                End Try
            End Using
        End Sub

        Private Function EscapeCsv(value As String) As String
            If value Is Nothing Then Return """"""

            Return """" & value.Replace("""", """""") & """"
        End Function

    End Class

End Namespace
