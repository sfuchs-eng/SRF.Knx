using System;

namespace SRF.Knx.Core.DPT;

public abstract class DptBase
{
    public required DataPointTypeId Id { get; init; }

    public abstract object ToValue(GroupValue groupValue);

    public abstract GroupValue ToGroupValue(object value);
}
