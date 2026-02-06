using System;
using System.Text.Json.Serialization;

namespace SRF.Knx.Config.Domain;

/// <summary>
/// Reflects a group of Group Addresses belonging together.
/// In the future things might be reflected as .NET objects, providing additional code level functionality.
/// </summary>
public class Thing : GroupAddressExtraConfigJsonContainer
{
    public string Name { get; set; } = string.Empty;
}
