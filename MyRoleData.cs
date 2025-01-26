namespace RoleSelection;

public class MyRoleData
{
    public string AccAndSlot { get; set; }
    public string Role { get; set; }
    public int Account { get; set; }
    public string Name { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Mana { get; set; }
    public int MaxMana { get; set; }
    public string Inventory { get; set; }
    public bool extraSlot { get; set; }
    public int spawnX { get; set; }
    public int spawnY { get; set; }
    public int skinVariant { get; set; }
    public int hair { get; set; }
    public byte hairDye { get; set; }
    public long hairColor { get; set; }
    public long pantsColor { get; set; }
    public long shirtColor { get; set; }
    public long underShirtColor { get; set; }
    public long shoeColor { get; set; }
    public bool[] hideVisuals { get; set; }
    public long skinColor { get; set; }
    public long eyeColor { get; set; }
    public int questsCompleted { get; set; }
    public bool usingBiomeTorches { get; set; }
    public bool happyFunTorchTime { get; set; }
    public bool unlockedBiomeTorches { get; set; }
    public int currentLoadoutIndex { get; set; }
    public bool ateArtisanBread { get; set; }
    public bool usedAegisCrystal { get; set; }
    public bool usedAegisFruit { get; set; }
    public bool usedArcaneCrystal { get; set; }
    public bool usedGalaxyPearl { get; set; }
    public bool usedGummyWorm { get; set; }
    public bool usedAmbrosia { get; set; }
    public bool unlockedSuperCart { get; set; }
    public bool enabledSuperCart { get; set; }

    public MyRoleData(string accAndSlot, string role, int account, string name, int health, int maxHealth, int mana, int maxMana, string inventoryString, bool extraslot, int spawnx, int spawny, int skinvariant, int hairs, byte hairdye, long haircolor, long pantscolor, long shirtcolor, long undershirtcolor, long shoecolor, bool[] hidevisuals, long skincolor, long eyecolor, int questscompleted, bool usingbiometorches, bool happyfuntorchtime, bool unlockedbiometorches, int currentloadoutindex, bool ateartisanbread, bool usedaegiscrystal, bool usedaegisfruit, bool usedarcanecrystal, bool usedgalaxypearl, bool usedgummyworm, bool usedambrosia, bool unlockedsupercart, bool enabledsupercart)
    {
        AccAndSlot = accAndSlot;
        Role = role;
        Account = account;
        Name = name;
        Health = health;
        MaxHealth = maxHealth;
        Mana = mana;
        MaxMana = maxMana;
        Inventory = inventoryString;
        extraSlot = extraslot;
        spawnX = spawnx;
        spawnY = spawny;
        skinVariant = skinvariant;
        hair = hairs;
        hairDye = hairdye;
        hairColor = haircolor;
        pantsColor = pantscolor;
        shirtColor = shirtcolor;
        underShirtColor = undershirtcolor;
        shoeColor = shoecolor;
        hideVisuals = hidevisuals;
        skinColor = skincolor;
        eyeColor = eyecolor;
        questsCompleted = questscompleted;
        usingBiomeTorches = usingbiometorches;
        happyFunTorchTime = happyfuntorchtime;
        unlockedBiomeTorches = unlockedbiometorches;
        currentLoadoutIndex = currentloadoutindex;
        ateArtisanBread = ateartisanbread;
        usedAegisCrystal = usedaegiscrystal;
        usedAegisFruit = usedaegisfruit;
        usedArcaneCrystal = usedarcanecrystal;
        usedGalaxyPearl = usedgalaxypearl;
        usedGummyWorm = usedgummyworm;
        usedAmbrosia = usedambrosia;
        unlockedSuperCart = unlockedsupercart;
        enabledSuperCart = enabledsupercart;
    }
}
