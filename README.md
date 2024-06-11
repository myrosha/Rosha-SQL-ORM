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

4- Add Syn Method to Form,Page or Class

```
    Public Sub SyncStart(sender As Object, ByRef e As ORM.Events.SyncStartEvent)
    End Sub
```

5- Add Event Handle to Load Form or Page or Component

```
        AddHandler ORM.SyncStart, AddressOf SyncStart
```
