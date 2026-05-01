# SRF.Knx.Config

Library to deal with KNX configuration files such as e.g. ETS Group Address exports, XML version of ETS 5.
Older ETS version exports are not supported, newer ones not yet. There are presently no plans to support KNX IoT neither.

It's developed to configure [OpenHAB](https://www.openhab.org/) for KNX systems and is used by `SRF.Network.Knx`.
Fairly complex to install and use, ties to legacy code and ETS projects. Check whether [KNX to openhab generator](https://github.com/maxpautsch/knx_to_openhab) suits you better.

See the command line tool `SRF.Network.Cli` in the [SRF.Network](https://github.com/sfuchs-eng/SRF.Network) solution supporting direct use of the library at hands.

## Usage

### Repo branches and depending libraries

- `SRF.Network.Knx` and the related command line tool `SRF.Network.Cli` depend on the branch `ForFalconPath`. That branch contains massive structural and functional modifications compared to its originating branch main/master.
- there are legacy libraries (not published) depending on the `legacy` branch which was formerly called `main` and `master`. Those branches are discontinued.

### One time setup per KNX ETS project

- Pool your config files in a separate git repository, using the templates from the [Resources](./Resources/) folder of this repo. This allows you to keep track of changes in your KNX configuration and to easily update your OpenHAB configuration when you change the KNX configuration in ETS.:
  - Get the `project-20` folder in which there's the `knx_master.xml` file from your ETS installation. Copy the entire folder.
  - Update [OpenHabItemTemplates.json](./Resources/OpenHabItemTemplates.json) in that new folder to suit your Group Address naming patterns.
  - The mapping file [OpenHabDptMappings.json](./Resources/OpenHabDptMappings.json) should not require adaptions.
  - Export the Group Address configuration from ETS 5 to the XML format export file.
  ETS 4 won't work due to missing DPT information. ETS 6 could not be tested yet.
  - Adjust [OpenHabItemTemplates.json](./Resources/OpenHabItemTemplates.json) to the needs of your project(s)
- Ensure the configuration settings of `KnxConfiguration` are reflected either in your appsettings.json or in a user specific configuration file. If you're using `SRF.Network.Cli`, the latter is the recommended approach, using a file named
`SRF.Network.json` placed in the AppData directory, e.g. [/home/sfuchs/.config/SRF.Network.json](/home/sfuchs/.config/SRF.Network.json). By default, `KnxConfiguration` is bound to config section `Knx`.

Example file SRF.Network.json in the AppData directory:

```json
{
    "Knx": {
        "ConnectionString": "Type=IpRouting",
        "EtsGAExportFile": "/home/sfuchs/src/knx-master/XProj_GroupAddressesETS.xml",
        "KnxMasterFolder": "/home/sfuchs/src/knx-master/project-20/",
        "KnxDomainConfigFile": "/home/sfuchs/src/knx-master/XProj_KnxDomainConfig.json",
        "HomeCompanionCodeGenFile": "/home/sfuchs/src/HomeCompanion/HomeCompanion.Knx/KnxValues.generated.cs",
        "OpenHab": {
            "BaseConfigFile": "/home/sfuchs/src/knx-master/XProj_OpenHabKnxConfig.json",
            "TemplatesFolder": "/home/sfuchs/src/knx-master/OpenHabTemplates",
            "OHConfigRoot": "/home/sfuchs/src/openhab.git"
        }
    }
}
```

whereas the folders `knx-master` and `openhab.git` I manage as dedicated git repos. The latter corresponds to an [OpenHAB](https://www.openhab.org/) installation's `conf` folder.

### ETS based configuration

- ensure Group Addresses have a DPT configured. Channel / Item types and unit dimensions are derived from those.
- use the Group Address Label to steer OpenHab entity model associations as follows:
  - Thing name is the label before `+`; if there's no separator, a key word based thing name determination is tried before the full label is used as thing name.
  - optional: Channel name is between `+` and `#`
  - optional: The channel parameter name is the single word following `#`
  - optional: `[%.1f m/s]` or similar strings in `[]` at the end of the Label are passed to the OpenHAB label and UoM system

### Pulling updates from ETS to OpenHAB

1. Update the ETS Group Address export file.
1. Update the Domain configurations: run `SRF.Network.Cli` (command ...). Then edit the Domain Meta configuration files and adjust new names if required.
1. Update the OpenHab Meta Configuration: also via `SRF.Network.Cli` (command ...), editing automatically generated additions as required. Set the `EntryStatus` to `Manual`, preventing automatic overrides on the next run.
1. Use `SRF.Network.Cli` to update the OpenHAB configuration files (things and items)

Run the updater and just check diffs in your config git repo for proper change propagation along the chain

1. ETS export: ETS GA export, xml file version 5
1. Domain meta config: -u
1. OpenHAB meta config: -om
1. OpenHAB config: -o

### Generating KnxValues.generated.cs for HomeCompanion

`KnxValues.generated.cs` is a git-ignored partial class file that gives the HomeCompanion project typed, IDE-visible `ValueBase<T>` properties for every KNX group address. Regenerate it whenever the KNX domain configuration changes.

Prerequisites:

- `HomeCompanionCodeGenFile` is set in your local `SRF.Network.json` to the absolute path of `HomeCompanion.Knx/KnxValues.generated.cs` in your HomeCompanion checkout.
- The domain configuration (`KnxDomainConfigFile`) is up to date.

For updating after changes in the ETS export, the recommended procedure is:

1. Update the domain configuration from the ETS export file, using `srf-network-cli kc -u` (or `--update-domain-config`).
2. Generate the new `KnxValues.generated.cs` file, using `srf-network-cli kc -hc` (or `--home-companion-code-gen`).

### Mechanism

Applied procedure, for each Group Address newly taken from ETS into domain and OpenHAB configurations:

1. Update the domain configuration
    1. Determine Thing name based on ETS GA Label using [`IThingNameExtractor`](./Domain/IThingNameExtractor.cs). The default implementation [`DefaultLabelToNameConverter`](./Domain/DefaultLabelToNameConverter.cs) uses the separator tokens acc. section ETS configuration above.
    1. IMPL: Associate GA to Thing. If there is already another GA with the same name, enumerate the name until it's distinct
1. Update the OpenHAB meta configuration
    1. Use [OpenHabDptMappings.json](./Resources/OpenHabDptMappings.json) to determine channel type, parameter and UoM dimension
    1. Use [OpenHabItemTemplates.json](./Resources/OpenHabItemTemplates.json) to override config if the group address ETS config matches
1. Produce the desired output, e.g. OpenHAB thing and item config files, HomeCompanion `KnxValues.generated.cs`.
