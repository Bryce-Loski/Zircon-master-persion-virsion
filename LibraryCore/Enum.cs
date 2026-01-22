using System;
using System.ComponentModel;

/*
 * 各种全局枚举：职业、性别、装备位、技能系别、怪物目标类型之类的通常都在这里
 */
namespace Library
{
    public enum MirGender : byte
    {
        Male,
        Female
    }

    //职业
    public enum MirClass : byte
    {
        Warrior,
        Wizard,
        Taoist,
        Assassin,
    }

    //攻击模式
    public enum AttackMode : byte
    {
        [Description("攻击模式: 和平模式")]
        Peace,
        [Description("攻击模式: 组队攻击")]
        Group,
        [Description("攻击模式: 行会攻击")]
        Guild,
        [Description("攻击模式: 善恶攻击")]
        WarRedBrown,
        [Description("攻击模式: 所有人攻击")]
        All
    }

    //随从模式
    public enum PetMode : byte
    {
        [Description("随从: 跟随,攻击")]
        Both,
        [Description("随从: 跟随")]
        Move,
        [Description("随从: 攻击")]
        Attack,
        [Description("随从: PvP")]
        PvP,
        [Description("随从: 停止")]
        None,
    }

    //角色（玩家、怪物、NPC）面朝的 8 个方向
    public enum MirDirection : byte
    {
        Up = 0,
        [Description("右上")]
        UpRight = 1,
        Right = 2,
        [Description("右下")]
        DownRight = 3,
        Down = 4,
        [Description("左下")]
        DownLeft = 5,
        Left = 6,
        [Description("左上")]
        UpLeft = 7
    }

    /*
     * 职业需求
     * 用位进制来表示职业
     * Warrior = 1 → 0001（战士）
     * Wizard = 2 → 0010（法师）
     * Taoist = 4 → 0100（道士）
     * Assassin = 8 → 1000（刺客）
     */
    [Flags]
    public enum RequiredClass : byte
    {
        None = 0,
        Warrior = 1,
        Wizard = 2,
        Taoist = 4,
        Assassin = 8,
        [Description("战士/法师/道士")]
        WarWizTao = Warrior | Wizard | Taoist,
        [Description("法师/道士")]
        WizTao = Wizard | Taoist,
        [Description("战士/刺客")]
        AssWar = Warrior | Assassin,
        All = WarWizTao | Assassin
    }

    //同上性别要求
    [Flags]
    public enum RequiredGender : byte
    {
        Male = 1,
        Female = 2,
        None = Male | Female
    }


    //装备栏位类型
    public enum EquipmentSlot
    {
        [Description("武器")]
        Weapon = 0,
        [Description("防具")]
        Armour = 1,
        [Description("头盔")]
        Helmet = 2,
        [Description("火把")]
        Torch = 3,
        [Description("项链")]
        Necklace = 4,
        [Description("左手镯")]
        BraceletL = 5,
        [Description("右手镯")]
        BraceletR = 6,
        [Description("左戒指")]
        RingL = 7,
        [Description("右戒指")]
        RingR = 8,
        [Description("鞋子")]
        Shoes = 9,
        [Description("毒药")]
        Poison = 10,
        [Description("护身符")]
        Amulet = 11,
        [Description("鲜花")]
        Flower = 12,
        [Description("马凯")]
        HorseArmour = 13,
        [Description("徽章")]
        Emblem = 14,
        [Description("盾牌")]
        Shield = 15,
        [Description("/时装")]
        Costume = 16,

        //钓鱼系统
        [Description("/鱼钩")]
        Hook = 17,
        [Description("/浮漂")]
        Float = 18,
        [Description("/鱼饵")]
        Bait = 19,
        [Description("/探鱼器")]
        Finder = 20,
        [Description("/鱼竿")]
        Reel = 21
    }

    //宠物的装备栏:CompanionInfo / CompanionLevelInfo / CompanionSkillInfo
    public enum CompanionSlot
    {
        Bag = 0, //宠物背包
        Head = 1,//宠物头部装备
        Back = 2,//宠物背部装备
        Food = 3,//宠物食物栏
    }

    //宠物当前行为 / 状态
    public enum CompanionAction
    {
        None = 0, //当前行为 / 状态
        Moving = 1,//跟随/移动中
        Pickup = 2,//在捡东西
        Hunger = 3,//处于饥饿状态
        Eating = 4,//正在吃东西
        Idle = 5//空闲站立
    }

    /*
     * 一周的天数
     * 地图事件 / 活动 / Buff 的“生效日期”
     * 定时刷新、定时开启功能（比如某些攻城战，只在周六开）
     */
    [Flags]
    public enum DaysOfWeek
    {
        None = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64,
        [Description("工作日")]
        Weekday = Monday | Tuesday | Wednesday | Thursday | Friday,
        [Description("周末")]
        Weekend = Saturday | Sunday 
    }

    /**
     * 地图配置、地图效果（比如某图常年雷雨）
     * 设计：某些技能、掉率、移动速度可以受天气影响
     */
    [Flags]
    public enum Weather
    {
        None = 0,
        Rain = 1,
        Snow = 2,
        Fog = 4,
        Lightning = 8,

        [Description("Snow, Fog")]
        SnowFog = 6,
        [Description("Rain, Lightning")]
        RainLightning = 9,
        [Description("Fog, Lightning")]
        FogLightning = 12,
        [Description("Rain, Fog, Lightning")]
        RainFogLightning = 13
    }

    /*
     * 物品格子/背包“网格类型,基本是所有能放物品的地方
     */
    public enum GridType
    {
        None,
        Inventory,
        Equipment,
        Belt,
        Repair,
        Storage,
        AutoPotion,
        RefineBlackIronOre,
        RefineAccessory,
        RefineSpecial,
        Inspect,
        Consign,
        SendMail,
        TradeUser,
        TradePlayer,
        GuildStorage,
        CompanionInventory,
        CompanionEquipment,
        WeddingRing,
        RefinementStoneIronOre,
        RefinementStoneSilverOre,
        RefinementStoneDiamond,
        RefinementStoneGoldOre,
        RefinementStoneCrystal,
        ItemFragment,
        AccessoryRefineUpgradeTarget,
        AccessoryRefineLevelTarget,
        AccessoryRefineLevelItems,
        MasterRefineFragment1,
        MasterRefineFragment2,
        MasterRefineFragment3,
        MasterRefineStone,
        MasterRefineSpecial,
        AccessoryReset,
        WeaponCraftTemplate,
        WeaponCraftYellow,
        WeaponCraftBlue,
        WeaponCraftRed,
        WeaponCraftPurple,
        WeaponCraftGreen,
        WeaponCraftGrey,
        RefineCorundumOre,
        AccessoryRefineCombTarget,
        AccessoryRefineCombItems,
        PartsStorage,
        Bundle,
        LootBox
    }

    /**
     * 背包模式
     * Normal：正常模式（单纯整理、使用、装备物品）
     * Sell：出售模式（比如打开 NPC 商人时，背包变成“可点选出售”的模式）
     */
    public enum InventoryMode
    {
        Normal,
        Sell
    }

    /*
     * BUFF ID表
     */
    public enum BuffType
    {
        None,

        Server = 1, //服务器全局 Buff（可能是活动加成）
        HuntGold = 2, //打怪金币加成 Buff

        Observable = 3, //可观测状态（比如让别的玩家看到某图标）
        Brown = 4, //棕名（PK 惩罚状态）
        PKPoint = 5, //PK 点数 Buff（或展示用）
        PvPCurse = 6, //PvP 诅咒（惩罚）
        Redemption = 7, //赎罪/减免 PK 点数
        Companion = 8, //同伴相关 Buff（宠物加成）

        Castle = 9, //攻城战相关 Buff（城主、攻城方加成等）

        ItemBuff = 10, //道具临时 Buff
        ItemBuffPermanent = 11, //永久型道具 Buff（称号、戒指等）

        Ranking = 12,//排行加成 Buff（比如前三名有属性加成）
        Developer = 13, //开发者专用 Buff（调试用）
        Veteran = 14,//老玩家荣誉 Buff

        MapEffect = 15, //地图效果 Buff
        InstanceEffect = 16, //副本效果 Buff
        Guild = 17, //行会 Buff

        DeathDrops = 18, //死亡掉落相关 buff

        Fame = 19, //声望加成/影响类 Buff

        //War
        Defiance = 100,
        Might = 101,
        Endurance = 102,
        ReflectDamage = 103,
        Invincibility = 104,
        DefensiveBlow = 105,
        Dash = 106,
        ElementalSwords = 107,

        //Wiz
        Renounce = 200,
        MagicShield = 201,
        JudgementOfHeaven = 202,
        ElementalHurricane = 203,
        SuperiorMagicShield = 204,
        FrostBite = 205,
        Tornado = 206,

        //Tao
        Heal = 300,
        Invisibility = 301,
        MagicResistance = 302,
        Resilience = 303,
        ElementalSuperiority = 304,
        BloodLust = 305,
        StrengthOfFaith = 306,
        CelestialLight = 307,
        Transparency = 308,
        LifeSteal = 309,
        Spiritualism = 310,
        SoulResonance = 311,

        //Ass
        PoisonousCloud = 400,
        FullBloom = 401,
        WhiteLotus = 402,
        RedLotus = 403,
        Cloak = 404,
        GhostWalk = 405,
        TheNewBeginning = 406,
        DarkConversion = 407,
        DragonRepulse = 408,
        Evasion = 409,
        RagingWind = 410,
        LastStand = 411,
        Concentration = 412,

        MagicWeakness = 500, //魔法易伤 Debuff
    }

    public enum RequiredType : byte
    {
        [Description("等级")]
        Level,
        [Description("最高等级")]
        MaxLevel,
        [Description("物理防御")]
        AC,
        [Description("魔法防御")]
        MR,
        [Description("物理攻击")]
        DC,
        [Description("魔法攻击")]
        MC,
        [Description("道术攻击")]
        SC,
        [Description("生命值")]
        Health,
        [Description("法力值")]
        Mana,
        [Description("命中")]
        Accuracy,
        [Description("闪避")]
        Agility,
        [Description("同伴等级")]
        CompanionLevel,
        [Description("同伴最高等级")]
        MaxCompanionLevel,
        [Description("转生等级")]
        RebirthLevel,
        [Description("最大转生等级")]
        MaxRebirthLevel,
    }


    public enum Rarity : byte
    {
        [Description("普通")]
        Common,

        [Description("优秀")]
        Superior,

        [Description("精良")]
        Elite,
    }

    public enum LightSetting : byte
    {
        [Description("默认")]
        Default,

        [Description("明亮")]
        Light,
        [Description("黑夜")]
        Night,
        [Description("黄昏")]
        Twilight,
    }

    public enum TimeOfDay : byte
    {
        [Description("黎明")]
        Dawn,
        [Description("白天")]
        Day,
        [Description("黄昏")]
        Dusk,
        [Description("夜晚")]
        Night
    }

    public enum FightSetting : byte
    {
        [Description("默认")]
        None,
        [Description("安全区域")]
        Safe,
        [Description("战斗区域")]
        Fight,
    }

    public enum InstanceType : byte
    {
        [Description("单人副本")]
        Player = 0,
        [Description("组队副本")]
        Group = 1,
        [Description("行会副本")]
        Guild = 2,
        [Description("城战副本")]
        Castle = 3
    }

    /**
     * Connection 一般用于过图
     * Spawn 怪物刷新点
     * SpawnConnection 让一组刷怪区域之间互相关联（比如怪物从入口刷出然后往别的 Region 走）
     */
    public enum RegionType : byte
    {
        [Description("无")]
        None = 0,

        [Description("区域")]
        Area = 1,
        [Description("连接点")]
        Connection = 2,
        [Description("刷新点")]
        Spawn = 3,
        [Description("NPC 区域")]
        Npc = 4,
        [Description("刷新连接")]
        SpawnConnection = 5
    }

    public enum ObjectType : byte
    {
        None, //Error

        [Description("玩家")]
        Player,
        [Description("物品")]
        Item,
        [Description("NPC")]
        NPC,
        [Description("法术/技能效果")]
        Spell,
        [Description("怪物")]
        Monster
    }

    public enum ItemType : byte
    {

        [Description("无")]
        Nothing = 0,

        [Description("消耗品")]
        Consumable = 1,

        [Description("武器")]
        Weapon = 2,

        [Description("护甲")]
        Armour = 3,

        [Description("火把")]
        Torch = 4,

        [Description("头盔")]
        Helmet = 5,

        [Description("项链")]
        Necklace = 6,

        [Description("手镯")]
        Bracelet = 7,

        [Description("戒指")]
        Ring = 8,

        [Description("鞋子")]
        Shoes = 9,

        [Description("毒药")]
        Poison = 10,

        [Description("护身符")]
        Amulet = 11,

        [Description("肉类")]
        Meat = 12,

        [Description("矿石")]
        Ore = 13,

        [Description("技能书")]
        Book = 14,

        [Description("卷轴")]
        Scroll = 15,

        [Description("黑暗之石")]
        DarkStone = 16,

        [Description("特殊精炼材料")]
        RefineSpecial = 17,

        [Description("马凯")]
        HorseArmour = 18,

        [Description("鲜花")]
        Flower = 19,

        [Description("宠物饲料")]
        CompanionFood = 20,

        [Description("宠物背包")]
        CompanionBag = 21,

        [Description("宠物头饰")]
        CompanionHead = 22,

        [Description("宠物翅膀")]
        CompanionBack = 23,

        [Description("系统物品")]
        System = 24,

        [Description("物品部件")]
        ItemPart = 25,

        [Description("徽章")]
        Emblem = 26,

        [Description("盾牌")]
        Shield = 27,

        [Description("时装")]
        Costume = 28,

        [Description("鱼钩")]
        Hook = 29,

        [Description("浮标")]
        Float = 30,

        [Description("鱼饵")]
        Bait = 31,

        [Description("探鱼器")]
        Finder = 32,

        [Description("鱼线轮")]
        Reel = 33,

        [Description("货币")]
        Currency = 34,

        [Description("礼包")]
        Bundle = 35,

        [Description("战利品宝箱")]
        LootBox = 36
    }

    public enum MirAction : byte
    {
        Standing,
        Moving,
        Pushed,
        Attack,
        RangeAttack,
        Spell,
        Harvest,
        Struck,
        Die,
        Dead,
        Show,
        Hide,
        Mount,
        Mining,
        Fishing,
        Idle
    }

    public enum MirAnimation : byte
    {
        Standing,
        Walking,
        CreepStanding,
        CreepWalkSlow,
        CreepWalkFast,
        Running,
        Pushed,
        Combat1,
        Combat2,
        Combat3,
        Combat4,
        Combat5,
        Combat6,
        Combat7,
        Combat8,
        Combat9,
        Combat10,
        Combat11,
        Combat12,
        Combat13,
        Combat14,
        Combat15,
        Harvest,
        Stance,
        Struck,
        Die,
        Dead,
        Skeleton,
        Show,
        Hide,

        HorseStanding,
        HorseWalking,
        HorseRunning,
        HorseStruck,

        StoneStanding,

        DragonRepulseStart,
        DragonRepulseMiddle,
        DragonRepulseEnd,

        ChannellingStart,
        ChannellingMiddle,
        ChannellingEnd,

        FishingCast,
        FishingWait,
        FishingReel
    }

    public enum MessageAction
    {
        None,
        Revive,
    }

    public enum MessageType
    {
        Normal,
        Shout,
        WhisperIn,
        GMWhisperIn,
        WhisperOut,
        Group,
        Global,
        Hint,
        System,
        Announcement,
        Combat,
        ObserverChat,
        Guild,
        Debug
    }

    public enum NPCDialogType
    {
        None,
        BuySell,
        Repair,
        Refine,
        RefineRetrieve,
        CompanionManage,
        WeddingRing,
        RefinementStone,
        MasterRefine,
        WeaponReset,
        ItemFragment,
        AccessoryRefineUpgrade,
        AccessoryRefineLevel,
        AccessoryReset,
        WeaponCraft,
        AccessoryRefine,

        RollDie,
        RollYut
    }

    public enum MagicSchool
    {
        None,

        Passive = 1,
        Active,
        Toggle,
        Fire,
        Ice,
        Lightning,
        Wind,
        Holy,
        Dark,
        Phantom,
        Physical,
        Atrocity,
        Kill,
        Assassination,

        Horse,

        Discipline = 20
    }

    public enum Element : byte
    {
        None,

        Fire,
        Ice,
        Lightning,
        Wind,
        Holy,
        Dark,
        Phantom,
    }

    public enum MagicType
    {
        None,

        Swordsmanship = 100,
        PotionMastery = 101,
        Slaying = 102,
        Thrusting = 103,
        HalfMoon = 104,
        ShoulderDash = 105,
        FlamingSword = 106,
        DragonRise = 107,
        BladeStorm = 108,
        DestructiveSurge = 109,
        Interchange = 110,
        Defiance = 111,
        Beckon = 112,
        Might = 113,
        SwiftBlade = 114,
        Assault = 115,
        Endurance = 116,
        ReflectDamage = 117,
        Fetter = 118,
        AugmentDestructiveSurge = 119,
        AugmentDefiance = 120,
        AugmentReflectDamage = 121,
        AdvancedPotionMastery = 122,
        MassBeckon = 123,
        SeismicSlam = 124,
        Invincibility = 125,
        CrushingWave = 126,
        DefensiveMastery = 127,
        PhysicalImmunity = 128,
        MagicImmunity = 129,
        DefensiveBlow = 130,
        ElementalSwords = 131,
        Shuriken = 132,
        HundredFist = 133,
        OffensiveBlow = 134,
        TaecheonSword = 135,
        FireSword = 136,
        FlameArt = 137, //NOT CODED

        FireBall = 201,
        LightningBall = 202,
        IceBolt = 203,
        GustBlast = 204,
        Repulsion = 205,
        ElectricShock = 206,
        Teleportation = 207,
        AdamantineFireBall = 208,
        ThunderBolt = 209,
        IceBlades = 210,
        Cyclone = 211,
        ScortchedEarth = 212,
        LightningBeam = 213,
        FrozenEarth = 214,
        BlowEarth = 215,
        FireWall = 216,
        ExpelUndead = 217,
        GeoManipulation = 218,
        MagicShield = 219,
        FireStorm = 220,
        LightningWave = 221,
        IceStorm = 222,
        DragonTornado = 223,
        GreaterFrozenEarth = 224,
        ChainLightning = 225,
        MeteorShower = 226,
        Renounce = 227,
        Tempest = 228,
        JudgementOfHeaven = 229,
        ThunderStrike = 230,
        FireBounce = 231,
        ElementalHurricane = 232,
        SuperiorMagicShield = 233,
        Burning = 234,
        Shocked = 235,
        LightningStrike = 236,
        MirrorImage = 237,
        IceRain = 238,
        FrostBite = 239,
        Asteroid = 240,
        Storm = 241,//NOT CODED
        Tornado = 242,//NOT CODED
        IceAura = 243,
        IceDragon = 244,
        IceBreaker = 245,
        FrozenDragon = 246,
        UnityWithNature = 247, //NOT CODED

        Heal = 300,
        SpiritSword = 301,
        PoisonDust = 302,
        ExplosiveTalisman = 303,
        EvilSlayer = 304,
        Invisibility = 305,
        MagicResistance = 306,
        MassInvisibility = 307,
        GreaterEvilSlayer = 308,
        Resilience = 309,
        TrapOctagon = 310,
        CombatKick = 311,
        ElementalSuperiority = 312,
        MassHeal = 313,
        BloodLust = 314,
        Resurrection = 315,
        Purification = 316,
        Transparency = 317,
        CelestialLight = 318,
        EmpoweredHealing = 319,
        LifeSteal = 320,
        ImprovedExplosiveTalisman = 321,
        AugmentPoisonDust = 322,
        CursedDoll = 323,
        ThunderKick = 324,
        SoulResonance = 325,
        Parasite = 326,
        Spiritualism = 327,
        AugmentExplosiveTalisman = 328,
        AugmentEvilSlayer = 329,
        AugmentPurification = 330,
        AugmentResurrection = 331,
        SummonSkeleton = 332,
        SummonShinsu = 333,
        SummonJinSkeleton = 334,
        StrengthOfFaith = 335,
        SummonDemonicCreature = 336,
        DemonExplosion = 337,
        Infection = 338,
        DemonicRecovery = 339,
        Neutralize = 340,
        AugmentNeutralize = 341,
        DarkSoulPrison = 342,
        SearingLight = 343,
        AugmentCelestialLight = 344,
        CorpseExploder = 345,
        SummonDead = 346,
        BindingTalisman = 347,
        BrainStorm = 348,
        HeavenlySky = 349,
        PoisonCloud = 350,
        SupremeHealing = 351, //NOT CODED

        WillowDance = 401,
        VineTreeDance = 402,
        Discipline = 403,
        PoisonousCloud = 404,
        FullBloom = 405,
        Cloak = 406,
        WhiteLotus = 407,
        CalamityOfFullMoon = 408,
        WraithGrip = 409,
        RedLotus = 410,
        HellFire = 411,
        PledgeOfBlood = 412,
        Rake = 413,
        SweetBrier = 414,
        SummonPuppet = 415,
        Karma = 416,
        TouchOfTheDeparted = 417,
        WaningMoon = 418,
        GhostWalk = 419,
        ElementalPuppet = 420,
        Rejuvenation = 421,
        Resolution = 422,
        ChangeOfSeasons = 423,
        Release = 424,
        FlameSplash = 425,
        BloodyFlower = 426,
        TheNewBeginning = 427,
        DanceOfSwallow = 428,
        DarkConversion = 429,
        DragonRepulse = 430,
        AdventOfDemon = 431,
        AdventOfDevil = 432,
        Abyss = 433,
        FlashOfLight = 434,
        Stealth = 435,
        Evasion = 436,
        RagingWind = 437,
        Unused = 438,//UNUSED
        Massacre = 439,
        ArtOfShadows = 440,
        DragonBlood = 441,
        FatalBlow = 442,
        LastStand = 443,
        MagicCombustion = 444,
        Vitality = 445,
        Chain = 446,
        Concentration = 447,
        DualWeaponSkills = 448,
        Containment = 449,
        DragonWave = 450,
        Hemorrhage = 451,
        BurningFire = 452,
        ChainOfFire = 453,
        FlamingDaggers = 454,
        Shredding = 455,
        FourWheels = 456,
        CrescentMoon = 457,
        ManaBurn = 458, //NOT CODED

        MonsterScortchedEarth = 501,
        MonsterIceStorm = 502,
        MonsterDeathCloud = 503,
        MonsterThunderStorm = 504,

        SamaGuardianFire = 505,
        SamaGuardianIce = 506,
        SamaGuardianLightning = 507,
        SamaGuardianWind = 508,

        SamaPhoenixFire = 509,
        SamaBlackIce = 510,
        SamaBlueLightning = 511,
        SamaWhiteWind = 512,

        SamaProphetFire = 513,
        SamaProphetLightning = 514,
        SamaProphetWind = 515,

        DoomClawLeftPinch = 520,
        DoomClawLeftSwipe = 521,
        DoomClawRightPinch = 522,
        DoomClawRightSwipe = 523,
        DoomClawWave = 524,
        DoomClawSpit = 525,

        PinkFireBall = 530,
        GreenSludgeBall = 540,
    }

    public enum MagicProperty
    {
        None = 0,

        Active = 1,
        Passive = 2,
        Augmentation = 3,
        Toggle = 4,
        Charge = 5
    }

    //NF = No Frame
    public enum MonsterImage
    {
        None,

        //NF_StonePillar = 10,
        //NF_BlackPumpkinMan = 11,
        MutatedOctopus = 12,
        //NF_StoneBuilding13 = 13,
        StoneGolem = 14,
        NetherWorldGate = 15,
        LightArmedSoldier = 16,
        AntHealer = 17,
        ArmoredAnt = 18,
        Stomper = 19,

        ChaosKnight = 20,
        //NF_CrystalPillar = 21,
        CorpseStalker = 22,
        NumaMage = 23,
        AntSoldier = 24,
        //NF_StoneBuilding25 = 25,
        //NF_StoneBuilding26 = 26,
        NumaElite = 27,
        Phantom = 28,
        CrimsonNecromancer = 29,

        Chicken = 30,
        Deer = 31,
        //NF_Man1 = 32,
        Oma = 33,
        OmaHero = 34,
        SpittingSpider = 35,
        Guard = 36,
        OmaWarlord = 37,
        Scorpion = 38,
        CaveBat = 39,

        ForestYeti = 40,
        CarnivorousPlant = 41,
        Skeleton = 42,
        SkeletonAxeThrower = 43,
        SkeletonAxeMan = 44,
        SkeletonWarrior = 45,
        SkeletonLord = 46,
        CaveMaggot = 47,
        ClawCat = 48,
        //NF_KoreanFlag = 49,

        Scarecrow = 50,
        UmaInfidel = 51,
        BloodThirstyGhoul = 52,
        UmaFlameThrower = 53,
        UmaAnguisher = 54,
        UmaKing = 55,
        SpinedDarkLizard = 56,
        Dung = 57,
        GhostSorcerer = 58,
        GhostMage = 59,

        VoraciousGhost = 60,
        DevouringGhost = 61,
        CorpseRaisingGhost = 62,
        GhoulChampion = 63,
        RedSnake = 64,
        //NF_KatanaGuard = 65,
        WhiteBone = 66,
        TigerSnake = 67,
        Sheep = 68,
        SkyStinger = 69,

        ShellNipper = 70,
        VisceralWorm = 71,
        //NF_KingScorpion = 72,
        Beetle = 73,
        SpikedBeetle = 74,
        Wolf = 75,
        Centipede = 76,
        LordNiJae = 77,
        MutantMaggot = 78,
        Earwig = 79,

        IronLance = 80,
        WaspHatchling = 81,
        ButterflyWorm = 82,
        WedgeMothLarva = 83,
        LesserWedgeMoth = 84,
        WedgeMoth = 85,
        RedBoar = 86,
        BlackBoar = 87,
        TuskLord = 88,
        ClawSerpent = 89,

        EvilSnake = 90,
        ViciousRat = 91,
        ZumaSharpShooter = 92,
        ZumaFanatic = 93,
        ZumaGuardian = 94,
        ZumaKing = 95,
        ArcherGuard = 96,
        //NF_DemonGuardMace = 97,
        //NF_DemonGuardSword = 98,
        Shinsu = 99, //Small

        Shinsu1 = 100, //Large
        UmaMaceInfidel = 101,
        AquaLizard = 102,
        CorrosivePoisonSpitter = 103,
        SandShark = 104,
        CursedCactus = 105,
        AntNeedler = 106,
        WindfurySorceress = 107,
        //NF_NumaMounted = 108,
        PhantomSoldier = 109,

        //NF_FoxWarrior = 110,
        SpiderBat = 111,
        //NF_FoxTaoist = 112,
        //NF_FoxWizard = 113,
        RedMoonTheFallen = 114,
        Larva = 115,
        ArachnidGazer = 116,
        RedMoonGuardian = 117,
        RedMoonProtector = 118,
        RedMoonRedProtector = 119,

        RedMoonGrayProtector = 120,
        VenomousArachnid = 121,
        DarkArachnid = 122,
        ForestGuard = 123,
        TownGuard = 124,
        SandGuard = 125,
        //NF_Blank126 = 126,
        //NF_Blank127 = 127,
        //NF_Blank128 = 128,
        Pig = 129,

        PachonTheChaosBringer = 130,
        Cow = 131,
        //NF_NumaAxeman = 132,
        //NF_Football = 133,
        //NF_HermitFemale = 134,
        //NF_HermitMale = 135,
        //NF_WhiteSnake = 136,
        ChestnutTree = 137,
        NumaGrunt = 138,
        NumaWarrior = 139,

        BanyaRightGuard = 140,
        BanyaLeftGuard = 141,
        DecayingGhoul = 142,
        FrostMinotaur = 143,
        ShockMinotaur = 144,
        FuryMinotaur = 145,
        FlameMinotaur = 146,
        Minotaur = 147,
        RottingGhoul = 148,
        EmperorSaWoo = 149,

        BoneCaptain = 150,
        ArchLichTaedu = 151,
        BoneSoldier = 152,
        BoneBladesman = 153,
        BoneArcher = 154,
        MutantFlea = 155,
        //NF_PurpleFlea = 156,
        BlasterMutantFlea = 157,
        //NF_BlueBlasterMutantFlea = 158,
        PoisonousMutantFlea = 159,

        RazorTusk = 160,
        //NF_Reindeer = 161,
        //NF_EvilScorpion = 162,
        //NF_ChristmasTree = 163,
        Monkey = 164,
        //NF_Santa = 165,
        CannibalFanatic = 166,
        EvilFanatic = 167,
        EvilElephant = 168,
        FlameGriffin = 169,

        StoneGriffin = 170,
        MutantCaptain = 171,
        PinkGoddess = 172,
        GreenGoddess = 173,
        JinchonDevil = 174,
        //NF_JungleAxeman = 175,
        //NF_JungleClubman = 176,
        //NF_Catapult177 = 177,
        //NF_Blank178 = 178,
        //NF_Catapult179 = 179,

        IcyGoddess = 180,
        WildBoar = 181,
        //NF_AngelGuardian = 182,
        //NF_Blank183 = 183,
        //NF_NumaElder = 184,
        //NF_Blank185 = 185,
        //NF_Blank186 = 186,
        //NF_NumaPriest = 187,
        //NF_Blank188 = 188,
        //NF_BonePile189 = 189,

        NumaCavalry = 190,
        NumaArmoredSoldier = 191,
        //NF_NumaAxeSoldier = 192,
        NumaStoneThrower = 193,
        NumaHighMage = 194,
        NumaRoyalGuard = 195,
        //NF_NumaWarlord = 196,
        BloodStone = 197,
        //NF_Chest = 198,
        //NF_BonePile199 = 199,

        //NF_Snowman = 200,
        RagingLizard = 201,
        SawToothLizard = 202,
        MutantLizard = 203,
        VenomSpitter = 204,
        SonicLizard = 205,
        GiantLizard = 206,
        TaintedTerror = 207,
        DeathLordJichon = 208,
        CrazedLizard = 209,

        IcyRanger = 210,
        FerociousIceTiger = 211,
        IcySpiritWarrior = 212,
        IcySpiritGeneral = 213,
        GhostKnight = 214,
        FrostLordHwa = 215,
        IcySpiritSpearman = 216,
        Werewolf = 217,
        Whitefang = 218,
        IcySpiritSolider = 219,

        EscortCommander = 220,
        QueenOfDawn = 221,
        FieryDancer = 222,
        EmeraldDancer = 223,
        //NF_Blank224 = 224,
        //NF_Blank225 = 225,
        //NF_Blank226 = 226,
        //NF_Blank227 = 227,
        //NF_Blank228 = 228,
        //NF_Blank229 = 229,

        ChiwooGeneral = 230,
        DragonLord = 231,
        DragonQueen = 232,
        OYoungBeast = 233,
        MaWarlord = 234,
        YumgonGeneral = 235,
        YumgonWitch = 236,
        JinhwanSpirit = 237,
        JinhwanGuardian = 238,
        JinamStoneGate = 239,

        //Mon24

        SamaFireGuardian = 250,
        SamaIceGuardian = 251,
        SamaLightningGuardian = 252,
        SamaWindGuardian = 253,
        Phoenix = 254,
        BlackTortoise = 255,
        BlueDragon = 256,
        WhiteTiger = 257,

        InfernalSoldier = 260,
        //NF_Blank261 = 261,
        //NF_Blank262 = 262,
        //NF_Blank263 = 263,
        //NF_Blank264 = 264,
        //NF_Blank265 = 265,
        //NF_Blank266 = 266,
        //NF_Blank267 = 267,
        //NF_Blank268 = 268,
        //NF_Blank269 = 269,

        SamaCursedBladesman = 270,
        SamaCursedSlave = 271,
        SamaCursedFlameMage = 272,
        SamaProphet = 273,
        SamaSorcerer = 274,
        EnshrinementBox = 275,
        //NF_AssassinMale = 276,
        //NF_AssassinFemale = 277,
        //NF_UmaMaceSoldier = 278,
        //NF_Blank279 = 279,

        Salamander = 280,
        SandGolem = 281,
        //NF_NumaLoneGuard = 282,
        //NF_SmallSpider = 283,
        OmaInfant = 284,
        Yob = 285,
        RakingCat = 286,
        UmaTridentInfidel = 287,
        GangSpider = 288,
        VenomSpider = 289,

        SDMob4 = 290,
        SDMob5 = 291,
        SDMob6 = 292,
        //NF_SpiritSpider = 293,
        //NF_DarkMage = 294,
        //NF_Lizard = 295,
        //NF_DarkDevil = 296,
        //NF_NumaSoldier = 297,
        SDMob7 = 298,
        OmaMage = 299,

        WildMonkey = 300,
        FrostYeti = 301,
        //NF_SnakeTower = 302,
        //NF_Duck = 303,
        //NF_Rabbit = 304,
        //NF_BonePile305 = 305,
        //NF_BigFootball = 306,
        //NF_BluePumpkinMan = 307,
        //NF_RedPumpkinMan = 308,
        //NF_Blank309 = 309,

        //Mon31

        SDMob8 = 320,
        SDMob9 = 321,
        //NF_BlueMouseWithTail = 322,
        //NF_VampireDagger = 323,
        //NF_VampireSpear = 324,
        SDMob10 = 325,
        SDMob11 = 326,
        SDMob12 = 327,
        SDMob13 = 328,
        SDMob14 = 329,

        //Mon33

        //Mon34
        Companion_Pig = 340,
        Companion_TuskLord = 341,
        Companion_SkeletonLord = 342,
        Companion_Griffin = 343,
        Companion_Dragon = 344,
        Companion_Donkey = 345,
        Companion_Sheep = 346,
        Companion_BanyoLordGuzak = 347,
        Companion_Panda = 348,
        Companion_Rabbit = 349,

        OrangeTiger = 350,
        RegularTiger = 351,
        RedTiger = 352,
        SnowTiger = 353,
        BlackTiger = 354,
        BigBlackTiger = 355,
        BigWhiteTiger = 356,
        OrangeBossTiger = 357,
        BigBossTiger = 358,
        //NF_Blank359 = 359,

        //Mon36

        //NF_YurinMon0 = 370,
        //NF_YurinMon1 = 371,
        //NF_WhiteBeardedTiger = 372,
        //NF_BlackBeardedTiger = 373,
        //NF_HardenedRhino = 374,
        //NF_Mammoth = 375,
        //NF_CursedSlave1 = 376,
        //NF_CursedSlave2 = 377,
        //NF_CursedSlave3 = 378,
        //NF_PoisonousGolem = 379,

        //NF_GardenSoldier = 380,
        //NF_GardenDefender = 381,
        //NF_RedBlossom = 382,
        //NF_BlueBlossom = 383,
        //NF_FireBird = 384,
        //NF_BlueGorilla = 385,
        //NF_RedGorilla = 386,
        //NF_Blossom = 387,
        //NF_BlueBird = 388,
        //NF_Blank389 = 389,

        //NF_Nameless390 = 390,
        //NF_Nameless391 = 391,
        //NF_Nameless392 = 392,
        //NF_Nameless393 = 393,
        //NF_Nameless394 = 394,
        //NF_Nameless395 = 395,
        //NF_Nameless396 = 396,
        //NF_Nameless397 = 397,
        //NF_Nameless398 = 398,
        //NF_Nameless399 = 399,

        CrystalGolem = 400,
        //NF_Nameless401 = 401,
        //NF_Nameless402 = 402,
        //NF_Nameless403 = 403,
        //NF_Nameless404 = 404,
        //NF_Nameless405 = 405,
        //NF_Nameless406 = 406,
        //NF_Nameless407 = 407,
        //NF_Nameless408 = 408,
        //NF_Nameless409 = 409,

        //NF_Nameless410 = 410,
        DustDevil = 411,
        TwinTailScorpion = 412,
        BloodyMole = 413,
        //NF_Nameless414 = 414,
        //NF_Nameless415 = 415,
        //NF_Nameless416 = 416,
        //NF_Nameless417 = 417,
        //NF_Blank418 = 418,
        //NF_Blank419 = 419,

        //NF_HellVampire = 420,
        //NF_HellSmelterer = 421,
        //NF_HellPuddle = 422,
        //NF_CrystalPillar2 = 423,
        Terracotta1 = 424,
        Terracotta2 = 425,
        Terracotta3 = 426,
        Terracotta4 = 427,
        TerracottaSub = 428,
        TerracottaBoss = 429,

        //Mon43

        //NF_Nameless440 = 440,
        //NF_Nameless441 = 441,
        //NF_Nameless442 = 442,
        SDMob19 = 443,
        SDMob20 = 444,
        SDMob21 = 445,
        SDMob22 = 446,
        SDMob23 = 447,
        SDMob24 = 448,
        SDMob25 = 449,

        SDMob26 = 450,
        LobsterLord = 453,

        //Mon46

        NewMob1 = 470,
        NewMob2 = 471,
        NewMob3 = 472,
        NewMob4 = 473,
        NewMob5 = 474,
        NewMob6 = 475,
        NewMob7 = 476,
        NewMob8 = 477,
        NewMob9 = 478,
        NewMob10 = 479,

        //Mon48

        MonasteryMon0 = 490,
        MonasteryMon1 = 491,
        MonasteryMon2 = 492,
        MonasteryMon3 = 493,
        MonasteryMon4 = 494,
        MonasteryMon5 = 495,
        MonasteryMon6 = 496,
        //NF_Blank497 = 497,
        //NF_Blank498 = 498,
        //NF_Blank499 = 499,

        //Mon50

        //Mon51

        //Mon52
        WildBrownHorse = 520,
        WildWhiteHorse = 521,
        WildBlackHorse = 522,
        //NF_Blank523 = 523,
        WildRedHorse = 524,
        //NF_Blank525 = 525,
        //NF_Blank526 = 526,
        //NF_Blank527 = 527,
        //NF_Blank528 = 528,
        //NF_Blank529 = 529,

        //MonMagicEx25
        SeaHorseCavalry = 530,
        Seamancer = 531,
        CoralStoneDuin = 532,
        Brachiopod = 533,
        GiantClam = 534,
        BlueMassif = 535,
        Mollusk = 536,
        Kraken = 537,
        KrakenLeg = 538,
        GiantClam1 = 539,

        //Mon54
        SabukGateSouth = 540,
        SabukGateNorth = 541,
        SabukGateEast = 542,
        SabukGateWest = 543,
        //NF_NorthBarrier = 544,
        //NF_SouthBarrier = 545,
        //NF_EastBarrier = 546,
        //NF_WestBarrier = 547,
        //NF_TaoSungDoor = 548,
        //NF_Blank_549 = 549,

        //Mon55

        //Mon56
        Tornado = 566,

        //Mon57
        Companion_Dog = 570,
        Companion_Jinchon = 571,
        Companion_Dino = 572,

        //Mon58

        //Mon59


        //Flag
        CastleFlag = 1000
    }


    public enum MapIcon
    {
        None,

        Cave = 1,
        Exit = 2,
        Down = 3,
        Up = 4,
        Province = 5,
        Building = 6,

        BichonCity = 100,
        Castle,
        BugCaves,
        CaveUpDown,
        SmallManInTriangle,
        Dunes,
        Doorway,
        GinkoTree,
        Forest,
        InsectCaveBubble,
        AntCave,
        JinchonTemple,
        MiningCave,
        Mudwall,
        BorderTown,
        Oasis,
        UnknownPalace,
        Pointer,
        Serpent,
        Shrine,
        SkullCave,
        SkullBonesCave,
        StairDown,
        StairUp,
        UnknownTemple,
        Walkway,
        StoneTemple,
        WoomaTemple,
        ZumaTemple,
        IslandShores,
        DuneWalkway,
        DuneWalkway2,
        ForestWalkway,
        ForestWalkway2,
        ForestWalkway3,
        Star,
        Lock,
        Boat
    }

    public enum Effect
    {
        TeleportOut,
        TeleportIn,

        FullBloom,
        WhiteLotus,
        RedLotus,
        SweetBrier,
        Karma,

        MirrorImage,

        Puppet,
        PuppetFire,
        PuppetIce,
        PuppetLightning,
        PuppetWind,

        SummonSkeleton,
        SummonShinsu,
        CursedDoll,
        UndeadSoul,

        ThunderBolt,
        FrostBiteEnd,

        DanceOfSwallow,
        FlashOfLight,
        ChainOfFireExplode,

        DemonExplosion,
        ParasiteExplode,
        BurningFireExplode,

        FireWallSmoke,

        HundredFist,
        HundredFistStruck,

        IceAuraEnd
    }

    [Flags]
    public enum PoisonType
    {
        None = 0,

        Green = 1 << 0,         //Tick damage, displays green
        Red = 1 << 1,           //Increases damage received by 20%, displays red
        Slow = 1 << 2,          //Reduces attackTime, actionTime, 100ms per value, displays blue
        Paralysis = 1 << 3,     //Stops movement, physical and magic attacks (all races), displays grey
        WraithGrip = 1 << 4,    //Stops shoulderdash, movement, displays effect (needs code revisiting)
        HellFire = 1 << 5,      //Tick damage, no colour
        Silenced = 1 << 6,      //Stops movement (all races), physical and magic attacks (monster), displays effect
        Abyss = 1 << 7,         //Reduces monster viewrange, displays blinding effect (player)
        Parasite = 1 << 8,      //Tick damage, explosion, ignores transparency (monster), displays effect
        Neutralize = 1 << 9,    //Stops attackTime, slows actionTime, displays effect (needs code revisiting)
        Fear = 1 << 10,         //Stops attack (monster), forces runaway (monster), displays effect
        Burn = 1 << 11,         //Tick damage, displays effect
        Containment = 1 << 12,  //Tick damage, stops movement, displays effect
        Chain = 1 << 13,        //Tick damage, limits movement, displays effect
        Hemorrhage = 1 << 14,   //Tick damage, stops recovery, displays effect
        Binding = 1 << 15,      //Tick damage, stops movement, displays effect
    }

    public enum SpellEffect
    {
        None,

        SafeZone,

        FireWall,
        Tempest,
        IceAura,

        TrapOctagon,
        DarkSoulPrison,

        PoisonousCloud,
        BurningFire,

        Rubble,

        MonsterDeathCloud,

        ZombieHole
    }


    public enum MagicEffect
    {
        ReflectDamage,
        Assault,
        ElementalSwords,
        DefensiveBlow,
        HundredFist,

        MagicShield,
        MagicShieldStruck,
        SuperiorMagicShield,
        SuperiorMagicShieldStruck,
        ElementalHurricane,
        FrostBite,
        Burn,

        CelestialLight,
        CelestialLightStruck,
        Parasite,
        Neutralize,

        WraithGrip,
        LifeSteal,
        Silence,
        Blind,
        Fear,
        Abyss,
        DragonRepulse,
        Containment,
        Chain,
        Hemorrhage,
        Binding,

        Ranking,
        Developer,
    }

    public enum MarketPlaceSort
    {
        Newest,
        Oldest,
        [Description("Highest Price")]
        HighestPrice,
        [Description("Lowest Price")]
        LowestPrice,
    }

    public enum MarketPlaceStoreSort
    {
        Alphabetical,
        [Description("Highest Price")]
        HighestPrice,
        [Description("Lowest Price")]
        LowestPrice,
        Favourite
    }

    public enum DungeonFinderSort
    {
        Name,
        Level,
        [Description("Player Count")]
        PlayerCount,
    }

    public enum RefineType : byte
    {
        None,

        Durability,
        DC,
        SpellPower,
        Fire,
        Ice,
        Lightning,
        Wind,
        Holy,
        Dark,
        Phantom,
        Reset,
        Health,
        Mana,
        AC,
        MR,
        Accuracy,
        Agility,
        DCPercent,
        SPPercent,
        HealthPercent,
        ManaPercent,
    }

    public enum RefineQuality : byte
    {
        Rush = 0,
        Quick = 1,
        Standard = 2,
        Careful = 3,
        Precise = 4,
    }

    public enum ExteriorEffect : byte
    {
        None = 0,

        //EquipEffect_Part [1~99] 
        A_WhiteAura = 1,
        A_FlameAura = 2,
        A_BlueAura = 3,

        A_FlameAura2 = 9,
        A_GreenWings = 10,
        A_FlameWings = 11,
        A_BlueWings = 12,
        A_RedSinWings = 13,

        A_DiamondFireWings = 14,
        A_PurpleTentacles2 = 15,
        A_PhoenixWings = 16,
        A_IceKingWings = 17,
        A_BlueButterflyWings = 18,


        S_WarThurible = 50,
        S_PenanceThurible = 51,
        S_CensorshipThurible = 52,
        S_PetrichorThurible = 53,

        //EquipEffect_Full [100~119]
        A_FireDragonWings = 100,
        A_SmallYellowWings = 101,
        A_GreenFeatherWings = 102,
        A_RedFeatherWings = 103,
        A_BlueFeatherWings = 104,
        A_WhiteFeatherWings = 105,
        A_PurpleTentacles = 106,

        W_ChaoticHeavenBlade = 110,
        W_JanitorsScimitar = 111,
        W_JanitorsDualBlade = 112,

        //EquipEffect_FullEx1 [120~139] 
        A_LionWings = 120,
        A_AngelicWings = 121,

        //EquipEffect_FullEx2 [140~159] 
        A_BlueDragonWings = 140,

        //EquipEffect_FullEx3 [160~179]
        A_RedWings2 = 160,

        //EquipEffect_Item [180~199]
        //Reserved

        //MonMagicEx26 [200~250] 
        E_RedEyeRing = 200,
        E_BlueEyeRing = 201,
        E_GreenSpiralRing = 202,
        E_Fireworks = 203
    }

    public enum ItemEffect : byte
    {
        None,

        //Gold = 1,
        Experience = 2,
        CompanionTicket = 3,
        BasicCompanionBag = 4,
        PickAxe = 5,
        UmaKingHorn = 6,
        ItemPart = 7,
        Carrot = 8,

        DestructionElixir = 10,
        HasteElixir = 11,
        LifeElixir = 12,
        ManaElixir = 13,
        NatureElixir = 14,
        SpiritElixir = 15,

        BlackIronOre = 20,
        GoldOre = 21,
        Diamond = 22,
        SilverOre = 23,
        IronOre = 24,
        Corundum = 25,

        ElixirOfPurification = 30,
        PillOfReincarnation = 31,

        Crystal = 40,
        RefinementStone = 41,
        Fragment1 = 42,
        Fragment2 = 43,
        Fragment3 = 44,

        GenderChange = 50,
        HairChange = 51,
        ArmourDye = 52,
        NameChange = 53,
        FortuneChecker = 54,
        Caption = 55,

        WeaponTemplate = 60,
        WarriorWeapon = 61,
        WizardWeapon = 63,
        TaoistWeapon = 64,
        AssassinWeapon = 65,

        YellowSlot = 70,
        BlueSlot = 71,
        RedSlot = 72,
        PurpleSlot = 73,
        GreenSlot = 74,
        GreySlot = 75,

        FootballArmour = 80,
        FootBallWhistle = 81,

        FishingRod = 82,
        FishingRobe = 83,

        StatExtractor = 90,
        SpiritBlade = 91,
        RefineExtractor = 92,

        DualWield = 100,
        MagicRing = 101
    }

    public enum BundleType
    {
        [Description("Any Of")]
        AnyOf,
        [Description("All Of")]
        AllOf,
        [Description("One Of")]
        OneOf
    }

    public enum CurrencyType
    {
        Gold = 0,
        GameGold = 1,
        HuntGold = 2,
        Other = 3,
        FP = 4,
        CP = 5
    }

    public enum CurrencyCategory
    {
        Basic = 0,
        Player = 1,
        Event = 2,
        Map = 3,
        Other = 4
    }

    [Flags]
    public enum UserItemFlags
    {
        None = 0,

        Locked = 1,
        Bound = 2,
        Worthless = 4,
        Refinable = 8,
        Expirable = 16,
        QuestItem = 32,
        GameMaster = 64,
        Marriage = 128,
        NonRefinable = 256
    }

    public enum HorseType : byte
    {
        None = 0,
        Brown = 1,
        White = 2,
        Red = 3,
        Black = 4,
        WhiteUnicorn = 5,
        RedUnicorn = 6
    }

    public enum OnlineState : byte
    {
        Online,
        Busy,
        Away,
        Offline
    }

    [Flags]
    public enum GuildPermission
    {
        None = 0,

        Leader = -1,

        EditNotice = 1,
        AddMember = 2,
        RemoveMember = 4,
        Storage = 8,
        FundsRepair = 16,
        FundsMerchant = 32,
        FundsMarket = 64,
        StartWar = 128,
    }

    public enum NPCRequirementType
    {
        MinLevel = 0,
        MaxLevel = 1,
        Accepted = 2,
        NotAccepted = 3,
        HaveCompleted = 4,
        HaveNotCompleted = 5,
        Class = 6,
        DaysOfWeek = 7,
    }

    public enum QuestType
    {
        General = 0,
        Daily = 1,
        Weekly = 2,
        Repeatable = 3,
        Story = 4,
        Account = 5
    }

    public enum QuestIcon
    {
        None = 0,

        New = 1,
        Incomplete = 2,
        Complete = 3,
    }

    public enum QuestRequirementType
    {
        MinLevel = 0,
        MaxLevel = 1,
        NotAccepted = 2,
        HaveCompleted = 3,
        HaveNotCompleted = 4,
        Class = 5,
    }

    public enum QuestTaskType
    {
        KillMonster = 0,
        GainItem = 1,
        Region = 2
    }

    public enum MovementEffect
    {
        None = 0,

        SpecialRepair = 1,
    }

    public enum SpellKey : byte
    {
        None,

        [Description("Key\n1")]
        Spell01,
        [Description("Key\n2")]
        Spell02,
        [Description("Key\n3")]
        Spell03,
        [Description("Key\n4")]
        Spell04,
        [Description("Key\n5")]
        Spell05,
        [Description("Key\n6")]
        Spell06,
        [Description("Key\n7")]
        Spell07,
        [Description("Key\n8")]
        Spell08,
        [Description("Key\n9")]
        Spell09,
        [Description("Key\n10")]
        Spell10,
        [Description("Key\n11")]
        Spell11,
        [Description("Key\n12")]
        Spell12,

        [Description("Key\n13")]
        Spell13,
        [Description("Key\n14")]
        Spell14,
        [Description("Key\n15")]
        Spell15,
        [Description("Key\n16")]
        Spell16,
        [Description("Key\n17")]
        Spell17,
        [Description("Key\n18")]
        Spell18,
        [Description("Key\n19")]
        Spell19,
        [Description("Key\n20")]
        Spell20,
        [Description("Key\n21")]
        Spell21,
        [Description("Key\n22")]
        Spell22,
        [Description("Key\n23")]
        Spell23,
        [Description("Key\n24")]
        Spell24,
    }

    public enum MonsterFlag
    {
        None = 0,

        [Description("骷髅")]
        Skeleton = 1,

        [Description("金骷髅")]
        JinSkeleton = 2,

        [Description("神兽")]
        Shinsu = 3,

        [Description("地狱士兵")]
        InfernalSoldier = 4,

        [Description("诅咒人偶")]
        CursedDoll = 5,

        [Description("召唤傀儡")]
        SummonPuppet = 6,

        [Description("镜像")]
        MirrorImage = 7,

        [Description("龙卷风")]
        Tornado = 8,

        [Description("亡灵之魂")]
        UndeadSoul = 9,

        [Description("城堡目标")]
        CastleObjective = 10,

        [Description("城堡防御")]
        CastleDefense = 11,

        [Description("阻挡物")]
        Blocker = 20,

        // 虫系 / 洞穴
        [Description("幼虫")]
        Larva = 100,

        [Description("楔蛾")]
        LesserWedgeMoth = 110,

        // 祖玛系
        [Description("祖玛弓箭手")]
        ZumaArcherMonster = 120,
        [Description("祖玛卫士")]
        ZumaGuardianMonster = 121,
        [Description("祖玛狂热者")]
        ZumaFanaticMonster = 122,
        [Description("祖玛守护者")]
        ZumaKeeperMonster = 123,

        // 骨系
        [Description("骷髅弓箭手")]
        BoneArcher = 130,
        [Description("骷髅队长")]
        BoneCaptain = 131,
        [Description("骷髅刀斧手")]
        BoneBladesman = 132,
        [Description("骷髅士兵")]
        BoneSoldier = 133,
        [Description("骷髅执法者")]
        SkeletonEnforcer = 134,

        // 昆虫系
        [Description("成熟蠼螋")]
        MatureEarwig = 140,
        [Description("金甲甲虫")]
        GoldenArmouredBeetle = 141,
        [Description("马陆")]
        Millipede = 142,

        // 火焰系
        [Description("狂暴火焰恶魔")]
        FerociousFlameDemon = 150,
        [Description("火焰恶魔")]
        FlameDemon = 151,

        // Goru 系（音译常用“哥鲁”）
        [Description("哥鲁枪兵")]
        GoruSpearman = 160,
        [Description("哥鲁弓箭手")]
        GoruArcher = 161,
        [Description("哥鲁将军")]
        GoruGeneral = 162,

        // 魔域/将领系（这些不同汉化包差异最大，这里给常见译法）
        [Description("龙王")]
        DragonLord = 170,
        [Description("五影兽")]
        OYoungBeast = 171,
        [Description("幽冥巫女")]
        YumgonWitch = 172,
        [Description("魔狱守卫")]
        MaWarden = 173,
        [Description("魔狱战将")]
        MaWarlord = 174,
        [Description("镇魂灵")]
        JinhwanSpirit = 175,
        [Description("镇魂护卫")]
        JinhwanGuardian = 176,
        [Description("五影将军")]
        OyoungGeneral = 177,
        [Description("幽冥将军")]
        YumgonGeneral = 178,

        [Description("班妖队长")]
        BanyoCaptain = 180,

        [Description("萨玛术士")]
        SamaSorcerer = 190,
        [Description("血石")]
        BloodStone = 191,

        // 石英洞（Quartz）系
        [Description("石英粉蝙蝠")]
        QuartzPinkBat = 200,
        [Description("石英蓝蝙蝠")]
        QuartzBlueBat = 201,
        [Description("石英蓝水晶")]
        QuartzBlueCrystal = 202,
        [Description("石英红兜帽")]
        QuartzRedHood = 203,
        [Description("石英小海龟")]
        QuartzMiniTurtle = 204,
        [Description("石英海龟")]
        QuartzTurtleSub = 205,

        [Description("献祭")]
        Sacrifice = 210

    }

    public enum FishingState : byte
    {
        None,
        Cast,
        Reel,
        Cancel
    }

    public enum HintPosition : byte
    {
        TopLeft,
        BottomLeft,

        FixedY,

        Fluid
    }

    #region Packet Enums

    public enum NewAccountResult : byte
    {
        Disabled,
        BadEMail,
        BadPassword,
        BadRealName,
        AlreadyExists,
        BadReferral,
        ReferralNotFound,
        ReferralNotActivated,
        Success
    }

    public enum ChangePasswordResult : byte
    {
        Disabled,
        BadEMail,
        BadCurrentPassword,
        BadNewPassword,
        AccountNotFound,
        AccountNotActivated,
        WrongPassword,
        Banned,
        Success
    }
    public enum RequestPasswordResetResult : byte
    {
        Disabled,
        BadEMail,
        AccountNotFound,
        AccountNotActivated,
        ResetDelay,
        Banned,
        Success
    }
    public enum ResetPasswordResult : byte
    {
        Disabled,
        AccountNotFound,
        BadNewPassword,
        KeyExpired,
        Success
    }


    public enum ActivationResult : byte
    {
        Disabled,
        AccountNotFound,
        Success,
    }

    public enum RequestActivationKeyResult : byte
    {
        Disabled,
        BadEMail,
        AccountNotFound,
        AlreadyActivated,
        RequestDelay,
        Success,
    }

    public enum LoginResult : byte
    {
        Disabled,
        BadEMail,
        BadPassword,
        AccountNotExists,
        AccountNotActivated,
        WrongPassword,
        Banned,
        AlreadyLoggedIn,
        AlreadyLoggedInPassword,
        AlreadyLoggedInAdmin,
        Success
    }

    public enum NewCharacterResult : byte
    {
        Disabled,
        BadCharacterName,
        BadGender,
        BadClass,
        BadHairType,
        BadHairColour,
        BadArmourColour,
        ClassDisabled,
        MaxCharacters,
        AlreadyExists,
        Success
    }

    public enum DeleteCharacterResult : byte
    {
        Disabled,
        AlreadyDeleted,
        NotFound,
        Success
    }

    public enum StartGameResult : byte
    {
        Disabled,
        Deleted,
        Delayed,
        UnableToSpawn,
        NotFound,
        Success
    }

    public enum DisconnectReason : byte
    {
        Unknown,
        TimedOut,
        WrongVersion,
        ServerClosing,
        AnotherUser,
        AnotherUserPassword,
        AnotherUserAdmin,
        Banned,
        Kicked,
        Crashed
    }

    public enum InstanceResult : byte
    {
        Invalid,
        InsufficientLevel,
        SafeZoneOnly,
        NotInGroup,
        NotInGuild,
        NotInCastle,
        TooFewInGroup,
        TooManyInGroup,
        ConnectRegionNotSet,
        NoSlots,
        NoRejoin,
        NotGroupLeader,
        UserCooldown,
        GuildCooldown,
        MissingItem,
        NoMap,
        Success
    }

    #endregion

    #region Sound

    public enum SoundIndex
    {
        None,
        LoginScene,
        SelectScene,

        #region Province Music
        B000 = 3,
        B2,
        B8,
        B009D,
        B009N,
        B0014D,
        B0014N,
        B100,
        B122,
        B300,
        B400,
        B14001,
        BD00,
        BD01,
        BD02,
        BD041,
        BD042,
        BD50,
        BD60,
        BD70,
        BD99,
        BD100,
        BD101,
        BD210,
        BD211,
        BDUnderseaCave,
        BDUnderseaCaveBoss,
        D3101,
        D3102,
        D3400,
        Dungeon_1,
        Dungeon_2,
        ID1_001,
        ID1_002,
        ID1_003,
        TS001,
        TS002,
        TS003,
        #endregion

        LoginScene2,
        LoginScene3,

        ButtonA = 100,
        ButtonB,
        ButtonC,

        SelectWarriorMale,
        SelectWarriorFemale,
        SelectWizardMale,
        SelectWizardFemale,
        SelectTaoistMale,
        SelectTaoistFemale,
        SelectAssassinMale,
        SelectAssassinFemale,

        TeleportOut,
        TeleportIn,

        ItemPotion,
        ItemWeapon,
        ItemArmour,
        ItemRing,
        ItemBracelet,
        ItemNecklace,
        ItemHelmet,
        ItemShoes,
        ItemDefault,

        GoldPickUp,
        GoldGained,

        RollDice,
        RollYut,

        DaggerSwing,
        WoodSwing,
        IronSwordSwing,
        ShortSwordSwing,
        AxeSwing,
        ClubSwing,
        WandSwing,
        FistSwing,
        GlaiveAttack,
        ClawAttack,

        MiningHit,
        MiningStruck,

        GenericStruckPlayer,
        GenericStruckMonster,

        Foot1,
        Foot2,
        Foot3,
        Foot4,

        FishingCast,
        FishingBob,
        FishingReel,

        HorseWalk1,
        HorseWalk2,
        HorseRun,

        MaleStruck,
        FemaleStruck,

        MaleDie,
        FemaleDie,

        QuestTake,
        QuestComplete,

        #region Magics

        SlayingMale,
        SlayingFemale,

        EnergyBlast,

        HalfMoon,

        FlamingSword,

        DragonRise,

        BladeStorm,

        DefensiveBlow,

        DestructiveSurge,

        DefianceStart,

        ReflectDamageStart,

        InvincibilityStart,

        AssaultStart,

        SwiftBladeEnd,

        SeismicSlam,

        ElementalSwordsStart,
        ElementalSwordsEnd,

        FireBallStart,
        FireBallTravel,
        FireBallEnd,

        ThunderBoltStart,
        ThunderBoltTravel,
        ThunderBoltEnd,

        IceBoltStart,
        IceBoltTravel,
        IceBoltEnd,

        GustBlastStart,
        GustBlastTravel,
        GustBlastEnd,

        RepulsionEnd,

        ElectricShockStart,
        ElectricShockEnd,

        GreaterFireBallStart,
        GreaterFireBallTravel,
        GreaterFireBallEnd,

        LightningStrikeStart,
        LightningStrikeEnd,

        GreaterIceBoltStart,
        GreaterIceBoltTravel,
        GreaterIceBoltEnd,

        CycloneStart,
        CycloneEnd,

        TeleportationStart,

        LavaStrikeStart,

        LightningBeamEnd,

        FrozenEarthStart,
        FrozenEarthEnd,

        BlowEarthStart,
        BlowEarthEnd,
        BlowEarthTravel,

        FireWallStart,
        FireWallDurationLong,
        FireWallDuration,

        ExpelUndeadStart,
        ExpelUndeadEnd,

        MagicShieldStart,

        FireStormStart,
        FireStormEnd,

        LightningWaveStart,
        LightningWaveEnd,

        IceStormStart,
        IceStormEnd,

        DragonTornadoStart,
        DragonTornadoEnd,

        GreaterFrozenEarthStart,
        GreaterFrozenEarthEnd,

        ChainLightningStart,
        ChainLightningEnd,

        TempestDuration,

        ParasiteTravel,
        ParasiteExplode,

        FrostBiteStart,

        ElementalHurricane,

        TornadoStart,

        HealStart,
        HealEnd,

        PoisonDustStart,
        PoisonDustEnd,

        ExplosiveTalismanStart,
        ExplosiveTalismanTravel,
        ExplosiveTalismanEnd,

        HolyStrikeStart,
        HolyStrikeTravel,
        HolyStrikeEnd,

        ImprovedHolyStrikeStart,
        ImprovedHolyStrikeTravel,
        ImprovedHolyStrikeEnd,

        MagicResistanceTravel,
        MagicResistanceEnd,

        ResilienceTravel,
        ResilienceEnd,

        ShacklingTalismanStart,
        ShacklingTalismanEnd,

        SummonSkeletonStart,
        SummonSkeletonEnd,

        CursedDollEnd,

        InvisibilityEnd,

        MassInvisibilityTravel,
        MassInvisibilityEnd,

        TaoistCombatKickStart,

        MassHealStart,
        MassHealEnd,

        BloodLustTravel,
        BloodLustEnd,

        ResurrectionStart,

        PurificationStart,
        PurificationEnd,

        SummonShinsuStart,
        SummonShinsuEnd,

        StrengthOfFaithStart,
        StrengthOfFaithEnd,

        NeutralizeTravel,
        NeutralizeEnd,

        DarkSoulPrison,

        CorpseExploderEnd,

        SummonDeadEnd,

        PoisonousCloudStart,

        CloakStart,

        WraithGripStart,
        WraithGripEnd,

        HellFireStart,

        FullBloom,
        WhiteLotus,
        RedLotus,
        SweetBrier,
        SweetBrierMale,
        SweetBrierFemale,

        WaningMoon,

        CalamityOfFullMoon,

        RakeStart,

        Karma,

        TheNewBeginning,
        Concentration,

        SummonPuppet,

        DanceOfSwallowsEnd,
        DragonRepulseStart,
        AbyssStart,
        FlashOfLightEnd,
        EvasionStart,
        RagingWindStart,

        Hemorrhage,
        ChainofFireExplode,

        HundredFist,
        OffensiveBlow,

        IceAuraTravel,
        IceDragonTravel,
        IceDragonBreak,
        BindingTalisman,
        BrainStorm,
        FlamingDaggers,
        Shredding,

        TaecheonSword,
        FireSword,
        IceBreaker,
        FrozenDragon,
        HeavenlySky,
        PoisonCloud,
        FourWheels,
        CrescentMoon,

        #endregion

        #region Monsters

        ChickenAttack,
        ChickenStruck,
        ChickenDie,

        PigAttack,
        PigStruck,
        PigDie,

        DeerAttack,
        DeerStruck,
        DeerDie,

        CowAttack,
        CowStruck,
        CowDie,

        SheepAttack,
        SheepStruck,
        SheepDie,

        SkyStingerAttack,
        SkyStingerStruck,
        SkyStingerDie,

        ClawCatAttack,
        ClawCatStruck,
        ClawCatDie,

        WolfAttack,
        WolfStruck,
        WolfDie,

        ForestYetiAttack,
        ForestYetiStruck,
        ForestYetiDie,

        CarnivorousPlantAttack,
        CarnivorousPlantStruck,
        CarnivorousPlantDie,

        YobAttack,
        YobStruck,
        YobDie,

        OmaAttack,
        OmaStruck,
        OmaDie,

        TigerSnakeAttack,
        TigerSnakeStruck,
        TigerSnakeDie,

        SpittingSpiderAttack,
        SpittingSpiderStruck,
        SpittingSpiderDie,

        ScarecrowAttack,
        ScarecrowStruck,
        ScarecrowDie,

        OmaHeroAttack,
        OmaHeroStruck,
        OmaHeroDie,

        CaveBatAttack,
        CaveBatStruck,
        CaveBatDie,

        ScorpionAttack,
        ScorpionStruck,
        ScorpionDie,

        SkeletonAttack,
        SkeletonStruck,
        SkeletonDie,

        SkeletonAxeManAttack,
        SkeletonAxeManStruck,
        SkeletonAxeManDie,

        SkeletonAxeThrowerAttack,
        SkeletonAxeThrowerStruck,
        SkeletonAxeThrowerDie,

        SkeletonWarriorAttack,
        SkeletonWarriorStruck,
        SkeletonWarriorDie,

        SkeletonLordAttack,
        SkeletonLordStruck,
        SkeletonLordDie,

        CaveMaggotAttack,
        CaveMaggotStruck,
        CaveMaggotDie,

        GhostSorcererAttack,
        GhostSorcererStruck,
        GhostSorcererDie,

        GhostMageAppear,
        GhostMageAttack,
        GhostMageStruck,
        GhostMageDie,

        VoraciousGhostAttack,
        VoraciousGhostStruck,
        VoraciousGhostDie,

        GhoulChampionAttack,
        GhoulChampionStruck,
        GhoulChampionDie,

        ArmoredAntAttack,
        ArmoredAntStruck,
        ArmoredAntDie,

        AntNeedlerAttack,
        AntNeedlerStruck,
        AntNeedlerDie,

        KeratoidAttack,
        KeratoidStruck,
        KeratoidDie,

        ShellNipperAttack,
        ShellNipperStruck,
        ShellNipperDie,

        VisceralWormAttack,
        VisceralWormStruck,
        VisceralWormDie,

        MutantFleaAttack,
        MutantFleaStruck,
        MutantFleaDie,

        PoisonousMutantFleaAttack,
        PoisonousMutantFleaStruck,
        PoisonousMutantFleaDie,

        BlasterMutantFleaAttack,
        BlasterMutantFleaStruck,
        BlasterMutantFleaDie,

        WasHatchlingAttack,
        WasHatchlingStruck,
        WasHatchlingDie,

        CentipedeAttack,
        CentipedeStruck,
        CentipedeDie,

        ButterflyWormAttack,
        ButterflyWormStruck,
        ButterflyWormDie,

        MutantMaggotAttack,
        MutantMaggotStruck,
        MutantMaggotDie,

        EarwigAttack,
        EarwigStruck,
        EarwigDie,

        IronLanceAttack,
        IronLanceStruck,
        IronLanceDie,

        LordNiJaeAttack,
        LordNiJaeStruck,
        LordNiJaeDie,

        RottingGhoulAttack,
        RottingGhoulStruck,
        RottingGhoulDie,

        DecayingGhoulAttack,
        DecayingGhoulStruck,
        DecayingGhoulDie,

        BloodThirstyGhoulAttack,
        BloodThirstyGhoulStruck,
        BloodThirstyGhoulDie,


        SpinedDarkLizardAttack,
        SpinedDarkLizardStruck,
        SpinedDarkLizardDie,

        DungAttack,
        DungStruck,
        DungDie,

        UmaInfidelAttack,
        UmaInfidelStruck,
        UmaInfidelDie,

        UmaFlameThrowerAttack,
        UmaFlameThrowerStruck,
        UmaFlameThrowerDie,

        UmaAnguisherAttack,
        UmaAnguisherStruck,
        UmaAnguisherDie,

        UmaKingAttack,
        UmaKingStruck,
        UmaKingDie,

        UmaMaceInfidelAttack,
        UmaMaceInfidelStruck,
        UmaMaceInfidelDie,

        SpiderBatAttack,
        SpiderBatStruck,
        SpiderBatDie,

        ArachnidGazerStruck,
        ArachnidGazerDie,

        LarvaAttack,
        LarvaStruck,

        RedMoonGuardianAttack,
        RedMoonGuardianStruck,
        RedMoonGuardianDie,

        RedMoonProtectorAttack,
        RedMoonProtectorStruck,
        RedMoonProtectorDie,

        VenomousArachnidAttack,
        VenomousArachnidStruck,
        VenomousArachnidDie,

        DarkArachnidAttack,
        DarkArachnidStruck,
        DarkArachnidDie,

        RedMoonTheFallenAttack,
        RedMoonTheFallenStruck,
        RedMoonTheFallenDie,


        ViciousRatAttack,
        ViciousRatStruck,
        ViciousRatDie,

        ZumaSharpShooterAttack,
        ZumaSharpShooterStruck,
        ZumaSharpShooterDie,

        ZumaFanaticAttack,
        ZumaFanaticStruck,
        ZumaFanaticDie,

        ZumaGuardianAttack,
        ZumaGuardianStruck,
        ZumaGuardianDie,

        ZumaKingAppear,
        ZumaKingAttack,
        ZumaKingStruck,
        ZumaKingDie,

        EvilFanaticAttack,
        EvilFanaticStruck,
        EvilFanaticDie,

        MonkeyAttack,
        MonkeyStruck,
        MonkeyDie,

        EvilElephantAttack,
        EvilElephantStruck,
        EvilElephantDie,

        CannibalFanaticAttack,
        CannibalFanaticStruck,
        CannibalFanaticDie,

        SpikedBeetleAttack,
        SpikedBeetleStruck,
        SpikedBeetleDie,

        NumaGruntAttack,
        NumaGruntStruck,
        NumaGruntDie,

        NumaMageAttack,
        NumaMageStruck,
        NumaMageDie,

        NumaEliteAttack,
        NumaEliteStruck,
        NumaEliteDie,

        SandSharkAttack,
        SandSharkStruck,
        SandSharkDie,

        StoneGolemAppear,
        StoneGolemAttack,
        StoneGolemStruck,
        StoneGolemDie,

        WindfurySorceressAttack,
        WindfurySorceressStruck,
        WindfurySorceressDie,

        CursedCactusAttack,
        CursedCactusStruck,
        CursedCactusDie,

        RagingLizardAttack,
        RagingLizardStruck,
        RagingLizardDie,

        SawToothLizardAttack,
        SawToothLizardStruck,
        SawToothLizardDie,

        MutantLizardAttack,
        MutantLizardStruck,
        MutantLizardDie,

        VenomSpitterAttack,
        VenomSpitterStruck,
        VenomSpitterDie,

        SonicLizardAttack,
        SonicLizardStruck,
        SonicLizardDie,

        GiantLizardAttack,
        GiantLizardStruck,
        GiantLizardDie,

        CrazedLizardAttack,
        CrazedLizardStruck,
        CrazedLizardDie,

        TaintedTerrorAttack,
        TaintedTerrorStruck,
        TaintedTerrorDie,
        TaintedTerrorAttack2,

        DeathLordJichonAttack,
        DeathLordJichonStruck,
        DeathLordJichonDie,
        DeathLordJichonAttack2,
        DeathLordJichonAttack3,

        MinotaurAttack,
        MinotaurStruck,
        MinotaurDie,

        FrostMinotaurAttack,
        FrostMinotaurStruck,
        FrostMinotaurDie,

        BanyaLeftGuardAttack,
        BanyaLeftGuardStruck,
        BanyaLeftGuardDie,

        EmperorSaWooAttack,
        EmperorSaWooStruck,
        EmperorSaWooDie,

        BoneArcherAttack,
        BoneArcherStruck,
        BoneArcherDie,

        BoneCaptainAttack,
        BoneCaptainStruck,
        BoneCaptainDie,

        ArchLichTaeduAttack,
        ArchLichTaeduStruck,
        ArchLichTaeduDie,

        WedgeMothLarvaAttack,
        WedgeMothLarvaStruck,
        WedgeMothLarvaDie,

        LesserWedgeMothAttack,
        LesserWedgeMothStruck,
        LesserWedgeMothDie,

        WedgeMothAttack,
        WedgeMothStruck,
        WedgeMothDie,

        RedBoarAttack,
        RedBoarStruck,
        RedBoarDie,

        ClawSerpentAttack,
        ClawSerpentStruck,
        ClawSerpentDie,

        BlackBoarAttack,
        BlackBoarStruck,
        BlackBoarDie,

        TuskLordAttack,
        TuskLordStruck,
        TuskLordDie,

        RazorTuskAttack,
        RazorTuskStruck,
        RazorTuskDie,

        PinkGoddessAttack,
        PinkGoddessStruck,
        PinkGoddessDie,

        GreenGoddessAttack,
        GreenGoddessStruck,
        GreenGoddessDie,

        MutantCaptainAttack,
        MutantCaptainStruck,
        MutantCaptainDie,

        StoneGriffinAttack,
        StoneGriffinStruck,
        StoneGriffinDie,

        FlameGriffinAttack,
        FlameGriffinStruck,
        FlameGriffinDie,

        WhiteBoneAttack,
        WhiteBoneStruck,
        WhiteBoneDie,

        ShinsuSmallStruck,
        ShinsuSmallDie,

        ShinsuBigAttack,
        ShinsuBigStruck,
        ShinsuBigDie,

        ShinsuShow,

        CorpseStalkerAttack,
        CorpseStalkerStruck,
        CorpseStalkerDie,

        LightArmedSoldierAttack,
        LightArmedSoldierStruck,
        LightArmedSoldierDie,

        CorrosivePoisonSpitterAttack,
        CorrosivePoisonSpitterStruck,
        CorrosivePoisonSpitterDie,

        PhantomSoldierAttack,
        PhantomSoldierStruck,
        PhantomSoldierDie,

        MutatedOctopusAttack,
        MutatedOctopusStruck,
        MutatedOctopusDie,

        AquaLizardAttack,
        AquaLizardStruck,
        AquaLizardDie,

        CrimsonNecromancerAttack,
        CrimsonNecromancerStruck,
        CrimsonNecromancerDie,

        ChaosKnightAttack,
        ChaosKnightStruck,
        ChaosKnightDie,

        PachontheChaosbringerAttack,
        PachontheChaosbringerStruck,
        PachontheChaosbringerDie,


        NumaCavalryAttack,
        NumaCavalryStruck,
        NumaCavalryDie,

        NumaHighMageAttack,
        NumaHighMageStruck,
        NumaHighMageDie,

        NumaStoneThrowerAttack,
        NumaStoneThrowerStruck,
        NumaStoneThrowerDie,

        NumaRoyalGuardAttack,
        NumaRoyalGuardStruck,
        NumaRoyalGuardDie,

        NumaArmoredSoldierAttack,
        NumaArmoredSoldierStruck,
        NumaArmoredSoldierDie,

        IcyRangerAttack,
        IcyRangerStruck,
        IcyRangerDie,

        IcyGoddessAttack,
        IcyGoddessStruck,
        IcyGoddessDie,

        IcySpiritWarriorAttack,
        IcySpiritWarriorStruck,
        IcySpiritWarriorDie,

        IcySpiritGeneralAttack,
        IcySpiritGeneralStruck,
        IcySpiritGeneralDie,

        GhostKnightAttack,
        GhostKnightStruck,
        GhostKnightDie,

        IcySpiritSpearmanAttack,
        IcySpiritSpearmanStruck,
        IcySpiritSpearmanDie,

        WerewolfAttack,
        WerewolfStruck,
        WerewolfDie,

        WhitefangAttack,
        WhitefangStruck,
        WhitefangDie,

        IcySpiritSoliderAttack,
        IcySpiritSoliderStruck,
        IcySpiritSoliderDie,

        WildBoarAttack,
        WildBoarStruck,
        WildBoarDie,

        FrostLordHwaAttack,
        FrostLordHwaStruck,
        FrostLordHwaDie,

        JinchonDevilAttack,
        JinchonDevilAttack2,
        JinchonDevilAttack3,
        JinchonDevilStruck,
        JinchonDevilDie,

        EscortCommanderAttack,
        EscortCommanderStruck,
        EscortCommanderDie,

        FieryDancerAttack,
        FieryDancerStruck,
        FieryDancerDie,

        EmeraldDancerAttack,
        EmeraldDancerStruck,
        EmeraldDancerDie,

        QueenOfDawnAttack,
        QueenOfDawnStruck,
        QueenOfDawnDie,

        OYoungBeastAttack,
        OYoungBeastStruck,
        OYoungBeastDie,

        YumgonWitchAttack,
        YumgonWitchStruck,
        YumgonWitchDie,

        MaWarlordAttack,
        MaWarlordStruck,
        MaWarlordDie,

        JinhwanSpiritAttack,
        JinhwanSpiritStruck,
        JinhwanSpiritDie,

        JinhwanGuardianAttack,
        JinhwanGuardianStruck,
        JinhwanGuardianDie,

        YumgonGeneralAttack,
        YumgonGeneralStruck,
        YumgonGeneralDie,

        ChiwooGeneralAttack,
        ChiwooGeneralStruck,
        ChiwooGeneralDie,

        DragonQueenAttack,
        DragonQueenStruck,
        DragonQueenDie,

        DragonLordAttack,
        DragonLordStruck,
        DragonLordDie,

        FerociousIceTigerAttack,
        FerociousIceTigerStruck,
        FerociousIceTigerDie,

        SamaFireGuardianAttack,
        SamaFireGuardianStruck,
        SamaFireGuardianDie,

        SamaIceGuardianAttack,
        SamaIceGuardianStruck,
        SamaIceGuardianDie,

        SamaLightningGuardianAttack,
        SamaLightningGuardianStruck,
        SamaLightningGuardianDie,

        SamaWindGuardianAttack,
        SamaWindGuardianStruck,
        SamaWindGuardianDie,

        PhoenixAttack,
        PhoenixStruck,
        PhoenixDie,

        BlackTortoiseAttack,
        BlackTortoiseStruck,
        BlackTortoiseDie,

        BlueDragonAttack,
        BlueDragonStruck,
        BlueDragonDie,

        WhiteTigerAttack,//TODO - missing sound
        WhiteTigerStruck,//TODO - missing sound
        WhiteTigerDie,//TODO - missing sound

        Terracotta1Attack,
        Terracotta1Struck,
        Terracotta1Die,

        Terracotta2Attack,
        Terracotta2Struck,
        Terracotta2Die,

        Terracotta3Attack,
        Terracotta3Struck,
        Terracotta3Die,

        Terracotta4Attack,
        Terracotta4Struck,
        Terracotta4Die,

        TerracottaSubAttack,
        TerracottaSubAttack2,
        TerracottaSubStruck,
        TerracottaSubDie,

        TerracottaBossAttack,
        TerracottaBossAttack2,
        TerracottaBossStruck,
        TerracottaBossDie,

        #endregion
    }

    #endregion

}