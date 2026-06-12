* **Tools used:** Gemini (Model Version: Gemini 1.5 Pro), Copilot (Claude Haiku 4.5)
* **How it was used:** Chat-based code suggestions, troubleshooting Silk.NET/SDL2 native pointer interop issues (specifically overcoming missing SDL macros for reading .bmp files from memory)
restoring deleted work.
* **Fully AI-generated regions:** * `Silk.NET/SDL2 Interop Section` (Entirely AI-generated)

Since we used SDL2 and have .bmp files for the textures, backgrounds etc, the Silk.NET wrapper was missing the standard SDL_LoadBMP shortcut macro. To fix it, we used GetProcAddress to dynamically hook into the native C library. We converted our C# strings into null-terminated C byte arrays, used the fixed keyword to pin them in managed memory so the Garbage Collector wouldn't move them, and passed those raw, pinned pointers directly into SDL's native file reader.

I initially just copied the main code from git and after I made my project in the IDE I just put it in a branch on a fork of the project. Problem was that git did not recognise the history of my branch and did not allow me to do a pull request, in the effort of trying to fix this I managed to delete all my work. This is the reason why you will see the commit mention of -Restored work, I used copilot to help me undo my mistake.