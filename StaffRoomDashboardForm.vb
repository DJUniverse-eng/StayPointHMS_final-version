
Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class StaffRoomDashboardForm
        Inherits Form

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly sidebarColor As Color = Color.FromArgb(32, 58, 95)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly pageBg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)

        Private ReadOnly logo As New PictureBox()
        Private ReadOnly navigation As New FlowLayoutPanel()
        Private ReadOnly pageTitle As New Label()
        Private ReadOnly pageDescription As New Label()
        Private ReadOnly statusLabel As New Label()
        Private ReadOnly navigationButtons As New System.Collections.Generic.List(Of Button)()
        Private ReadOnly contentHost As New Panel()

        Private _currentControl As Control
        Private _isLoadingDashboard As Boolean

        Public Property LogoutRequested As Boolean
        Public Property ExitRequested As Boolean

        Public Sub New()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.StaffRoom)

            Text = "StayPoint HMS - Room Operations"
            StartPosition = FormStartPosition.CenterParent
            WindowState = FormWindowState.Maximized
            MinimumSize = New Size(950, 600)
            BackColor = pageBg
            Font = New Font("Segoe UI", 10.0F)
            AutoScaleMode = AutoScaleMode.Dpi

            BuildLayout()
            ShowRoomDashboard()
        End Sub

        Private Sub BuildLayout()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .Margin = New Padding(0),
                .Padding = New Padding(0),
                .BackColor = pageBg
            }

            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 76))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 54))

            root.Controls.Add(BuildHeader(), 0, 0)
            root.Controls.Add(BuildBody(), 0, 1)
            root.Controls.Add(BuildFooter(), 0, 2)

            Controls.Add(root)
        End Sub

        Private Function BuildHeader() As Control
            Dim header As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = navy,
                .Padding = New Padding(18, 6, 18, 6),
                .Margin = New Padding(0)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = navy,
                .Margin = New Padding(0)
            }

            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 65))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 35))

            Dim brandPanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .WrapContents = False,
                .FlowDirection = FlowDirection.LeftToRight,
                .BackColor = navy,
                .Margin = New Padding(0)
            }

            logo.Size = New Size(54, 54)
            logo.SizeMode = PictureBoxSizeMode.Zoom
            logo.Image = LoadLogo()
            logo.Margin = New Padding(0, 1, 10, 0)

            Dim brand As New Label With {
                .Text = "STAYPOINT HMS" & Environment.NewLine & "ROOM OPERATIONS",
                .AutoSize = True,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 13.0F, FontStyle.Bold),
                .Margin = New Padding(0, 6, 0, 0)
            }

            brandPanel.Controls.Add(logo)
            brandPanel.Controls.Add(brand)

            Dim staffLabel As New Label With {
                .Text = "Staff: " & SafeStaffName(),
                .Dock = DockStyle.Fill,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleRight,
                .AutoEllipsis = True,
                .Margin = New Padding(4)
            }

            layout.Controls.Add(brandPanel, 0, 0)
            layout.Controls.Add(staffLabel, 1, 0)
            header.Controls.Add(layout)

            Return header
        End Function

        Private Function BuildBody() As Control
            Dim body As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            body.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220))
            body.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))

            body.Controls.Add(BuildSidebar(), 0, 0)
            body.Controls.Add(BuildMainContent(), 1, 0)

            Return body
        End Function

        Private Function BuildSidebar() As Control
            Dim sidebar As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = sidebarColor,
                .Padding = New Padding(12, 14, 12, 10),
                .Margin = New Padding(0)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = sidebarColor,
                .Margin = New Padding(0)
            }

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim sectionTitle As New Label With {
                .Text = "ROOM MENU",
                .Dock = DockStyle.Fill,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(4, 0, 0, 4)
            }

            navigation.Dock = DockStyle.Fill
            navigation.FlowDirection = FlowDirection.TopDown
            navigation.WrapContents = False
            navigation.AutoScroll = True
            navigation.BackColor = sidebarColor
            navigation.Margin = New Padding(0)

            AddNavigationItem("Room Dashboard", "Room Dashboard")
            AddNavigationItem("Guest Registration", "Guest Registration")
            AddNavigationItem("Room Assignments", "Room Assignments")
            AddNavigationItem("Check-In / Check-Out", "Check-In / Check-Out")
            AddNavigationItem("Guest Stays and Folios", "Guest Stays and Folios")

            layout.Controls.Add(sectionTitle, 0, 0)
            layout.Controls.Add(navigation, 0, 1)
            sidebar.Controls.Add(layout)

            Return sidebar
        End Function

        Private Sub AddNavigationItem(caption As String, taskName As String)
            Dim item As New Button With {
                .Text = caption,
                .Tag = taskName,
                .Width = 190,
                .Height = 42,
                .MinimumSize = New Size(175, 40),
                .MaximumSize = New Size(205, 42),
                .BackColor = sidebarColor,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Padding = New Padding(10, 0, 3, 0),
                .FlatStyle = FlatStyle.Flat,
                .UseVisualStyleBackColor = False,
                .Cursor = Cursors.Hand,
                .Margin = New Padding(0, 0, 0, 6)
            }

            item.FlatAppearance.BorderSize = 0
            item.FlatAppearance.MouseOverBackColor = Color.FromArgb(43, 75, 113)

            AddHandler item.Click, AddressOf HandleNavigationClick

            navigationButtons.Add(item)
            navigation.Controls.Add(item)
        End Sub

        Private Function BuildMainContent() As Control
            Dim main As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 4,
                .Padding = New Padding(18, 10, 18, 8),
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            main.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            main.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
            main.RowStyles.Add(New RowStyle(SizeType.Absolute, 30))
            main.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            main.RowStyles.Add(New RowStyle(SizeType.Absolute, 22))

            pageTitle.Text = "Room Operations"
            pageTitle.Dock = DockStyle.Fill
            pageTitle.ForeColor = navy
            pageTitle.Font = New Font("Segoe UI", 19.0F, FontStyle.Bold)
            pageTitle.TextAlign = ContentAlignment.MiddleLeft
            pageTitle.Margin = New Padding(0)

            pageDescription.Text = "Monitor room availability and current guest stays."
            pageDescription.Dock = DockStyle.Fill
            pageDescription.ForeColor = muted
            pageDescription.Font = New Font("Segoe UI", 9.0F)
            pageDescription.TextAlign = ContentAlignment.MiddleLeft
            pageDescription.Margin = New Padding(0)

            contentHost.Dock = DockStyle.Fill
            contentHost.BackColor = pageBg
            contentHost.Margin = New Padding(0)
            contentHost.MinimumSize = New Size(0, 0)

            statusLabel.Text = "Ready."
            statusLabel.Dock = DockStyle.Fill
            statusLabel.ForeColor = muted
            statusLabel.Font = New Font("Segoe UI", 8.5F)
            statusLabel.TextAlign = ContentAlignment.MiddleLeft
            statusLabel.Margin = New Padding(0)

            main.Controls.Add(pageTitle, 0, 0)
            main.Controls.Add(pageDescription, 0, 1)
            main.Controls.Add(contentHost, 0, 2)
            main.Controls.Add(statusLabel, 0, 3)

            Return main
        End Function

        Private Sub HandleNavigationClick(sender As Object, e As EventArgs)
            Dim item As Button = TryCast(sender, Button)
            If item Is Nothing Then Return

            Select Case Convert.ToString(item.Tag)
                Case "Room Dashboard"
                    ShowRoomDashboard()

                Case "Guest Registration"
                    OpenGuestRegistration()

                Case "Room Assignments"
                    ShowRoomAssignments()

                Case "Check-In / Check-Out",
                     "Guest Stays and Folios"
                    ShowPlaceholder(Convert.ToString(item.Tag))

                Case Else
                    ShowPlaceholder(Convert.ToString(item.Tag))
            End Select
        End Sub

        Private Sub OpenGuestRegistration()
            Try
                Using registration As New StaffGuestRegistrationForm()
                    If registration.ShowDialog(Me) = DialogResult.OK Then
                        ShowRoomDashboard()
                        statusLabel.Text = "Guest check-in completed. Dashboard refreshed."
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show(
                    "Unable to open Guest Registration / Check-In." &
                    Environment.NewLine & Environment.NewLine & ex.Message,
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub ShowRoomDashboard()
            Dim dashboard As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = pageBg,
                .Margin = New Padding(0),
                .Padding = New Padding(0)
            }

            dashboard.RowStyles.Add(New RowStyle(SizeType.Absolute, 142))
            dashboard.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim availableValue As New Label()
            Dim occupiedValue As New Label()
            Dim maintenanceValue As New Label()
            Dim activeStaysValue As New Label()

            Dim summaryPanel As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 4,
                .RowCount = 1,
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            For i As Integer = 0 To 3
                summaryPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))
            Next

            summaryPanel.Controls.Add(
                CreateSummaryCard("AVAILABLE ROOMS", availableValue, teal), 0, 0)
            summaryPanel.Controls.Add(
                CreateSummaryCard("OCCUPIED ROOMS", occupiedValue,
                                  Color.FromArgb(59, 130, 246)), 1, 0)
            summaryPanel.Controls.Add(
                CreateSummaryCard("CLEANING / MAINTENANCE", maintenanceValue,
                                  Color.FromArgb(217, 119, 6)), 2, 0)
            summaryPanel.Controls.Add(
                CreateSummaryCard("ACTIVE STAYS", activeStaysValue,
                                  Color.FromArgb(124, 58, 237)), 3, 0)

            Dim toolbar As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = pageBg,
                .Margin = New Padding(0, 0, 0, 5)
            }

            Dim registerButton As New Button With {
                .Text = " + Guest Registration / Check-In ",
                .Dock = DockStyle.Left,
                .Width = 235,
                .BackColor = teal,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .UseVisualStyleBackColor = False,
                .Cursor = Cursors.Hand,
                .Margin = New Padding(0)
            }
            registerButton.FlatAppearance.BorderSize = 0
            AddHandler registerButton.Click, AddressOf RegisterButtonClick

            Dim refreshButton As New Button With {
                .Text = "Refresh Dashboard",
                .Dock = DockStyle.Right,
                .Width = 150,
                .BackColor = navy,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .UseVisualStyleBackColor = False,
                .Cursor = Cursors.Hand,
                .Margin = New Padding(0)
            }
            refreshButton.FlatAppearance.BorderSize = 0

            Dim hint As New Label With {
                .Text = "Live room inventory and active stay overview",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Font = New Font("Segoe UI", 8.5F),
                .Padding = New Padding(8, 0, 4, 0),
                .Margin = New Padding(0)
            }

            toolbar.Controls.Add(hint)
            toolbar.Controls.Add(refreshButton)
            toolbar.Controls.Add(registerButton)

            Dim topSection As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            topSection.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
            topSection.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            topSection.Controls.Add(toolbar, 0, 0)
            topSection.Controls.Add(summaryPanel, 0, 1)

            Dim roomGrid As DataGridView = CreateDataGrid()
            Dim staysGrid As DataGridView = CreateDataGrid()

            Dim roomsPanel As Control = CreateGridPanel(
                "Room Inventory", "Current room status from the database", roomGrid)

            Dim staysPanel As Control = CreateGridPanel(
                "Active Guest Stays", "Guests currently checked in", staysGrid)

            Dim lowerLayout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            lowerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            lowerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            lowerLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            roomsPanel.Margin = New Padding(0, 0, 5, 0)
            staysPanel.Margin = New Padding(5, 0, 0, 0)
            lowerLayout.Controls.Add(roomsPanel, 0, 0)
            lowerLayout.Controls.Add(staysPanel, 1, 0)

            dashboard.Controls.Add(topSection, 0, 0)
            dashboard.Controls.Add(lowerLayout, 0, 1)

            AddHandler refreshButton.Click,
                Sub(sender As Object, e As EventArgs)
                    LoadDashboardData(
                        availableValue, occupiedValue, maintenanceValue,
                        activeStaysValue, roomGrid, staysGrid)
                End Sub

            pageTitle.Text = "Room Dashboard"
            pageDescription.Text =
                "Monitor available rooms, occupied rooms, and active guest stays."

            SetContent(dashboard)

            LoadDashboardData(
                availableValue, occupiedValue, maintenanceValue,
                activeStaysValue, roomGrid, staysGrid)
        End Sub

        Private Sub RegisterButtonClick(sender As Object, e As EventArgs)
            OpenGuestRegistration()
        End Sub

        Private Function CreateSummaryCard(
            caption As String,
            valueLabel As Label,
            accentColor As Color) As Control

            Dim card As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(8),
                .Margin = New Padding(0, 0, 6, 0),
                .MinimumSize = New Size(0, 0)
            }

            Dim accent As New Panel With {
                .Dock = DockStyle.Left,
                .Width = 4,
                .BackColor = accentColor,
                .Margin = New Padding(0)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = Color.White,
                .Padding = New Padding(7, 0, 0, 0),
                .Margin = New Padding(0)
            }

            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 48))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 52))

            Dim title As New Label With {
                .Text = caption,
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 7.5F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .AutoEllipsis = True,
                .Margin = New Padding(0)
            }

            valueLabel.Text = "—"
            valueLabel.Dock = DockStyle.Fill
            valueLabel.ForeColor = navy
            valueLabel.Font = New Font("Segoe UI", 20.0F, FontStyle.Bold)
            valueLabel.TextAlign = ContentAlignment.MiddleLeft
            valueLabel.AutoEllipsis = True
            valueLabel.Margin = New Padding(0)

            layout.Controls.Add(title, 0, 0)
            layout.Controls.Add(valueLabel, 0, 1)
            card.Controls.Add(layout)
            card.Controls.Add(accent)

            Return card
        End Function

        Private Function CreateGridPanel(
            headingText As String,
            descriptionText As String,
            grid As DataGridView) As Control

            Dim panel As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(9),
                .Margin = New Padding(0),
                .MinimumSize = New Size(0, 0)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .BackColor = Color.White,
                .Margin = New Padding(0),
                .Padding = New Padding(0)
            }

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 27))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 23))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim heading As New Label With {
                .Text = headingText,
                .Dock = DockStyle.Fill,
                .ForeColor = navy,
                .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(0)
            }

            Dim description As New Label With {
                .Text = descriptionText,
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 8.0F),
                .TextAlign = ContentAlignment.MiddleLeft,
                .AutoEllipsis = True,
                .Margin = New Padding(0)
            }

            grid.Dock = DockStyle.Fill
            grid.MinimumSize = New Size(0, 0)
            grid.Margin = New Padding(0)

            layout.Controls.Add(heading, 0, 0)
            layout.Controls.Add(description, 0, 1)
            layout.Controls.Add(grid, 0, 2)
            panel.Controls.Add(layout)

            Return panel
        End Function

        Private Function CreateDataGrid() As DataGridView
            Dim grid As New DataGridView With {
                .Dock = DockStyle.Fill,
                .MinimumSize = New Size(0, 0),
                .BackgroundColor = Color.White,
                .BorderStyle = BorderStyle.None,
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                .AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None,
                .AllowUserToAddRows = False,
                .AllowUserToDeleteRows = False,
                .AllowUserToResizeRows = False,
                .ReadOnly = True,
                .MultiSelect = False,
                .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                .RowHeadersVisible = False,
                .AutoGenerateColumns = True,
                .EnableHeadersVisualStyles = False,
                .GridColor = borderColor,
                .ScrollBars = ScrollBars.Both,
                .ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                .ColumnHeadersHeight = 32
            }

            grid.ColumnHeadersDefaultCellStyle.BackColor = navy
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 8.0F, FontStyle.Bold)
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True

            grid.DefaultCellStyle.Font = New Font("Segoe UI", 8.0F)
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 242, 239)
            grid.DefaultCellStyle.SelectionForeColor = navy
            grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False
            grid.RowTemplate.Height = 26

            Return grid
        End Function

        Private Sub LoadDashboardData(
            availableValue As Label,
            occupiedValue As Label,
            maintenanceValue As Label,
            activeStaysValue As Label,
            roomGrid As DataGridView,
            staysGrid As DataGridView)

            If _isLoadingDashboard Then Return

            _isLoadingDashboard = True
            statusLabel.Text = "Loading room information..."
            statusLabel.ForeColor = muted

            Try
                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Using command As New MySqlCommand(
                        "SELECT " &
                        "COALESCE(SUM(CASE WHEN status = 'AVAILABLE' THEN 1 ELSE 0 END), 0) AS available_rooms, " &
                        "COALESCE(SUM(CASE WHEN status = 'OCCUPIED' THEN 1 ELSE 0 END), 0) AS occupied_rooms, " &
                        "COALESCE(SUM(CASE WHEN status IN ('CLEANING', 'UNDER_MAINTENANCE') THEN 1 ELSE 0 END), 0) AS maintenance_rooms " &
                        "FROM rooms WHERE is_active = 1;",
                        connection)

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            If reader.Read() Then
                                availableValue.Text = Convert.ToString(reader("available_rooms"))
                                occupiedValue.Text = Convert.ToString(reader("occupied_rooms"))
                                maintenanceValue.Text = Convert.ToString(reader("maintenance_rooms"))
                            End If
                        End Using
                    End Using

                    Using command As New MySqlCommand(
                        "SELECT COUNT(*) FROM stays WHERE stay_status = 'ACTIVE';",
                        connection)

                        activeStaysValue.Text = Convert.ToString(command.ExecuteScalar())
                    End Using

                    Dim roomsTable As New DataTable()

                    Using command As New MySqlCommand(
                        "SELECT r.room_number AS `Room Number`, " &
                        "rt.type_name AS `Room Type`, r.floor_number AS `Floor`, " &
                        "r.status AS `Status` " &
                        "FROM rooms r " &
                        "LEFT JOIN room_types rt ON rt.room_type_id = r.room_type_id " &
                        "WHERE r.is_active = 1 " &
                        "ORDER BY r.floor_number, r.room_number LIMIT 200;",
                        connection)

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            roomsTable.Load(reader)
                        End Using
                    End Using

                    roomGrid.DataSource = roomsTable

                    Dim staysTable As New DataTable()

                    Using command As New MySqlCommand(
                        "SELECT s.stay_reference AS `Stay Reference`, " &
                        "CONCAT(g.first_name, ' ', g.last_name) AS `Guest Name`, " &
                        "s.stay_status AS `Status`, s.checked_in_at AS `Checked In` " &
                        "FROM stays s " &
                        "INNER JOIN guests g ON g.guest_id = s.guest_id " &
                        "WHERE s.stay_status = 'ACTIVE' " &
                        "ORDER BY s.checked_in_at DESC LIMIT 200;",
                        connection)

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            staysTable.Load(reader)
                        End Using
                    End Using

                    staysGrid.DataSource = staysTable
                End Using

                statusLabel.Text = "Dashboard updated at " & DateTime.Now.ToString("hh:mm tt") & "."

            Catch ex As Exception
                statusLabel.Text = "Unable to load dashboard data."
                statusLabel.ForeColor = Color.Firebrick

                MessageBox.Show(
                    "Unable to load room dashboard data." &
                    Environment.NewLine & Environment.NewLine & ex.Message,
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            Finally
                _isLoadingDashboard = False
            End Try
        End Sub

        Private Sub ShowRoomAssignments()
            Try
                Dim control As New StaffRoomAssignmentControl With {
                    .Dock = DockStyle.Fill
                }

                pageTitle.Text = "Room Assignments"
                pageDescription.Text =
                    "Search available rooms and assign rooms to eligible active stays."

                SetContent(control)
                statusLabel.Text = "Room assignment module loaded."

            Catch ex As Exception
                ShowPlaceholder("Room Assignments")

                MessageBox.Show(
                    "Unable to open the room assignment module." &
                    Environment.NewLine & ex.Message,
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub ShowPlaceholder(taskName As String)
            Dim placeholder As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(22),
                .Margin = New Padding(0)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .BackColor = Color.White,
                .Margin = New Padding(0)
            }

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim heading As New Label With {
                .Text = taskName,
                .Dock = DockStyle.Fill,
                .ForeColor = navy,
                .Font = New Font("Segoe UI", 17.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(0)
            }

            Dim description As New Label With {
                .Text = "This operational module has not been connected yet.",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .Font = New Font("Segoe UI", 10.0F),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(0)
            }

            Dim accent As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 5,
                .BackColor = teal,
                .Margin = New Padding(0)
            }

            layout.Controls.Add(heading, 0, 0)
            layout.Controls.Add(description, 0, 1)
            layout.Controls.Add(accent, 0, 2)
            placeholder.Controls.Add(layout)

            pageTitle.Text = taskName
            pageDescription.Text = "Room operations"
            SetContent(placeholder)
            statusLabel.Text = taskName & " is awaiting its UserControl."
        End Sub

        Private Sub SetContent(newControl As Control)
            If newControl Is Nothing Then Return

            contentHost.SuspendLayout()

            Try
                If _currentControl IsNot Nothing Then
                    contentHost.Controls.Remove(_currentControl)
                    _currentControl.Dispose()
                    _currentControl = Nothing
                End If

                _currentControl = newControl
                _currentControl.Dock = DockStyle.Fill
                _currentControl.Margin = New Padding(0)
                contentHost.Controls.Add(_currentControl)
                _currentControl.BringToFront()
            Finally
                contentHost.ResumeLayout(True)
            End Try
        End Sub

        Private Function BuildFooter() As Control
            Dim footer As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(10, 6, 10, 6),
                .Margin = New Padding(0)
            }

            Dim backButton As Button = CreateFooterButton(
                "Back to Workspace", Color.White, navy, 150)
            Dim logoutButton As Button = CreateFooterButton(
                "Log Out", navy, Color.White, 100)
            Dim exitButton As Button = CreateFooterButton(
                "Exit Application", Color.White, navy, 140)

            backButton.Dock = DockStyle.Left
            exitButton.Dock = DockStyle.Right
            logoutButton.Dock = DockStyle.Right

            AddHandler backButton.Click,
                Sub(sender As Object, e As EventArgs)
                    Close()
                End Sub

            AddHandler logoutButton.Click, AddressOf HandleStaffLogout
            AddHandler exitButton.Click, AddressOf HandleApplicationExit

            footer.Controls.Add(backButton)
            footer.Controls.Add(exitButton)
            footer.Controls.Add(logoutButton)

            Return footer
        End Function

        Private Function CreateFooterButton(
            caption As String,
            background As Color,
            foreground As Color,
            buttonWidth As Integer) As Button

            Dim result As New Button With {
                .Text = caption,
                .Width = buttonWidth,
                .BackColor = background,
                .ForeColor = foreground,
                .FlatStyle = FlatStyle.Flat,
                .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .Cursor = Cursors.Hand,
                .UseVisualStyleBackColor = False,
                .Margin = New Padding(4, 0, 4, 0)
            }

            result.FlatAppearance.BorderColor = borderColor
            result.FlatAppearance.BorderSize = 1

            Return result
        End Function

        Private Sub HandleStaffLogout(sender As Object, e As EventArgs)
            If MessageBox.Show(
                "Log out of the staff account and return to login?",
                "StayPoint HMS",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            LogoutRequested = True
            Close()
        End Sub

        Private Sub HandleApplicationExit(sender As Object, e As EventArgs)
            If MessageBox.Show(
                "Exit StayPoint HMS?",
                "StayPoint HMS",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            ExitRequested = True
            Close()
        End Sub

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
            If _currentControl IsNot Nothing Then
                _currentControl.Dispose()
                _currentControl = Nothing
            End If

            If logo.Image IsNot Nothing Then
                logo.Image.Dispose()
                logo.Image = Nothing
            End If

            MyBase.OnFormClosed(e)
        End Sub

    End Class

End Namespace
