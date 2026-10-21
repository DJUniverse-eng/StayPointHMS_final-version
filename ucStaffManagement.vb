Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class ucStaffManagement
        Inherits UserControl

        Private Shared ReadOnly Navy As Color =
            Color.FromArgb(23, 43, 77)

        Private Shared ReadOnly SidebarColor As Color =
            Color.FromArgb(32, 58, 95)

        Private Shared ReadOnly Teal As Color =
            Color.FromArgb(22, 160, 145)

        Private Shared ReadOnly PageBg As Color =
            Color.FromArgb(245, 247, 250)

        Private Shared ReadOnly BorderColor As Color =
            Color.FromArgb(225, 230, 237)

        Private Shared ReadOnly Muted As Color =
            Color.FromArgb(110, 120, 135)

        Private ReadOnly searchBox As New TextBox()
        Private ReadOnly roleFilter As New ComboBox()
        Private ReadOnly usersGrid As New DataGridView()

        Private ReadOnly totalValue As New Label()
        Private ReadOnly activeValue As New Label()
        Private ReadOnly inactiveValue As New Label()

        Private ReadOnly addButton As New Button()
        Private ReadOnly editButton As New Button()
        Private ReadOnly statusButton As New Button()
        Private ReadOnly refreshButton As New Button()

        Private usersTable As DataTable

        Public Sub New()
            UserRoleAuthorization.RequireRole(UserRoleAuthorization.Admin)

            InitializeControl()
            LoadUsers()
        End Sub

        Private Sub InitializeControl()
            Dock = DockStyle.Fill
            BackColor = PageBg
            ForeColor = Navy
            Font = New Font("Segoe UI", 9.5F)
            MinimumSize = New Size(650, 450)
            AutoScaleMode = AutoScaleMode.Dpi

            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .Padding = New Padding(22),
                .ColumnCount = 1,
                .RowCount = 4,
                .BackColor = PageBg
            }

            root.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100))

            root.RowStyles.Add(
                New RowStyle(SizeType.AutoSize))

            root.RowStyles.Add(
                New RowStyle(SizeType.Absolute, 112))

            root.RowStyles.Add(
                New RowStyle(SizeType.AutoSize))

            root.RowStyles.Add(
                New RowStyle(SizeType.Percent, 100))

            ' Header
            Dim header As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 2,
                .AutoSize = True,
                .Margin = New Padding(0, 0, 0, 14)
            }

            header.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 100))

            Dim title As New Label With {
                .Text = "Staff Management",
                .Font = New Font("Segoe UI", 21, FontStyle.Bold),
                .ForeColor = Navy,
                .AutoSize = True,
                .Margin = New Padding(0, 0, 0, 4)
            }

            Dim subtitle As New Label With {
                .Text = "Manage user accounts, access roles, and account status.",
                .Font = New Font("Segoe UI", 9.5F),
                .ForeColor = Muted,
                .AutoSize = True,
                .Margin = New Padding(2, 0, 0, 0)
            }

            header.Controls.Add(title, 0, 0)
            header.Controls.Add(subtitle, 0, 1)

            root.Controls.Add(header, 0, 0)

            ' Summary cards
            Dim cards As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 3,
                .RowCount = 1,
                .Margin = New Padding(0, 0, 0, 14),
                .BackColor = PageBg
            }

            For i As Integer = 0 To 2
                cards.ColumnStyles.Add(
                    New ColumnStyle(SizeType.Percent, 33.3333F))
            Next

            cards.Controls.Add(
                CreateSummaryCard("Total Users", totalValue, Navy), 0, 0)

            cards.Controls.Add(
                CreateSummaryCard("Active Users", activeValue, Teal), 1, 0)

            cards.Controls.Add(
                CreateSummaryCard(
                    "Inactive Users",
                    inactiveValue,
                    Color.FromArgb(190, 105, 55)), 2, 0)

            root.Controls.Add(cards, 0, 1)

            ' Toolbar
            Dim toolbar As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .AutoSize = True,
                .WrapContents = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .BackColor = Color.White,
                .Padding = New Padding(10),
                .Margin = New Padding(0, 0, 0, 12)
            }

            searchBox.Width = 220
            searchBox.Height = 34
            searchBox.Font = New Font("Segoe UI", 10)
            searchBox.PlaceholderText = "Search name or username"
            searchBox.Margin = New Padding(0, 3, 8, 3)

            roleFilter.Width = 120
            roleFilter.DropDownStyle = ComboBoxStyle.DropDownList
            roleFilter.Font = New Font("Segoe UI", 9.5F)
            roleFilter.Margin = New Padding(0, 3, 12, 3)
            roleFilter.Items.AddRange(
                New Object() {
                    "All Roles", "Admin", "Staff - Room",
                    "Staff - Parking", "Unassigned (legacy)"})
            roleFilter.SelectedIndex = 0

            ConfigureButton(
                addButton, "+ Add User", Teal, Color.White, 105)

            ConfigureButton(
                editButton, "Edit User", Navy, Color.White, 95)

            ConfigureButton(
                statusButton,
                "Change Status",
                Color.FromArgb(90, 105, 125),
                Color.White,
                115)

            ConfigureButton(
                refreshButton,
                "Refresh",
                Color.FromArgb(235, 239, 244),
                Navy,
                85)

            toolbar.Controls.Add(searchBox)
            toolbar.Controls.Add(roleFilter)
            toolbar.Controls.Add(addButton)
            toolbar.Controls.Add(editButton)
            toolbar.Controls.Add(statusButton)
            toolbar.Controls.Add(refreshButton)

            AddHandler searchBox.TextChanged, AddressOf ApplyFilters
            AddHandler roleFilter.SelectedIndexChanged, AddressOf ApplyFilters
            AddHandler addButton.Click, AddressOf AddUser
            AddHandler editButton.Click, AddressOf EditUser
            AddHandler statusButton.Click, AddressOf ChangeStatus
            AddHandler refreshButton.Click,
                Sub(sender, e) LoadUsers()

            root.Controls.Add(toolbar, 0, 2)

            ' User table
            ConfigureUsersGrid()

            AddGridColumn("user_id", "User ID", 55)
            AddGridColumn("full_name", "Full Name", 150)
            AddGridColumn("username", "Username", 120)
            AddGridColumn("role", "Role", 115)
            AddGridColumn("status", "Account Status", 95)
            AddGridColumn(
                "created_at",
                "Date Created",
                125,
                "MM/dd/yyyy hh:mm tt")

            AddHandler usersGrid.CellDoubleClick,
                AddressOf UsersGrid_CellDoubleClick
            AddHandler usersGrid.CellFormatting,
                AddressOf UsersGrid_CellFormatting

            root.Controls.Add(usersGrid, 0, 3)

            Controls.Add(root)
        End Sub

        Private Shared Sub ConfigureButton(
            button As Button,
            caption As String,
            background As Color,
            foreground As Color,
            buttonWidth As Integer)

            With button
                .Text = caption
                .Width = buttonWidth
                .Height = 36
                .BackColor = background
                .ForeColor = foreground
                .FlatStyle = FlatStyle.Flat
                .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
                .Cursor = Cursors.Hand
                .Margin = New Padding(0, 1, 8, 1)
                .UseVisualStyleBackColor = False
            End With

            button.FlatAppearance.BorderSize = 0
        End Sub

        Private Function CreateSummaryCard(
            caption As String,
            valueLabel As Label,
            accent As Color) As Panel

            Dim card As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Margin = New Padding(0, 0, 12, 0),
                .Padding = New Padding(14)
            }

            Dim captionLabel As New Label With {
                .Text = caption,
                .Font = New Font("Segoe UI", 9.5F),
                .ForeColor = Muted,
                .AutoSize = True,
                .Location = New Point(14, 12)
            }

            valueLabel.Text = "0"
            valueLabel.Font = New Font("Segoe UI", 21, FontStyle.Bold)
            valueLabel.ForeColor = accent
            valueLabel.AutoSize = True
            valueLabel.Location = New Point(14, 38)

            card.Controls.Add(captionLabel)
            card.Controls.Add(valueLabel)

            AddHandler card.Paint,
                Sub(sender, e)
                    Using pen As New Pen(BorderColor)
                        e.Graphics.DrawRectangle(
                            pen,
                            0,
                            0,
                            Math.Max(0, card.Width - 1),
                            Math.Max(0, card.Height - 1))
                    End Using
                End Sub

            Return card
        End Function

        Private Sub ConfigureUsersGrid()
            With usersGrid
                .Dock = DockStyle.Fill
                .BackgroundColor = Color.White
                .BorderStyle = BorderStyle.None
                .AllowUserToAddRows = False
                .AllowUserToDeleteRows = False
                .AllowUserToResizeRows = False
                .ReadOnly = True
                .MultiSelect = False
                .SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect
                .AutoGenerateColumns = False
                .AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill
                .RowHeadersVisible = False
                .EnableHeadersVisualStyles = False
                .GridColor = BorderColor
                .Font = New Font("Segoe UI", 9.5F)
                .ColumnHeadersHeight = 42
                .ColumnHeadersDefaultCellStyle.BackColor = Navy
                .ColumnHeadersDefaultCellStyle.ForeColor = Color.White
                .ColumnHeadersDefaultCellStyle.Font =
                    New Font("Segoe UI", 9.5F, FontStyle.Bold)
                .DefaultCellStyle.ForeColor =
                    Color.FromArgb(45, 55, 72)
                .DefaultCellStyle.SelectionBackColor =
                    Color.FromArgb(220, 242, 239)
                .DefaultCellStyle.SelectionForeColor = Navy
                .DefaultCellStyle.Padding = New Padding(6, 0, 6, 0)
                .AlternatingRowsDefaultCellStyle.BackColor =
                    Color.FromArgb(248, 250, 252)
                .RowTemplate.Height = 38
            End With
        End Sub

        Private Sub AddGridColumn(
            name As String,
            caption As String,
            weight As Single,
            Optional format As String = Nothing)

            Dim column As New DataGridViewTextBoxColumn With {
                .Name = name,
                .HeaderText = caption,
                .DataPropertyName = name,
                .FillWeight = weight,
                .SortMode = DataGridViewColumnSortMode.Automatic
            }

            If Not String.IsNullOrWhiteSpace(format) Then
                column.DefaultCellStyle.Format = format
            End If

            usersGrid.Columns.Add(column)
        End Sub

        Private Sub UsersGrid_CellFormatting(
            sender As Object,
            e As DataGridViewCellFormattingEventArgs)

            If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
            If usersGrid.Columns(e.ColumnIndex).Name <> "role" Then Return
            If e.Value Is Nothing Then Return

            Select Case Convert.ToString(e.Value).Trim().ToUpperInvariant()
                Case "ADMIN"
                    e.Value = "Admin"
                Case "STAFF_ROOM"
                    e.Value = "Staff - Room"
                Case "STAFF_PARKING"
                    e.Value = "Staff - Parking"
                Case "STAFF"
                    e.Value = "Unassigned (legacy)"
                Case Else
                    Return
            End Select

            e.FormattingApplied = True
        End Sub

        Private Sub LoadUsers()
            Try
                Dim data As New DataTable()

                Using connection = DbConnectionFactory.Create()
                    connection.Open()

                    Const sql As String =
                        "SELECT user_id, full_name, username, role, " &
                        "is_active, " &
                        "CASE WHEN is_active = 1 THEN 'Active' " &
                        "ELSE 'Inactive' END AS status, created_at " &
                        "FROM users ORDER BY user_id DESC"

                    Using command As New MySqlCommand(sql, connection)
                        Using adapter As New MySqlDataAdapter(command)
                            adapter.Fill(data)
                        End Using
                    End Using
                End Using

                usersTable = data
                usersGrid.DataSource = usersTable.DefaultView

                Dim activeCount As Integer =
                    usersTable.AsEnumerable().
                    Count(
                        Function(row)
                            Return Not row.IsNull("is_active") AndAlso
                                Convert.ToInt32(row("is_active")) = 1
                        End Function)

                totalValue.Text = usersTable.Rows.Count.ToString()
                activeValue.Text = activeCount.ToString()
                inactiveValue.Text =
                    (usersTable.Rows.Count - activeCount).ToString()

                ApplyFilters(Nothing, EventArgs.Empty)

            Catch ex As Exception
                MessageBox.Show(
                    "Unable to load user accounts." &
                    Environment.NewLine & ex.Message,
                    "Staff Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub ApplyFilters(sender As Object, e As EventArgs)
            If usersTable Is Nothing Then Return

            Dim filters As New List(Of String)()
            Dim search As String = searchBox.Text.Trim()

            search = search.Replace("'", "''")
            search = search.Replace("[", "[[]")
            search = search.Replace("%", "[%]")
            search = search.Replace("*", "[*]")

            If search.Length > 0 Then
                filters.Add(
                    "(full_name LIKE '%" & search &
                    "%' OR username LIKE '%" & search & "%')")
            End If

            Dim selectedRoleLabel As String =
                Convert.ToString(roleFilter.SelectedItem)
            Dim selectedRole As String = ""

            Select Case selectedRoleLabel
                Case "Admin"
                    selectedRole = "ADMIN"
                Case "Staff - Room"
                    selectedRole = "STAFF_ROOM"
                Case "Staff - Parking"
                    selectedRole = "STAFF_PARKING"
                Case "Unassigned (legacy)"
                    selectedRole = "STAFF"
            End Select

            If selectedRole <> "" Then
                filters.Add("role = '" & selectedRole & "'")
            End If

            Try
                usersTable.DefaultView.RowFilter =
                    String.Join(" AND ", filters)

            Catch ex As EvaluateException
                usersTable.DefaultView.RowFilter = ""
            End Try
        End Sub

        Private Sub AddUser(sender As Object, e As EventArgs)
            Using editor As New UserEditorForm()
                If editor.ShowDialog(FindForm()) <> DialogResult.OK Then Return

                Try
                    Using connection = DbConnectionFactory.Create()
                        connection.Open()

                        Const sql As String =
                            "INSERT INTO users " &
                            "(full_name, username, password_hash, role, is_active) " &
                            "VALUES (@name, @username, @password, @role, @active)"

                        Using command As New MySqlCommand(sql, connection)
                            command.Parameters.AddWithValue(
                                "@name", editor.FullName)

                            command.Parameters.AddWithValue(
                                "@username", editor.Username)

                            command.Parameters.AddWithValue(
                                "@password",
                                PasswordHasher.HashPassword(editor.Password))

                            command.Parameters.AddWithValue(
                                "@role", editor.UserRole)

                            command.Parameters.AddWithValue(
                                "@active", If(editor.IsActive, 1, 0))

                            command.ExecuteNonQuery()
                        End Using
                    End Using

                    MessageBox.Show(
                        "User account created successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)

                    LoadUsers()

                Catch ex As MySqlException When ex.Number = 1062
                    MessageBox.Show(
                        "That username is already in use.",
                        "Duplicate Username",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)

                Catch ex As Exception
                    MessageBox.Show(
                        "Unable to create the user account." &
                        Environment.NewLine & ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
                End Try
            End Using
        End Sub

        Private Sub EditUser(sender As Object, e As EventArgs)
            If usersGrid.CurrentRow Is Nothing Then
                MessageBox.Show(
                    "Select a user first.",
                    "Staff Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
                Return
            End If

            Dim selectedRow As DataGridViewRow = usersGrid.CurrentRow

            Dim userId As Long =
                Convert.ToInt64(selectedRow.Cells("user_id").Value)

            Dim oldRole As String =
                Convert.ToString(selectedRow.Cells("role").Value)

            Dim fullName As String =
                Convert.ToString(selectedRow.Cells("full_name").Value)

            Dim username As String =
                Convert.ToString(selectedRow.Cells("username").Value)

            Dim isActive As Boolean =
                Convert.ToString(selectedRow.Cells("status").Value) = "Active"

            Using editor As New UserEditorForm(
                fullName,
                username,
                oldRole,
                isActive)

                If editor.ShowDialog(FindForm()) <> DialogResult.OK Then Return

                Try
                    Using connection = DbConnectionFactory.Create()
                        connection.Open()

                        Using transaction = connection.BeginTransaction()
                            Try
                                If oldRole = "ADMIN" AndAlso
                                    (editor.UserRole <> "ADMIN" OrElse
                                     Not editor.IsActive) Then

                                    EnsureAnotherActiveAdmin(
                                        connection, transaction, userId)
                                End If

                                Const updateSql As String =
                                    "UPDATE users SET " &
                                    "full_name = @name, " &
                                    "username = @username, " &
                                    "role = @role, " &
                                    "is_active = @active " &
                                    "WHERE user_id = @id"

                                Using command As New MySqlCommand(
                                    updateSql, connection, transaction)

                                    command.Parameters.AddWithValue(
                                        "@name", editor.FullName)

                                    command.Parameters.AddWithValue(
                                        "@username", editor.Username)

                                    command.Parameters.AddWithValue(
                                        "@role", editor.UserRole)

                                    command.Parameters.AddWithValue(
                                        "@active", If(editor.IsActive, 1, 0))

                                    command.Parameters.AddWithValue(
                                        "@id", userId)

                                    command.ExecuteNonQuery()
                                End Using

                                If editor.Password.Length > 0 Then
                                    Const passwordSql As String =
                                        "UPDATE users SET password_hash = @password " &
                                        "WHERE user_id = @id"

                                    Using command As New MySqlCommand(
                                        passwordSql, connection, transaction)

                                        command.Parameters.AddWithValue(
                                            "@password",
                                            PasswordHasher.HashPassword(
                                                editor.Password))

                                        command.Parameters.AddWithValue(
                                            "@id", userId)

                                        command.ExecuteNonQuery()
                                    End Using
                                End If

                                transaction.Commit()

                            Catch
                                Try
                                    transaction.Rollback()
                                Catch
                                    ' Keep the original error.
                                End Try

                                Throw
                            End Try
                        End Using
                    End Using

                    MessageBox.Show(
                        "User account updated successfully.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)

                    LoadUsers()

                Catch ex As MySqlException When ex.Number = 1062
                    MessageBox.Show(
                        "That username is already in use.",
                        "Duplicate Username",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)

                Catch ex As Exception
                    MessageBox.Show(
                        "Unable to update the account." &
                        Environment.NewLine & ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
                End Try
            End Using
        End Sub

        Private Shared Sub EnsureAnotherActiveAdmin(
            connection As MySqlConnection,
            transaction As MySqlTransaction,
            excludedUserId As Long)

            Const sql As String =
                "SELECT user_id FROM users " &
                "WHERE role = 'ADMIN' AND is_active = 1 " &
                "AND user_id <> @id FOR UPDATE"

            Using command As New MySqlCommand(sql, connection, transaction)
                command.Parameters.AddWithValue("@id", excludedUserId)

                Using reader = command.ExecuteReader()
                    If Not reader.Read() Then
                        Throw New InvalidOperationException(
                            "The last active administrator cannot be " &
                            "deactivated or changed to a staff role.")
                    End If
                End Using
            End Using
        End Sub

        Private Sub ChangeStatus(sender As Object, e As EventArgs)
            If usersGrid.CurrentRow Is Nothing Then
                MessageBox.Show(
                    "Select a user first.",
                    "Staff Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
                Return
            End If

            Dim selectedRow As DataGridViewRow = usersGrid.CurrentRow

            Dim userId As Long =
                Convert.ToInt64(selectedRow.Cells("user_id").Value)

            Dim role As String =
                Convert.ToString(selectedRow.Cells("role").Value)

            Dim fullName As String =
                Convert.ToString(selectedRow.Cells("full_name").Value)

            Dim currentlyActive As Boolean =
                Convert.ToString(selectedRow.Cells("status").Value) = "Active"

            Dim makeActive As Boolean = Not currentlyActive

            Dim actionText As String =
                If(makeActive, "activate", "deactivate")

            If MessageBox.Show(
                "Are you sure you want to " & actionText &
                " the account for " & fullName & "?",
                "Confirm Account Status",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) <> DialogResult.Yes Then
                Return
            End If

            Try
                Using connection = DbConnectionFactory.Create()
                    connection.Open()

                    Using transaction = connection.BeginTransaction()
                        Try
                            If Not makeActive AndAlso role = "ADMIN" Then
                                EnsureAnotherActiveAdmin(
                                    connection, transaction, userId)
                            End If

                            Const sql As String =
                                "UPDATE users SET is_active = @active " &
                                "WHERE user_id = @id"

                            Using command As New MySqlCommand(
                                sql, connection, transaction)

                                command.Parameters.AddWithValue(
                                    "@active", If(makeActive, 1, 0))

                                command.Parameters.AddWithValue(
                                    "@id", userId)

                                command.ExecuteNonQuery()
                            End Using

                            transaction.Commit()

                        Catch
                            Try
                                transaction.Rollback()
                            Catch
                                ' Keep the original error.
                            End Try

                            Throw
                        End Try
                    End Using
                End Using

                LoadUsers()

            Catch ex As Exception
                MessageBox.Show(
                    ex.Message,
                    "Unable to Change Status",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)
            End Try
        End Sub

        Private Sub UsersGrid_CellDoubleClick(
            sender As Object,
            e As DataGridViewCellEventArgs)

            If e.RowIndex >= 0 Then
                EditUser(Nothing, EventArgs.Empty)
            End If
        End Sub

    End Class

End Namespace