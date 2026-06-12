using System;
using System.Diagnostics;
using Silk.NET.SDL;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());
        var ev = new Event();

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer);
        if (sdlInitResult < 0) throw new InvalidOperationException("Failed to initialize SDL.");

        IntPtr window;
        IntPtr renderer;
        unsafe
        {
            // 1. Create the Window
            window = (IntPtr)sdl.CreateWindow("Top Gun Turbulence", Sdl.WindowposUndefined, Sdl.WindowposUndefined, 800, 800, (uint)WindowFlags.Shown);
            
            // 2. Create the Renderer (This is the Graphics Card hook!)
            renderer = (IntPtr)sdl.CreateRenderer((Window*)window, -1, (uint)RendererFlags.Accelerated);
            
            if (renderer == IntPtr.Zero) throw new Exception("Failed to create renderer.");
            sdl.RenderSetVSync((Renderer*)renderer, 1);
        }

        // 3. Initialize Game
        var game = new FlappyGame();

        // 4. LOAD ASSETS (Must happen AFTER CreateRenderer, but BEFORE the game loop)
        unsafe 
        {
            game.LoadAssets(sdl, (Renderer*)renderer);
        }

        var timer = new Stopwatch();
        timer.Start();

        bool quit = false;
        while (!quit)
        {
            while (sdl.PollEvent(ref ev) != 0)
            {
                if (ev.Type == (uint)EventType.Quit) quit = true;
                
                // Route Input to Game
                if (ev.Type == (uint)EventType.Keydown)
                {
                    game.HandleInput((KeyCode)ev.Key.Keysym.Scancode);
                }
            }

            // Calculate DeltaTime for smooth movement
            double dt = timer.Elapsed.TotalSeconds;
            timer.Restart();

            // Run the game update step
            game.UpdateAsync(dt).GetAwaiter().GetResult();

            // Render the frame
            unsafe
            {
                var r = (Renderer*)renderer;
                game.Render(sdl, r);
                sdl.RenderPresent(r);
            }
        }

        // Clean up when the game closes
        unsafe
        {
            sdl.DestroyRenderer((Renderer*)renderer);
            sdl.DestroyWindow((Window*)window);
        }
        sdl.Quit();
    }
}