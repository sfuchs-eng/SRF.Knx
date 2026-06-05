using System;

namespace SRF.Knx.Core.DPT;

public class DptMemoryCache(IDptFactory dptFactory) : IDptFactory
{
    private readonly IDptFactory _dptFactory = dptFactory;
    private readonly Dictionary<DataPointTypeId, DptBase> _cache = [];
    private readonly object _lock = new();

    public DptBase Get(int main, int sub)
    {
        return Get(new DataPointTypeId(main, sub));
    }

    public DptBase Get(DataPointTypeId dpstId)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(dpstId, out var dpt))
                return dpt;

            dpt = _dptFactory.Get(dpstId);
            _cache[dpstId] = dpt;
            return dpt;
        }
    }

    public DptBase Get(string dptId)
    {
        var dpstId = new DataPointTypeId(dptId);
        return Get(dpstId);
    }
}
