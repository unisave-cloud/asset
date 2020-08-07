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
        - Fullstack
```

But in fact is does not contain a game but a set of tests that verify
Unisave's features. There are two parts that are verified:

- **Core** - Facets can be requested, entities saved. These tests are
  inside folders named `Core`.
- **Templates** - Tested primarily via unit and integration tests that
  allow for mocking. Usually tested the same way a game would be.

The `Fullstack` testing suite is not present in a game. It's present
here to thoroughly test the Unisave core.

The following is a list of sub-modules of this fixture together with
their purpose:


### Core

Tests the core functionality of Unisave on top which anyone should be
able to build.


### Example testing

Provides an example test. Its purpose is to test testing itself.


### Steam microtransactions

Tests the template for Steam microtransactions.
