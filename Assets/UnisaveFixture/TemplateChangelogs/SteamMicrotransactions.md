# Steam microtransactions


## 0.9.2

Added ID of the authenticated player's entity.

What has changed:

- The `SteamTransactionEntity` now contains the field `public string
  authenticatedPlayerId;`
- The field is assigned in
  `SteamPurchasingServerFacet.InitiateTransaction` right after
  transaction validation and it is assigned to the current `Auth.Id()`
  value.


## 0.9.0

This is the first version of this template.


### Changes

- Everything


### Upgrade guide

- Just install the template
