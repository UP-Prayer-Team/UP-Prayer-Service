UP Prayer Service
=================

Setting Up
----------

 - Clone this repository
 - Build the project (see **Building & Running** below)
 - Apply database migrations to create an initial database (see **Applying Migrations to Database** below)

Building & Running
------------------

**Visual Studio**:  
Use the standard Build & Debug tools (play button, menu commands, etc.)

**Command-Line**:  
Build with
```
dotnet build
```
  
Build & run with
```
dotnet run
```

Database Migrations
-------------------

When someone makes changes to the `DataContext` class or any of the types it uses (mainly the `Models` namepace), the database needs to be updated to reflect the changes that have been made.  
Entity Framework Core tracks the entire history of the database format using 'Migrations', which are records of the changes that have been made from one version of the database schema to the next.
EF Core also provides command-line tools to carry out this process.

Creating Migrations to Match Model Changes
------------------------------------------

**Command-Line**:  
```
dotnet ef migrations add <MigrationName>
```

Applying Migrations to Database
-------------------------------

**Command-Line**:  
```
dotnet ef database update
```