# Snake Game - CLI Game Project

* **Version:** 4
* **Author:** Bohdan Morus
* **Date:** 18.10.2024

## Contents
#### 1. Document history
#### 2. Solution goals
#### 3. Functional requirements
#### 4. Non-functional requirements

## Document History
| Version | Author | Comment |
| --- | :----: | :---- |
| 1 | Bohdan Morus | First version of the SRS |
| 1.1 | Bohdan Morus | Fixed a typo |
| 2 | Bohdan Morus | Prioritized the requirements, added non-functional |
| 2.2 | Bohdan Morus | Correction - added "non-functional requirements" to contents |
| 3 | Bohdan Morus | Removed ambiguities in specifications |
| 4 | Bohdan Morus | Clarified some of the definitions |

## Solution Goals
* Create a free CLI game based on the popular game "Snake".
* Create a "go to solution" for wasting the time while waiting or procrastinating.
* Widen the amount of entertainment available though CLI (Command Line Interface).

## Functional Requirements - in order of priority
1. Controls - 4 buttons for different directions (up, down, left, right).
2. Simple goal - collecting food items on the gameboard. The one food item spawns randomly on any place on the gameboard, that is not occupied by the player.
   After being collected by the player - another food item spawns according to the same rules.
3. Score system.
4. Persistence of the score and other user information locally.
5. Different shapes of the gameboard (simple rectangle, square, "edgeless arena", etc.).

## Non-functional requirements - in order of priority
1. CLI - application does not need a full GUI. Can run on remote servers with only terminal access.
2. Wide range of supported platforms with .NET framework.
3. Local backing up of the saves - ensures data integrity when program crashes.
4. Deployed self-contained for linux and widows, pre-compiled for .net environments and as a source code repository.
5. Deployment as docker container and tarballs.
6. Deployment via MSI package for windows.
