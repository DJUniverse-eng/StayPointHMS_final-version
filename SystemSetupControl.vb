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

    ' Admin-only capacity setup and expansion. This form intentionally does not
    ' delete or renumber existing inventory.
    Public NotInheritable Class SystemSetupControl
        Inherits UserControl

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly pageBg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)

        Private ReadOnly hotelName As New TextBox()
        Private ReadOnly hotelAddress As New TextBox()
        Private ReadOnly hotelPhone As New TextBox()
        Private ReadOnly floors As New NumericUpDown()
        Private ReadOnly roomGrid As New DataGridView()
        Private ReadOnly parkingGrid As New DataGridView()
        Private ReadOnly status As New Label()
        Private ReadOnly saveHotel As New Button()
        Private ReadOnly generate As New Button()
        Private isLoading As Boolean

        Public Sub New()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.Admin)
            Dock = DockStyle.Fill
            BackColor = pageBg
            Font = New Font("Segoe UI", 9.5F)
            BuildUi()
            LoadSetup()
        End Sub

        Private Sub BuildUi()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill, .ColumnCount = 1, .RowCount = 4,
                .Padding = New Padding(20), .BackColor = pageBg,
                .AutoScroll = True
            }
            root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 50))

            Dim heading As New Label With {
                .Text = "System Setup & Capacity Expansion",
                .Font = New Font("Segoe UI", 20, FontStyle.Bold),
                .ForeColor = navy, .AutoSize = True,
                .Margin = New Padding(0, 0, 0, 12)
            }
            root.Controls.Add(heading, 0, 0)

            Dim hotel As New TableLayoutPanel With {
                .Dock = DockStyle.Top, .ColumnCount = 4, .RowCount = 4,
                .AutoSize = True, .Padding = New Padding(12),
                .BackColor = Color.White, .Margin = New Padding(0, 0, 0, 12)
            }
            hotel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 120))
            hotel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            hotel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 100))
            hotel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            AddHotelField(hotel, "Hotel name", hotelName, 0, 0)
            AddHotelField(hotel, "Address", hotelAddress, 0, 1)
            AddHotelField(hotel, "Phone", hotelPhone, 0, 2)
            Dim floorLabel As New Label With {.Text = "Floors to include", .AutoSize = True, .Anchor = AnchorStyles.Left}
            floors.Minimum = 1 : floors.Maximum = 9 : floors.Value = 1
            floors.Width = 90
            hotel.Controls.Add(floorLabel, 2, 2)
            hotel.Controls.Add(floors, 3, 2)
            saveHotel.Text = "Save Hotel Information"
            StyleButton(saveHotel)
            saveHotel.AutoSize = True
            AddHandler saveHotel.Click, AddressOf SaveHotelInformation
            hotel.Controls.Add(saveHotel, 0, 3)
            hotel.SetColumnSpan(saveHotel, 2)
            root.Controls.Add(hotel, 0, 1)

            Dim roomGroup As New GroupBox With {
                .Text = "Room Capacity (per floor)", .Dock = DockStyle.Fill,
                .ForeColor = navy, .Padding = New Padding(10),
                .Margin = New Padding(0, 0, 0, 10)
            }
            roomGrid.Dock = DockStyle.Fill
            roomGrid.AllowUserToAddRows = False
            roomGrid.AllowUserToDeleteRows = False
            roomGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            roomGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            roomGrid.RowHeadersVisible = False
            roomGrid.BackgroundColor = Color.White
            roomGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "type_id", .HeaderText = "Room Type ID", .ReadOnly = True})
            roomGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "type_name", .HeaderText = "Room Type", .ReadOnly = True})
            roomGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "bed_code", .HeaderText = "Bed Code", .ReadOnly = True})
            roomGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "bed_name", .HeaderText = "Bed / Size", .ReadOnly = True})
            roomGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "count", .HeaderText = "Rooms per floor (0-99)"})
            roomGroup.Controls.Add(roomGrid)
            Dim roomWrap As New Panel With {.Dock = DockStyle.Fill}
            roomWrap.Controls.Add(roomGroup)
            root.Controls.Add(roomWrap, 0, 2)

            Dim parkingGroup As New GroupBox With {
                .Text = "Parking Capacity (per floor)", .Dock = DockStyle.Fill,
                .ForeColor = navy, .Padding = New Padding(10),
                .Margin = New Padding(0)
            }
            parkingGrid.Dock = DockStyle.Fill
            parkingGrid.AllowUserToAddRows = False
            parkingGrid.AllowUserToDeleteRows = False
            parkingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            parkingGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            parkingGrid.RowHeadersVisible = False
            parkingGrid.BackgroundColor = Color.White
            parkingGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "category", .HeaderText = "Vehicle Category", .ReadOnly = True})
            parkingGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "prefix", .HeaderText = "Prefix", .ReadOnly = True})
            parkingGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "count", .HeaderText = "Spaces per floor (0-99)"})
            parkingGroup.Controls.Add(parkingGrid)
            Dim parkingWrap As New Panel With {.Dock = DockStyle.Fill}
            parkingWrap.Controls.Add(parkingGroup)
            root.Controls.Add(parkingWrap, 0, 3)

            Dim footer As New Panel With {.Dock = DockStyle.Bottom, .Height = 62, .BackColor = pageBg}
            generate.Text = "Generate / Expand Inventory"
            generate.Size = New Size(230, 38)
            generate.Anchor = AnchorStyles.Right Or AnchorStyles.Bottom
            StyleButton(generate)
            AddHandler generate.Click, AddressOf GenerateInventory
            status.AutoSize = False
            status.Dock = DockStyle.Left
            status.Width = 650
            status.ForeColor = muted
            status.TextAlign = ContentAlignment.MiddleLeft
            footer.Controls.Add(status)
            footer.Controls.Add(generate)
            AddHandler footer.Resize, Sub()
                generate.Location = New Point(Math.Max(0, footer.ClientSize.Width - generate.Width), 10)
            End Sub

            Controls.Add(root)
            Controls.Add(footer)
        End Sub

        Private Sub AddHotelField(table As TableLayoutPanel, caption As String, box As TextBox, column As Integer, row As Integer)
            Dim label As New Label With {.Text = caption, .AutoSize = True, .Anchor = AnchorStyles.Left}
            box.Dock = DockStyle.Fill
            box.Margin = New Padding(3, 5, 10, 5)
            table.Controls.Add(label, column, row)
            table.Controls.Add(box, column + 1, row)
        End Sub

        Private Sub StyleButton(button As Button)
            button.BackColor = teal
            button.ForeColor = Color.White
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderSize = 0
            button.Cursor = Cursors.Hand
            button.Padding = New Padding(8)
        End Sub

        Private Sub LoadSetup()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.Admin)
            isLoading = True
            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()
                    Using cmd As New MySqlCommand("SELECT setting_key, setting_value FROM app_settings WHERE setting_key IN ('hotel_name','hotel_address','hotel_phone','setup_floors')", conn)
                        Using r = cmd.ExecuteReader()
                            While r.Read()
                                Dim k = Convert.ToString(r("setting_key"))
                                Dim v = Convert.ToString(r("setting_value"))
                                Select Case k
                                    Case "hotel_name" : hotelName.Text = v
                                    Case "hotel_address" : hotelAddress.Text = v
                                    Case "hotel_phone" : hotelPhone.Text = v
                                    Case "setup_floors"
                                        Dim n As Integer
                                        If Integer.TryParse(v, n) AndAlso n >= 1 AndAlso n <= 9 Then floors.Value = n
                                End Select
                            End While
                        End Using
                    End Using

                    Using cmd As New MySqlCommand(
                        "SELECT rt.room_type_id, rt.type_name, rto.bed_code, rto.bed_name " &
                        "FROM room_types rt INNER JOIN room_type_options rto ON rto.room_type_id=rt.room_type_id " &
                        "WHERE rto.is_active=1 ORDER BY rt.room_type_id, rto.bed_code", conn)
                        Using r = cmd.ExecuteReader()
                            roomGrid.Rows.Clear()
                            While r.Read()
                                roomGrid.Rows.Add(Convert.ToString(r("room_type_id")), Convert.ToString(r("type_name")),
                                                  Convert.ToString(r("bed_code")), Convert.ToString(r("bed_name")), "0")
                            End While
                        End Using
                    End Using
                End Using

                parkingGrid.Rows.Clear()
                parkingGrid.Rows.Add("MOTORCYCLE", "M", "0")
                parkingGrid.Rows.Add("SEDAN", "S", "0")
                parkingGrid.Rows.Add("SUV", "U", "0")
                parkingGrid.Rows.Add("VAN", "V", "0")
            Catch ex As Exception
                status.Text = "Setup data could not be loaded: " & ex.Message
            Finally
                isLoading = False
            End Try
        End Sub

        Private Sub SaveHotelInformation(sender As Object, e As EventArgs)
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.Admin)
            If String.IsNullOrWhiteSpace(hotelName.Text) Then
                MessageBox.Show(Me, "Enter the hotel name.", "System Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                hotelName.Focus()
                Return
            End If
            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()
                    Using tx = conn.BeginTransaction()
                        SaveSetting(conn, tx, "hotel_name", hotelName.Text.Trim())
                        SaveSetting(conn, tx, "hotel_address", hotelAddress.Text.Trim())
                        SaveSetting(conn, tx, "hotel_phone", hotelPhone.Text.Trim())
                        SaveSetting(conn, tx, "setup_floors", CInt(floors.Value).ToString(CultureInfo.InvariantCulture))
                        tx.Commit()
                    End Using
                End Using
                status.Text = "Hotel information saved."
            Catch ex As Exception
                status.Text = "Could not save hotel information: " & ex.Message
            End Try
        End Sub

        Private Sub SaveSetting(conn As MySqlConnection, tx As MySqlTransaction, key As String, value As String)
            Using cmd As New MySqlCommand(
                "INSERT INTO app_settings (setting_key, setting_value) VALUES (@key,@value) " &
                "ON DUPLICATE KEY UPDATE setting_value=VALUES(setting_value)", conn, tx)
                cmd.Parameters.AddWithValue("@key", key)
                cmd.Parameters.AddWithValue("@value", value)
                cmd.ExecuteNonQuery()
            End Using
        End Sub

        Private Sub GenerateInventory(sender As Object, e As EventArgs)
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.Admin)
            ' Pricing is configured from the dedicated pricing screens after initial inventory
            ' exists. Staff operations remain blocked until inventory and both rate sets are valid.

            If MessageBox.Show(Me,
                "Generate only missing inventory for the selected floors? Existing rooms, active parking tickets, and financial history will not be reset.",
                "Confirm Capacity Generation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            Try
                roomGrid.EndEdit()
                parkingGrid.EndEdit()
                Dim floorCount = CInt(floors.Value)
                Using conn = DbConnectionFactory.Create()
                    conn.Open()
                    Using tx = conn.BeginTransaction()
                        SaveSetting(conn, tx, "setup_floors", floorCount.ToString(CultureInfo.InvariantCulture))

                        For floor = 1 To floorCount
                            For Each row As DataGridViewRow In roomGrid.Rows
                                Dim count = ParseCount(row.Cells("count").Value, "room")
                                If count = 0 Then Continue For
                                Dim typeId = Convert.ToInt32(row.Cells("type_id").Value)
                                Dim bedCode = Convert.ToInt32(row.Cells("bed_code").Value)
                                Dim typeName = Convert.ToString(row.Cells("type_name").Value)
                                ' Keep the agreed three-room limit per Room ID. Groups of three receive
                                ' successive Room IDs; each group has room sequence 01-03.
                                Dim remaining = count
                                Dim groupNo = 0
                                While remaining > 0
                                    Dim typeCode = ((typeId + groupNo - 1) Mod 9) + 1
                                    Dim roomIdCode = (floor * 10) + typeCode
                                    For seq = 1 To Math.Min(3, remaining)
                                        Dim roomNo = (bedCode * 100) + (groupNo * 3) + seq
                                        Dim fullCode = roomIdCode.ToString("00", CultureInfo.InvariantCulture) & "-" &
                                                       roomNo.ToString("000", CultureInfo.InvariantCulture)
                                        Using cmd As New MySqlCommand(
                                            "INSERT INTO rooms (room_number, floor_number, room_type_id, bed_code, capacity, description, status, is_active) " &
                                            "SELECT @number,@floor,@type,@bed,1,@description,'AVAILABLE',1 " &
                                            "WHERE NOT EXISTS (SELECT 1 FROM rooms WHERE room_number=@number)",
                                            conn, tx)
                                            cmd.Parameters.AddWithValue("@number", fullCode)
                                            cmd.Parameters.AddWithValue("@floor", floor)
                                            cmd.Parameters.AddWithValue("@type", typeId)
                                            cmd.Parameters.AddWithValue("@bed", bedCode)
                                            cmd.Parameters.AddWithValue("@description", typeName & " / generated by System Setup")
                                            cmd.ExecuteNonQuery()
                                        End Using
                                    Next
                                    remaining -= Math.Min(3, remaining)
                                    groupNo += 1
                                End While
                            Next

                            For Each row As DataGridViewRow In parkingGrid.Rows
                                Dim count = ParseCount(row.Cells("count").Value, "parking")
                                Dim category = Convert.ToString(row.Cells("category").Value)
                                Dim prefix = Convert.ToString(row.Cells("prefix").Value)
                                For seq = 1 To count
                                    Dim code = prefix & floor.ToString(CultureInfo.InvariantCulture) & seq.ToString("00", CultureInfo.InvariantCulture)
                                    Using cmd As New MySqlCommand(
                                        "INSERT INTO parking_spaces (parking_code, floor_number, vehicle_category, area_name, status, notes, is_active) " &
                                        "SELECT @code,@floor,@vehicle,@area,'AVAILABLE','Generated by System Setup',1 " &
                                        "WHERE NOT EXISTS (SELECT 1 FROM parking_spaces WHERE parking_code=@code)",
                                        conn, tx)
                                        cmd.Parameters.AddWithValue("@code", code)
                                        cmd.Parameters.AddWithValue("@floor", floor)
                                        cmd.Parameters.AddWithValue("@vehicle", category)
                                        cmd.Parameters.AddWithValue("@area", "Floor " & floor.ToString(CultureInfo.InvariantCulture))
                                        cmd.ExecuteNonQuery()
                                    End Using
                                Next
                            Next
                        Next
                        SaveSetting(conn, tx, "setup_completed", "1")
                        SaveSetting(conn, tx, "setup_completed_by", CurrentUser.UserId.ToString(CultureInfo.InvariantCulture))
                        SaveSetting(conn, tx, "setup_completed_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                        Using setupCmd As New MySqlCommand(
                            "INSERT INTO system_setup (setup_id, setup_completed, completed_by, completed_at) " &
                            "VALUES (1,1,@user,NOW()) ON DUPLICATE KEY UPDATE " &
                            "setup_completed=1, completed_by=VALUES(completed_by), completed_at=VALUES(completed_at)",
                            conn, tx)
                            setupCmd.Parameters.AddWithValue("@user", CurrentUser.UserId)
                            setupCmd.ExecuteNonQuery()
                        End Using
                        tx.Commit()
                    End Using
                End Using
                status.Text = "Inventory generation completed. Duplicate room and parking codes were skipped."
                MessageBox.Show(Me, status.Text, "System Setup", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                status.Text = "Inventory generation failed; changes were rolled back: " & ex.Message
                MessageBox.Show(Me, status.Text, "System Setup", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

        Private Function ParseCount(value As Object, kind As String) As Integer
            Dim result As Integer
            If value Is Nothing OrElse Not Integer.TryParse(Convert.ToString(value), result) OrElse result < 0 OrElse result > 99 Then
                Throw New InvalidOperationException("Enter a whole number from 0 to 99 for every " & kind & " capacity.")
            End If
            Return result
        End Function
    End Class
End Namespace
