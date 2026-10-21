Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms
Imports MySqlConnector

Namespace StayPointHMS
    Public NotInheritable Class OperationsWorkspaceForm
        Inherits Form

        Private ReadOnly mode As String
        Private ReadOnly grid As New DataGridView()
        Private ReadOnly tabs As New TabControl()
        Private ReadOnly statusLabel As New Label()
        Private ReadOnly searchBox As New TextBox()
        Private ReadOnly navy As Color = Color.FromArgb(23, 43, 77)
        Private ReadOnly teal As Color = Color.FromArgb(22, 160, 145)

        Public Sub New(workspaceMode As String)
            mode = workspaceMode
            Text = If(mode = "ROOM", "StayPoint HMS — Room Operations", "StayPoint HMS — Parking Operations")
            StartPosition = FormStartPosition.CenterParent
            WindowState = FormWindowState.Maximized
            MinimumSize = New Size(1100, 700)
            BackColor = Color.FromArgb(240, 244, 248)
            Font = New Font("Segoe UI", 10.0F)

            Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 76, .BackColor = navy}
            Dim title As New Label With {
                .Text = If(mode = "ROOM", "▦  Room Operations", "▤  Parking Operations"),
                .Font = New Font("Segoe UI", 18.0F, FontStyle.Bold),
                .ForeColor = Color.White,
                .AutoSize = True,
                .Location = New Point(22, 22)
            }
            header.Controls.Add(title)
            Dim user As New Label With {.Text = CurrentUser.FullName, .ForeColor = Color.White, .AutoSize = True}
            header.Controls.Add(user)
            AddHandler header.Resize, Sub() user.Location = New Point(Math.Max(300, header.ClientSize.Width - user.Width - 24), 28)

            searchBox.PlaceholderText = "Search by code, plate, guest, or reference..."
            searchBox.SetBounds(24, 92, 440, 36)
            Dim search As New Button With {.Text = "⌕  Search", .Bounds = New Rectangle(475, 91, 125, 38), .BackColor = teal, .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat}
            AddHandler search.Click, Sub() LoadCurrentTab()

            tabs.SetBounds(24, 145, 1020, 500)
            tabs.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
            If mode = "ROOM" Then
                AddTab("Available Rooms", "rooms")
                AddTab("Active Stays", "stays")
                AddTab("Guest Search", "guests")
                AddTab("Guest Folios", "folios")
            Else
                AddTab("Parking Spaces", "parking_spaces")
                AddTab("Active Tickets", "active_tickets")
                AddTab("Ticket History", "ticket_history")
                AddTab("Departures", "departures")
            End If

            grid.Dock = DockStyle.Fill
            grid.ReadOnly = True
            grid.AllowUserToAddRows = False
            grid.AllowUserToDeleteRows = False
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            grid.BackgroundColor = Color.White
            grid.RowHeadersVisible = False
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
            grid.MultiSelect = False
            grid.AutoGenerateColumns = True

            For Each tab As TabPage In tabs.TabPages
                Dim tabGrid As New DataGridView With {
                    .Dock = DockStyle.Fill,
                    .ReadOnly = True,
                    .AllowUserToAddRows = False,
                    .AllowUserToDeleteRows = False,
                    .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    .BackgroundColor = Color.White,
                    .RowHeadersVisible = False,
                    .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    .MultiSelect = False,
                    .AutoGenerateColumns = True
}

                tab.Controls.Add(tabGrid)
            Next

            statusLabel.SetBounds(25, 650, 1000, 28)
            statusLabel.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
            statusLabel.ForeColor = Color.FromArgb(100, 116, 139)

            Controls.AddRange(New Control() {header, searchBox, search, tabs, statusLabel})
            AddHandler tabs.SelectedIndexChanged, Sub() LoadCurrentTab()
            AddHandler Shown, Sub() LoadCurrentTab()
        End Sub

        Private Sub AddTab(caption As String, key As String)
            Dim page As New TabPage(caption) With {.Tag = key}
            tabs.TabPages.Add(page)
        End Sub

        Private Sub LoadCurrentTab()
            If tabs.SelectedTab Is Nothing Then Return
            Dim key = Convert.ToString(tabs.SelectedTab.Tag)
            Dim filter = searchBox.Text.Trim()
            Dim sql As String = ""
            Select Case key
                Case "rooms"
                    sql = "SELECT r.room_id, r.room_number AS RoomCode, r.floor_number AS Floor, rt.type_name AS RoomType, " &
                          "r.capacity AS Capacity, r.status AS Status, r.description AS Description " &
                          "FROM rooms r JOIN room_types rt ON rt.room_type_id=r.room_type_id " &
                          "WHERE r.is_active=1 AND r.status='AVAILABLE' AND (@q='' OR r.room_number LIKE @like OR rt.type_name LIKE @like) ORDER BY r.floor_number,r.room_number"
                Case "stays"
                    sql = "SELECT s.stay_id, s.stay_reference AS StayReference, CONCAT(g.first_name,' ',g.last_name) AS Guest, " &
                          "g.phone AS Phone, s.checked_in_at AS CheckIn, s.expected_checkout_at AS ExpectedCheckout, s.stay_status AS Status " &
                          "FROM stays s JOIN guests g ON g.guest_id=s.guest_id WHERE s.stay_status='ACTIVE' " &
                          "AND (@q='' OR s.stay_reference LIKE @like OR g.first_name LIKE @like OR g.last_name LIKE @like OR g.phone LIKE @like) ORDER BY s.checked_in_at DESC"
                Case "guests"
                    sql = "SELECT guest_id, first_name AS FirstName, last_name AS LastName, phone AS Phone, email AS Email, address AS Address " &
                          "FROM guests WHERE (@q='' OR first_name LIKE @like OR last_name LIKE @like OR phone LIKE @like OR email LIKE @like) ORDER BY last_name,first_name"
                Case "folios"
                    sql = "SELECT f.folio_id, s.stay_reference AS StayReference, CONCAT(g.first_name,' ',g.last_name) AS Guest, f.folio_status AS Status, " &
                          "COALESCE((SELECT SUM(fc.amount) FROM folio_charges fc WHERE fc.folio_id=f.folio_id),0) AS Charges, " &
                          "COALESCE((SELECT SUM(p.amount) FROM payments p WHERE p.folio_id=f.folio_id AND p.payment_status='COMPLETED'),0) - " &
                          "COALESCE((SELECT SUM(rf.amount) FROM refunds rf JOIN payments p2 ON p2.payment_id=rf.payment_id WHERE p2.folio_id=f.folio_id AND rf.refund_status='COMPLETED'),0) AS NetPayments " &
                          "FROM guest_folios f JOIN stays s ON s.stay_id=f.stay_id JOIN guests g ON g.guest_id=s.guest_id " &
                          "WHERE (@q='' OR s.stay_reference LIKE @like OR g.first_name LIKE @like OR g.last_name LIKE @like) ORDER BY f.opened_at DESC"
                Case "parking_spaces"
                    sql = "SELECT parking_space_id, parking_code AS ParkingCode, floor_number AS Floor, vehicle_category AS VehicleCategory, area_name AS Area, status AS Status " &
                          "FROM parking_spaces WHERE is_active=1 AND (@q='' OR parking_code LIKE @like OR vehicle_category LIKE @like OR area_name LIKE @like) ORDER BY floor_number,parking_code"
                Case "active_tickets"
                    sql = "SELECT t.parking_ticket_id, t.ticket_reference AS Ticket, t.parking_space_id, ps.parking_code AS Space, t.vehicle_category AS VehicleCategory, " &
                          "t.plate_number AS Plate, t.entered_at AS EntryTime, t.guest_id, t.stay_id " &
                          "FROM parking_tickets t JOIN parking_spaces ps ON ps.parking_space_id=t.parking_space_id " &
                          "WHERE t.ticket_status='ACTIVE' AND (@q='' OR t.ticket_reference LIKE @like OR t.plate_number LIKE @like OR ps.parking_code LIKE @like) ORDER BY t.entered_at DESC"
                Case "ticket_history", "departures"
                    sql = "SELECT t.ticket_reference AS Ticket, ps.parking_code AS Space, t.vehicle_category AS VehicleCategory, t.plate_number AS Plate, " &
                          "t.entered_at AS EntryTime, t.departed_at AS DepartureTime, t.ticket_status AS Status, t.folio_charge_id AS FolioCharge " &
                          "FROM parking_tickets t JOIN parking_spaces ps ON ps.parking_space_id=t.parking_space_id " &
                          "WHERE (@q='' OR t.ticket_reference LIKE @like OR t.plate_number LIKE @like OR ps.parking_code LIKE @like) " &
                          If(key = "departures", "AND t.ticket_status='DEPARTED' AND DATE(t.departed_at)=CURDATE() ", "AND t.ticket_status<>'ACTIVE' ") &
                          "ORDER BY t.departed_at DESC"
            End Select

            Try
                Using conn = DbConnectionFactory.Create()
                    conn.Open()
                    Using cmd As New MySqlCommand(sql, conn)
                        cmd.Parameters.AddWithValue("@q", filter)
                        cmd.Parameters.AddWithValue("@like", "%" & filter & "%")
                        Using adapter As New MySqlDataAdapter(cmd)
                            Dim dt As New DataTable()
                            adapter.Fill(dt)
                            Dim target = TryCast(tabs.SelectedTab.Controls(0), DataGridView)
                            If target IsNot Nothing Then target.DataSource = dt
                        End Using
                    End Using
                End Using
                statusLabel.Text = "Loaded " & key.Replace("_", " ") & ". Read-only listing; actions that change records are implemented in the next workflow pass."
            Catch ex As Exception
                statusLabel.Text = "Database error: " & ex.Message
            End Try
        End Sub
    End Class
End Namespace
