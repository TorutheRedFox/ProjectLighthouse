﻿using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

public struct ApiSlot
{
    public int SlotId { get; set; }
    public string Name { get; set; }
    public string IconHash { get; set; }
    public bool TeamPick { get; set; }
    public bool IsAdventure { get; set; }
    public Location Location { get; set; }
    public GameVersion GameVersion { get; set; }
    public long FirstUploaded { get; set; }
    public long LastUpdated { get; set; }
    public int Plays { get; set; }
    public int PlaysUnique { get; set; }
    public int PlaysComplete { get; set; }
    public bool CommentsEnabled { get; set; }

    public static ApiSlot CreateFromEntity(SlotEntity slot) =>
        new()
        {
            SlotId = slot.SlotId,
            Name = slot.Name,
            IconHash = slot.IconHash,
            TeamPick = slot.TeamPick,
            IsAdventure = slot.IsAdventurePlanet,
            Location = slot.Location,
            GameVersion = slot.GameVersion,
            FirstUploaded = slot.FirstUploaded,
            LastUpdated = slot.LastUpdated,
            Plays = slot.Plays,
            PlaysUnique = slot.PlaysUnique,
            PlaysComplete = slot.PlaysComplete,
            CommentsEnabled = slot.CommentsEnabled,
        };
}