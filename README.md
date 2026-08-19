# wormwars

A Unity project.

## What's in this repo

This repo is a ready-to-use starting point for a Unity project, set up with source control best practices already in place:

- `Assets/` — your game's scripts, scenes, art, audio, prefabs, and materials, organized into subfolders
- `Packages/` — Unity's package manager files (Unity will fill this in automatically)
- `ProjectSettings/` — Unity's project configuration (Unity will fill this in automatically)
- `.gitignore` — tells git to ignore folders Unity regenerates on its own (`Library/`, `Temp/`, `Build/`, etc.), so they never get committed
- `.gitattributes` — normalizes line endings for code, and routes binary assets (images, audio, models, fonts) through [Git LFS](https://git-lfs.com) so the repo stays small

Empty folders contain a `.gitkeep` file just so git tracks them — feel free to delete a `.gitkeep` once you've added real files to that folder.

## Getting started (beginner-friendly walkthrough)

1. **Install Unity Hub** from [unity.com/download](https://unity.com/download) if you don't already have it, and install a Unity Editor version through it.
2. **Open this folder as your project.** In Unity Hub, click *Open* → *Add project from disk*, and select this `wormwars` folder. Unity will detect it's an empty project shell and generate the `ProjectSettings` and `Packages/manifest.json` files for you the first time it opens.
3. **(Optional but recommended) Install Git LFS** so large art/audio files are handled properly instead of bloating the repo:
   ```
   git lfs install
   ```
   Run this once per computer, before you add any binary assets.

## Pushing this to GitHub

This repo has already been initialized locally with git and an initial commit. To connect it to GitHub:

1. Create a new, empty repository on GitHub named `wormwars` (don't initialize it with a README, license, or .gitignore — this repo already has those).
2. In a terminal, inside this project folder, run:
   ```
   git remote add origin https://github.com/<your-username>/wormwars.git
   git branch -M main
   git push -u origin main
   ```
   Replace `<your-username>` with your actual GitHub username.

After that first push, you can commit and push future changes the normal way:
```
git add .
git commit -m "your message here"
git push
```

## Notes for later

- Never commit your `Library/`, `Temp/`, or `Build/` folders — they're already ignored, and committing them causes painful merge conflicts.
- If you add plugins or packages through Unity's Package Manager, `Packages/manifest.json` and `Packages/packages-lock.json` should be committed — they're small text files that record what's installed.
- Consider adding a `LICENSE` file once you've decided how you want to share (or not share) this project.
