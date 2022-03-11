CREATE FUNCTION tSQLt.Info()
RETURNS TABLE
AS
RETURN
SELECT Version = '1.0.8083.3529',
       ClrVersion = (SELECT tSQLt.Private::Info()),
       ClrSigningKey = (SELECT tSQLt.Private::SigningKey()),
       InstalledOnSqlVersion = (SELECT SqlVersion FROM tSQLt.Private_InstallationInfo()),
       V.SqlVersion,
       V.SqlBuild,
       V.SqlEdition,
       V.HostPlatform
  FROM
  (
    SELECT CAST(PSSV.Major+'.'+PSSV.Minor AS NUMERIC(10,2)) AS SqlVersion,
           CAST(PSSV.Build+'.'+PSSV.Revision AS NUMERIC(10,2)) AS SqlBuild,
           PSV.Edition AS SqlEdition,
           PHP.host_platform AS HostPlatform
          FROM tSQLt.Private_SqlVersion() AS PSV
         CROSS APPLY tSQLt.Private_SplitSqlVersion(PSV.ProductVersion) AS PSSV
         CROSS JOIN tSQLt.Private_HostPlatform AS PHP
  )V;
