# Top Gun Turbulence

A fast-paced flight action game built in C# using SDL2.

## Game Description

Top Gun Turbulence is an action-packed aerial combat game inspired by classic flight simulators. Navigate your aircraft through challenging scenarios, dodge obstacles, and test your piloting skills. The game features smooth controls, dynamic gameplay, and persistent high-score tracking.

## Setup Instructions

### Prerequisites
- .NET 10.0 or higher
- SDL2 libraries

### Build & Run

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the game
dotnet run
```

The game will launch and automatically load the high score from `highscore.txt`.

## Gameplay Rules

- **Controls**: Use mouse and keyboard to control your aircraft
  - Mouse: Aim and maneuver
  - Keyboard: Accelerate, brake, and special actions

- **Objective**: Survive as long as possible while avoiding obstacles and enemies

- **Scoring**: Earn points for successful maneuvers and enemy eliminations

- **High Scores**: Your best score is automatically saved to `highscore.txt`

- **Game Over**: Collision with obstacles or enemy fire ends the game

## Project Structure

- `Program.cs` - Main entry point
- `FlappyGame.cs` - Core game logic and state management
- `SdlContext.cs` - SDL2 rendering and input handling
- `KeyCodes.cs` & `MouseButton.cs` - Input constant definitions
- `TheAdventure.csproj` - Project configuration

---

Enjoy the skies! ✈️
