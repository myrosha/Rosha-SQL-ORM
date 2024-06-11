# Rosha SQL ORM
<p align="center"><img width="400" src="https://myrosha.ir/wp-content/uploads/2024/04/Logo.png" alt="Image"></p>

** An Advanced SQL ORM **

[![](https://img.shields.io/github/v/release/myrosha/Rosha-SQL-ORM.svg)](https://github.com/myrosha/Rosha-SQL-ORM/releases)
[![Downloads](https://img.shields.io/github/downloads/myrosha/Rosha-SQL-ORM/total.svg)](#)
[![License](https://img.shields.io/badge/license-GPL%20V3-blue.svg?longCache=true)](https://www.gnu.org/licenses/gpl-3.0.en.html)

> **Disclaimer:** This project is only for personal learning and communication, please do not use it for illegal purposes, please do not use it in a production environment

## .NET Version
.Net Framework 4.8

## Install
Just Download and Add to Project

##How to Use

1- Add Refrence To Project

2- Import Library

```
Imports RoshaORM.Rosha
```

3- Create an Instance of ORM 

```
Dim ORM As ORM
```

4- Add Sync Method to Form,Page or Class

```
   Public Sub SyncStart(sender As Object, ByRef e As ORM.Events.SyncStartEvent)
   End Sub
```

5- Add Event Handle to Load Form or Page or Component

```
   AddHandler ORM.SyncStart, AddressOf SyncStart
```

6- Create SQL Connection Instance

  ```
   Dim SQLConnetction As New Framework.SQL.Connection
  ```

 If SQl Server Instaled Standard and Windows Auth
 If SQl Server Instaled Standard and Windows AuthJust Use This Code

```
 Dim SQLConnetction As New Framework.SQL.Connection(%DataBase Name%)
```
* Database Name : The name of the database you want to create in SQL Server for the project

 Else complete Other Properties of SQLConnection

 ```
 Dim SQLConnetction As New Framework.SQL.Connection(%DataBase Name%)
 SQLConnetction.TrustedConnection = False
 SQLConnetction.Server = "%ServerAddress%"
 SQLConnetction.UserName = "%SQLUserName%"
 SQLConnetction.Password = "%SQLPassword%"
 ```
* Database Name : The name of the database you want to create in SQL Server for the project
* TrustedConnection : Set to False if Use SQl Auth (Boolean)
* ServerAddress : SQL Server Address and SQL Instance
* SQLUserName : SQL Server UserName
* SQLPassword : SQL Server Password

7- Set ORM Instance

```
 ORM = New ORM(SQLConnetction)
```

8- Create Objects Model

```
  Public Class Users
     Inherits EntityModel
     Property UserName As String
     Property Password As String
     Property Description As String
     Property Enable As Boolean
     Property Role As New List(Of Roles)

     Sub New()

     End Sub
 End Class
 Public Class Roles
     Inherits EntityModel
     Property Name As String = ""
     Property Description As String
     Sub New()

     End Sub
 End Class
 Public Class User_Role
     Property UserID As Integer = 0
     Property RoleID As Integer = 0
     Sub New()

     End Sub
 End Class
```
* There is no need to create an ID. ID is created automatically in the model.
* Add Inherits EntityModel to All Models

9- Add Attributes to Model Fields

## Attributes

* IsIdentity : User For Create Identity Field
* NotNULL : Set NotNULL Field
* Ignore : Ignore Field And Don't Create in Database
* Size : Set Fiels Size (Integer)
* SQLType : Set Field Type
* PK : Set Field as Primary Key
* Joinable : Set Field With Multi Join Tables (String) : Sample <Joinable("%JoinTo% %TargetKey% %ParentKey%")>

  - Split Join Data With ,
  - JoinTo : Table Name of Tables For Join to it
  - TargetKey : Condination of Joine Table : Default Check with Parrent.ID
  - ParentKey : Return Field of JoinTo

```
Imports RoshaORM.Rosha.CustomAttributes
Imports RoshaORM.Rosha.Framework.SQL
```

```
 Public Class Users
     Inherits EntityModel
     <SQLType(SQLDataTypes.Varchar)> <Size(50)> <NotNULL>
     Property UserName As String
     <SQLType(SQLDataTypes.Varchar)> <Size(100)> <NotNULL>
     Property Password As String
     <SQLType(SQLDataTypes.NVarchar)> <Size(500)>
     Property Description As String
     <SQLType(SQLDataTypes.Bit)>
     Property Enable As Boolean
     <Ignore> <Joinable("User_Role UserID ID,Roles ID ID")>
     Property Role As New List(Of Roles)

     Sub New()

     End Sub
 End Class
 Public Class Roles
     Inherits EntityModel
     <SQLType(SQLDataTypes.NVarchar)> <Size(50)>
     Property Name As String = ""
     <SQLType(SQLDataTypes.NVarchar)> <Size(500)>
     Property Description As String
     Sub New()

     End Sub
 End Class
 Public Class User_Role
     Inherits EntityModel
     <SQLType(SQLDataTypes.int)>
     Property UserID As Integer = 0
     <SQLType(SQLDataTypes.int)>
     Property RoleID As Integer = 0
     Sub New()

     End Sub
 End Class
```

10- Add Models to ORM

```
     Public Sub SyncStart(sender As Object, ByRef e As ORM.Events.SyncStartEvent)
         e.Objects.Add(New Models.Users)
         e.Objects.Add(New Models.User_Role)
         e.Objects.Add(New Models.Roles)
     End Sub
```

* if You Whant Add Default Record In Table Use This Code

```
    Public Sub SyncStart(sender As Object, ByRef e As ORM.Events.SyncStartEvent)
         Dim User As New Users With {
            .UserName = "Admin",
            .Password = "1234560",
            .Description = "Admin User",
            .Enable = True,
            .HasDefaultValue = True
        }
        Dim UserRole As New User_Role With {
            .UserID = 1,
            .RoleID = 1,
            .HasDefaultValue = True
        }
        Dim Role As New Roles With {
            .Name = "Admin",
            .Description = "Admin Role",
            .HasDefaultValue = True
        }
        e.Objects.Add(User)
        e.Objects.Add(UserRole)
        e.Objects.Add(Role)
    End Sub
```
* HasDefaultValue : Set to True For Insert Model Fields Values To Database
  
11- Syn ORM

```
  ORM.StartSync()
```

Enjoy All models are built inside SQL.

<p align="center"><img width="400" src="https://myrosha.ir/Images/SQL1.png" alt="Image"></p>
