CREATE FUNCTION bll.fn_UserSettingsFavorites__RemoveUnauthorizedAccess(@FavValue NVARCHAR(MAX), @ReportDefinitionId INT)
RETURNS NVARCHAR(MAX)
AS BEGIN
    -- Find the position of /report/70 (without wildcards)
    DECLARE @searchPattern NVARCHAR(50) = '/report/' + CAST(@ReportDefinitionId AS NVARCHAR(50));
    DECLARE @pos INT = CHARINDEX(@searchPattern, @FavValue);
    
    IF @pos > 0
    BEGIN
        -- Find the opening brace before the pattern
        DECLARE @startPos INT = @pos;
        WHILE @startPos > 0 AND SUBSTRING(@FavValue, @startPos, 1) != '{'
            SET @startPos = @startPos - 1;
    
        -- Find the closing brace after the pattern
        DECLARE @endPos INT = @startPos;
        DECLARE @braceCount INT = 0;
    
        WHILE @endPos <= LEN(@FavValue)
        BEGIN
            DECLARE @char NCHAR(1) = SUBSTRING(@FavValue, @endPos, 1);
            IF @char = '{' SET @braceCount = @braceCount + 1;
            IF @char = '}' 
            BEGIN
                SET @braceCount = @braceCount - 1;
                IF @braceCount = 0
                BEGIN
                    SET @endPos = @endPos + 1;
                    BREAK;
                END
            END
            SET @endPos = @endPos + 1;
        END
    
        -- Handle comma removal (check what comes after and before)
        DECLARE @removeLength INT = @endPos - @startPos;
        DECLARE @removeStart INT = @startPos;
        
        -- Check if there's a comma after the object
        IF SUBSTRING(@FavValue, @endPos, 1) = ','
        BEGIN
            SET @removeLength = @removeLength + 1; -- Include the comma after
        END
        -- Otherwise check if there's a comma before the object
        ELSE IF @startPos > 1 AND SUBSTRING(@FavValue, @startPos - 1, 1) = ','
        BEGIN
            SET @removeStart = @startPos - 1;
            SET @removeLength = @removeLength + 1; -- Include the comma before
        END
        
        SET @FavValue = STUFF(@FavValue, @removeStart, @removeLength, '');
    END
    
    RETURN @FavValue;
END
GO