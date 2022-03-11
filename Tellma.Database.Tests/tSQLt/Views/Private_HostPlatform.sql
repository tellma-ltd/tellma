--CREATE   VIEW tSQLt.Private_HostPlatform AS SELECT host_platform FROM sys.dm_os_host_info;
CREATE   VIEW tSQLt.Private_HostPlatform AS SELECT N'Windows' AS host_platform 