
Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class StaffGuestRegistrationForm
        Inherits Form

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly sidebarColor As Color = Color.FromArgb(32, 58, 95)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly pageBg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)

        Private ReadOnly searchBox As New TextBox()
        Private ReadOnly guestGrid As New DataGridView()
        Private ReadOnly firstNameBox As New TextBox()
        Private ReadOnly lastNameBox As New TextBox()
        Private ReadOnly phoneBox As New TextBox()
        Private ReadOnly emailBox As New TextBox()
        Private ReadOnly addressBox As New TextBox()
        Private ReadOnly idTypeBox As New ComboBox()
        Private ReadOnly idReferenceBox As New TextBox()
        Private ReadOnly notesBox As New TextBox()

        Private ReadOnly roomBox As New ComboBox()
        Private ReadOnly adultsBox As New NumericUpDown()
        Private ReadOnly childrenBox As New NumericUpDown()
        Private ReadOnly checkoutPicker As New DateTimePicker()
        Private ReadOnly rateSummary As New Label()
        Private ReadOnly statusLabel As New Label()

        Private ReadOnly selectedGuestLabel As New Label()
        Private ReadOnly newGuestButton As New Button()
        Private ReadOnly clearGuestButton As New Button()
        Private ReadOnly saveButton As New Button()

        Private ReadOnly logo As New PictureBox()

        Private _selectedGuestId As Long? = Nothing
        Private _loadingGuests As Boolean
        Private _saving As Boolean

        Private NotInheritable Class RoomChoice
            Public Property RoomId As Long
            Public Property RoomNumber As String
            Public Property RoomType As String
            Public Property BedName As String
            Public Property FloorNumber As Integer
            Public Property Capacity As Integer
            Public Property RateOptionId As Long
            Public Property HourlyRate As Decimal
            Public Property AdditionalHourlyRate As Decimal
            Public Property DailyRate As Decimal
            Public Property MinimumHours As Integer

            Public Overrides Function ToString() As String
                Return RoomNumber & " | " & RoomType & " | " &
                       BedName & " | Floor " & FloorNumber
            End Function
        End Class

        Public Sub New()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.StaffRoom)

            Text = "StayPoint HMS - Guest Registration / Check-In"
            StartPosition = FormStartPosition.CenterParent
            WindowState = FormWindowState.Maximized
            MinimumSize = New Size(950, 650)
            BackColor = pageBg
            Font = New Font("Segoe UI", 9.0F)
            AutoScaleMode = AutoScaleMode.Dpi

            BuildLayout()
            LoadGuests(String.Empty)
            LoadAvailableRooms()
        End Sub

        Private Sub BuildLayout()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 76))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))

            root.Controls.Add(BuildHeader(), 0, 0)
            root.Controls.Add(BuildMainContent(), 0, 1)
            root.Controls.Add(BuildFooter(), 0, 2)

            Controls.Add(root)
        End Sub

        Private Function BuildHeader() As Control
            Dim header As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = navy,
                .Padding = New Padding(18, 8, 18, 8)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = navy,
                .Margin = New Padding(0)
            }

            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 70))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30))

            Dim brandPanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .WrapContents = False,
                .FlowDirection = FlowDirection.LeftToRight,
                .BackColor = navy,
                .Margin = New Padding(0)
            }

            logo.Size = New Size(54, 54)
            logo.SizeMode = PictureBoxSizeMode.Zoom
            logo.Margin = New Padding(0, 1, 10, 0)
            logo.Image = LoadLogo()

            Dim title As New Label With {
                .Text = "STAYPOINT HMS" & Environment.NewLine &
                        "GUEST REGISTRATION / CHECK-IN",
                .AutoSize = True,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
                .Margin = New Padding(0, 5, 0, 0)
            }

            brandPanel.Controls.Add(logo)
            brandPanel.Controls.Add(title)

            Dim staff As New Label With {
                .Text = "Staff: " & SafeStaffName(),
                .Dock = DockStyle.Fill,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleRight,
                .AutoEllipsis = True,
                .Margin = New Padding(4)
            }

            layout.Controls.Add(brandPanel, 0, 0)
            layout.Controls.Add(staff, 1, 0)
            header.Controls.Add(layout)

            Return header
        End Function

        Private Function BuildMainContent() As Control
            Dim main As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .Padding = New Padding(16, 12, 16, 10),
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            main.RowStyles.Add(New RowStyle(SizeType.Absolute, 65))
            main.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim headingPanel As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            headingPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 36))
            headingPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim heading As New Label With {
                .Text = "Guest Registration and Check-In",
                .Dock = DockStyle.Fill,
                .ForeColor = navy,
                .Font = New Font("Segoe UI", 18.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(0)
            }

            Dim subtitle As New Label With {
                .Text = "Select an existing guest or enter a new guest's details, then assign an available room.",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 8.5F),
                .AutoEllipsis = True,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(0)
            }

            headingPanel.Controls.Add(heading, 0, 0)
            headingPanel.Controls.Add(subtitle, 0, 1)

            Dim content As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            content.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 43))
            content.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 57))

            content.Controls.Add(BuildGuestPanel(), 0, 0)
            content.Controls.Add(BuildStayPanel(), 1, 0)

            headingPanel.Margin = New Padding(0, 0, 0, 4)
            main.Controls.Add(headingPanel, 0, 0)
            main.Controls.Add(content, 0, 1)

            Return main
        End Function

        Private Function BuildGuestPanel() As Control
            Dim panel As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 0, 7, 0)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 5,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 30))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 36))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 33))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 42))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 58))

            Dim heading As Label = CreateSectionTitle("Guest Search")
            layout.Controls.Add(heading, 0, 0)

            searchBox.Dock = DockStyle.Fill
            searchBox.PlaceholderText = "Search name, phone, or email..."
            searchBox.Margin = New Padding(0, 2, 0, 5)
            AddHandler searchBox.TextChanged,
                AddressOf HandleGuestSearchChanged

            layout.Controls.Add(searchBox, 0, 1)

            Dim guestButtons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents = False,
                .BackColor = Color.White,
                .Margin = New Padding(0, 0, 0, 4)
            }

            newGuestButton.Text = "New Guest"
            StyleButton(newGuestButton, teal, Color.White, 100)
            newGuestButton.Width = 100

            clearGuestButton.Text = "Clear Selection"
            StyleButton(clearGuestButton, Color.White, navy, 125)
            clearGuestButton.Width = 125

            AddHandler newGuestButton.Click,
                Sub(sender As Object, e As EventArgs)
                    ClearGuestDetails()
                    statusLabel.Text = "Enter details for a new guest."
                    statusLabel.ForeColor = muted
                End Sub

            AddHandler clearGuestButton.Click,
                Sub(sender As Object, e As EventArgs)
                    _selectedGuestId = Nothing
                    ClearGuestDetails()
                    guestGrid.ClearSelection()
                    selectedGuestLabel.Text = "No existing guest selected. New guest details will be saved."
                    statusLabel.Text = "New guest registration mode."
                    statusLabel.ForeColor = muted
                End Sub

            guestButtons.Controls.Add(newGuestButton)
            guestButtons.Controls.Add(clearGuestButton)
            layout.Controls.Add(guestButtons, 0, 2)

            ConfigureGrid(guestGrid)
            AddHandler guestGrid.CellClick, AddressOf HandleGuestGridClick
            layout.Controls.Add(guestGrid, 0, 3)

            Dim detailsPanel As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(0, 8, 0, 0),
                .AutoScroll = True,
                .Margin = New Padding(0)
            }

            Dim details As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 2,
                .RowCount = 7,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            details.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            details.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))

            For i As Integer = 0 To 5
                details.RowStyles.Add(New RowStyle(SizeType.Absolute, 58))
            Next
            details.RowStyles.Add(New RowStyle(SizeType.Absolute, 75))

            AddField(details, "First Name *", firstNameBox, 0, 0)
            AddField(details, "Last Name *", lastNameBox, 1, 0)
            AddField(details, "Phone", phoneBox, 0, 1)
            AddField(details, "Email", emailBox, 1, 1)

            idTypeBox.DropDownStyle = ComboBoxStyle.DropDownList
            idTypeBox.Items.AddRange(New Object() {
                "Not provided", "National ID", "Passport",
                "Driver's License", "Other"
            })
            idTypeBox.SelectedIndex = 0

            AddField(details, "Identification Type", idTypeBox, 0, 2)
            AddField(details, "Identification Reference", idReferenceBox, 1, 2)

            AddField(details, "Address", addressBox, 0, 3, 2)

            notesBox.Multiline = True
            notesBox.ScrollBars = ScrollBars.Vertical
            AddField(details, "Guest Notes", notesBox, 0, 4, 2)

            selectedGuestLabel.Text =
                "No existing guest selected. New guest details will be saved."
            selectedGuestLabel.Dock = DockStyle.Fill
            selectedGuestLabel.ForeColor = muted
            selectedGuestLabel.Font = New Font("Segoe UI", 8.0F)
            selectedGuestLabel.AutoEllipsis = True
            selectedGuestLabel.Margin = New Padding(0, 5, 0, 0)

            details.Controls.Add(selectedGuestLabel, 0, 5)
            details.SetColumnSpan(selectedGuestLabel, 2)

            detailsPanel.Controls.Add(details)
            layout.Controls.Add(detailsPanel, 0, 4)

            panel.Controls.Add(layout)
            Return panel
        End Function

        Private Function BuildStayPanel() As Control
            Dim panel As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(14),
                .Margin = New Padding(7, 0, 0, 0)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 8,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 65))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 60))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 62))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 65))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 80))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))

            layout.Controls.Add(CreateSectionTitle("Stay and Room Details"), 0, 0)

            Dim roomField As Control = CreateLabeledControl(
                "Available Room *", roomBox)

            roomBox.Dock = DockStyle.Fill
            roomBox.DropDownStyle = ComboBoxStyle.DropDownList
            AddHandler roomBox.SelectedIndexChanged,
                AddressOf HandleRoomSelectionChanged

            layout.Controls.Add(roomField, 0, 1)

            Dim occupancy As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            occupancy.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            occupancy.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))

            adultsBox.Minimum = 1
            adultsBox.Maximum = 20
            adultsBox.Value = 1
            adultsBox.Dock = DockStyle.Fill

            childrenBox.Minimum = 0
            childrenBox.Maximum = 20
            childrenBox.Value = 0
            childrenBox.Dock = DockStyle.Fill

            occupancy.Controls.Add(
                CreateLabeledControl("Adults", adultsBox), 0, 0)
            occupancy.Controls.Add(
                CreateLabeledControl("Children", childrenBox), 1, 0)

            layout.Controls.Add(occupancy, 0, 2)

            checkoutPicker.Format = DateTimePickerFormat.Custom
            checkoutPicker.CustomFormat = "MMM dd, yyyy  hh:mm tt"
            checkoutPicker.ShowUpDown = False
            checkoutPicker.Value = DateTime.Now.AddHours(24)
            checkoutPicker.MinDate = DateTime.Now.AddMinutes(1)
            checkoutPicker.Dock = DockStyle.Fill

            layout.Controls.Add(
                CreateLabeledControl("Expected Check-Out *", checkoutPicker),
                0, 3)

            rateSummary.Text = "Select a room to view its configured rates."
            rateSummary.Dock = DockStyle.Fill
            rateSummary.ForeColor = muted
            rateSummary.Font = New Font("Segoe UI", 9.0F)
            rateSummary.TextAlign = ContentAlignment.MiddleLeft
            rateSummary.Padding = New Padding(10, 5, 10, 5)
            rateSummary.BackColor = Color.FromArgb(248, 250, 252)
            rateSummary.Margin = New Padding(0, 6, 0, 4)

            layout.Controls.Add(rateSummary, 0, 4)

            Dim info As New Label With {
                .Text = "Check-in time will be recorded automatically when you save. " &
                        "Room rates are loaded from Admin System Settings. " &
                        "The room, stay, assignment, and folio are saved together.",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 9.0F),
                .TextAlign = ContentAlignment.TopLeft,
                .Margin = New Padding(0, 8, 0, 0)
            }

            layout.Controls.Add(info, 0, 5)

            statusLabel.Text = "Ready."
            statusLabel.Dock = DockStyle.Fill
            statusLabel.ForeColor = muted
            statusLabel.Font = New Font("Segoe UI", 8.5F)
            statusLabel.TextAlign = ContentAlignment.MiddleLeft
            statusLabel.AutoEllipsis = True
            statusLabel.Margin = New Padding(0)

            layout.Controls.Add(statusLabel, 0, 6)

            Dim actionPanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            saveButton.Text = "Register Guest and Check In"
            StyleButton(saveButton, teal, Color.White, 235)
            saveButton.Height = 36
            AddHandler saveButton.Click, AddressOf HandleSaveClick

            Dim refreshRoomsButton As New Button With {
                .Text = "Refresh Rooms",
                .Width = 115,
                .Height = 36
            }
            StyleButton(refreshRoomsButton, Color.White, navy, 115)

            AddHandler refreshRoomsButton.Click,
                Sub(sender As Object, e As EventArgs)
                    LoadAvailableRooms()
                End Sub

            actionPanel.Controls.Add(saveButton)
            actionPanel.Controls.Add(refreshRoomsButton)
            layout.Controls.Add(actionPanel, 0, 7)

            panel.Controls.Add(layout)
            Return panel
        End Function

        Private Function BuildFooter() As Control
            Dim footer As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(12, 4, 12, 4)
            }

            Dim backButton As New Button With {
                .Text = "Close",
                .Dock = DockStyle.Right,
                .Width = 95,
                .BackColor = Color.White,
                .ForeColor = navy,
                .FlatStyle = FlatStyle.Flat,
                .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .Cursor = Cursors.Hand,
                .UseVisualStyleBackColor = False
            }

            backButton.FlatAppearance.BorderColor = borderColor
            backButton.FlatAppearance.BorderSize = 1
            AddHandler backButton.Click,
                Sub(sender As Object, e As EventArgs)
                    Close()
                End Sub

            Dim footerText As New Label With {
                .Text = "StayPoint HMS | Staff Room Operations",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Font = New Font("Segoe UI", 8.0F)
            }

            footer.Controls.Add(footerText)
            footer.Controls.Add(backButton)
            Return footer
        End Function

        Private Function CreateSectionTitle(caption As String) As Label
            Return New Label With {
                .Text = caption,
                .Dock = DockStyle.Fill,
                .ForeColor = navy,
                .Font = New Font("Segoe UI", 11.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(0)
            }
        End Function

        Private Sub AddField(
            table As TableLayoutPanel,
            caption As String,
            input As Control,
            column As Integer,
            row As Integer,
            Optional columnSpan As Integer = 1)

            Dim host As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = Color.White,
                .Padding = New Padding(4, 2, 4, 2),
                .Margin = New Padding(0)
            }

            host.RowStyles.Add(New RowStyle(SizeType.Absolute, 20))
            host.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim label As New Label With {
                .Text = caption,
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 8.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(0)
            }

            input.Dock = DockStyle.Fill
            input.Font = New Font("Segoe UI", 9.0F)
            input.Margin = New Padding(0, 1, 0, 1)

            If TypeOf input Is TextBox Then
                DirectCast(input, TextBox).BorderStyle = BorderStyle.FixedSingle
            ElseIf TypeOf input Is ComboBox Then
                DirectCast(input, ComboBox).DropDownStyle =
                    ComboBoxStyle.DropDownList
            End If

            host.Controls.Add(label, 0, 0)
            host.Controls.Add(input, 0, 1)

            table.Controls.Add(host, column, row)

            If columnSpan > 1 Then
                table.SetColumnSpan(host, columnSpan)
            End If
        End Sub

        Private Function CreateLabeledControl(
            caption As String,
            input As Control) As Control

            Dim host As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = Color.White,
                .Margin = New Padding(0, 2, 0, 2)
            }

            host.RowStyles.Add(New RowStyle(SizeType.Absolute, 23))
            host.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim label As New Label With {
                .Text = caption,
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 8.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(0)
            }

            input.Dock = DockStyle.Fill
            input.Margin = New Padding(0)

            host.Controls.Add(label, 0, 0)
            host.Controls.Add(input, 0, 1)
            Return host
        End Function

        Private Sub StyleButton(
            button As Button,
            background As Color,
            foreground As Color,
            width As Integer)

            button.Width = width
            button.Height = 31
            button.BackColor = background
            button.ForeColor = foreground
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderColor = borderColor
            button.FlatAppearance.BorderSize = 1
            button.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
            button.Cursor = Cursors.Hand
            button.UseVisualStyleBackColor = False
            button.Margin = New Padding(0, 0, 7, 0)
        End Sub

        Private Sub ConfigureGrid(grid As DataGridView)
            grid.Dock = DockStyle.Fill
            grid.BackgroundColor = Color.White
            grid.BorderStyle = BorderStyle.None
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.AllowUserToResizeRows = False
            grid.ReadOnly = True
            grid.MultiSelect = False
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            grid.RowHeadersVisible = False
            grid.AutoGenerateColumns = True
            grid.EnableHeadersVisualStyles = False
            grid.GridColor = borderColor
            grid.ColumnHeadersHeight = 30
            grid.RowTemplate.Height = 25
            grid.ScrollBars = ScrollBars.Both

            grid.ColumnHeadersDefaultCellStyle.BackColor = navy
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 8.0F, FontStyle.Bold)

            grid.DefaultCellStyle.Font = New Font("Segoe UI", 8.0F)
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(220, 242, 239)
            grid.DefaultCellStyle.SelectionForeColor = navy
        End Sub

        Private Sub HandleGuestSearchChanged(sender As Object, e As EventArgs)
            LoadGuests(searchBox.Text.Trim())
        End Sub

        Private Sub LoadGuests(searchText As String)
            If _loadingGuests Then Return

            _loadingGuests = True

            Try
                Dim table As New DataTable()

                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Const sql As String =
                        "SELECT guest_id AS GuestID, " &
                        "CONCAT(first_name, ' ', last_name) AS `Guest Name`, " &
                        "COALESCE(phone, '') AS Phone, " &
                        "COALESCE(email, '') AS Email " &
                        "FROM guests " &
                        "WHERE (@search = '' " &
                        "OR first_name LIKE @pattern " &
                        "OR last_name LIKE @pattern " &
                        "OR phone LIKE @pattern " &
                        "OR email LIKE @pattern) " &
                        "ORDER BY last_name, first_name LIMIT 300;"

                    Using command As New MySqlCommand(sql, connection)
                        command.Parameters.AddWithValue("@search", searchText)
                        command.Parameters.AddWithValue(
                            "@pattern", "%" & searchText & "%")

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            table.Load(reader)
                        End Using
                    End Using
                End Using

                guestGrid.DataSource = table

                If guestGrid.Columns.Contains("GuestID") Then
                    guestGrid.Columns("GuestID").Visible = False
                End If

                guestGrid.ClearSelection()

            Catch ex As Exception
                statusLabel.Text = "Unable to load guest records."
                statusLabel.ForeColor = Color.Firebrick
            Finally
                _loadingGuests = False
            End Try
        End Sub

        Private Sub HandleGuestGridClick(sender As Object, e As DataGridViewCellEventArgs)
            If e.RowIndex < 0 OrElse guestGrid.Rows.Count = 0 Then Return

            Try
                Dim row As DataGridViewRow = guestGrid.Rows(e.RowIndex)
                Dim value As Object = row.Cells("GuestID").Value

                If value Is Nothing OrElse value Is DBNull.Value Then Return

                Dim guestId As Long = Convert.ToInt64(value)

                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Const sql As String =
                        "SELECT guest_id, first_name, last_name, phone, email, " &
                        "address, identification_type, identification_reference, notes " &
                        "FROM guests WHERE guest_id = @guest_id LIMIT 1;"

                    Using command As New MySqlCommand(sql, connection)
                        command.Parameters.AddWithValue("@guest_id", guestId)

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            If Not reader.Read() Then Return

                            _selectedGuestId = guestId
                            firstNameBox.Text = SafeString(reader("first_name"))
                            lastNameBox.Text = SafeString(reader("last_name"))
                            phoneBox.Text = SafeString(reader("phone"))
                            emailBox.Text = SafeString(reader("email"))
                            addressBox.Text = SafeString(reader("address"))
                            idReferenceBox.Text =
                                SafeString(reader("identification_reference"))
                            notesBox.Text = SafeString(reader("notes"))

                            Dim identificationType As String =
                                SafeString(reader("identification_type"))

                            If String.IsNullOrWhiteSpace(identificationType) Then
                                idTypeBox.SelectedIndex = 0
                            Else
                                Dim index As Integer =
                                    idTypeBox.Items.IndexOf(identificationType)

                                If index < 0 Then
                                    idTypeBox.SelectedItem = "Other"
                                Else
                                    idTypeBox.SelectedIndex = index
                                End If
                            End If
                        End Using
                    End Using
                End Using

                selectedGuestLabel.Text =
                    "Selected existing guest ID: " & guestId.ToString() &
                    ". Saving will not create a duplicate guest."

                statusLabel.Text = "Existing guest selected."
                statusLabel.ForeColor = teal

            Catch ex As Exception
                MessageBox.Show(
                    "Unable to select this guest." &
                    Environment.NewLine & ex.Message,
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub ClearGuestDetails()
            _selectedGuestId = Nothing
            firstNameBox.Clear()
            lastNameBox.Clear()
            phoneBox.Clear()
            emailBox.Clear()
            addressBox.Clear()
            idReferenceBox.Clear()
            notesBox.Clear()
            idTypeBox.SelectedIndex = 0

            selectedGuestLabel.Text =
                "No existing guest selected. New guest details will be saved."
        End Sub

        Private Sub LoadAvailableRooms()
            Dim previousRoomId As Long? = Nothing

            If TypeOf roomBox.SelectedItem Is RoomChoice Then
                previousRoomId =
                    DirectCast(roomBox.SelectedItem, RoomChoice).RoomId
            End If

            roomBox.BeginUpdate()

            Try
                roomBox.Items.Clear()

                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Const sql As String =
                        "SELECT r.room_id, r.room_number, r.floor_number, " &
                        "r.capacity, rt.type_name, o.option_id, o.bed_name, " &
                        "o.hourly_rate, o.additional_hourly_rate, o.daily_rate, " &
                        "o.minimum_hours " &
                        "FROM rooms r " &
                        "INNER JOIN room_types rt ON rt.room_type_id = r.room_type_id " &
                        "INNER JOIN room_type_options o " &
                        "ON o.room_type_id = r.room_type_id " &
                        "AND o.bed_code = r.bed_code " &
                        "AND o.is_active = 1 " &
                        "WHERE r.is_active = 1 AND r.status = 'AVAILABLE' " &
                        "ORDER BY r.floor_number, r.room_number;"

                    Using command As New MySqlCommand(sql, connection)
                        Using reader As MySqlDataReader = command.ExecuteReader()
                            While reader.Read()
                                Dim choice As New RoomChoice With {
                                    .RoomId = Convert.ToInt64(reader("room_id")),
                                    .RoomNumber = SafeString(reader("room_number")),
                                    .RoomType = SafeString(reader("type_name")),
                                    .BedName = SafeString(reader("bed_name")),
                                    .FloorNumber = Convert.ToInt32(reader("floor_number")),
                                    .Capacity = Convert.ToInt32(reader("capacity")),
                                    .RateOptionId = Convert.ToInt64(reader("option_id")),
                                    .HourlyRate = Convert.ToDecimal(reader("hourly_rate")),
                                    .AdditionalHourlyRate =
                                        Convert.ToDecimal(reader("additional_hourly_rate")),
                                    .DailyRate = Convert.ToDecimal(reader("daily_rate")),
                                    .MinimumHours = Convert.ToInt32(reader("minimum_hours"))
                                }

                                roomBox.Items.Add(choice)
                            End While
                        End Using
                    End Using
                End Using

                If previousRoomId.HasValue Then
                    For i As Integer = 0 To roomBox.Items.Count - 1
                        Dim choice As RoomChoice =
                            DirectCast(roomBox.Items(i), RoomChoice)

                        If choice.RoomId = previousRoomId.Value Then
                            roomBox.SelectedIndex = i
                            Exit For
                        End If
                    Next
                End If

                If roomBox.SelectedIndex < 0 AndAlso roomBox.Items.Count > 0 Then
                    roomBox.SelectedIndex = 0
                End If

                If roomBox.Items.Count = 0 Then
                    rateSummary.Text =
                        "No available rooms with active room rates were found."
                    statusLabel.Text = "No rooms are currently available."
                    statusLabel.ForeColor = Color.DarkOrange
                Else
                    statusLabel.Text =
                        roomBox.Items.Count.ToString() & " available room option(s) loaded."
                    statusLabel.ForeColor = muted
                End If

            Catch ex As Exception
                rateSummary.Text = "Unable to load room rates."
                statusLabel.Text = "Unable to load available rooms."
                statusLabel.ForeColor = Color.Firebrick

                MessageBox.Show(
                    "Unable to load available rooms and configured rates." &
                    Environment.NewLine & Environment.NewLine &
                    ex.Message,
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            Finally
                roomBox.EndUpdate()
            End Try
        End Sub

        Private Sub HandleRoomSelectionChanged(sender As Object, e As EventArgs)
            If Not TypeOf roomBox.SelectedItem Is RoomChoice Then
                rateSummary.Text = "Select a room to view its configured rates."
                Return
            End If

            Dim room As RoomChoice = DirectCast(roomBox.SelectedItem, RoomChoice)

            rateSummary.Text =
                "Hourly: " & FormatPeso(room.HourlyRate) &
                " | Additional hour: " & FormatPeso(room.AdditionalHourlyRate) &
                Environment.NewLine &
                "Daily: " & FormatPeso(room.DailyRate) &
                " | Minimum hours: " & room.MinimumHours.ToString() &
                " | Capacity: " & room.Capacity.ToString()
        End Sub

        Private Sub HandleSaveClick(sender As Object, e As EventArgs)
            If _saving Then Return

            If Not ValidateInput() Then Return

            _saving = True
            saveButton.Enabled = False
            Cursor = Cursors.WaitCursor

            Try
                Dim newStayReference As String = GenerateStayReference()
                Dim checkedInAt As DateTime = DateTime.Now
                Dim staffId As Long? = GetCurrentStaffId()

                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Using transaction As MySqlTransaction =
                        connection.BeginTransaction(IsolationLevel.ReadCommitted)

                        Try
                            Dim guestId As Long

                            If _selectedGuestId.HasValue Then
                                guestId = _selectedGuestId.Value

                                Using guestCommand As New MySqlCommand(
                                    "SELECT guest_id FROM guests " &
                                    "WHERE guest_id = @guest_id FOR UPDATE;",
                                    connection, transaction)

                                    guestCommand.Parameters.AddWithValue(
                                        "@guest_id", guestId)

                                    Dim guestResult As Object =
                                        guestCommand.ExecuteScalar()

                                    If guestResult Is Nothing OrElse
                                       guestResult Is DBNull.Value Then
                                        Throw New InvalidOperationException(
                                            "The selected guest no longer exists. Refresh the guest list.")
                                    End If
                                End Using
                            Else
                                guestId = InsertGuest(connection, transaction)
                            End If

                            Dim room As RoomChoice =
                                DirectCast(roomBox.SelectedItem, RoomChoice)

                            Dim roomInfo As RoomChoice =
                                LockAndValidateRoom(
                                    connection, transaction, room.RoomId)

                            If roomInfo Is Nothing Then
                                Throw New InvalidOperationException(
                                    "This room is no longer available or its active rate was changed. Refresh rooms and try again.")
                            End If

                            If Convert.ToInt32(adultsBox.Value + childrenBox.Value) >
                               roomInfo.Capacity Then
                                Throw New InvalidOperationException(
                                    "The number of guests exceeds the selected room's capacity.")
                            End If

                            Dim stayId As Long =
                                InsertStay(
                                    connection,
                                    transaction,
                                    guestId,
                                    newStayReference,
                                    checkedInAt,
                                    staffId)

                            InsertRoomAssignment(
                                connection,
                                transaction,
                                stayId,
                                roomInfo,
                                staffId)

                            InsertGuestFolio(
                                connection,
                                transaction,
                                stayId,
                                staffId)

                            Using updateRoom As New MySqlCommand(
                                "UPDATE rooms SET status = 'OCCUPIED' " &
                                "WHERE room_id = @room_id " &
                                "AND status = 'AVAILABLE' AND is_active = 1;",
                                connection, transaction)

                                updateRoom.Parameters.AddWithValue(
                                    "@room_id", roomInfo.RoomId)

                                If updateRoom.ExecuteNonQuery() <> 1 Then
                                    Throw New InvalidOperationException(
                                        "The room could not be marked occupied. No check-in changes were saved.")
                                End If
                            End Using

                            transaction.Commit()

                            DialogResult = DialogResult.OK
                            Close()
                            Return

                            MessageBox.Show(
                                "Guest check-in completed successfully." &
                                Environment.NewLine & Environment.NewLine &
                                "Stay Reference: " & newStayReference &
                                Environment.NewLine &
                                "Guest ID: " & guestId.ToString() &
                                Environment.NewLine &
                                "Room: " & roomInfo.RoomNumber &
                                Environment.NewLine &
                                "Check-In: " & checkedInAt.ToString("MMM dd, yyyy hh:mm tt"),
                                "StayPoint HMS",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information)

                            ClearGuestDetails()
                            searchBox.Clear()
                            LoadGuests(String.Empty)
                            LoadAvailableRooms()

                        Catch
                            Try
                                transaction.Rollback()
                            Catch
                                ' Preserve the original transaction error.
                            End Try

                            Throw
                        End Try
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show(
                    "Guest check-in was not completed." &
                    Environment.NewLine & Environment.NewLine &
                    ex.Message,
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)

                statusLabel.Text = "Check-in failed. No partial changes were committed."
                statusLabel.ForeColor = Color.Firebrick
            Finally
                Cursor = Cursors.Default
                saveButton.Enabled = True
                _saving = False
            End Try
        End Sub

        Private Function ValidateInput() As Boolean
            If String.IsNullOrWhiteSpace(firstNameBox.Text) Then
                Return RejectInput("Enter the guest's first name.", firstNameBox)
            End If

            If String.IsNullOrWhiteSpace(lastNameBox.Text) Then
                Return RejectInput("Enter the guest's last name.", lastNameBox)
            End If

            If firstNameBox.Text.Trim().Length > 80 OrElse
               lastNameBox.Text.Trim().Length > 80 Then
                Return RejectInput(
                    "First and last names must not exceed 80 characters.",
                    firstNameBox)
            End If

            If phoneBox.Text.Trim().Length > 30 Then
                Return RejectInput("Phone must not exceed 30 characters.", phoneBox)
            End If

            If emailBox.Text.Trim().Length > 120 Then
                Return RejectInput("Email must not exceed 120 characters.", emailBox)
            End If

            If addressBox.Text.Trim().Length > 300 Then
                Return RejectInput("Address must not exceed 300 characters.", addressBox)
            End If

            If idReferenceBox.Text.Trim().Length > 100 Then
                Return RejectInput(
                    "Identification reference must not exceed 100 characters.",
                    idReferenceBox)
            End If

            If Not String.IsNullOrWhiteSpace(emailBox.Text) AndAlso
               Not emailBox.Text.Contains("@") Then
                Return RejectInput("Enter a valid email address or leave it blank.", emailBox)
            End If

            If Not TypeOf roomBox.SelectedItem Is RoomChoice Then
                Return RejectInput("Select an available room.", roomBox)
            End If

            If checkoutPicker.Value <= DateTime.Now Then
                Return RejectInput(
                    "Expected check-out must be later than the current time.",
                    checkoutPicker)
            End If

            Dim room As RoomChoice = DirectCast(roomBox.SelectedItem, RoomChoice)

            If adultsBox.Value + childrenBox.Value > room.Capacity Then
                Return RejectInput(
                    "The total number of guests exceeds the room capacity.",
                    roomBox)
            End If

            Return True
        End Function

        Private Function RejectInput(message As String, control As Control) As Boolean
            MessageBox.Show(
                message,
                "Check-In Validation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            If control IsNot Nothing Then control.Focus()
            Return False
        End Function

        Private Function InsertGuest(
            connection As MySqlConnection,
            transaction As MySqlTransaction) As Long

            Const sql As String =
                "INSERT INTO guests " &
                "(first_name, last_name, phone, email, address, " &
                "identification_type, identification_reference, notes) " &
                "VALUES (@first_name, @last_name, @phone, @email, @address, " &
                "@identification_type, @identification_reference, @notes);"

            Using command As New MySqlCommand(sql, connection, transaction)
                command.Parameters.AddWithValue("@first_name", firstNameBox.Text.Trim())
                command.Parameters.AddWithValue("@last_name", lastNameBox.Text.Trim())
                command.Parameters.AddWithValue("@phone", DbValue(phoneBox.Text))
                command.Parameters.AddWithValue("@email", DbValue(emailBox.Text))
                command.Parameters.AddWithValue("@address", DbValue(addressBox.Text))

                Dim idType As String = Convert.ToString(idTypeBox.SelectedItem)
                If String.Equals(idType, "Not provided", StringComparison.OrdinalIgnoreCase) Then
                    idType = Nothing
                End If

                command.Parameters.AddWithValue("@identification_type", DbValue(idType))
                command.Parameters.AddWithValue(
                    "@identification_reference", DbValue(idReferenceBox.Text))
                command.Parameters.AddWithValue("@notes", DbValue(notesBox.Text))

                command.ExecuteNonQuery()
                Return command.LastInsertedId
            End Using
        End Function

        Private Function LockAndValidateRoom(
            connection As MySqlConnection,
            transaction As MySqlTransaction,
            roomId As Long) As RoomChoice

            Const sql As String =
                "SELECT r.room_id, r.room_number, r.floor_number, r.capacity, " &
                "rt.type_name, o.option_id, o.bed_name, o.hourly_rate, " &
                "o.additional_hourly_rate, o.daily_rate, o.minimum_hours " &
                "FROM rooms r " &
                "INNER JOIN room_types rt ON rt.room_type_id = r.room_type_id " &
                "INNER JOIN room_type_options o " &
                "ON o.room_type_id = r.room_type_id " &
                "AND o.bed_code = r.bed_code AND o.is_active = 1 " &
                "WHERE r.room_id = @room_id " &
                "AND r.status = 'AVAILABLE' AND r.is_active = 1 " &
                "FOR UPDATE;"

            Using command As New MySqlCommand(sql, connection, transaction)
                command.Parameters.AddWithValue("@room_id", roomId)

                Using reader As MySqlDataReader = command.ExecuteReader()
                    If Not reader.Read() Then Return Nothing

                    Return New RoomChoice With {
                        .RoomId = Convert.ToInt64(reader("room_id")),
                        .RoomNumber = SafeString(reader("room_number")),
                        .FloorNumber = Convert.ToInt32(reader("floor_number")),
                        .Capacity = Convert.ToInt32(reader("capacity")),
                        .RoomType = SafeString(reader("type_name")),
                        .RateOptionId = Convert.ToInt64(reader("option_id")),
                        .BedName = SafeString(reader("bed_name")),
                        .HourlyRate = Convert.ToDecimal(reader("hourly_rate")),
                        .AdditionalHourlyRate =
                            Convert.ToDecimal(reader("additional_hourly_rate")),
                        .DailyRate = Convert.ToDecimal(reader("daily_rate")),
                        .MinimumHours = Convert.ToInt32(reader("minimum_hours"))
                    }
                End Using
            End Using
        End Function

        Private Function InsertStay(
            connection As MySqlConnection,
            transaction As MySqlTransaction,
            guestId As Long,
            stayReference As String,
            checkedInAt As DateTime,
            staffId As Long?) As Long

            Const sql As String =
                "INSERT INTO stays " &
                "(guest_id, stay_reference, stay_status, expected_checkout_at, " &
                "checked_in_at, adults, children, notes, created_by) " &
                "VALUES (@guest_id, @stay_reference, 'ACTIVE', @expected_checkout_at, " &
                "@checked_in_at, @adults, @children, @notes, @created_by);"

            Using command As New MySqlCommand(sql, connection, transaction)
                command.Parameters.AddWithValue("@guest_id", guestId)
                command.Parameters.AddWithValue("@stay_reference", stayReference)
                command.Parameters.AddWithValue(
                    "@expected_checkout_at", checkoutPicker.Value)
                command.Parameters.AddWithValue("@checked_in_at", checkedInAt)
                command.Parameters.AddWithValue("@adults", Convert.ToInt32(adultsBox.Value))
                command.Parameters.AddWithValue("@children", Convert.ToInt32(childrenBox.Value))
                command.Parameters.AddWithValue("@notes", DbValue(notesBox.Text))
                command.Parameters.AddWithValue("@created_by", DbValue(staffId))

                command.ExecuteNonQuery()
                Return command.LastInsertedId
            End Using
        End Function

        Private Sub InsertRoomAssignment(
            connection As MySqlConnection,
            transaction As MySqlTransaction,
            stayId As Long,
            room As RoomChoice,
            staffId As Long?)

            Const sql As String =
                "INSERT INTO room_assignments " &
                "(stay_id, room_id, assigned_at, released_at, rate_option_id, " &
                "hourly_rate_snapshot, additional_hourly_rate_snapshot, " &
                "daily_rate_snapshot, minimum_hours_snapshot, assigned_by) " &
                "VALUES (@stay_id, @room_id, @assigned_at, NULL, @rate_option_id, " &
                "@hourly_rate, @additional_hourly_rate, @daily_rate, " &
                "@minimum_hours, @assigned_by);"

            Using command As New MySqlCommand(sql, connection, transaction)
                command.Parameters.AddWithValue("@stay_id", stayId)
                command.Parameters.AddWithValue("@room_id", room.RoomId)
                command.Parameters.AddWithValue("@assigned_at", DateTime.Now)
                command.Parameters.AddWithValue("@rate_option_id", room.RateOptionId)
                command.Parameters.AddWithValue("@hourly_rate", room.HourlyRate)
                command.Parameters.AddWithValue(
                    "@additional_hourly_rate", room.AdditionalHourlyRate)
                command.Parameters.AddWithValue("@daily_rate", room.DailyRate)
                command.Parameters.AddWithValue("@minimum_hours", room.MinimumHours)
                command.Parameters.AddWithValue("@assigned_by", DbValue(staffId))

                command.ExecuteNonQuery()
            End Using
        End Sub

        Private Sub InsertGuestFolio(
            connection As MySqlConnection,
            transaction As MySqlTransaction,
            stayId As Long,
            staffId As Long?)

            Const sql As String =
                "INSERT INTO guest_folios (stay_id, folio_status, opened_by) " &
                "VALUES (@stay_id, 'OPEN', @opened_by);"

            Using command As New MySqlCommand(sql, connection, transaction)
                command.Parameters.AddWithValue("@stay_id", stayId)
                command.Parameters.AddWithValue("@opened_by", DbValue(staffId))
                command.ExecuteNonQuery()
            End Using
        End Sub

        Private Shared Function GenerateStayReference() As String
            Dim bytes(5) As Byte

            Using generator As RandomNumberGenerator = RandomNumberGenerator.Create()
                generator.GetBytes(bytes)
            End Using

            Dim suffix As New StringBuilder()

            For Each value As Byte In bytes
                suffix.Append(value.ToString("X2"))
            Next

            Return "ST" & DateTime.Now.ToString("yyMMddHHmmss") &
                   suffix.ToString().Substring(0, 6)
        End Function

        Private Shared Function GetCurrentStaffId() As Long?
            Try
                Dim value As Long = Convert.ToInt64(CurrentUser.UserId)

                If value > 0 Then Return value
            Catch
                ' Allow a nullable staff ID if no authenticated ID is available.
            End Try

            Return Nothing
        End Function

        Private Shared Function DbValue(value As Object) As Object
            If value Is Nothing Then Return DBNull.Value

            If TypeOf value Is String Then
                Dim textValue As String = DirectCast(value, String).Trim()

                If textValue.Length = 0 Then Return DBNull.Value
                Return textValue
            End If

            If TypeOf value Is Long? Then
                Dim nullableValue As Long? = DirectCast(value, Long?)
                If Not nullableValue.HasValue Then Return DBNull.Value
                Return nullableValue.Value
            End If

            Return value
        End Function

        Private Shared Function SafeString(value As Object) As String
            If value Is Nothing OrElse value Is DBNull.Value Then Return String.Empty
            Return Convert.ToString(value)
        End Function

        Private Shared Function FormatPeso(value As Decimal) As String
            Return "₱" & value.ToString("N2")
        End Function

        Private Shared Function SafeStaffName() As String
            If Not String.IsNullOrWhiteSpace(CurrentUser.FullName) Then
                Return CurrentUser.FullName
            End If

            If Not String.IsNullOrWhiteSpace(CurrentUser.Username) Then
                Return CurrentUser.Username
            End If

            Return "Staff"
        End Function

        Private Shared Function LoadLogo() As Image
            Dim logoPath As String = Path.Combine(
                AppContext.BaseDirectory, "Resources", "staypoint-logo.png")

            If Not File.Exists(logoPath) Then Return Nothing

            Try
                Using source As Image = Image.FromFile(logoPath)
                    Return New Bitmap(source)
                End Using
            Catch
                Return Nothing
            End Try
        End Function

        Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
            If logo.Image IsNot Nothing Then
                logo.Image.Dispose()
                logo.Image = Nothing
            End If

            MyBase.OnFormClosed(e)
        End Sub

    End Class

End Namespace
