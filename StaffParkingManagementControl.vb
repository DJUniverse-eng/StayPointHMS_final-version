
Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class StaffParkingManagementControl
        Inherits UserControl

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly sidebarColor As Color = Color.FromArgb(32, 58, 95)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly pageBg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)

        Private ReadOnly searchBox As New TextBox()
        Private ReadOnly ticketsGrid As New DataGridView()
        Private ReadOnly spacesGrid As New DataGridView()
        Private ReadOnly categoryBox As New ComboBox()
        Private ReadOnly plateBox As New TextBox()
        Private ReadOnly spaceBox As New ComboBox()
        Private ReadOnly stayBox As New ComboBox()
        Private ReadOnly notesBox As New TextBox()
        Private ReadOnly statusLabel As New Label()
        Private ReadOnly rateLabel As New Label()
        Private ReadOnly spacesSearchBox As New TextBox()

        Private ReadOnly refreshButton As New Button()
        Private ReadOnly issueButton As New Button()
        Private ReadOnly departButton As New Button()

        Private isLoading As Boolean

        Public Sub New()
            Text = "Parking Operations"
            Dock = DockStyle.Fill
            BackColor = pageBg
            Font = New Font("Segoe UI", 9.5F)
            MinimumSize = New Size(650, 450)

            BuildInterface()
            LoadReferenceData()
            LoadTickets()
            LoadSpaces()
        End Sub

        Private Sub BuildInterface()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 4,
                .BackColor = pageBg,
                .Padding = New Padding(18),
                .Margin = New Padding(0)
            }

            root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 62))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 38))

            Dim heading As New Label With {
                .Text = "Parking Tickets & Vehicle Operations",
                .AutoSize = True,
                .ForeColor = navy,
                .Font = New Font("Segoe UI", 19.0F, FontStyle.Bold),
                .Margin = New Padding(0, 0, 0, 12)
            }

            root.Controls.Add(heading, 0, 0)
            root.Controls.Add(BuildEntryPanel(), 0, 1)
            root.Controls.Add(BuildTicketsPanel(), 0, 2)
            root.Controls.Add(BuildSpacesPanel(), 0, 3)

            Controls.Add(root)
        End Sub

        Private Function BuildEntryPanel() As Control
            Dim outer As New Panel With {
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .BackColor = Color.White,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 0, 0, 12)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .ColumnCount = 4,
                .RowCount = 5,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))

            For i As Integer = 0 To 3
                layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            Next
            layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            categoryBox.DropDownStyle = ComboBoxStyle.DropDownList
            categoryBox.Items.AddRange(
                New Object() {"MOTORCYCLE", "SEDAN", "VAN", "SUV"})
            categoryBox.SelectedIndex = 0
            categoryBox.Dock = DockStyle.Fill
            categoryBox.Margin = New Padding(4, 3, 8, 8)

            plateBox.Dock = DockStyle.Fill
            plateBox.MaxLength = 20
            plateBox.Margin = New Padding(4, 3, 8, 8)

            spaceBox.DropDownStyle = ComboBoxStyle.DropDownList
            spaceBox.Dock = DockStyle.Fill
            spaceBox.Margin = New Padding(4, 3, 8, 8)

            stayBox.DropDownStyle = ComboBoxStyle.DropDownList
            stayBox.Dock = DockStyle.Fill
            stayBox.Margin = New Padding(4, 3, 8, 8)

            notesBox.Dock = DockStyle.Fill
            notesBox.MaxLength = 2000
            notesBox.Margin = New Padding(4, 3, 8, 8)

            layout.Controls.Add(MakeField("Vehicle category", categoryBox), 0, 0)
            layout.Controls.Add(MakeField("Plate number", plateBox), 1, 0)
            layout.Controls.Add(MakeField("Available parking space", spaceBox), 2, 0)
            layout.Controls.Add(MakeField("Link to active stay (optional)", stayBox), 3, 0)

            layout.Controls.Add(MakeField("Entry notes (optional)", notesBox), 0, 1)
            layout.SetColumnSpan(layout.GetControlFromPosition(0, 1), 2)

            rateLabel.Text = "Rate: loading..."
            rateLabel.AutoSize = True
            rateLabel.ForeColor = navy
            rateLabel.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
            rateLabel.Margin = New Padding(4, 8, 4, 8)
            layout.Controls.Add(rateLabel, 2, 1)

            issueButton.Text = "Record Vehicle Entry"
            StyleButton(issueButton, teal, Color.White)
            issueButton.Margin = New Padding(4, 4, 4, 8)
            AddHandler issueButton.Click, AddressOf HandleIssueTicket
            layout.Controls.Add(issueButton, 3, 1)

            refreshButton.Text = "Refresh Records"
            StyleButton(refreshButton, navy, Color.White)
            refreshButton.Margin = New Padding(4, 4, 4, 4)
            AddHandler refreshButton.Click, AddressOf HandleRefresh

            departButton.Text = "Record Departure"
            StyleButton(departButton, Color.White, navy)
            departButton.FlatAppearance.BorderSize = 1
            departButton.FlatAppearance.BorderColor = borderColor
            departButton.Margin = New Padding(4, 4, 4, 4)
            AddHandler departButton.Click, AddressOf HandleDeparture

            layout.Controls.Add(refreshButton, 0, 2)
            layout.Controls.Add(departButton, 1, 2)

            statusLabel.AutoSize = True
            statusLabel.ForeColor = muted
            statusLabel.Margin = New Padding(4, 8, 4, 0)
            statusLabel.Text = "Ready."
            layout.Controls.Add(statusLabel, 2, 2)
            layout.SetColumnSpan(statusLabel, 2)

            AddHandler categoryBox.SelectedIndexChanged,
                AddressOf HandleCategoryChanged

            outer.Controls.Add(layout)
            Return outer
        End Function

        Private Function BuildTicketsPanel() As Control
            Dim panel As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = Color.White,
                .Padding = New Padding(10),
                .Margin = New Padding(0, 0, 0, 12)
            }

            panel.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            panel.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim top As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .AutoSize = True,
                .BackColor = Color.White
            }
            top.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45))
            top.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55))

            Dim label As New Label With {
                .Text = "Parking Tickets",
                .AutoSize = True,
                .ForeColor = navy,
                .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
                .Margin = New Padding(2, 5, 8, 8)
            }

            searchBox.Dock = DockStyle.Fill
            searchBox.PlaceholderText = "Search ticket, plate, guest or space..."
            searchBox.Margin = New Padding(4, 2, 4, 8)
            AddHandler searchBox.TextChanged, AddressOf HandleSearchChanged

            top.Controls.Add(label, 0, 0)
            top.Controls.Add(searchBox, 1, 0)

            ConfigureGrid(ticketsGrid)
            ticketsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            ticketsGrid.MultiSelect = False
            ticketsGrid.ReadOnly = True
            ticketsGrid.AutoGenerateColumns = True

            panel.Controls.Add(top, 0, 0)
            panel.Controls.Add(ticketsGrid, 0, 1)

            Return panel
        End Function

        Private Function BuildSpacesPanel() As Control
            Dim panel As New TableLayoutPanel With {
        .Dock = DockStyle.Fill,
        .ColumnCount = 1,
        .RowCount = 2,
        .BackColor = Color.White,
        .Padding = New Padding(10),
        .Margin = New Padding(0)
    }

            panel.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            panel.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim top As New TableLayoutPanel With {
        .Dock = DockStyle.Fill,
        .ColumnCount = 2,
        .RowCount = 1,
        .AutoSize = True,
        .BackColor = Color.White,
        .Margin = New Padding(0)
    }

            top.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45))
            top.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55))

            Dim label As New Label With {
        .Text = "Parking Space Availability",
        .AutoSize = True,
        .ForeColor = navy,
        .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
        .Margin = New Padding(2, 5, 8, 8)
    }

            spacesSearchBox.Dock = DockStyle.Fill
            spacesSearchBox.Font = New Font("Segoe UI", 9.5F)
            spacesSearchBox.Margin = New Padding(4, 2, 4, 8)

            AddHandler spacesSearchBox.TextChanged,
        AddressOf HandleSpacesSearchChanged

            ConfigureGrid(spacesGrid)
            spacesGrid.ReadOnly = True
            spacesGrid.AutoGenerateColumns = True
            spacesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            spacesGrid.MultiSelect = False

            top.Controls.Add(label, 0, 0)
            top.Controls.Add(spacesSearchBox, 1, 0)

            panel.Controls.Add(top, 0, 0)
            panel.Controls.Add(spacesGrid, 0, 1)

            Return panel
        End Function

        Private Shared Function MakeField(
            caption As String,
            editor As Control) As Control

            Dim panel As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .AutoSize = True,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            panel.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            panel.RowStyles.Add(New RowStyle(SizeType.AutoSize))

            Dim label As New Label With {
                .Text = caption,
                .AutoSize = True,
                .ForeColor = Color.FromArgb(71, 85, 105),
                .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .Margin = New Padding(4, 2, 4, 2)
            }

            editor.MinimumSize = New Size(0, 28)

            panel.Controls.Add(label, 0, 0)
            panel.Controls.Add(editor, 0, 1)
            Return panel
        End Function

        Private Sub ConfigureGrid(grid As DataGridView)
            grid.Dock = DockStyle.Fill
            grid.BackgroundColor = Color.White
            grid.BorderStyle = BorderStyle.None
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.AllowUserToResizeRows = False
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
            grid.RowHeadersVisible = False
            grid.ReadOnly = True
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            grid.MultiSelect = False
            grid.EnableHeadersVisualStyles = False
            grid.ColumnHeadersDefaultCellStyle.BackColor = navy
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 9.0F, FontStyle.Bold)
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 241, 237)
            grid.DefaultCellStyle.SelectionForeColor = navy
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(247, 249, 252)
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
        End Sub

        Private Shared Sub StyleButton(
            button As Button,
            background As Color,
            foreground As Color)

            button.AutoSize = True
            button.MinimumSize = New Size(120, 34)
            button.Padding = New Padding(8, 4, 8, 4)
            button.BackColor = background
            button.ForeColor = foreground
            button.FlatStyle = FlatStyle.Flat
            button.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
            button.Cursor = Cursors.Hand
            button.UseVisualStyleBackColor = False
            button.FlatAppearance.BorderColor = Color.FromArgb(225, 230, 237)
        End Sub

        Private Sub LoadReferenceData()
            isLoading = True

            Try
                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Dim staysTable As New DataTable()

                    Using command As New MySqlCommand(
                        "SELECT s.stay_id, " &
                        "CONCAT(s.stay_reference, ' - ', g.first_name, ' ', " &
                        "g.last_name, ' (Guest #', g.guest_id, ')') AS stay_label " &
                        "FROM stays s " &
                        "INNER JOIN guests g ON g.guest_id = s.guest_id " &
                        "WHERE s.stay_status = 'ACTIVE' " &
                        "ORDER BY s.checked_in_at DESC;", connection)

                        Using adapter As New MySqlDataAdapter(command)
                            adapter.Fill(staysTable)
                        End Using
                    End Using

                    Dim noStay As DataRow = staysTable.NewRow()
                    noStay("stay_id") = DBNull.Value
                    noStay("stay_label") = "(No guest / stay linked)"
                    staysTable.Rows.InsertAt(noStay, 0)

                    stayBox.DataSource = staysTable
                    stayBox.DisplayMember = "stay_label"
                    stayBox.ValueMember = "stay_id"
                    stayBox.SelectedIndex = 0
                End Using

                LoadRateLabel()

            Catch ex As Exception
                SetStatus("Could not load active stays: " & ex.Message, True)
            Finally
                isLoading = False
            End Try
        End Sub

        Private Sub LoadRateLabel()
            If categoryBox.SelectedItem Is Nothing Then Return

            Try
                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Using command As New MySqlCommand(
                        "SELECT hourly_rate, minimum_charge, " &
                        "billing_increment_minutes " &
                        "FROM parking_rate_options " &
                        "WHERE vehicle_category = @category AND is_active = 1 " &
                        "LIMIT 1;", connection)

                        command.Parameters.AddWithValue(
                            "@category", categoryBox.SelectedItem.ToString())

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            If reader.Read() Then
                                Dim hourly As Decimal = reader.GetDecimal(0)
                                Dim minimum As Decimal = reader.GetDecimal(1)
                                Dim increment As Integer = reader.GetInt32(2)

                                rateLabel.Text =
                                    "₱" & hourly.ToString("N2") & "/hour | " &
                                    "Minimum ₱" & minimum.ToString("N2") &
                                    " | Increment " & increment & " min"
                            Else
                                rateLabel.Text = "No active rate configured."
                            End If
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                rateLabel.Text = "Rate unavailable."
                SetStatus("Could not load parking rate: " & ex.Message, True)
            End Try
        End Sub

        Private Sub LoadTickets()
            Try
                Dim results As New DataTable()

                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Dim sql As String =
                        "SELECT t.parking_ticket_id, t.ticket_reference, " &
                        "t.plate_number, t.vehicle_category, p.parking_code, " &
                        "t.ticket_status, t.entered_at, t.departed_at, " &
                        "t.hourly_rate_snapshot, t.minimum_charge_snapshot, " &
                        "t.billing_increment_minutes_snapshot, " &
                        "t.guest_id, t.stay_id, t.folio_charge_id, " &
                        "CONCAT(COALESCE(g.first_name, ''), ' ', " &
                        "COALESCE(g.last_name, '')) AS guest_name " &
                        "FROM parking_tickets t " &
                        "INNER JOIN parking_spaces p " &
                        "ON p.parking_space_id = t.parking_space_id " &
                        "LEFT JOIN guests g ON g.guest_id = t.guest_id " &
                        "ORDER BY CASE WHEN t.ticket_status = 'ACTIVE' " &
                        "THEN 0 ELSE 1 END, t.entered_at DESC;"

                    Using command As New MySqlCommand(sql, connection)
                        Using adapter As New MySqlDataAdapter(command)
                            adapter.Fill(results)
                        End Using
                    End Using
                End Using

                ticketsGrid.DataSource = results

                SetHeader("parking_ticket_id", "Ticket ID")
                SetHeader("ticket_reference", "Ticket Reference")
                SetHeader("plate_number", "Plate Number")
                SetHeader("vehicle_category", "Vehicle")
                SetHeader("parking_code", "Space")
                SetHeader("ticket_status", "Status")
                SetHeader("entered_at", "Entry Time")
                SetHeader("departed_at", "Departure Time")
                SetHeader("hourly_rate_snapshot", "Hourly Rate")
                SetHeader("minimum_charge_snapshot", "Minimum Charge")
                SetHeader("billing_increment_minutes_snapshot", "Billing Increment")
                SetHeader("guest_name", "Guest")
                SetHeader("guest_id", "Guest ID")
                SetHeader("stay_id", "Stay ID")
                SetHeader("folio_charge_id", "Folio Charge ID")

                HideColumn("parking_ticket_id")
                HideColumn("guest_id")
                HideColumn("stay_id")
                HideColumn("folio_charge_id")

                FilterTickets()
            Catch ex As Exception
                SetStatus("Could not load parking tickets: " & ex.Message, True)
            End Try
        End Sub

        Private Sub SetHeader(columnName As String, caption As String)
            If ticketsGrid.Columns.Contains(columnName) Then
                ticketsGrid.Columns(columnName).HeaderText = caption
            End If
        End Sub

        Private Sub HideColumn(columnName As String)
            If ticketsGrid.Columns.Contains(columnName) Then
                ticketsGrid.Columns(columnName).Visible = False
            End If
        End Sub

        Private Sub LoadSpaces()
            Try
                Dim results As New DataTable()

                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Using command As New MySqlCommand(
                        "SELECT parking_space_id, parking_code, floor_number, " &
                        "vehicle_category, area_name, status " &
                        "FROM parking_spaces " &
                        "WHERE is_active = 1 " &
                        "ORDER BY floor_number, parking_code;", connection)

                        Using adapter As New MySqlDataAdapter(command)
                            adapter.Fill(results)
                        End Using
                    End Using
                End Using

                spacesGrid.DataSource = results

                If spacesGrid.Columns.Contains("parking_space_id") Then
                    spacesGrid.Columns("parking_space_id").Visible = False
                End If

                If spacesGrid.Columns.Contains("parking_code") Then
                    spacesGrid.Columns("parking_code").HeaderText = "Space"
                End If
                If spacesGrid.Columns.Contains("floor_number") Then
                    spacesGrid.Columns("floor_number").HeaderText = "Floor"
                End If
                If spacesGrid.Columns.Contains("vehicle_category") Then
                    spacesGrid.Columns("vehicle_category").HeaderText = "Vehicle Type"
                End If
                If spacesGrid.Columns.Contains("area_name") Then
                    spacesGrid.Columns("area_name").HeaderText = "Area"
                End If
                If spacesGrid.Columns.Contains("status") Then
                    spacesGrid.Columns("status").HeaderText = "Status"
                End If

                LoadAvailableSpaces()
            Catch ex As Exception
                SetStatus("Could not load parking spaces: " & ex.Message, True)
            End Try
        End Sub

        Private Sub LoadAvailableSpaces()
            If categoryBox.SelectedItem Is Nothing Then Return

            Try
                Dim results As New DataTable()

                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Using command As New MySqlCommand(
                        "SELECT parking_space_id, " &
                        "CONCAT(parking_code, ' - Floor ', floor_number, " &
                        "CASE WHEN area_name IS NULL OR area_name = '' " &
                        "THEN '' ELSE CONCAT(' - ', area_name) END) AS space_label " &
                        "FROM parking_spaces " &
                        "WHERE is_active = 1 AND status = 'AVAILABLE' " &
                        "AND vehicle_category = @category " &
                        "AND NOT EXISTS (" &
                        "SELECT 1 FROM parking_tickets t " &
                        "WHERE t.parking_space_id = parking_spaces.parking_space_id " &
                        "AND t.ticket_status = 'ACTIVE') " &
                        "ORDER BY floor_number, parking_code;", connection)

                        command.Parameters.AddWithValue(
                            "@category", categoryBox.SelectedItem.ToString())

                        Using adapter As New MySqlDataAdapter(command)
                            adapter.Fill(results)
                        End Using
                    End Using
                End Using

                spaceBox.DataSource = results
                spaceBox.DisplayMember = "space_label"
                spaceBox.ValueMember = "parking_space_id"
                spaceBox.SelectedIndex = If(results.Rows.Count > 0, 0, -1)

                issueButton.Enabled = results.Rows.Count > 0

                If results.Rows.Count = 0 Then
                    SetStatus("No available spaces for this vehicle category.")
                End If
            Catch ex As Exception
                SetStatus("Could not load available spaces: " & ex.Message, True)
            End Try
        End Sub

        Private Sub HandleCategoryChanged(sender As Object, e As EventArgs)
            If isLoading Then Return
            LoadRateLabel()
            LoadAvailableSpaces()
        End Sub

        Private Sub HandleSearchChanged(sender As Object, e As EventArgs)
            FilterTickets()
        End Sub

        Private Sub HandleSpacesSearchChanged(sender As Object, e As EventArgs)
            FilterSpaces()
        End Sub

        Private Sub FilterTickets()
            ApplyGridSearch(
        ticketsGrid,
        searchBox.Text,
        {
            "ticket_reference",
            "plate_number",
            "parking_code",
            "guest_name",
            "vehicle_category",
            "ticket_status"
        })
        End Sub

        Private Sub FilterSpaces()
            ApplyGridSearch(
        spacesGrid,
        spacesSearchBox.Text,
        {
            "parking_code",
            "floor_number",
            "vehicle_category",
            "area_name",
            "status"
        })
        End Sub

        Private Sub ApplyGridSearch(
    grid As DataGridView,
    searchText As String,
    searchableColumns As String())

            Dim table As DataTable = TryCast(grid.DataSource, DataTable)
            If table Is Nothing Then Return

            Dim query As String = searchText.Trim()

            If query.Length = 0 Then
                table.DefaultView.RowFilter = String.Empty
                Return
            End If

            ' Escape characters that have special meaning in DataView expressions.
            query = query.Replace("'", "''")
            query = query.Replace("[", "[[]")
            query = query.Replace("%", "[%]")
            query = query.Replace("*", "[*]")

            Dim conditions As New List(Of String)()

            For Each columnName As String In searchableColumns
                If table.Columns.Contains(columnName) Then
                    conditions.Add(
                "Convert([" & columnName.Replace("]", "]]") &
                "], 'System.String') LIKE '%" & query & "%'")
                End If
            Next

            If conditions.Count = 0 Then
                table.DefaultView.RowFilter = String.Empty
                Return
            End If

            Try
                table.DefaultView.RowFilter = String.Join(" OR ", conditions)
            Catch ex As EvaluateException
                table.DefaultView.RowFilter = String.Empty
                SetStatus("Unable to apply the search filter.", True)
            End Try
        End Sub

        Private Sub HandleRefresh(sender As Object, e As EventArgs)
            LoadReferenceData()
            LoadTickets()
            LoadSpaces()
            SetStatus("Parking records refreshed.")
        End Sub

        Private Sub HandleIssueTicket(sender As Object, e As EventArgs)
            Dim plate As String = plateBox.Text.Trim().ToUpperInvariant()

            If plate.Length = 0 Then
                MessageBox.Show("Enter the vehicle plate number.",
                                "Parking Entry",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
                plateBox.Focus()
                Return
            End If

            If categoryBox.SelectedItem Is Nothing OrElse
               spaceBox.SelectedValue Is Nothing Then
                MessageBox.Show("Select a vehicle category and available parking space.",
                                "Parking Entry",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
                Return
            End If

            Dim category As String = categoryBox.SelectedItem.ToString()
            Dim spaceId As Long

            If Not Long.TryParse(
                Convert.ToString(spaceBox.SelectedValue, CultureInfo.InvariantCulture),
                spaceId) Then
                MessageBox.Show("The selected parking space is invalid.")
                Return
            End If

            Dim stayId As Long? = Nothing
            If stayBox.SelectedValue IsNot Nothing AndAlso
               stayBox.SelectedValue IsNot DBNull.Value Then

                Dim parsedStayId As Long
                If Long.TryParse(
                    Convert.ToString(stayBox.SelectedValue, CultureInfo.InvariantCulture),
                    parsedStayId) Then
                    stayId = parsedStayId
                End If
            End If

            Dim ticketReference As String =
                "PK" & DateTime.Now.ToString("yyMMddHHmmss") &
                Guid.NewGuid().ToString("N").Substring(0, 4).ToUpperInvariant()

            Try
                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Using transaction As MySqlTransaction = connection.BeginTransaction()
                        Dim hourlyRate As Decimal
                        Dim minimumCharge As Decimal
                        Dim increment As Integer

                        Using rateCommand As New MySqlCommand(
                            "SELECT hourly_rate, minimum_charge, " &
                            "billing_increment_minutes " &
                            "FROM parking_rate_options " &
                            "WHERE vehicle_category = @category AND is_active = 1 " &
                            "LIMIT 1 FOR UPDATE;", connection, transaction)

                            rateCommand.Parameters.AddWithValue("@category", category)

                            Using reader As MySqlDataReader = rateCommand.ExecuteReader()
                                If Not reader.Read() Then
                                    Throw New InvalidOperationException(
                                        "No active parking rate exists for " & category & ".")
                                End If

                                hourlyRate = reader.GetDecimal(0)
                                minimumCharge = reader.GetDecimal(1)
                                increment = reader.GetInt32(2)
                            End Using
                        End Using

                        If increment <= 0 Then
                            Throw New InvalidOperationException(
                                "The configured billing increment must be greater than zero.")
                        End If

                        Dim guestId As Long? = Nothing

                        If stayId.HasValue Then
                            Using stayCommand As New MySqlCommand(
                                "SELECT guest_id FROM stays " &
                                "WHERE stay_id = @stayId AND stay_status = 'ACTIVE' " &
                                "FOR UPDATE;", connection, transaction)

                                stayCommand.Parameters.AddWithValue("@stayId", stayId.Value)
                                Dim value As Object = stayCommand.ExecuteScalar()

                                If value Is Nothing OrElse value Is DBNull.Value Then
                                    Throw New InvalidOperationException(
                                        "The selected stay is no longer active.")
                                End If

                                guestId = Convert.ToInt64(value, CultureInfo.InvariantCulture)
                            End Using
                        End If

                        Using spaceCommand As New MySqlCommand(
                            "SELECT status, vehicle_category, is_active " &
                            "FROM parking_spaces " &
                            "WHERE parking_space_id = @spaceId FOR UPDATE;",
                            connection, transaction)

                            spaceCommand.Parameters.AddWithValue("@spaceId", spaceId)

                            Using reader As MySqlDataReader = spaceCommand.ExecuteReader()
                                If Not reader.Read() Then
                                    Throw New InvalidOperationException(
                                        "The selected parking space no longer exists.")
                                End If

                                If Convert.ToString(reader("status")) <> "AVAILABLE" OrElse
                                   Convert.ToInt32(reader("is_active")) <> 1 OrElse
                                   Convert.ToString(reader("vehicle_category")) <> category Then

                                    Throw New InvalidOperationException(
                                        "The selected space is no longer available for this vehicle.")
                                End If
                            End Using
                        End Using

                        Using duplicateCommand As New MySqlCommand(
                            "SELECT COUNT(*) FROM parking_tickets " &
                            "WHERE parking_space_id = @spaceId AND ticket_status = 'ACTIVE';",
                            connection, transaction)

                            duplicateCommand.Parameters.AddWithValue("@spaceId", spaceId)

                            If Convert.ToInt32(duplicateCommand.ExecuteScalar()) > 0 Then
                                Throw New InvalidOperationException(
                                    "An active ticket already occupies this parking space.")
                            End If
                        End Using

                        Using plateCommand As New MySqlCommand(
                            "SELECT COUNT(*) FROM parking_tickets " &
                            "WHERE plate_number = @plate AND ticket_status = 'ACTIVE';",
                            connection, transaction)

                            plateCommand.Parameters.AddWithValue("@plate", plate)

                            If Convert.ToInt32(plateCommand.ExecuteScalar()) > 0 Then
                                Throw New InvalidOperationException(
                                    "This plate number already has an active parking ticket.")
                            End If
                        End Using

                        Using insertCommand As New MySqlCommand(
                            "INSERT INTO parking_tickets " &
                            "(ticket_reference, parking_space_id, vehicle_category, " &
                            "plate_number, guest_id, stay_id, ticket_status, entered_at, " &
                            "hourly_rate_snapshot, minimum_charge_snapshot, " &
                            "billing_increment_minutes_snapshot, issued_by, notes) " &
                            "VALUES (@reference, @spaceId, @category, @plate, @guestId, " &
                            "@stayId, 'ACTIVE', NOW(), @rate, @minimum, @increment, " &
                            "@staffId, @notes);", connection, transaction)

                            insertCommand.Parameters.AddWithValue("@reference", ticketReference)
                            insertCommand.Parameters.AddWithValue("@spaceId", spaceId)
                            insertCommand.Parameters.AddWithValue("@category", category)
                            insertCommand.Parameters.AddWithValue("@plate", plate)
                            insertCommand.Parameters.AddWithValue(
                                "@guestId", If(guestId.HasValue,
                                    CType(guestId.Value, Object), DBNull.Value))
                            insertCommand.Parameters.AddWithValue(
                                "@stayId", If(stayId.HasValue,
                                    CType(stayId.Value, Object), DBNull.Value))
                            insertCommand.Parameters.AddWithValue("@rate", hourlyRate)
                            insertCommand.Parameters.AddWithValue("@minimum", minimumCharge)
                            insertCommand.Parameters.AddWithValue("@increment", increment)
                            insertCommand.Parameters.AddWithValue("@staffId", CurrentStaffId())
                            insertCommand.Parameters.AddWithValue(
                                "@notes", If(String.IsNullOrWhiteSpace(notesBox.Text),
                                    CType(DBNull.Value, Object), notesBox.Text.Trim()))

                            insertCommand.ExecuteNonQuery()
                        End Using

                        Using updateSpace As New MySqlCommand(
                            "UPDATE parking_spaces SET status = 'OCCUPIED' " &
                            "WHERE parking_space_id = @spaceId AND status = 'AVAILABLE' " &
                            "AND is_active = 1;", connection, transaction)

                            updateSpace.Parameters.AddWithValue("@spaceId", spaceId)

                            If updateSpace.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException(
                                    "The space availability changed. Refresh and try again.")
                            End If
                        End Using

                        transaction.Commit()
                    End Using
                End Using

                plateBox.Clear()
                notesBox.Clear()
                LoadTickets()
                LoadSpaces()
                SetStatus("Vehicle entry recorded. Ticket: " & ticketReference)

                MessageBox.Show(
                    "Vehicle entry recorded." & Environment.NewLine &
                    "Ticket reference: " & ticketReference,
                    "Parking Entry",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

            Catch ex As Exception
                SetStatus("Vehicle entry failed: " & ex.Message, True)
                MessageBox.Show(ex.Message, "Parking Entry",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Sub HandleDeparture(sender As Object, e As EventArgs)
            If ticketsGrid.CurrentRow Is Nothing Then
                MessageBox.Show("Select an active parking ticket first.",
                                "Vehicle Departure",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information)
                Return
            End If

            Dim row As DataGridViewRow = ticketsGrid.CurrentRow
            Dim ticketId As Long
            Dim ticketStatus As String = Convert.ToString(row.Cells("ticket_status").Value)
            Dim reference As String = Convert.ToString(row.Cells("ticket_reference").Value)

            If Not Long.TryParse(
                Convert.ToString(row.Cells("parking_ticket_id").Value,
                                 CultureInfo.InvariantCulture), ticketId) Then
                MessageBox.Show("The selected ticket ID is invalid.")
                Return
            End If

            If ticketStatus <> "ACTIVE" Then
                MessageBox.Show("Only active tickets can be closed.",
                                "Vehicle Departure",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information)
                Return
            End If

            If MessageBox.Show(
                "Record departure for ticket " & reference & " now?",
                "Vehicle Departure",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            Try
                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Using transaction As MySqlTransaction = connection.BeginTransaction()
                        Dim spaceId As Long
                        Dim stayId As Long? = Nothing
                        Dim enteredAt As DateTime
                        Dim hourlyRate As Decimal
                        Dim minimumCharge As Decimal
                        Dim increment As Integer
                        Dim existingChargeId As Long? = Nothing

                        Using ticketCommand As New MySqlCommand(
                            "SELECT parking_space_id, stay_id, entered_at, " &
                            "hourly_rate_snapshot, minimum_charge_snapshot, " &
                            "billing_increment_minutes_snapshot, folio_charge_id " &
                            "FROM parking_tickets " &
                            "WHERE parking_ticket_id = @ticketId " &
                            "AND ticket_status = 'ACTIVE' FOR UPDATE;",
                            connection, transaction)

                            ticketCommand.Parameters.AddWithValue("@ticketId", ticketId)

                            Using reader As MySqlDataReader = ticketCommand.ExecuteReader()
                                If Not reader.Read() Then
                                    Throw New InvalidOperationException(
                                        "This ticket is no longer active. Refresh the records.")
                                End If

                                spaceId = reader.GetInt64(0)
                                If Not reader.IsDBNull(1) Then
                                    stayId = reader.GetInt64(1)
                                End If

                                enteredAt = reader.GetDateTime(2)
                                hourlyRate = reader.GetDecimal(3)
                                minimumCharge = reader.GetDecimal(4)
                                increment = reader.GetInt32(5)

                                If Not reader.IsDBNull(6) Then
                                    existingChargeId = reader.GetInt64(6)
                                End If
                            End Using
                        End Using

                        If increment <= 0 Then
                            Throw New InvalidOperationException(
                                "Ticket has an invalid billing increment.")
                        End If

                        Dim departedAt As DateTime = DateTime.Now
                        Dim elapsedMinutes As Double =
                            Math.Max(0.0, (departedAt - enteredAt).TotalMinutes)

                        Dim billedIncrements As Decimal =
                            CDec(Math.Ceiling(elapsedMinutes / CDbl(increment)))

                        Dim incrementRate As Decimal =
                            hourlyRate * CDec(increment) / 60D

                        Dim calculatedCharge As Decimal =
                            Decimal.Round(billedIncrements * incrementRate,
                                          2, MidpointRounding.AwayFromZero)

                        Dim charge As Decimal = Math.Max(
                            minimumCharge, calculatedCharge)

                        Dim folioChargeId As Long? = existingChargeId
                        Dim chargeNote As String = "No folio charge created."

                        If stayId.HasValue AndAlso Not existingChargeId.HasValue Then
                            Dim folioId As Long? = Nothing

                            Using folioCommand As New MySqlCommand(
                                "SELECT folio_id FROM guest_folios " &
                                "WHERE stay_id = @stayId AND folio_status = 'OPEN' " &
                                "LIMIT 1 FOR UPDATE;", connection, transaction)

                                folioCommand.Parameters.AddWithValue("@stayId", stayId.Value)
                                Dim value As Object = folioCommand.ExecuteScalar()

                                If value IsNot Nothing AndAlso value IsNot DBNull.Value Then
                                    folioId = Convert.ToInt64(value, CultureInfo.InvariantCulture)
                                End If
                            End Using

                            If folioId.HasValue Then
                                Using insertCharge As New MySqlCommand(
                                    "INSERT INTO folio_charges " &
                                    "(folio_id, charge_type, description, quantity, " &
                                    "unit_price, amount, source_ticket_id, charged_at, charged_by) " &
                                    "VALUES (@folioId, 'PARKING', @description, " &
                                    "@quantity, @unitPrice, @amount, @ticketId, NOW(), @staffId);",
                                    connection, transaction)

                                    insertCharge.Parameters.AddWithValue("@folioId", folioId.Value)
                                    insertCharge.Parameters.AddWithValue(
                                        "@description", "Parking ticket " & reference)
                                    insertCharge.Parameters.AddWithValue("@quantity", 1D)
                                    insertCharge.Parameters.AddWithValue("@unitPrice", charge)
                                    insertCharge.Parameters.AddWithValue("@amount", charge)
                                    insertCharge.Parameters.AddWithValue("@ticketId", ticketId)
                                    insertCharge.Parameters.AddWithValue("@staffId", CurrentStaffId())

                                    insertCharge.ExecuteNonQuery()
                                    folioChargeId = insertCharge.LastInsertedId
                                    chargeNote = "Parking charge added to the open guest folio."
                                End Using
                            Else
                                chargeNote =
                                    "No open folio found. Departure saved; collect the parking fee " &
                                    "through the approved payment workflow."
                            End If
                        End If

                        Using closeTicket As New MySqlCommand(
                            "UPDATE parking_tickets SET ticket_status = 'DEPARTED', " &
                            "departed_at = @departedAt, closed_by = @staffId, " &
                            "folio_charge_id = @chargeId " &
                            "WHERE parking_ticket_id = @ticketId " &
                            "AND ticket_status = 'ACTIVE';", connection, transaction)

                            closeTicket.Parameters.AddWithValue("@departedAt", departedAt)
                            closeTicket.Parameters.AddWithValue("@staffId", CurrentStaffId())
                            closeTicket.Parameters.AddWithValue(
                                "@chargeId", If(folioChargeId.HasValue,
                                    CType(folioChargeId.Value, Object), DBNull.Value))
                            closeTicket.Parameters.AddWithValue("@ticketId", ticketId)

                            If closeTicket.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException(
                                    "Ticket status changed before departure could be saved.")
                            End If
                        End Using

                        Using freeSpace As New MySqlCommand(
                            "UPDATE parking_spaces SET status = 'AVAILABLE' " &
                            "WHERE parking_space_id = @spaceId AND status = 'OCCUPIED' " &
                            "AND is_active = 1;", connection, transaction)

                            freeSpace.Parameters.AddWithValue("@spaceId", spaceId)

                            If freeSpace.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException(
                                    "The parking space status changed. The departure was not saved.")
                            End If
                        End Using

                        transaction.Commit()

                        Dim message As String =
                            "Ticket: " & reference & Environment.NewLine &
                            "Entry: " & enteredAt.ToString("yyyy-MM-dd HH:mm:ss") &
                            Environment.NewLine &
                            "Departure: " & departedAt.ToString("yyyy-MM-dd HH:mm:ss") &
                            Environment.NewLine &
                            "Billable duration: " &
                            Math.Ceiling(elapsedMinutes).ToString("N0") & " minutes" &
                            Environment.NewLine &
                            "Parking charge: ₱" & charge.ToString("N2") &
                            Environment.NewLine & Environment.NewLine & chargeNote

                        SetStatus("Departure recorded. Charge: ₱" & charge.ToString("N2"))
                        MessageBox.Show(message, "Departure Summary",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End Using
                End Using

                LoadTickets()
                LoadSpaces()

            Catch ex As Exception
                SetStatus("Departure failed: " & ex.Message, True)
                MessageBox.Show(ex.Message, "Vehicle Departure",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Shared Function CurrentStaffId() As Object
            If CurrentUser.UserId > 0 Then
                Return CurrentUser.UserId
            End If

            Return DBNull.Value
        End Function

        Private Sub SetStatus(message As String, Optional isError As Boolean = False)
            statusLabel.Text = message
            statusLabel.ForeColor = If(isError,
                                       Color.Firebrick,
                                       muted)
        End Sub

    End Class

End Namespace
