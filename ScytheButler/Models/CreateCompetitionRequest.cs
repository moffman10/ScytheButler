using Discord;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScytheButler.Models
{
    public class CreateCompetitionRequest
    {
        public string Title { get; set; }
        public Metric Metric{ get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int? groupId { get; set; }
        public string? groupVerificationCode { get; set; }
        public string[]? Participants { get; set; }
        public Team[]? teams { get; set; }
    }

    public class Team
    {
        public string name { get; set; }
        public string[] participants { get; set; }
    }

    public enum Metric
    {
        Overall,
        Attack,
        Strength,
        Defence,
        Hitpoints,
        Ranged,
        Magic,
        Prayer,
        Cooking,
        Woodcutting,
        Fletching,
        Fishing,
        Firemaking,
        Crafting,
        Smithing,
        Mining,
        Herblore,
        Agility,
        Thieving,
        Slayer,
        Farming,
        Runecrafting,
        Hunter,
        Construction,
        league_points,
        bounty_hunter_hunter,
        bounty_hunter_rogue,
        clue_scrolls_all,
        clue_scrolls_beginner,
        clue_scrolls_easy,
        clue_scrolls_medium,
        clue_scrolls_hard,
        clue_scrolls_elite,
        clue_scrolls_master,
        last_man_standing,
        pvp_arena,
        soul_wars_zeal,
        guardians_of_the_rift,
        colosseum_glory,
        collections_logged,
        abyssal_sire,
        alchemical_hydra,
        amoxliatl,
        araxxor,
        artio,
        barrows_chests,
        brutus,
        bryophyta,
        callisto,
        calvarion,
        cerberus,
        chambers_of_xeric,
        chambers_of_xeric_challenge_mode,
        chaos_elemental,
        chaos_fanatic,
        commander_zilyana,
        corporeal_beast,
        crazy_archaeologist,
        dagannoth_prime,
        dagannoth_rex,
        dagannoth_supreme,
        deranged_archaeologist,
        doom_of_mokhaiotl,
        duke_sucellus,
        general_graardor,
        giant_mole,
        grotesque_guardians,
        hespori,
        kalphite_queen,
        king_black_dragon,
        kraken,
        kreearra,
        kril_tsutsaroth,
        lunar_chests,
        mimic,
        nex,
        nightmare,
        phosanis_nightmare,
        obor,
        phantom_muspah,
        sarachnis,
        scorpia,
        scurrius,
        shellbane_gryphon,
        skotizo,
        sol_heredit,
        spindel,
        tempoross,
        the_gauntlet,
        the_corrupted_gauntlet,
        the_hueycoatl,
        the_leviathan,
        the_royal_titans,
        the_whisperer,
        theatre_of_blood,
        theatre_of_blood_hard_mode,
        thermonuclear_smoke_devil,
        tombs_of_amascut,
        tombs_of_amascut_expert,
        tzkal_zuk,
        tztok_jad,
        vardorvis,
        venenatis,
        vetion,
        vorkath,
        wintertodt,
        yama,
        zalcano,
        zulrah,
        ehp,
        ehb
    }

}
