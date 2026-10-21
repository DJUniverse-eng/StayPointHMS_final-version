Imports System
Imports System.Data
Imports System.Drawing
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class RoomEditorForm
        Inherits Form

        Private ReadOnly roomId As Nullable(Of ULong)

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly bg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)

        Private ReadOnly txtRoomNumber As New TextBox()
        Private ReadOnly cmbFloor As New ComboBox()
        Private ReadOnly cmbType As New ComboBox()
        Private ReadOnly cmbBed As New ComboBox()
        Private ReadOnly nudCapacity As New NumericUpDown()
        Private ReadOnly cmbStatus As New ComboBox()
        Private ReadOnly txtDescription As New TextBox()
        Private ReadOnly lblRates As New Label()
        Private ReadOnly lblMessage As New Label()
        Private ReadOnly btnSave As New Button()
        Private ReadOnly btnCancel As New Button()

        Private isLoading As Boolean
        Private originalStatus As String = ""
        Private originalRoomNumber As String = ""
        Private originalTypeId As Integer
        Private originalBedCode As Integer

        Public Sub New(Optional existingRoomId As Nullable(Of ULong) = Nothing)
            roomId = existingRoomId

            Text = If(roomId.HasValue, "Edit Room", "Add Room")
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.Sizable
            MinimumSize = New Size(650, 600)
            Size = New Size(760, 700)
            BackColor = bg
            Font = New Font("Segoe UI", 9.5F)
            AutoScaleMode = AutoScaleMode.Dpi

            BuildForm()
            ConfigureCombo(cmbFloor)
            ConfigureCombo(cmbType)
            ConfigureCombo(cmbBed)
            ConfigureCombo(cmbStatus)

            For floor As Integer = 1 To 5
                cmbFloor.Items.Add(floor)
            Next

            cmbStatus.Items.AddRange(New Object() {
                "AVAILABLE",
                "OCCUPIED",
                "CLEANING",
                "UNDER_MAINTENANCE",
                "INACTIVE"
            })

            AddHandler cmbType.SelectedIndexChanged, AddressOf TypeChanged
            AddHandler cmbBed.SelectedIndexChanged, AddressOf BedChanged
            AddHandler cmbFloor.SelectedIndexChanged, AddressOf FloorChanged

            Try
                isLoading = True
                LoadRoomTypes()

                If roomId.HasValue Then
                    LoadExistingRoom(roomId.Value)
                Else
                    If cmbFloor.Items.Count > 0 Then cmbFloor.SelectedIndex = 0
                    If cmbType.Items.Count > 0 Then cmbType.SelectedIndex = 0

                    LoadBedOptions()

                    If cmbBed.Items.Count > 0 Then cmbBed.SelectedIndex = 0
                    cmbStatus.SelectedItem = "AVAILABLE"
                End If

            Catch ex As Exception
                lblMessage.Text = "Unable to load room details: " & ex.Message
                btnSave.Enabled = False
            Finally
                isLoading = False
            End Try

            UpdateGeneratedRoomNumber()
            LoadRatePreview()
        End Sub

        Private Sub BuildForm()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .Padding = New Padding(24),
                .BackColor = bg
            }

            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 58.0F))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 104.0F))

            Dim heading As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = bg
            }

            Dim title As New Label With {
                .Text = If(roomId.HasValue, "Edit Room", "Create Room"),
                .Font = New Font("Segoe UI", 19, FontStyle.Bold),
                .ForeColor = navy,
                .AutoSize = True,
                .Location = New Point(0, 0)
            }

            Dim subtitle As New Label With {
                .Text = "Maintain room details and operational settings.",
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = muted,
                .AutoSize = True,
                .Location = New Point(2, 34)
            }

            heading.Controls.Add(title)
            heading.Controls.Add(subtitle)
            root.Controls.Add(heading, 0, 0)

            Dim content As New Panel With {
                .Dock = DockStyle.Fill,
                .AutoScroll = True,
                .BackColor = Color.White,
                .Padding = New Padding(16),
                .BorderStyle = BorderStyle.FixedSingle
            }

            Dim fields As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 2,
                .RowCount = 0,
                .BackColor = Color.White,
                .Padding = New Padding(0, 0, 0, 8)
            }

            fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
            fields.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))

            AddField(fields, "Room Number", txtRoomNumber, 0, 0)
            AddField(fields, "Floor", cmbFloor, 1, 0)
            AddField(fields, "Room Type", cmbType, 0, 1)
            AddField(fields, "Bed Size", cmbBed, 1, 1)
            AddField(fields, "Guest Capacity", nudCapacity, 0, 2)
            AddField(fields, "Operational Status", cmbStatus, 1, 2)

            txtRoomNumber.ReadOnly = True
            txtRoomNumber.BackColor = Color.FromArgb(245, 247, 250)

            nudCapacity.Minimum = 1
            nudCapacity.Maximum = 20
            nudCapacity.Value = 2
            nudCapacity.Dock = DockStyle.Fill
            nudCapacity.Margin = New Padding(3, 5, 12, 10)

            AddFullWidthField(fields, "Description", txtDescription, 3, 70)
            ConfigureMultiline(txtDescription, 500)

            lblRates.Text = "Room rates will appear after selecting a room type and bed size."
            lblRates.ForeColor = navy
            lblRates.BackColor = Color.FromArgb(239, 247, 247)
            lblRates.Padding = New Padding(12)
            lblRates.Dock = DockStyle.Fill
            lblRates.AutoSize = False
            lblRates.MinimumSize = New Size(0, 65)

            fields.RowCount += 1
            fields.RowStyles.Add(New RowStyle(SizeType.Absolute, 78.0F))
            fields.Controls.Add(lblRates, 0, fields.RowCount - 1)
            fields.SetColumnSpan(lblRates, 2)

            content.Controls.Add(fields)
            root.Controls.Add(content, 0, 1)

            Dim footer As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = bg,
                .Padding = New Padding(0, 10, 0, 0)
            }

            footer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            footer.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
            footer.RowStyles.Add(New RowStyle(SizeType.Absolute, 42.0F))

            lblMessage.Text = ""
            lblMessage.ForeColor = Color.Firebrick
            lblMessage.Dock = DockStyle.Fill
            lblMessage.AutoEllipsis = True
            footer.Controls.Add(lblMessage, 0, 0)

            Dim buttons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False,
                .BackColor = bg,
                .Margin = New Padding(0)
            }

            btnCancel.Text = "Cancel"
            btnCancel.Size = New Size(110, 36)
            btnCancel.DialogResult = DialogResult.Cancel
            btnCancel.FlatStyle = FlatStyle.Flat
            btnCancel.FlatAppearance.BorderColor = borderColor
            btnCancel.Margin = New Padding(8, 2, 0, 2)

            btnSave.Text = "Save Room"
            btnSave.Size = New Size(130, 36)
            btnSave.BackColor = teal
            btnSave.ForeColor = Color.White
            btnSave.FlatStyle = FlatStyle.Flat
            btnSave.FlatAppearance.BorderSize = 0
            btnSave.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
            btnSave.Margin = New Padding(8, 2, 0, 2)
            AddHandler btnSave.Click, AddressOf SaveRoom

            buttons.Controls.Add(btnCancel)
            buttons.Controls.Add(btnSave)
            footer.Controls.Add(buttons, 0, 1)

            root.Controls.Add(footer, 0, 2)
            Controls.Add(root)

            AcceptButton = btnSave
            CancelButton = btnCancel
        End Sub

        Private Sub AddField(
            parent As TableLayoutPanel,
            caption As String,
            input As Control,
            column As Integer,
            row As Integer)

            While parent.RowCount <= row
                parent.RowCount += 1
                parent.RowStyles.Add(New RowStyle(SizeType.Absolute, 76.0F))
            End While

            Dim cell As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = Color.White,
                .Margin = New Padding(0, 0, 10, 0),
                .Padding = New Padding(0, 0, 4, 0)
            }

            cell.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            cell.RowStyles.Add(New RowStyle(SizeType.Absolute, 25.0F))
            cell.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

            Dim label As New Label With {
                .Text = caption,
                .ForeColor = navy,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft
            }

            input.Dock = DockStyle.Fill
            input.Margin = New Padding(3, 3, 3, 10)

            If TypeOf input Is TextBox Then
                DirectCast(input, TextBox).BorderStyle = BorderStyle.FixedSingle
            End If

            cell.Controls.Add(label, 0, 0)
            cell.Controls.Add(input, 0, 1)
            parent.Controls.Add(cell, column, row)
        End Sub

        Private Sub AddFullWidthField(
            parent As TableLayoutPanel,
            caption As String,
            input As TextBox,
            row As Integer,
            fieldHeight As Integer)

            parent.RowCount += 1
            parent.RowStyles.Add(New RowStyle(SizeType.Absolute, fieldHeight + 28))

            Dim cell As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = Color.White,
                .Margin = New Padding(0, 2, 0, 2)
            }

            cell.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
            cell.RowStyles.Add(New RowStyle(SizeType.Absolute, 24.0F))
            cell.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))

            Dim label As New Label With {
                .Text = caption,
                .ForeColor = navy,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .Dock = DockStyle.Fill
            }

            input.Multiline = True
            input.ScrollBars = ScrollBars.Vertical
            input.Dock = DockStyle.Fill
            input.Margin = New Padding(3, 2, 3, 6)
            input.BorderStyle = BorderStyle.FixedSingle

            cell.Controls.Add(label, 0, 0)
            cell.Controls.Add(input, 0, 1)

            parent.Controls.Add(cell, 0, parent.RowCount - 1)
            parent.SetColumnSpan(cell, 2)
        End Sub

        Private Sub ConfigureCombo(combo As ComboBox)
            combo.DropDownStyle = ComboBoxStyle.DropDownList
            combo.Dock = DockStyle.Fill
            combo.IntegralHeight = False
            combo.Margin = New Padding(3, 5, 12, 10)
        End Sub

        Private Sub ConfigureMultiline(box As TextBox, maxLength As Integer)
            box.Multiline = True
            box.MaxLength = maxLength
            box.ScrollBars = ScrollBars.Vertical
            box.Dock = DockStyle.Fill
        End Sub

        Private Sub LoadRoomTypes()
            Dim dt As New DataTable()

            Using conn = DbConnectionFactory.Create()
                conn.Open()

                Using cmd As New MySqlCommand(
                    "SELECT room_type_id, type_name, default_capacity " &
                    "FROM room_types WHERE is_active=1 ORDER BY type_name", conn)

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            cmbType.DataSource = dt
            cmbType.DisplayMember = "type_name"
            cmbType.ValueMember = "room_type_id"

            If dt.Rows.Count = 0 Then
                lblMessage.Text = "No active room types are configured."
                btnSave.Enabled = False
            End If
        End Sub

        Private Sub LoadBedOptions()
            If cmbType.SelectedValue Is Nothing OrElse
               TypeOf cmbType.SelectedValue Is DataRowView Then
                cmbBed.DataSource = Nothing
                Return
            End If

            Dim selectedTypeId As Integer =
                Convert.ToInt32(cmbType.SelectedValue)

            Dim dt As New DataTable()

            Using conn = DbConnectionFactory.Create()
                conn.Open()

                Using cmd As New MySqlCommand(
                    "SELECT bed_code, bed_name FROM room_type_options " &
                    "WHERE room_type_id=@type AND is_active=1 ORDER BY bed_code", conn)

                    cmd.Parameters.Add("@type", MySqlDbType.Int32).Value = selectedTypeId

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            cmbBed.DataSource = Nothing
            cmbBed.DisplayMember = "bed_name"
            cmbBed.ValueMember = "bed_code"
            cmbBed.DataSource = dt

            If dt.Rows.Count = 0 Then
                lblMessage.Text =
                    "No active bed/pricing option exists for this room type. " &
                    "Configure it in Room Management."
                btnSave.Enabled = False
            Else
                lblMessage.Text = ""
                btnSave.Enabled = True
            End If
        End Sub

        Private Sub TypeChanged(sender As Object, e As EventArgs)
            If isLoading Then Return

            Try
                LoadBedOptions()

                Dim selectedType As DataRowView =
                    TryCast(cmbType.SelectedItem, DataRowView)

                If selectedType IsNot Nothing Then
                    Dim capacity As Integer =
                        Convert.ToInt32(selectedType("default_capacity"))

                    nudCapacity.Value = Math.Max(
                        CDec(nudCapacity.Minimum),
                        Math.Min(CDec(nudCapacity.Maximum), CDec(capacity)))
                End If

                UpdateGeneratedRoomNumber()
                LoadRatePreview()

            Catch ex As Exception
                lblMessage.Text = "Unable to load bed options: " & ex.Message
                btnSave.Enabled = False
            End Try
        End Sub

        Private Sub BedChanged(sender As Object, e As EventArgs)
            If isLoading Then Return
            UpdateGeneratedRoomNumber()
            LoadRatePreview()
        End Sub

        Private Sub FloorChanged(sender As Object, e As EventArgs)
            If isLoading Then Return
            UpdateGeneratedRoomNumber()
        End Sub

        Private Function GetRoomTypeDigit(typeName As String) As Integer
            Select Case typeName.Trim().ToUpperInvariant()
                Case "STANDARD" : Return 1
                Case "FAMILY" : Return 2
                Case "DELUXE" : Return 3
                Case "SUITE" : Return 4
                Case Else
                    Throw New InvalidOperationException(
                        "The room type '" & typeName &
                        "' has no assigned room-number code.")
            End Select
        End Function

        Private Function GetBedDigit(bedCode As Integer) As Integer
            If bedCode < 1 OrElse bedCode > 4 Then
                Throw New InvalidOperationException(
                    "The selected bed code must be between 1 and 4.")
            End If

            Return bedCode
        End Function

        Private Sub UpdateGeneratedRoomNumber()
            If roomId.HasValue Then Return

            If cmbFloor.SelectedItem Is Nothing OrElse
               cmbType.SelectedItem Is Nothing OrElse
               cmbBed.SelectedValue Is Nothing OrElse
               TypeOf cmbBed.SelectedValue Is DataRowView Then
                txtRoomNumber.Text = ""
                Return
            End If

            Try
                Dim typeView As DataRowView =
                    TryCast(cmbType.SelectedItem, DataRowView)

                If typeView Is Nothing Then Return

                Dim prefix As String =
                    Convert.ToInt32(cmbFloor.SelectedItem).ToString() &
                    GetRoomTypeDigit(typeView("type_name").ToString()).ToString() &
                    GetBedDigit(Convert.ToInt32(cmbBed.SelectedValue)).ToString()

                txtRoomNumber.Text = GetNextRoomNumber(prefix)
                lblMessage.Text = ""

            Catch ex As Exception
                txtRoomNumber.Text = ""
                lblMessage.Text = ex.Message
            End Try
        End Sub

        Private Function GetNextRoomNumber(prefix As String) As String
            Dim used As New HashSet(Of Integer)()

            Using conn = DbConnectionFactory.Create()
                conn.Open()

                Using cmd As New MySqlCommand(
                    "SELECT room_number FROM rooms WHERE room_number LIKE @prefix", conn)

                    cmd.Parameters.Add("@prefix", MySqlDbType.VarChar).Value = prefix & "%"

                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim number As String = reader("room_number").ToString()
                            Dim sequence As Integer

                            If number.Length = 5 AndAlso
                               number.StartsWith(prefix, StringComparison.Ordinal) AndAlso
                               Integer.TryParse(number.Substring(3, 2), sequence) Then
                                used.Add(sequence)
                            End If
                        End While
                    End Using
                End Using
            End Using

            For sequence As Integer = 1 To 99
                If Not used.Contains(sequence) Then
                    Return prefix & sequence.ToString("00")
                End If
            Next

            Throw New InvalidOperationException(
                "All room numbers for this floor, type, and bed combination are in use.")
        End Function

        Private Sub LoadRatePreview()
            If cmbType.SelectedValue Is Nothing OrElse
               cmbBed.SelectedValue Is Nothing OrElse
               TypeOf cmbType.SelectedValue Is DataRowView OrElse
               TypeOf cmbBed.SelectedValue Is DataRowView Then
                lblRates.Text = "Select a room type and bed size to view the current rate."
                Return
            End If

            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Using cmd As New MySqlCommand(
                        "SELECT bed_name, hourly_rate, additional_hourly_rate, " &
                        "daily_rate, minimum_hours FROM room_type_options " &
                        "WHERE room_type_id=@type AND bed_code=@bed " &
                        "AND is_active=1 LIMIT 1", conn)

                        cmd.Parameters.Add("@type", MySqlDbType.Int32).Value =
                            Convert.ToInt32(cmbType.SelectedValue)

                        cmd.Parameters.Add("@bed", MySqlDbType.Int32).Value =
                            Convert.ToInt32(cmbBed.SelectedValue)

                        Using reader = cmd.ExecuteReader()
                            If reader.Read() Then
                                lblRates.Text =
            "Selected rate: " & reader("bed_name").ToString() &
            Environment.NewLine &
            "Hourly: ₱" & Convert.ToDecimal(reader("hourly_rate")).ToString("N2") &
            "   |   Additional hour: ₱" & Convert.ToDecimal(reader("additional_hourly_rate")).ToString("N2") &
            Environment.NewLine &
            "Daily: ₱" & Convert.ToDecimal(reader("daily_rate")).ToString("N2") &
            "   |   Minimum: " & reader("minimum_hours").ToString() & " hour(s)"
                            Else
                                lblRates.Text = "No active rate is configured for this selection."
                            End If
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                lblRates.Text = "Unable to load the current rate preview."
            End Try
        End Sub

        Private Sub LoadExistingRoom(id As ULong)
            Using conn = DbConnectionFactory.Create()
                conn.Open()

                Using cmd As New MySqlCommand(
                    "SELECT room_number, floor_number, room_type_id, bed_code, " &
                    "capacity, description, status FROM rooms " &
                    "WHERE room_id=@id AND is_active=1", conn)

                    cmd.Parameters.Add("@id", MySqlDbType.UInt64).Value = id

                    Using reader = cmd.ExecuteReader()
                        If Not reader.Read() Then
                            Throw New InvalidOperationException(
                                "The selected room no longer exists or is inactive.")
                        End If

                        originalRoomNumber = reader("room_number").ToString()
                        originalTypeId = Convert.ToInt32(reader("room_type_id"))
                        originalBedCode = Convert.ToInt32(reader("bed_code"))
                        originalStatus = reader("status").ToString()

                        txtRoomNumber.Text = originalRoomNumber
                        cmbFloor.SelectedItem = Convert.ToInt32(reader("floor_number"))
                        cmbType.SelectedValue = originalTypeId

                        nudCapacity.Value = Math.Max(
                            CDec(nudCapacity.Minimum),
                            Math.Min(CDec(nudCapacity.Maximum),
                                Convert.ToDecimal(reader("capacity"))))

                        txtDescription.Text =
                            If(reader.IsDBNull(reader.GetOrdinal("description")),
                               "", reader("description").ToString())
                    End Using
                End Using
            End Using

            LoadBedOptions()
            cmbBed.SelectedValue = originalBedCode
            cmbStatus.SelectedItem = originalStatus

            If cmbStatus.SelectedIndex < 0 Then
                Throw New InvalidOperationException(
                    "The room has an unrecognized status. Correct its status before editing.")
            End If

            cmbFloor.Enabled = False
            cmbType.Enabled = False
            cmbBed.Enabled = False
            txtRoomNumber.ReadOnly = True

            If originalStatus = "OCCUPIED" Then
                cmbStatus.Enabled = False
                lblMessage.Text =
                    "Occupied room status is managed through the guest-stay workflow."
            End If

            LoadRatePreview()
        End Sub

        Private Sub SaveRoom(sender As Object, e As EventArgs)
            If Not btnSave.Enabled Then Return

            If String.IsNullOrWhiteSpace(txtRoomNumber.Text) OrElse
               txtRoomNumber.Text.Length <> 5 Then
                lblMessage.Text = "A valid five-character room number is required."
                Return
            End If

            If cmbType.SelectedValue Is Nothing OrElse
               cmbBed.SelectedValue Is Nothing OrElse
               TypeOf cmbType.SelectedValue Is DataRowView OrElse
               TypeOf cmbBed.SelectedValue Is DataRowView Then
                lblMessage.Text = "Select a valid room type and bed size."
                Return
            End If

            If cmbFloor.SelectedItem Is Nothing OrElse
               cmbStatus.SelectedItem Is Nothing Then
                lblMessage.Text = "Select a valid floor and operational status."
                Return
            End If

            If roomId.HasValue AndAlso txtRoomNumber.Text <> originalRoomNumber Then
                lblMessage.Text = "An existing room number cannot be changed here."
                Return
            End If

            btnSave.Enabled = False
            lblMessage.Text = ""

            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Using tx = conn.BeginTransaction()
                        Dim statusToSave As String = cmbStatus.SelectedItem.ToString()

                        Dim typeView As DataRowView =
                            TryCast(cmbType.SelectedItem, DataRowView)

                        If typeView Is Nothing Then
                            Throw New InvalidOperationException("The selected room type is invalid.")
                        End If

                        Dim selectedTypeId As Integer =
                            Convert.ToInt32(cmbType.SelectedValue)

                        Dim selectedBedCode As Integer =
                            Convert.ToInt32(cmbBed.SelectedValue)

                        If roomId.HasValue Then
                            Dim currentStatus As String = ""
                            Dim isActive As Boolean

                            Using check As New MySqlCommand(
                                "SELECT status, is_active FROM rooms " &
                                "WHERE room_id=@id FOR UPDATE", conn, tx)

                                check.Parameters.Add("@id", MySqlDbType.UInt64).Value = roomId.Value

                                Using reader = check.ExecuteReader()
                                    If Not reader.Read() Then
                                        Throw New InvalidOperationException("The room no longer exists.")
                                    End If

                                    currentStatus = reader("status").ToString()
                                    isActive = Convert.ToBoolean(reader("is_active"))
                                End Using
                            End Using

                            If Not isActive Then
                                Throw New InvalidOperationException(
                                    "This room is inactive and cannot be edited here.")
                            End If

                            If currentStatus = "OCCUPIED" AndAlso statusToSave <> "OCCUPIED" Then
                                Throw New InvalidOperationException(
                                    "An occupied room's status cannot be changed here.")
                            End If

                            If currentStatus <> "OCCUPIED" AndAlso statusToSave = "OCCUPIED" Then
                                Throw New InvalidOperationException(
                                    "A room can only become occupied through the guest-stay workflow.")
                            End If

                            If statusToSave <> currentStatus Then
                                Using check As New MySqlCommand(
                                    "SELECT COUNT(*) FROM room_assignments " &
                                    "WHERE room_id=@id AND released_at IS NULL", conn, tx)

                                    check.Parameters.Add("@id", MySqlDbType.UInt64).Value = roomId.Value

                                    If Convert.ToInt32(check.ExecuteScalar()) > 0 Then
                                        Throw New InvalidOperationException(
                                            "This room has an active assignment. Resolve the guest stay before changing its status.")
                                    End If
                                End Using
                            End If

                            Using check As New MySqlCommand(
                                "SELECT COUNT(*) FROM room_type_options " &
                                "WHERE room_type_id=@type AND bed_code=@bed AND is_active=1", conn, tx)

                                check.Parameters.Add("@type", MySqlDbType.Int32).Value = originalTypeId
                                check.Parameters.Add("@bed", MySqlDbType.Int32).Value = originalBedCode

                                If Convert.ToInt32(check.ExecuteScalar()) = 0 Then
                                    Throw New InvalidOperationException(
                                        "This room's bed/pricing option is no longer active.")
                                End If
                            End Using

                            Using cmd As New MySqlCommand(
                                "UPDATE rooms SET capacity=@capacity, description=@description, " &
                                "status=@status, is_active=CASE WHEN @status='INACTIVE' THEN 0 ELSE 1 END " &
                                "WHERE room_id=@id AND is_active=1", conn, tx)

                                AddRoomParameters(cmd, statusToSave)
                                cmd.Parameters.Add("@id", MySqlDbType.UInt64).Value = roomId.Value

                                If cmd.ExecuteNonQuery() <> 1 Then
                                    Throw New InvalidOperationException(
                                        "The room was not updated. Refresh and try again.")
                                End If
                            End Using

                        Else
                            If statusToSave <> "AVAILABLE" Then
                                Throw New InvalidOperationException(
                                    "New rooms must start with AVAILABLE status.")
                            End If

                            Dim floorNumber As Integer =
                                Convert.ToInt32(cmbFloor.SelectedItem)

                            Dim expectedPrefix As String =
                                floorNumber.ToString() &
                                GetRoomTypeDigit(typeView("type_name").ToString()).ToString() &
                                GetBedDigit(selectedBedCode).ToString()

                            If txtRoomNumber.Text.Length <> 5 OrElse
                               Not txtRoomNumber.Text.StartsWith(expectedPrefix, StringComparison.Ordinal) Then
                                Throw New InvalidOperationException(
                                    "The generated room number does not match the selected floor, type, and bed.")
                            End If

                            Using check As New MySqlCommand(
                                "SELECT COUNT(*) FROM room_type_options " &
                                "WHERE room_type_id=@type AND bed_code=@bed AND is_active=1", conn, tx)

                                check.Parameters.Add("@type", MySqlDbType.Int32).Value = selectedTypeId
                                check.Parameters.Add("@bed", MySqlDbType.Int32).Value = selectedBedCode

                                If Convert.ToInt32(check.ExecuteScalar()) = 0 Then
                                    Throw New InvalidOperationException(
                                        "The selected bed/pricing option is no longer active.")
                                End If
                            End Using

                            Using cmd As New MySqlCommand(
                                "INSERT INTO rooms " &
                                "(room_number, floor_number, room_type_id, bed_code, capacity, description, status, is_active) " &
                                "VALUES (@number,@floor,@type,@bed,@capacity,@description,'AVAILABLE',1)", conn, tx)

                                cmd.Parameters.Add("@number", MySqlDbType.String).Value = txtRoomNumber.Text
                                cmd.Parameters.Add("@floor", MySqlDbType.Byte).Value = floorNumber
                                cmd.Parameters.Add("@type", MySqlDbType.Int32).Value = selectedTypeId
                                cmd.Parameters.Add("@bed", MySqlDbType.Byte).Value = selectedBedCode

                                AddRoomParameters(cmd, "AVAILABLE")

                                If cmd.ExecuteNonQuery() <> 1 Then
                                    Throw New InvalidOperationException("The room was not created.")
                                End If
                            End Using
                        End If

                        tx.Commit()
                    End Using
                End Using

                DialogResult = DialogResult.OK
                Close()

            Catch ex As MySqlException When ex.Number = 1062
                lblMessage.Text =
                    "That room number already exists. Refresh and generate a new number."

            Catch ex As Exception
                lblMessage.Text = "Could not save room: " & ex.Message

            Finally
                If Not IsDisposed Then btnSave.Enabled = True
            End Try
        End Sub

        Private Sub AddRoomParameters(cmd As MySqlCommand, status As String)
            cmd.Parameters.Add("@capacity", MySqlDbType.Int32).Value =
                Convert.ToInt32(nudCapacity.Value)

            cmd.Parameters.Add("@description", MySqlDbType.VarChar).Value =
                NullableText(txtDescription.Text)

            cmd.Parameters.Add("@status", MySqlDbType.VarChar).Value = status
        End Sub

        Private Function NullableText(value As String) As Object
            If String.IsNullOrWhiteSpace(value) Then Return DBNull.Value
            Return value.Trim()
        End Function

    End Class

End Namespace