Imports System.Drawing
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS
    Public NotInheritable Class FirstAdminSetupForm
        Inherits Form

        Private ReadOnly txtFullName As New TextBox()
        Private ReadOnly txtUsername As New TextBox()
        Private ReadOnly txtPassword As New TextBox()
        Private ReadOnly txtConfirm As New TextBox()
        Private ReadOnly btnCreate As New Button()
        Private ReadOnly lblStatus As New Label()

        Public Sub New()
            Text = "StayPoint HMS — Initial Administrator Setup"
            StartPosition = FormStartPosition.CenterScreen
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            ClientSize = New Size(560, 490)
            BackColor = Color.FromArgb(240, 244, 248)
            Font = New Font("Segoe UI", 10.0F)

            Dim title As New Label With {
                .Text = "Create the first administrator",
                .Font = New Font("Segoe UI", 19.0F, FontStyle.Bold),
                .ForeColor = Color.FromArgb(23, 43, 77),
                .TextAlign = ContentAlignment.MiddleCenter
            }
            title.SetBounds(25, 22, 510, 48)
            Controls.Add(title)

            AddField("Full name", txtFullName, 95)
            AddField("Username", txtUsername, 165)
            AddField("Password (minimum 12 characters)", txtPassword, 235)
            AddField("Confirm password", txtConfirm, 305)
            txtPassword.UseSystemPasswordChar = True
            txtConfirm.UseSystemPasswordChar = True

            btnCreate.Text = "Create Administrator"
            btnCreate.SetBounds(90, 375, 380, 44)
            btnCreate.BackColor = Color.FromArgb(22, 160, 145)
            btnCreate.ForeColor = Color.White
            btnCreate.FlatStyle = FlatStyle.Flat
            btnCreate.FlatAppearance.BorderSize = 0
            AddHandler btnCreate.Click, AddressOf CreateAdmin
            Controls.Add(btnCreate)

            lblStatus.SetBounds(25, 430, 510, 40)
            lblStatus.ForeColor = Color.FromArgb(180, 40, 40)
            lblStatus.TextAlign = ContentAlignment.MiddleCenter
            Controls.Add(lblStatus)
        End Sub

        Private Sub AddField(caption As String, box As TextBox, top As Integer)
            Dim label As New Label With {.Text = caption, .ForeColor = Color.FromArgb(23, 43, 77)}
            label.SetBounds(55, top, 450, 22)
            box.SetBounds(55, top + 24, 450, 34)
            Controls.Add(label)
            Controls.Add(box)
        End Sub

        Private Sub CreateAdmin(sender As Object, e As EventArgs)
            Dim fullName = txtFullName.Text.Trim()
            Dim username = txtUsername.Text.Trim()
            Dim password = txtPassword.Text

            If fullName.Length = 0 OrElse username.Length = 0 Then
                lblStatus.Text = "Enter your name and username."
                Return
            End If
            If username.Length > 50 OrElse username.Contains(" "c) Then
                lblStatus.Text = "Username must be 1–50 characters with no spaces."
                Return
            End If
            If password.Length < 12 Then
                lblStatus.Text = "Use a password with at least 12 characters."
                Return
            End If
            If password <> txtConfirm.Text Then
                lblStatus.Text = "Passwords do not match."
                Return
            End If

            btnCreate.Enabled = False
            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()
                    Using tx = conn.BeginTransaction()
                        Using checkCmd As New MySqlCommand("SELECT COUNT(*) FROM users WHERE role='ADMIN'", conn, tx)
                            If Convert.ToInt32(checkCmd.ExecuteScalar()) > 0 Then
                                tx.Rollback()
                                lblStatus.Text = "An administrator already exists. Restart the application to sign in."
                                Return
                            End If
                        End Using
                        Using cmd As New MySqlCommand(
                            "INSERT INTO users (full_name, username, password_hash, role, is_active) VALUES (@name,@username,@hash,'ADMIN',1)", conn, tx)
                            cmd.Parameters.AddWithValue("@name", fullName)
                            cmd.Parameters.AddWithValue("@username", username)
                            cmd.Parameters.AddWithValue("@hash", PasswordHasher.HashPassword(password))
                            cmd.ExecuteNonQuery()
                        End Using
                        tx.Commit()
                    End Using
                End Using
                MessageBox.Show("Administrator created. Restart StayPoint HMS and sign in.", "Setup Complete",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Close()
            Catch ex As MySqlException When ex.Number = 1062
                lblStatus.Text = "That username is already in use."
                btnCreate.Enabled = True
            Catch ex As Exception
                lblStatus.Text = "Could not create administrator: " & ex.Message
                btnCreate.Enabled = True
            End Try
        End Sub
    End Class
End Namespace
