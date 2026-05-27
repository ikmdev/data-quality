using System;
using Microsoft.EntityFrameworkCore;

namespace PIQI.Data
{
    /// <summary>
    /// Example seed data class to show how you could populate the database with initial reference data.
    /// </summary>
    public static class SeedData
    {

        public static void Seed(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<PrimaryUnitMart>().HasData(
            //    new PrimaryUnitMart
            //    {
            //        Id = 1,
            //        ContentSetMnemonic = "PC_UOM",
            //        CodeSystemMnemonic = "REGEN_LOINC",
            //        CodeValue = "100374-8",
            //        UOMText = "[GPL'U]"
            //    }
            //    // Add more PrimaryUnitMart records here
            //);

            //modelBuilder.Entity<RangeSetMart>().HasData(
            //    new RangeSetMart
            //    {
            //        Id = 1,
            //        ContentSetMnemonic = "PC_UOM_VALUE_RANGE",
            //        CodeSystemMnemonic = "REGEN_LOINC",
            //        CodeValue = "2075-0",
            //        UOMText = "mmol/L",
            //        MinValue = 0,
            //        MaxValue = 400
            //    }
            //    // Add more RangeSetMart records here
            //);

            //modelBuilder.Entity<TextMart>().HasData(
            //    new TextMart
            //    {
            //        Id = 1,
            //        ContentSetMnemonic = "UCUM",
            //        TextValue = "mg/L"
            //    }
            //    // Add more TextMart records here
            //);
        }
    }
}