Imports System
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS

    Public NotInheritable Class ParkingManagementControl
        Inherits UserControl

        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)
        Private ReadOnly bg As Color = Color.FromArgb(240, 244, 248)
        Private ReadOnly muted As Color = Color.FromArgb(100, 116, 139)
        Private ReadOnly borderColor As Color = Color.FromArgb(225, 230, 237)

        Private ReadOnly txtSearch As New TextBox()
        Private ReadOnly cmbStatus As New ComboBox()
        Private ReadOnly cmbFloor As New ComboBox()
        Private ReadOnly cmbVehicle As New ComboBox()
        Private ReadOnly lblSummary As New Label()
        Private ReadOnly floorsPanel As New FlowLayoutPanel()
        Private ReadOnly toolbar As New Panel()

        Private ReadOnly btnAdd As New Button()
        Private ReadOnly btnPricing As New Button()
        Private ReadOnly btnRefresh As New Button()

        Private isLoading As Boolean

        Public Sub New()
            Dock = DockStyle.Fill
            BackColor = bg
            Font = New Font("Segoe UI", 9.5F)
            MinimumSize = New Size(500, 350)

            BuildInterface()
            LoadParkingSpaces()
        End Sub

        Private Sub BuildInterface()
            toolbar.Dock = DockStyle.Top
            toolbar.Height = 150
            toolbar.BackColor = bg
            toolbar.Padding = New Padding(20, 12, 20, 8)

            Dim title As New Label With {
                .Text = "Parking Management",
                .Font = New Font("Segoe UI", 20, FontStyle.Bold),
                .ForeColor = navy,
                .AutoSize = True,
                .Location = New Point(20, 12)
            }
            toolbar.Controls.Add(title)

            Dim subtitle As New Label With {
                .Text = "Manage parking spaces, vehicle categories, availability, and parking rates.",
                .Font = New Font("Segoe UI", 9.5F),
                .ForeColor = muted,
                .AutoSize = True,
                .Location = New Point(22, 48)
            }
            toolbar.Controls.Add(subtitle)

            ' Initial parking inventory is generated from System Setup.
            btnAdd.Visible = False

            ConfigureButton(btnPricing, "Parking Rates", Color.White, navy)
            btnPricing.Size = New Size(115, 36)
            btnPricing.FlatAppearance.BorderColor = borderColor
            AddHandler btnPricing.Click, AddressOf OpenPricing
            toolbar.Controls.Add(btnPricing)

            ConfigureButton(btnRefresh, "Refresh", Color.White, navy)
            btnRefresh.Size = New Size(85, 36)
            btnRefresh.FlatAppearance.BorderColor = borderColor
            AddHandler btnRefresh.Click, Sub(sender, e) LoadParkingSpaces()
            toolbar.Controls.Add(btnRefresh)

            Dim filterPanel As New FlowLayoutPanel With {
                .Location = New Point(20, 78),
                .Size = New Size(900, 35),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents = False,
                .AutoScroll = True,
                .BackColor = bg,
                .Margin = New Padding(0),
                .Padding = New Padding(0)
            }

            txtSearch.PlaceholderText = "Search code or area..."
            txtSearch.Size = New Size(220, 30)
            txtSearch.Margin = New Padding(0, 2, 8, 0)
            AddHandler txtSearch.TextChanged, AddressOf FilterChanged
            filterPanel.Controls.Add(txtSearch)

            ConfigureFilter(cmbStatus, 150)
            cmbStatus.Items.AddRange(New Object() {
                "All Statuses",
                "AVAILABLE",
                "OCCUPIED",
                "MAINTENANCE"
            })
            cmbStatus.SelectedIndex = 0
            AddHandler cmbStatus.SelectedIndexChanged, AddressOf FilterChanged
            filterPanel.Controls.Add(cmbStatus)

            ConfigureFilter(cmbFloor, 115)
            cmbFloor.Items.AddRange(New Object() {
                "All Floors",
                "Floor 1",
                "Floor 2",
                "Floor 3",
                "Floor 4",
                "Floor 5"
            })
            cmbFloor.SelectedIndex = 0
            AddHandler cmbFloor.SelectedIndexChanged, AddressOf FilterChanged
            filterPanel.Controls.Add(cmbFloor)

            ConfigureFilter(cmbVehicle, 145)
            cmbVehicle.Items.AddRange(New Object() {
                "All Vehicles",
                "MOTORCYCLE",
                "SEDAN",
                "VAN",
                "SUV"
            })
            cmbVehicle.SelectedIndex = 0
            AddHandler cmbVehicle.SelectedIndexChanged, AddressOf FilterChanged
            filterPanel.Controls.Add(cmbVehicle)

            toolbar.Controls.Add(filterPanel)

            lblSummary.Location = New Point(22, 120)
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
            AddHandler Me.Resize, AddressOf ControlResized

            ToolbarResized(Nothing, EventArgs.Empty)
        End Sub

        Private Sub ConfigureButton(
            button As Button,
            caption As String,
            backColor As Color,
            foreColor As Color)

            button.Text = caption
            button.BackColor = backColor
            button.ForeColor = foreColor
            button.FlatStyle = FlatStyle.Flat
            button.FlatAppearance.BorderSize =
                If(backColor = Color.White, 1, 0)
            button.Cursor = Cursors.Hand
            button.Font = New Font("Segoe UI", 9.0F)
        End Sub

        Private Sub ConfigureFilter(combo As ComboBox, width As Integer)
            combo.DropDownStyle = ComboBoxStyle.DropDownList
            combo.Size = New Size(width, 30)
            combo.Margin = New Padding(0, 2, 8, 0)
        End Sub

        Private Sub ToolbarResized(sender As Object, e As EventArgs)
            If toolbar.ClientSize.Width <= 0 Then Return

            Dim right As Integer = toolbar.ClientSize.Width - 20

            btnAdd.Location = New Point(
                Math.Max(20, right - btnAdd.Width), 12)

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
                Math.Max(280,
                    floorsPanel.ClientSize.Width -
                    floorsPanel.Padding.Horizontal - 8)

            For Each control As Control In floorsPanel.Controls
                Dim section As Panel = TryCast(control, Panel)
                If section Is Nothing Then Continue For

                section.Width = sectionWidth

                For Each child As Control In section.Controls
                    Dim cards As FlowLayoutPanel =
                        TryCast(child, FlowLayoutPanel)

                    If cards IsNot Nothing Then
                        cards.Width = Math.Max(260, sectionWidth - 8)
                        LayoutParkingCards(cards)
                        section.Height = cards.Bottom + 4
                    End If
                Next
            Next
        End Sub

        Private Sub LayoutParkingCards(cards As FlowLayoutPanel)
            If cards Is Nothing OrElse cards.IsDisposed Then Return

            Dim cardWidth As Integer = 245
            Dim gap As Integer = 12

            Dim usableWidth As Integer =
                Math.Max(cardWidth, cards.ClientSize.Width - 12)

            Dim columns As Integer =
                Math.Max(1, (usableWidth + gap) \ (cardWidth + gap))

            Dim actualWidth As Integer =
                Math.Max(210,
                    Math.Min(280,
                        (usableWidth - ((columns - 1) * gap)) \ columns))

            For Each control As Control In cards.Controls
                Dim card As Panel = TryCast(control, Panel)
                If card IsNot Nothing Then
                    card.Width = actualWidth
                End If
            Next
        End Sub

        Private Sub FilterChanged(sender As Object, e As EventArgs)
            If isLoading Then Return
            LoadParkingSpaces()
        End Sub

        Private Sub LoadParkingSpaces()
            If IsDisposed OrElse floorsPanel.IsDisposed Then Return

            isLoading = True
            floorsPanel.SuspendLayout()

            Try
                floorsPanel.Controls.Clear()

                Dim counts As New Dictionary(Of String, Integer) From {
                    {"AVAILABLE", 0},
                    {"OCCUPIED", 0},
                    {"MAINTENANCE", 0}
                }

                Dim totalInventory As Integer = 0
                Dim dt As New DataTable()

                Using conn = DbConnectionFactory.Create()
                    conn.Open()

                    ' Summary is independent of search and filters.
                    Using cmd As New MySqlCommand(
                        "SELECT status, COUNT(*) AS space_count " &
                        "FROM parking_spaces " &
                        "WHERE is_active=1 " &
                        "GROUP BY status", conn)

                        Using reader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim status As String =
                                    reader("status").ToString()

                                Dim count As Integer =
                                    Convert.ToInt32(reader("space_count"))

                                totalInventory += count

                                If counts.ContainsKey(status) Then
                                    counts(status) = count
                                End If
                            End While
                        End Using
                    End Using

                    Dim sql As String =
                        "SELECT p.parking_space_id, p.parking_code, " &
                        "p.floor_number, p.vehicle_category, p.area_name, " &
                        "p.status, " &
                        "r.hourly_rate, r.minimum_charge, " &
                        "r.billing_increment_minutes " &
                        "FROM parking_spaces p " &
                        "LEFT JOIN parking_rate_options r " &
                        "ON r.vehicle_category=p.vehicle_category " &
                        "AND r.is_active=1 " &
                        "WHERE p.is_active=1 " &
                        "AND (@search='' " &
                        "OR p.parking_code LIKE @pattern " &
                        "OR p.area_name LIKE @pattern) "

                    If cmbStatus.SelectedIndex > 0 Then
                        sql &= "AND p.status=@status "
                    End If

                    If cmbFloor.SelectedIndex > 0 Then
                        sql &= "AND p.floor_number=@floor "
                    End If

                    If cmbVehicle.SelectedIndex > 0 Then
                        sql &= "AND p.vehicle_category=@vehicle "
                    End If

                    sql &= "ORDER BY p.floor_number, p.parking_code"

                    Using cmd As New MySqlCommand(sql, conn)
                        Dim search As String = txtSearch.Text.Trim()

                        cmd.Parameters.AddWithValue("@search", search)
                        cmd.Parameters.AddWithValue(
                            "@pattern", "%" & search & "%")

                        If cmbStatus.SelectedIndex > 0 Then
                            cmd.Parameters.AddWithValue(
                                "@status", cmbStatus.SelectedItem.ToString())
                        End If

                        If cmbFloor.SelectedIndex > 0 Then
                            cmd.Parameters.AddWithValue(
                                "@floor", cmbFloor.SelectedIndex)
                        End If

                        If cmbVehicle.SelectedIndex > 0 Then
                            cmd.Parameters.AddWithValue(
                                "@vehicle", cmbVehicle.SelectedItem.ToString())
                        End If

                        Using adapter As New MySqlDataAdapter(cmd)
                            adapter.Fill(dt)
                        End Using
                    End Using
                End Using

                lblSummary.Text =
                    $"Total spaces: {totalInventory}   |   " &
                    $"Available: {counts("AVAILABLE")}   |   " &
                    $"Occupied: {counts("OCCUPIED")}   |   " &
                    $"Maintenance: {counts("MAINTENANCE")}"

                If dt.Rows.Count = 0 Then
                    ShowEmptyState()
                    Return
                End If

                Dim groups = dt.AsEnumerable().
                    GroupBy(Function(r) Convert.ToInt32(r("floor_number"))).
                    OrderBy(Function(g) g.Key)

                For Each group In groups
                    Dim sectionWidth As Integer =
                        Math.Max(280,
                            floorsPanel.ClientSize.Width -
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

                    Dim floorTitle As New Label With {
                        .Text = $"Floor {group.Key}",
                        .Font = New Font("Segoe UI", 13, FontStyle.Bold),
                        .ForeColor = navy,
                        .Location = New Point(2, 0),
                        .AutoSize = True
                    }
                    floorSection.Controls.Add(floorTitle)

                    Dim floorCount As New Label With {
                        .Text = $"{group.Count()} space(s)",
                        .Font = New Font("Segoe UI", 9.0F),
                        .ForeColor = muted,
                        .AutoSize = True,
                        .Location = New Point(105, 4)
                    }
                    floorSection.Controls.Add(floorCount)

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
                        cards.Controls.Add(CreateParkingCard(row))
                    Next

                    floorSection.Controls.Add(cards)
                    floorSection.Height = cards.Bottom + 4
                    floorsPanel.Controls.Add(floorSection)

                    LayoutParkingCards(cards)
                Next

                ResizeFloorSections()

            Catch ex As Exception
                floorsPanel.Controls.Clear()

                Dim errorLabel As New Label With {
                    .Text = "Unable to load parking spaces from the database." &
                            Environment.NewLine & ex.Message,
                    .ForeColor = Color.Firebrick,
                    .AutoSize = False,
                    .Dock = DockStyle.Top,
                    .Height = 100,
                    .Padding = New Padding(12)
                }

                floorsPanel.Controls.Add(errorLabel)

            Finally
                floorsPanel.ResumeLayout(True)
                isLoading = False
            End Try
        End Sub

        Private Function CreateParkingCard(row As DataRow) As Panel
            Dim parkingId As ULong =
                Convert.ToUInt64(row("parking_space_id"))

            Dim status As String = row("status").ToString()
            Dim code As String = row("parking_code").ToString()

            Dim statusColor As Color

            Select Case status
                Case "AVAILABLE"
                    statusColor = teal
                Case "OCCUPIED"
                    statusColor = Color.FromArgb(210, 75, 75)
                Case "MAINTENANCE"
                    statusColor = Color.FromArgb(205, 140, 25)
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
                .Tag = parkingId
            }

            Dim accent As New Panel With {
                .Dock = DockStyle.Left,
                .Width = 5,
                .BackColor = statusColor
            }
            card.Controls.Add(accent)

            Dim codeLabel As New Label With {
                .Text = code,
                .Font = New Font("Segoe UI", 18, FontStyle.Bold),
                .ForeColor = navy,
                .Location = New Point(16, 9),
                .AutoSize = True,
                .Cursor = Cursors.Hand
            }
            card.Controls.Add(codeLabel)

            Dim statusLabel As New Label With {
                .Text = status.Replace("_", " "),
                .Font = New Font("Segoe UI", 8.0F, FontStyle.Bold),
                .ForeColor = statusColor,
                .AutoSize = True,
                .Location = New Point(16, 42),
                .Cursor = Cursors.Hand
            }
            card.Controls.Add(statusLabel)

            Dim area As String = If(
                row.IsNull("area_name"),
                "No area specified",
                row("area_name").ToString())

            Dim details As New Label With {
                .Text = row("vehicle_category").ToString() &
                        Environment.NewLine &
                        area & "  |  Floor " &
                        row("floor_number").ToString(),
                .Font = New Font("Segoe UI", 8.8F),
                .ForeColor = muted,
                .Location = New Point(16, 65),
                .Size = New Size(210, 38),
                .AutoEllipsis = True,
                .Cursor = Cursors.Hand
            }
            card.Controls.Add(details)

            Dim rateText As String = "Rate not configured"

            If Not row.IsNull("hourly_rate") AndAlso
               Not row.IsNull("minimum_charge") AndAlso
               Not row.IsNull("billing_increment_minutes") Then

                Dim hourly As Decimal =
                    Convert.ToDecimal(row("hourly_rate"))

                Dim minimum As Decimal =
                    Convert.ToDecimal(row("minimum_charge"))

                Dim increment As Integer =
                    Convert.ToInt32(row("billing_increment_minutes"))

                rateText =
                    "₱" & hourly.ToString("N2") & "/hr" &
                    "  |  Min ₱" & minimum.ToString("N2") &
                    "  |  " & increment.ToString() & " min"
            End If

            Dim rateLabel As New Label With {
                .Text = rateText,
                .Font = New Font("Segoe UI", 8.2F, FontStyle.Bold),
                .ForeColor = If(
                    rateText = "Rate not configured", muted, navy),
                .Location = New Point(16, 119),
                .Size = New Size(210, 20),
                .AutoEllipsis = True,
                .Cursor = Cursors.Hand
            }
            card.Controls.Add(rateLabel)

            AddHandler card.Click,
                Sub(sender, e) EditSpace(parkingId)

            AddHandler codeLabel.Click,
                Sub(sender, e) EditSpace(parkingId)

            AddHandler statusLabel.Click,
                Sub(sender, e) EditSpace(parkingId)

            AddHandler details.Click,
                Sub(sender, e) EditSpace(parkingId)

            AddHandler rateLabel.Click,
                Sub(sender, e) EditSpace(parkingId)

            Return card
        End Function

        Private Sub ShowEmptyState()
            Dim message As String

            If String.IsNullOrWhiteSpace(txtSearch.Text) AndAlso
               cmbStatus.SelectedIndex = 0 AndAlso
               cmbFloor.SelectedIndex = 0 AndAlso
               cmbVehicle.SelectedIndex = 0 Then

                message = "No active parking spaces found." &
                          Environment.NewLine &
                          "Select Add Space to create a parking space."
            Else
                message = "No parking spaces match your search or filters." &
                          Environment.NewLine &
                          "Try changing the search text or filters."
            End If

            Dim empty As New Label With {
                .Text = message,
                .Font = New Font("Segoe UI", 12, FontStyle.Regular),
                .ForeColor = muted,
                .TextAlign = ContentAlignment.MiddleCenter,
                .Width = Math.Max(300, floorsPanel.ClientSize.Width - 50),
                .Height = 120,
                .Margin = New Padding(4, 16, 4, 0)
            }

            floorsPanel.Controls.Add(empty)
        End Sub

        Private Sub AddSpace(sender As Object, e As EventArgs)
            Try
                Using editor As New ParkingSpaceEditorForm()
                    If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                        LoadParkingSpaces()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show(
                    "Unable to open the parking space editor." &
                    Environment.NewLine & ex.Message,
                    "Parking Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub EditSpace(parkingId As ULong)
            Try
                Using editor As New ParkingSpaceEditorForm(parkingId)
                    If editor.ShowDialog(FindForm()) = DialogResult.OK Then
                        LoadParkingSpaces()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show(
                    "Unable to open the selected parking space." &
                    Environment.NewLine & ex.Message,
                    "Parking Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub OpenPricing(sender As Object, e As EventArgs)
            Try
                Using pricing As New ParkingPricingForm()
                    If pricing.ShowDialog(FindForm()) = DialogResult.OK Then
                        LoadParkingSpaces()
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show(
                    "Unable to open parking rates." &
                    Environment.NewLine & ex.Message,
                    "Parking Management",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)
            End Try
        End Sub

    End Class

End Namespace