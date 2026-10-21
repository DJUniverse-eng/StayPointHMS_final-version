Imports MySqlConnector

Namespace StayPointHMS
    Friend NotInheritable Class DbConnectionFactory
        Private Sub New()
        End Sub

        ' Update credentials for your local XAMPP setup. Do not commit production secrets.
        Private Shared ReadOnly ConnectionString As String =
            "Server=127.0.0.1;Port=3306;Database=staypoint_hms;User ID=root;Password=;SslMode=None;"

        Public Shared Function Create() As MySqlConnection
            Return New MySqlConnection(ConnectionString)
        End Function
    End Class
End Namespace
