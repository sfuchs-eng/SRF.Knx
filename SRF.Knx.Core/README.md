# SRF.Knx.Core

This library is neither affiliated with nor endorsed by the KNX Association.

It is an independent minimalistic implementation of KNX DPT handling based on the publicly available KNX master data XML.

The library allows KNX Datapoint Type (DPT) encoding, decoding, and formatting. Provides `IDptFactory` as the primary entry point for converting between typed .NET values and the raw `GroupValue` wrapped byte arrays exchanged on the KNX bus.

No external KNX SDK dependency — pure .NET 10 with `Microsoft.Extensions.*`.

## Concepts

| Term | Description |
|---|---|
| `GroupValue` | Wrapper for the raw byte array as transmitted on the KNX bus |
| `DptBase` | Resolved DPT instance with encode/decode/format capability |
| `DataPointTypeId` | Identifies a DPT by main/sub number (e.g. `9.001` = temperature) |
| `NumericInfo` | Unit, min, and max metadata for numeric DPTs (e.g. `°C`, `-273 – 670760`) |
| `IDptFactory` | Resolves a `DptBase` for a given main/sub number using KNX master data |
| `IKnxMasterDataProvider` | Your app-supplied service that loads the `knx_master.xml` file |

## Setup

### 1. Implement `IKnxMasterDataProvider`

Derive from the abstract `KnxMasterDataProvider` base class and load the KNX master XML:

```csharp
using SRF.Knx.Core.Master;

public class MyMasterDataProvider : KnxMasterDataProvider
{
    private readonly IOptions<MyOptions> _options;

    public MyMasterDataProvider(IOptions<MyOptions> options) => _options = options;

    public override KnxMasterData GetMasterData()
        => GetMasterDataFromFile(_options.Value.KnxMasterXmlPath);
}
```

> The `knx_master.xml` file is distributed by the KNX Association and shipped with ETS or available from the KNX member portal.

### 2. Register services

```csharp
// Register your master data provider first, then call AddKnxCore()
services.AddSingleton<IKnxMasterDataProvider, MyMasterDataProvider>();
services.AddKnxCore();   // registers IDptFactory, IPdtEncoderFactory, IDptNumericInfoFactory
```

`AddKnxCore()` registers all three internal components as singletons:

| Service | Description |
|---|---|
| `IDptFactory` | Resolves DPT instances from master data — **the main consumer-facing service** |
| `IPdtEncoderFactory` | Provides raw PDT encoders/decoders (internal use) |
| `IDptNumericInfoFactory` | Supplies unit and range info per DPT (internal use) |

## Usage

### Resolve a DPT

```csharp
public class TemperatureService(IDptFactory dptFactory)
{
    // DPST-9-1 = DPT_Value_Temp (℃, 2-byte KNX float)
    private readonly DptBase _tempDpt = dptFactory.Get(9, 1);
}
```

You can also parse a string identifier:

```csharp
var id = new DataPointTypeId("9.001");   // or "DPST-9-1", "DPT-9"
var dpt = dptFactory.Get(id.Main, id.Sub);
```

### Decode a `GroupValue` received from the bus

```csharp
// GroupValue arrives from the KNX bus (e.g. via IKnxBus.MessageReceived)
GroupValue received = new GroupValue(new byte[] { 0x0C, 0xE2 });

// Generic (object) API — suitable when type is unknown at compile time
object value = _tempDpt.ToValue(received);   // returns float (boxed)

// Typed API — cast to DptSimple<T> when the CLR type is known
var typed = (DptSimple<float>)_tempDpt;
float temperature = typed.Decode(received);  // 22.5f
```

### Encode a value to send on the bus

```csharp
// Generic API
GroupValue wire = _tempDpt.ToGroupValue(21.5f);

// Typed API
GroupValue wire = typed.Encode(21.5f);
```

### Format a value for display

```csharp
// Format with culture-specific number formatting
string display = _tempDpt.Format(received, "de", CultureInfo.GetCultureInfo("de-DE"));
// → "22,5"
```

### Show unit and range information

Use `NumericInfo` (available on all numeric DPTs) to enrich display strings or validate user input:

```csharp
if (_tempDpt is DptSimple simple && simple.IsNumeric)
{
    NumericInfo info = simple.NumericInfo!;

    Console.WriteLine($"Unit   : {info.Unit}");        // °C
    Console.WriteLine($"Minimum: {info.MinValue}");    // -273.0
    Console.WriteLine($"Maximum: {info.MaxValue}");    // 670760.0
}
```

### Full example: display a received KNX telegram

```csharp
public class KnxTelegramDisplay(IDptFactory dptFactory)
{
    public string Render(int dptMain, int dptSub, GroupValue value)
    {
        DptBase dpt = dptFactory.Get(dptMain, dptSub);

        string formatted = dpt.Format(value, null, CultureInfo.CurrentCulture);

        if (dpt is DptSimple { IsNumeric: true } numeric)
            return $"{formatted} {numeric.NumericInfo!.Unit}";

        return formatted;
    }
}
```

## Supported Encodings

`IDptFactory` resolves any DPT whose property data type (PDT) is known to the built-in encoder factory:

| KNX PDT | .NET Type | Size |
|---|---|---|
| `PDT_BOOLEAN` / 1-bit group values | `bool` | 1 bit (1 byte on wire) |
| `PDT_UNSIGNED_CHAR` | `byte` | 1 byte |
| `PDT_CHAR` | `sbyte` | 1 byte |
| `PDT_UNSIGNED_INT` | `ushort` | 2 bytes |
| `PDT_INT` | `short` | 2 bytes |
| `PDT_KNX_FLOAT` | `float` | 2 bytes (KNX mantissa/exponent format) |
| `PDT_UNSIGNED_LONG` | `uint` | 4 bytes |
| `PDT_LONG` | `int` | 4 bytes |
| `PDT_FLOAT` | `float` | 4 bytes (IEEE 754) |
| `PDT_DOUBLE` | `double` | 8 bytes |
| `PDT_CHAR_BLOCK` | `string` | 10 bytes ASCII |
| `PDT_VARIABLE_LENGTH` / `PDT_GENERIC_*` | `byte[]` | variable |

## Master data

See [Master/README.md](Master/README.md) for details on loading and querying the KNX master XML directly (e.g. to enumerate all available DPTs or inspect format structures).
