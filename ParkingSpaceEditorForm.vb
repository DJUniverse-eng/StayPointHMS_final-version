Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class ParkingSpaceEditorForm
        Inherits Form

        Private ReadOnly parkingSpaceId As Nullable(Of ULong)

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly bg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)

        Private ReadOnly txtCode As New TextBox()
        Private ReadOnly cmbFloor As New ComboBox()
        Private ReadOnly cmbVehicle As New ComboBox()
        Private ReadOnly txtArea As New TextBox()
        Private ReadOnly cmbStatus As New ComboBox()
        Private ReadOnly txtNotes As New TextBox()

        Private ReadOnly lblRates As New Label()
        Private ReadOnly lblMessage As New Label()

        Private ReadOnly btnSave As New Button()
        Private ReadOnly btnCancel As New Button()

        Private isLoading As Boolean
        Private originalStatus As String = ""
        Private originalCode As String = ""

        Public Sub New(Optional existingParkingSpaceId As Nullable(Of ULong) = Nothing)
            parkingSpaceId = existingParkingSpaceId

            Text = If(parkingSpaceId.HasValue,
                      "Edit Parking Space",
                      "Add Parking Space")

            StartPosition = FormStartPosition.CenterParent
            Size = New Size(650, 720)
            MinimumSize = New Size(540, 600)
            BackColor = bg
            ForeColor = navy
            Font = New Font("Segoe UI", 9.5F)
            FormBorderStyle = FormBorderStyle.Sizable
            MaximizeBox = False
            MinimizeBox = False

            BuildInterface()
            LoadVehicleCategories()

            If parkingSpaceId.HasValue Then
                LoadExistingSpace()
            Else
                cmbFloor.SelectedIndex = 0
                cmbVehicle.SelectedIndex = 0
                cmbStatus.SelectedItem = "AVAILABLE"
                txtCode.Text = ""
                txtCode.ReadOnly = False
            End If

            UpdateRatePreview()
        End Sub

        Private Sub BuildInterface()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .BackColor = bg,
                .Padding = New Padding(20)
            }

            root.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100))

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 76))

            root.RowStyles.Add(
                New RowStyle(SizeType.Percent, 100))

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 76))

            Controls.Add(root)

            Dim heading As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = bg
            }

            Dim lblTitle As New Label With {
                .Text = If(parkingSpaceId.HasValue,
                           "Edit Parking Space",
                           "Add Parking Space"),
                .Font = New Font("Segoe UI", 19, FontStyle.Bold),
                .ForeColor = navy,
                .AutoSize = True,
                .Location = New Point(0, 0)
            }

            Dim lblSubtitle As New Label With {
                .Text = "Configure parking space information and status.",
                .Font = New Font("Segoe UI", 9.5F),
                .ForeColor = muted,
                .AutoSize = True,
                .Location = New Point(2, 39)
            }

            heading.Controls.Add(lblTitle)
            heading.Controls.Add(lblSubtitle)
            root.Controls.Add(heading, 0, 0)

            Dim scrollPanel As New Panel With {
                .Dock = DockStyle.Fill,
                .AutoScroll = True,
                .BackColor = Color.White,
                .Padding = New Padding(18)
            }

            root.Controls.Add(scrollPanel, 0, 1)

            Dim fields As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .ColumnCount = 2,
                .RowCount = 0,
                .BackColor = Color.White,
                .Padding = New Padding(0, 4, 0, 12)
            }

            fields.ColumnStyles.Add(
                New ColumnStyle(SizeType.Absolute, 145))

            fields.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100))

            scrollPanel.Controls.Add(fields)

            ConfigureTextBox(txtCode)
            txtCode.MaxLength = 4
            txtCode.CharacterCasing = CharacterCasing.Upper
            txtCode.PlaceholderText = "e.g. P101"

            ConfigureCombo(cmbFloor)
            cmbFloor.Items.AddRange(New Object() {
                "Floor 1",
                "Floor 2",
                "Floor 3",
                "Floor 4",
                "Floor 5"
            })

            ConfigureCombo(cmbVehicle)

            ConfigureTextBox(txtArea)
            txtArea.MaxLength = 60
            txtArea.PlaceholderText = "e.g. Main Parking Area"

            ConfigureCombo(cmbStatus)
            cmbStatus.Items.AddRange(New Object() {
                "AVAILABLE",
                "OCCUPIED",
                "MAINTENANCE"
            })

            txtNotes.Multiline = True
            txtNotes.ScrollBars = ScrollBars.Vertical
            txtNotes.MaxLength = 5000
            txtNotes.Height = 90
            txtNotes.Dock = DockStyle.Fill
            txtNotes.Font = New Font("Segoe UI", 9.5F)
            txtNotes.BorderStyle = BorderStyle.FixedSingle

            AddField(fields, "Parking Code", txtCode, 0)
            AddField(fields, "Floor", cmbFloor, 1)
            AddField(fields, "Vehicle Category", cmbVehicle, 2)
            AddField(fields, "Area", txtArea, 3)
            AddField(fields, "Operational Status", cmbStatus, 4)
            AddField(fields, "Notes", txtNotes, 5, 100)

            Dim rateHeading As New Label With {
                .Text = "Parking Rate Preview",
                .Font = New Font("Segoe UI", 11, FontStyle.Bold),
                .ForeColor = navy,
                .AutoSize = True,
                .Margin = New Padding(0, 14, 0, 6)
            }

            Dim rateContainer As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 86,
                .BackColor = bg,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 8, 0, 0)
            }

            lblRates.Dock = DockStyle.Fill
            lblRates.Font = New Font("Segoe UI", 9.0F)
            lblRates.ForeColor = navy
            lblRates.TextAlign = ContentAlignment.MiddleLeft
            lblRates.AutoEllipsis = True

            rateContainer.Controls.Add(lblRates)

            Dim rateLayout As New FlowLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .FlowDirection = FlowDirection.TopDown,
                .WrapContents = False,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            rateLayout.Controls.Add(rateHeading)
            rateLayout.Controls.Add(rateContainer)

            scrollPanel.Controls.Add(rateLayout)
            rateLayout.BringToFront()

            AddHandler cmbVehicle.SelectedIndexChanged,
                AddressOf VehicleChanged

            Dim footer As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = bg,
                .Padding = New Padding(0, 12, 0, 0)
            }

            footer.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100))

            footer.ColumnStyles.Add(
                New ColumnStyle(SizeType.Absolute, 220))

            root.Controls.Add(footer, 0, 2)

            lblMessage.Dock = DockStyle.Fill
            lblMessage.ForeColor = muted
            lblMessage.TextAlign = ContentAlignment.MiddleLeft
            lblMessage.AutoEllipsis = True
            footer.Controls.Add(lblMessage, 0, 0)

            Dim buttons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False,
                .BackColor = bg,
                .Margin = New Padding(0)
            }

            ConfigureButton(btnSave, "Save Space", teal, Color.White)
            btnSave.Size = New Size(112, 38)
            btnSave.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
            btnSave.Margin = New Padding(8, 0, 0, 0)
            btnSave.DialogResult = DialogResult.None
            AddHandler btnSave.Click, AddressOf SaveSpace

            ConfigureButton(btnCancel, "Cancel", Color.White, navy)
            btnCancel.Size = New Size(92, 38)
            btnCancel.FlatAppearance.BorderColor = borderColor
            btnCancel.Margin = New Padding(8, 0, 0, 0)
            btnCancel.DialogResult = DialogResult.Cancel

            buttons.Controls.Add(btnSave)
            buttons.Controls.Add(btnCancel)
            footer.Controls.Add(buttons, 1, 0)

            AcceptButton = btnSave
            CancelButton = btnCancel
        End Sub

        Private Sub ConfigureTextBox(textBox As TextBox)
            textBox.Dock = DockStyle.Fill
            textBox.Height = 34
            textBox.Font = New Font("Segoe UI", 9.5F)
            textBox.BorderStyle = BorderStyle.FixedSingle
        End Sub

        Private Sub ConfigureCombo(combo As ComboBox)
            combo.Dock = DockStyle.Fill
            combo.DropDownStyle = ComboBoxStyle.DropDownList
            combo.Font = New Font("Segoe UI", 9.5F)
            combo.IntegralHeight = False
            combo.DropDownHeight = 180
        End Sub

        Private Sub ConfigureButton(
            button As Button,
            caption As String,
            backColor As Color,
            foreColor As Color)

            button.Text = caption
            button.BackColor = backColor
            button.ForeColor = foreColor
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderSize =
                If(backColor = Color.White, 1, 0)
            button.Cursor = Cursors.Hand
            button.Font = New Font("Segoe UI", 9.0F)
        End Sub

        Private Sub AddField(
            layout As TableLayoutPanel,
            caption As String,
            input As Control,
            rowIndex As Integer,
            Optional rowHeight As Integer = 44)

            layout.RowCount += 1
            layout.RowStyles.Add(
                New RowStyle(SizeType.Absolute, rowHeight))

            Dim label As New Label With {
                .Text = caption,
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .ForeColor = navy,
                .Margin = New Padding(0, 3, 8, 3)
            }

            input.Margin = New Padding(0, 5, 0, 5)

            layout.Controls.Add(label, 0, rowIndex)
            layout.Controls.Add(input, 1, rowIndex)
        End Sub

        Private Sub LoadVehicleCategories()
            isLoading = True

            Try
                cmbVehicle.Items.Clear()
                cmbVehicle.Items.AddRange(New Object() {
                    "MOTORCYCLE",
                    "SEDAN",
                    "VAN",
                    "SUV"
                })

                If cmbVehicle.Items.Count > 0 Then
                    cmbVehicle.SelectedIndex = 0
                End If

                cmbStatus.SelectedItem = "AVAILABLE"

            Finally
                isLoading = False
            End Try
        End Sub

        Private Sub VehicleChanged(sender As Object, e As EventArgs)
            If isLoading Then Return
            UpdateRatePreview()
        End Sub

        Private Sub UpdateRatePreview()
            If cmbVehicle.SelectedItem Is Nothing Then
                lblRates.Text = "Select a vehicle category."
                Return
            End If

            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Dim sql As String =
                        "SELECT hourly_rate, minimum_charge, " &
                        "billing_increment_minutes " &
                        "FROM parking_rate_options " &
                        "WHERE vehicle_category=@vehicle " &
                        "AND is_active=1 LIMIT 1"

                    Using cmd As New MySqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue(
                            "@vehicle", cmbVehicle.SelectedItem.ToString())

                        Using reader = cmd.ExecuteReader()
                            If reader.Read() Then
                                lblRates.Text =
                                    "Hourly: ₱" &
                                    Convert.ToDecimal(
                                        reader("hourly_rate")).ToString("N2") &
                                    Environment.NewLine &
                                    "Minimum charge: ₱" &
                                    Convert.ToDecimal(
                                        reader("minimum_charge")).ToString("N2") &
                                    "   |   Billing increment: " &
                                    Convert.ToInt32(
                                        reader("billing_increment_minutes")).ToString() &
                                    " minute(s)"
                            Else
                                lblRates.Text =
                                    "No active rate is configured for this vehicle category."
                            End If
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                lblRates.Text = "Unable to load parking rates."
            End Try
        End Sub

        Private Sub LoadExistingSpace()
            isLoading = True

            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Dim sql As String =
                        "SELECT parking_code, floor_number, " &
                        "vehicle_category, area_name, status, notes " &
                        "FROM parking_spaces " &
                        "WHERE parking_space_id=@id AND is_active=1 LIMIT 1"

                    Using cmd As New MySqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@id", parkingSpaceId.Value)

                        Using reader = cmd.ExecuteReader()
                            If Not reader.Read() Then
                                Throw New InvalidOperationException(
                                    "The parking space no longer exists or is inactive.")
                            End If

                            originalCode = reader("parking_code").ToString()
                            originalStatus = reader("status").ToString()

                            txtCode.Text = originalCode
                            txtCode.ReadOnly = True

                            Dim floor As Integer =
                                Convert.ToInt32(reader("floor_number"))

                            If floor >= 1 AndAlso floor <= 5 Then
                                cmbFloor.SelectedIndex = floor - 1
                            End If

                            cmbVehicle.SelectedItem =
                                reader("vehicle_category").ToString()

                            txtArea.Text =
                                If(reader.IsDBNull(reader.GetOrdinal("area_name")),
                                   "",
                                   reader("area_name").ToString())

                            cmbStatus.SelectedItem = originalStatus

                            txtNotes.Text =
                                If(reader.IsDBNull(reader.GetOrdinal("notes")),
                                   "",
                                   reader("notes").ToString())
                        End Using
                    End Using
                End Using

                ' A currently occupied space must remain occupied
                ' until the operational workflow releases it.
                If originalStatus = "OCCUPIED" Then
                    cmbStatus.Enabled = False
                    lblMessage.Text =
                        "Occupied spaces cannot be changed here."
                End If

                ' Changing identity fields for existing spaces could
                ' invalidate their vehicle category and operational use.
                cmbFloor.Enabled = False
                cmbVehicle.Enabled = False

            Finally
                isLoading = False
            End Try
        End Sub

        Private Sub SaveSpace(sender As Object, e As EventArgs)
            If Not ValidateInput() Then Return

            btnSave.Enabled = False
            lblMessage.ForeColor = muted
            lblMessage.Text = "Saving parking space..."

            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Using transaction = conn.BeginTransaction()
                        Try
                            Dim selectedStatus As String =
                                cmbStatus.SelectedItem.ToString()

                            Dim vehicle As String =
                                cmbVehicle.SelectedItem.ToString()

                            ' A new space must start available.
                            If Not parkingSpaceId.HasValue AndAlso
                               selectedStatus <> "AVAILABLE" Then

                                Throw New InvalidOperationException(
                                    "New parking spaces must start as AVAILABLE.")
                            End If

                            ' Validate that an active rate exists before
                            ' adding or modifying an operational space.
                            Using rateCmd As New MySqlCommand(
                                "SELECT COUNT(*) FROM parking_rate_options " &
                                "WHERE vehicle_category=@vehicle AND is_active=1",
                                conn, transaction)

                                rateCmd.Parameters.AddWithValue(
                                    "@vehicle", vehicle)

                                If Convert.ToInt32(rateCmd.ExecuteScalar()) = 0 Then
                                    Throw New InvalidOperationException(
                                        "There is no active parking rate for " &
                                        vehicle & ". Configure the rate first.")
                                End If
                            End Using

                            If parkingSpaceId.HasValue Then
                                ' Lock the row to prevent conflicting edits.
                                Dim currentStatus As String = ""

                                Using lockCmd As New MySqlCommand(
                                    "SELECT status FROM parking_spaces " &
                                    "WHERE parking_space_id=@id " &
                                    "AND is_active=1 FOR UPDATE",
                                    conn, transaction)

                                    lockCmd.Parameters.AddWithValue(
                                        "@id", parkingSpaceId.Value)

                                    Dim result As Object =
                                        lockCmd.ExecuteScalar()

                                    If result Is Nothing OrElse
                                       result Is DBNull.Value Then

                                        Throw New InvalidOperationException(
                                            "This parking space is no longer active.")
                                    End If

                                    currentStatus = result.ToString()
                                End Using

                                If currentStatus = "OCCUPIED" AndAlso
                                   selectedStatus <> "OCCUPIED" Then

                                    Throw New InvalidOperationException(
                                        "An occupied parking space cannot be released " &
                                        "from this editor. Complete the parking " &
                                        "checkout or release workflow first.")
                                End If

                                If currentStatus <> "OCCUPIED" AndAlso
                                   selectedStatus = "OCCUPIED" Then

                                    Throw New InvalidOperationException(
                                        "Use the parking assignment or check-in " &
                                        "workflow to mark a space occupied.")
                                End If

                                Dim updateSql As String =
                                    "UPDATE parking_spaces SET " &
                                    "area_name=@area, notes=@notes, status=@status " &
                                    "WHERE parking_space_id=@id AND is_active=1"

                                Using cmd As New MySqlCommand(
                                    updateSql, conn, transaction)

                                    AddSpaceParameters(cmd, selectedStatus)

                                    cmd.Parameters.AddWithValue(
                                        "@id", parkingSpaceId.Value)

                                    If cmd.ExecuteNonQuery() <> 1 Then
                                        Throw New InvalidOperationException(
                                            "The parking space could not be updated.")
                                    End If
                                End Using

                            Else
                                Dim insertSql As String =
                                    "INSERT INTO parking_spaces " &
                                    "(parking_code, floor_number, vehicle_category, " &
                                    "area_name, status, notes, is_active) " &
                                    "VALUES " &
                                    "(@code, @floor, @vehicle, @area, " &
                                    "'AVAILABLE', @notes, 1)"

                                Using cmd As New MySqlCommand(
                                    insertSql, conn, transaction)

                                    cmd.Parameters.AddWithValue(
                                        "@code", txtCode.Text.Trim().ToUpperInvariant())

                                    cmd.Parameters.AddWithValue(
                                        "@floor", cmbFloor.SelectedIndex + 1)

                                    cmd.Parameters.AddWithValue(
                                        "@vehicle", vehicle)

                                    AddNullableTextParameter(
                                        cmd, "@area", txtArea.Text.Trim())

                                    AddNullableTextParameter(
                                        cmd, "@notes", txtNotes.Text.Trim())

                                    cmd.ExecuteNonQuery()
                                End Using
                            End If

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

                DialogResult = DialogResult.OK
                Close()

            Catch ex As MySqlException When ex.Number = 1062
                lblMessage.ForeColor = Color.Firebrick
                lblMessage.Text = "That parking code already exists."

                MessageBox.Show(
                    "The parking code must be unique. Choose another code.",
                    "Duplicate Parking Code",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

            Catch ex As Exception
                lblMessage.ForeColor = Color.Firebrick
                lblMessage.Text = "Unable to save parking space."

                MessageBox.Show(
                    "Unable to save the parking space." &
                    Environment.NewLine & ex.Message,
                    "Parking Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)

            Finally
                If Not IsDisposed Then
                    btnSave.Enabled = True
                End If
            End Try
        End Sub

        Private Sub AddSpaceParameters(
            cmd As MySqlCommand,
            selectedStatus As String)

            AddNullableTextParameter(cmd, "@area", txtArea.Text.Trim())
            AddNullableTextParameter(cmd, "@notes", txtNotes.Text.Trim())

            cmd.Parameters.AddWithValue("@status", selectedStatus)
        End Sub

        Private Sub AddNullableTextParameter(
            cmd As MySqlCommand,
            parameterName As String,
            value As String)

            If String.IsNullOrWhiteSpace(value) Then
                cmd.Parameters.AddWithValue(parameterName, DBNull.Value)
            Else
                cmd.Parameters.AddWithValue(parameterName, value)
            End If
        End Sub

        Private Function ValidateInput() As Boolean
            If String.IsNullOrWhiteSpace(txtCode.Text) OrElse
               txtCode.Text.Trim().Length <> 4 Then

                Return ShowValidationError(
                    "Parking Code must contain exactly four characters.")
            End If

            For Each character As Char In txtCode.Text.Trim()
                If Not Char.IsLetterOrDigit(character) Then
                    Return ShowValidationError(
                        "Parking Code can contain letters and numbers only.")
                End If
            Next

            If cmbFloor.SelectedIndex < 0 Then
                Return ShowValidationError("Select a floor.")
            End If

            If cmbVehicle.SelectedItem Is Nothing Then
                Return ShowValidationError("Select a vehicle category.")
            End If

            If cmbStatus.SelectedItem Is Nothing Then
                Return ShowValidationError("Select a parking status.")
            End If

            If txtArea.Text.Trim().Length > 60 Then
                Return ShowValidationError(
                    "Area name cannot exceed 60 characters.")
            End If

            Return True
        End Function

        Private Function ShowValidationError(message As String) As Boolean
            lblMessage.ForeColor = Color.Firebrick
            lblMessage.Text = message

            MessageBox.Show(
                message,
                "Check Parking Space Details",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Return False
        End Function

    End Class

End Namespace