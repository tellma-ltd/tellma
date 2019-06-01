﻿// <auto-generated />
using System;
using BSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BSharp.Data.Migrations.Application
{
    [DbContext(typeof(ApplicationContext))]
    partial class ApplicationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BSharp.Data.Model.Blob", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("TenantId");

                    b.Property<byte[]>("Content")
                        .IsRequired();

                    b.HasKey("Id", "TenantId");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.ToTable("Blobs");
                });

            modelBuilder.Entity("BSharp.Data.Model.Custody", b =>
                {
                    b.Property<int>("TenantId");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Address")
                        .HasMaxLength(1024);

                    b.Property<DateTimeOffset?>("BirthDateTime");

                    b.Property<string>("Code")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<int>("CreatedById");

                    b.Property<string>("CustodyType")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<DateTimeOffset>("ModifiedAt");

                    b.Property<int>("ModifiedById");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Name2")
                        .HasMaxLength(255);

                    b.HasKey("TenantId", "Id");

                    b.HasIndex("TenantId", "Code")
                        .IsUnique()
                        .HasFilter("[Code] IS NOT NULL");

                    b.HasIndex("TenantId", "CreatedById");

                    b.HasIndex("TenantId", "ModifiedById");

                    b.ToTable("Custodies");

                    b.HasDiscriminator<string>("CustodyType").HasValue("Custody");
                });

            modelBuilder.Entity("BSharp.Data.Model.IfrsConcept", b =>
                {
                    b.Property<int>("TenantId")
                        .HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'TenantId'))");

                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<int>("CreatedById")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'UserId'))");

                    b.Property<string>("Documentation")
                        .IsRequired();

                    b.Property<string>("Documentation2");

                    b.Property<string>("Documentation3");

                    b.Property<DateTime>("EffectiveDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("'0001-01-01 00:00:00'");

                    b.Property<DateTime>("ExpiryDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("'9999-12-31 23:59:59'");

                    b.Property<string>("IfrsType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(255)
                        .HasDefaultValue("Regulatory");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(1024);

                    b.Property<string>("Label2")
                        .HasMaxLength(1024);

                    b.Property<string>("Label3")
                        .HasMaxLength(1024);

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<int>("ModifiedById")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'UserId'))");

                    b.HasKey("TenantId", "Id");

                    b.HasIndex("TenantId", "CreatedById");

                    b.HasIndex("TenantId", "ModifiedById");

                    b.ToTable("IfrsConcepts");
                });

            modelBuilder.Entity("BSharp.Data.Model.IfrsNote", b =>
                {
                    b.Property<int>("TenantId")
                        .HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'TenantId'))");

                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(255);

                    b.Property<bool>("ForCredit")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<bool>("ForDebit")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<bool>("IsAggregate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<short>("Level")
                        .HasMaxLength(255);

                    b.Property<string>("Node");

                    b.Property<string>("ParentNode");

                    b.HasKey("TenantId", "Id");

                    b.ToTable("IfrsNotes");
                });

            modelBuilder.Entity("BSharp.Data.Model.LocalUser", b =>
                {
                    b.Property<int>("TenantId");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("AgentId");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<int>("CreatedById");

                    b.Property<int?>("CreatedById1");

                    b.Property<int?>("CreatedByTenantId");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("ExternalId")
                        .HasMaxLength(450);

                    b.Property<string>("ImageId");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<DateTimeOffset?>("LastAccess");

                    b.Property<DateTimeOffset>("ModifiedAt");

                    b.Property<int>("ModifiedById");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Name2")
                        .HasMaxLength(255);

                    b.Property<Guid>("PermissionsVersion")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new Guid("aafc6590-cadf-45fe-8c4a-045f4d6f73b1"));

                    b.Property<Guid>("UserSettingsVersion")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new Guid("aafc6590-cadf-45fe-8c4a-045f4d6f73b1"));

                    b.HasKey("TenantId", "Id");

                    b.HasIndex("CreatedByTenantId", "CreatedById1")
                        .IsUnique()
                        .HasFilter("[CreatedByTenantId] IS NOT NULL AND [CreatedById1] IS NOT NULL");

                    b.HasIndex("TenantId", "AgentId");

                    b.HasIndex("TenantId", "Email")
                        .IsUnique();

                    b.HasIndex("TenantId", "ExternalId")
                        .IsUnique()
                        .HasFilter("[ExternalId] IS NOT NULL");

                    b.ToTable("LocalUsers");
                });

            modelBuilder.Entity("BSharp.Data.Model.MeasurementUnit", b =>
                {
                    b.Property<int>("TenantId")
                        .HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'TenantId'))");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("BaseAmount");

                    b.Property<string>("Code")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<int>("CreatedById")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'UserId'))");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<DateTimeOffset>("ModifiedAt")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("SYSDATETIMEOFFSET()");

                    b.Property<int>("ModifiedById")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("CONVERT(INT, SESSION_CONTEXT(N'UserId'))");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Name2")
                        .HasMaxLength(255);

                    b.Property<double>("UnitAmount");

                    b.Property<string>("UnitType")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.HasKey("TenantId", "Id");

                    b.HasIndex("TenantId", "Code")
                        .IsUnique()
                        .HasFilter("[Code] IS NOT NULL");

                    b.HasIndex("TenantId", "CreatedById");

                    b.HasIndex("TenantId", "ModifiedById");

                    b.ToTable("MeasurementUnits");
                });

            modelBuilder.Entity("BSharp.Data.Model.Permission", b =>
                {
                    b.Property<int>("TenantId");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<int>("CreatedById");

                    b.Property<string>("Criteria")
                        .HasMaxLength(1024);

                    b.Property<string>("Level")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Mask")
                        .HasMaxLength(2048);

                    b.Property<string>("Memo")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("ModifiedAt");

                    b.Property<int>("ModifiedById");

                    b.Property<int>("RoleId");

                    b.Property<string>("ViewId")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.HasKey("TenantId", "Id");

                    b.HasIndex("TenantId", "CreatedById");

                    b.HasIndex("TenantId", "ModifiedById");

                    b.HasIndex("TenantId", "RoleId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("BSharp.Data.Model.Role", b =>
                {
                    b.Property<int>("TenantId");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Code")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<int>("CreatedById");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.Property<bool>("IsPublic");

                    b.Property<DateTimeOffset>("ModifiedAt");

                    b.Property<int>("ModifiedById");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Name2")
                        .HasMaxLength(255);

                    b.HasKey("TenantId", "Id");

                    b.HasIndex("TenantId", "Code")
                        .IsUnique()
                        .HasFilter("[Code] IS NOT NULL");

                    b.HasIndex("TenantId", "CreatedById");

                    b.HasIndex("TenantId", "IsPublic")
                        .HasFilter("[IsPublic] = 1");

                    b.HasIndex("TenantId", "ModifiedById");

                    b.HasIndex("TenantId", "Name")
                        .IsUnique();

                    b.HasIndex("TenantId", "Name2")
                        .IsUnique()
                        .HasFilter("[Name2] IS NOT NULL");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("BSharp.Data.Model.RoleMembership", b =>
                {
                    b.Property<int>("TenantId");

                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<int>("CreatedById");

                    b.Property<string>("Memo");

                    b.Property<DateTimeOffset>("ModifiedAt");

                    b.Property<int>("ModifiedById");

                    b.Property<int>("RoleId");

                    b.Property<int>("UserId");

                    b.HasKey("TenantId", "Id");

                    b.HasIndex("TenantId", "CreatedById");

                    b.HasIndex("TenantId", "ModifiedById");

                    b.HasIndex("TenantId", "RoleId");

                    b.HasIndex("TenantId", "UserId");

                    b.ToTable("RoleMemberships");
                });

            modelBuilder.Entity("BSharp.Data.Model.Settings", b =>
                {
                    b.Property<int>("TenantId");

                    b.Property<string>("BrandColor")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("ModifiedAt");

                    b.Property<int>("ModifiedById");

                    b.Property<string>("PrimaryLanguageId")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("PrimaryLanguageSymbol")
                        .HasMaxLength(255);

                    b.Property<DateTimeOffset>("ProvisionedAt");

                    b.Property<string>("SecondaryLanguageId")
                        .HasMaxLength(255);

                    b.Property<string>("SecondaryLanguageSymbol")
                        .HasMaxLength(255);

                    b.Property<Guid>("SettingsVersion")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd"));

                    b.Property<string>("ShortCompanyName")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("ShortCompanyName2")
                        .HasMaxLength(255);

                    b.Property<Guid>("ViewsAndSpecsVersion")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(new Guid("132e9cf1-e0d2-4dfd-a3a8-22e4b9b8b9fd"));

                    b.HasKey("TenantId");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("BSharp.Data.Model.View", b =>
                {
                    b.Property<int>("TenantId");

                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(255);

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(true);

                    b.HasKey("TenantId", "Id");

                    b.ToTable("Views");
                });

            modelBuilder.Entity("BSharp.Data.Model.Agent", b =>
                {
                    b.HasBaseType("BSharp.Data.Model.Custody");

                    b.Property<string>("AgentType")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("Gender")
                        .HasConversion(new ValueConverter<string, string>(v => default(string), v => default(string), new ConverterMappingHints(size: 1)));

                    b.Property<bool>("IsRelated")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<string>("TaxIdentificationNumber")
                        .HasMaxLength(255);

                    b.Property<string>("Title")
                        .HasMaxLength(255);

                    b.Property<string>("Title2")
                        .HasMaxLength(255);

                    b.HasDiscriminator().HasValue("Agent");
                });

            modelBuilder.Entity("BSharp.Data.Model.Custody", b =>
                {
                    b.HasOne("BSharp.Data.Model.LocalUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "CreatedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("BSharp.Data.Model.LocalUser", "ModifiedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "ModifiedById")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("BSharp.Data.Model.IfrsConcept", b =>
                {
                    b.HasOne("BSharp.Data.Model.LocalUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "CreatedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("BSharp.Data.Model.LocalUser", "ModifiedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "ModifiedById")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("BSharp.Data.Model.IfrsNote", b =>
                {
                    b.HasOne("BSharp.Data.Model.IfrsConcept", "Concept")
                        .WithOne("Note")
                        .HasForeignKey("BSharp.Data.Model.IfrsNote", "TenantId", "Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BSharp.Data.Model.LocalUser", b =>
                {
                    b.HasOne("BSharp.Data.Model.LocalUser", "CreatedBy")
                        .WithOne("ModifiedBy")
                        .HasForeignKey("BSharp.Data.Model.LocalUser", "CreatedByTenantId", "CreatedById1");

                    b.HasOne("BSharp.Data.Model.Agent", "Agent")
                        .WithMany("Users")
                        .HasForeignKey("TenantId", "AgentId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("BSharp.Data.Model.MeasurementUnit", b =>
                {
                    b.HasOne("BSharp.Data.Model.LocalUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "CreatedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("BSharp.Data.Model.LocalUser", "ModifiedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "ModifiedById")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("BSharp.Data.Model.Permission", b =>
                {
                    b.HasOne("BSharp.Data.Model.LocalUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "CreatedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("BSharp.Data.Model.LocalUser", "ModifiedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "ModifiedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("BSharp.Data.Model.Role", "Role")
                        .WithMany("Permissions")
                        .HasForeignKey("TenantId", "RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("BSharp.Data.Model.Role", b =>
                {
                    b.HasOne("BSharp.Data.Model.LocalUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "CreatedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("BSharp.Data.Model.LocalUser", "ModifiedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "ModifiedById")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("BSharp.Data.Model.RoleMembership", b =>
                {
                    b.HasOne("BSharp.Data.Model.LocalUser", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "CreatedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("BSharp.Data.Model.LocalUser", "ModifiedBy")
                        .WithMany()
                        .HasForeignKey("TenantId", "ModifiedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("BSharp.Data.Model.Role", "Role")
                        .WithMany("Members")
                        .HasForeignKey("TenantId", "RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("BSharp.Data.Model.LocalUser", "User")
                        .WithMany("Roles")
                        .HasForeignKey("TenantId", "UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
