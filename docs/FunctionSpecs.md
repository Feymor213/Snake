# Functional specifications

* **Version:** 3
* **Author:** Bohdan Morus
* **Date:** 8.11.2024

## Contents
#### 1. Document history
#### 2. Controls
#### 3. Interface design
#### 4. Character and asset design
#### 5. Gameplay
#### 6. Different levels
#### 7. Persistence

## Document History
| Version | Author | Comment |
| --- | :----: | :---- |
| 1 | Bohdan Morus | First version of the Document |
| 2 | Bohdan Morus | Added the gameplay, levels and persistence description |
| 3 | Bohdan Morus | Tweaked visual diagrams. Removed layous "square" and "edgeless" |

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

Main menu will consist of three buttons: **PLAY**, diffuculty selector (possible values are **EASY**, **MEDUIM** and **HARD**), level shape selector and **EXIT**. They will be positioned vertically in the center of a viewport. The user will be able to cycle through the buttons using the *down arrow* and *up arrow* keys.
The selected option will be higlighted using the * (asterisk) characters on both sides of the button.
The █ characters indicate the edge of the viewport.

The maximum score achieved by the user on the selected level layout is displayed in the top left corner of the viewport. It persists between the game sessions and program restarts.

#### Example
```text
HIGH SCORE: 69420
SCORE: 679
████████████████████████████████████████████████████████████████████████████████████
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                      PLAY                                        █
█                                    * EASY *                                      █
█                                   Level: BOX                                     █
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
Snake going right: ----> 
Snake going left:  <---- 

Up & down:
| ^
| |
v |

Turns from left to down, down to right, right to up, up to left respectfully:

--|        -->            ^            |
  |        |              |            |
  v        |              |--        <--

Food: *

Snake approaching the food:  ----> *

Edge of arena: ██████

```

## Gameplay

After pressing "Start" the gameplay starts. The user interface consists of two text labels showing the maximim and the current score and the gameboard.

#### Example
```text
HIGH SCORE: 69420
SCORE: 679
████████████████████████████████████████████████████████████████████████████████████
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                                            *                     █
█                                                                                  █
█                                                                                  █
█                                                            ^                     █
█                                                            |                     █
█                                                            |                     █
█                                                        ----|                     █
█                                                                                  █
█                                                                                  █
████████████████████████████████████████████████████████████████████████████████████
```

#### Game over

When the snake hits its owh body or the edge of the board - the game stops. Current and maximum score labels remain, the snake sprite dissapears and the text "Game over" shows up in the middle of the viewport. After pressing the ***Return*** key the user is sent back to the main menu.

```text
HIGH SCORE: 69420
SCORE: 679
████████████████████████████████████████████████████████████████████████████████████
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                   * GAME OVER *                                  █
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                                                                  █
█                                                                                  █
████████████████████████████████████████████████████████████████████████████████████
```

## Different levels

The level design ideas (scaled down).

```text
BOX:
██████████████████████████████
█                            █
█                            █
█                            █
█                            █
█                            █
██████████████████████████████

BRIDGE:
██████████         ███████████
█        █         █         █
█        ███████████         █
█                            █
█        ███████████         █
█        █         █         █
██████████         ███████████

PANTS:
███████████████████████████████
█                             █
█                             █
█         ███████████         █
█         █         █         █
█         █         █         █
█         █         █         █
█         █         █         █
█         █         █         █
███████████         ███████████

```

## Persistence

The game saves the maximum score the player achieves on each board layout. The value is written to after every game, that ended with the score higher that the previous maximum. The value is read from upon the rendering of the main menu. The save is backed-up
