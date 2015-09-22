README.txt

This file is in reference to just the database part.  

There are references to Microsoft Visual Studio and SQL Server, both which are free (Express versions).

The best way to start this project is to create a new Single Page Application in Visual Studio 2013 with MVC and Web API core references. This will create the basic infrastucture, 
including an Access database that you can use for security.  Below are optional steps that allows you to move the Access database to SQL Server.  

ASPNET-TMS-API [Optional]

1) First, create a blank database called aspnet-tms-api, then run aspnet-tms-api.sql.

2) Create a SQL login and give the user db_datareader and db_datawriter access to aspnet-tms-api.

3) The user will also need rights to execute a stored procedure called procValidateToken.  Run the following:

USE [aspnet-moma-api]
GO

GRANT EXECUTE ON OBJECT::dbo.procValidateToken TO {TheNameOfYourUser}
GO

TMS and TMSThes

You should run the API against a copy of your TMS database.  To make the API public, it is wise to set up Snapshot Replication and use a copy of your database. This also allows 
you to chose which tables/views/etc you want to copy.

The API has a rather important dependency on stored procedures.  The script called tms-api-sql.sql will create all stored procedures and views used by the API.  It assumes that the
copy of your databases are called TMS-API and TMSThes-API.  No "native" TMS database objects are changed.      

There are three areas that have values that need to be changed for your environment.

1) procTmsApiObjects and procTmsApiTerms have have references to ThesXrefTypeID that need to be changed.
2) procTmsApiObjects has a field called LastModifiedDate.  This is set to GETDATE().  There is a complicated daily process to find the date an object has changed, but this is not
part of the scope of this project.
3) There are two services which allow you to search for an artist name or exhibition title with AND, OR, and NOT.  These services rely on SQL Server Full-Text Search.  

You will need to create a user if you did not already in Step 2 of the aspnet-tms-api database.  The user should have db_datareader access to both your TMS and TMSThes databases (copies!).
The user will also need Execute permissions on a number of stored procedures.  Run the following after running tms-api-sql.sql 

GRANT EXECUTE ON OBJECT::dbo.procTmsApiArtistObjects TO {TheNameOfYourUser}
GO

GRANT EXECUTE ON OBJECT::dbo.procTmsApiArtistSearch TO {TheNameOfYourUser}
GO

GRANT EXECUTE ON OBJECT::dbo.procTmsApiExhibitionObjects TO {TheNameOfYourUser}
GO

GRANT EXECUTE ON OBJECT::dbo.procTmsApiExhibitionSearch TO {TheNameOfYourUser}
GO

GRANT EXECUTE ON OBJECT::dbo.procTmsApiObjects TO {TheNameOfYourUser}
GO

GRANT EXECUTE ON OBJECT::dbo.procTmsApiTerms TO {TheNameOfYourUser}
GO

GRANT EXECUTE ON OBJECT::dbo.procTmsApiTermsObjects TO {TheNameOfYourUser}
GO

GRANT EXECUTE ON OBJECT::dbo.procTmsGetObjectID TO {TheNameOfYourUser}
GO




