# Checkers GoDot

A local two-player checkers game developed using **Godot 4 and C#**, featuring rule enforcement, customisable appearances, player profiles, and an interactive game interface.

## Features

### Gameplay
- Local two-player matches
- Standard diagonal movement and captures
- Mandatory capture enforcement
- Continued captures and multiple jumps
- King promotion and backward movement for kings
- Automatic winner detection, including when an opponent has no legal moves
- Legal-move and capture indicators
- Last-move highlighting

### Customisation
- Custom player names
- Adjustable checker-piece colours
- Five board themes: Classic, Ocean, Forest, Slate and Royal
- Persistent appearance and audio preferences

### Interface
- Main menu and player setup
- Piece counters and match scores
- Move and capture statistics
- Pause menu with Escape-key support
- Restart and return-to-menu controls
- How to Play instructions
- Move, capture and victory sound effects

## Technologies

- **Engine:** Godot 4 (.NET)
- **Language:** C#
- **Interface:** Godot UI and custom drawing
- **Version Control:** Git and GitHub

## Getting Started

1. Clone or download this repository.
2. Install Godot 4 with C#/.NET support and a compatible .NET SDK.
3. Open `project.godot` in Godot.
4. Build the C# project.
5. Run the project using **F5**.

## Controls

- **Left Mouse Button:** Select pieces and move to legal destinations.
- **Escape:** Open or close the pause menu.

Captures are mandatory when available. If a piece can capture another opposing piece after jumping, the capture sequence must continue.

## Project Structure

- `Game/CheckersGame.cs` — Board state, movement rules, captures and winner detection
- `Main.cs` — Game presentation, menus and interface management
- `UI/PauseMenu.cs` — Pause-menu behaviour
- `CheckerPiece.cs` — Checker-piece model
- `CheckerSquare.cs` — Square rendering and visual indicators
- `PlayerProfile.cs` — Player information and match scores
- `Assets/` — Visual assets
- `Audio/` — Gameplay sound effects

## Development Status

**Playable (ongoing development)**

The current version supports local two-player gameplay with the core rules and interface implemented. Further improvements and additional features are planned.

## Author

Developed by [Mikhaeel1710](https://github.com/Mikhaeel1710).
