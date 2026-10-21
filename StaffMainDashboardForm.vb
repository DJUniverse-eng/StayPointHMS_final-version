
Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Namespace StayPointHMS

    Public NotInheritable Class StaffMainDashboardForm
        Inherits Form

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly pageBg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)

        Private ReadOnly logo As New PictureBox()
        Private ReadOnly workspace As New FlowLayoutPanel()
        Private ReadOnly roomButton As New Button()
        Private ReadOnly parkingButton As New Button()

        Public Property LogoutRequested As Boolean

        Public Sub New()
            UserRoleAuthorization.RequireRole(
                UserRoleAuthorization.StaffRoom,
                UserRoleAuthorization.StaffParking)

            Text = "StayPoint HMS - Staff Workspace"
            StartPosition = FormStartPosition.CenterScreen
            WindowState = FormWindowState.Maximized
            MinimumSize = New Size(850, 600)
            BackColor = pageBg
            Font = New Font("Segoe UI", 10.0F)
            AutoScaleMode = AutoScaleMode.Dpi

            BuildLayout()
        End Sub

        Private Sub BuildLayout()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .Margin = New Padding(0),
                .BackColor = pageBg
            }

            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 96))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 66))

            root.Controls.Add(BuildHeader(), 0, 0)
            root.Controls.Add(BuildContent(), 0, 1)
            root.Controls.Add(BuildFooter(), 0, 2)

            Controls.Add(root)
        End Sub

        Private Function BuildHeader() As Control
            Dim header As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = navy,
                .Padding = New Padding(24, 10, 24, 10)
            }

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 3,
                .RowCount = 1,
                .BackColor = navy,
                .Margin = New Padding(0)
            }

            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))

            logo.Size = New Size(66, 66)
            logo.SizeMode = PictureBoxSizeMode.Zoom
            logo.Anchor = AnchorStyles.Left
            logo.Margin = New Padding(0)
            logo.Image = LoadLogo()

            Dim logoPanel As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = navy
            }
            logoPanel.Controls.Add(logo)

            Dim brand As New Label With {
                .Text = "STAYPOINT HMS",
                .Dock = DockStyle.Fill,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 21, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Margin = New Padding(0)
            }

            Dim staff As New Label With {
                .Text = "Welcome, " & SafeStaffName() &
                        Environment.NewLine &
                        If(UserRoleAuthorization.IsRoomStaff(),
                           "STAFF - ROOM", "STAFF - PARKING"),
                .Dock = DockStyle.Fill,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleRight,
                .Margin = New Padding(0)
            }

            layout.Controls.Add(logoPanel, 0, 0)
            layout.Controls.Add(brand, 1, 0)
            layout.Controls.Add(staff, 2, 0)

            header.Controls.Add(layout)
            Return header
        End Function

        Private Function BuildContent() As Control
            Dim content As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .Padding = New Padding(24, 28, 24, 20),
                .BackColor = pageBg,
                .Margin = New Padding(0)
            }

            content.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            content.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            content.RowStyles.Add(New RowStyle(SizeType.AutoSize))
            content.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim heading As New Label With {
                .Text = If(UserRoleAuthorization.IsRoomStaff(),
                          "Room Staff Dashboard",
                          "Parking Staff Dashboard"),
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .Font = New Font("Segoe UI", 24, FontStyle.Bold),
                .ForeColor = navy,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Margin = New Padding(0, 0, 0, 8)
            }

            Dim subtitle As New Label With {
                .Text = If(UserRoleAuthorization.IsRoomStaff(),
                          "Room operations available to your account.",
                          "Parking operations available to your account."),
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .Font = New Font("Segoe UI", 10.5F),
                .ForeColor = muted,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Margin = New Padding(0, 0, 0, 22)
            }

            Dim workspaceHost As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = pageBg,
                .AutoScroll = True,
                .Margin = New Padding(0)
            }

            workspace.Dock = DockStyle.None
            workspace.AutoSize = True
            workspace.AutoSizeMode = AutoSizeMode.GrowAndShrink
            workspace.WrapContents = True
            workspace.FlowDirection = FlowDirection.LeftToRight
            workspace.BackColor = pageBg
            workspace.Padding = New Padding(10)
            workspace.Margin = New Padding(0)

            ConfigureWorkspaceButton(
                roomButton,
                "ROOM MANAGEMENT",
                "Guest registration, room assignments, check-in and check-out.",
                navy)

            ConfigureWorkspaceButton(
                parkingButton,
                "PARKING MANAGEMENT",
                "Parking activity, vehicle entry, tickets and departures.",
                teal)

            If UserRoleAuthorization.IsRoomStaff() Then
                parkingButton.Visible = False
                parkingButton.Enabled = False
            ElseIf UserRoleAuthorization.IsParkingStaff() Then
                roomButton.Visible = False
                roomButton.Enabled = False
            End If

            AddHandler roomButton.Click, AddressOf OpenRoomDashboard
            AddHandler parkingButton.Click, AddressOf OpenParkingDashboard
            AddHandler workspaceHost.Resize, AddressOf CenterWorkspace
            AddHandler workspace.Resize, AddressOf CenterWorkspace

            workspace.Controls.Add(roomButton)
            workspace.Controls.Add(parkingButton)
            workspaceHost.Controls.Add(workspace)

            content.Controls.Add(heading, 0, 0)
            content.Controls.Add(subtitle, 0, 1)
            content.Controls.Add(workspaceHost, 0, 2)

            AddHandler workspaceHost.HandleCreated,
                Sub(sender As Object, e As EventArgs)
                    CenterWorkspace(Nothing, EventArgs.Empty)
                End Sub

            Return content
        End Function

        Private Sub ConfigureWorkspaceButton(
            button As Button,
            title As String,
            description As String,
            accent As Color)

            button.Text = title & Environment.NewLine &
                          Environment.NewLine & description
            button.Size = New Size(370, 190)
            button.MinimumSize = New Size(270, 165)
            button.BackColor = Color.White
            button.ForeColor = navy
            button.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
            button.TextAlign = ContentAlignment.MiddleCenter
            button.Padding = New Padding(24)
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderColor = borderColor
            button.FlatAppearance.BorderSize = 1
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 250, 252)
            button.UseVisualStyleBackColor = False
            button.Cursor = Cursors.Hand
            button.Margin = New Padding(12)

            AddHandler button.Paint,
                Sub(sender As Object, e As PaintEventArgs)
                    Using brush As New SolidBrush(accent)
                        e.Graphics.FillRectangle(brush, 0, 0, 7, button.Height)
                    End Using
                End Sub
        End Sub

        Private Sub CenterWorkspace(sender As Object, e As EventArgs)
            If workspace.Parent Is Nothing Then Return

            workspace.Left = Math.Max(
                0,
                (workspace.Parent.ClientSize.Width - workspace.Width) \ 2)

            workspace.Top = Math.Max(
                0,
                (workspace.Parent.ClientSize.Height - workspace.Height) \ 2)
        End Sub

        Private Sub OpenRoomDashboard(sender As Object, e As EventArgs)
            If Not UserRoleAuthorization.IsRoomStaff() Then
                MessageBox.Show(Me, "This account is not authorized for room operations.",
                                "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Using dashboard As New StaffRoomDashboardForm()
                dashboard.ShowDialog(Me)

                If dashboard.LogoutRequested Then
                    LogoutRequested = True
                    Close()
                    Return
                End If

                If dashboard.ExitRequested Then
                    Application.Exit()
                End If
            End Using
        End Sub

        Private Sub OpenParkingDashboard(sender As Object, e As EventArgs)
            If Not UserRoleAuthorization.IsParkingStaff() Then
                MessageBox.Show(Me, "This account is not authorized for parking operations.",
                                "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim parkingDashboard As New StaffParkingDashboardControl With {
        .Dock = DockStyle.Fill
    }

            AddHandler parkingDashboard.BackRequested,
        Sub(s, args)
            RemoveHandler parkingDashboard.LogoutRequested, Nothing
            Close()
        End Sub

            AddHandler parkingDashboard.LogoutRequested,
        Sub(s, args)
            LogoutRequested = True
            Close()
        End Sub

            AddHandler parkingDashboard.ExitRequested,
        Sub(s, args)
            Application.Exit()
        End Sub

            Dim host As New Form With {
        .Text = "StayPoint HMS - Parking Operations",
        .StartPosition = FormStartPosition.CenterParent,
        .WindowState = FormWindowState.Maximized,
        .MinimumSize = New Size(1000, 650),
        .BackColor = Color.FromArgb(240, 244, 248)
    }

            host.Controls.Add(parkingDashboard)
            host.ShowDialog(Me)
        End Sub

        Private Function BuildFooter() As Control
            Dim footer As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(20, 10, 20, 10)
            }

            Dim message As New Label With {
                .Text = "StayPoint HMS  |  Staff Operations",
                .Dock = DockStyle.Fill,
                .ForeColor = muted,
                .TextAlign = ContentAlignment.MiddleLeft
            }

            Dim exitButton As Button = CreateButton(
                "Exit Application", Color.White, navy, 145)
            exitButton.Dock = DockStyle.Right

            Dim logoutButton As Button = CreateButton(
                "Log Out", navy, Color.White, 105)
            logoutButton.Dock = DockStyle.Right

            AddHandler logoutButton.Click, AddressOf HandleStaffLogout
            AddHandler exitButton.Click, AddressOf HandleApplicationExit

            footer.Controls.Add(message)
            footer.Controls.Add(exitButton)
            footer.Controls.Add(logoutButton)

            Return footer
        End Function

        Private Function CreateButton(
            caption As String,
            background As Color,
            foreground As Color,
            buttonWidth As Integer) As Button

            Dim result As New Button With {
                .Text = caption,
                .Width = buttonWidth,
                .Height = 36,
                .BackColor = background,
                .ForeColor = foreground,
                .FlatStyle = FlatStyle.Flat,
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
                .Cursor = Cursors.Hand,
                .UseVisualStyleBackColor = False,
                .Margin = New Padding(5, 0, 5, 0)
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

            Application.Exit()
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
            If logo.Image IsNot Nothing Then
                logo.Image.Dispose()
                logo.Image = Nothing
            End If

            MyBase.OnFormClosed(e)
        End Sub

    End Class

End Namespace
