
Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class StaffRoomAssignmentControl
        Inherits UserControl

        Private ReadOnly _root As New TableLayoutPanel()
        Private ReadOnly _header As New Panel()
        Private ReadOnly _title As New Label()
        Private ReadOnly _subtitle As New Label()
        Private ReadOnly _search As New TextBox()
        Private ReadOnly _stayPicker As New ComboBox()
        Private ReadOnly _roomsGrid As New DataGridView()
        Private ReadOnly _assignButton As New Button()
        Private ReadOnly _refreshButton As New Button()
        Private ReadOnly _status As New Label()
        Private ReadOnly _summary As New Label()

        Private ReadOnly _navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly _teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly _background As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly _border As Color = Color.FromArgb(226, 232, 240)
        Private ReadOnly _muted As Color = Color.FromArgb(100, 116, 139)

        Private _loading As Boolean

        Public Sub New()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.StaffRoom)

            BuildLayout()
            AddHandler Load, AddressOf ControlLoaded
        End Sub

        Private Sub BuildLayout()
            Dock = DockStyle.Fill
            BackColor = _background
            MinimumSize = New Size(700, 450)
            Padding = New Padding(20)

            _root.Dock = DockStyle.Fill
            _root.ColumnCount = 1
            _root.RowCount = 4
            _root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            _root.RowStyles.Add(New RowStyle(SizeType.Absolute, 66))
            _root.RowStyles.Add(New RowStyle(SizeType.Absolute, 126))
            _root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            _root.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
            _root.Margin = New Padding(0)
            Controls.Add(_root)

            BuildHeader()
            BuildFilters()
            BuildGrid()
            BuildFooter()
        End Sub

        Private Sub BuildHeader()
            _header.Dock = DockStyle.Fill
            _header.BackColor = _background
            _header.Margin = New Padding(0, 0, 0, 12)

            _title.Text = "Room Assignment"
            _title.Font = New Font("Segoe UI", 19.0F, FontStyle.Bold)
            _title.ForeColor = _navy
            _title.AutoSize = True
            _title.Location = New Point(0, 0)

            _subtitle.Text = "Find available rooms and assign them to active stays."
            _subtitle.Font = New Font("Segoe UI", 9.0F)
            _subtitle.ForeColor = _muted
            _subtitle.AutoSize = True
            _subtitle.Location = New Point(2, 37)

            _header.Controls.Add(_title)
            _header.Controls.Add(_subtitle)
            _root.Controls.Add(_header, 0, 0)
        End Sub

        Private Sub BuildFilters()
            Dim panel As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 4,
                .RowCount = 3,
                .BackColor = Color.White,
                .Padding = New Padding(14),
                .Margin = New Padding(0, 0, 0, 12)
            }

            panel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38))
            panel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38))
            panel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12))
            panel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 12))
            panel.RowStyles.Add(New RowStyle(SizeType.Absolute, 23))
            panel.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
            panel.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim searchLabel As New Label With {
                .Text = "Search rooms",
                .Dock = DockStyle.Fill,
                .ForeColor = _navy,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft
            }

            Dim stayLabel As New Label With {
                .Text = "Active stay",
                .Dock = DockStyle.Fill,
                .ForeColor = _navy,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft
            }

            _search.Dock = DockStyle.Fill
            _search.Font = New Font("Segoe UI", 10.0F)
            _search.Margin = New Padding(0, 2, 10, 0)

            _stayPicker.Dock = DockStyle.Fill
            _stayPicker.DropDownStyle = ComboBoxStyle.DropDownList
            _stayPicker.Font = New Font("Segoe UI", 10.0F)
            _stayPicker.Margin = New Padding(0, 2, 10, 0)

            StyleButton(_refreshButton, "Refresh", _navy)
            StyleButton(_assignButton, "Assign Room", _teal)

            _refreshButton.Dock = DockStyle.Fill
            _refreshButton.Margin = New Padding(0, 2, 6, 0)
            _assignButton.Dock = DockStyle.Fill
            _assignButton.Margin = New Padding(0, 2, 0, 0)

            panel.Controls.Add(searchLabel, 0, 0)
            panel.Controls.Add(stayLabel, 1, 0)
            panel.Controls.Add(_search, 0, 1)
            panel.Controls.Add(_stayPicker, 1, 1)
            panel.Controls.Add(_refreshButton, 2, 1)
            panel.Controls.Add(_assignButton, 3, 1)

            _summary.Text = "Loading room inventory..."
            _summary.Dock = DockStyle.Fill
            _summary.ForeColor = _muted
            _summary.Font = New Font("Segoe UI", 9.0F)
            _summary.TextAlign = ContentAlignment.MiddleLeft
            panel.Controls.Add(_summary, 0, 2)
            panel.SetColumnSpan(_summary, 4)

            AddHandler _search.TextChanged, AddressOf SearchChanged
            AddHandler _refreshButton.Click, AddressOf RefreshClicked
            AddHandler _assignButton.Click, AddressOf AssignClicked

            _root.Controls.Add(panel, 0, 1)
        End Sub

        Private Sub BuildGrid()
            _roomsGrid.Dock = DockStyle.Fill
            _roomsGrid.BackgroundColor = Color.White
            _roomsGrid.BorderStyle = BorderStyle.FixedSingle
            _roomsGrid.AllowUserToAddRows = False
            _roomsGrid.AllowUserToDeleteRows = False
            _roomsGrid.AllowUserToResizeRows = False
            _roomsGrid.ReadOnly = True
            _roomsGrid.MultiSelect = False
            _roomsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            _roomsGrid.AutoGenerateColumns = True
            _roomsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            _roomsGrid.RowHeadersVisible = False
            _roomsGrid.EnableHeadersVisualStyles = False
            _roomsGrid.ColumnHeadersDefaultCellStyle.BackColor = _navy
            _roomsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            _roomsGrid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 9.0F, FontStyle.Bold)
            _roomsGrid.DefaultCellStyle.Font = New Font("Segoe UI", 9.0F)
            _roomsGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 242, 238)
            _roomsGrid.DefaultCellStyle.SelectionForeColor = _navy
            _roomsGrid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252)

            _root.Controls.Add(_roomsGrid, 0, 2)
        End Sub

        Private Sub BuildFooter()
            _status.Dock = DockStyle.Fill
            _status.ForeColor = _muted
            _status.Font = New Font("Segoe UI", 9.0F)
            _status.TextAlign = ContentAlignment.MiddleLeft
            _status.Text = "Ready."

            _root.Controls.Add(_status, 0, 3)
        End Sub

        Private Sub StyleButton(button As Button, caption As String, color As Color)
            button.Text = caption
            button.BackColor = color
            button.ForeColor = Color.White
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderSize = 0
            button.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
            button.Cursor = Cursors.Hand
            button.UseVisualStyleBackColor = False
        End Sub

        Private Sub ControlLoaded(sender As Object, e As EventArgs)
            RefreshData()
        End Sub

        Private Sub RefreshClicked(sender As Object, e As EventArgs)
            RefreshData()
        End Sub

        Private Sub SearchChanged(sender As Object, e As EventArgs)
            If Not _loading Then LoadRooms()
        End Sub

        Private Sub RefreshData()
            If _loading Then Return

            _loading = True
            Try
                LoadStays()
                LoadRooms()
            Catch ex As Exception
                ShowError("Unable to load room assignment data.", ex)
            Finally
                _loading = False
            End Try
        End Sub

        Private Sub LoadStays()
            Dim previousId As Long = 0

            If _stayPicker.SelectedValue IsNot Nothing Then
                Long.TryParse(Convert.ToString(_stayPicker.SelectedValue), previousId)
            End If

            Dim table As New DataTable()

            Using connection As MySqlConnection = DbConnectionFactory.Create()
                connection.Open()

                Const sql As String =
                    "SELECT s.stay_id, " &
                    "CONCAT(s.stay_reference, ' - ', g.last_name, ', ', g.first_name) AS stay_label " &
                    "FROM stays s " &
                    "INNER JOIN guests g ON g.guest_id = s.guest_id " &
                    "WHERE s.stay_status = 'ACTIVE' " &
                    "ORDER BY s.checked_in_at DESC, s.stay_reference"

                Using command As New MySqlCommand(sql, connection)
                    Using adapter As New MySqlDataAdapter(command)
                        adapter.Fill(table)
                    End Using
                End Using
            End Using

            _stayPicker.DataSource = Nothing
            _stayPicker.DisplayMember = "stay_label"
            _stayPicker.ValueMember = "stay_id"
            _stayPicker.DataSource = table

            If previousId > 0 AndAlso table.Rows.Count > 0 Then
                For Each item As DataRowView In _stayPicker.Items
                    If Convert.ToInt64(item("stay_id")) = previousId Then
                        _stayPicker.SelectedValue = previousId
                        Exit For
                    End If
                Next
            End If

            If table.Rows.Count = 0 Then
                _stayPicker.SelectedIndex = -1
                _status.Text = "No active stays found. Register/check in a guest first."
            End If
        End Sub

        Private Sub LoadRooms()
            Dim table As New DataTable()
            Dim searchText As String = _search.Text.Trim()

            Using connection As MySqlConnection = DbConnectionFactory.Create()
                connection.Open()

                Const sql As String =
                    "SELECT r.room_id, " &
                    "r.room_number AS `Room`, " &
                    "COALESCE(rt.type_name, 'Unknown') AS `Room Type`, " &
                    "COALESCE(ro.bed_name, 'Unspecified') AS `Bed`, " &
                    "r.floor_number AS `Floor`, " &
                    "r.capacity AS `Capacity`, " &
                    "UPPER(r.status) AS `Status`, " &
                    "COALESCE(ro.hourly_rate, 0) AS `Hourly Rate`, " &
                    "COALESCE(ro.additional_hourly_rate, 0) AS `Additional Hourly`, " &
                    "COALESCE(ro.daily_rate, 0) AS `Daily Rate`, " &
                    "COALESCE(ro.minimum_hours, 1) AS `Minimum Hours`, " &
                    "ro.option_id AS rate_option_id " &
                    "FROM rooms r " &
                    "INNER JOIN room_types rt ON rt.room_type_id = r.room_type_id " &
                    "LEFT JOIN room_type_options ro " &
                    "ON ro.room_type_id = r.room_type_id " &
                    "AND ro.bed_code = r.bed_code AND ro.is_active = 1 " &
                    "WHERE r.is_active = 1 " &
                    "AND (@search = '' OR r.room_number LIKE @pattern " &
                    "OR rt.type_name LIKE @pattern OR ro.bed_name LIKE @pattern " &
                    "OR CAST(r.floor_number AS CHAR) LIKE @pattern " &
                    "OR r.status LIKE @pattern) " &
                    "ORDER BY r.floor_number, r.room_number"

                Using command As New MySqlCommand(sql, connection)
                    command.Parameters.Add("@search", MySqlDbType.VarChar, 100).Value = searchText
                    command.Parameters.Add("@pattern", MySqlDbType.VarChar, 210).Value =
                        "%" & searchText & "%"

                    Using adapter As New MySqlDataAdapter(command)
                        adapter.Fill(table)
                    End Using
                End Using
            End Using

            _roomsGrid.DataSource = table

            If _roomsGrid.Columns.Contains("room_id") Then
                _roomsGrid.Columns("room_id").Visible = False
            End If

            If _roomsGrid.Columns.Contains("rate_option_id") Then
                _roomsGrid.Columns("rate_option_id").Visible = False
            End If

            If _roomsGrid.Columns.Contains("Hourly Rate") Then
                _roomsGrid.Columns("Hourly Rate").DefaultCellStyle.Format = "₱#,##0.00"
            End If

            If _roomsGrid.Columns.Contains("Additional Hourly") Then
                _roomsGrid.Columns("Additional Hourly").DefaultCellStyle.Format = "₱#,##0.00"
            End If

            If _roomsGrid.Columns.Contains("Daily Rate") Then
                _roomsGrid.Columns("Daily Rate").DefaultCellStyle.Format = "₱#,##0.00"
            End If

            Dim availableCount As Integer = 0
            Dim occupiedCount As Integer = 0
            Dim otherCount As Integer = 0

            For Each row As DataRow In table.Rows
                Select Case Convert.ToString(row("Status")).ToUpperInvariant()
                    Case "AVAILABLE"
                        availableCount += 1
                    Case "OCCUPIED"
                        occupiedCount += 1
                    Case Else
                        otherCount += 1
                End Select
            Next

            _summary.Text =
                $"Rooms shown: {table.Rows.Count}    Available: {availableCount}    " &
                $"Occupied: {occupiedCount}    Other status: {otherCount}"
        End Sub

        Private Sub AssignClicked(sender As Object, e As EventArgs)
            If _stayPicker.SelectedValue Is Nothing Then
                MessageBox.Show(
                    "Select an active stay before assigning a room.",
                    "Room Assignment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
                Return
            End If

            If _roomsGrid.CurrentRow Is Nothing Then
                MessageBox.Show(
                    "Select a room from the room list.",
                    "Room Assignment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
                Return
            End If

            Dim selectedRoomId As Long
            Dim selectedStayId As Long

            If Not Long.TryParse(
                Convert.ToString(_roomsGrid.CurrentRow.Cells("room_id").Value),
                selectedRoomId) Then
                MessageBox.Show("The selected room ID is invalid.")
                Return
            End If

            If Not Long.TryParse(
                Convert.ToString(_stayPicker.SelectedValue),
                selectedStayId) Then
                MessageBox.Show("The selected stay ID is invalid.")
                Return
            End If

            Dim confirmation As DialogResult = MessageBox.Show(
                "Assign the selected room to the selected active stay?",
                "Confirm Room Assignment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

            If confirmation <> DialogResult.Yes Then Return

            Try
                PerformAssignment(selectedRoomId, selectedStayId)

                MessageBox.Show(
                    "Room assignment saved successfully.",
                    "Room Assignment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

                RefreshData()
                _status.Text = "Room assignment completed."

            Catch ex As Exception
                ShowError("The room could not be assigned.", ex)
                RefreshData()
            End Try
        End Sub

        Private Sub PerformAssignment(roomId As Long, stayId As Long)
            Using connection As MySqlConnection = DbConnectionFactory.Create()
                connection.Open()

                Using transaction As MySqlTransaction = connection.BeginTransaction()
                    Try
                        Dim roomStatus As String = ""
                        Dim roomTypeId As Integer
                        Dim bedCode As Integer
                        Dim isActive As Boolean

                        Const roomSql As String =
                            "SELECT status, room_type_id, bed_code, is_active " &
                            "FROM rooms WHERE room_id = @roomId FOR UPDATE"

                        Using command As New MySqlCommand(roomSql, connection, transaction)
                            command.Parameters.Add("@roomId", MySqlDbType.UInt64).Value = roomId

                            Using reader As MySqlDataReader = command.ExecuteReader()
                                If Not reader.Read() Then
                                    Throw New InvalidOperationException("The selected room no longer exists.")
                                End If

                                roomStatus = Convert.ToString(reader("status")).ToUpperInvariant()
                                roomTypeId = Convert.ToInt32(reader("room_type_id"))
                                bedCode = Convert.ToInt32(reader("bed_code"))
                                isActive = Convert.ToBoolean(reader("is_active"))
                            End Using
                        End Using

                        If Not isActive OrElse roomStatus <> "AVAILABLE" Then
                            Throw New InvalidOperationException(
                                "This room is no longer available. Refresh the list and select another room.")
                        End If

                        Dim stayActive As Boolean

                        Const staySql As String =
                            "SELECT stay_status FROM stays WHERE stay_id = @stayId FOR UPDATE"

                        Using command As New MySqlCommand(staySql, connection, transaction)
                            command.Parameters.Add("@stayId", MySqlDbType.UInt64).Value = stayId

                            Dim result As Object = command.ExecuteScalar()

                            If result Is Nothing OrElse result Is DBNull.Value Then
                                Throw New InvalidOperationException("The selected stay no longer exists.")
                            End If

                            stayActive =
                                String.Equals(
                                    Convert.ToString(result),
                                    "ACTIVE",
                                    StringComparison.OrdinalIgnoreCase)
                        End Using

                        If Not stayActive Then
                            Throw New InvalidOperationException(
                                "Only active stays can receive a room assignment.")
                        End If

                        Const existingRoomSql As String =
                            "SELECT COUNT(*) FROM room_assignments " &
                            "WHERE room_id = @roomId AND released_at IS NULL"

                        Using command As New MySqlCommand(existingRoomSql, connection, transaction)
                            command.Parameters.Add("@roomId", MySqlDbType.UInt64).Value = roomId

                            If Convert.ToInt32(command.ExecuteScalar()) > 0 Then
                                Throw New InvalidOperationException(
                                    "This room already has an active assignment.")
                            End If
                        End Using

                        Const existingStaySql As String =
                            "SELECT COUNT(*) FROM room_assignments " &
                            "WHERE stay_id = @stayId AND released_at IS NULL"

                        Using command As New MySqlCommand(existingStaySql, connection, transaction)
                            command.Parameters.Add("@stayId", MySqlDbType.UInt64).Value = stayId

                            If Convert.ToInt32(command.ExecuteScalar()) > 0 Then
                                Throw New InvalidOperationException(
                                    "This stay already has an active room assignment.")
                            End If
                        End Using

                        Dim rateOptionId As Long
                        Dim hourlyRate As Decimal
                        Dim additionalHourlyRate As Decimal
                        Dim dailyRate As Decimal
                        Dim minimumHours As Integer

                        Const rateSql As String =
                            "SELECT option_id, hourly_rate, additional_hourly_rate, " &
                            "daily_rate, minimum_hours " &
                            "FROM room_type_options " &
                            "WHERE room_type_id = @typeId AND bed_code = @bedCode " &
                            "AND is_active = 1 FOR UPDATE"

                        Using command As New MySqlCommand(rateSql, connection, transaction)
                            command.Parameters.Add("@typeId", MySqlDbType.UInt32).Value = roomTypeId
                            command.Parameters.Add("@bedCode", MySqlDbType.UByte).Value = bedCode

                            Using reader As MySqlDataReader = command.ExecuteReader()
                                If Not reader.Read() Then
                                    Throw New InvalidOperationException(
                                        "No active Admin-managed rate exists for this room type and bed.")
                                End If

                                rateOptionId = Convert.ToInt64(reader("option_id"))
                                hourlyRate = Convert.ToDecimal(reader("hourly_rate"))
                                additionalHourlyRate =
                                    Convert.ToDecimal(reader("additional_hourly_rate"))
                                dailyRate = Convert.ToDecimal(reader("daily_rate"))
                                minimumHours = Convert.ToInt32(reader("minimum_hours"))

                                If reader.Read() Then
                                    Throw New InvalidOperationException(
                                        "Multiple active rate options match this room. Ask Admin to correct the pricing setup.")
                                End If
                            End Using
                        End Using

                        Const insertSql As String =
                            "INSERT INTO room_assignments " &
                            "(stay_id, room_id, assigned_at, rate_option_id, " &
                            "hourly_rate_snapshot, additional_hourly_rate_snapshot, " &
                            "daily_rate_snapshot, minimum_hours_snapshot) " &
                            "VALUES (@stayId, @roomId, NOW(), @rateOptionId, " &
                            "@hourlyRate, @additionalHourlyRate, @dailyRate, @minimumHours)"

                        Using command As New MySqlCommand(insertSql, connection, transaction)
                            command.Parameters.Add("@stayId", MySqlDbType.UInt64).Value = stayId
                            command.Parameters.Add("@roomId", MySqlDbType.UInt64).Value = roomId
                            command.Parameters.Add("@rateOptionId", MySqlDbType.UInt64).Value = rateOptionId
                            command.Parameters.Add("@hourlyRate", MySqlDbType.Decimal).Value = hourlyRate
                            command.Parameters.Add("@additionalHourlyRate", MySqlDbType.Decimal).Value =
                                additionalHourlyRate
                            command.Parameters.Add("@dailyRate", MySqlDbType.Decimal).Value = dailyRate
                            command.Parameters.Add("@minimumHours", MySqlDbType.UByte).Value = minimumHours
                            command.ExecuteNonQuery()
                        End Using

                        Const updateRoomSql As String =
                            "UPDATE rooms SET status = 'OCCUPIED' " &
                            "WHERE room_id = @roomId AND status = 'AVAILABLE' AND is_active = 1"

                        Using command As New MySqlCommand(updateRoomSql, connection, transaction)
                            command.Parameters.Add("@roomId", MySqlDbType.UInt64).Value = roomId

                            If command.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException(
                                    "Room availability changed during assignment. Please refresh and try again.")
                            End If
                        End Using

                        transaction.Commit()

                    Catch
                        Try
                            transaction.Rollback()
                        Catch
                            ' Preserve the original database error.
                        End Try

                        Throw
                    End Try
                End Using
            End Using
        End Sub

        Private Sub ShowError(message As String, ex As Exception)
            MessageBox.Show(
                message & Environment.NewLine & Environment.NewLine & ex.Message,
                "StayPoint HMS",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            _status.Text = message
        End Sub

    End Class

End Namespace
