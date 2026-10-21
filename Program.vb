
Imports System
Imports System.Windows.Forms
Imports MySqlConnector
Imports StayPointHMS.StayPointHMS

Friend Module Program

    <STAThread()>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Try
            If Not AdministratorExists() Then
                Using setup As New StayPointHMS.FirstAdminSetupForm()
                    setup.ShowDialog()
                End Using

                ' Recheck after setup closes.
                If Not AdministratorExists() Then
                    Return
                End If
            End If

            Using login As New StayPointHMS.LoginForm()
                If login.ShowDialog() <> DialogResult.OK Then
                    Return
                End If
            End Using

            If String.Equals(CurrentUser.Role, "ADMIN",
                             StringComparison.OrdinalIgnoreCase) Then

                If Not SystemSetupComplete() Then
                    Using setup As New StayPointHMS.SystemSetupForm()
                        If setup.ShowDialog() <> DialogResult.OK Then Return
                    End Using
                End If

                Application.Run(
                    New StayPointHMS.AdminDashboardForm())

            ElseIf Not StaffOperationsReady() Then
                MessageBox.Show(
                    "The administrator must complete System Setup, generate room and parking inventory, and configure positive room and parking rates before staff operations are available.",
                    "StayPoint HMS - Setup Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
                Return

            ElseIf String.Equals(CurrentUser.Role, "STAFF_ROOM",
                                 StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(CurrentUser.Role, "STAFF_PARKING",
                                 StringComparison.OrdinalIgnoreCase) Then

                Application.Run(
                    New StayPointHMS.StaffMainDashboardForm())

            Else
                MessageBox.Show(
                    "Your account role is invalid. Contact the administrator.",
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
            End If

        Catch ex As Exception
            MessageBox.Show(
                "StayPoint HMS could not start." &
                Environment.NewLine & Environment.NewLine &
                ex.Message,
                "Startup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function SystemSetupComplete() As Boolean
        Try
            Using conn = DbConnectionFactory.Create()
                conn.Open()
                Using cmd As New MySqlCommand(
                    "SELECT setting_value FROM app_settings WHERE setting_key='setup_completed' LIMIT 1",
                    conn)
                    Return String.Equals(Convert.ToString(cmd.ExecuteScalar()), "1",
                                         StringComparison.Ordinal)
                End Using
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Function StaffOperationsReady() As Boolean
        Try
            Using conn = DbConnectionFactory.Create()
                conn.Open()
                Using cmd As New MySqlCommand(
                    "SELECT " &
                    "(SELECT COUNT(*) FROM app_settings WHERE setting_key='setup_completed' AND setting_value='1') AS completed, " &
                    "(SELECT COUNT(*) FROM rooms WHERE is_active=1) AS rooms, " &
                    "(SELECT COUNT(*) FROM parking_spaces WHERE is_active=1) AS spaces, " &
                    "(SELECT COUNT(*) FROM room_type_options WHERE is_active=1 AND hourly_rate>0 AND daily_rate>0) AS room_rates, " &
                    "(SELECT COUNT(*) FROM parking_rate_options WHERE is_active=1 AND hourly_rate>0) AS parking_rates", conn)
                    Using r = cmd.ExecuteReader()
                        If Not r.Read() Then Return False
                        Return Convert.ToInt32(r("completed")) > 0 AndAlso
                               Convert.ToInt32(r("rooms")) > 0 AndAlso
                               Convert.ToInt32(r("spaces")) > 0 AndAlso
                               Convert.ToInt32(r("room_rates")) > 0 AndAlso
                               Convert.ToInt32(r("parking_rates")) > 0
                    End Using
                End Using
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Function AdministratorExists() As Boolean
        Using conn = DbConnectionFactory.Create()
            conn.Open()

            Using cmd As New MySqlCommand(
                "SELECT COUNT(*) FROM users WHERE role = 'ADMIN' AND is_active = 1",
                conn)

                Return Convert.ToInt32(cmd.ExecuteScalar()) > 0
            End Using
        End Using
    End Function

End Module
