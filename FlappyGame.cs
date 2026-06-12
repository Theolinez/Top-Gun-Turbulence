using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Silk.NET.SDL;

namespace TheAdventure;

public enum GameState { Start, Playing, GameOver, Win }

public record struct Rectangle(int X, int Y, int Width, int Height)
{
    public bool Intersects(Rectangle other) =>
        X < other.X + other.Width && X + Width > other.X &&
        Y < other.Y + other.Height && Y + Height > other.Y;
}

public class FlappyGame
{
    private GameState _state = GameState.Start;
    
    // Plane state 
    private double _birdY = 400;
    private double _birdVelocity = 0;
    private readonly int _birdX = 100;
    private readonly int _birdSize = 40; 

    // Difficulty & Contrail state
    private double _baseSpeed = 200;
    private double _speedMultiplier = 1.0;
    private readonly List<Rectangle> _contrails = new();
    private double _contrailTimer = 0;

    // Generics & Collections
    private readonly List<Pipe> _pipes = new();
    private double _pipeSpawnTimer = 0;
    private readonly Random _random = new();

    private int _score = 0;
    private int _highScore = 0;
    private readonly string _highScorePath = "highscore.txt";

    // Textures
    private unsafe Texture* _planeTexture = null;
    private unsafe Texture* _dayCloudTexture = null;
    private unsafe Texture* _dawnCloudTexture = null;
    private unsafe Texture* _nightCloudTexture = null;
    private unsafe Texture* _mainMenuTexture = null;
    private unsafe Texture* _gameOverTexture = null;
     private unsafe Texture* _gameWinTexture = null;
    private bool _assetsLoaded = false;

    // Pixel Font Map for Scores
    private static readonly byte[,,] Digits = new byte[10, 5, 3]
    {
        { {1,1,1}, {1,0,1}, {1,0,1}, {1,0,1}, {1,1,1} }, // 0
        { {0,1,0}, {0,1,0}, {0,1,0}, {0,1,0}, {0,1,0} }, // 1
        { {1,1,1}, {0,0,1}, {1,1,1}, {1,0,0}, {1,1,1} }, // 2
        { {1,1,1}, {0,0,1}, {1,1,1}, {0,0,1}, {1,1,1} }, // 3
        { {1,0,1}, {1,0,1}, {1,1,1}, {0,0,1}, {0,0,1} }, // 4
        { {1,1,1}, {1,0,0}, {1,1,1}, {0,0,1}, {1,1,1} }, // 5
        { {1,1,1}, {1,0,0}, {1,1,1}, {1,0,1}, {1,1,1} }, // 6
        { {1,1,1}, {0,0,1}, {0,0,1}, {0,0,1}, {0,0,1} }, // 7
        { {1,1,1}, {1,0,1}, {1,1,1}, {1,0,1}, {1,1,1} }, // 8
        { {1,1,1}, {1,0,1}, {1,1,1}, {0,0,1}, {1,1,1} }  // 9
    };

    private class Pipe
    {
        public double X { get; set; }
        public int GapTop { get; set; }
        public int GapHeight { get; } = 160;
        public int Width { get; } = 80; 
        public bool Passed { get; set; }

        public Rectangle TopRect => new((int)X, 0, Width, GapTop);
        public Rectangle BottomRect => new((int)X, GapTop + GapHeight, Width, 800 - (GapTop + GapHeight));
    }

    public FlappyGame()
    {
        if (File.Exists(_highScorePath))
        {
            if (int.TryParse(File.ReadAllText(_highScorePath), out var hs))
                _highScore = hs;
        }
        Console.WriteLine($"Welcome to Top Gun Turbulence! High Score: {_highScore}. Press SPACE to flap.");
    }
    //AI GENERATED SOLUTION
    // Manually load BMP files using SDL_RWops to bypass DllImport issues with SDL_LoadBMP
    private unsafe Surface* LoadBmpNative(Sdl sdl, string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Warning: Could not find {filePath}");
            return null;
        }

        // Get SDL_RWFromFile directly from Silk.NET's loaded library (Bypasses DllImport issues)
        var ptr = sdl.Context.GetProcAddress("SDL_RWFromFile");
        if (ptr == IntPtr.Zero)
        {
            Console.WriteLine("Failed to hook into SDL_RWFromFile.");
            return null;
        }

        var rwFromFile = (delegate* unmanaged[Cdecl]<byte*, byte*, RWops*>)ptr;

        // Convert C# strings to C strings
        byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(filePath + "\0");
        byte[] modeBytes = System.Text.Encoding.UTF8.GetBytes("rb\0");

        fixed (byte* fPtr = fileBytes)
        fixed (byte* mPtr = modeBytes)
        {
            var rwOps = rwFromFile(fPtr, mPtr);
            if (rwOps == null)
            {
                string err = Marshal.PtrToStringUTF8((IntPtr)sdl.GetError()) ?? "Unknown error";
                Console.WriteLine($"SDL Error opening {filePath}: {err}");
                return null;
            }

            var surface = sdl.LoadBMPRW(rwOps, 1);
            if (surface == null)
            {
                string err = Marshal.PtrToStringUTF8((IntPtr)sdl.GetError()) ?? "Unknown error";
                Console.WriteLine($"SDL Error parsing {filePath}: {err}");
            }
            return surface;
        }
    }
    //END OF AI GENERATED SOLUTION

    public unsafe void LoadAssets(Sdl sdl, Renderer* renderer)
    {
        if (_assetsLoaded) return;

        // Load Plane
        var planeSurface = LoadBmpNative(sdl, "plane.bmp");
        if (planeSurface != null)
        {
            sdl.SetColorKey(planeSurface, 1, sdl.MapRGB(planeSurface->Format, 255, 255, 255));
            _planeTexture = sdl.CreateTextureFromSurface(renderer, planeSurface);
            if (_planeTexture == null) Console.WriteLine($"Texture Error (plane): {Marshal.PtrToStringUTF8((IntPtr)sdl.GetError())}");
            sdl.FreeSurface(planeSurface);
        }

        // Load Menus
        var mainSurface = LoadBmpNative(sdl, "main.bmp");
        if (mainSurface != null) { _mainMenuTexture = sdl.CreateTextureFromSurface(renderer, mainSurface); sdl.FreeSurface(mainSurface); }

        var overSurface = LoadBmpNative(sdl, "game-over.bmp");
        if (overSurface != null) { _gameOverTexture = sdl.CreateTextureFromSurface(renderer, overSurface); sdl.FreeSurface(overSurface); }

        // Load Game Win Texture
        var winSurface = LoadBmpNative(sdl, "win.bmp");
        if (winSurface != null) { _gameWinTexture = sdl.CreateTextureFromSurface(renderer, winSurface); sdl.FreeSurface(winSurface); }

        // Load Clouds
        var daySurface = LoadBmpNative(sdl, "day-cloud.bmp");
        if (daySurface != null) { _dayCloudTexture = sdl.CreateTextureFromSurface(renderer, daySurface); sdl.FreeSurface(daySurface); }

        var dawnSurface = LoadBmpNative(sdl, "dawn-cloud.bmp");
        if (dawnSurface != null) { _dawnCloudTexture = sdl.CreateTextureFromSurface(renderer, dawnSurface); sdl.FreeSurface(dawnSurface); }

        var nightSurface = LoadBmpNative(sdl, "night-cloud.bmp");
        if (nightSurface != null) { _nightCloudTexture = sdl.CreateTextureFromSurface(renderer, nightSurface); sdl.FreeSurface(nightSurface); }

        _assetsLoaded = true;
    }

    public void HandleInput(KeyCode key)
    {
        if (key != KeyCode.Space) return;
        switch (_state)
        {
            case GameState.Start:
            case GameState.GameOver:
                ResetGame();
                _state = GameState.Playing;
                break;
            case GameState.Playing:
                _birdVelocity = -350; 
                break;
            case GameState.Win:
                ResetGame();
                _state = GameState.Playing;
                break;          
        }
    }

    private void ResetGame()
    {
        _birdY = 400;
        _birdVelocity = 0;
        _pipes.Clear();
        _contrails.Clear();
        _score = 0;
        _pipeSpawnTimer = 0;
    }

    public async Task UpdateAsync(double dt)
    {
        if (_state != GameState.Playing) return;

        // Calculate Difficulty
        if (_score >= 6)
            _speedMultiplier = 1.7; // Speed 2
        else if (_score >= 3)
            _speedMultiplier = 1.25;   // Speed 1
        else
            _speedMultiplier = 1.0;    // Base

        double currentSpeed = _baseSpeed * _speedMultiplier;

        // Apply gravity
        _birdVelocity += 1000 * dt; 
        _birdY += _birdVelocity * dt;

        // Spawn new clouds
        _pipeSpawnTimer -= dt;
        if (_pipeSpawnTimer <= 0)
        {
            _pipes.Add(new Pipe { X = 800, GapTop = _random.Next(100, 450) });
            _pipeSpawnTimer = 1.6 / _speedMultiplier; 
        }

        // Update clouds and score
        foreach (var pipe in _pipes)
        {
            pipe.X -= currentSpeed * dt; 

            if (!pipe.Passed && pipe.X + pipe.Width < _birdX)
            {
                pipe.Passed = true;
                _score++;
                Console.WriteLine($"Score: {_score}");
            }
        }

        if (_score >= 10)
        {
            _state = GameState.Win;
            if (_score > _highScore)
            {
                _highScore = _score;
                await File.WriteAllTextAsync(_highScorePath, _highScore.ToString());
            }
            return;
        }

        // Update Contrail
        _contrailTimer -= dt;
        if (_contrailTimer <= 0)
        {
            _contrails.Add(new Rectangle(_birdX - 5, (int)_birdY + (_birdSize / 2), 10, 4));
            _contrailTimer = 0.05;
        }

        for (int i = 0; i < _contrails.Count; i++)
        {
            var c = _contrails[i];
            _contrails[i] = new Rectangle(c.X - (int)(currentSpeed * dt), c.Y, c.Width, c.Height);
        }

        // Cleanup off-screen items
        _pipes.RemoveAll(p => p.X < -100);
        _contrails.RemoveAll(c => c.X < 0);

        // Check Collisions
        var birdRect = new Rectangle(_birdX, (int)_birdY, _birdSize, _birdSize);
        bool hitPipe = _pipes.Any(p => birdRect.Intersects(p.TopRect) || birdRect.Intersects(p.BottomRect));
        bool hitBounds = _birdY < 0 || _birdY > 800;  

        if (hitPipe || hitBounds)
        {
            _state = GameState.GameOver;
            if (_score > _highScore)
            {
                _highScore = _score;
                await File.WriteAllTextAsync(_highScorePath, _highScore.ToString());
            }
        }
    }

    private unsafe void DrawNumber(Sdl sdl, Renderer* renderer, int number, int startX, int startY, int pixelSize)
    {
        string numStr = number.ToString();
        int currentX = startX;
        sdl.SetRenderDrawColor(renderer, 255, 255, 255, 255); 

        foreach (char c in numStr)
        {
            int digit = c - '0'; 
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (Digits[digit, y, x] == 1)
                    {
                        var rect = new Silk.NET.Maths.Rectangle<int>(currentX + (x * pixelSize), startY + (y * pixelSize), pixelSize, pixelSize);
                        sdl.RenderFillRect(renderer, ref rect);
                    }
                }
            }
            currentX += (2 * pixelSize) + pixelSize; 
        }
    }

    public unsafe void Render(Sdl sdl, Renderer* renderer)
    {
        // Draw the Backgrounds
        if (_state == GameState.Start)
        {
            if (_mainMenuTexture != null)
                sdl.RenderCopy(renderer, _mainMenuTexture, null, null); 
            else
            {
                sdl.SetRenderDrawColor(renderer, 50, 50, 60, 255); 
                sdl.RenderClear(renderer);
            }
        }
        else if (_state == GameState.GameOver)
        {
            if (_gameOverTexture != null)
                sdl.RenderCopy(renderer, _gameOverTexture, null, null);
            else
            {
                sdl.SetRenderDrawColor(renderer, 150, 50, 50, 255); 
                sdl.RenderClear(renderer);
            }
        }
        else if (_state == GameState.Win)
        {
            if (_gameWinTexture != null)
                sdl.RenderCopy(renderer, _gameWinTexture, null, null);
            else
            {
                sdl.SetRenderDrawColor(renderer, 50, 150, 50, 255);
                sdl.RenderClear(renderer);
            }
        }
        else // Playing State
        {
            byte bgR = 135, bgG = 206, bgB = 235; // Default: Sky Blue
            Texture* currentCloudTex = _dayCloudTexture;

            if (_score >= 6)
            {
                bgR = 20; bgG = 24; bgB = 82; // Night Sky
                currentCloudTex = _nightCloudTexture;
            }
            else if (_score >= 3)
            {
                bgR = 247; bgG = 194; bgB = 186; // Sunset
                currentCloudTex = _dawnCloudTexture;
            }

            sdl.SetRenderDrawColor(renderer, bgR, bgG, bgB, 255); 
            sdl.RenderClear(renderer);

            // Render Contrails
            sdl.SetRenderDrawColor(renderer, 200, 200, 200, 255);
            foreach (var c in _contrails)
            {
                var rect = new Silk.NET.Maths.Rectangle<int>(c.X, c.Y, c.Width, c.Height);
                sdl.RenderFillRect(renderer, ref rect);
            }

            // Render Cloud Pipes
            foreach (var pipe in _pipes)
            {
                var topRect = new Silk.NET.Maths.Rectangle<int>(pipe.TopRect.X, pipe.TopRect.Y, pipe.TopRect.Width, pipe.TopRect.Height);
                var botRect = new Silk.NET.Maths.Rectangle<int>(pipe.BottomRect.X, pipe.BottomRect.Y, pipe.BottomRect.Width, pipe.BottomRect.Height);
                
                if (currentCloudTex != null)
                {
                    sdl.RenderCopy(renderer, currentCloudTex, null, ref topRect);
                    sdl.RenderCopy(renderer, currentCloudTex, null, ref botRect);
                }
                else
                {
                    sdl.SetRenderDrawColor(renderer, 255, 255, 255, 255);
                    sdl.RenderFillRect(renderer, ref topRect);
                    sdl.RenderFillRect(renderer, ref botRect);
                }
            }

            // Render Plane
            var bRect = new Silk.NET.Maths.Rectangle<int>(_birdX, (int)_birdY, _birdSize, _birdSize);
            if (_planeTexture != null)
                sdl.RenderCopy(renderer, _planeTexture, null, ref bRect);
            else
            {
                sdl.SetRenderDrawColor(renderer, 255, 215, 0, 255);
                sdl.RenderFillRect(renderer, ref bRect);
            }
        }

        // Draw Dynamic UI Numbers
        if (_state == GameState.Playing)
        {
            DrawNumber(sdl, renderer, _score, 20, 20, 6);
        }
        else if (_state == GameState.GameOver)
        {
            DrawNumber(sdl, renderer, _score, 525, 10, 10);
            DrawNumber(sdl, renderer, _highScore, 580, 600, 8);
        }
    }
}