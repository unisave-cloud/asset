Unisave Fixture
===============

This folder acts as a game placeholder. It should contain all the code
of a game built on top of Unisave. Therefore its structure mirrors that
of a game:

```
- UnisaveFixture
    - Backend
    - Scripts
    - Tests
        - Unit
        - Integration
```

But in fact is does not contain a game but a set of tests that verify
Unisave's features. There are two parts that are verified:

- **Core** - Facets can be requested, entities saved. Verified against
  the real server - no simulation or mocking should happen anywhere.
- **Templates** - Tested via unit and integration tests that allow for
  mocking. Tested the same way a game would be.

The following is a list of sub-modules of this fixture together with
their purpose:


### Core

Contains code that test the Unisave system full-stack. It does no
mocking and tests core features on top of which other systems are built.


### Example testing

Provides an example integration test. Its purpose is to test integration
testing itself.


### Steam microtransactions

Tests the template for Steam microtransactions.
