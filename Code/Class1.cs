using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player1
{

    static void Main(string[] args)
    {
        string[] inputs;

        int numSites = int.Parse(Console.ReadLine());
        var sites = new Site[numSites];
        for (int i = 0; i < numSites; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int siteId = int.Parse(inputs[0]);
            int x = int.Parse(inputs[1]);
            int y = int.Parse(inputs[2]);
            int radius = int.Parse(inputs[3]);
            sites[i] = new Site();
            sites[i].siteId = siteId;
            sites[i].x = x;
            sites[i].y = y;
            sites[i].r = radius;

        }
        var maxTower = 3;
        var towerAlterer = 0;
        Site barrack = null;
        var minIncome = 6;
        Site currentMine = null;


        // game loop
        while (true)
        {
            var built = false;
            Console.Error.WriteLine("Start");
            inputs = Console.ReadLine().Split(' ');
            Console.Error.WriteLine("read input");
            int gold = int.Parse(inputs[0]);
            int touchedSite = int.Parse(inputs[1]); // -1 if none
            Console.Error.WriteLine("setting current site status");
            for (int i = 0; i < numSites; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int siteId = int.Parse(inputs[0]);
                int sitegold = int.Parse(inputs[1]); // used in future leagues
                int maxMineSize = int.Parse(inputs[2]); // used in future leagues
                int structureType = int.Parse(inputs[3]); // -1 = No structure, 2 = Barracks
                int owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
                int param1 = int.Parse(inputs[5]);
                int param2 = int.Parse(inputs[6]);

                sites[i].gold = sitegold;
                sites[i].maxMineSize = maxMineSize;
                sites[i].owner = owner;
                sites[i].structureType = structureType;
                sites[i].param1 = param1;
                sites[i].param2 = param2;
            }

            int numUnits = int.Parse(Console.ReadLine());
            var units = new Unit[numUnits];
            for (int i = 0; i < numUnits; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                int owner = int.Parse(inputs[2]);
                int unitType = int.Parse(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
                int health = int.Parse(inputs[4]);
                units[i] = new Unit()
                {
                    x = x,
                    y = y,
                    owner = owner,
                    type = unitType,
                    health = health
                };

            }

            var queen = units.First(x => x.type == -1 && x.owner == 0);
            foreach (var site in sites)
            {
                var dist = Math.Abs(queen.x - site.x) + Math.Abs(queen.y - site.y);
                site.dist = dist;
            }

            Console.Error.WriteLine("pre barrack");
            barrack = sites.FirstOrDefault(x => x.owner == 0 && x.structureType == 2);
            var towers = sites.Where(x => x.owner == 0 && x.structureType == 1).Select(x => x).ToArray();
            var closestSite = sites.Where(x => x.structureType == -1).OrderBy(x => x.dist).First();
            var mines = sites.Where(x => x.owner == 0 && x.structureType == 0).Select(x => x).ToArray();
            var closestGold = sites.Where(x => x.structureType == -1 && x.gold != 0).OrderBy(x => x.dist).First();

            var b=DoBuild(gold, touchedSite, sites, units);
           var t= DoTrain(gold, touchedSite, sites, units);

        }
    }

    private static bool DoTrain(int gold, int touchedSite, Site[] sites, Unit[] units)
    {
        var attackers = sites.Where(x => x.owner == 0 && x.structureType == 2&&x.param2!=1).ToList();
        var defenders= sites.Where(x => x.owner == 0 && x.structureType == 2 && x.param2 == 1).ToList();
        var towers= sites.Where(x => x.owner == 0 && x.structureType == 1).Select(x => x).ToArray();
        var archers = units.Count(x => x.owner == 0 && x.type == 1);
        var enemyKnights= units.Count(x => x.owner == 1 && x.type == 0);

        if (gold > 400 && attackers.Count > 2)
        {
            Console.WriteLine($"TRAIN {attackers[0].siteId} {attackers[1].siteId} {attackers[2].siteId}");
            return true;
        }

        if (towers.Length < 4 && gold > 100 && archers < 2 && enemyKnights > 1)
        {
            Console.WriteLine($"TRAIN {defenders[0].siteId}");
            return true;
        }

        Console.WriteLine($"TRAIN");
        return true;
    }

    public static bool DoBuild(int gold, int tuchedSite, Site[] sites, Unit[] unit)
    {
        var barracks = sites.Where(x => x.owner == 0 && x.structureType == 2).ToList();
        var towers = sites.Where(x => x.owner == 0 && x.structureType == 1).Select(x => x).ToArray();
        var closestSite = sites.Where(x => x.structureType == -1).OrderBy(x => x.dist).First();
        var mines = sites.Where(x => x.owner == 0 && x.structureType == 0).Select(x => x).ToArray();
        var closestMine = mines.OrderBy(x => x.dist).FirstOrDefault();
        var closestGold = sites.Where(x => x.structureType == -1 && x.gold != 0).OrderBy(x => x.dist).First();
       

        if (!barracks.Any())
        {
            Console.WriteLine($"BUILD {closestSite.siteId} BARRACKS-ARCHER");
            return true;
        }
        if (gold < 400)
        {
            BuildMine(closestMine,closestGold);
            return true;
        }
        if (gold >= 400)
        {
            BuildBarrack(barracks, closestSite);
            return true;
        }

        BuildTowers(towers, closestSite);
        return true;
    }

    private static void BuildTowers(Site[] towers, Site closestSite)
    {
        var closestTower = towers.OrderBy(x => x.dist).FirstOrDefault();
        if (closestTower!=null&&closestTower.param1 < 700)
        {
            Console.WriteLine($"BUILD {closestTower.siteId} TOWER");
        }

        Console.WriteLine($"BUILD {closestSite.siteId} TOWER");
    }

    private static void BuildBarrack(List<Site> barracks, Site closestSite)
    {
        if (barracks.Count(x => x.param2 == 0) < 2)
        {
            Console.WriteLine($"BUILD {closestSite.siteId} BARRACKS-KNIGHT");
        }

        if (barracks.Count(x => x.param2 == 2) < 1)
        {
            Console.WriteLine($"BUILD {closestSite.siteId} BARRACKS-GIANT");
        }
        Console.WriteLine($"BUILD {closestSite.siteId} BARRACKS-TOWER");
    }


    private static void BuildMine(Site closestMine, Site closestGold)
    {
        Console.Error.WriteLine(closestMine.maxMineSize +" "+ closestMine.param1);
        if (closestMine!=null &&closestMine.maxMineSize < closestMine.param1)
        {
            Console.WriteLine($"BUILD {closestMine.siteId} MINE");
            return;
        }
        Console.WriteLine($"BUILD {closestGold.siteId} MINE");
    }


    public class Site
    {
        public int siteId { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int r { get; set; }
        public int dist { get; set; }
        public int structureType { get; set; }
        public int owner { get; set; }
        public int gold { get; set; }
        public int maxMineSize { get; set; }
        public int param1 { get; set; }
        public int param2 { get; set; }
    }

    public class Unit
    {

        public int y { get; set; }
        public int x { get; set; }
        public int owner { get; set; }
        public int health { get; set; }
        public int type { get; set; }
    }
}