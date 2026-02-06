using System;

namespace SRF.Knx.Config.OpenHab.Templating;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class OHConfigParamAttribute(string Name, bool Optional = true) : Attribute
{
    public string Name { get; } = Name;
    public bool Optional { get; } = Optional;
}
