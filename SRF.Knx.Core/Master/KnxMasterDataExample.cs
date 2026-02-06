namespace SRF.Knx.Core.Master;

/// <summary>
/// Example usage of KNX Master Data loader
/// </summary>
public static class KnxMasterDataExample
{
    /// <summary>
    /// Example: Load master data and display datapoint type information
    /// </summary>
    public static void DisplayDatapointTypes(string knxMasterXmlPath)
    {
        // Load the master data from XML file
        var masterData = KnxMasterDataLoader.LoadFromFile(knxMasterXmlPath);

        // Get all datapoint types
        var datapoints = KnxMasterDataLoader.GetDatapointTypes(masterData);

        Console.WriteLine($"Loaded {datapoints.Count} datapoint types");
        Console.WriteLine();

        // Display first few datapoint types
        foreach (var dpt in datapoints.Take(5))
        {
            Console.WriteLine($"DPT-{dpt.Number}: {dpt.Name} - {dpt.Text}");
            Console.WriteLine($"  Size: {dpt.SizeInBit} bits, PDT: {dpt.PDT}");
            
            var subtypes = dpt.DatapointSubtypes?.DatapointSubtype ?? [];
            Console.WriteLine($"  Subtypes: {subtypes.Count}");
            
            foreach (var dpst in subtypes.Take(3))
            {
                Console.WriteLine($"    DPST-{dpt.Number}-{dpst.Number}: {dpst.Name} - {dpst.Text}");
            }
            
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Example: Get specific datapoint type and analyze its format
    /// </summary>
    public static void AnalyzeDatapointType(string knxMasterXmlPath, int dptNumber, int dpstNumber)
    {
        var masterData = KnxMasterDataLoader.LoadFromFile(knxMasterXmlPath);
        
        // Get specific datapoint subtype
        var dpst = KnxMasterDataLoader.GetDatapointSubtype(masterData, dptNumber, dpstNumber);
        
        if (dpst == null)
        {
            Console.WriteLine($"DPST-{dptNumber}-{dpstNumber} not found");
            return;
        }

        Console.WriteLine($"Analyzing: {dpst.Id} - {dpst.Name}");
        Console.WriteLine($"Description: {dpst.Text}");
        Console.WriteLine();

        if (dpst.Format != null)
        {
            Console.WriteLine("Format Elements:");
            foreach (var element in dpst.Format.Elements)
            {
                switch (element)
                {
                    case BitFormat bit:
                        Console.WriteLine($"  Bit: {bit.Name} (Cleared: {bit.Cleared}, Set: {bit.Set})");
                        break;
                    case UnsignedIntegerFormat uintFmt:
                        Console.WriteLine($"  UnsignedInteger: {uintFmt.Name}, Width: {uintFmt.Width} bits, Unit: {uintFmt.Unit}");
                        if (uintFmt.CoefficientSpecified)
                            Console.WriteLine($"    Coefficient: {uintFmt.Coefficient}");
                        break;
                    case SignedIntegerFormat sint:
                        Console.WriteLine($"  SignedInteger: {sint.Name}, Width: {sint.Width} bits, Unit: {sint.Unit}");
                        break;
                    case FloatFormat flt:
                        Console.WriteLine($"  Float: {flt.Name}, Width: {flt.Width} bits, Unit: {flt.Unit}");
                        Console.WriteLine($"    Range: {flt.MinValue} to {flt.MaxValue}");
                        break;
                    case StringFormat str:
                        Console.WriteLine($"  String: {str.Name}, Width: {str.Width} bits, Encoding: {str.Encoding}");
                        break;
                    case EnumerationFormat enm:
                        Console.WriteLine($"  Enumeration: {enm.Name}, Width: {enm.Width} bits");
                        foreach (var val in enm.EnumValues)
                        {
                            Console.WriteLine($"    {val.Value}: {val.Text}");
                        }
                        break;
                    case ReservedFormat res:
                        Console.WriteLine($"  Reserved: {res.Width} bits (padding)");
                        break;
                    case RefTypeFormat rf:
                        Console.WriteLine($"  RefType: references {rf.RefId}");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Example: Search for datapoint types by name
    /// </summary>
    public static void SearchDatapointTypes(string knxMasterXmlPath, string searchTerm)
    {
        var masterData = KnxMasterDataLoader.LoadFromFile(knxMasterXmlPath);
        var datapoints = KnxMasterDataLoader.GetDatapointTypes(masterData);

        var results = datapoints
            .Where(dpt => dpt.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                         dpt.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Console.WriteLine($"Found {results.Count} datapoint types matching '{searchTerm}':");
        
        foreach (var dpt in results)
        {
            Console.WriteLine($"  {dpt.Id}: {dpt.Name} - {dpt.Text}");
        }
    }

    /// <summary>
    /// Example: Display property data types
    /// </summary>
    public static void DisplayPropertyDataTypes(string knxMasterXmlPath)
    {
        // Load the master data from XML file
        var masterData = KnxMasterDataLoader.LoadFromFile(knxMasterXmlPath);

        // Get all property data types
        var propertyTypes = KnxMasterDataLoader.GetPropertyDataTypes(masterData);

        Console.WriteLine($"Loaded {propertyTypes.Count} property data types");
        Console.WriteLine();

        // Display all property data types
        foreach (var pdt in propertyTypes)
        {
            var sizeInfo = pdt.HasSize ? $"{pdt.Size} bytes" : "variable length";
            var readSizeInfo = pdt.HasReadSize ? $", ReadSize: {pdt.ReadSize}" : "";
            Console.WriteLine($"{pdt.Id}: {pdt.Name} - Size: {sizeInfo}{readSizeInfo}");
        }
    }

    /// <summary>
    /// Example: Get specific property data type by name
    /// </summary>
    public static void GetPropertyDataType(string knxMasterXmlPath, string pdtName)
    {
        var masterData = KnxMasterDataLoader.LoadFromFile(knxMasterXmlPath);
        
        // Get specific property data type by name
        var pdt = KnxMasterDataLoader.GetPropertyDataTypeByName(masterData, pdtName);
        
        if (pdt == null)
        {
            Console.WriteLine($"Property data type '{pdtName}' not found");
            return;
        }

        Console.WriteLine($"Property Data Type: {pdt.Id}");
        Console.WriteLine($"Name: {pdt.Name}");
        Console.WriteLine($"Number: {pdt.Number}");
        
        if (pdt.HasSize)
        {
            Console.WriteLine($"Size: {pdt.Size} bytes");
        }
        else
        {
            Console.WriteLine("Size: Variable length");
        }

        if (pdt.HasReadSize)
        {
            Console.WriteLine($"Read Size: {pdt.ReadSize} bytes");
        }
    }
}
