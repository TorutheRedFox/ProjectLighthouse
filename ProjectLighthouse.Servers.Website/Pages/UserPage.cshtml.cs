#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class UserPage : BaseLayout
{
    public Dictionary<CommentEntity, RatedCommentEntity?> Comments = new();

    public bool CommentsEnabled;

    public bool IsProfileUserHearted;

    public bool IsProfileUserBlocked;

    public List<PhotoEntity>? Photos;
    public List<SlotEntity>? Slots;

    public List<SlotEntity>? HeartedSlots;
    public List<SlotEntity>? QueuedSlots;

    public UserEntity? ProfileUser;
    public UserPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int userId)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        // Determine if user can view profile according to profileUser's privacy settings
        if (this.User == null || !this.User.IsAdmin)
        {
            switch (this.ProfileUser.ProfileVisibility)
            {
                case PrivacyType.PSN:
                {
                    if (this.User != null) return this.NotFound();

                    break;
                }
                case PrivacyType.Game:
                {
                    if (this.ProfileUser != this.User) return this.NotFound();

                    break;
                }
                case PrivacyType.All: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        this.Photos = await this.Database.Photos.Include(p => p.Slot)
            .Include(p => p.PhotoSubjects)
            .ThenInclude(ps => ps.User)
            .OrderByDescending(p => p.Timestamp)
            .Where(p => p.CreatorId == userId)
            .Take(6)
            .ToListAsync();

        this.Slots = await this.Database.Slots.Include(p => p.Creator)
            .OrderByDescending(s => s.LastUpdated)
            .Where(p => p.CreatorId == userId)
            .Take(10)
            .ToListAsync();

        if (this.User == this.ProfileUser)
        {
            this.QueuedSlots = await this.Database.QueuedLevels.Include(h => h.Slot)
                .Where(q => this.User != null && q.UserId == this.User.UserId)
                .OrderByDescending(q => q.QueuedLevelId)
                .Select(q => q.Slot)
                .Where(s => s.Type == SlotType.User)
                .Take(10)
                .ToListAsync();
            this.HeartedSlots = await this.Database.HeartedLevels.Include(h => h.Slot)
                .Where(h => this.User != null && h.UserId == this.User.UserId)
                .OrderByDescending(h => h.HeartedLevelId)
                .Select(h => h.Slot)
                .Where(s => s.Type == SlotType.User)
                .Take(10)
                .ToListAsync();
        }

        this.CommentsEnabled = ServerConfiguration.Instance.UserGeneratedContentLimits.LevelCommentsEnabled && this.ProfileUser.CommentsEnabled;

        if (this.CommentsEnabled)
        {
            List<int> blockedUsers = this.User == null ? new List<int>() : await 
            (from blockedProfile in this.Database.BlockedProfiles
                where blockedProfile.UserId == this.User.UserId
                select blockedProfile.BlockedUserId).ToListAsync();
            
            this.Comments = await this.Database.Comments.Include(p => p.Poster)
                .OrderByDescending(p => p.Timestamp)
                .Where(p => p.TargetId == userId && p.Type == CommentType.Profile)
                .Where(p => !blockedUsers.Contains(p.PosterUserId))
                .Take(50)
                .ToDictionaryAsync(c => c, _ => (RatedCommentEntity?) null);
        }
        else
        {
            this.Comments = new Dictionary<CommentEntity, RatedCommentEntity?>();
        }

        if (this.User == null) return this.Page();

        foreach (KeyValuePair<CommentEntity, RatedCommentEntity?> kvp in this.Comments)
        {
            RatedCommentEntity? reaction = await this.Database.RatedComments.Where(r => r.CommentId == kvp.Key.CommentId)
                .Where(r => r.UserId == this.User.UserId)
                .FirstOrDefaultAsync();
            this.Comments[kvp.Key] = reaction;
        }

        this.IsProfileUserHearted = await this.Database.HeartedProfiles
            .Where(h => h.HeartedUserId == this.ProfileUser.UserId)
            .Where(h => h.UserId == this.User.UserId)
            .AnyAsync();

        this.IsProfileUserBlocked = await this.Database.IsUserBlockedBy(this.ProfileUser.UserId, this.User.UserId);
        
        return this.Page();
    }
}