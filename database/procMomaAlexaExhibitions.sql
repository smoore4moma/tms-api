-- =============================================
-- Author:		MOORE, Steven, MoMA DBA
-- Create date: 8 September 2016
-- Description:	MoMA API data for Amazon Echo
-- =============================================

CREATE PROCEDURE [dbo].[procMomaAlexaExhibitions]

	@p_alexa_date VARCHAR(50) = ''

AS
BEGIN

SET NOCOUNT ON;

DECLARE	@p_start_date VARCHAR(10) = ''
DECLARE @p_end_date VARCHAR(10) = ''
--DECLARE @p_alexa_date VARCHAR(50)  
--SET @p_alexa_date = '2015-W20-WE'

	-- Today if null
	IF(@p_alexa_date = '' OR @p_alexa_date IS NULL)
	BEGIN
	SET @p_start_date = (SELECT CONVERT(VARCHAR(10), GETDATE(), 121)) 
	SET @p_end_date = (SELECT CONVERT(VARCHAR(10), GETDATE(), 121)) 
	END

	-- Alexa provided a normal date
	IF(ISDATE(@p_alexa_date) = 1)
	BEGIN
	SET @p_start_date =  CONVERT(VARCHAR(10), @p_alexa_date, 121) 
	SET @p_end_date =  CONVERT(VARCHAR(10), @p_alexa_date, 121) 
	END

	-- Alexa provided a year-month
	IF(LEN(@p_alexa_date) <= 8 AND ISNUMERIC(REPLACE(@p_alexa_date,'-','')) = 1)
	BEGIN
	SET @p_start_date = @p_alexa_date + '-01' 
	SET @p_end_date =  @p_alexa_date + '-31' 
	END

	-- weekend
	IF(RIGHT(@p_alexa_date, 3) = '-WE') 
		BEGIN
		DECLARE @p_week VARCHAR(2)
		DECLARE @p_year VARCHAR(4)
		DECLARE	@p_start_date_wk VARCHAR(10) = ''
		DECLARE @p_end_date_wk VARCHAR(10) = ''
		
		SET @p_week = REPLACE(SUBSTRING(@p_alexa_date,7, LEN(@p_alexa_date)),'-WE','')
		SET @p_year = LEFT(@p_alexa_date, 4)

		SET @p_start_date_wk = CONVERT(VARCHAR(10),  DATEADD(wk, DATEDIFF(wk, 6, '1/1/' + @p_year) + (@p_week-1), 6), 121)  
		SET @p_end_date_wk = CONVERT(VARCHAR(10), DATEADD(wk, DATEDIFF(wk, 5, '1/1/' + @p_year) + (@p_week-1), 5), 121)    

		SET @p_start_date = CASE WHEN datename(weekday, CAST(@p_start_date_wk AS smalldatetime)) = 'Saturday' THEN CONVERT(VARCHAR(10), CAST(@p_start_date_wk AS smalldatetime), 121)  
		                         WHEN datename(weekday, CAST(@p_start_date_wk AS smalldatetime) + 1) = 'Saturday' THEN CONVERT(VARCHAR(10), CAST(@p_start_date_wk AS smalldatetime) + 1, 121)  
								 WHEN datename(weekday, CAST(@p_start_date_wk AS smalldatetime) + 2) = 'Saturday' THEN CONVERT(VARCHAR(10), CAST(@p_start_date_wk AS smalldatetime) + 2, 121)  
								 WHEN datename(weekday, CAST(@p_start_date_wk AS smalldatetime) + 3) = 'Saturday' THEN CONVERT(VARCHAR(10), CAST(@p_start_date_wk AS smalldatetime) + 3, 121)  
								 WHEN datename(weekday, CAST(@p_start_date_wk AS smalldatetime) + 4) = 'Saturday' THEN CONVERT(VARCHAR(10), CAST(@p_start_date_wk AS smalldatetime) + 4, 121)  
								 WHEN datename(weekday, CAST(@p_start_date_wk AS smalldatetime) + 5) = 'Saturday' THEN CONVERT(VARCHAR(10), CAST(@p_start_date_wk AS smalldatetime) + 5, 121)  
								 WHEN datename(weekday, CAST(@p_start_date_wk AS smalldatetime) + 6) = 'Saturday' THEN CONVERT(VARCHAR(10), CAST(@p_start_date_wk AS smalldatetime) + 6, 121)  
								 END
		SET @p_end_date = CASE WHEN datename(weekday, CAST(@p_end_date_wk AS smalldatetime)) = 'Sunday' THEN CONVERT(VARCHAR(10), CAST(@p_end_date_wk AS smalldatetime), 121)  
		                         WHEN datename(weekday, CAST(@p_end_date_wk AS smalldatetime) + 1) = 'Sunday' THEN CONVERT(VARCHAR(10), CAST(@p_end_date_wk AS smalldatetime) + 1, 121)  
								 WHEN datename(weekday, CAST(@p_end_date_wk AS smalldatetime) + 2) = 'Sunday' THEN CONVERT(VARCHAR(10), CAST(@p_end_date_wk AS smalldatetime) + 2, 121)  
								 WHEN datename(weekday, CAST(@p_end_date_wk AS smalldatetime) + 3) = 'Sunday' THEN CONVERT(VARCHAR(10), CAST(@p_end_date_wk AS smalldatetime) + 3, 121)  
								 WHEN datename(weekday, CAST(@p_end_date_wk AS smalldatetime) + 4) = 'Sunday' THEN CONVERT(VARCHAR(10), CAST(@p_end_date_wk AS smalldatetime) + 4, 121)  
								 WHEN datename(weekday, CAST(@p_end_date_wk AS smalldatetime) + 5) = 'Sunday' THEN CONVERT(VARCHAR(10), CAST(@p_end_date_wk AS smalldatetime) + 5, 121)  
								 WHEN datename(weekday, CAST(@p_end_date_wk AS smalldatetime) + 6) = 'Sunday' THEN CONVERT(VARCHAR(10), CAST(@p_end_date_wk AS smalldatetime) + 6, 121)  
								 END
		END

	-- not a weekend
	IF((@p_alexa_date LIKE '%-W%') AND (RIGHT(@p_alexa_date, 3) <> '-WE'))
		BEGIN
		
		SET @p_week = SUBSTRING(@p_alexa_date,7, LEN(@p_alexa_date))
		SET @p_year = LEFT(@p_alexa_date, 4)

		SET @p_start_date = CONVERT(VARCHAR(10),  DATEADD(wk, DATEDIFF(wk, 6, '1/1/' + @p_year) + (@p_week-1), 6), 121)  
		SET @p_end_date = CONVERT(VARCHAR(10), DATEADD(wk, DATEDIFF(wk, 5, '1/1/' + @p_year) + (@p_week-1), 5), 121)    

		END
	

SELECT DISTINCT Exhibitions.ExhTitle, Exhibitions.DisplayDate
FROM   Exhibitions INNER JOIN
       ExhVenuesXrefs ON Exhibitions.ExhibitionID = ExhVenuesXrefs.ExhibitionID
WHERE  (ExhVenuesXrefs.ConstituentID = 8901) 
AND (ExhVenuesXrefs.BeginISODate <= @p_start_date) 
AND (ExhVenuesXrefs.EndISODate >= @p_end_date) 
AND Exhibitions.ExhDepartment not in (54, 69, 76, 83)


END







GO
