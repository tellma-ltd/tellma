﻿CREATE FUNCTION [map].[Lines]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Lines]
);
