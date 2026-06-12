# Top Gun Turbulence

Top Gun Turbulence is a Flappy Bird based game. You are a plane that has to avoid clouds in order to arrive safely, pass through the day to the night and reach the highest score in order to win the game.

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

- **Controls**: Use the space bar

- **Objective**: Survive as long as possible while avoiding obstacles and enemies

- **How to win**: Have a score higher than 10.

- **Scoring**: Earn points for every cloud passed

- **High Scores**: Your best score is automatically saved to `highscore.txt`

- **Game Over**: Collision with obstacles or enemy fire ends the game

## AI Usage

* **Tools used:** Gemini (Model Version: Gemini 1.5 Pro), Copilot (Claude Haiku 4.5)
* **How it was used:** Chat-based code suggestions, troubleshooting Silk.NET/SDL2 native pointer interop issues (specifically overcoming missing SDL macros for reading .bmp files from memory)
* **Fully AI-generated regions:** * `Silk.NET/SDL2 Interop Section` (Entirely AI-generated)