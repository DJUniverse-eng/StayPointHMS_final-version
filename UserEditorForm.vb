Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Drawing
Imports System.Windows.Forms

Namespace StayPointHMS

    Public NotInheritable Class UserEditorForm
        Inherits Form

        Private Shared ReadOnly Navy As Color =
            Color.FromArgb(23, 43, 77)

        Private Shared ReadOnly Teal As Color =
            Color.FromArgb(22, 160, 145)

        Private Shared ReadOnly BorderColor As Color =
            Color.FromArgb(225, 230, 237)

        Private Shared ReadOnly Muted As Color =
            Color.FromArgb(110, 120, 135)

        Private ReadOnly fullNameBox As New TextBox()
        Private ReadOnly usernameBox As New TextBox()
        Private ReadOnly passwordBox As New TextBox()
        Private ReadOnly roleBox As New ComboBox()
        Private ReadOnly activeBox As New CheckBox()

        Private ReadOnly isEdit As Boolean

        Public ReadOnly Property FullName As String
            Get
                Return fullNameBox.Text.Trim()
            End Get
        End Property

        Public ReadOnly Property Username As String
            Get
                Return usernameBox.Text.Trim()
            End Get
        End Property

        Public ReadOnly Property Password As String
            Get
                Return passwordBox.Text
            End Get
        End Property

        Public ReadOnly Property UserRole As String
            Get
                If roleBox.SelectedItem Is Nothing Then
                    Return ""
                End If

                Select Case roleBox.SelectedItem.ToString()
                    Case "Admin"
                        Return "ADMIN"
                    Case "Staff - Room"
                        Return "STAFF_ROOM"
                    Case "Staff - Parking"
                        Return "STAFF_PARKING"
                    Case Else
                        Return ""
                End Select
            End Get
        End Property

        Public ReadOnly Property IsActive As Boolean
            Get
                Return activeBox.Checked
            End Get
        End Property

        Public Sub New(
            Optional initialFullName As String = "",
            Optional initialUsername As String = "",
            Optional initialRole As String = "STAFF_ROOM",
            Optional initialActive As Boolean = True)

            isEdit = Not String.IsNullOrWhiteSpace(initialUsername)

            Text = If(isEdit, "Edit User Account", "Add User Account")
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            ShowInTaskbar = False
            BackColor = Color.White
            ForeColor = Navy
            Font = New Font("Segoe UI", 10)
            MinimumSize = New Size(420, 490)
            Size = New Size(460, 540)
            AutoScaleMode = AutoScaleMode.Dpi

            BuildInterface(
                initialFullName,
                initialUsername,
                initialRole,
                initialActive)
        End Sub

        Private Sub BuildInterface(
            initialFullName As String,
            initialUsername As String,
            initialRole As String,
            initialActive As Boolean)

            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(24),
                .ColumnCount = 1,
                .RowCount = 0,
                .BackColor = Color.White,
                .AutoScroll = True
            }

            root.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100))

            Dim heading As New Label With {
                .Text = If(isEdit, "Edit User Account", "Add User Account"),
                .Font = New Font("Segoe UI", 19, FontStyle.Bold),
                .ForeColor = Navy,
                .AutoSize = True,
                .Margin = New Padding(0, 0, 0, 4)
            }

            root.Controls.Add(heading)

            Dim description As New Label With {
                .Text = "Enter the account information below.",
                .Font = New Font("Segoe UI", 9.5F),
                .ForeColor = Muted,
                .AutoSize = True,
                .Margin = New Padding(0, 0, 0, 18)
            }

            root.Controls.Add(description)

            AddField(root, "Full Name", fullNameBox, initialFullName)
            AddField(root, "Username", usernameBox, initialUsername)
            AddField(root, "Password", passwordBox, "")

            passwordBox.UseSystemPasswordChar = True

            Dim passwordHint As New Label With {
                .Text = If(
                    isEdit,
                    "Leave blank to retain the current password.",
                    "Password must contain at least 8 characters."),
                .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = Muted,
                .AutoSize = True,
                .Margin = New Padding(0, -2, 0, 12)
            }

            root.Controls.Add(passwordHint)

            Dim roleLabel As New Label With {
                .Text = "Account Role",
                .AutoSize = True,
                .ForeColor = Navy,
                .Margin = New Padding(0, 4, 0, 5)
            }

            root.Controls.Add(roleLabel)

            With roleBox
                .DropDownStyle = ComboBoxStyle.DropDownList
                .Font = New Font("Segoe UI", 10)
                .Items.AddRange(New Object() {
                    "Admin", "Staff - Room", "Staff - Parking"})
                .Dock = DockStyle.Top
                .Margin = New Padding(0, 0, 0, 14)
            End With

            Select Case initialRole.Trim().ToUpperInvariant()
                Case "ADMIN"
                    roleBox.SelectedItem = "Admin"
                Case "STAFF_ROOM"
                    roleBox.SelectedItem = "Staff - Room"
                Case "STAFF_PARKING"
                    roleBox.SelectedItem = "Staff - Parking"
                Case "STAFF"
                    ' Legacy role: require the administrator to choose explicitly.
                    roleBox.SelectedIndex = -1
                Case Else
                    If Not isEdit Then roleBox.SelectedItem = "Staff - Room"
            End Select

            root.Controls.Add(roleBox)

            activeBox.Text = "Account is active"
            activeBox.Checked = initialActive
            activeBox.AutoSize = True
            activeBox.ForeColor = Navy
            activeBox.Margin = New Padding(0, 6, 0, 18)

            root.Controls.Add(activeBox)

            Dim footer As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .WrapContents = True,
                .FlowDirection = FlowDirection.RightToLeft,
                .Margin = New Padding(0, 8, 0, 0)
            }

            Dim saveButton As New Button With {
                .Text = "Save User",
                .Width = 110,
                .Height = 38,
                .BackColor = Teal,
                .ForeColor = Color.White,
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                .FlatStyle = FlatStyle.Flat,
                .UseVisualStyleBackColor = False,
                .Cursor = Cursors.Hand,
                .Margin = New Padding(8, 0, 0, 0)
            }

            saveButton.FlatAppearance.BorderSize = 0

            Dim cancelButton As New Button With {
                .Text = "Cancel",
                .Width = 95,
                .Height = 38,
                .BackColor = Color.FromArgb(235, 239, 244),
                .ForeColor = Navy,
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                .FlatStyle = FlatStyle.Flat,
                .DialogResult = DialogResult.Cancel,
                .UseVisualStyleBackColor = False,
                .Cursor = Cursors.Hand
            }

            cancelButton.FlatAppearance.BorderSize = 0

            AddHandler saveButton.Click, AddressOf SaveClicked

            footer.Controls.Add(saveButton)
            footer.Controls.Add(cancelButton)

            root.Controls.Add(footer)

            Controls.Add(root)

            AcceptButton = saveButton
            cancelButton = cancelButton
        End Sub

        Private Shared Sub AddField(
            layout As TableLayoutPanel,
            caption As String,
            field As TextBox,
            initialValue As String)

            Dim label As New Label With {
                .Text = caption,
                .AutoSize = True,
                .ForeColor = Navy,
                .Margin = New Padding(0, 4, 0, 5)
            }

            layout.Controls.Add(label)

            With field
                .Text = initialValue
                .Dock = DockStyle.Top
                .Font = New Font("Segoe UI", 10)
                .BorderStyle = BorderStyle.FixedSingle
                .Margin = New Padding(0, 0, 0, 12)
            End With

            layout.Controls.Add(field)
        End Sub

        Private Sub SaveClicked(sender As Object, e As EventArgs)
            If FullName.Length = 0 Then
                ShowValidation("Please enter the user's full name.")
                fullNameBox.Focus()
                Return
            End If

            If FullName.Length > 120 Then
                ShowValidation("Full name cannot exceed 120 characters.")
                fullNameBox.Focus()
                Return
            End If

            If Username.Length = 0 Then
                ShowValidation("Please enter a username.")
                usernameBox.Focus()
                Return
            End If

            If Username.Length > 50 Then
                ShowValidation("Username cannot exceed 50 characters.")
                usernameBox.Focus()
                Return
            End If

            If Username.Any(Function(character) Char.IsWhiteSpace(character)) Then
                ShowValidation("Username cannot contain spaces.")
                usernameBox.Focus()
                Return
            End If

            If Not isEdit OrElse Password.Length > 0 Then
                If Password.Length < 8 Then
                    ShowValidation(
                        "Password must contain at least 8 characters.")
                    passwordBox.Focus()
                    Return
                End If
            End If

            If UserRole <> "ADMIN" AndAlso
               UserRole <> "STAFF_ROOM" AndAlso
               UserRole <> "STAFF_PARKING" Then
                ShowValidation("Please select Admin, Staff - Room, or Staff - Parking.")
                roleBox.Focus()
                Return
            End If

            DialogResult = DialogResult.OK
            Close()
        End Sub

        Private Sub ShowValidation(message As String)
            MessageBox.Show(
                Me,
                message,
                "Check User Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)
        End Sub

    End Class

End Namespace