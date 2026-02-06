# SRF.Knx.Config TODO

## Backlog

### Required

- [x] De-conflict openhab baseconfig and metaconfig --> turned metaconfig into templating and dpt mapping
- [x] Correct Thing association in Domain configuration creation from ETS export, no loose group addresses `SRF.Knx.Config.Domain.DomainConfigurationFactory`
- [x] Correct Thing association in OpenHAB configuration generation from Domain configuration, `SRF.Knx.Config.OpenHab.OpenHabKnxConfigFactory`
- [ ] Get rid of the Falcon SDK dependency: ran into limitations and struggles with it. Decided to make Falcon SDK
usage optional, moving dependent code into separate libraries in case I might need it one day.

### Should have

- [ ] Use `SRF.Knx.Config.OpenHab.Templating.ConfigTemplatesManager` for Item configuration overrides in OpenHAB configuration generation.

### Nice to have

- [ ] Use `SRF.Knx.Config.OpenHab.Templating.DptMappingLookupItem` for Channel type and parameter determination in OpenHAB configuration generation in `SRF.Knx.Config.OpenHab.OpenHabKnxConfigFactory`
- [ ] Ensure overriding legacy imports too ?? Some settings at least?
- [ ] Domain config delta update from ETS export `SRF.Knx.Config.Domain.DomainConfigurationFactory` and implementations of `SRF.Knx.Config.Domain.ConfigModifiers.IDomainConfigModifier`
- [ ] Implement delta update of OpenHAB KNX configuration based on JsonNode diffing, `SRF.Knx.Config.OpenHab.BaseConfig.Modifiers.IOpenHabKnxBaseConfigModifier` and implementations.
