
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

    Public NotInheritable Class StaffParkingDashboardControl
        Inherits UserControl

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
        Private ReadOnly contentHost As New Panel()
        Private ReadOnly statusLabel As New Label()

        Private activeContent As Control
        Private isLoading As Boolean

        Public Event BackRequested As EventHandler
        Public Event LogoutRequested As EventHandler
        Public Event ExitRequested As EventHandler

        Public Sub New()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.StaffParking)

            Name = "StaffParkingDashboardControl"
            Dock = DockStyle.Fill
            BackColor = pageBg
            Font = New Font("Segoe UI", 9.5F)
            AutoScaleMode = AutoScaleMode.Dpi
            MinimumSize = New Size(700, 450)

            BuildLayout()
            ShowParkingDashboard()
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

            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 72))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 50))

            root.Controls.Add(BuildHeader(), 0, 0)
            root.Controls.Add(BuildBody(), 0, 1)
            root.Controls.Add(BuildFooter(), 0, 2)

            Controls.Add(root)
        End Sub

        Private Function BuildHeader() As Control
            Dim header As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = navy,
                .Padding = New Padding(16, 5, 18, 5)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = navy,
                .Margin = New Padding(0)
            }

            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 68))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 32))

            Dim brandPanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .WrapContents = False,
                .FlowDirection = FlowDirection.LeftToRight,
                .BackColor = navy,
                .Margin = New Padding(0)
            }

            logo.Size = New Size(52, 52)
            logo.SizeMode = PictureBoxSizeMode.Zoom
            logo.Image = LoadLogo()
            logo.Margin = New Padding(0, 2, 10, 0)

            Dim brand As New Label With {
                .Text = "STAYPOINT HMS" & Environment.NewLine &
                        "PARKING OPERATIONS",
                .AutoSize = True,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
                .Margin = New Padding(0, 5, 0, 0)
            }

            brandPanel.Controls.Add(logo)
            brandPanel.Controls.Add(brand)

            Dim staffLabel As New Label With {
                .Text = "Staff: " & SafeStaffName(),
                .Dock = DockStyle.Fill,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleRight,
                .AutoEllipsis = True
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
                .Padding = New Padding(12, 14, 12, 10)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = sidebarColor,
                .Margin = New Padding(0)
            }

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim sectionTitle As New Label With {
                .Text = "PARKING MENU",
                .Dock = DockStyle.Fill,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Margin = New Padding(3, 0, 0, 5)
            }

            navigation.Dock = DockStyle.Fill
            navigation.FlowDirection = FlowDirection.TopDown
            navigation.WrapContents = False
            navigation.AutoScroll = True
            navigation.BackColor = sidebarColor
            navigation.Margin = New Padding(0)

            AddNavigationItem("Parking Dashboard")
            AddNavigationItem("Parking Spaces")
            AddNavigationItem("Parking Tickets")
            AddNavigationItem("Vehicle Entry / Departure")
            AddNavigationItem("Parking History")
            AddNavigationItem("Parking Charges")

            layout.Controls.Add(sectionTitle, 0, 0)
            layout.Controls.Add(navigation, 0, 1)
            sidebar.Controls.Add(layout)

            Return sidebar
        End Function

        Private Sub AddNavigationItem(caption As String)
            Dim item As New Button With {
                .Text = caption,
                .Tag = caption,
                .Width = 190,
                .Height = 42,
                .MinimumSize = New Size(175, 40),
                .MaximumSize = New Size(210, 42),
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
            item.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(43, 75, 113)

            AddHandler item.Click, AddressOf HandleNavigationClick
            navigation.Controls.Add(item)
        End Sub

        Private Function BuildMainContent() As Control
            Dim main As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 4,
                .Padding = New Padding(18, 12, 18, 8),
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            main.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            main.RowStyles.Add(New RowStyle(SizeType.Absolute, 34))
            main.RowStyles.Add(New RowStyle(SizeType.Absolute, 28))
            main.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            main.RowStyles.Add(New RowStyle(SizeType.Absolute, 22))

            pageTitle.Text = "Parking Dashboard"
            pageTitle.Dock = DockStyle.Fill
            pageTitle.ForeColor = navy
            pageTitle.Font = New Font("Segoe UI", 19.0F, FontStyle.Bold)
            pageTitle.TextAlign = ContentAlignment.MiddleLeft
            pageTitle.Margin = New Padding(0)

            pageDescription.Text = "Parking availability, active tickets, and vehicle activity."
            pageDescription.Dock = DockStyle.Fill
            pageDescription.ForeColor = muted
            pageDescription.Font = New Font("Segoe UI", 9.0F)
            pageDescription.TextAlign = ContentAlignment.MiddleLeft
            pageDescription.AutoEllipsis = True
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
            statusLabel.AutoEllipsis = True
            statusLabel.Margin = New Padding(0)

            main.Controls.Add(pageTitle, 0, 0)
            main.Controls.Add(pageDescription, 0, 1)
            main.Controls.Add(contentHost, 0, 2)
            main.Controls.Add(statusLabel, 0, 3)

            Return main
        End Function

        Private Function BuildFooter() As Control
            Dim footer As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(8, 5, 8, 5)
            }

            Dim backButton = CreateFooterButton(
                "Back to Workspace", Color.White, navy, 145)

            Dim logoutButton = CreateFooterButton(
                "Log Out", navy, Color.White, 90)

            Dim exitButton = CreateFooterButton(
                "Exit Application", Color.White, navy, 130)

            backButton.Dock = DockStyle.Left
            exitButton.Dock = DockStyle.Right
            logoutButton.Dock = DockStyle.Right

            AddHandler backButton.Click,
                Sub(sender As Object, e As EventArgs)
                    RaiseEvent BackRequested(Me, EventArgs.Empty)
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
                .Dock = DockStyle.None,
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

        Private Sub HandleNavigationClick(sender As Object, e As EventArgs)
            Dim clickedButton As Button = TryCast(sender, Button)
            If clickedButton Is Nothing Then Return

            Dim selectedPage As String = Convert.ToString(clickedButton.Tag)

            If selectedPage = "Parking Dashboard" Then
                ShowParkingDashboard()
            Else
                ShowParkingModule(selectedPage)
            End If
        End Sub

        Private Sub ShowParkingDashboard()
            pageTitle.Text = "Parking Dashboard"
            pageDescription.Text =
                "Live parking-space and ticket information from the database."

            Dim dashboard As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            dashboard.RowStyles.Add(New RowStyle(SizeType.Absolute, 104))
            dashboard.RowStyles.Add(New RowStyle(SizeType.Absolute, 36))
            dashboard.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim cards As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 4,
                .RowCount = 1,
                .BackColor = pageBg,
                .Margin = New Padding(0, 0, 0, 8)
            }

            For i As Integer = 0 To 3
                cards.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))
            Next

            Dim availableValue As New Label()
            Dim occupiedValue As New Label()
            Dim activeValue As New Label()
            Dim departedValue As New Label()

            cards.Controls.Add(
                CreateSummaryCard("AVAILABLE SPACES", availableValue, teal),
                0, 0)

            cards.Controls.Add(
                CreateSummaryCard("OCCUPIED SPACES", occupiedValue,
                                  Color.FromArgb(59, 130, 246)),
                1, 0)

            cards.Controls.Add(
                CreateSummaryCard("ACTIVE TICKETS", activeValue,
                                  Color.FromArgb(217, 119, 6)),
                2, 0)

            cards.Controls.Add(
                CreateSummaryCard("DEPARTED TICKETS", departedValue,
                                  Color.FromArgb(124, 58, 237)),
                3, 0)

            Dim toolbar As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            Dim refreshButton As New Button With {
                .Text = "Refresh",
                .Dock = DockStyle.Right,
                .Width = 95,
                .BackColor = teal,
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .UseVisualStyleBackColor = False,
                .Cursor = Cursors.Hand
            }

            refreshButton.FlatAppearance.BorderSize = 0

            Dim recentLabel As New Label With {
                .Text = "Recent Parking Tickets",
                .Dock = DockStyle.Fill,
                .ForeColor = navy,
                .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft
            }

            toolbar.Controls.Add(recentLabel)
            toolbar.Controls.Add(refreshButton)

            Dim ticketsGrid As DataGridView = CreateDataGrid()

            Dim gridPanel As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(8),
                .Margin = New Padding(0)
            }

            gridPanel.Controls.Add(ticketsGrid)

            dashboard.Controls.Add(cards, 0, 0)
            dashboard.Controls.Add(toolbar, 0, 1)
            dashboard.Controls.Add(gridPanel, 0, 2)

            AddHandler refreshButton.Click,
                Sub(sender As Object, e As EventArgs)
                    LoadDashboardData(
                        availableValue,
                        occupiedValue,
                        activeValue,
                        departedValue,
                        ticketsGrid)
                End Sub

            SetActiveContent(dashboard)

            LoadDashboardData(
                availableValue,
                occupiedValue,
                activeValue,
                departedValue,
                ticketsGrid)
        End Sub

        Private Function CreateSummaryCard(
            caption As String,
            valueLabel As Label,
            accentColor As Color) As Control

            Dim card As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(9, 7, 6, 7),
                .Margin = New Padding(0, 0, 7, 0)
            }

            Dim accent As New Panel With {
                .Dock = DockStyle.Left,
                .Width = 4,
                .BackColor = accentColor
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
                .AutoEllipsis = True
            }

            valueLabel.Text = "—"
            valueLabel.Dock = DockStyle.Fill
            valueLabel.ForeColor = navy
            valueLabel.Font = New Font("Segoe UI", 19.0F, FontStyle.Bold)
            valueLabel.TextAlign = ContentAlignment.MiddleLeft

            layout.Controls.Add(title, 0, 0)
            layout.Controls.Add(valueLabel, 0, 1)

            card.Controls.Add(layout)
            card.Controls.Add(accent)

            Return card
        End Function

        Private Function CreateDataGrid() As DataGridView
            Dim grid As New DataGridView With {
                .Dock = DockStyle.Fill,
                .BackgroundColor = Color.White,
                .BorderStyle = BorderStyle.None,
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                .AllowUserToAddRows = False,
                .AllowUserToDeleteRows = False,
                .AllowUserToResizeRows = False,
                .ReadOnly = True,
                .MultiSelect = False,
                .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                .RowHeadersVisible = False,
                .AutoGenerateColumns = True,
                .EnableHeadersVisualStyles = False,
                .GridColor = borderColor
            }

            grid.ColumnHeadersDefaultCellStyle.BackColor = navy
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 8.5F, FontStyle.Bold)
            grid.ColumnHeadersHeight = 32

            grid.DefaultCellStyle.Font = New Font("Segoe UI", 8.5F)
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(220, 242, 239)
            grid.DefaultCellStyle.SelectionForeColor = navy
            grid.RowTemplate.Height = 28

            Return grid
        End Function

        Private Sub LoadDashboardData(
            availableValue As Label,
            occupiedValue As Label,
            activeValue As Label,
            departedValue As Label,
            ticketsGrid As DataGridView)

            If isLoading Then Return

            isLoading = True
            statusLabel.Text = "Loading parking data..."
            statusLabel.ForeColor = muted

            Try
                Using connection As MySqlConnection = DbConnectionFactory.Create()
                    connection.Open()

                    Using command As New MySqlCommand(
                        "SELECT " &
                        "COALESCE(SUM(CASE WHEN UPPER(status) = 'AVAILABLE' THEN 1 ELSE 0 END), 0) AS available_count, " &
                        "COALESCE(SUM(CASE WHEN UPPER(status) = 'OCCUPIED' THEN 1 ELSE 0 END), 0) AS occupied_count " &
                        "FROM parking_spaces WHERE is_active = 1;",
                        connection)

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            If reader.Read() Then
                                availableValue.Text =
                                    Convert.ToString(reader("available_count"))
                                occupiedValue.Text =
                                    Convert.ToString(reader("occupied_count"))
                            End If
                        End Using
                    End Using

                    Using command As New MySqlCommand(
                        "SELECT " &
                        "COALESCE(SUM(CASE WHEN ticket_status = 'ACTIVE' THEN 1 ELSE 0 END), 0) AS active_count, " &
                        "COALESCE(SUM(CASE WHEN ticket_status = 'DEPARTED' THEN 1 ELSE 0 END), 0) AS departed_count " &
                        "FROM parking_tickets;",
                        connection)

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            If reader.Read() Then
                                activeValue.Text =
                                    Convert.ToString(reader("active_count"))
                                departedValue.Text =
                                    Convert.ToString(reader("departed_count"))
                            End If
                        End Using
                    End Using

                    Dim ticketTable As New DataTable()

                    Using command As New MySqlCommand(
                        "SELECT pt.ticket_reference AS `Ticket Reference`, " &
                        "pt.plate_number AS `Plate Number`, " &
                        "pt.vehicle_category AS `Vehicle`, " &
                        "ps.parking_code AS `Parking Space`, " &
                        "pt.ticket_status AS `Status`, " &
                        "pt.entered_at AS `Entry Time` " &
                        "FROM parking_tickets pt " &
                        "LEFT JOIN parking_spaces ps " &
                        "ON ps.parking_space_id = pt.parking_space_id " &
                        "ORDER BY pt.entered_at DESC " &
                        "LIMIT 100;",
                        connection)

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            ticketTable.Load(reader)
                        End Using
                    End Using

                    ticketsGrid.DataSource = ticketTable
                End Using

                statusLabel.Text =
                    "Parking data refreshed at " &
                    DateTime.Now.ToString("hh:mm tt") & "."

            Catch ex As Exception
                statusLabel.Text = "Unable to load parking dashboard data."
                statusLabel.ForeColor = Color.Firebrick

                MessageBox.Show(
                    "Unable to load parking dashboard data." &
                    Environment.NewLine & Environment.NewLine &
                    ex.Message,
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            Finally
                isLoading = False
            End Try
        End Sub

        Private Sub ShowParkingModule(selectedPage As String)
            Try
                Dim parkingControl As New StaffParkingManagementControl With {
                    .Dock = DockStyle.Fill
                }

                pageTitle.Text = selectedPage
                pageDescription.Text =
                    "Parking operations and records from the database."

                SetActiveContent(parkingControl)
                statusLabel.Text =
                    selectedPage & " module loaded."

            Catch ex As Exception
                statusLabel.Text = "Unable to open the parking module."

                MessageBox.Show(
                    "Unable to open the parking management module." &
                    Environment.NewLine & ex.Message,
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub SetActiveContent(newContent As Control)
            If newContent Is Nothing Then Return

            contentHost.SuspendLayout()

            Try
                If activeContent IsNot Nothing Then
                    contentHost.Controls.Remove(activeContent)
                    activeContent.Dispose()
                    activeContent = Nothing
                End If

                activeContent = newContent
                activeContent.Dock = DockStyle.Fill
                activeContent.MinimumSize = New Size(0, 0)

                contentHost.Controls.Add(activeContent)
                activeContent.BringToFront()

            Finally
                contentHost.ResumeLayout(True)
            End Try
        End Sub

        Private Sub HandleStaffLogout(sender As Object, e As EventArgs)
            If MessageBox.Show(
                "Log out of the staff account and return to login?",
                "StayPoint HMS",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            RaiseEvent LogoutRequested(Me, EventArgs.Empty)
        End Sub

        Private Sub HandleApplicationExit(sender As Object, e As EventArgs)
            If MessageBox.Show(
                "Exit StayPoint HMS?",
                "StayPoint HMS",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then Return

            RaiseEvent ExitRequested(Me, EventArgs.Empty)
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
                AppContext.BaseDirectory,
                "Resources",
                "staypoint-logo.png")

            If Not File.Exists(logoPath) Then Return Nothing

            Try
                Using source As Image = Image.FromFile(logoPath)
                    Return New Bitmap(source)
                End Using
            Catch
                Return Nothing
            End Try
        End Function

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If activeContent IsNot Nothing Then
                    If contentHost.Controls.Contains(activeContent) Then
                        contentHost.Controls.Remove(activeContent)
                    End If

                    activeContent.Dispose()
                    activeContent = Nothing
                End If

                If logo.Image IsNot Nothing Then
                    logo.Image.Dispose()
                    logo.Image = Nothing
                End If
            End If

            MyBase.Dispose(disposing)
        End Sub

    End Class

End Namespace