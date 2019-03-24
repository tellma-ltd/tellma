using Microsoft.EntityFrameworkCore;
using System;

namespace BSharp.Data.DbModel
{
    /// <summary>
    /// These settings live in the manager DB and are global for the entire instance
    /// </summary>
    public class GlobalSettings : DbModelBase
    {
        /// <summary>
        /// Just to keep EF happy
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The version of all settings that are cached in any layer
        /// </summary>
        public Guid SettingsVersion { get; set; }

        internal static void OnModelCreating(ModelBuilder builder)
        {
            // Just a random GUID
            Guid defaultGuid = new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd");

            // This adds the only row that will be ever added when the DB is provisioned
            builder.Entity<GlobalSettings>().HasData(new GlobalSettings
            {
                Id = 1,
                SettingsVersion = defaultGuid
            });
        }
    }
}

