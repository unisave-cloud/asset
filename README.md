Unisave
=======

Cool package. See the examples.

## Todo

- LoadAs attribute for loading only, but not saving (primarily for cloud)
- Component that can save other component's fields


## Cloud flow

*With extra login scene, other scenes already logegd in only*

- login in first scene, pull data
- go to second scene
- awake - distribute data
- ((( continuous saving & explicit saving )))
- call logout - collect data
- logout successful, go back to the first scene

*Explicit saving*

- collect data SYNC
- make saving request ASYNC

> data collection should be quick --> keep references on distribution

*Login within single scene*

- login, pull data
- trigger distribution after data available
- now it's the same


### Data stages

    Pull --> Distribute --> Mutate --> Collect --> Push

Pull:
After clicking the login button; user expects an async operation, no scenes disappear or simmilar crap.

Distribution:
- On awake for login-only scenes.
- By sending an event for login scenes.

> Key holds a reference to its user. Throw warning on key access from multiple places.

Mutation:
- Simply by setting marked fields
- Or direct editing. This sidesteps distributon. Beware of value sharing (everywhere, but here especially).

Collect:
- Quickly from references kept during distribution
- Memory leak? If used wrongly, warn user, mark only fields on long-living game objects

> Optimization: save only changed fields

Push:
- Explicitly
- On logout (expects an async operation, but shouldn't be blocking UI... non-destroyable GO?)
- On app quit cannot be done. It's too fast for a coroutine to start.
