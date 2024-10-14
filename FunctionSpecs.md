# Functional specifications

* **Version:** 1
* **Author:** Bohdan Morus
* **Date:** 14.10.2024

## Contents
#### 1. Document history
#### 2. Controls
#### 3. Interface design
#### 4. Character and asset design

## Document History
| Version | Author | Comment |
| --- | :----: | :---- |
| 1 | Bohdan Morus | First version of the Document |

## Controls

#### Gameplay
The game needs only 4 buttons during the gameplay: one to turn in any direction

- UP: ***up arrow*** or ***W***
- DOWN: ***down arrow*** or ***S***
- RIGHT: ***right arrow*** or ***D***
- LEFT: ***left arrow*** or ***A***

Arrow keys or WASD can be used interchangably and are identical in their function.

#### Menu navigation

In-game menus will consist of several option positioned vertically in the center of the viewport. The user will be able to cycle through the different option using the ***down arrow*** and ***up arrow*** keys.
The selected option will be higlighted using the * symbols on both sides of the button.
To activate the selected option user must press the ***Return*** key.


## Interface design

### Main menu

Main menu will consist of three buttons: **PLAY**, diffuculty selector (possible values are **EASY**, **MEDUIM** and **HARD**) and **EXIT**. They will be positioned vertically in the center of a viewport. The user will be able to cycle through the buttons using the *down arrow* and *up arrow* keys.
The selected option will be higlighted using the * (asterisk) characters on both sides of the button.
The █ characters indicate the edge of the viewport.

The maximum score achieved by the user is displayed in the top left corner of the viewport. It persists between the game sessions and program restarts.

#### Example
```text
████████████████████████████████████████████████████████████████████████████████████
█  MAX SCORE: 69420                                                                █
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                      PLAY                                        █
█                                    * EASY *                                      █
█                                      EXIT                                        █
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                                                                  █
████████████████████████████████████████████████████████████████████████████████████
```

## Character and asset design
```text
Snake going right: - - - - > 
Snake going left:  < - - - - 

Up & down:
| ^
| |
v |

Turns from left to down, down to right, right to up, up to left respectfully:

- - |        - - >          ^                |
    |        |              |                |
    v        |              | - -        < - -

Food: *

Snake approaching the food:  - - - - > *

Edge of arena: ██████

```
