Namespace Rosha
    Public Class CustomAttributes
        <System.AttributeUsage(System.AttributeTargets.Property)>
        Public Class IsIdentity
            Inherits System.Attribute
            'Property IsIdentity As Boolean
            Public Sub New()
                ' _IsIdentity = IsIdentity
            End Sub
        End Class
        <System.AttributeUsage(System.AttributeTargets.Property)>
        Public Class NotNULL
            Inherits System.Attribute
            'Property IsIdentity As Boolean
            Public Sub New()
                ' _IsIdentity = IsIdentity
            End Sub
        End Class
        <System.AttributeUsage(System.AttributeTargets.Property)>
        Public Class Ignore
            Inherits System.Attribute
            'Property IsIdentity As Boolean
            Public Sub New()
                ' _IsIdentity = IsIdentity
            End Sub
        End Class
        <System.AttributeUsage(System.AttributeTargets.Property)>
        Public Class Size
            Inherits System.Attribute

            Public Property Size As Integer

            Public Sub New(Size As Integer)
                _Size = Size
            End Sub
        End Class
        <System.AttributeUsage(System.AttributeTargets.Property)>
        Public Class SQLType
            Inherits System.Attribute

            Public Property Type As Framework.SQL.SQLDataTypes

            Public Sub New(Type As Framework.SQL.SQLDataTypes)
                _Type = Type
            End Sub
        End Class
        <System.AttributeUsage(System.AttributeTargets.Property)>
        Public Class FK
            Inherits System.Attribute

            Public Property FK As String

            Public Sub New(FK As String)
                _FK = FK
            End Sub
        End Class
        <System.AttributeUsage(System.AttributeTargets.Property)>
        Public Class PK
            Inherits System.Attribute
            Public Sub New()
            End Sub
        End Class
        <System.AttributeUsage(System.AttributeTargets.Property)>
        Public Class Joinable
            Inherits Attribute

            Property JonableInformation As List(Of Framework.SQL.JoinInformation)

            Public Sub New(JonableInformation As String)
                Dim Temp As New List(Of Framework.SQL.JoinInformation)
                For Each S As String In JonableInformation.Split(",")
                    Temp.Add(New Framework.SQL.JoinInformation(S.Split(" ")(0), S.Split(" ")(1), S.Split(" ")(2)))
                Next
                _JonableInformation = Temp
            End Sub
        End Class
    End Class
End Namespace

