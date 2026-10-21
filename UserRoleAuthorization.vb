Option Strict On
Option Explicit On
Option Infer On

Imports System

Namespace StayPointHMS

    Public NotInheritable Class UserRoleAuthorization
        Private Sub New()
        End Sub

        Public Const Admin As String = "ADMIN"
        Public Const StaffRoom As String = "STAFF_ROOM"
        Public Const StaffParking As String = "STAFF_PARKING"

        Public Shared Function IsAdmin() As Boolean
            Return String.Equals(CurrentUser.Role, Admin, StringComparison.OrdinalIgnoreCase)
        End Function

        Public Shared Function IsRoomStaff() As Boolean
            Return String.Equals(CurrentUser.Role, StaffRoom, StringComparison.OrdinalIgnoreCase)
        End Function

        Public Shared Function IsParkingStaff() As Boolean
            Return String.Equals(CurrentUser.Role, StaffParking, StringComparison.OrdinalIgnoreCase)
        End Function

        Public Shared Sub RequireRole(ParamArray allowedRoles As String())
            For Each role As String In allowedRoles
                If String.Equals(CurrentUser.Role, role, StringComparison.OrdinalIgnoreCase) Then
                    Return
                End If
            Next

            Throw New UnauthorizedAccessException(
                "Your account does not have permission to open this workspace.")
        End Sub
    End Class

End Namespace
