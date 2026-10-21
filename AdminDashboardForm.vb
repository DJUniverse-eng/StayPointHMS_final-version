Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class AdminDashboardForm
        Inherits Form

        Private ReadOnly sidebar As New Panel()
        Private ReadOnly header As New Panel()
        Private ReadOnly contentPanel As New Panel()

        Private ReadOnly lblPage As New Label()
        Private ReadOnly lblSubtitle As New Label()
        Private ReadOnly lblUser As New Label()

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly sidebarColor As Color = Color.FromArgb(32, 58, 95)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly backgroundColor As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(220, 226, 234)

        Private ReadOnly navigationButtons As New List(Of Button)()
        Private currentNavigation As String = "Dashboard"

        Private metricHost As FlowLayoutPanel
        Private ReadOnly dashboardCards As New List(Of Panel)()

        Private Class StatusItem
            Public Property Caption As String = ""
            Public Property Count As Integer
            Public Property BarColor As Color
        End Class

        Private Class DashboardData
            Public TotalRooms As Integer
            Public TotalParkingSpaces As Integer
            Public TotalIncome As Decimal
            Public GuestsCheckedIn As Integer

            Public ReadOnly RoomStatuses As New List(Of StatusItem)()
            Public ReadOnly ParkingStatuses As New List(Of StatusItem)()
        End Class

        Public Sub New()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.Admin)

            Text = "StayPoint HMS - Admin Dashboard"
            StartPosition = FormStartPosition.CenterScreen
            WindowState = FormWindowState.Maximized
            MinimumSize = New Size(1100, 700)
            BackColor = backgroundColor
            Font = New Font("Segoe UI", 10.0F)
            AutoScaleMode = AutoScaleMode.Dpi
            KeyPreview = True

            BuildShell()
            BuildSidebar()
            BuildHeader()
            ShowDashboard()
        End Sub

        ' =========================================================
        ' MAIN WINDOW
        ' =========================================================

        Private Sub BuildShell()
            sidebar.Dock = DockStyle.Left
            sidebar.Width = 245
            sidebar.BackColor = sidebarColor

            header.Dock = DockStyle.Top
            header.Height = 84
            header.BackColor = Color.White
            header.Padding = New Padding(22, 5, 22, 5)

            contentPanel.Dock = DockStyle.Fill
            contentPanel.BackColor = backgroundColor
            contentPanel.Padding = New Padding(24)
            contentPanel.AutoScroll = True

            Controls.Add(contentPanel)
            Controls.Add(header)
            Controls.Add(sidebar)
        End Sub

        Private Sub BuildHeader()
            Dim headerLayout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = Color.White,
                .Margin = New Padding(0),
                .Padding = New Padding(0)
            }

            headerLayout.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100.0F))
            headerLayout.ColumnStyles.Add(
                New ColumnStyle(SizeType.AutoSize))

            Dim titleLayout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = Color.White,
                .Margin = New Padding(0),
                .Padding = New Padding(0)
            }

            titleLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 62.0F))
            titleLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 38.0F))

            lblPage.Text = "Dashboard"
            lblPage.Font = New Font("Segoe UI", 17.0F, FontStyle.Bold)
            lblPage.ForeColor = navy
            lblPage.Dock = DockStyle.Fill
            lblPage.TextAlign = ContentAlignment.BottomLeft
            lblPage.AutoEllipsis = True
            lblPage.Margin = New Padding(0)

            lblSubtitle.Text = "Hotel operations at a glance"
            lblSubtitle.Font = New Font("Segoe UI", 9.0F)
            lblSubtitle.ForeColor = muted
            lblSubtitle.Dock = DockStyle.Fill
            lblSubtitle.TextAlign = ContentAlignment.TopLeft
            lblSubtitle.AutoEllipsis = True
            lblSubtitle.Margin = New Padding(0)

            titleLayout.Controls.Add(lblPage, 0, 0)
            titleLayout.Controls.Add(lblSubtitle, 0, 1)

            lblUser.Text = GetCurrentUserDisplay()
            lblUser.Font = New Font("Segoe UI", 9.5F)
            lblUser.ForeColor = muted
            lblUser.AutoSize = True
            lblUser.TextAlign = ContentAlignment.MiddleRight
            lblUser.Anchor = AnchorStyles.Right
            lblUser.Margin = New Padding(16, 0, 0, 0)

            headerLayout.Controls.Add(titleLayout, 0, 0)
            headerLayout.Controls.Add(lblUser, 1, 0)
            header.Controls.Add(headerLayout)
        End Sub

        Private Function GetCurrentUserDisplay() As String
            Dim fullName As String = If(CurrentUser.FullName, "").Trim()

            If fullName = "" Then
                fullName = If(CurrentUser.Username, "Administrator")
            End If

            Return fullName & "  |  ADMIN"
        End Function

        ' =========================================================
        ' SIDEBAR
        ' =========================================================

        Private Sub BuildSidebar()
            sidebar.Controls.Clear()
            navigationButtons.Clear()

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .BackColor = sidebarColor,
                .Margin = New Padding(0),
                .Padding = New Padding(10, 12, 10, 12)
            }

            layout.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100.0F))
            layout.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 176.0F))
            layout.RowStyles.Add(
                New RowStyle(SizeType.Percent, 100.0F))
            layout.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 54.0F))

            Dim logo As New PictureBox With {
                .Image = LoadLogoImage(),
                .SizeMode = PictureBoxSizeMode.Zoom,
                .Dock = DockStyle.Fill,
                .BackColor = sidebarColor,
                .Margin = New Padding(4, 4, 4, 8)
            }

            Dim menu As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.TopDown,
                .WrapContents = False,
                .AutoScroll = True,
                .BackColor = sidebarColor,
                .Margin = New Padding(0),
                .Padding = New Padding(0, 12, 0, 4)
            }

            AddNavigationButton(menu, "Dashboard", "Dashboard",
                               AddressOf ShowDashboard)
            AddNavigationButton(menu, "System Setup", "System Setup",
                               AddressOf ShowSystemSetup)
            AddNavigationButton(menu, "Room Management", "Room Management",
                               AddressOf ShowRooms)
            AddNavigationButton(menu, "Parking Management", "Parking Management",
                               AddressOf ShowParking)
            AddNavigationButton(menu, "Guest Stay History", "Guest Stay History",
                               AddressOf ShowHistory)
            AddNavigationButton(menu, "Reports", "Reports",
                               AddressOf ShowReports)
            AddNavigationButton(menu, "Staff Management", "Staff Management",
                               AddressOf ShowStaff)

            Dim logout As New Button With {
                .Text = "Log Out",
                .Dock = DockStyle.Fill,
                .Height = 44,
                .FlatStyle = FlatStyle.Flat,
                .ForeColor = Color.White,
                .BackColor = sidebarColor,
                .Font = New Font("Segoe UI", 10.0F),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Padding = New Padding(12, 0, 0, 0),
                .Cursor = Cursors.Hand,
                .Margin = New Padding(0, 4, 0, 0),
                .UseVisualStyleBackColor = False
            }

            logout.FlatAppearance.BorderSize = 0
            logout.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(43, 76, 119)

            AddHandler logout.Click, AddressOf LogoutClicked

            layout.Controls.Add(logo, 0, 0)
            layout.Controls.Add(menu, 0, 1)
            layout.Controls.Add(logout, 0, 2)

            sidebar.Controls.Add(layout)

            AddHandler menu.Resize,
                Sub()
                    ResizeNavigationButtons(menu)
                End Sub
        End Sub

        Private Function LoadLogoImage() As Image
            Dim logoPath As String = Path.Combine(
                Application.StartupPath,
                "Resources",
                "staypoint-logo.png")

            If File.Exists(logoPath) Then
                Try
                    Using source As Image = Image.FromFile(logoPath)
                        Return New Bitmap(source)
                    End Using
                Catch
                    ' Use the generated fallback logo.
                End Try
            End If

            Return CreateLogoFallback()
        End Function

        Private Function CreateLogoFallback() As Image
            Dim bitmap As New Bitmap(220, 150)

            Using graphics As Graphics = Graphics.FromImage(bitmap)
                graphics.Clear(sidebarColor)
                graphics.SmoothingMode = SmoothingMode.AntiAlias

                Using titleFont As New Font("Segoe UI", 23.0F, FontStyle.Bold)
                    Using titleBrush As New SolidBrush(Color.White)
                        Using format As New StringFormat With {
                            .Alignment = StringAlignment.Center,
                            .LineAlignment = StringAlignment.Center
                        }
                            graphics.DrawString(
                                "STAYPOINT",
                                titleFont,
                                titleBrush,
                                New RectangleF(0, 32, 220, 45),
                                format)
                        End Using
                    End Using
                End Using

                Using subFont As New Font("Segoe UI", 10.0F)
                    Using subBrush As New SolidBrush(Color.FromArgb(190, 225, 225))
                        Using format As New StringFormat With {
                            .Alignment = StringAlignment.Center
                        }
                            graphics.DrawString(
                                "HOTEL MANAGEMENT",
                                subFont,
                                subBrush,
                                New RectangleF(0, 83, 220, 24),
                                format)
                        End Using
                    End Using
                End Using
            End Using

            Return bitmap
        End Function

        Private Sub AddNavigationButton(
            menu As FlowLayoutPanel,
            caption As String,
            pageKey As String,
            action As Action)

            Dim button As New Button With {
                .Text = caption,
                .Tag = pageKey,
                .Width = Math.Max(160, menu.ClientSize.Width - 8),
                .Height = 46,
                .FlatStyle = FlatStyle.Flat,
                .ForeColor = Color.White,
                .BackColor = sidebarColor,
                .Font = New Font("Segoe UI", 10.0F),
                .TextAlign = ContentAlignment.MiddleLeft,
                .Padding = New Padding(12, 0, 6, 0),
                .Cursor = Cursors.Hand,
                .Margin = New Padding(0, 2, 0, 3),
                .UseVisualStyleBackColor = False
            }

            button.FlatAppearance.BorderSize = 0
            button.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(43, 76, 119)

            AddHandler button.Click,
                Sub()
                    action.Invoke()
                    UpdateNavigationSelection(pageKey)
                End Sub

            menu.Controls.Add(button)
            navigationButtons.Add(button)
        End Sub

        Private Sub ResizeNavigationButtons(menu As FlowLayoutPanel)
            For Each button As Button In navigationButtons
                button.Width = Math.Max(160, menu.ClientSize.Width - 8)
            Next
        End Sub

        Private Sub UpdateNavigationSelection(pageKey As String)
            currentNavigation = pageKey

            For Each button As Button In navigationButtons
                If String.Equals(
                    Convert.ToString(button.Tag),
                    pageKey,
                    StringComparison.OrdinalIgnoreCase) Then

                    button.BackColor = teal
                Else
                    button.BackColor = sidebarColor
                End If
            Next
        End Sub

        Private Sub LogoutClicked(sender As Object, e As EventArgs)
            If MessageBox.Show(
                "Are you sure you want to log out of StayPoint HMS?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            DialogResult = DialogResult.Cancel
            Close()
        End Sub

        ' =========================================================
        ' PAGE HELPERS
        ' =========================================================

        Private Sub SetPage(
            title As String,
            Optional navigationKey As String = Nothing)

            lblPage.Text = title

            If String.Equals(title, "Dashboard",
                             StringComparison.OrdinalIgnoreCase) Then
                lblSubtitle.Text = "Hotel operations at a glance"
            Else
                lblSubtitle.Text = "StayPoint Hotel Management System"
            End If

            For Each control As Control In
                contentPanel.Controls.Cast(Of Control)().ToArray()

                contentPanel.Controls.Remove(control)
                control.Dispose()
            Next

            dashboardCards.Clear()
            metricHost = Nothing

            UpdateNavigationSelection(
                If(String.IsNullOrWhiteSpace(navigationKey),
                   title, navigationKey))
        End Sub

        Private Function CreatePageLayout() As TableLayoutPanel
            Dim page As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 1,
                .RowCount = 0,
                .BackColor = backgroundColor,
                .Margin = New Padding(0),
                .Padding = New Padding(0)
            }

            page.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100.0F))

            Return page
        End Function

        Private Sub AddPageRow(page As TableLayoutPanel, control As Control)
            Dim rowIndex As Integer = page.RowCount

            page.RowCount += 1
            page.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            control.Dock = DockStyle.Top
            page.Controls.Add(control, 0, rowIndex)
        End Sub

        Private Function CreateSectionTitle(
            title As String,
            description As String) As Control

            Dim panel As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 1,
                .RowCount = 2,
                .BackColor = backgroundColor,
                .Margin = New Padding(0, 0, 0, 12),
                .Padding = New Padding(0)
            }

            panel.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100.0F))

            Dim heading As New Label With {
                .Text = title,
                .Font = New Font("Segoe UI", 13.0F, FontStyle.Bold),
                .ForeColor = navy,
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .Margin = New Padding(0, 0, 0, 4)
            }

            Dim subtitle As New Label With {
                .Text = description,
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = muted,
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .Margin = New Padding(0)
            }

            panel.Controls.Add(heading, 0, 0)
            panel.Controls.Add(subtitle, 0, 1)

            Return panel
        End Function

        ' =========================================================
        ' DASHBOARD
        ' =========================================================

        Private Sub ShowSystemSetup()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.Admin)
            SetPage("System Setup")
            Dim setupControl As New SystemSetupControl With {
                .Dock = DockStyle.Fill
            }
            contentPanel.Controls.Add(setupControl)
            setupControl.BringToFront()
        End Sub

        Private Sub ShowDashboard()
            SetPage("Dashboard")

            Dim page As TableLayoutPanel = CreatePageLayout()
            Dim data As New DashboardData()
            Dim loadError As String = Nothing

            Try
                LoadDashboardData(data)
            Catch ex As Exception
                loadError =
                    "Some dashboard statistics could not be loaded. " &
                    "Check the database connection and table definitions."
            End Try

            metricHost = New FlowLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .WrapContents = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .BackColor = backgroundColor,
                .Margin = New Padding(0, 0, 0, 12),
                .Padding = New Padding(0)
            }

            Dim roomCard As Panel = CreateInventoryCard(
                "Total Rooms", data.TotalRooms, data.RoomStatuses)

            Dim parkingCard As Panel = CreateInventoryCard(
                "Total Parking Spaces", data.TotalParkingSpaces,
                data.ParkingStatuses)

            Dim incomeCard As Panel = CreateMetricCard(
                "Total Income",
                "₱" & data.TotalIncome.ToString("N2"),
                "Completed payments",
                teal)

            Dim guestsCard As Panel = CreateMetricCard(
                "Guests Checked In",
                data.GuestsCheckedIn.ToString("N0"),
                "Guests in active stays",
                navy)

            metricHost.Controls.Add(roomCard)
            metricHost.Controls.Add(parkingCard)
            metricHost.Controls.Add(incomeCard)
            metricHost.Controls.Add(guestsCard)

            dashboardCards.Add(roomCard)
            dashboardCards.Add(parkingCard)
            dashboardCards.Add(incomeCard)
            dashboardCards.Add(guestsCard)

            AddHandler metricHost.Resize, AddressOf ResizeDashboardCards
            AddPageRow(page, metricHost)

            If loadError IsNot Nothing Then
                AddPageRow(page, CreateInfoPanel(loadError))
            End If

            AddPageRow(page, CreateSectionTitle(
                "Occupancy",
                "Current room and parking availability."))

            Dim occupancyLayout As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .Height = 160,
                .ColumnCount = 2,
                .RowCount = 1,
                .BackColor = backgroundColor,
                .Margin = New Padding(0, 0, 0, 16),
                .Padding = New Padding(0)
            }

            occupancyLayout.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 50.0F))
            occupancyLayout.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 50.0F))

            Dim roomOccupancy As New OccupancyVisual With {
                .Title = "Room Occupancy",
                .Items = data.RoomStatuses,
                .Dock = DockStyle.Fill,
                .Margin = New Padding(0, 0, 8, 0)
            }

            Dim parkingOccupancy As New OccupancyVisual With {
                .Title = "Parking Occupancy",
                .Items = data.ParkingStatuses,
                .Dock = DockStyle.Fill,
                .Margin = New Padding(8, 0, 0, 0)
            }

            occupancyLayout.Controls.Add(roomOccupancy, 0, 0)
            occupancyLayout.Controls.Add(parkingOccupancy, 1, 0)
            AddPageRow(page, occupancyLayout)

            AddPageRow(page, CreateSectionTitle(
                "Recent Activity",
                "Latest recorded actions from the audit log."))

            Dim activityGrid As DataGridView = CreateActivityGrid()
            activityGrid.Height = 240
            AddPageRow(page, activityGrid)

            Try
                LoadRecentActivity(activityGrid)
            Catch
                activityGrid.Visible = False
                AddPageRow(page, CreateInfoPanel(
                    "Recent activity is unavailable. Verify the audit_logs table."))
            End Try

            contentPanel.Controls.Add(page)
            ResizeDashboardCards(Nothing, EventArgs.Empty)
        End Sub

        Private Sub LoadDashboardData(data As DashboardData)
            Using conn As MySqlConnection = DbConnectionFactory.Create()
                conn.Open()

                data.TotalRooms = ScalarCount(
                    conn,
                    "SELECT COUNT(*) FROM rooms WHERE is_active = 1")

                LoadRoomStatuses(conn, data)

                data.TotalParkingSpaces = ScalarCount(
                    conn,
                    "SELECT COUNT(*) FROM parking_spaces")

                LoadParkingStatuses(conn, data)

                data.TotalIncome = ScalarDecimal(
                    conn,
                    "SELECT COALESCE(SUM(amount), 0) FROM payments " &
                    "WHERE payment_status = 'COMPLETED'")

                ' This metric is optional because the active-stay table
                ' can differ between database versions.
                Try
                    data.GuestsCheckedIn = ScalarCount(
                        conn,
                        "SELECT COUNT(DISTINCT guest_id) FROM stays " &
                        "WHERE stay_status = 'ACTIVE'")
                Catch
                    data.GuestsCheckedIn = 0
                End Try
            End Using
        End Sub

        Private Sub LoadRoomStatuses(
            conn As MySqlConnection,
            data As DashboardData)

            data.RoomStatuses.Clear()

            Dim definitions As New List(Of StatusItem) From {
                New StatusItem With {
                    .Caption = "Available",
                    .BarColor = teal
                },
                New StatusItem With {
                    .Caption = "Occupied",
                    .BarColor = navy
                },
                New StatusItem With {
                    .Caption = "Cleaning",
                    .BarColor = Color.FromArgb(234, 179, 8)
                },
                New StatusItem With {
                    .Caption = "Under Maintenance",
                    .BarColor = Color.FromArgb(220, 91, 91)
                }
            }

            Dim counts As New Dictionary(Of String, Integer)(
                StringComparer.OrdinalIgnoreCase)

            Using cmd As New MySqlCommand(
                "SELECT status, COUNT(*) AS total " &
                "FROM rooms WHERE is_active = 1 GROUP BY status", conn)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        counts(Convert.ToString(reader("status"))) =
                            Convert.ToInt32(reader("total"))
                    End While
                End Using
            End Using

            For Each item As StatusItem In definitions
                Dim dbStatus As String =
                    item.Caption.ToUpperInvariant().Replace(" ", "_")

                item.Count = If(counts.ContainsKey(dbStatus),
                                counts(dbStatus), 0)

                data.RoomStatuses.Add(item)
            Next
        End Sub

        Private Sub LoadParkingStatuses(
            conn As MySqlConnection,
            data As DashboardData)

            data.ParkingStatuses.Clear()

            Dim definitions As New List(Of StatusItem) From {
                New StatusItem With {
                    .Caption = "Available",
                    .BarColor = teal
                },
                New StatusItem With {
                    .Caption = "Occupied",
                    .BarColor = navy
                },
                New StatusItem With {
                    .Caption = "Maintenance",
                    .BarColor = Color.FromArgb(234, 179, 8)
                },
                New StatusItem With {
                    .Caption = "Inactive",
                    .BarColor = Color.FromArgb(148, 163, 184)
                }
            }

            Dim counts As New Dictionary(Of String, Integer)(
                StringComparer.OrdinalIgnoreCase)

            Dim sql As String =
                "SELECT CASE " &
                "WHEN is_active = 0 OR status = 'INACTIVE' THEN 'INACTIVE' " &
                "ELSE status END AS effective_status, COUNT(*) AS total " &
                "FROM parking_spaces GROUP BY effective_status"

            Using cmd As New MySqlCommand(sql, conn)
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        counts(Convert.ToString(reader("effective_status"))) =
                            Convert.ToInt32(reader("total"))
                    End While
                End Using
            End Using

            For Each item As StatusItem In definitions
                Dim dbStatus As String =
                    item.Caption.ToUpperInvariant().Replace(" ", "_")

                item.Count = If(counts.ContainsKey(dbStatus),
                                counts(dbStatus), 0)

                data.ParkingStatuses.Add(item)
            Next
        End Sub

        Private Function ScalarCount(
            conn As MySqlConnection,
            sql As String) As Integer

            Using cmd As New MySqlCommand(sql, conn)
                Return Convert.ToInt32(cmd.ExecuteScalar())
            End Using
        End Function

        Private Function ScalarDecimal(
            conn As MySqlConnection,
            sql As String) As Decimal

            Using cmd As New MySqlCommand(sql, conn)
                Dim result As Object = cmd.ExecuteScalar()

                If result Is Nothing OrElse result Is DBNull.Value Then
                    Return 0D
                End If

                Return Convert.ToDecimal(result)
            End Using
        End Function

        ' =========================================================
        ' DASHBOARD CARDS
        ' =========================================================

        Private Function CreateInventoryCard(
            caption As String,
            total As Integer,
            statuses As List(Of StatusItem)) As Panel

            Dim card As New Panel With {
                .Width = 350,
                .Height = 208,
                .BackColor = Color.White,
                .Margin = New Padding(0, 0, 12, 12),
                .Padding = New Padding(14),
                .BorderStyle = BorderStyle.FixedSingle
            }

            Dim heading As New Label With {
                .Text = caption,
                .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
                .ForeColor = muted,
                .Location = New Point(14, 12),
                .Size = New Size(300, 22),
                .AutoEllipsis = True
            }

            Dim totalLabel As New Label With {
                .Text = total.ToString("N0"),
                .Font = New Font("Segoe UI", 22.0F, FontStyle.Bold),
                .ForeColor = navy,
                .Location = New Point(14, 34),
                .Size = New Size(300, 34),
                .AutoEllipsis = True
            }

            card.Controls.Add(heading)
            card.Controls.Add(totalLabel)

            Dim startY As Integer = 76
            Dim rowHeight As Integer = 28

            For index As Integer = 0 To statuses.Count - 1
                Dim item As StatusItem = statuses(index)
                Dim y As Integer = startY + index * rowHeight

                Dim marker As New Panel With {
                    .BackColor = item.BarColor,
                    .Location = New Point(15, y + 6),
                    .Size = New Size(9, 9)
                }

                Dim statusLabel As New Label With {
                    .Text = item.Caption,
                    .Font = New Font("Segoe UI", 9.0F),
                    .ForeColor = muted,
                    .Location = New Point(31, y),
                    .Size = New Size(220, 22),
                    .AutoEllipsis = True
                }

                Dim countLabel As New Label With {
                    .Text = item.Count.ToString("N0"),
                    .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                    .ForeColor = navy,
                    .Location = New Point(260, y),
                    .Size = New Size(55, 22),
                    .TextAlign = ContentAlignment.MiddleRight
                }

                card.Controls.Add(marker)
                card.Controls.Add(statusLabel)
                card.Controls.Add(countLabel)
            Next

            AddHandler card.Resize,
                Sub()
                    heading.Width = Math.Max(100, card.ClientSize.Width - 28)
                    totalLabel.Width = Math.Max(100, card.ClientSize.Width - 28)

                    For Each child As Control In card.Controls
                        If TypeOf child Is Label AndAlso
                           child IsNot heading AndAlso
                           child IsNot totalLabel Then

                            If child.Location.X = 31 Then
                                child.Width = Math.Max(
                                    90, card.ClientSize.Width - 125)
                            ElseIf child.Location.X >= 260 Then
                                child.Location = New Point(
                                    card.ClientSize.Width - 78,
                                    child.Location.Y)
                            End If
                        End If
                    Next
                End Sub

            Return card
        End Function

        Private Function CreateMetricCard(
            caption As String,
            value As String,
            description As String,
            accent As Color) As Panel

            Dim card As New Panel With {
                .Width = 230,
                .Height = 208,
                .BackColor = Color.White,
                .Margin = New Padding(0, 0, 12, 12),
                .Padding = New Padding(14),
                .BorderStyle = BorderStyle.FixedSingle
            }

            Dim accentPanel As New Panel With {
                .BackColor = accent,
                .Location = New Point(14, 14),
                .Size = New Size(4, 28)
            }

            Dim heading As New Label With {
                .Text = caption,
                .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
                .ForeColor = muted,
                .Location = New Point(26, 14),
                .Size = New Size(175, 28),
                .AutoEllipsis = True
            }

            Dim valueLabel As New Label With {
                .Text = value,
                .Font = New Font("Segoe UI", 21.0F, FontStyle.Bold),
                .ForeColor = navy,
                .Location = New Point(14, 66),
                .Size = New Size(195, 46),
                .AutoEllipsis = True
            }

            Dim descriptionLabel As New Label With {
                .Text = description,
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = muted,
                .Location = New Point(14, 132),
                .Size = New Size(195, 34),
                .AutoEllipsis = True
            }

            card.Controls.Add(accentPanel)
            card.Controls.Add(heading)
            card.Controls.Add(valueLabel)
            card.Controls.Add(descriptionLabel)

            AddHandler card.Resize,
                Sub()
                    heading.Width = Math.Max(80, card.ClientSize.Width - 42)
                    valueLabel.Width = Math.Max(100, card.ClientSize.Width - 28)
                    descriptionLabel.Width =
                        Math.Max(100, card.ClientSize.Width - 28)
                End Sub

            Return card
        End Function

        Private Sub ResizeDashboardCards(sender As Object, e As EventArgs)
            If metricHost Is Nothing OrElse dashboardCards.Count = 0 Then Return

            Dim availableWidth As Integer = metricHost.ClientSize.Width
            If availableWidth <= 0 Then Return

            Dim spacing As Integer = 12
            Dim inventoryWidth As Integer = 350
            Dim metricWidth As Integer = 230

            If availableWidth < 900 Then
                inventoryWidth = Math.Max(260, availableWidth - 8)
                metricWidth = Math.Max(220,
                    (availableWidth - spacing - 8) \ 2)
            Else
                Dim preferredWidth As Integer =
                    inventoryWidth * 2 + metricWidth * 2 + spacing * 3

                If availableWidth < preferredWidth Then
                    inventoryWidth = Math.Max(
                        300,
                        (availableWidth - metricWidth * 2 - spacing * 3) \ 2)
                End If
            End If

            dashboardCards(0).Width = inventoryWidth
            dashboardCards(1).Width = inventoryWidth
            dashboardCards(2).Width = metricWidth
            dashboardCards(3).Width = metricWidth
        End Sub

        ' =========================================================
        ' OCCUPANCY VISUAL
        ' =========================================================
        Private NotInheritable Class OccupancyVisual
            Inherits Panel

            Private ReadOnly navyColor As Color =
        Color.FromArgb(23, 43, 77)

            Private ReadOnly mutedColor As Color =
        Color.FromArgb(100, 116, 139)

            Private ReadOnly emptyBarColor As Color =
        Color.FromArgb(220, 226, 234)

            Public Property Title As String = ""

            Public Property Items As List(Of StatusItem) =
        New List(Of StatusItem)()

            Public Sub New()
                DoubleBuffered = True
                BackColor = Color.White
                BorderStyle = BorderStyle.FixedSingle
                Padding = New Padding(14)
                MinimumSize = New Size(220, 140)
            End Sub

            Protected Overrides Sub OnPaint(e As PaintEventArgs)
                MyBase.OnPaint(e)

                Dim g As Graphics = e.Graphics
                g.SmoothingMode = SmoothingMode.AntiAlias

                Using titleFont As New Font(
            "Segoe UI", 10.0F, FontStyle.Bold)

                    Using titleBrush As New SolidBrush(navyColor)
                        g.DrawString(Title, titleFont, titleBrush, 14, 12)
                    End Using
                End Using

                If Items Is Nothing OrElse Items.Count = 0 Then Return

                Dim total As Integer = Items.Sum(
            Function(item) Math.Max(0, item.Count))

                Dim barX As Integer = 14
                Dim barY As Integer = 44
                Dim barWidth As Integer = Math.Max(
            10, ClientSize.Width - 28)
                Dim barHeight As Integer = 14

                ' Draw the empty background bar.
                Using emptyBrush As New SolidBrush(emptyBarColor)
                    g.FillRectangle(
                emptyBrush, barX, barY, barWidth, barHeight)
                End Using

                ' Draw each status as a proportional segment.
                If total > 0 Then
                    Dim currentX As Integer = barX
                    Dim drawnWidth As Integer = 0

                    For index As Integer = 0 To Items.Count - 1
                        Dim item As StatusItem = Items(index)
                        Dim count As Integer = Math.Max(0, item.Count)
                        Dim segmentWidth As Integer

                        If index = Items.Count - 1 Then
                            segmentWidth = barWidth - drawnWidth
                        Else
                            segmentWidth = CInt(
                        Math.Round(
                            barWidth * CDbl(count) / total))
                        End If

                        segmentWidth = Math.Max(0, segmentWidth)

                        If segmentWidth > 0 Then
                            Using segmentBrush As New SolidBrush(item.BarColor)
                                g.FillRectangle(
                            segmentBrush,
                            currentX,
                            barY,
                            segmentWidth,
                            barHeight)
                            End Using
                        End If

                        currentX += segmentWidth
                        drawnWidth += segmentWidth
                    Next
                End If

                ' Draw the status legend in two columns.
                Using labelFont As New Font("Segoe UI", 8.0F)
                    Using boldFont As New Font(
                "Segoe UI", 8.0F, FontStyle.Bold)

                        Dim startY As Integer = 70
                        Dim usableWidth As Integer = Math.Max(
                    200, ClientSize.Width - 35)

                        Dim columnWidth As Integer = Math.Max(
                    100, usableWidth \ 2)

                        For index As Integer = 0 To Items.Count - 1
                            Dim item As StatusItem = Items(index)
                            Dim column As Integer = index Mod 2
                            Dim row As Integer = index \ 2

                            Dim x As Integer = 14 + column * columnWidth
                            Dim y As Integer = startY + row * 24

                            Using markerBrush As New SolidBrush(item.BarColor)
                                g.FillRectangle(
                            markerBrush, x, y + 4, 8, 8)
                            End Using

                            Using labelBrush As New SolidBrush(mutedColor)
                                g.DrawString(
                            item.Caption,
                            labelFont,
                            labelBrush,
                            x + 14,
                            y)
                            End Using

                            Using valueBrush As New SolidBrush(navyColor)
                                Dim valueText As String =
                            item.Count.ToString("N0")

                                g.DrawString(
                            valueText,
                            boldFont,
                            valueBrush,
                            x + columnWidth - 35,
                            y)
                            End Using
                        Next
                    End Using
                End Using
            End Sub
        End Class

        ' =========================================================
        ' RECENT ACTIVITY
        ' =========================================================

        Private Function CreateActivityGrid() As DataGridView
            Dim grid As New DataGridView With {
                .Dock = DockStyle.Top,
                .BackgroundColor = Color.White,
                .BorderStyle = BorderStyle.FixedSingle,
                .GridColor = borderColor,
                .AllowUserToAddRows = False,
                .AllowUserToDeleteRows = False,
                .AllowUserToResizeRows = False,
                .ReadOnly = True,
                .MultiSelect = False,
                .RowHeadersVisible = False,
                .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                .AutoGenerateColumns = True,
                .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                .AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                .ScrollBars = ScrollBars.Both
            }

            grid.EnableHeadersVisualStyles = False
            grid.ColumnHeadersDefaultCellStyle.BackColor = navy
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 9.0F, FontStyle.Bold)
            grid.DefaultCellStyle.Font = New Font("Segoe UI", 9.0F)
            grid.DefaultCellStyle.ForeColor = navy
            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(220, 242, 239)
            grid.DefaultCellStyle.SelectionForeColor = navy
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 252)

            Return grid
        End Function

        Private Sub LoadRecentActivity(grid As DataGridView)
            Dim table As New DataTable()

            Using conn As MySqlConnection = DbConnectionFactory.Create()
                conn.Open()

                Using cmd As New MySqlCommand(
                    "SELECT created_at AS `Time`, " &
                    "action_type AS `Action`, " &
                    "entity_type AS `Record Type`, " &
                    "entity_id AS `Record ID`, " &
                    "description AS `Description` " &
                    "FROM audit_logs ORDER BY created_at DESC LIMIT 8", conn)

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(table)
                    End Using
                End Using
            End Using

            grid.DataSource = table

            If grid.Columns.Contains("Time") Then
                grid.Columns("Time").DefaultCellStyle.Format =
                    "MMM dd, yyyy hh:mm tt"
                grid.Columns("Time").FillWeight = 24
            End If

            If grid.Columns.Contains("Action") Then
                grid.Columns("Action").FillWeight = 18
            End If

            If grid.Columns.Contains("Record Type") Then
                grid.Columns("Record Type").FillWeight = 18
            End If

            If grid.Columns.Contains("Record ID") Then
                grid.Columns("Record ID").FillWeight = 12
            End If

            If grid.Columns.Contains("Description") Then
                grid.Columns("Description").FillWeight = 42
                grid.Columns("Description").MinimumWidth = 220
            End If
        End Sub

        ' =========================================================
        ' ROOM MANAGEMENT
        ' =========================================================

        Private Sub ShowRooms()
            SetPage("Room Management")

            Dim roomControl As New RoomManagementControl With {
                .Dock = DockStyle.Fill
            }

            contentPanel.Controls.Add(roomControl)
            roomControl.BringToFront()
        End Sub

        ' =========================================================
        ' PARKING MANAGEMENT
        ' =========================================================

        Private Sub ShowParking()
            SetPage("Parking Management")

            Dim page As TableLayoutPanel = CreatePageLayout()

            AddPageRow(page, CreateSectionTitle(
                "Parking Management",
                "Manage parking spaces and vehicle-category hourly rates."))

            Dim menu As New FlowLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .WrapContents = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .BackColor = backgroundColor,
                .Margin = New Padding(0, 0, 0, 14),
                .Padding = New Padding(0)
            }

            AddParkingSectionButton(menu, "Parking Spaces", "Spaces")
            AddParkingSectionButton(menu, "Active Tickets", "Active Tickets")
            AddParkingSectionButton(menu, "Parking History", "History")
            AddParkingSectionButton(menu, "Departures", "Departures")
            AddParkingSectionButton(menu, "Parking Pricing", "Pricing")

            AddPageRow(page, menu)

            Dim stats As New FlowLayoutPanel With {
                .Dock = DockStyle.Top,
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .WrapContents = True,
                .BackColor = backgroundColor,
                .Margin = New Padding(0, 0, 0, 14),
                .Padding = New Padding(0)
            }

            Try
                Using conn As MySqlConnection = DbConnectionFactory.Create()
                    conn.Open()

                    AddSimpleStat(
                        stats,
                        "Total Spaces",
                        ScalarCount(
                            conn,
                            "SELECT COUNT(*) FROM parking_spaces").ToString("N0"))

                    AddSimpleStat(
                        stats,
                        "Available Spaces",
                        ScalarCount(
                            conn,
                            "SELECT COUNT(*) FROM parking_spaces " &
                            "WHERE is_active = 1 AND status = 'AVAILABLE'").ToString("N0"))

                    AddSimpleStat(
                        stats,
                        "Occupied Spaces",
                        ScalarCount(
                            conn,
                            "SELECT COUNT(*) FROM parking_spaces " &
                            "WHERE is_active = 1 AND status = 'OCCUPIED'").ToString("N0"))
                End Using
            Catch
                AddPageRow(page, CreateInfoPanel(
                    "Parking statistics could not be loaded. " &
                    "Check the parking_spaces table and database connection."))
            End Try

            AddPageRow(page, stats)
            AddPageRow(page, CreateInfoPanel(
                "Choose Parking Spaces to manage inventory, or Parking Pricing to edit hourly rates."))

            contentPanel.Controls.Add(page)
        End Sub

        Private Sub AddSimpleStat(
            host As FlowLayoutPanel,
            caption As String,
            value As String)

            Dim card As New Panel With {
                .Width = 190,
                .Height = 86,
                .BackColor = Color.White,
                .Margin = New Padding(0, 0, 12, 12),
                .Padding = New Padding(12),
                .BorderStyle = BorderStyle.FixedSingle
            }

            Dim valueLabel As New Label With {
                .Text = value,
                .Font = New Font("Segoe UI", 18.0F, FontStyle.Bold),
                .ForeColor = navy,
                .Dock = DockStyle.Top,
                .Height = 32
            }

            Dim captionLabel As New Label With {
                .Text = caption,
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = muted,
                .Dock = DockStyle.Fill
            }

            card.Controls.Add(captionLabel)
            card.Controls.Add(valueLabel)
            host.Controls.Add(card)
        End Sub

        Private Sub AddParkingSectionButton(
    host As FlowLayoutPanel,
    caption As String,
    sectionKey As String)

            Dim accentColor As Color = teal

            Dim iconText As String

            Select Case sectionKey
                Case "Spaces"
                    iconText = "P"
                Case "Active Tickets"
                    iconText = "A"
                Case "History"
                    iconText = "H"
                Case "Departures"
                    iconText = "D"
                Case "Pricing"
                    iconText = "₱"
                Case Else
                    iconText = ">"
            End Select

            Dim button As New Button With {
        .Text = iconText & "    " & caption & "    ›",
        .Width = 225,
        .Height = 58,
        .BackColor = Color.White,
        .ForeColor = navy,
        .FlatStyle = FlatStyle.Flat,
        .Font = New Font("Segoe UI Semibold", 10.0F),
        .TextAlign = ContentAlignment.MiddleLeft,
        .Padding = New Padding(14, 0, 8, 0),
        .Margin = New Padding(0, 0, 12, 12),
        .Cursor = Cursors.Hand,
        .UseVisualStyleBackColor = False,
        .AccessibleName = caption,
        .AccessibleDescription = "Open " & caption
    }

            button.FlatAppearance.BorderSize = 1
            button.FlatAppearance.BorderColor = borderColor
            button.FlatAppearance.MouseOverBackColor =
        Color.FromArgb(232, 247, 244)
            button.FlatAppearance.MouseDownBackColor =
        Color.FromArgb(211, 239, 234)

            AddHandler button.Paint,
        Sub(sender As Object, e As PaintEventArgs)
            Dim currentButton As Button = DirectCast(sender, Button)
            Dim g As Graphics = e.Graphics

            g.SmoothingMode = SmoothingMode.AntiAlias

            ' Teal accent bar on the left.
            Using accentBrush As New SolidBrush(accentColor)
                g.FillRectangle(
                    accentBrush, 0, 0, 4, currentButton.Height)
            End Using

            ' Keep the arrow aligned to the right.
            TextRenderer.DrawText(
                g,
                "›",
                New Font("Segoe UI", 19.0F, FontStyle.Regular),
                New Rectangle(
                    currentButton.Width - 32,
                    0,
                    24,
                    currentButton.Height),
                accentColor,
                TextFormatFlags.HorizontalCenter Or
                TextFormatFlags.VerticalCenter)
        End Sub

            AddHandler button.Click,
        Sub()
            ShowParkingSection(sectionKey)
        End Sub

            host.Controls.Add(button)
        End Sub

        Private Sub ShowParkingSection(sectionKey As String)
            ' Pricing is a separate existing Form, so open it as a dialog.
            ' It is not embedded inside the dashboard content panel.
            If sectionKey = "Pricing" Then
                OpenParkingPricing()
                Return
            End If

            ' The existing parking control owns space listing, filters,
            ' rates display, adding, editing and refreshing.
            If sectionKey = "Spaces" Then
                ShowParkingSpaces()
                Return
            End If

            SetPage("Parking Management - " & sectionKey, "Parking Management")

            Dim page As TableLayoutPanel = CreatePageLayout()

            Dim backButton As New Button With {
                .Text = "< Back to Parking Management",
                .AutoSize = True,
                .FlatStyle = FlatStyle.Flat,
                .ForeColor = navy,
                .BackColor = backgroundColor,
                .Cursor = Cursors.Hand,
                .Margin = New Padding(0, 0, 0, 14)
            }

            backButton.FlatAppearance.BorderSize = 0
            AddHandler backButton.Click,
                Sub()
                    ShowParking()
                End Sub

            AddPageRow(page, backButton)
            AddPageRow(page, CreateSectionTitle(
                sectionKey, GetParkingSectionDescription(sectionKey)))

            AddPageRow(page, CreateInfoPanel(
                "This section is not connected because its underlying parking-session " &
                "table and columns have not been confirmed. Parking space management " &
                "and pricing are connected to the existing forms."))

            contentPanel.Controls.Add(page)
        End Sub

        Private Sub ShowParkingSpaces()
            SetPage("Parking Spaces", "Parking Management")

            Dim parkingControl As New ParkingManagementControl With {
                .Dock = DockStyle.Fill,
                .Margin = New Padding(0)
            }

            contentPanel.Controls.Add(parkingControl)
            parkingControl.BringToFront()
        End Sub

        Private Sub OpenParkingPricing()
            Try
                Using pricingForm As New ParkingPricingForm()
                    pricingForm.StartPosition = FormStartPosition.CenterParent
                    pricingForm.ShowDialog(Me)
                End Using

                ' Refresh the parking overview after the pricing form closes.
                If Not IsDisposed Then
                    ShowParking()
                End If

            Catch ex As Exception
                MessageBox.Show(
                    "Unable to open Parking Pricing." &
                    Environment.NewLine & Environment.NewLine &
                    ex.Message,
                    "Parking Pricing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Function GetParkingSectionDescription(
            sectionKey As String) As String

            Select Case sectionKey
                Case "Spaces"
                    Return "Manage parking-space inventory and operational status."
                Case "Active Tickets"
                    Return "Review current parking sessions and assigned vehicles."
                Case "History"
                    Return "Review completed parking sessions."
                Case "Departures"
                    Return "Review parking sessions for departing vehicles."
                Case Else
                    Return "Manage parking operations."
            End Select
        End Function

        ' =========================================================
        ' OTHER MODULES
        ' =========================================================

        Private Sub ShowHistory()
            SetPage("Guest Stay History")

            Dim history As New GuestStayHistoryControl With {
        .Dock = DockStyle.Fill
    }

            contentPanel.Controls.Add(history)
        End Sub


        Private Sub ShowReports()
            SetPage("Reports")

            contentPanel.Controls.Clear()

            Dim reports As New ReportsControl With {
        .Dock = DockStyle.Fill
    }

            contentPanel.Controls.Add(reports)
        End Sub


        Private Sub ShowStaff()
            SetPage("Staff Management")

            Dim staffControl As New ucStaffManagement With {
                .Dock = DockStyle.Fill
            }

            contentPanel.Controls.Add(staffControl)
            staffControl.BringToFront()
        End Sub

        Private Sub ShowModulePlaceholder(
            title As String,
            description As String)

            SetPage(title)

            Dim page As TableLayoutPanel = CreatePageLayout()
            AddPageRow(page, CreateSectionTitle(title, description))
            AddPageRow(page, CreateInfoPanel(description))

            contentPanel.Controls.Add(page)
        End Sub

        Private Function CreateInfoPanel(message As String) As Panel
            Dim panel As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 72,
                .BackColor = Color.White,
                .Padding = New Padding(16),
                .Margin = New Padding(0, 0, 0, 14),
                .BorderStyle = BorderStyle.FixedSingle
            }

            Dim label As New Label With {
                .Text = message,
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .TextAlign = ContentAlignment.MiddleLeft,
                .AutoEllipsis = True
            }

            panel.Controls.Add(label)
            Return panel
        End Function

    End Class

End Namespace