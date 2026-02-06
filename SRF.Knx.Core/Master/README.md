# SRF.Knx.Core.Master

This namespace provides entity models and XML deserialization functionality for loading KNX master data from the `knx_master.xml` file, version 23 (latest as of 2026-02)

## Overview

The KNX master data contains definitions of all standard datapoint types (DPT) and their subtypes (DPST), including format specifications, units, ranges, and encoding information.

## Entity Model

### Core Classes

- **`KnxMasterData`**: Root element representing the entire XML structure
- **`MasterData`**: Container for master data with ID, version, and signature
- **`DatapointTypes`**: Collection of all datapoint types
- **`DatapointType`**: Represents a DPT (e.g., DPT-1, DPT-9) with metadata
- **`DatapointSubtype`**: Represents a DPST (e.g., DPST-1-1, DPST-9-1) with specific format

### Format Elements

The `Format` class contains a collection of format elements that define the binary structure:

- **`BitFormat`**: Single bit with cleared/set text values
- **`UnsignedIntegerFormat`**: Unsigned integer with width, unit, coefficient, and range
- **`SignedIntegerFormat`**: Signed integer with width, unit, coefficient, and range
- **`FloatFormat`**: Floating-point number with width, unit, and value range
- **`StringFormat`**: String with width and encoding (ASCII, ISO-8859-1)
- **`EnumerationFormat`**: Enumeration with named values
- **`ReservedFormat`**: Reserved bits (padding)
- **`RefTypeFormat`**: Reference to another format element

## Usage Examples

### Load Master Data

```csharp
using SRF.Knx.Core.Master;

// Load from file
var masterData = KnxMasterDataLoader.LoadFromFile("path/to/knx_master.xml");

// Load from stream
using var stream = File.OpenRead("knx_master.xml");
var masterData = KnxMasterDataLoader.LoadFromStream(stream);

// Load from string
var xmlContent = File.ReadAllText("knx_master.xml");
var masterData = KnxMasterDataLoader.LoadFromString(xmlContent);
```

### Query Datapoint Types

```csharp
// Get all datapoint types
var allDpts = KnxMasterDataLoader.GetDatapointTypes(masterData);
Console.WriteLine($"Total DPTs: {allDpts.Count}");

// Get specific DPT by number
var dpt1 = KnxMasterDataLoader.GetDatapointTypeByNumber(masterData, 1);
Console.WriteLine($"{dpt1.Id}: {dpt1.Text}");

// Get specific DPT by ID
var dpt9 = KnxMasterDataLoader.GetDatapointTypeById(masterData, "DPT-9");
Console.WriteLine($"DPT-9 has {dpt9.DatapointSubtypes?.DatapointSubtype.Count} subtypes");

// Get specific DPST
var dpst = KnxMasterDataLoader.GetDatapointSubtype(masterData, 1, 1); // DPST-1-1
Console.WriteLine($"{dpst.Name}: {dpst.Text}");
```

### Analyze Format Structure

```csharp
// Get DPST-1-1 (DPT_Switch)
var switchType = KnxMasterDataLoader.GetDatapointSubtype(masterData, 1, 1);

if (switchType?.Format != null)
{
    foreach (var element in switchType.Format.Elements)
    {
        if (element is BitFormat bit)
        {
            Console.WriteLine($"Bit: {bit.Cleared} / {bit.Set}");
        }
    }
}

// Get DPST-9-1 (DPT_Value_Temp - Temperature)
var tempType = KnxMasterDataLoader.GetDatapointSubtype(masterData, 9, 1);

if (tempType?.Format != null)
{
    foreach (var element in tempType.Format.Elements)
    {
        if (element is FloatFormat flt)
        {
            Console.WriteLine($"Float: {flt.Width} bits");
            Console.WriteLine($"Unit: {flt.Unit}");
            Console.WriteLine($"Range: {flt.MinValue} to {flt.MaxValue}");
        }
    }
}
```

### Enumerate All Subtypes

```csharp
var datapoints = KnxMasterDataLoader.GetDatapointTypes(masterData);

foreach (var dpt in datapoints)
{
    Console.WriteLine($"\n{dpt.Id} - {dpt.Text} ({dpt.SizeInBit} bits)");
    
    var subtypes = dpt.DatapointSubtypes?.DatapointSubtype ?? [];
    foreach (var dpst in subtypes)
    {
        var defaultMarker = dpst.Default ? " [DEFAULT]" : "";
        Console.WriteLine($"  {dpst.Id}: {dpst.Name} - {dpst.Text}{defaultMarker}");
    }
}
```

## Notes

- The XML namespace is defined as `http://knx.org/xml/project/23`
- Boolean attributes like `Default` use the `*Specified` pattern for optional XML serialization
- Format elements are polymorphic and can be of various types (Bit, Integer, Float, etc.)
- The loader provides helper methods to simplify common queries
- All classes are in the `SRF.Knx.Core.Master` namespace
