using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ScytheButler.Data;
using ScytheButler.data.DatabaseModels.Lfr;

namespace ScytheButler.Services
{
    public class LfrService
    {
        private readonly AppDbContext _db;

        /*
         * ============================================================
         * RAID TYPES
         * ============================================================
         */
        public const string ToA = "ToA";
        public const string ToB = "ToB";
        public const string HMT = "HMT";
        public const string CoX = "CoX";
        public const string CmCoX = "CM CoX";
        public const string TFA = "TFA";

        /*
         * ============================================================
         * TOB / HMT ROLES
         * ============================================================
         */
        public const string Mage = "Mage";
        public const string NFRZ = "NFRZ";
        public const string RDPS = "RDPS";
        public const string MDPS = "MDPS";
        public const string MDPS2 = "MDPS2";

        public LfrService(AppDbContext db)
        {
            _db = db;
        }

        /*
         * ============================================================
         * CREATE PARTY
         * ============================================================
         *
         * Creating an LFR does NOT make the creator a member.
         *
         * There is intentionally NO check here preventing the creator
         * from creating multiple active LFRs.
         */
        public async Task<LfrParty> CreatePartyAsync(
            long guildId,
            long channelId,
            long messageId,
            long creatorId,
            string raidType)
        {
            ValidateRaidType(raidType);

            var teamSize =
                GetDefaultTeamSize(raidType);

            return await CreatePartyInternalAsync(
                guildId,
                channelId,
                messageId,
                creatorId,
                raidType,
                teamSize);
        }

        /*
         * ============================================================
         * CREATE PARTY WITH TEAM SIZE
         * ============================================================
         */
        public async Task<LfrParty> CreatePartyAsync(
            long guildId,
            long channelId,
            long messageId,
            long creatorId,
            string raidType,
            int teamSize)
        {
            ValidateRaidType(raidType);

            ValidateTeamSize(
                raidType,
                teamSize);

            return await CreatePartyInternalAsync(
                guildId,
                channelId,
                messageId,
                creatorId,
                raidType,
                teamSize);
        }

        /*
         * ============================================================
         * CREATE PARTY INTERNAL
         * ============================================================
         */
        private async Task<LfrParty> CreatePartyInternalAsync(
            long guildId,
            long channelId,
            long messageId,
            long creatorId,
            string raidType,
            int teamSize)
        {
            var party = new LfrParty
            {
                GuildId = guildId,
                ChannelId = channelId,
                MessageId = messageId,
                CreatorId = creatorId,
                RaidType = raidType,
                TeamSize = teamSize,
                IsClosed = false,
                CreatedAt = DateTime.UtcNow,
                ClosedAt = null
            };

            _db.LfrParties.Add(party);

            await _db.SaveChangesAsync();

            return party;
        }

        /*
         * ============================================================
         * GET PARTY
         * ============================================================
         */
        public async Task<LfrParty?> GetPartyAsync(
            int partyId)
        {
            return await _db.LfrParties
                .Include(p => p.Members)
                .FirstOrDefaultAsync(
                    p => p.Id == partyId);
        }

        /*
         * ============================================================
         * UPDATE PARTY MESSAGE ID
         * ============================================================
         *
         * Used by the Bump button.
         *
         * The LFR message is deleted and recreated in Discord, so
         * the database must point at the new Discord message.
         */
        public async Task UpdatePartyMessageIdAsync(
            int partyId,
            long newMessageId)
        {
            var party =
                await _db.LfrParties
                    .FirstOrDefaultAsync(
                        p => p.Id == partyId);

            if (party == null)
            {
                throw new InvalidOperationException(
                    "That LFR party no longer exists.");
            }

            party.MessageId =
                newMessageId;

            await _db.SaveChangesAsync();
        }

        /*
         * ============================================================
         * JOIN GENERIC PARTY
         * ============================================================
         */
        public async Task JoinPartyAsync(
            int partyId,
            long userId,
            string username)
        {
            var party =
                await GetPartyAsync(
                    partyId);

            if (party == null)
            {
                throw new InvalidOperationException(
                    "That LFR party does not exist.");
            }

            if (party.IsClosed)
            {
                throw new InvalidOperationException(
                    "This LFR party is closed.");
            }

            /*
             * A user can only actively belong to one LFR.
             *
             * The creator does NOT count unless they actually joined.
             */
            if (await IsUserInAnyActivePartyAsync(userId))
            {
                throw new InvalidOperationException(
                    "You are already a member of another active LFR party. " +
                    "Leave that party before joining another.");
            }

            /*
             * Prevent duplicate membership in this party.
             */
            if (party.Members.Any(
                    m => m.UserId == userId))
            {
                throw new InvalidOperationException(
                    "You are already in this LFR party.");
            }

            /*
             * ToA is limited to 8 players.
             *
             * CoX and CM CoX are treated as unlimited.
             */
            if ((party.RaidType == ToA || party.RaidType == TFA) &&
                party.Members.Count >= 8)
            {
                throw new InvalidOperationException(
                    "This Raid party is already full.");
            }

            if (IsRoleBasedRaid(party.RaidType))
            {
                throw new InvalidOperationException(
                    "This raid requires you to select a role.");
            }

            var member = new LfrPartyMember
            {
                LfrPartyId = party.Id,
                UserId = userId,
                Username = username,
                Role = null,
                JoinedAt = DateTime.UtcNow
            };

            _db.LfrPartyMembers.Add(member);

            /*
             * ToA automatically closes at 8 players.
             */
            if (party.RaidType == ToA &&
                party.Members.Count + 1 >= 8)
            {
                party.IsClosed = true;
                party.ClosedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        /*
         * ============================================================
         * JOIN WITH ROLE
         * ============================================================
         */
        public async Task JoinWithRoleAsync(
            int partyId,
            long userId,
            string username,
            string role)
        {
            var party =
                await GetPartyAsync(
                    partyId);

            if (party == null)
            {
                throw new InvalidOperationException(
                    "That LFR party does not exist.");
            }

            if (party.IsClosed)
            {
                throw new InvalidOperationException(
                    "This LFR party is closed.");
            }

            if (!IsRoleBasedRaid(
                    party.RaidType))
            {
                throw new InvalidOperationException(
                    "This raid does not use role selection.");
            }

            if (!IsValidRole(
                party.TeamSize,
                role))
            {
                throw new InvalidOperationException(
                    "That role is not available for this team size.");
            }

            var existingMember =
                party.Members.FirstOrDefault(
                    m => m.UserId == userId);

            /*
             * If the user is already in this party, allow them to
             * change their role, provided the requested role is free.
             */
            if (existingMember != null)
            {
                if (existingMember.Role == role)
                {
                    throw new InvalidOperationException(
                        $"You are already assigned as **{role}**.");
                }

                var roleTaken =
                    party.Members.Any(
                        m =>
                            m.UserId != userId &&
                            string.Equals(
                                m.Role,
                                role,
                                StringComparison.OrdinalIgnoreCase));

                if (roleTaken)
                {
                    throw new InvalidOperationException(
                        $"The **{role}** role is already taken.");
                }

                existingMember.Role = role;

                await _db.SaveChangesAsync();

                return;
            }

            /*
             * User is not in this party, so enforce the global
             * one-active-party membership rule.
             */
            if (await IsUserInAnyActivePartyAsync(userId))
            {
                throw new InvalidOperationException(
                    "You are already a member of another active LFR party. " +
                    "Leave that party before joining another.");
            }

            /*
             * Make sure the role isn't already occupied.
             */
            var roleAlreadyTaken =
                party.Members.Any(
                    m =>
                        string.Equals(
                            m.Role,
                            role,
                            StringComparison.OrdinalIgnoreCase));

            if (roleAlreadyTaken)
            {
                throw new InvalidOperationException(
                    $"The **{role}** role is already taken.");
            }

            /*
             * Make sure the party isn't already full.
             */
            if (party.Members.Count >= party.TeamSize)
            {
                throw new InvalidOperationException(
                    "This LFR party is already full.");
            }

            var member = new LfrPartyMember
            {
                LfrPartyId = party.Id,
                UserId = userId,
                Username = username,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };

            _db.LfrPartyMembers.Add(member);

            /*
             * Automatically close the party when it becomes full.
             */
            if (party.Members.Count >= party.TeamSize)
            {
                party.IsClosed = true;
                party.ClosedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        /*
         * ============================================================
         * LEAVE PARTY
         * ============================================================
         *
         * If the party auto-closed because it was full, leaving
         * reopens it.
         *
         * If the creator manually closed it, leaving does NOT reopen it.
         */
        public async Task LeavePartyAsync(
            int partyId,
            long userId)
        {
            var party =
                await GetPartyAsync(
                    partyId);

            if (party == null)
            {
                throw new InvalidOperationException(
                    "That LFR party does not exist.");
            }

            var member =
                party.Members.FirstOrDefault(
                    m => m.UserId == userId);

            if (member == null)
            {
                throw new InvalidOperationException(
                    "You are not a member of this LFR party.");
            }

            var memberCount =
                party.Members.Count;

            var wasFull =
                IsLimitedParty(party) &&
                memberCount >= party.TeamSize;

            _db.LfrPartyMembers.Remove(member);

            /*
             * Only reopen if the party was full immediately before
             * the member left.
             *
             * This means manually closed parties stay closed.
             */
            if (party.IsClosed &&
                wasFull)
            {
                party.IsClosed = false;
                party.ClosedAt = null;
            }

            await _db.SaveChangesAsync();
        }

        /*
         * ============================================================
         * CLOSE PARTY
         * ============================================================
         *
         * Only the creator can manually close their LFR.
         */
        public async Task ClosePartyAsync(
            int partyId,
            long userId)
        {
            var party =
                await GetPartyAsync(
                    partyId);

            if (party == null)
            {
                throw new InvalidOperationException(
                    "That LFR party does not exist.");
            }

            if (party.CreatorId != userId)
            {
                throw new InvalidOperationException(
                    "Only the creator of this LFR can close it.");
            }

            if (party.IsClosed)
            {
                throw new InvalidOperationException(
                    "This LFR is already closed.");
            }

            party.IsClosed = true;
            party.ClosedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        /*
         * ============================================================
         * BUMP PARTY
         * ============================================================
         *
         * Discord message recreation is performed in LfrCommand.
         *
         * This method validates that the user is allowed to bump
         * the party, while the command handles Discord itself.
         */
        public async Task BumpPartyAsync(
            int partyId,
            long userId)
        {
            var party =
                await GetPartyAsync(
                    partyId);

            if (party == null)
            {
                throw new InvalidOperationException(
                    "That LFR party does not exist.");
            }

            if (party.IsClosed)
            {
                throw new InvalidOperationException(
                    "This LFR party is closed.");
            }

            if (party.CreatorId != userId)
            {
                throw new InvalidOperationException(
                    "Only the creator of this LFR can bump it.");
            }
        }

        /*
         * ============================================================
         * GET ACTIVE PARTY FOR USER
         * ============================================================
         */
        public async Task<LfrParty?> GetActivePartyForUserAsync(
            long userId)
        {
            return await _db.LfrParties
                .Include(p => p.Members)
                .Where(p => !p.IsClosed)
                .FirstOrDefaultAsync(
                    p => p.Members.Any(
                        m => m.UserId == userId));
        }

        /*
         * ============================================================
         * CHECK ACTIVE MEMBERSHIP
         * ============================================================
         *
         * Creator status does NOT count as membership.
         */
        public async Task<bool> IsUserInAnyActivePartyAsync(
            long userId)
        {
            return await _db.LfrPartyMembers
                .Include(m => m.LfrParty)
                .AnyAsync(
                    m =>
                        m.UserId == userId &&
                        !m.LfrParty.IsClosed);
        }

        /*
         * ============================================================
         * GET ACTIVE PARTIES CREATED BY USER
         * ============================================================
         *
         * A user may own/create unlimited active LFRs.
         */
        public async Task<List<LfrParty>>
            GetActivePartiesCreatedByUserAsync(
                long userId)
        {
            return await _db.LfrParties
                .Include(p => p.Members)
                .Where(
                    p =>
                        p.CreatorId == userId &&
                        !p.IsClosed)
                .OrderByDescending(
                    p => p.CreatedAt)
                .ToListAsync();
        }

        /*
         * ============================================================
         * GET ACTIVE PARTY CREATED BY USER
         * ============================================================
         *
         * Kept for compatibility with existing code.
         */
        public async Task<LfrParty?>
            GetActivePartyCreatedByUserAsync(
                long userId)
        {
            return await _db.LfrParties
                .Include(p => p.Members)
                .Where(
                    p =>
                        p.CreatorId == userId &&
                        !p.IsClosed)
                .OrderByDescending(
                    p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /*
         * ============================================================
         * RAID VALIDATION
         * ============================================================
         */
        private static void ValidateRaidType(
            string raidType)
        {
            if (raidType != ToA &&
                raidType != TFA &&
                raidType != ToB &&
                raidType != HMT &&
                raidType != CoX &&
                raidType != CmCoX)
            {
                throw new InvalidOperationException(
                    "Invalid raid type.");
            }
        }

        /*
         * ============================================================
         * TEAM SIZE VALIDATION
         * ============================================================
         */
        private static void ValidateTeamSize(
            string raidType,
            int teamSize)
        {
            if (raidType != ToB &&
                raidType != HMT)
            {
                return;
            }

            if (teamSize != 3 &&
                teamSize != 4 &&
                teamSize != 5)
            {
                throw new InvalidOperationException(
                    "ToB and HMT team sizes must be 3, 4, or 5.");
            }
        }

        /*
         * ============================================================
         * DEFAULT TEAM SIZE
         * ============================================================
         *
         * ToA = 8.
         * CoX / CM CoX = 0 because they are unlimited.
         */
        private static int GetDefaultTeamSize(
            string raidType)
        {
            return raidType switch
            {
                ToA => 8,
                CoX => 0,
                CmCoX => 0,
                ToB => 5,
                HMT => 5,
                _ => 0
            };
        }

        /*
         * ============================================================
         * LIMITED PARTY
         * ============================================================
         */
        private static bool IsLimitedParty(
            LfrParty party)
        {
            return party.RaidType == ToA ||
                   party.RaidType == ToB ||
                   party.RaidType == HMT;
        }

        /*
         * ============================================================
         * ROLE-BASED RAID
         * ============================================================
         */
        private static bool IsRoleBasedRaid(
            string raidType)
        {
            return raidType == ToB ||
                   raidType == HMT;
        }

        /*
         * ============================================================
         * VALID ROLE
         * ============================================================
         */
        private static bool IsValidRole(
            int teamSize,
            string role)
        {
            return teamSize switch
            {
                3 =>
                    role == Mage ||
                    role == RDPS ||
                    role == MDPS,

                4 =>
                    role == Mage ||
                    role == NFRZ ||
                    role == RDPS ||
                    role == MDPS,

                5 =>
                    role == Mage ||
                    role == NFRZ ||
                    role == RDPS ||
                    role == MDPS ||
                    role == MDPS2,

                _ => false
            };
        }
    }
}