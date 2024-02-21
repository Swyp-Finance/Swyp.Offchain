﻿// <auto-generated />
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Swyp.Sync.Data;

#nullable disable

namespace Swyp.Sync.Migrations
{
    [DbContext(typeof(SwypDbContext))]
    [Migration("20240215054544_TeddyAdaLiquidityBySlot")]
    partial class TeddyAdaLiquidityBySlot
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Cardano.Sync.Data.Models.Block", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<decimal>("Number")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id", "Number", "Slot");

                    b.ToTable("Blocks", "public");
                });

            modelBuilder.Entity("Cardano.Sync.Data.Models.ReducerState", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Name");

                    b.ToTable("ReducerStates", "public");
                });

            modelBuilder.Entity("Cardano.Sync.Data.Models.TransactionOutput", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<long>("Index")
                        .HasColumnType("bigint");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id", "Index");

                    b.ToTable("TransactionOutputs", "public");
                });

            modelBuilder.Entity("Swyp.Sync.Data.Models.TbcByAddress", b =>
                {
                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Address", "Slot");

                    b.ToTable("TbcByAddress", "public");
                });

            modelBuilder.Entity("Swyp.Sync.Data.Models.TeddyAdaLiquidityBySlot", b =>
                {
                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<JsonElement>("LiquidityPoolJson")
                        .HasColumnType("jsonb");

                    b.HasKey("Slot", "TxHash", "TxIndex");

                    b.ToTable("TeddyAdaLiquidityBySlot", "public");
                });

            modelBuilder.Entity("Swyp.Sync.Data.Models.TeddyByAddress", b =>
                {
                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Address", "Slot");

                    b.ToTable("TeddyByAddress", "public");
                });

            modelBuilder.Entity("Cardano.Sync.Data.Models.TransactionOutput", b =>
                {
                    b.OwnsOne("Cardano.Sync.Data.Models.Datum", "Datum", b1 =>
                        {
                            b1.Property<string>("TransactionOutputId")
                                .HasColumnType("text");

                            b1.Property<long>("TransactionOutputIndex")
                                .HasColumnType("bigint");

                            b1.Property<byte[]>("Data")
                                .IsRequired()
                                .HasColumnType("bytea");

                            b1.Property<int>("Type")
                                .HasColumnType("integer");

                            b1.HasKey("TransactionOutputId", "TransactionOutputIndex");

                            b1.ToTable("TransactionOutputs", "public");

                            b1.WithOwner()
                                .HasForeignKey("TransactionOutputId", "TransactionOutputIndex");
                        });

                    b.OwnsOne("Cardano.Sync.Data.Models.Value", "Amount", b1 =>
                        {
                            b1.Property<string>("TransactionOutputId")
                                .HasColumnType("text");

                            b1.Property<long>("TransactionOutputIndex")
                                .HasColumnType("bigint");

                            b1.Property<decimal>("Coin")
                                .HasColumnType("numeric(20,0)");

                            b1.Property<JsonElement>("MultiAssetJson")
                                .HasColumnType("jsonb");

                            b1.HasKey("TransactionOutputId", "TransactionOutputIndex");

                            b1.ToTable("TransactionOutputs", "public");

                            b1.WithOwner()
                                .HasForeignKey("TransactionOutputId", "TransactionOutputIndex");
                        });

                    b.Navigation("Amount")
                        .IsRequired();

                    b.Navigation("Datum");
                });

            modelBuilder.Entity("Swyp.Sync.Data.Models.TbcByAddress", b =>
                {
                    b.OwnsOne("Cardano.Sync.Data.Models.Value", "Amount", b1 =>
                        {
                            b1.Property<string>("TbcByAddressAddress")
                                .HasColumnType("text");

                            b1.Property<decimal>("TbcByAddressSlot")
                                .HasColumnType("numeric(20,0)");

                            b1.Property<decimal>("Coin")
                                .HasColumnType("numeric(20,0)");

                            b1.Property<JsonElement>("MultiAssetJson")
                                .HasColumnType("jsonb");

                            b1.HasKey("TbcByAddressAddress", "TbcByAddressSlot");

                            b1.ToTable("TbcByAddress", "public");

                            b1.WithOwner()
                                .HasForeignKey("TbcByAddressAddress", "TbcByAddressSlot");
                        });

                    b.Navigation("Amount")
                        .IsRequired();
                });

            modelBuilder.Entity("Swyp.Sync.Data.Models.TeddyAdaLiquidityBySlot", b =>
                {
                    b.OwnsOne("Cardano.Sync.Data.Models.Value", "Amount", b1 =>
                        {
                            b1.Property<decimal>("TeddyAdaLiquidityBySlotSlot")
                                .HasColumnType("numeric(20,0)");

                            b1.Property<string>("TeddyAdaLiquidityBySlotTxHash")
                                .HasColumnType("text");

                            b1.Property<decimal>("TeddyAdaLiquidityBySlotTxIndex")
                                .HasColumnType("numeric(20,0)");

                            b1.Property<decimal>("Coin")
                                .HasColumnType("numeric(20,0)");

                            b1.Property<JsonElement>("MultiAssetJson")
                                .HasColumnType("jsonb");

                            b1.HasKey("TeddyAdaLiquidityBySlotSlot", "TeddyAdaLiquidityBySlotTxHash", "TeddyAdaLiquidityBySlotTxIndex");

                            b1.ToTable("TeddyAdaLiquidityBySlot", "public");

                            b1.WithOwner()
                                .HasForeignKey("TeddyAdaLiquidityBySlotSlot", "TeddyAdaLiquidityBySlotTxHash", "TeddyAdaLiquidityBySlotTxIndex");
                        });

                    b.Navigation("Amount")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
