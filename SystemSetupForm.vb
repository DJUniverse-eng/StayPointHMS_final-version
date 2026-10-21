Option Strict On
Option Explicit On
Option Infer On

Imports System.Drawing
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS
    Public NotInheritable Class SystemSetupForm
        Inherits Form

        Public Sub New()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.Admin)
            Text = "StayPoint HMS - System Setup"
            StartPosition = FormStartPosition.CenterScreen
            WindowState = FormWindowState.Maximized
            MinimumSize = New Size(1100, 700)
            BackColor = Color.FromArgb(240, 244, 248)

            Dim control As New SystemSetupControl With {.Dock = DockStyle.Fill}
            Controls.Add(control)

            Dim footer As New Panel With {.Dock = DockStyle.Bottom, .Height = 48, .BackColor = Color.White}
            Dim done As New Button With {
                .Text = "Finish Setup",
                .Dock = DockStyle.Right,
                .Width = 160,
                .BackColor = Color.FromArgb(22, 160, 145),
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat
            }
            AddHandler done.Click, Sub()
                If IsSetupComplete() Then
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    MessageBox.Show(Me, "Save hotel information and generate inventory before finishing setup.",
                                    "System Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            End Sub
            footer.Controls.Add(done)
            Controls.Add(footer)
        End Sub

        Private Function IsSetupComplete() As Boolean
            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()
                    Using cmd As New MySqlCommand("SELECT setting_value FROM app_settings WHERE setting_key='setup_completed' LIMIT 1", conn)
                        Return String.Equals(Convert.ToString(cmd.ExecuteScalar()), "1", StringComparison.Ordinal)
                    End Using
                End Using
            Catch
                Return False
            End Try
        End Function
    End Class
End Namespace
