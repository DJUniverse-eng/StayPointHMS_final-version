
Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class GuestStayHistoryControl
        Inherits UserControl

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly sidebarColor As Color = Color.FromArgb(32, 58, 95)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly pageColor As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)

        Private ReadOnly txtSearch As New TextBox()
        Private ReadOnly cboStatus As New ComboBox()
        Private ReadOnly dtFrom As New DateTimePicker()
        Private ReadOnly dtTo As New DateTimePicker()
        Private ReadOnly btnRefresh As New Button()
        Private ReadOnly btnDetails As New Button()
        Private ReadOnly grid As New DataGridView()
        Private ReadOnly lblResult As New Label()

        Private ReadOnly lblTotal As New Label()
        Private ReadOnly lblActive As New Label()
        Private ReadOnly lblCompleted As New Label()
        Private ReadOnly lblRevenue As New Label()

        Private isLoading As Boolean

        Public Sub New()
            BuildInterface()
            AddHandler Load, AddressOf ControlLoaded
        End Sub

        Private Sub ControlLoaded(sender As Object, e As EventArgs)
            LoadHistory()
        End Sub

        Private Sub BuildInterface()
            Dock = DockStyle.Fill
            BackColor = pageColor
            Padding = New Padding(18)

            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 5,
                .BackColor = pageColor,
                .Margin = New Padding(0)
            }

            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 58))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 104))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 105))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 48))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim heading As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = pageColor,
                .Margin = New Padding(0)
            }

            Dim title As New Label With {
                .Text = "Guest Stay History",
                .Font = New Font("Segoe UI", 19, FontStyle.Bold),
                .ForeColor = navy,
                .AutoSize = True,
                .Location = New Point(0, 0)
            }

            Dim subtitle As New Label With {
                .Text = "Review guest stays, room assignments, parking, and folio totals.",
                .Font = New Font("Segoe UI", 9),
                .ForeColor = muted,
                .AutoSize = True,
                .Location = New Point(2, 34)
            }

            heading.Controls.Add(title)
            heading.Controls.Add(subtitle)

            root.Controls.Add(heading, 0, 0)
            root.Controls.Add(BuildSummaryCards(), 0, 1)
            root.Controls.Add(BuildFilters(), 0, 2)
            root.Controls.Add(BuildGridToolbar(), 0, 3)

            ConfigureGrid()

            Dim gridHost As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(1),
                .Margin = New Padding(0, 4, 0, 0)
            }

            grid.Dock = DockStyle.Fill
            gridHost.Controls.Add(grid)
            root.Controls.Add(gridHost, 0, 4)

            Controls.Add(root)
        End Sub

        Private Function BuildSummaryCards() As Control
            Dim host As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 4,
                .RowCount = 1,
                .BackColor = pageColor,
                .Margin = New Padding(0, 4, 0, 8)
            }

            For i As Integer = 0 To 3
                host.ColumnStyles.Add(
                    New ColumnStyle(SizeType.Percent, 25))
            Next

            host.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            host.Controls.Add(
                CreateMetricCard("Total Stays", lblTotal, navy), 0, 0)

            host.Controls.Add(
                CreateMetricCard("Currently Active", lblActive, teal), 1, 0)

            host.Controls.Add(
                CreateMetricCard("Checked Out", lblCompleted, sidebarColor), 2, 0)

            host.Controls.Add(
                CreateMetricCard("Completed Payments", lblRevenue, teal), 3, 0)

            Return host
        End Function

        Private Function CreateMetricCard(
            caption As String,
            valueLabel As Label,
            accent As Color) As Control

            Dim card As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Margin = New Padding(0, 0, 10, 0),
                .Padding = New Padding(14, 10, 10, 8)
            }

            AddHandler card.Paint,
                Sub(sender As Object, e As PaintEventArgs)
                    Using pen As New Pen(borderColor)
                        e.Graphics.DrawRectangle(
                            pen, 0, 0,
                            Math.Max(0, card.Width - 1),
                            Math.Max(0, card.Height - 1))
                    End Using

                    Using brush As New SolidBrush(accent)
                        e.Graphics.FillRectangle(brush, 0, 0, 4, card.Height)
                    End Using
                End Sub

            Dim captionLabel As New Label With {
                .Text = caption,
                .Dock = DockStyle.Top,
                .Height = 25,
                .Font = New Font("Segoe UI", 9),
                .ForeColor = muted
            }

            valueLabel.Text = "—"
            valueLabel.Dock = DockStyle.Fill
            valueLabel.Font = New Font("Segoe UI", 19, FontStyle.Bold)
            valueLabel.ForeColor = navy
            valueLabel.TextAlign = ContentAlignment.MiddleLeft
            valueLabel.AutoEllipsis = True

            card.Controls.Add(valueLabel)
            card.Controls.Add(captionLabel)

            Return card
        End Function

        Private Function BuildFilters() As Control
            Dim outer As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 0, 0, 8)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 6,
                .RowCount = 2,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 135))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 21))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 21))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 1))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 1))

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 25))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim lblSearch As New Label With {
                .Text = "Search guest / phone / stay reference",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 8.5)
            }

            Dim lblStatus As New Label With {
                .Text = "Stay Status",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 8.5)
            }

            Dim lblFrom As New Label With {
                .Text = "Check-in From",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 8.5)
            }

            Dim lblTo As New Label With {
                .Text = "Check-in To",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 8.5)
            }

            txtSearch.Dock = DockStyle.Fill
            txtSearch.Font = New Font("Segoe UI", 10)
            txtSearch.BorderStyle = BorderStyle.FixedSingle
            txtSearch.Margin = New Padding(0, 2, 10, 0)

            cboStatus.Dock = DockStyle.Fill
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList
            cboStatus.Font = New Font("Segoe UI", 9)
            cboStatus.Items.AddRange(
                New Object() {"ALL", "ACTIVE", "CHECKED_OUT", "CANCELLED"})
            cboStatus.SelectedIndex = 0
            cboStatus.Margin = New Padding(0, 2, 10, 0)

            ConfigureDatePicker(dtFrom)
            ConfigureDatePicker(dtTo)

            layout.Controls.Add(lblSearch, 0, 0)
            layout.Controls.Add(lblStatus, 1, 0)
            layout.Controls.Add(lblFrom, 2, 0)
            layout.Controls.Add(lblTo, 3, 0)

            layout.Controls.Add(txtSearch, 0, 1)
            layout.Controls.Add(cboStatus, 1, 1)
            layout.Controls.Add(dtFrom, 2, 1)
            layout.Controls.Add(dtTo, 3, 1)

            outer.Controls.Add(layout)

            AddHandler txtSearch.TextChanged,
                AddressOf FilterChanged
            AddHandler cboStatus.SelectedIndexChanged,
                AddressOf FilterChanged
            AddHandler dtFrom.ValueChanged,
                AddressOf FilterChanged
            AddHandler dtTo.ValueChanged,
                AddressOf FilterChanged

            Return outer
        End Function

        Private Sub ConfigureDatePicker(picker As DateTimePicker)
            picker.Dock = DockStyle.Fill
            picker.Format = DateTimePickerFormat.Short
            picker.ShowCheckBox = True
            picker.Checked = False
            picker.Font = New Font("Segoe UI", 9)
            picker.Margin = New Padding(0, 2, 10, 0)
        End Sub

        Private Function BuildGridToolbar() As Control
            Dim host As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 3,
                .RowCount = 1,
                .BackColor = pageColor,
                .Margin = New Padding(0)
            }

            host.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            host.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 115))
            host.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 125))

            lblResult.Dock = DockStyle.Fill
            lblResult.Text = "Stay records"
            lblResult.TextAlign = ContentAlignment.MiddleLeft
            lblResult.ForeColor = muted
            lblResult.Font = New Font("Segoe UI", 9)

            StyleButton(btnRefresh, "Refresh", navy)
            StyleButton(btnDetails, "View Details", teal)

            AddHandler btnRefresh.Click,
                Sub(sender As Object, e As EventArgs)
                    LoadHistory()
                End Sub

            AddHandler btnDetails.Click,
                Sub(sender As Object, e As EventArgs)
                    OpenSelectedDetails()
                End Sub

            host.Controls.Add(lblResult, 0, 0)
            host.Controls.Add(btnRefresh, 1, 0)
            host.Controls.Add(btnDetails, 2, 0)

            Return host
        End Function

        Private Sub StyleButton(
            button As Button,
            caption As String,
            backColor As Color)

            button.Text = caption
            button.Dock = DockStyle.Fill
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderSize = 0
            button.BackColor = backColor
            button.ForeColor = Color.White
            button.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            button.Cursor = Cursors.Hand
            button.Margin = New Padding(5, 3, 5, 3)
        End Sub

        Private Sub ConfigureGrid()
            grid.BackgroundColor = Color.White
            grid.BorderStyle = BorderStyle.None
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            grid.GridColor = borderColor
            grid.RowHeadersVisible = False
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.AllowUserToResizeRows = False
            grid.ReadOnly = True
            grid.MultiSelect = False
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            grid.AutoGenerateColumns = False
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            grid.ColumnHeadersHeight = 42
            grid.RowTemplate.Height = 38
            grid.EnableHeadersVisualStyles = False
            grid.ColumnHeadersDefaultCellStyle.BackColor = navy
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 9, FontStyle.Bold)
            grid.DefaultCellStyle.Font = New Font("Segoe UI", 9)
            grid.DefaultCellStyle.ForeColor = navy
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(218, 242, 238)
            grid.DefaultCellStyle.SelectionForeColor = navy
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252)

            AddTextColumn("stay_id", "Stay ID", 65, False)
            AddTextColumn("stay_reference", "Stay Reference", 115)
            AddTextColumn("guest_name", "Guest Name", 150)
            AddTextColumn("phone", "Contact Number", 105)
            AddTextColumn("room_numbers", "Room(s)", 95)
            AddTextColumn("checked_in_at", "Check-in", 125)
            AddTextColumn("checked_out_at", "Check-out", 125)
            AddTextColumn("stay_status", "Status", 95)
            AddTextColumn("total_charges", "Charges (₱)", 95, True, "N2")
            AddTextColumn("amount_paid", "Paid (₱)", 95, True, "N2")
            AddTextColumn("balance", "Balance (₱)", 95, True, "N2")

            AddHandler grid.CellDoubleClick,
                Sub(sender As Object, e As DataGridViewCellEventArgs)
                    If e.RowIndex >= 0 Then OpenSelectedDetails()
                End Sub
        End Sub

        Private Sub AddTextColumn(
            propertyName As String,
            heading As String,
            fillWeight As Single,
            Optional rightAlign As Boolean = False,
            Optional format As String = Nothing)

            Dim column As New DataGridViewTextBoxColumn With {
                .Name = propertyName,
                .HeaderText = heading,
                .DataPropertyName = propertyName,
                .fillWeight = fillWeight,
                .MinimumWidth = 65,
                .SortMode = DataGridViewColumnSortMode.Automatic
            }

            If Not String.IsNullOrEmpty(format) Then
                column.DefaultCellStyle.Format = format
            End If

            If rightAlign Then
                column.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight
            End If

            grid.Columns.Add(column)
        End Sub

        Private Sub FilterChanged(sender As Object, e As EventArgs)
            If isLoading OrElse Not IsHandleCreated Then Return
            LoadHistory()
        End Sub

        Private Sub LoadHistory()
            If isLoading Then Return
            isLoading = True

            Try
                If dtFrom.Checked AndAlso dtTo.Checked AndAlso
                   dtFrom.Value.Date > dtTo.Value.Date Then

                    lblResult.Text = "Check-in start date must not exceed end date."
                    Return
                End If

                Dim sql As String =
                    "SELECT " &
                    " s.stay_id, s.stay_reference, s.stay_status, " &
                    " s.checked_in_at, s.checked_out_at, " &
                    " CONCAT(g.first_name, ' ', g.last_name) AS guest_name, " &
                    " COALESCE(g.phone, '') AS phone, " &
                    " COALESCE(ra.room_numbers, '') AS room_numbers, " &
                    " COALESCE(fc.total_charges, 0.00) AS total_charges, " &
                    " COALESCE(pm.amount_paid, 0.00) AS amount_paid, " &
                    " COALESCE(fc.total_charges, 0.00) - " &
                    " COALESCE(pm.amount_paid, 0.00) AS balance " &
                    "FROM stays s " &
                    "INNER JOIN guests g ON g.guest_id = s.guest_id " &
                    "LEFT JOIN guest_folios gf ON gf.stay_id = s.stay_id " &
                    "LEFT JOIN ( " &
                    " SELECT stay_id, " &
                    " GROUP_CONCAT(DISTINCT room_number " &
                    " ORDER BY room_number SEPARATOR ', ') AS room_numbers " &
                    " FROM ( " &
                    " SELECT ra.stay_id, r.room_number " &
                    " FROM room_assignments ra " &
                    " INNER JOIN rooms r ON r.room_id = ra.room_id " &
                    " ) room_data GROUP BY stay_id " &
                    ") ra ON ra.stay_id = s.stay_id " &
                    "LEFT JOIN ( " &
                    " SELECT folio_id, SUM(amount) AS total_charges " &
                    " FROM folio_charges GROUP BY folio_id " &
                    ") fc ON fc.folio_id = gf.folio_id " &
                    "LEFT JOIN ( " &
                    " SELECT folio_id, SUM(amount) AS amount_paid " &
                    " FROM payments " &
                    " WHERE payment_status = 'COMPLETED' " &
                    " GROUP BY folio_id " &
                    ") pm ON pm.folio_id = gf.folio_id " &
                    "WHERE ( " &
                    " @search = '' OR " &
                    " s.stay_reference LIKE @pattern OR " &
                    " g.first_name LIKE @pattern OR " &
                    " g.last_name LIKE @pattern OR " &
                    " CONCAT(g.first_name, ' ', g.last_name) LIKE @pattern OR " &
                    " COALESCE(g.phone, '') LIKE @pattern " &
                    ") " &
                    "AND (@status = 'ALL' OR s.stay_status = @status) " &
                    "AND (@useFrom = 0 OR s.checked_in_at >= @fromDate) " &
                    "AND (@useTo = 0 OR s.checked_in_at < @toDate) " &
                    "ORDER BY s.checked_in_at DESC, s.stay_id DESC"

                Dim result As New DataTable()

                Using connection = DbConnectionFactory.Create()
                    connection.Open()

                    Using command As New MySqlCommand(sql, connection)
                        Dim searchText As String = txtSearch.Text.Trim()

                        command.Parameters.AddWithValue("@search", searchText)
                        command.Parameters.AddWithValue(
                            "@pattern", "%" & searchText & "%")
                        command.Parameters.AddWithValue(
                            "@status", cboStatus.SelectedItem.ToString())

                        command.Parameters.AddWithValue(
                            "@useFrom", If(dtFrom.Checked, 1, 0))
                        command.Parameters.AddWithValue(
                            "@fromDate", dtFrom.Value.Date)

                        command.Parameters.AddWithValue(
                            "@useTo", If(dtTo.Checked, 1, 0))
                        command.Parameters.AddWithValue(
                            "@toDate", dtTo.Value.Date.AddDays(1))

                        Using reader = command.ExecuteReader()
                            result.Load(reader)
                        End Using
                    End Using
                End Using

                grid.DataSource = result
                UpdateSummary()
                lblResult.Text = result.Rows.Count.ToString() & " stay record(s)"

            Catch ex As Exception
                MessageBox.Show(
                    "Unable to load guest stay history." &
                    Environment.NewLine & ex.Message,
                    "Guest Stay History",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            Finally
                isLoading = False
            End Try
        End Sub

        Private Sub UpdateSummary()
            Try
                Using connection = DbConnectionFactory.Create()
                    connection.Open()

                    Dim sql As String =
                        "SELECT " &
                        " (SELECT COUNT(*) FROM stays) AS total_stays, " &
                        " (SELECT COUNT(*) FROM stays " &
                        " WHERE stay_status = 'ACTIVE') AS active_stays, " &
                        " (SELECT COUNT(*) FROM stays " &
                        " WHERE stay_status = 'CHECKED_OUT') AS checked_out, " &
                        " (SELECT COALESCE(SUM(amount), 0) FROM payments " &
                        " WHERE payment_status = 'COMPLETED') AS total_paid"

                    Using command As New MySqlCommand(sql, connection)
                        Using reader = command.ExecuteReader()
                            If reader.Read() Then
                                lblTotal.Text =
                                    Convert.ToInt64(reader("total_stays")).ToString()

                                lblActive.Text =
                                    Convert.ToInt64(reader("active_stays")).ToString()

                                lblCompleted.Text =
                                    Convert.ToInt64(reader("checked_out")).ToString()

                                lblRevenue.Text =
                                    "₱" & Convert.ToDecimal(
                                        reader("total_paid")).ToString("N2")
                            End If
                        End Using
                    End Using
                End Using

            Catch
                lblTotal.Text = "—"
                lblActive.Text = "—"
                lblCompleted.Text = "—"
                lblRevenue.Text = "—"
            End Try
        End Sub

        Private Sub OpenSelectedDetails()
            If grid.CurrentRow Is Nothing Then Return

            Dim row As DataGridViewRow = grid.CurrentRow
            If row.IsNewRow Then Return

            Dim stayId As Long =
                Convert.ToInt64(row.Cells("stay_id").Value)

            Using details As New GuestStayDetailsForm(stayId)
                details.ShowDialog(FindForm())
            End Using
        End Sub

    End Class

    Friend NotInheritable Class GuestStayDetailsForm
        Inherits Form

        Private ReadOnly stayId As Long
        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)

        Private ReadOnly lblSummary As New Label()
        Private ReadOnly tabs As New TabControl()

        Public Sub New(id As Long)
            stayId = id

            Text = "Stay Details"
            StartPosition = FormStartPosition.CenterParent
            Size = New Size(950, 620)
            MinimumSize = New Size(720, 480)
            BackColor = Color.FromArgb(240, 244, 248)
            Font = New Font("Segoe UI", 9)

            BuildInterface()

            AddHandler Shown,
                Sub(sender As Object, e As EventArgs)
                    LoadDetails()
                End Sub
        End Sub

        Private Sub BuildInterface()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .Padding = New Padding(16)
            }

            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 90))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            lblSummary.Dock = DockStyle.Fill
            lblSummary.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            lblSummary.ForeColor = navy
            lblSummary.Text = "Loading stay information..."

            tabs.Dock = DockStyle.Fill
            tabs.Font = New Font("Segoe UI", 9)

            tabs.TabPages.Add(CreateTab("Rooms", "rooms"))
            tabs.TabPages.Add(CreateTab("Parking", "parking"))
            tabs.TabPages.Add(CreateTab("Charges", "charges"))
            tabs.TabPages.Add(CreateTab("Payments", "payments"))

            root.Controls.Add(lblSummary, 0, 0)
            root.Controls.Add(tabs, 0, 1)
            Controls.Add(root)
        End Sub

        Private Function CreateTab(
            caption As String,
            key As String) As TabPage

            Dim page As New TabPage(caption) With {
                .Name = key,
                .BackColor = Color.White,
                .Padding = New Padding(6)
            }

            Dim view As New DataGridView With {
                .Name = key & "Grid",
                .Dock = DockStyle.Fill,
                .ReadOnly = True,
                .AllowUserToAddRows = False,
                .AllowUserToDeleteRows = False,
                .RowHeadersVisible = False,
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                .BackgroundColor = Color.White,
                .BorderStyle = BorderStyle.None,
                .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                .AutoGenerateColumns = True,
                .EnableHeadersVisualStyles = False
            }

            view.ColumnHeadersDefaultCellStyle.BackColor = navy
            view.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            view.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 9, FontStyle.Bold)
            view.DefaultCellStyle.Font = New Font("Segoe UI", 9)
            view.DefaultCellStyle.ForeColor = navy
            view.ColumnHeadersHeight = 38
            view.RowTemplate.Height = 32

            page.Controls.Add(view)
            Return page
        End Function

        Private Sub LoadDetails()
            Try
                Using connection = DbConnectionFactory.Create()
                    connection.Open()

                    Dim summarySql As String =
                        "SELECT s.stay_reference, s.stay_status, " &
                        "s.checked_in_at, s.checked_out_at, " &
                        "CONCAT(g.first_name, ' ', g.last_name) AS guest_name, " &
                        "COALESCE(g.phone, '') AS phone, " &
                        "COALESCE(g.email, '') AS email, " &
                        "COALESCE(gf.folio_id, 0) AS folio_id " &
                        "FROM stays s " &
                        "INNER JOIN guests g ON g.guest_id = s.guest_id " &
                        "LEFT JOIN guest_folios gf ON gf.stay_id = s.stay_id " &
                        "WHERE s.stay_id = @stayId"

                    Dim folioId As Long = 0

                    Using command As New MySqlCommand(summarySql, connection)
                        command.Parameters.AddWithValue("@stayId", stayId)

                        Using reader = command.ExecuteReader()
                            If Not reader.Read() Then
                                lblSummary.Text = "Stay record not found."
                                Return
                            End If

                            folioId = Convert.ToInt64(reader("folio_id"))

                            Dim checkedOut As String = "Not checked out"
                            If Not Convert.IsDBNull(reader("checked_out_at")) Then
                                checkedOut = Convert.ToDateTime(
                                    reader("checked_out_at")).ToString("yyyy-MM-dd HH:mm")
                            End If

                            lblSummary.Text =
                                "Stay: " & reader("stay_reference").ToString() &
                                "    |    Guest: " & reader("guest_name").ToString() &
                                Environment.NewLine &
                                "Phone: " & reader("phone").ToString() &
                                "    |    Email: " & reader("email").ToString() &
                                Environment.NewLine &
                                "Status: " & reader("stay_status").ToString() &
                                "    |    Check-in: " &
                                Convert.ToDateTime(reader("checked_in_at")).ToString(
                                    "yyyy-MM-dd HH:mm") &
                                "    |    Check-out: " & checkedOut
                        End Using
                    End Using

                    LoadGrid(connection,
                             "rooms",
                             "SELECT r.room_number AS Room, " &
                             "ra.assigned_at AS AssignedAt, " &
                             "ra.released_at AS ReleasedAt, " &
                             "ra.hourly_rate_snapshot AS HourlyRate, " &
                             "ra.additional_hourly_rate_snapshot AS AdditionalHourlyRate, " &
                             "ra.daily_rate_snapshot AS DailyRate, " &
                             "ra.minimum_hours_snapshot AS MinimumHours " &
                             "FROM room_assignments ra " &
                             "INNER JOIN rooms r ON r.room_id = ra.room_id " &
                             "WHERE ra.stay_id = @stayId " &
                             "ORDER BY ra.assigned_at DESC",
                             stayId)

                    LoadGrid(connection,
                             "parking",
                             "SELECT ticket_reference AS Ticket, " &
                             "plate_number AS Plate, vehicle_category AS Vehicle, " &
                             "ticket_status AS Status, entered_at AS EnteredAt, " &
                             "departed_at AS DepartedAt, " &
                             "hourly_rate_snapshot AS HourlyRate " &
                             "FROM parking_tickets WHERE stay_id = @stayId " &
                             "ORDER BY entered_at DESC",
                             stayId)

                    LoadGrid(connection,
                             "charges",
                             "SELECT charge_type AS Type, description AS Description, " &
                             "quantity AS Quantity, unit_price AS UnitPrice, " &
                             "amount AS Amount, charged_at AS ChargedAt " &
                             "FROM folio_charges WHERE folio_id = @folioId " &
                             "ORDER BY charged_at DESC",
                             folioId)

                    LoadGrid(connection,
                             "payments",
                             "SELECT receipt_number AS Receipt, amount AS Amount, " &
                             "payment_method AS Method, payment_status AS Status, " &
                             "paid_at AS PaidAt, notes AS Notes " &
                             "FROM payments WHERE folio_id = @folioId " &
                             "ORDER BY paid_at DESC",
                             folioId)
                End Using

            Catch ex As Exception
                MessageBox.Show(
                    "Unable to load stay details." &
                    Environment.NewLine & ex.Message,
                    "Stay Details",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub LoadGrid(
            connection As MySqlConnection,
            gridName As String,
            sql As String,
            id As Long)

            Dim page As TabPage = tabs.TabPages(gridName)
            Dim view As DataGridView =
                DirectCast(page.Controls(gridName & "Grid"), DataGridView)

            Dim parameterName As String =
                If(gridName = "charges" OrElse gridName = "payments",
                   "@folioId", "@stayId")

            Dim data As New DataTable()

            Using command As New MySqlCommand(sql, connection)
                command.Parameters.AddWithValue(parameterName, id)

                Using reader = command.ExecuteReader()
                    data.Load(reader)
                End Using
            End Using

            view.DataSource = data

            For Each column As DataGridViewColumn In view.Columns
                If column.Name.IndexOf("Amount",
                    StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   column.Name.IndexOf("Rate",
                    StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   column.Name.IndexOf("Price",
                    StringComparison.OrdinalIgnoreCase) >= 0 Then

                    column.DefaultCellStyle.Format = "N2"
                    column.DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight
                End If
            Next
        End Sub

    End Class

End Namespace