using System;
using BattleGame.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BattleGame.Migrations
{
    [DbContext(typeof(BattleGameDbContext))]
    [Migration("20261009012944_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BattleGame.Models.Asset", b =>
                {
                    b.Property<Guid>("AssetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AssetName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<int>("LevelRequire")
                        .HasColumnType("int");

                    b.HasKey("AssetId");

                    b.ToTable("Asset", (string)null);
                });

            modelBuilder.Entity("BattleGame.Models.Player", b =>
                {
                    b.Property<Guid>("PlayerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Age")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<int>("Level")
                        .HasColumnType("int");

                    b.Property<string>("PlayerName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("PlayerId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Player", (string)null);
                });

            modelBuilder.Entity("BattleGame.Models.PlayerAsset", b =>
                {
                    b.Property<Guid>("PlayerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AssetId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("PlayerId", "AssetId");

                    b.HasIndex("AssetId");

                    b.ToTable("PlayerAsset", (string)null);
                });

            modelBuilder.Entity("BattleGame.Models.PlayerAsset", b =>
                {
                    b.HasOne("BattleGame.Models.Asset", "Asset")
                        .WithMany("PlayerAssets")
                        .HasForeignKey("AssetId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("BattleGame.Models.Player", "Player")
                        .WithMany("PlayerAssets")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Asset");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("BattleGame.Models.Asset", b =>
                {
                    b.Navigation("PlayerAssets");
                });

            modelBuilder.Entity("BattleGame.Models.Player", b =>
                {
                    b.Navigation("PlayerAssets");
                });
#pragma warning restore 612, 618
        }
    }
}
