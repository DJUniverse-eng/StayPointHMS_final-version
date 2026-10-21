
Imports System.Drawing
Imports System.Windows.Forms
Imports MySqlConnector
Imports System.IO

Namespace StayPointHMS

    Public NotInheritable Class LoginForm
        Inherits Form

        Private ReadOnly txtUsername As New TextBox()
        Private ReadOnly txtPassword As New TextBox()
        Private ReadOnly chkShowPassword As New CheckBox()
        Private ReadOnly btnLogin As New Button()
        Private ReadOnly btnExit As New Button()
        Private ReadOnly lblStatus As New Label()
        Private ReadOnly picLogo As New PictureBox()

        Private ReadOnly brandPanel As New Panel()
        Private ReadOnly loginPanel As New Panel()

        Private Shared ReadOnly Navy As Color = Color.FromArgb(23, 43, 77)
        Private Shared ReadOnly SidebarColor As Color = Color.FromArgb(32, 58, 95)
        Private Shared ReadOnly Teal As Color = Color.FromArgb(22, 160, 145)
        Private Shared ReadOnly BackgroundColor As Color = Color.FromArgb(240, 244, 248)
        Private Shared ReadOnly MutedColor As Color = Color.FromArgb(100, 116, 139)
        Private Shared ReadOnly BorderColor As Color = Color.FromArgb(220, 226, 234)

        Public Sub New()
            InitializeLoginForm()
        End Sub

        Private Sub InitializeLoginForm()
            Text = "StayPoint HMS | Sign In"
            Name = "LoginForm"
            StartPosition = FormStartPosition.CenterScreen
            FormBorderStyle = FormBorderStyle.FixedSingle
            MaximizeBox = False
            MinimizeBox = True
            ShowIcon = False
            ClientSize = New Size(920, 560)
            BackColor = BackgroundColor
            Font = New Font("Segoe UI", 10.0F)
            KeyPreview = True

            BuildBrandPanel()
            BuildLoginPanel()

            Controls.Add(loginPanel)
            Controls.Add(brandPanel)

            AcceptButton = btnLogin
            CancelButton = btnExit

            AddHandler chkShowPassword.CheckedChanged,
                AddressOf ShowPasswordChanged

            AddHandler btnLogin.Click, AddressOf LoginClicked
            AddHandler btnExit.Click, AddressOf ExitClicked
            AddHandler Me.Shown, AddressOf LoginFormShown
        End Sub

        ' LEFT SIDE: BRANDING AND LOGO

        Private Sub BuildBrandPanel()
            brandPanel.SetBounds(0, 0, 390, 560)
            brandPanel.BackColor = Navy
            brandPanel.Anchor = AnchorStyles.Top Or
                        AnchorStyles.Bottom Or
                        AnchorStyles.Left

            picLogo.Name = "picLogo"
            picLogo.SetBounds(35, 130, 320, 200)
            picLogo.SizeMode = PictureBoxSizeMode.Zoom
            picLogo.BackColor = Color.Transparent

            Dim logoPath As String = Path.Combine(
        AppContext.BaseDirectory,
        "Resources",
        "staypoint-logo.png"
    )

            If File.Exists(logoPath) Then
                If picLogo.Image IsNot Nothing Then
                    picLogo.Image.Dispose()
                    picLogo.Image = Nothing
                End If

                Using fs As New FileStream(
            logoPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite)

                    Using tempImage As Image = Image.FromStream(fs)
                        picLogo.Image = New Bitmap(tempImage)
                    End Using
                End Using
            End If

            Dim lblDescription As New Label With {
        .Text = "Hotel Management System" & Environment.NewLine &
                "Simplifying hotel operations, one stay at a time.",
        .Font = New Font("Segoe UI", 9.5F),
        .ForeColor = Color.FromArgb(210, 222, 239),
        .TextAlign = ContentAlignment.MiddleCenter
    }
            lblDescription.SetBounds(25, 345, 340, 55)

            Dim accent As New Panel With {
        .BackColor = Teal
    }
            accent.SetBounds(145, 420, 100, 4)

            brandPanel.Controls.AddRange(
        New Control() {
            picLogo,
            lblDescription,
            accent
        })
        End Sub


        ' RIGHT SIDE: LOGIN FORM

        Private Sub BuildLoginPanel()
            loginPanel.SetBounds(430, 55, 440, 450)
            loginPanel.BackColor = Color.White
            loginPanel.BorderStyle = BorderStyle.FixedSingle

            Dim lblWelcome As New Label With {
        .Text = "Welcome Back!",
        .Font = New Font("Segoe UI", 25.0F, FontStyle.Bold),
        .ForeColor = Navy,
        .TextAlign = ContentAlignment.MiddleLeft
    }
            lblWelcome.SetBounds(35, 22, 365, 45)

            Dim lblSubtitle As New Label With {
        .Text = "Sign in to continue to StayPoint HMS.",
        .Font = New Font("Segoe UI", 10.0F),
        .ForeColor = MutedColor
    }
            lblSubtitle.SetBounds(37, 68, 365, 25)

            Dim lblUsername As New Label With {
        .Text = "Username",
        .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
        .ForeColor = Navy
    }
            lblUsername.SetBounds(37, 112, 365, 23)

            ConfigureTextBox(txtUsername, "Enter your username")
            txtUsername.SetBounds(35, 139, 366, 40)

            Dim lblPassword As New Label With {
        .Text = "Password",
        .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
        .ForeColor = Navy
    }
            lblPassword.SetBounds(37, 190, 365, 23)

            ConfigureTextBox(txtPassword, "Enter your password")
            txtPassword.UseSystemPasswordChar = True
            txtPassword.SetBounds(35, 217, 366, 40)

            chkShowPassword.Text = "Show password"
            chkShowPassword.Font = New Font("Segoe UI", 9.0F)
            chkShowPassword.ForeColor = MutedColor
            chkShowPassword.BackColor = Color.White
            chkShowPassword.AutoSize = True
            chkShowPassword.SetBounds(37, 265, 150, 24)

            btnLogin.Text = "Sign In  →"
            btnLogin.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
            btnLogin.BackColor = Teal
            btnLogin.ForeColor = Color.White
            btnLogin.FlatStyle = FlatStyle.Flat
            btnLogin.FlatAppearance.BorderSize = 0
            btnLogin.FlatAppearance.MouseOverBackColor =
        Color.FromArgb(18, 137, 124)
            btnLogin.Cursor = Cursors.Hand
            btnLogin.SetBounds(35, 305, 366, 45)

            btnExit.Text = "Exit Application"
            btnExit.Font = New Font("Segoe UI", 9.0F)
            btnExit.BackColor = Color.White
            btnExit.ForeColor = MutedColor
            btnExit.FlatStyle = FlatStyle.Flat
            btnExit.FlatAppearance.BorderSize = 0
            btnExit.Cursor = Cursors.Hand
            btnExit.SetBounds(35, 355, 366, 28)

            lblStatus.Text = ""
            lblStatus.Font = New Font("Segoe UI", 8.5F)
            lblStatus.ForeColor = Color.FromArgb(180, 40, 40)
            lblStatus.TextAlign = ContentAlignment.MiddleLeft
            lblStatus.AutoEllipsis = True
            lblStatus.SetBounds(35, 395, 366, 30)

            loginPanel.Controls.AddRange(
        New Control() {
            lblWelcome,
            lblSubtitle,
            lblUsername,
            txtUsername,
            lblPassword,
            txtPassword,
            chkShowPassword,
            btnLogin,
            btnExit,
            lblStatus
        })
        End Sub


        Private Sub ConfigureTextBox(
            textBox As TextBox,
            placeholder As String)

            textBox.Font = New Font("Segoe UI", 10.5F)
            textBox.ForeColor = Navy
            textBox.BackColor = Color.White
            textBox.BorderStyle = BorderStyle.FixedSingle
            textBox.PlaceholderText = placeholder
        End Sub

        Private Sub ShowPasswordChanged(
            sender As Object,
            e As EventArgs)

            txtPassword.UseSystemPasswordChar =
                Not chkShowPassword.Checked

            txtPassword.Focus()
            txtPassword.SelectionStart = txtPassword.TextLength
        End Sub

        Private Sub LoginFormShown(
            sender As Object,
            e As EventArgs)

            txtUsername.Focus()
        End Sub

        Private Sub ExitClicked(
            sender As Object,
            e As EventArgs)

            DialogResult = DialogResult.Cancel
            Close()
        End Sub

        Private Sub LoginClicked(
            sender As Object,
            e As EventArgs)

            Dim username As String = txtUsername.Text.Trim()
            Dim password As String = txtPassword.Text

            lblStatus.Text = ""

            If username.Length = 0 OrElse password.Length = 0 Then
                lblStatus.Text = "Enter your username and password."

                If username.Length = 0 Then
                    txtUsername.Focus()
                Else
                    txtPassword.Focus()
                End If

                Return
            End If

            btnLogin.Enabled = False
            btnExit.Enabled = False
            btnLogin.Text = "Signing in..."

            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Const sql As String =
                        "SELECT user_id, full_name, username, " &
                        "password_hash, role, is_active " &
                        "FROM users WHERE username = @username LIMIT 1"

                    Using cmd As New MySqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@username", username)

                        Using reader = cmd.ExecuteReader()
                            If Not reader.Read() Then
                                lblStatus.Text =
                                    "Invalid username or password."
                                Return
                            End If

                            If Not Convert.ToBoolean(reader("is_active")) Then
                                lblStatus.Text =
                                    "This account is inactive. Contact the administrator."
                                Return
                            End If

                            Dim storedHash As String =
                                Convert.ToString(reader("password_hash"))

                            If Not PasswordHasher.VerifyPassword(
                                password, storedHash) Then

                                lblStatus.Text =
                                    "Invalid username or password."
                                Return
                            End If

                            Dim role As String =
                                Convert.ToString(reader("role")).Trim().ToUpperInvariant()

                            If role <> "ADMIN" AndAlso
                               role <> "STAFF_ROOM" AndAlso
                               role <> "STAFF_PARKING" Then
                                lblStatus.Text =
                                    "This account role has not been assigned. Ask an administrator to edit this account and select Admin, Staff - Room, or Staff - Parking."
                                Return
                            End If

                            CurrentUser.UserId =
                                Convert.ToInt64(reader("user_id"))

                            CurrentUser.FullName =
                                Convert.ToString(reader("full_name"))

                            CurrentUser.Username =
                                Convert.ToString(reader("username"))

                            CurrentUser.Role = role
                        End Using
                    End Using
                End Using


                DialogResult = DialogResult.OK
                Close()
                Return

            Catch ex As Exception
                lblStatus.Text =
                    "Unable to sign in. Check the database connection."

                MessageBox.Show(
                    "Login failed: " & ex.Message,
                    "StayPoint HMS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)

            Finally
                If Not IsDisposed AndAlso Not Disposing Then
                    btnLogin.Enabled = True
                    btnExit.Enabled = True
                    btnLogin.Text = "Sign In  →"
                End If
            End Try
        End Sub

    End Class

    Friend NotInheritable Class CurrentUser

        Private Sub New()
        End Sub

        Public Shared Property UserId As Long
        Public Shared Property FullName As String = ""
        Public Shared Property Username As String = ""
        Public Shared Property Role As String = ""

    End Class

End Namespace
