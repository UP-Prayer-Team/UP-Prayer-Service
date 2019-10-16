UP Prayer Service
=================

Setting Up
----------

 - Clone repository
 - Build (see **Building & Running** below)
 - Apply Database Migrations to create an initial datbase (see **Applying Migrations to Datbase** below)
 - You are set!

Building & Running
------------------

**Visual Studio**:  
Use the standard Build & Debug tools (play button, etc.)

**Command-Line**:  
Build with
```
dotnet build
```
  
Build & run with
```
dotnet run
```

Applying Migrations to Database
-------------------------------

**Command-Line**:  
```
dotnet ef database update
```

Creating Migrations to Match Model Changes
------------------------------------------

**Command-Line**:  
```
dotnet ef migrations add <MigrationName>
```

Make sure to apply the created migrations (see **Applying Migrations to Database** above)