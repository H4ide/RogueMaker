# Specification of the final project for relevant C# courses

## C# Courses selection

- [x] NPRG035 (Programming in C# language | Programování v jazyce C#)
- [x] NPRG038 (Advanced C# Programming | Pokročilé programování v jazyce C#)
- [ ] NPRG057 (Advanced .NET Programming II | Pokročilé programování pro .NET II)
- [ ] NPRG064 (Programming user interfaces in .NET | Programování uživatelských rozhraní v .NET)

## Specification

### Rogue Maker: 2D top-down puzzle video game with Level Builder

Game will have 2 mode: Level Builder where you create map (add enemies/ objects / walls,floor, from selected list of objects). And 2nd is the Game Mode where you actually try to play levels you have created. The goal is to find the exit or kill all enemies with minimum amount of actions. After player make an action, based on the world state, all enemies decide what to do based on their AI (e.g. are they they just waiting Ai, chasers, random wanderer, have some predefined route), so, basically player do his acion - everything else comes to life and do their action.
 - Motivation: I liked the turn-synchronous concept in *Crypt of the Necrodancer* https://store.steampowered.com/app/247080/Crypt_of_the_NecroDancer/, but I am bad at rhythm games. So in my game, you can take as much time as you want to think before making your move.
 - Use case scenarios: Have fun. Create level and try to beat it with lowest amount of actions as possible.
 - Main Features: All logic is inside C# scipts. Godot is mostly just UI that draws map/animation/buttons/Menu that catches all info from C# scripts. I will try to solve all Enemy actions in one turn asynchoniuosly.
 - UI/UX: Godot App. User build level and play it in the same app.
 - Persistence: JSON data - Level Builder saves data about the level using JsonSerializer (how map looks like, where are creatures located) and that level can be loaded from the game menu.
 - Libraries/Technologies: Godot
 - Testing: Main logic (game rules, map, etc) will have xUnit tests that will try to cover base and edge cases. UI part will be tested using manual tests (describe in words or diagrams) what we expected and whether that is what actually happened. Main goal is to make game playable and at least a bit interesting.

