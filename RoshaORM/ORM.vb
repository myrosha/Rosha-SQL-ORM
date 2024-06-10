Imports System.Data.SqlClient
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports RoshaORM.Rosha.CustomAttributes
Imports RoshaORM.Rosha.Framework.SQL
Imports RoshaORM.Rosha.Framework.SQL.Commands

Namespace Rosha
    Public Class ORM
        Class Events
            Class SyncStartEvent
                Property StartDateTime As DateTime = Now
                Property Objects As New List(Of EntityModel)
                Sub New()

                End Sub
            End Class
        End Class
        Private Function GetJoinableFields() As List(Of PropertyInfo)
            Dim Type = Me.GetType
            Dim Items As New List(Of PropertyInfo)
            If Not Type Is Nothing Then
                For Each p As PropertyInfo In Type.GetProperties

                    For Each C As CustomAttributeData In p.CustomAttributes
                        If C.AttributeType.Name.ToLower = "joinable" Then
                            Items.Add(p)
                        End If
                    Next
                Next
            End If
            Return Items
        End Function

        Shared _Connection As Connection
        Event SyncStart(sender As Object, ByRef e As Events.SyncStartEvent)
        Sub New(Connection As Connection)
            _Connection = Connection
        End Sub
        Sub StartSync()
            Dim SE As New Events.SyncStartEvent
            RaiseEvent SyncStart(Me, SE)
            Framework.SQL.Commands.DataBase.CreateDataBase(_Connection)
            For Each [Object] In SE.Objects
                Framework.SQL.Commands.Table.CreateTable(_Connection, [Object])
            Next
        End Sub

        Public MustInherit Class EntityModel
            Private _TableName As String
            <IsIdentity> <Size(200)> <PK> <SQLType(SQLDataTypes.int)>
            Property ID As Integer
            <Ignore>
            Property TableName As String
                Get
                    Return _TableName
                End Get
                Set
                    _TableName = Value
                    IIf(Value = "", _TableName = Me.GetType.Name, _TableName = Value)
                End Set
            End Property
            <Ignore>
            Property HasDefaultValue As Boolean
            Public Function GetData()
                Return Table.Select(_Connection, Me)
            End Function
            Public Function GetData(Of T)(match As Predicate(Of T))
                Return Table.Select(Of T)(_Connection, Me, match)
            End Function
            Public Sub Delete()
                Table.Delete(_Connection, Me)
            End Sub
            Public Function Save() As EntityModel
                Dim En As EntityModel
                If _ID = 0 Then
                    En = Framework.SQL.Commands.Table.Insert(_Connection, Me)
                Else
                    En = Framework.SQL.Commands.Table.Update(_Connection, Me)
                End If

                Return En
            End Function
            Sub New()
                _TableName = Me.GetType.Name
            End Sub
            Public Function GetColumns() As List(Of Column)
                Dim model As EntityModel = Me
                Dim Columns As New List(Of Column)

                Dim Properties() As PropertyInfo = Model.GetType.GetProperties
                For Each p As PropertyInfo In Properties
                    Dim C As New Column(p.Name)
                    C.Type = ConvertPropTypeToSQLType(p.PropertyType.ToString)

                    For Each CA As CustomAttributeData In p.CustomAttributes
                        Select Case CA.AttributeType.Name.ToLower
                            Case "IsIdentity".ToLower
                                C.ISIdentity = True
                                C.NotNULL = True
                            Case "size"
                                C.Size = CA.ConstructorArguments(0).Value
                            Case "notnull"
                                C.NotNULL = True
                            Case "pk"
                                C.PK = True
                            Case "sqltype"
                                C.Type = CA.ConstructorArguments(0).Value
                            Case "ignore"
                                C.Ignored = True
                            Case "joinable"
                                C.Joinable = True
                                C.JoinableData = CA.ConstructorArguments(0).Value
                            Case Else
                                Exit Select
                        End Select

                    Next

                    Try
                        C.Value = p.GetValue(Model)
                    Catch ex As Exception

                    End Try

                    Columns.Add(C)
                Next
                Return Columns
            End Function
        End Class

    End Class

End Namespace

