# Settlement and Raider Base Handoff

Both scenes are isolated from `Overworld.unity` and are intended to be loaded from the world map when scene transitions are wired.

## Settlement

`Settlement.unity` is a safe hub with three small homes, a Salvage & Supply shop, a Clinic, and a central market square. Systems should use these anchors:

- `SettlementSpawn`
- `ShopCounter_Salvage`, `ShopCounter_Clinic`
- `TownLoot_Bandage`, `TownLoot_Ammo`

## Raider Base

`RaiderBase.unity` is a ruined shipping yard. Each named tier has a spawn anchor, an enemy-count anchor, and an ammo-loot anchor. The intended encounter curve is:

| Tier | Area | Enemy count | Pace | Recommended drops |
| --- | --- | ---: | --- | --- |
| 1 | Lookouts | 2 | Slow patrol introduction | Bandage, 9mm rounds |
| 2 | Wreckers | 3 | Mixed patrols | Ammo, canned food |
| 3 | Garage | 4 | Crossfire lanes | Ammo crate, pistol chance |
| 4 | Barracks | 5 | Fast response | Bandage, pistol ammo |
| 5 | Boss Stash | 6 | High-pressure finale | Ammo crate, weapon, medical cache |

The `RaiderTier*_Spawn_*` and `RaiderTier*_LootAmmo` anchors are placement-only. A spawn/drop system can read their names or replace them with prefabs without changing the surrounding layout.
