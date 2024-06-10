Imports System.Data.SqlClient
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports RoshaORM.Rosha.CustomAttributes
Imports RoshaORM.Rosha.Framework
Imports RoshaORM.Rosha.Framework.SQL

Namespace Rosha
    Public Class Framework
        Class SQL
            Public Class Connection

                Property Server As String = "."
                Property DatabaseName As String = "RoshaDataBase"
                Property TrustedConnection As Boolean = True
                Property UserName As String = ""
                Property Password As String
                ReadOnly Property ConnectionString As String
                    Get
                        Return CreateConnectionString()
                    End Get
                End Property
                Property AutoInitialize As Boolean = False

                Private Function CreateConnectionString() As String
                    Dim Builder As New SqlConnectionStringBuilder()
                    Builder("Data Source") = _Server
                    Builder("Initial Catalog") = _DatabaseName
                    If _TrustedConnection Then
                        Builder("Trusted_Connection") = _TrustedConnection
                    Else
                        Builder("user id") = _UserName
                        Builder("password") = _Password
                    End If
                    Return Builder.ConnectionString
                End Function
                Shadows Function ToString() As String
                    Return CreateConnectionString()
                End Function

                Sub New()

                End Sub
                Sub New(DatabaseName As String)
                    _DatabaseName = DatabaseName
                End Sub
            End Class
            Public Enum SQLDataTypes
                NVarchar
                Varchar
                [Char]
                int
                BigInt
                FLOAT
                Bit
                DateTime
            End Enum
            Public Shared Function ConvertPropTypeToSQLType(Type As String) As SQLDataTypes
                Dim Temp As SQLDataTypes
                Select Case Type
                    Case "System.Int32"
                        Temp = SQLDataTypes.int
                    Case "System.String"
                        Temp = SQLDataTypes.NVarchar
                    Case "System.Boolean"
                        Temp = SQLDataTypes.Bit
                End Select
                Return Temp
            End Function
            Public Class JoinInformation
                Public Property TargetTable As String
                Public Property TargrgetKey As String
                Public Property ParrentKey As String

                Public Sub New(TargetTable As String, TargrgetKey As String, ParrentKey As String)
                    _TargetTable = TargetTable
                    _TargrgetKey = TargrgetKey
                    _ParrentKey = ParrentKey
                End Sub

            End Class
            Class Commands
                Shared SQLH As SQLHelper
                Class DataBase
                    Shared Sub CreateDataBase(Connection As Connection)
                        SQLH = New SQLHelper(Connection.ConnectionString.Replace(Connection.DatabaseName, "master"))
                        Dim Command As String = DataBase.CreateDataBaseQuery(Connection.DatabaseName)
                        SQLH.ExecNonQuery(Command)
                    End Sub
                    Shared Function CreateDataBaseQuery(DatabaseName As String)
                        Dim CreateDataBaseCommandTemp As String = "IF DB_ID('{0}') IS NULL CREATE database {0}"
                        Return String.Format(CreateDataBaseCommandTemp, DatabaseName)
                    End Function
                End Class
                Class Table
                    Shared Sub AddInsertProc(Connection As Connection, Entity As ORM.EntityModel)
                        Dim Command As String = My.Settings("CreateProcInsert")
                        Dim Columns As List(Of Column) = Entity.GetColumns
                        Dim Parameters As String = ""
                        Dim Values As String = ""

                        For Each Column As Column In Columns
                            If Not Column.Ignored And Not Column.ISIdentity Then
                                Parameters += Column.ToSpParameter + vbCrLf
                                Values += Column.ToSpValue

                            End If
                        Next
                        If Parameters.EndsWith(vbCrLf) Then Parameters = Parameters.Substring(0, Parameters.Length - 3)
                        If Values.EndsWith(",") Then Values = Values.Substring(0, Values.Length - 1)

                        Command = String.Format(Command, Entity.TableName + "_InsertProc", Parameters, Entity.TableName, Values)
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        SQLH.ExecNonQuery(Command)
                    End Sub
                    Shared Sub AddUpdateProc(Connection As Connection, Entity As ORM.EntityModel)
                        Dim Command As String = My.Settings("CreateProcUpdate")
                        Dim Columns As List(Of Column) = Entity.GetColumns
                        Dim Parameters As String = "@ID int," + vbCrLf
                        Dim Values As String = ""
                        For Each Column As Column In Columns
                            If Not Column.Ignored And Not Column.ISIdentity Then
                                Parameters += Column.ToSpParameter + vbCrLf
                                Values += Column.ToSpUpdateValue
                            End If
                        Next
                        If Parameters.EndsWith(vbCrLf) Then Parameters = Parameters.Substring(0, Parameters.Length - 3)
                        If Values.EndsWith(",") Then Values = Values.Substring(0, Values.Length - 1)
                        Command = String.Format(Command, Entity.TableName + "_UpdateProc", Parameters, Entity.TableName, Values)
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        SQLH.ExecNonQuery(Command)
                    End Sub
                    Shared Sub AddSelectProc(Connection As Connection, Entity As ORM.EntityModel)
                        Dim Command As String = My.Settings("CreateProcSelect")
                        Command = String.Format(Command, Entity.TableName + "_SelectProc", Entity.TableName)
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        SQLH.ExecNonQuery(Command)
                    End Sub
                    Shared Sub AddDeleteProc(Connection As Connection, Entity As ORM.EntityModel)
                        Dim Command As String = My.Settings("CreateProcDelete")
                        Command = String.Format(Command, Entity.TableName + "_DeleteProc", Entity.TableName)
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        SQLH.ExecNonQuery(Command)
                    End Sub
                    Shared Function CreateTableQuery(Entity As ORM.EntityModel) As String
                        Dim Name As String = Entity.TableName
                        Dim CreateTableCommandTemp As String = "if OBJECT_ID('{0}') is  null Begin CREATE TABLE {0} "
                        Dim Command As String = String.Format(CreateTableCommandTemp, Name) + "( "
                        Dim Columns As List(Of Column) = Entity.GetColumns
                        Columns.Sort(Function(x, y) y.ISIdentity.CompareTo(x.ISIdentity))
                        For Each Column As Column In Columns
                            If Not Column.Ignored Then Command += Column.CreateString() + ","
                        Next
                        If Command.EndsWith(",") Then Command = Command.Substring(0, Command.Length - 1)
                        Command += " ) Select 1 End Else Begin Select 0 End"
                        Return Command
                    End Function
                    Shared Sub CreateTable(Connection As Connection, Entity As ORM.EntityModel)
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        Dim Command As String = Table.CreateTableQuery(Entity)
                        If SQLH.ExecScalar(Command) Then
                            AddInsertProc(Connection, Entity)
                            AddUpdateProc(Connection, Entity)
                            AddSelectProc(Connection, Entity)
                            AddDeleteProc(Connection, Entity)
                            If Entity.HasDefaultValue Then
                                Insert(Connection, Entity)
                            End If
                        End If
                    End Sub
                    Shared Function Insert(Connection As Connection, Entity As ORM.EntityModel) As ORM.EntityModel
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        Dim Columns As List(Of Column) = Entity.GetColumns
                        Dim Params As New List(Of Object)
                        For Each Column As Column In Columns
                            If Not Column.Ignored And Not Column.ISIdentity Then
                                Params.Add("@" + Column.Name)
                                Params.Add(Column.Value)
                            End If
                        Next
                        Dim Temp As Integer = SQLH.ExecScalarProc(Entity.TableName + "_InsertProc", Params.ToArray)
                        Entity.ID = Temp
                        Return Entity
                    End Function
                    Shared Function Update(Connection As Connection, Entity As ORM.EntityModel) As ORM.EntityModel
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        Dim Columns As List(Of Column) = Entity.GetColumns
                        Columns.Sort(Function(x, y) y.ISIdentity.CompareTo(x.ISIdentity))
                        Dim Params As New List(Of Object)
                        For Each Column As Column In Columns
                            If Not Column.Ignored Then
                                Params.Add("@" + Column.Name)
                                Params.Add(Column.Value)
                            End If
                        Next
                        SQLH.ExecScalarProc(Entity.TableName + "_UpdateProc", Params.ToArray)
                        Return Entity
                    End Function
                    Shared Sub Delete(Connection As Connection, Entity As ORM.EntityModel)
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        SQLH.ExecNonQueryProc(Entity.TableName + "_DeleteProc", "@ID", Entity.ID.ToString)
                    End Sub
                    Shared Function [Select](Connection As Connection, Entity As ORM.EntityModel)
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        Dim DS As DataSet = SQLH.ExecDataSetProc(Entity.TableName + "_SelectProc")
                        Dim Items As Array = Array.CreateInstance(Entity.GetType, DS.Tables(0).Rows.Count)
                        If Not DS Is Nothing Then
                            For i As Integer = 0 To DS.Tables(0).Rows.Count - 1
                                Items(i) = ToObject(Connection, DS.Tables(0).Rows(i), Entity.GetType.GetConstructor(New System.Type() {}).Invoke(New Object() {}))
                            Next
                        End If
                        Dim List As Object = Activator.CreateInstance(GetType(List(Of)).MakeGenericType(Entity.GetType), New Object() {Items})
                        Return List
                    End Function
                    Shared Function [Select](Of T)(Connection As Connection, Entity As ORM.EntityModel, match As Predicate(Of T))
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        Dim DS As DataSet = SQLH.ExecDataSetProc(Entity.TableName + "_SelectProc")
                        Dim Items As Array = Array.CreateInstance(Entity.GetType, DS.Tables(0).Rows.Count)
                        If Not DS Is Nothing Then
                            For i As Integer = 0 To DS.Tables(0).Rows.Count - 1
                                Dim O = ToObject(Connection, DS.Tables(0).Rows(i), Entity.GetType.GetConstructor(New System.Type() {}).Invoke(New Object() {}))

                                Items(i) = O

                            Next
                        End If
                        Dim List As Object = Activator.CreateInstance(GetType(List(Of)).MakeGenericType(Entity.GetType), New Object() {Items})
                        Return CType(List, List(Of T)).FindAll(match)
                    End Function
                    Shared Function ToObject(Connection As Connection, DR As DataRow, Entity As ORM.EntityModel)
                        Dim Xobject As ORM.EntityModel = Entity
                        For Each Column As Column In Xobject.GetColumns
                            If Not Column.Ignored Then
                                Column.SetValue(Xobject, DR(Column.Name))
                            End If
                        Next
                        Dim Columns As List(Of Column) = Xobject.GetColumns.FindAll(Function(x) x.Joinable)
                        For Each Column As Column In Columns
                            Dim Target As DataSet = GetTargetDataBase(Connection, Xobject, Column.JoinableData)
                            Dim Trg = Column.Value.GetType.GetConstructor(New System.Type() {}).Invoke(New Object() {})
                            If Target.Tables.Count > 0 Then
                                For Each Data As DataRow In Target.Tables(0).Rows
                                    Trg.add(ToObject(Connection, Data, Column.Value.GetType.GenericTypeArguments(0).GetConstructor(New System.Type() {}).Invoke(New Object() {})))
                                Next

                            End If
                            Column.SetValue(Xobject, Trg)
                        Next
                        Return Xobject
                    End Function
                    Shared Function GetTargetDataBase(Connection As Connection, Entity As ORM.EntityModel, JoineQuery As String) As DataSet
                        Dim Output As New DataSet
                        Dim Temp As New List(Of JoinInformation)
                        For Each S As String In JoineQuery.Split(",")
                            Temp.Add(New JoinInformation(S.Split(" ")(0), S.Split(" ")(1), S.Split(" ")(2)))
                        Next

                        Dim Par As Integer = Entity.ID
                        For Each ji As JoinInformation In Temp

                            Output = GetData(Connection, ji.TargetTable.ToString, (ji.TargrgetKey + "=" + Par.ToString).ToString)
                            If Output.Tables(0).Rows.Count > 0 Then
                                Par = Output.Tables(0).Rows(0)(ji.ParrentKey)
                            Else
                                Dim DSS As New DataSet
                                DSS.Tables.Add("F")
                                Return (DSS)
                            End If


                        Next

                        Return Output
                    End Function
                    Shared Function GetData(Connection As Connection, TableName As String, Optional SelectQuery As String = "") As DataSet
                        Dim DS As DataSet
                        SQLH = New SQLHelper(Connection.ConnectionString)
                        Dim SQ As String = "Select * From " + TableName + " "
                        If SelectQuery <> "" Then
                            SQ += " where " + SelectQuery
                        End If
                        DS = SQLH.ExecDataSet(SQ)
                        Return DS
                    End Function
                    Shared Function GetJoinableFields(Type As Type) As List(Of PropertyInfo)
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
                End Class
                Public Class Column
                    Public Property Name As String
                    Public Property Type As SQLDataTypes
                    Public Property Size As Integer = 500
                    Public Property ISIdentity As Boolean = False
                    Public Property NotNULL As Boolean = False
                    Public Property PK As Boolean = False
                    Public Property FK As Boolean = False
                    Public Property FKField As String = ""
                    Public Property Value As Object
                    Public Property Ignored As Boolean = False
                    Public Property Joinable As Boolean = False
                    Public Property JoinableData As String = ""

                    Public Sub SetValue(ByRef Entity As ORM.EntityModel, Data As Object)
                        _Value = Value
                        For Each pr As PropertyInfo In Entity.GetType.GetProperties
                            If pr.Name = _Name Then
                                pr.SetValue(Entity, Data)
                            End If
                        Next
                    End Sub
                    Public Sub New(Name As String)
                        _Name = Name
                        _Type = Type
                    End Sub

                    Public Sub New(Name As String, Type As SQLDataTypes)
                        _Name = Name
                        _Type = Type
                    End Sub

                    Public Sub New(Name As String, Type As SQLDataTypes, Size As Integer)
                        _Name = Name
                        _Type = Type
                        _Size = Size
                    End Sub
                    Public Function ToSpParameter() As String
                        Dim Temp As String
                        If _Type <> SQLDataTypes.Bit And _Type <> SQLDataTypes.int Then
                            Temp = "@" + _Name + " " + _Type.ToString + " (" + _Size.ToString + ")" + " ,"
                        Else
                            Temp = "@" + _Name + " " + _Type.ToString + " ,"

                        End If
                        Return Temp
                    End Function
                    Public Function ToSpValue() As String
                        Dim Temp As String = "@" + Name + " ,"
                        Return Temp
                    End Function
                    Public Function ToSpUpdateValue() As String
                        Dim Temp As String = _Name + " = " + "@" + Name + " ,"
                        Return Temp
                    End Function
                    Public Function ToSpInsertValue() As String
                        Dim Temp As String = _Name + " = " + "@" + Name + " ,"
                        Return Temp
                    End Function
                    Public Function CreateString()
                        Dim Temp As String = " " + Name
                        Temp += " " + Type.ToString
                        If _Type <> SQLDataTypes.int And Type <> SQLDataTypes.Bit And Type <> SQLDataTypes.DateTime Then Temp += "(" + Size.ToString + ")"
                        If _PK Or _ISIdentity Then _NotNULL = True
                        If _NotNULL Then Temp += " NOT NULL "
                        If _ISIdentity Then Temp += " IDENTITY(1,1)  "
                        If _PK Then Temp += " PRIMARY KEY "
                        If _FK Then
                            Temp += " FOREIGN KEY REFERENCES  " + _FKField
                        End If
                        Return Temp
                    End Function
                    Public Sub New()

                    End Sub
                End Class

            End Class
        End Class
    End Class

End Namespace
