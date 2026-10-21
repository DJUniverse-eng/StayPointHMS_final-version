Imports System
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class RoomManagementControl
        Inherits UserControl

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly bg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)

        Private ReadOnly txtSearch As New TextBox()
        Private ReadOnly cmbStatus As New ComboBox()
        Private ReadOnly cmbFloor As New ComboBox()
        Private ReadOnly lblSummary As New Label()
        Private ReadOnly floorsPanel As New FlowLayoutPanel()
        Private ReadOnly btnAdd As New Button()
        Private ReadOnly btnRefresh As New Button()
        Private ReadOnly btnPricing As New Button()
        Private ReadOnly toolbar As New Panel()

        Private isLoading As Boolean

        Public Sub New()
            Dock = DockStyle.Fill
            BackColor = bg
            Font = New Font("Segoe UI", 9.5F)
            MinimumSize = New Size(500, 350)

            BuildInterface()
            LoadRooms()
        End Sub

        Private Sub BuildInterface()
            toolbar.Dock = DockStyle.Top
            toolbar.Height = 145
            toolbar.BackColor = bg
            toolbar.Padding = New Padding(20, 12, 20, 8)

            toolbar.Controls.Add(New Label With {
                .Text = "Room Management",
                .Font = New Font("Segoe UI", 20, FontStyle.Bold),
                .ForeColor = navy,
                .AutoSize = True,
                .Location = New Point(20, 12)
            })

            toolbar.Controls.Add(New Label With {
                .Text = "Manage room inventory, operational status, and room rates.",
                .Font = New Font("Segoe UI", 9.5F),
                .ForeColor = muted,
                .AutoSize = True,
                .Location = New Point(22, 48)
            })

            ' Initial inventory is generated from System Setup, not manually here.
            btnAdd.Visible = False

            ConfigureButton(btnPricing, "Manage Pricing", Color.White, navy, 145)
            btnPricing.FlatAppearance.BorderColor = borderColor
            AddHandler btnPricing.Click, AddressOf ManagePricing
            toolbar.Controls.Add(btnPricing)

            ConfigureButton(btnRefresh, "Refresh", Color.White, navy, 85)
            btnRefresh.FlatAppearance.BorderColor = borderColor
            AddHandler btnRefresh.Click, Sub() LoadRooms()
            toolbar.Controls.Add(btnRefresh)

            Dim filterPanel As New FlowLayoutPanel With {
                .Location = New Point(20, 76),
                .Size = New Size(800, 38),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents = False,
                .AutoScroll = True,
                .BackColor = bg,
                .Margin = New Padding(0),
                .Padding = New Padding(0)
            }

            txtSearch.PlaceholderText = "Search room, type, or bed..."
            txtSearch.Size = New Size(270, 32)
            txtSearch.Margin = New Padding(0, 2, 8, 0)
            AddHandler txtSearch.TextChanged, AddressOf FilterChanged
            filterPanel.Controls.Add(txtSearch)

            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList
            cmbStatus.Size = New Size(175, 32)
            cmbStatus.Margin = New Padding(0, 2, 8, 0)
            cmbStatus.Items.AddRange(New Object() {
                "All Statuses",
                "AVAILABLE",
                "OCCUPIED",
                "CLEANING",
                "UNDER_MAINTENANCE"
            })
            cmbStatus.SelectedIndex = 0
            AddHandler cmbStatus.SelectedIndexChanged, AddressOf FilterChanged
            filterPanel.Controls.Add(cmbStatus)

            cmbFloor.DropDownStyle = ComboBoxStyle.DropDownList
            cmbFloor.Size = New Size(125, 32)
            cmbFloor.Margin = New Padding(0, 2, 8, 0)
            cmbFloor.Items.AddRange(New Object() {
                "All Floors", "Floor 1", "Floor 2", "Floor 3",
                "Floor 4", "Floor 5"
            })
            cmbFloor.SelectedIndex = 0
            AddHandler cmbFloor.SelectedIndexChanged, AddressOf FilterChanged
            filterPanel.Controls.Add(cmbFloor)

            toolbar.Controls.Add(filterPanel)

            lblSummary.Location = New Point(22, 119)
            lblSummary.Size = New Size(900, 20)
            lblSummary.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
            lblSummary.ForeColor = muted
            lblSummary.Font = New Font("Segoe UI", 9.0F)
            toolbar.Controls.Add(lblSummary)

            floorsPanel.Dock = DockStyle.Fill
            floorsPanel.AutoScroll = True
            floorsPanel.FlowDirection = FlowDirection.TopDown
            floorsPanel.WrapContents = False
            floorsPanel.BackColor = bg
            floorsPanel.Padding = New Padding(20, 12, 20, 16)
            floorsPanel.Margin = New Padding(0)

            Controls.Add(floorsPanel)
            Controls.Add(toolbar)

            AddHandler toolbar.Resize, AddressOf ToolbarResized
            AddHandler floorsPanel.Resize, AddressOf FloorsPanelResized
            AddHandler Resize, AddressOf ControlResized

            ToolbarResized(Nothing, EventArgs.Empty)
        End Sub

        Private Sub ConfigureButton(
            button As Button,
            caption As String,
            backColor As Color,
            foreColor As Color,
            width As Integer)

            button.Text = caption
            button.Size = New Size(width, 36)
            button.Anchor = AnchorStyles.Top Or AnchorStyles.Right
            button.BackColor = backColor
            button.ForeColor = foreColor
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderSize = If(backColor = teal, 0, 1)
            button.Cursor = Cursors.Hand
            button.Margin = New Padding(0)
        End Sub

        Private Sub ToolbarResized(sender As Object, e As EventArgs)
            If toolbar.ClientSize.Width <= 0 Then Return

            Dim rightEdge As Integer = toolbar.ClientSize.Width - 20

            btnAdd.Location = New Point(
                Math.Max(20, rightEdge - btnAdd.Width), 12)

            btnPricing.Location = New Point(
                Math.Max(20, btnAdd.Left - btnPricing.Width - 8), 12)

            btnRefresh.Location = New Point(
                Math.Max(20, btnPricing.Left - btnRefresh.Width - 8), 12)

            Dim availableWidth As Integer =
                Math.Max(300, toolbar.ClientSize.Width - 40)

            For Each control As Control In toolbar.Controls
                If TypeOf control Is FlowLayoutPanel Then
                    control.Width = availableWidth
                End If
            Next

            lblSummary.Width = availableWidth
        End Sub

        Private Sub ControlResized(sender As Object, e As EventArgs)
            If IsDisposed Then Return
            ResizeFloorSections()
        End Sub

        Private Sub FloorsPanelResized(sender As Object, e As EventArgs)
            If IsDisposed Then Return
            ResizeFloorSections()
        End Sub

        Private Sub ResizeFloorSections()
            If floorsPanel.IsDisposed Then Return

            Dim sectionWidth As Integer =
                Math.Max(300, floorsPanel.ClientSize.Width -
                         floorsPanel.Padding.Horizontal - 8)

            For Each control As Control In floorsPanel.Controls
                Dim section As Panel = TryCast(control, Panel)
                If section Is Nothing Then Continue For

                section.Width = sectionWidth

                For Each child As Control In section.Controls
                    If TypeOf child Is FlowLayoutPanel Then
                        child.Width = Math.Max(260, sectionWidth - 8)
                        LayoutRoomCards(DirectCast(child, FlowLayoutPanel))
                    End If
                Next
            Next
        End Sub

        Private Sub LayoutRoomCards(cards As FlowLayoutPanel)
            If cards Is Nothing OrElse cards.IsDisposed Then Return

            Dim baseWidth As Integer = 245
            Dim gap As Integer = 12
            Dim usableWidth As Integer =
                Math.Max(baseWidth, cards.ClientSize.Width - 12)

            Dim columns As Integer =
                Math.Max(1, (usableWidth + gap) \ (baseWidth + gap))

            Dim actualWidth As Integer =
                Math.Max(210, Math.Min(280,
                    (usableWidth - ((columns - 1) * gap)) \ columns))

            For Each control As Control In cards.Controls
                Dim card As Panel = TryCast(control, Panel)
                If card IsNot Nothing Then card.Width = actualWidth
            Next
        End Sub

        Private Sub FilterChanged(sender As Object, e As EventArgs)
            If isLoading Then Return
            LoadRooms()
        End Sub

        Private Sub LoadRooms()
            If IsDisposed OrElse floorsPanel.IsDisposed Then Return

            isLoading = True
            floorsPanel.SuspendLayout()

            Try
                floorsPanel.Controls.Clear()

                Dim allCounts As New System.Collections.Generic.Dictionary(Of String, Integer) From {
                    {"AVAILABLE", 0},
                    {"OCCUPIED", 0},
                    {"CLEANING", 0},
                    {"UNDER_MAINTENANCE", 0}
                }

                Dim totalInventory As Integer = 0
                Dim dt As New DataTable()

                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Using cmd As New MySqlCommand(
                        "SELECT r.status, COUNT(*) AS room_count " &
                        "FROM rooms r WHERE r.is_active=1 GROUP BY r.status", conn)

                        Using reader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim status As String = reader("status").ToString()
                                Dim count As Integer = Convert.ToInt32(reader("room_count"))

                                totalInventory += count

                                If allCounts.ContainsKey(status) Then
                                    allCounts(status) = count
                                End If
                            End While
                        End Using
                    End Using

                    Dim sql As String =
                        "SELECT r.room_id, r.room_number, r.floor_number, " &
                        "r.room_type_id, r.bed_code, r.capacity, r.status, " &
                        "rt.type_name, " &
                        "COALESCE(o.bed_name, CONCAT('Bed ', r.bed_code)) AS bed_name, " &
                        "o.hourly_rate, o.additional_hourly_rate, o.daily_rate, " &
                        "o.minimum_hours " &
                        "FROM rooms r " &
                        "INNER JOIN room_types rt ON rt.room_type_id=r.room_type_id " &
                        "LEFT JOIN room_type_options o " &
                        "ON o.room_type_id=r.room_type_id " &
                        "AND o.bed_code=r.bed_code AND o.is_active=1 " &
                        "WHERE r.is_active=1 " &
                        "AND (@search='' OR r.room_number LIKE @pattern " &
                        "OR rt.type_name LIKE @pattern OR o.bed_name LIKE @pattern) "

                    If cmbStatus.SelectedIndex > 0 Then
                        sql &= "AND r.status=@status "
                    End If

                    If cmbFloor.SelectedIndex > 0 Then
                        sql &= "AND r.floor_number=@floor "
                    End If

                    sql &= "ORDER BY r.floor_number, r.room_number"

                    Using cmd As New MySqlCommand(sql, conn)
                        Dim search As String = txtSearch.Text.Trim()

                        cmd.Parameters.AddWithValue("@search", search)
                        cmd.Parameters.AddWithValue("@pattern", "%" & search & "%")

                        If cmbStatus.SelectedIndex > 0 Then
                            cmd.Parameters.AddWithValue(
                                "@status", cmbStatus.SelectedItem.ToString())
                        End If

                        If cmbFloor.SelectedIndex > 0 Then
                            cmd.Parameters.AddWithValue(
                                "@floor", cmbFloor.SelectedIndex)
                        End If

                        Using adapter As New MySqlDataAdapter(cmd)
                            adapter.Fill(dt)
                        End Using
                    End Using
                End Using

                lblSummary.Text =
                    $"Total inventory: {totalInventory}  |  " &
                    $"Available: {allCounts("AVAILABLE")}  |  " &
                    $"Occupied: {allCounts("OCCUPIED")}  |  " &
                    $"Cleaning: {allCounts("CLEANING")}  |  " &
                    $"Maintenance: {allCounts("UNDER_MAINTENANCE")}"

                If dt.Rows.Count = 0 Then
                    ShowEmptyState()
                    Return
                End If

                Dim groups = dt.AsEnumerable().
                    GroupBy(Function(r) Convert.ToInt32(r("floor_number"))).
                    OrderBy(Function(g) g.Key)

                For Each group In groups
                    Dim sectionWidth As Integer =
                        Math.Max(300, floorsPanel.ClientSize.Width -
                                 floorsPanel.Padding.Horizontal - 8)

                    Dim floorSection As New Panel With {
                        .Width = sectionWidth,
                        .AutoSize = True,
                        .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        .MinimumSize = New Size(280, 0),
                        .BackColor = bg,
                        .Margin = New Padding(0, 0, 0, 16),
                        .Padding = New Padding(0)
                    }

                    floorSection.Controls.Add(New Label With {
                        .Text = $"Floor {group.Key}",
                        .Font = New Font("Segoe UI", 13, FontStyle.Bold),
                        .ForeColor = navy,
                        .Location = New Point(2, 0),
                        .AutoSize = True
                    })

                    floorSection.Controls.Add(New Label With {
                        .Text = $"{group.Count()} room(s)",
                        .Font = New Font("Segoe UI", 9.0F),
                        .ForeColor = muted,
                        .AutoSize = True,
                        .Location = New Point(105, 4)
                    })

                    Dim cards As New FlowLayoutPanel With {
                        .Location = New Point(0, 30),
                        .Width = Math.Max(260, sectionWidth - 8),
                        .AutoSize = True,
                        .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        .MinimumSize = New Size(260, 0),
                        .FlowDirection = FlowDirection.LeftToRight,
                        .WrapContents = True,
                        .AutoScroll = False,
                        .BackColor = bg,
                        .Margin = New Padding(0),
                        .Padding = New Padding(0, 4, 0, 4)
                    }

                    For Each row As DataRow In group
                        cards.Controls.Add(CreateRoomCard(row))
                    Next

                    floorSection.Controls.Add(cards)
                    floorSection.Height = cards.Bottom + 4
                    floorsPanel.Controls.Add(floorSection)

                    LayoutRoomCards(cards)
                Next

                ResizeFloorSections()

            Catch ex As Exception
                floorsPanel.Controls.Clear()

                floorsPanel.Controls.Add(New Label With {
                    .Text = "Unable to load rooms from the database." &
                            Environment.NewLine & ex.Message,
                    .ForeColor = Color.Firebrick,
                    .AutoSize = False,
                    .Dock = DockStyle.Top,
                    .Height = 100,
                    .Padding = New Padding(12)
                })

            Finally
                floorsPanel.ResumeLayout(True)
                isLoading = False
            End Try
        End Sub

        Private Sub ShowEmptyState()
            Dim message As String

            If String.IsNullOrWhiteSpace(txtSearch.Text) AndAlso
               cmbStatus.SelectedIndex = 0 AndAlso
               cmbFloor.SelectedIndex = 0 Then

                message = "No active rooms found." & Environment.NewLine &
                          "Select Add Room to create your first room."
            Else
                message = "No rooms match your search or filters." &
                          Environment.NewLine &
                          "Try changing the search text or filters."
            End If

            floorsPanel.Controls.Add(New Label With {
                .Text = message,
                .Font = New Font("Segoe UI", 12),
                .ForeColor = muted,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Width = Math.Max(300, floorsPanel.ClientSize.Width - 50),
                .Height = 120,
                .Margin = New Padding(4, 16, 4, 0)
            })
        End Sub

        Private Function CreateRoomCard(row As DataRow) As Panel
            Dim roomId As ULong = Convert.ToUInt64(row("room_id"))
            Dim status As String = row("status").ToString()

            Dim statusColor As Color
            Select Case status
                Case "AVAILABLE"
                    statusColor = teal
                Case "OCCUPIED"
                    statusColor = Color.FromArgb(210, 75, 75)
                Case "CLEANING"
                    statusColor = Color.FromArgb(205, 140, 25)
                Case "UNDER_MAINTENANCE"
                    statusColor = Color.FromArgb(110, 116, 130)
                Case Else
                    statusColor = muted
            End Select

            Dim card As New Panel With {
                .Size = New Size(245, 158),
                .MinimumSize = New Size(210, 158),
                .BackColor = Color.White,
                .Margin = New Padding(0, 0, 12, 12),
                .BorderStyle = BorderStyle.FixedSingle,
                .Cursor = Cursors.Hand,
                .Tag = roomId
            }

            card.Controls.Add(New Panel With {
                .Dock = DockStyle.Left,
                .Width = 5,
                .BackColor = statusColor
            })

            card.Controls.Add(New Label With {
                .Text = row("room_number").ToString(),
                .Font = New Font("Segoe UI", 18, FontStyle.Bold),
                .ForeColor = navy,
                .Location = New Point(16, 9),
                .AutoSize = True,
                .Cursor = Cursors.Hand,
                .Tag = roomId
            })

            card.Controls.Add(New Label With {
                .Text = status.Replace("_", " "),
                .Font = New Font("Segoe UI", 8.0F, FontStyle.Bold),
                .ForeColor = statusColor,
                .AutoSize = True,
                .Location = New Point(16, 42),
                .Cursor = Cursors.Hand,
                .Tag = roomId
            })

            card.Controls.Add(New Label With {
                .Text = row("type_name").ToString() & " · " &
                        row("bed_name").ToString() & Environment.NewLine &
                        "Floor " & row("floor_number").ToString() &
                        "  |  Capacity: " & row("capacity").ToString(),
                .Font = New Font("Segoe UI", 8.8F),
                .ForeColor = muted,
                .Location = New Point(16, 65),
                .Size = New Size(210, 38),
                .AutoEllipsis = True,
                .Cursor = Cursors.Hand,
                .Tag = roomId
            })

            Dim rateText As String = "Rates not configured"

            If Not row.IsNull("hourly_rate") AndAlso
               Not row.IsNull("daily_rate") Then

                rateText =
                    $"₱{Convert.ToDecimal(row("hourly_rate")):N2}/hr  ·  " &
                    $"₱{Convert.ToDecimal(row("daily_rate")):N2}/day"
            End If

            card.Controls.Add(New Label With {
                .Text = rateText,
                .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .ForeColor = If(rateText = "Rates not configured", muted, navy),
                .Location = New Point(16, 119),
                .Size = New Size(210, 20),
                .AutoEllipsis = True,
                .Cursor = Cursors.Hand,
                .Tag = roomId
            })

            AddHandler card.Click, Sub(sender, e) EditRoom(roomId)

            For Each child As Control In card.Controls
                AddHandler child.Click, AddressOf RoomCardChildClicked
            Next

            Return card
        End Function

        Private Sub RoomCardChildClicked(sender As Object, e As EventArgs)
            Dim control As Control = TryCast(sender, Control)
            If control Is Nothing OrElse control.Tag Is Nothing Then Return

            Dim id As ULong
            If ULong.TryParse(control.Tag.ToString(), id) Then
                EditRoom(id)
            End If
        End Sub

        Private Sub AddRoom(sender As Object, e As EventArgs)
            Try
                Using editor As New RoomEditorForm()
                    If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                        LoadRooms()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show(
                    "Unable to open the room editor." &
                    Environment.NewLine & ex.Message,
                    "Room Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub EditRoom(roomId As ULong)
            Try
                Using editor As New RoomEditorForm(roomId)
                    If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                        LoadRooms()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show(
                    "Unable to open the selected room." &
                    Environment.NewLine & ex.Message,
                    "Room Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub ManagePricing(sender As Object, e As EventArgs)
            Try
                Using editor As New RoomPricingForm()
                    If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                        LoadRooms()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show(
                    "Unable to open room pricing." &
                    Environment.NewLine & ex.Message,
                    "Room Pricing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

    End Class

    ' Pricing editor for room_type_options.
    ' This form is intentionally in the same source file as RoomManagementControl.
    Friend NotInheritable Class RoomPricingForm
        Inherits Form

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly bg As Color = Color.FromArgb(240, 244, 248)

        Private ReadOnly grid As New DataGridView()
        Private ReadOnly btnSave As New Button()
        Private ReadOnly btnCancel As New Button()
        Private ReadOnly lblMessage As New Label()

        Public Sub New()
            Text = "Room Pricing Management"
            StartPosition = FormStartPosition.CenterParent
            FormBorderStyle = FormBorderStyle.Sizable
            MinimumSize = New Size(850, 500)
            Size = New Size(1100, 650)
            BackColor = bg
            Font = New Font("Segoe UI", 9.5F)
            AutoScaleMode = AutoScaleMode.Dpi

            BuildInterface()
            LoadPricing()
        End Sub

        Private Sub BuildInterface()
            Dim root As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .Padding = New Padding(20),
                .BackColor = bg
            }

            root.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 68))
            root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            root.RowStyles.Add(New RowStyle(SizeType.Absolute, 66))

            Dim heading As New Panel With {.Dock = DockStyle.Fill}

            heading.Controls.Add(New Label With {
                .Text = "Room Pricing",
                .Font = New Font("Segoe UI", 19, FontStyle.Bold),
                .ForeColor = navy,
                .AutoSize = True,
                .Location = New Point(0, 0)
            })

            heading.Controls.Add(New Label With {
                .Text = "Edit hourly, additional-hourly, daily, and minimum-hour rates by room type and bed size.",
                .ForeColor = Color.FromArgb(100, 116, 139),
                .AutoSize = True,
                .Location = New Point(2, 36)
            })

            root.Controls.Add(heading, 0, 0)

            grid.Dock = DockStyle.Fill
            grid.BackgroundColor = Color.White
            grid.BorderStyle = BorderStyle.FixedSingle
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.AllowUserToOrderColumns = False
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            grid.RowHeadersVisible = False
            grid.SelectionMode = DataGridViewSelectionMode.CellSelect
            grid.MultiSelect = False
            grid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2
            grid.EnableHeadersVisualStyles = False
            grid.ColumnHeadersDefaultCellStyle.BackColor = navy
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
            grid.ColumnHeadersDefaultCellStyle.Font =
                New Font("Segoe UI", 9.0F, FontStyle.Bold)
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 240, 237)
            grid.DefaultCellStyle.SelectionForeColor = navy
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(247, 249, 252)

            root.Controls.Add(grid, 0, 1)

            Dim footer As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 1,
                .Padding = New Padding(0, 10, 0, 0)
            }

            footer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
            footer.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))

            lblMessage.Dock = DockStyle.Fill
            lblMessage.ForeColor = Color.Firebrick
            lblMessage.TextAlign = ContentAlignment.MiddleLeft
            footer.Controls.Add(lblMessage, 0, 0)

            Dim buttons As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False,
                .AutoSize = True
            }

            btnCancel.Text = "Cancel"
            btnCancel.Size = New Size(105, 36)
            btnCancel.DialogResult = DialogResult.Cancel
            btnCancel.FlatStyle = FlatStyle.Flat
            btnCancel.Margin = New Padding(8, 2, 0, 2)

            btnSave.Text = "Save Pricing"
            btnSave.Size = New Size(135, 36)
            btnSave.BackColor = teal
            btnSave.ForeColor = Color.White
            btnSave.FlatStyle = FlatStyle.Flat
            btnSave.FlatAppearance.BorderSize = 0
            btnSave.Font = New Font("Segoe UI", 9.5F, FontStyle.Bold)
            btnSave.Margin = New Padding(8, 2, 0, 2)
            AddHandler btnSave.Click, AddressOf SavePricing

            buttons.Controls.Add(btnCancel)
            buttons.Controls.Add(btnSave)
            footer.Controls.Add(buttons, 1, 0)

            root.Controls.Add(footer, 0, 2)
            Controls.Add(root)

            AcceptButton = btnSave
            CancelButton = btnCancel
        End Sub

        Private Sub LoadPricing()
            Dim dt As New DataTable()

            Using conn = DbConnectionFactory.Create()
                conn.Open()

                Using cmd As New MySqlCommand(
                    "SELECT o.room_type_id, rt.type_name, o.bed_code, " &
                    "o.bed_name, o.hourly_rate, o.additional_hourly_rate, " &
                    "o.daily_rate, o.minimum_hours, o.is_active " &
                    "FROM room_type_options o " &
                    "INNER JOIN room_types rt ON rt.room_type_id=o.room_type_id " &
                    "ORDER BY o.room_type_id, o.bed_code", conn)

                    Using adapter As New MySqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            grid.DataSource = dt

            grid.Columns("room_type_id").Visible = False
            grid.Columns("type_name").HeaderText = "Room Type"
            grid.Columns("bed_code").Visible = False
            grid.Columns("bed_name").HeaderText = "Bed Size"
            grid.Columns("hourly_rate").HeaderText = "Hourly Rate (₱)"
            grid.Columns("additional_hourly_rate").HeaderText = "Additional Hour (₱)"
            grid.Columns("daily_rate").HeaderText = "Daily Rate (₱)"
            grid.Columns("minimum_hours").HeaderText = "Minimum Hours"
            grid.Columns("is_active").HeaderText = "Active"

            grid.Columns("type_name").ReadOnly = True
            grid.Columns("bed_name").ReadOnly = True

            grid.Columns("hourly_rate").DefaultCellStyle.Format = "N2"
            grid.Columns("additional_hourly_rate").DefaultCellStyle.Format = "N2"
            grid.Columns("daily_rate").DefaultCellStyle.Format = "N2"

            grid.Columns("type_name").FillWeight = 115
            grid.Columns("bed_name").FillWeight = 80
            grid.Columns("hourly_rate").FillWeight = 90
            grid.Columns("additional_hourly_rate").FillWeight = 115
            grid.Columns("daily_rate").FillWeight = 90
            grid.Columns("minimum_hours").FillWeight = 85
            grid.Columns("is_active").FillWeight = 55

            If dt.Rows.Count = 0 Then
                lblMessage.Text = "No room pricing options were found."
                btnSave.Enabled = False
            End If
        End Sub

        Private Sub SavePricing(sender As Object, e As EventArgs)
            If Not grid.EndEdit() Then Return

            Dim manager = TryCast(grid.BindingContext(grid.DataSource), CurrencyManager)
            If manager IsNot Nothing Then manager.EndCurrentEdit()

            Try
                For Each row As DataGridViewRow In grid.Rows
                    If row.IsNewRow Then Continue For

                    Dim hourly As Decimal
                    Dim additional As Decimal
                    Dim daily As Decimal
                    Dim minimumHours As Integer

                    If Not Decimal.TryParse(
                        Convert.ToString(row.Cells("hourly_rate").Value), hourly) OrElse
                       Not Decimal.TryParse(
                        Convert.ToString(row.Cells("additional_hourly_rate").Value), additional) OrElse
                       Not Decimal.TryParse(
                        Convert.ToString(row.Cells("daily_rate").Value), daily) OrElse
                       Not Integer.TryParse(
                        Convert.ToString(row.Cells("minimum_hours").Value), minimumHours) Then

                        Throw New InvalidOperationException(
                            "Enter valid numeric values for every rate and minimum-hour field.")
                    End If

                    If hourly < 0 OrElse additional < 0 OrElse daily < 0 Then
                        Throw New InvalidOperationException(
                            "Rates cannot be negative.")
                    End If

                    If minimumHours < 1 OrElse minimumHours > 255 Then
                        Throw New InvalidOperationException(
                            "Minimum hours must be between 1 and 255.")
                    End If
                Next

                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    Using tx = conn.BeginTransaction()
                        For Each row As DataGridViewRow In grid.Rows
                            If row.IsNewRow Then Continue For

                            Using cmd As New MySqlCommand(
                                "UPDATE room_type_options SET " &
                                "hourly_rate=@hourly, " &
                                "additional_hourly_rate=@additional, " &
                                "daily_rate=@daily, minimum_hours=@minimum, " &
                                "is_active=@active " &
                                "WHERE room_type_id=@type AND bed_code=@bed", conn, tx)

                                cmd.Parameters.AddWithValue(
                                    "@hourly", Convert.ToDecimal(row.Cells("hourly_rate").Value))
                                cmd.Parameters.AddWithValue(
                                    "@additional", Convert.ToDecimal(row.Cells("additional_hourly_rate").Value))
                                cmd.Parameters.AddWithValue(
                                    "@daily", Convert.ToDecimal(row.Cells("daily_rate").Value))
                                cmd.Parameters.AddWithValue(
                                    "@minimum", Convert.ToInt32(row.Cells("minimum_hours").Value))
                                cmd.Parameters.AddWithValue(
                                    "@active", Convert.ToBoolean(row.Cells("is_active").Value))
                                cmd.Parameters.AddWithValue(
                                    "@type", Convert.ToInt32(row.Cells("room_type_id").Value))
                                cmd.Parameters.AddWithValue(
                                    "@bed", Convert.ToInt32(row.Cells("bed_code").Value))

                                cmd.ExecuteNonQuery()
                            End Using
                        Next

                        tx.Commit()
                    End Using
                End Using

                MessageBox.Show(
                    "Room pricing has been saved.",
                    "Room Pricing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information)

                DialogResult = DialogResult.OK
                Close()

            Catch ex As Exception
                lblMessage.Text = "Could not save pricing: " & ex.Message
            End Try
        End Sub

    End Class

End Namespace