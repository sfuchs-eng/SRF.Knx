using System;

namespace SRF.Knx.Core.DPT;

public class DptMemoryCache(IDptFactory dptFactory) : IDptFactory
{
    private readonly IDptFactory _dptFactory = dptFactory;
    private readonly Dictionary<DataPointTypeId, DptBase> _cache = [];

    public DptBase Get(int main, int sub)
    {
        return Get(new DataPointTypeId(main, sub));
    }

    public DptBase Get(DataPointTypeId dpstId)
    {
        if (_cache.TryGetValue(dpstId, out var dpt))
            return dpt;

        dpt = _dptFactory.Get(dpstId);
        _cache[dpstId] = dpt;
        return dpt;
    }
}
