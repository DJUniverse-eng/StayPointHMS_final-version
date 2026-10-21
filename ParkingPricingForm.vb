Imports System
Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class ParkingPricingForm
        Inherits Form

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly bg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)

        Private ReadOnly grid As New DataGridView()
        Private ReadOnly cmbVehicle As New ComboBox()
        Private ReadOnly nudHourlyRate As New NumericUpDown()
        Private ReadOnly chkActive As New CheckBox()
        Private ReadOnly lblMessage As New Label()

        Private ReadOnly btnSave As New Button()
        Private ReadOnly btnRefresh As New Button()
        Private ReadOnly btnClose As New Button()

        Private isLoading As Boolean

        Public Sub New()
            Text = "Parking Rate Management"
            StartPosition = FormStartPosition.CenterParent
            Size = New Size(850, 650)
            MinimumSize = New Size(650, 500)
            BackColor = bg
            Font = New Font("Segoe UI", 9.5F)
            FormBorderStyle = FormBorderStyle.Sizable
            MinimizeBox = False

            BuildInterface()
            LoadRates()
        End Sub

        Private Sub BuildInterface()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 4,
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
                New RowStyle(SizeType.Absolute, 165))

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 64))

            Controls.Add(root)

            Dim heading As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = bg
            }

            Dim lblTitle As New Label With {
                .Text = "Parking Rates",
                .Font = New Font("Segoe UI", 20, FontStyle.Bold),
                .ForeColor = navy,
                .AutoSize = True,
                .Location = New Point(0, 0)
            }

            Dim lblSubtitle As New Label With {
                .Text = "Configure the hourly parking rate for each vehicle category.",
                .Font = New Font("Segoe UI", 9.5F),
                .ForeColor = muted,
                .AutoSize = True,
                .Location = New Point(2, 40)
            }

            heading.Controls.Add(lblTitle)
            heading.Controls.Add(lblSubtitle)
            root.Controls.Add(heading, 0, 0)

            ConfigureGrid()
            root.Controls.Add(grid, 0, 1)

            Dim editor As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 4,
                .BackColor = Color.White,
                .Padding = New Padding(16, 10, 16, 10),
                .Margin = New Padding(0, 12, 0, 0)
            }

            editor.ColumnStyles.Add(
                New ColumnStyle(SizeType.Absolute, 170))

            editor.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100))

            editor.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 32))

            editor.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 42))

            editor.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 42))

            editor.RowStyles.Add(
                New RowStyle(SizeType.Percent, 100))

            Dim sectionTitle As New Label With {
                .Text = "Edit Rate",
                .Font = New Font("Segoe UI", 11, FontStyle.Bold),
                .ForeColor = navy,
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft
            }

            editor.Controls.Add(sectionTitle, 0, 0)
            editor.SetColumnSpan(sectionTitle, 2)

            Dim vehicleLabel As New Label With {
                .Text = "Vehicle Category",
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft,
                .ForeColor = navy
            }

            cmbVehicle.DropDownStyle = ComboBoxStyle.DropDownList
            cmbVehicle.Dock = DockStyle.Fill
            cmbVehicle.Items.AddRange(New Object() {
                "MOTORCYCLE",
                "SEDAN",
                "VAN",
                "SUV"
            })

            editor.Controls.Add(vehicleLabel, 0, 1)
            editor.Controls.Add(cmbVehicle, 1, 1)

            Dim rateLabel As New Label With {
                .Text = "Hourly Rate (₱)",
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft,
                .ForeColor = navy
            }

            nudHourlyRate.Minimum = 0
            nudHourlyRate.Maximum = 100000
            nudHourlyRate.DecimalPlaces = 2
            nudHourlyRate.Increment = 5
            nudHourlyRate.ThousandsSeparator = True
            nudHourlyRate.Dock = DockStyle.Fill
            nudHourlyRate.TextAlign = HorizontalAlignment.Right

            editor.Controls.Add(rateLabel, 0, 2)
            editor.Controls.Add(nudHourlyRate, 1, 2)

            chkActive.Text = "Rate is active"
            chkActive.Checked = True
            chkActive.Dock = DockStyle.Fill
            chkActive.ForeColor = navy

            editor.Controls.Add(chkActive, 1, 3)

            root.Controls.Add(editor, 0, 2)

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
                New ColumnStyle(SizeType.Absolute, 300))

            root.Controls.Add(footer, 0, 3)

            lblMessage.Dock = DockStyle.Fill
            lblMessage.ForeColor = muted
            lblMessage.TextAlign = ContentAlignment.MiddleLeft
            lblMessage.AutoEllipsis = True
            footer.Controls.Add(lblMessage, 0, 0)

            Dim buttons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False,
                .Margin = New Padding(0)
            }

            ConfigureButton(btnClose, "Close", Color.White, navy)
            btnClose.Size = New Size(82, 36)
            btnClose.FlatAppearance.BorderColor = borderColor
            btnClose.Margin = New Padding(8, 0, 0, 0)
            AddHandler btnClose.Click,
                Sub(sender, e) Close()

            ConfigureButton(btnRefresh, "Refresh", Color.White, navy)
            btnRefresh.Size = New Size(88, 36)
            btnRefresh.FlatAppearance.BorderColor = borderColor
            btnRefresh.Margin = New Padding(8, 0, 0, 0)
            AddHandler btnRefresh.Click,
                Sub(sender, e) LoadRates()

            ConfigureButton(btnSave, "Save Rate", teal, Color.White)
            btnSave.Size = New Size(100, 36)
            btnSave.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
            btnSave.Margin = New Padding(8, 0, 0, 0)
            AddHandler btnSave.Click, AddressOf SaveRate

            buttons.Controls.Add(btnClose)
            buttons.Controls.Add(btnRefresh)
            buttons.Controls.Add(btnSave)

            footer.Controls.Add(buttons, 1, 0)

            AddHandler grid.SelectionChanged,
                AddressOf GridSelectionChanged

            AddHandler cmbVehicle.SelectedIndexChanged,
                AddressOf VehicleSelectionChanged
        End Sub

        Private Sub ConfigureGrid()
            grid.Dock = DockStyle.Fill
            grid.BackgroundColor = Color.White
            grid.BorderStyle = BorderStyle.FixedSingle
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.AllowUserToResizeRows = False
            grid.ReadOnly = True
            grid.MultiSelect = False
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            grid.AutoGenerateColumns = False
            grid.RowHeadersVisible = False
            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill
            grid.ColumnHeadersHeight = 38
            grid.RowTemplate.Height = 36
            grid.EnableHeadersVisualStyles = False

            grid.ColumnHeadersDefaultCellStyle.BackColor = navy
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 9.0F, FontStyle.Bold)

            grid.DefaultCellStyle.Font = New Font("Segoe UI", 9.0F)
            grid.DefaultCellStyle.ForeColor = navy
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(220, 241, 238)
            grid.DefaultCellStyle.SelectionForeColor = navy

            grid.Columns.Add(New DataGridViewTextBoxColumn With {
                .Name = "option_id",
                .DataPropertyName = "option_id",
                .HeaderText = "ID",
                .FillWeight = 45
            })

            grid.Columns.Add(New DataGridViewTextBoxColumn With {
                .Name = "vehicle_category",
                .DataPropertyName = "vehicle_category",
                .HeaderText = "Vehicle Category",
                .FillWeight = 125
            })

            grid.Columns.Add(New DataGridViewTextBoxColumn With {
                .Name = "hourly_rate",
                .DataPropertyName = "hourly_rate",
                .HeaderText = "Hourly Rate (₱)",
                .FillWeight = 100,
                .DefaultCellStyle = New DataGridViewCellStyle With {
                    .Format = "N2",
                    .Alignment = DataGridViewContentAlignment.MiddleRight
                }
            })

            grid.Columns.Add(New DataGridViewCheckBoxColumn With {
                .Name = "is_active",
                .DataPropertyName = "is_active",
                .HeaderText = "Active",
                .FillWeight = 65
            })
        End Sub

        Private Sub LoadRates()
            If IsDisposed Then Return

            isLoading = True
            grid.DataSource = Nothing

            Try
                Dim dt As New DataTable()

                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Dim sql As String =
                        "SELECT option_id, vehicle_category, " &
                        "hourly_rate, is_active " &
                        "FROM parking_rate_options " &
                        "ORDER BY FIELD(vehicle_category, " &
                        "'MOTORCYCLE','SEDAN','VAN','SUV')"

                    Using cmd As New MySqlCommand(sql, conn)
                        Using adapter As New MySqlDataAdapter(cmd)
                            adapter.Fill(dt)
                        End Using
                    End Using
                End Using

                grid.DataSource = dt

                If dt.Rows.Count = 0 Then
                    lblMessage.Text = "No parking rate records found."
                Else
                    lblMessage.Text =
                        "Select a rate from the table to edit it."
                End If

                If dt.Rows.Count > 0 Then
                    grid.ClearSelection()
                    grid.Rows(0).Selected = True
                    LoadSelectedRate()
                Else
                    ClearEditor()
                End If

            Catch ex As Exception
                lblMessage.ForeColor = Color.Firebrick
                lblMessage.Text = "Unable to load parking rates."

                MessageBox.Show(
                    "Unable to load parking rates from the database." &
                    Environment.NewLine & ex.Message,
                    "Parking Rates",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            Finally
                isLoading = False
            End Try
        End Sub

        Private Sub GridSelectionChanged(sender As Object, e As EventArgs)
            If isLoading Then Return
            LoadSelectedRate()
        End Sub

        Private Sub LoadSelectedRate()
            If grid.CurrentRow Is Nothing Then Return

            Dim vehicleValue As Object =
                grid.CurrentRow.Cells("vehicle_category").Value

            If vehicleValue Is Nothing OrElse
               vehicleValue Is DBNull.Value Then Return

            isLoading = True

            Try
                cmbVehicle.SelectedItem = vehicleValue.ToString()

                Dim rateValue As Object =
                    grid.CurrentRow.Cells("hourly_rate").Value

                If rateValue IsNot Nothing AndAlso
                   rateValue IsNot DBNull.Value Then

                    nudHourlyRate.Value = Math.Max(
                        nudHourlyRate.Minimum,
                        Math.Min(
                            nudHourlyRate.Maximum,
                            Convert.ToDecimal(rateValue)))
                Else
                    nudHourlyRate.Value = 0
                End If

                Dim activeValue As Object =
                    grid.CurrentRow.Cells("is_active").Value

                chkActive.Checked =
                    activeValue IsNot Nothing AndAlso
                    activeValue IsNot DBNull.Value AndAlso
                    Convert.ToBoolean(activeValue)

                lblMessage.ForeColor = muted
                lblMessage.Text =
                    "Editing rate for " & vehicleValue.ToString() & "."

            Finally
                isLoading = False
            End Try
        End Sub

        Private Sub VehicleSelectionChanged(sender As Object, e As EventArgs)
            If isLoading OrElse cmbVehicle.SelectedItem Is Nothing Then Return

            Dim vehicle As String = cmbVehicle.SelectedItem.ToString()

            For Each row As DataGridViewRow In grid.Rows
                If row.IsNewRow Then Continue For

                If String.Equals(
                    Convert.ToString(row.Cells("vehicle_category").Value),
                    vehicle,
                    StringComparison.OrdinalIgnoreCase) Then

                    isLoading = True

                    Try
                        grid.ClearSelection()
                        row.Selected = True
                        grid.CurrentCell = row.Cells("vehicle_category")
                        LoadSelectedRate()
                    Finally
                        isLoading = False
                    End Try

                    Return
                End If
            Next

            lblMessage.ForeColor = muted
            lblMessage.Text =
                "No saved rate exists for " & vehicle & "."
        End Sub

        Private Sub SaveRate(sender As Object, e As EventArgs)
            If cmbVehicle.SelectedItem Is Nothing Then
                MessageBox.Show(
                    "Select a vehicle category.",
                    "Parking Rates",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
                Return
            End If

            Dim vehicle As String = cmbVehicle.SelectedItem.ToString()
            Dim rate As Decimal = nudHourlyRate.Value

            If chkActive.Checked AndAlso rate <= 0D Then
                MessageBox.Show(
                    "An active parking rate must be greater than ₱0.00.",
                    "Invalid Rate",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
                Return
            End If

            btnSave.Enabled = False

            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Using transaction = conn.BeginTransaction()
                        Try
                            ' Lock an existing category rate, if present.
                            Dim existingId As Object

                            Using findCmd As New MySqlCommand(
                                "SELECT option_id " &
                                "FROM parking_rate_options " &
                                "WHERE vehicle_category=@vehicle " &
                                "LIMIT 1 FOR UPDATE",
                                conn, transaction)

                                findCmd.Parameters.AddWithValue(
                                    "@vehicle", vehicle)

                                existingId = findCmd.ExecuteScalar()
                            End Using

                            If existingId Is Nothing OrElse
                               existingId Is DBNull.Value Then

                                Using insertCmd As New MySqlCommand(
                                    "INSERT INTO parking_rate_options " &
                                    "(vehicle_category, hourly_rate, is_active) " &
                                    "VALUES (@vehicle, @rate, @active)",
                                    conn, transaction)

                                    insertCmd.Parameters.AddWithValue(
                                        "@vehicle", vehicle)

                                    insertCmd.Parameters.AddWithValue(
                                        "@rate", rate)

                                    insertCmd.Parameters.AddWithValue(
                                        "@active", chkActive.Checked)

                                    insertCmd.ExecuteNonQuery()
                                End Using

                            Else
                                Using updateCmd As New MySqlCommand(
                                    "UPDATE parking_rate_options " &
                                    "SET hourly_rate=@rate, is_active=@active " &
                                    "WHERE option_id=@id",
                                    conn, transaction)

                                    updateCmd.Parameters.AddWithValue(
                                        "@rate", rate)

                                    updateCmd.Parameters.AddWithValue(
                                        "@active", chkActive.Checked)

                                    updateCmd.Parameters.AddWithValue(
                                        "@id", Convert.ToUInt64(existingId))

                                    updateCmd.ExecuteNonQuery()
                                End Using
                            End If

                            transaction.Commit()

                        Catch
                            Try
                                transaction.Rollback()
                            Catch
                                ' Keep the original database exception.
                            End Try

                            Throw
                        End Try
                    End Using
                End Using

                lblMessage.ForeColor = teal
                lblMessage.Text =
                    "Rate saved for " & vehicle & "."

                LoadRates()

                For Each row As DataGridViewRow In grid.Rows
                    If String.Equals(
                        Convert.ToString(row.Cells("vehicle_category").Value),
                        vehicle,
                        StringComparison.OrdinalIgnoreCase) Then

                        row.Selected = True
                        grid.CurrentCell = row.Cells("vehicle_category")
                        Exit For
                    End If
                Next

                DialogResult = DialogResult.OK

            Catch ex As MySqlException
                lblMessage.ForeColor = Color.Firebrick
                lblMessage.Text = "Unable to save parking rate."

                MessageBox.Show(
                    "The parking rate could not be saved." &
                    Environment.NewLine & ex.Message,
                    "Parking Rates",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)

            Catch ex As Exception
                lblMessage.ForeColor = Color.Firebrick
                lblMessage.Text = "Unable to save parking rate."

                MessageBox.Show(
                    "An unexpected error occurred while saving the rate." &
                    Environment.NewLine & ex.Message,
                    "Parking Rates",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)

            Finally
                btnSave.Enabled = True
            End Try
        End Sub

        Private Sub ClearEditor()
            isLoading = True

            Try
                cmbVehicle.SelectedIndex = -1
                nudHourlyRate.Value = 0
                chkActive.Checked = False
            Finally
                isLoading = False
            End Try
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

    End Class

End Namespace