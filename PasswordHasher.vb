Imports System.Security.Cryptography

Namespace StayPointHMS
    Friend NotInheritable Class PasswordHasher
        Private Sub New()
        End Sub

        Private Const Iterations As Integer = 210000
        Private Const SaltBytes As Integer = 16
        Private Const KeyBytes As Integer = 32

        Public Shared Function HashPassword(password As String) As String
            If String.IsNullOrWhiteSpace(password) Then Throw New ArgumentException("Password is required.")
            Dim salt(SaltBytes - 1) As Byte
            RandomNumberGenerator.Fill(salt)
            Dim derived As Byte()
            Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256)
                derived = pbkdf2.GetBytes(KeyBytes)
            End Using
            Return $"PBKDF2-SHA256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(derived)}"
        End Function

        Public Shared Function VerifyPassword(password As String, storedHash As String) As Boolean
            If String.IsNullOrEmpty(password) OrElse String.IsNullOrEmpty(storedHash) Then Return False
            Dim parts = storedHash.Split("$"c)
            If parts.Length <> 4 OrElse parts(0) <> "PBKDF2-SHA256" Then Return False
            Dim iterationsValue As Integer
            If Not Integer.TryParse(parts(1), iterationsValue) OrElse iterationsValue < 100000 Then Return False
            Try
                Dim salt = Convert.FromBase64String(parts(2))
                Dim expected = Convert.FromBase64String(parts(3))
                Using pbkdf2 As New Rfc2898DeriveBytes(password, salt, iterationsValue, HashAlgorithmName.SHA256)
                    Dim actual = pbkdf2.GetBytes(expected.Length)
                    Return CryptographicOperations.FixedTimeEquals(actual, expected)
                End Using
            Catch ex As FormatException
                Return False
            End Try
        End Function
    End Class
End Namespace
