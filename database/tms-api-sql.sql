USE [TMS-API]
GO
/****** Object:  FullTextCatalog [FT_DisplayName]    Script Date: 12/23/2014 09:22:08 ******/
CREATE FULLTEXT CATALOG [FT_DisplayName]WITH ACCENT_SENSITIVITY = OFF
AUTHORIZATION [dbo]
GO

CREATE FULLTEXT INDEX ON [dbo].[Constituents] KEY INDEX [PK_Constituents] ON ([FT_DisplayName], FILEGROUP [PRIMARY]) WITH (CHANGE_TRACKING AUTO)
GO
USE [TMS-API]
GO
ALTER FULLTEXT INDEX ON [dbo].[Constituents] ADD ([DisplayName] LANGUAGE [English])
GO
USE [TMS-API]
GO
ALTER FULLTEXT INDEX ON [dbo].[Constituents] ENABLE
GO


/****** Object:  FullTextCatalog [FT_exhibition_titles]    Script Date: 12/23/2014 09:22:08 ******/
CREATE FULLTEXT CATALOG [FT_exhibition_titles]WITH ACCENT_SENSITIVITY = OFF
AUTHORIZATION [dbo]
GO

CREATE FULLTEXT INDEX ON [dbo].[Exhibitions] KEY INDEX [PK_Exhibitions] ON ([FT_exhibition_titles], FILEGROUP [PRIMARY]) WITH (CHANGE_TRACKING AUTO)
GO
USE [TMS-API]
GO
ALTER FULLTEXT INDEX ON [dbo].[Exhibitions] ADD ([ExhTitle] LANGUAGE [English])
GO
USE [TMS-API]
GO
ALTER FULLTEXT INDEX ON [dbo].[Exhibitions] ENABLE

/****** Object:  FullTextCatalog [FT_Objects]    Script Date: 12/23/2014 09:22:08 ******/
CREATE FULLTEXT CATALOG [FT_Objects]WITH ACCENT_SENSITIVITY = OFF
AUTHORIZATION [dbo]
GO

CREATE FULLTEXT INDEX ON [dbo].[Objects] KEY INDEX [PK_Objects] ON ([FT_Objects], FILEGROUP [PRIMARY]) WITH (CHANGE_TRACKING MANUAL)
GO
USE [TMS-API]
GO
ALTER FULLTEXT INDEX ON [dbo].[Objects] ADD ([Title] LANGUAGE [English])
GO
USE [TMS-API]
GO
ALTER FULLTEXT INDEX ON [dbo].[Objects] ENABLE
GO



/****** Object:  StoredProcedure [dbo].[procTmsApiExhibitionSearch]    Script Date: 12/23/2014 09:22:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[procTmsApiExhibitionSearch]

	@p_exhibitiontitle NVARCHAR(255)

AS
BEGIN

SET NOCOUNT ON;

SELECT E.ExhibitionID,ProjectNumber,ExhTitle,D.Department,E.DisplayDate,BeginISODate, EndISODate, COUNT(EX.ObjectID) AS ResultsCount
FROM Exhibitions E
  INNER JOIN Departments D ON D.DepartmentID = E.ExhDepartment
  INNER JOIN ExhObjXrefs EX ON E.ExhibitionID = EX.ExhibitionID 
WHERE CONTAINS(ExhTitle, @p_exhibitiontitle)
GROUP BY E.ExhibitionID,ProjectNumber,ExhTitle,D.Department,E.DisplayDate,BeginISODate, EndISODate

END
GO
/****** Object:  StoredProcedure [dbo].[procTmsApiExhibitionObjects]    Script Date: 12/23/2014 09:22:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[procTmsApiExhibitionObjects]

	@p_exhibitionid INTEGER

AS
BEGIN

SET NOCOUNT ON;

SELECT ExhibitionID,ProjectNumber,ExhTitle,D.Department,DisplayDate,BeginISODate,EndISODate
FROM Exhibitions E
  INNER JOIN Departments D ON D.DepartmentID = E.ExhDepartment
WHERE  E.ExhibitionID = @p_exhibitionid;

SELECT EX.ObjectID
FROM Exhibitions E
  INNER JOIN ExhObjXrefs EX ON E.ExhibitionID = EX.ExhibitionID 
WHERE E.ExhibitionID = @p_exhibitionid;

END
GO
/****** Object:  StoredProcedure [dbo].[procTmsGetObjectID]    Script Date: 12/23/2014 09:22:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[procTmsGetObjectID]
	@p_objectnumber nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

  SELECT O.ObjectID
  FROM [dbo].[Objects] O
  LEFT JOIN [dbo].[AltNums] AN ON O.ObjectID = AN.ID
  LEFT JOIN Associations A ON O.ObjectID = A.ID2   
  WHERE ObjectNumber = @p_objectnumber
  OR AN.AltNum = @p_objectnumber
  AND A.ID2 IS NULL
  GROUP BY ObjectID
  
END
GO


/****** Object:  StoredProcedure [dbo].[procTmsApiTermsObjects]    Script Date: 12/23/2014 09:22:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[procTmsApiTermsObjects]

  @p_termid integer

AS
BEGIN

SET NOCOUNT ON;

SELECT  TX.ID AS ObjectID
FROM [TMSThes-API].dbo.TermMaster TM 
	INNER JOIN [TMSThes-API].dbo.Terms T ON TM.TermMasterID = T.TermMasterID
	INNER JOIN ThesXrefs TX ON T.TermID = TX.TermID
	INNER JOIN ThesXrefTypes TXT ON TXT.ThesXrefTypeID = TX.ThesXrefTypeID
WHERE TX.TableID = 108
AND TX.ThesXrefTypeID IN (19,110,112,158)
AND T.TermID = @p_termid

END
GO
/****** Object:  StoredProcedure [dbo].[procTmsApiTerms]    Script Date: 12/23/2014 09:22:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[procTmsApiTerms]

AS
BEGIN

SET NOCOUNT ON;

SELECT  T.TermID, T.Term, TXT.ThesXrefType AS TermType, COUNT(TX.ID) AS TermCount
FROM [TMSThes-API].dbo.TermMaster TM 
	INNER JOIN [TMSThes-API].dbo.Terms T ON TM.TermMasterID = T.TermMasterID
	INNER JOIN ThesXrefs TX ON T.TermID = TX.TermID
	INNER JOIN ThesXrefTypes TXT ON TXT.ThesXrefTypeID = TX.ThesXrefTypeID
WHERE TX.TableID = 108
AND TX.ThesXrefTypeID IN (19,110,112,158)
GROUP BY T.TermID, T.Term, TXT.ThesXrefType
HAVING COUNT(TX.ID) > 5

END
GO
/****** Object:  View [dbo].[vwMomaPrimaryObjImages]    Script Date: 12/23/2014 09:22:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
/****** Object:  View ObjImages    Script Date: 5/6/2003 1:38:59 PM ******/
CREATE VIEW  [dbo].[vwMomaPrimaryObjImages]
AS
SELECT MediaFiles.FileName, MediaMaster.MediaView, 
   MediaMaster.PublicCaption, Objects.ObjectID, MediaXrefs.Rank, 
   MediaMaster.Description, MediaTypes.MediaType, 
   MediaFormats.Format, MediaFiles.PixelH, MediaFiles.PixelW, 
   MediaFiles.FileSize, MediaXrefs.PrimaryDisplay, 
   MediaFiles.ColorDepthID, MediaRenditions.Quality, 
   MediaRenditions.Remarks, MediaFiles.FileDate, 
   MediaRenditions.RenditionDate, 
   MediaRenditions.RenditionNumber, 
   Objects.ObjectNumber, MediaRenditions.RenditionID, 
   MediaRenditions.MediaMasterID,
   MediaFiles.ArchIDNum,
   'Size1/Images/' + MediaRenditions.ThumbFileName AS Thumbnail,
   'Size3/Images/' + MediaFiles.FileName AS FullImage,
   MediaFiles.FileID,
   MediaFiles.sysTimeStamp AS MediaSysTimeStamp
FROM MediaRenditions INNER JOIN
   MediaMaster ON 
   MediaRenditions.MediaMasterID = MediaMaster.MediaMasterID INNER
    JOIN
   MediaFiles ON 
   MediaRenditions.PrimaryFileID = MediaFiles.FileID INNER JOIN
   MediaXrefs ON 
   MediaMaster.MediaMasterID = MediaXrefs.MediaMasterID INNER
    JOIN
   Objects ON MediaXrefs.ID = Objects.ObjectID AND 
   MediaXrefs.TableID = 108 INNER JOIN
   MediaTypes ON 
   MediaRenditions.MediaTypeID = MediaTypes.MediaTypeID INNER
    JOIN
   MediaFormats ON 
   MediaFiles.FormatID = MediaFormats.FormatID 
 WHERE  MediaXrefs.PrimaryDisplay = 1
GO
/****** Object:  StoredProcedure [dbo].[procTmsApiObjects]    Script Date: 12/23/2014 09:22:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[procTmsApiObjects]

  @p_objectid integer

AS
BEGIN

SELECT O.ObjectNumber
,O.ObjectID
,OT.Title
,ConAltNames.DisplayName
, ConAltNames.AlphaSort
,ISNULL(Constituents.[ConstituentID],0) AS ArtistID
,Constituents.DisplayDate
,O.Dated
,O.DateBegin
,O.DateEnd
,O.Medium
,O.Dimensions
,D.Department
,C.Classification
,O.OnView
,O.Provenance
,O.Description
,O.ObjectStatusID
,O.CreditLine
,GETDATE() AS LastModifiedDate
,MOI.ArchIDNum AS ImageID
,MOI.Thumbnail AS Thumbnail
,MOI.FullImage AS FullImage
FROM Objects O 
INNER JOIN  Departments D ON O.DepartmentID = D.DepartmentID 
INNER JOIN Classifications C ON C.ClassificationID = O.ClassificationID  
LEFT JOIN ObjTitles OT ON O.ObjectID = OT.ObjectID And OT.Displayed = 1 And OT.DisplayOrder = 1 And OT.IsExhTitle = 0
LEFT JOIN [dbo].[vwMomaPrimaryObjImages] MOI ON O.ObjectID = MOI.ObjectID  
LEFT JOIN (ConXrefs    
    INNER JOIN Roles ON ConXrefs.RoleID = Roles.RoleID    
    INNER JOIN ConXrefDetails ON ConXrefs.ConXrefID = ConXrefDetails.ConXrefID AND ConXrefDetails.UnMasked = 1    
    INNER JOIN ConAltNames ON ConXrefDetails.NameID = ConAltNames.AltNameID    
    INNER JOIN Constituents On ConXrefDetails.ConstituentID = Constituents.ConstituentID)                
ON O.ObjectID = ConXrefs.ID AND ConXrefs.RoleTypeID = 1 AND ConXrefs.TableID = 108 AND ConXrefs.DisplayOrder = 1 AND ConXrefs.Displayed = 1
WHERE O.ObjectID = @p_objectid;                 


SELECT E.ExhibitionID,ProjectNumber,ExhTitle,D.Department,E.DisplayDate,BeginISODate, EndISODate, (SELECT COUNT(EOX.ObjectID) FROM ExhObjXrefs EOX WHERE EOX.ExhibitionID = E.ExhibitionID)  AS ResultsCount
FROM Exhibitions E
  INNER JOIN Departments D ON D.DepartmentID = E.ExhDepartment
  INNER JOIN ExhObjXrefs EX ON E.ExhibitionID = EX.ExhibitionID 
WHERE EX.ObjectID = @p_objectid
GROUP BY E.ExhibitionID,ProjectNumber,ExhTitle,D.Department,E.DisplayDate,BeginISODate, EndISODate;

SELECT T.TermID, T.Term, TXT.ThesXrefType AS TermType
FROM [TMSThes-API].dbo.TermMaster TM 
	INNER JOIN [TMSThes-API].dbo.Terms T ON TM.TermMasterID = T.TermMasterID
	INNER JOIN ThesXrefs TX ON T.TermID = TX.TermID
	INNER JOIN ThesXrefTypes TXT ON TXT.ThesXrefTypeID = TX.ThesXrefTypeID
WHERE TX.TableID = 108 
AND TX.ThesXrefTypeID IN (19,110,112,158)
AND TX.ID = @p_objectid;


END
GO
/****** Object:  StoredProcedure [dbo].[procTmsApiArtistSearch]    Script Date: 12/23/2014 09:22:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[procTmsApiArtistSearch]

	@p_displayname NVARCHAR(255)

AS
BEGIN

SET NOCOUNT ON;

SELECT C.ConstituentID AS ArtistID, C.AlphaSort, C.DisplayName, C.BeginDate, C.EndDate, C.DisplayDate, C.Code AS Sex, C.Nationality, COUNT(X.ID) AS ResultsCount
FROM   Constituents As C
       Inner Join ConXrefDetails As CXD On C.ConstituentID = CXD.ConstituentID And CXD.UnMasked = 1
       Inner Join ConXrefs As X On CXD.ConXrefID = X.ConXrefID
       Inner Join Roles As R On X.RoleID = R.RoleID
WHERE  R.RoleTypeID = 1 And X.TableID = 108 And X.DisplayOrder = 1
AND CONTAINS(C.DisplayName, @p_displayname) 
GROUP BY C.ConstituentID, C.AlphaSort, C.DisplayName, C.BeginDate, C.EndDate, C.DisplayDate, C.Code, C.Nationality

END
GO
/****** Object:  StoredProcedure [dbo].[procTmsApiArtistObjects]    Script Date: 12/23/2014 09:22:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[procTmsApiArtistObjects]

	@p_artistid INTEGER

AS
BEGIN

SET NOCOUNT ON;

SELECT C.ConstituentID AS ArtistID, C.AlphaSort, C.DisplayName, C.BeginDate, C.EndDate, C.DisplayDate, C.Code AS Sex, C.Nationality
FROM   Constituents As C
       Inner Join ConXrefDetails As CXD On C.ConstituentID = CXD.ConstituentID And CXD.UnMasked = 1
       Inner Join ConXrefs As X On CXD.ConXrefID = X.ConXrefID
       Inner Join Roles As R On X.RoleID = R.RoleID
WHERE  R.RoleTypeID = 1 And X.TableID = 108 And X.DisplayOrder = 1
AND C.ConstituentID = @p_artistid
GROUP BY C.ConstituentID, C.AlphaSort, C.DisplayName, C.BeginDate, C.EndDate, C.DisplayDate, C.Code, C.Nationality

SELECT ObjectID 
FROM ObjArtist
WHERE ConstituentID = @p_artistid

END
GO
