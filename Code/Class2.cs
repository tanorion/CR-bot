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
class Player
{




    static void Main(string[] args)
    {
        var player = new Play();
        player.Game();

    }

    public class Play
    {
        public int startX = -1;
        public int startY = -1;
        public Site latestSite = null;
        public int leastMines = -1;
        public int maxTowers = 3;
        public int numSites = -1;
        public int queenStartHp = -1;
        public int aimBarrack = -1;
        public bool movedToEnd = false;
        public void Game()
        {

            string[] inputs;

            numSites = int.Parse(Console.ReadLine());
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

            // game loop
            while (true)
            {
                var state = GetState(sites);

                Rush(state);
                //var b = this.DoBuild(state.Gold, state.TouchedSite, state.Sites, state.Units);
                //var t = DoTrain(state.Gold, state.TouchedSite, state.Sites, state.Units);
                if (state.TouchedSite != -1 && sites[state.TouchedSite].structureType != -1)
                {
                    latestSite = sites[state.TouchedSite];
                }

            }
        }



        public State GetState(Site[] sites)
        {
            var state = new State();
            string[] inputs;
            var built = false;

            inputs = Console.ReadLine().Split(' ');
            state.Turn++;
            state.Gold = int.Parse(inputs[0]);
            state.TouchedSite = int.Parse(inputs[1]); // -1 if none
            Console.Error.WriteLine($"setting current site status. TouchedSite: {state.TouchedSite}");


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

                if (sitegold != -1)
                {
                    sites[i].gold = sitegold;
                }
               
                
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
            state.Queen = queen;
            state.EnemyQueen = units.First(x => x.type == -1 && x.owner == 1);
            if (queenStartHp == -1)
            {
                queenStartHp = queen.health;
            }

            if (startX == -1)
            {
                if (queen.x < 1000)
                {
                    startX = 0;
                }
                else
                {
                    startX = 1920;
                }
                Console.Error.WriteLine($"StartX: {startX}");
            }

            if (startY == -1)
            {
                if (queen.y < 500)
                {
                    startY = 0;
                }
                else
                {
                    startY = 1000;
                }
                Console.Error.WriteLine($"StartY: {startY}");
            }

            foreach (var site in sites)
            {

                site.dist = DistansTo(queen.x, queen.y, site.x, site.y);
                site.distXstart = Math.Abs(startX - site.x);
                Console.Error.WriteLine($"id:{site.siteId} dist:{site.dist} owner:{site.owner} type: {site.structureType} param1:{site.param1} maxMine:{site.maxMineSize}");
            }
            SetLeastMines(queen);
            state.Sites = sites.ToList();
            state.Units = units.ToList();
            return state;

        }

        private void Rush(State state)
        {
            if (aimBarrack == -1)
            {
                var skip = 4; //state.Sites.Count(x => Math.Abs(x.y - startY) < 400) - 6;

                if (queenStartHp < 36)
                {
                    skip = 0;// state.Sites.Count(x => Math.Abs(x.y - startY) < 400) - 10;
                }
                else if (queenStartHp < 61)
                {
                    skip = 2;// state.Sites.Count(x => Math.Abs(x.y - startY) < 400) - 8;
                }
                else if (queenStartHp < 80)
                {
                    skip = 3;// state.Sites.Count(x => Math.Abs(x.y - startY) < 400) - 8;
                }
                if (skip > state.Sites.Count(x => Math.Abs(x.y - startY) < 400))
                {
                    skip = 1;
                }
                aimBarrack = state.Sites.Where(x => Math.Abs(x.y - startY) < 400).OrderBy(x => x.distXstart).Skip(skip).First().siteId;
                Console.Error.WriteLine("aimbarrack: " + aimBarrack + " skip: " + skip);
            }




            var barracks = state.Sites.Where(x => x.owner == 0 && x.structureType == 2);
            var mines = state.Sites.Where(x => x.owner == 0 && x.structureType == 0);

            if (state.EnemyQueen.health < state.Queen.health && state.Sites.Any(x => x.owner == 1 && x.structureType == 2))
            {
                if (state.Units.Any(x => x.owner == 1 && x.type == 0))
                {
                    SmallDefend(state);
                    Train(barracks);
                    return;
                }

                SmallDefend(state, 6);
                Train(barracks);
                return;

            }
            if (!barracks.Any())
            {
                if (state.TouchedSite != -1 && state.TouchedSite != aimBarrack && ((state.Sites[state.TouchedSite].structureType == -1 && state.Sites[state.TouchedSite].owner == -1)
                   || (state.Sites[state.TouchedSite].structureType == 0 && state.Sites[state.TouchedSite].owner == 0 && state.Sites[state.TouchedSite].maxMineSize > state.Sites[state.TouchedSite].param1)
                    || (state.Sites[state.TouchedSite].structureType != 1 && state.Sites[state.TouchedSite].owner == 1))
                    && state.Units.Count(x => x.owner == 1 && x.type == 0) == 0)
                {
                    Build(state, state.Sites[state.TouchedSite], "MINE");
                    Train();
                    return;
                }
                Build(state, state.Sites[aimBarrack], "BARRACKS-KNIGHT");
                Train();
                return;
            }
            var goodTowers = state.Sites.Where(x => x.structureType != 1 && x.siteId != aimBarrack && Math.Abs(x.x - startX) > Math.Abs(state.Queen.x - startX) && (x.owner != 0) && !InEnemyTowerRange(state, x)).OrderBy(x => x.dist);

            Console.Error.WriteLine("enemy knights: " + state.Units.Count(x => x.owner == 1 && x.type == 0));

            if (state.Units.Count(x => x.owner == 1 && x.type == 0) > 2 && state.Queen.health < 70 || state.EnemyQueen.health < state.Queen.health)
            {
                goodTowers = state.Sites.Where(x => x.structureType != 1 && x.siteId != aimBarrack && Math.Abs(x.x - startX) < Math.Abs(state.Queen.x - startX) && (x.owner != 0) && !InEnemyTowerRange(state, x)).OrderBy(x => x.dist);
                if (goodTowers.Count() == 0)
                {
                    SmallDefend(state);
                    Train(barracks);
                    return;
                }

            }
            if (goodTowers.Count() < 3 || movedToEnd)
            {
                Console.Error.WriteLine("MovedToEnd");
                movedToEnd = true;
                goodTowers = state.Sites.Where(x => x.structureType != 1 && !(x.structureType == 0 && x.owner == 0 && x.maxMineSize == x.param1) && !(x.structureType == 2 && x.owner == 0) && !InEnemyTowerRange(state, x)).OrderBy(x => x.dist);
                if (!goodTowers.Any())
                {
                    goodTowers = state.Sites.Where(x => x.structureType == 1 && x.owner == 0 && !InEnemyTowerRange(state, x)).OrderBy(x => x.dist);

                }
            }
            foreach (var t in goodTowers)
            {
                Console.Error.WriteLine("goodtower: " + t.siteId);

            }

            var closeEnemyBarrack = state.Sites.FirstOrDefault(x =>
                x.owner == 1 && x.structureType == 2 && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 500 && !InEnemyTowerRange(state, x));
            if (closeEnemyBarrack != null && state.Units.Count(x => x.owner == 1 && x.type == 0) < 2)
            {
                Build(state, closeEnemyBarrack, "TOWER");
                Train(barracks);
                return;
            }

            if (mines.Count() < 6 && state.Units.Any(x => x.owner == 1 && x.type == 0&&DistansTo(x.x,x.y,state.Queen.x,state.Queen.y)<300))
            {
                var goodMines = goodTowers.Where(x => x.gold != 0);
                foreach (var t in goodMines)
                {
                    Console.Error.WriteLine("goodMines: " + t.siteId);

                }
                if (state.TouchedSite != -1 && (state.Sites[state.TouchedSite].structureType == 0 && state.Sites[state.TouchedSite].owner == 0 &&
                     state.Sites[state.TouchedSite].maxMineSize > state.Sites[state.TouchedSite].param1 && state.Sites[state.TouchedSite].gold > 0))
                {
                    Build(state, state.Sites[state.TouchedSite], "MINE");
                    Train(barracks);
                    return;
                }

                if (goodMines.Any())
                {
                    Build(state, goodMines.First(), "MINE");
                    Train(barracks);
                    return;
                }
            }

            if (state.TouchedSite != -1 && state.Sites[state.TouchedSite].param1 < 700 &&
                state.Sites[state.TouchedSite].owner == 0 && state.Sites[state.TouchedSite].structureType == 1)
            {
                Build(state, state.Sites[state.TouchedSite], "TOWER");
                Train(barracks);
                return; 
            }

            Build(state, goodTowers.First(), "TOWER");
            Train(barracks);
            return; 
        }

        private void Train(IEnumerable<Site> barracks = null)
        {
            var s = "TRAIN";
            if (barracks != null)
            {
                foreach (var b in barracks)
                {
                    s = s + " " + b.siteId;
                }
            }
            Console.WriteLine(s);
        }

        private void SetLeastMines(Unit queen)
        {
            if (leastMines == -1)
            {
                if (queen.health < 30)
                {
                    leastMines = 1;
                    Console.Error.WriteLine("leastMines " + leastMines);
                    return;
                }

                if (queen.health < 60)
                {
                    leastMines = 2;
                    Console.Error.WriteLine("leastMines " + leastMines);
                    return;
                }

                leastMines = 3;
                Console.Error.WriteLine("leastMines " + leastMines);
            }
        }

        private bool InEnemyTowerRange(State state, Site site)
        {
            return state.Sites.Where(x => x.owner == 1 && x.structureType == 1)
                .Any(y => DistansTo(y.x, y.y, site.x, site.y) < y.param2);
        }


        private bool DoTrain(int gold, int touchedSite, List<Site> sites, List<Unit> units)
        {
            var knights = sites.Where(x => x.owner == 0 && x.structureType == 2 && x.param2 == 0).ToList();
            var defenders = sites.Where(x => x.owner == 0 && x.structureType == 2 && x.param2 == 1).ToList();
            var towers = sites.Where(x => x.owner == 0 && x.structureType == 1).Select(x => x).ToArray();
            var archers = units.Count(x => x.owner == 0 && x.type == 1);
            var enemyKnights = units.Count(x => x.owner == 1 && x.type == 0);
            var enemybuildingknights =
                sites.Any(x => x.owner == 1 && x.structureType == 2 && x.param2 == 0 && x.param1 > 0);
            var enemyTowers = sites.Count(x => x.owner == 1 && x.structureType == 1);
            var giants = sites.Where(x => x.owner == 0 && x.structureType == 2 && x.param2 == 2).ToList();
            if (gold > 400 && knights.Count > 1 && giants.Count > 0)
            {
                Console.WriteLine($"TRAIN {knights[0].siteId} {knights[1].siteId} {giants[0].siteId}");
                return true;
            }

            if (giants.Count > 0 && giants[0].param1 == 0)
            {
                Console.WriteLine($"TRAIN {giants[0].siteId}");
                return true;
            }

            if (gold > 300 && knights.Count > 1)
            {
                Console.WriteLine($"TRAIN {knights[0].siteId} {knights[1].siteId}");
                return true;
            }

            if (gold > 100 && gold < 350 && enemyTowers < 6 && knights.Count > 0)
            {
                Console.WriteLine($"TRAIN {knights[0].siteId}");
                return true;
            }




            //if (towers.Length < 4 && gold > 100 && archers < 2 && enemyKnights > 1 || enemybuildingknights)
            //{
            //    Console.WriteLine($"TRAIN {defenders[0].siteId}");
            //    return true;
            //}

            Console.WriteLine($"TRAIN");
            return true;
        }

        public bool DoBuild(int gold, int tuchedSite, List<Site> sites, List<Unit> units)
        {
            var mySites = sites.Where(x => x.distXstart < 1050).ToArray();
            var barracks = sites.Where(x => x.owner == 0 && x.structureType == 2).ToList();
            var towers = sites.Where(x => x.owner == 0 && x.structureType == 1).Select(x => x).ToArray();
            var closestSite = mySites.Where(x => x.structureType == -1 || x.structureType != 1 && x.owner == 1)
                .OrderBy(x => x.dist).FirstOrDefault();

            if (barracks.Count == 0 && latestSite != null && latestSite.distXstart >= closestSite.distXstart)
            {
                closestSite = mySites.Where(x => x.structureType == -1 && x.distXstart > latestSite.distXstart)
                    .OrderBy(x => x.dist).FirstOrDefault();

            }

            var mines = sites.Where(x => x.owner == 0 && x.structureType == 0).Select(x => x).ToArray();
            var closestMine = mines.OrderBy(x => x.dist).FirstOrDefault();
            var closestGold = mySites
                .Where(x => (x.structureType == -1 || x.structureType != 1 && x.owner == 1) && x.gold != 0)
                .OrderBy(x => x.dist).FirstOrDefault();
            if (barracks.Count == 0 && latestSite != null && latestSite.distXstart >= closestGold.distXstart)
            {
                closestGold = mySites
                    .Where(x => x.structureType == -1 && x.distXstart > closestGold.distXstart && x.gold != 0)
                    .OrderBy(x => x.dist).FirstOrDefault();

            }


            if (closestSite == null)
            {
                closestSite = mines.OrderBy(x => x.gold).First();
            }

            var enemyKnights = units.Count(x => x.owner == 1 && x.type == 0);


            if (!barracks.Any() && mines.Count() >= leastMines)
            {
                Console.WriteLine($"BUILD {closestSite.siteId} BARRACKS-KNIGHT");
                return true;
            }

            if (gold < 300 && enemyKnights < 2 || mines.Count() < leastMines)
            {
                BuildMine(closestMine, closestGold, closestSite);
                return true;
            }

            if (gold >= 300)
            {
                BuildBarrack(barracks, closestSite);
                return true;
            }

            if (closestSite.structureType == 0)
            {
                Console.WriteLine($"WAIT");
                return true;
            }

            BuildTowers(towers, closestSite, mySites);
            return true;
        }

        private void SmallDefend(State state, int numTowers = 3)
        {
            Console.Error.WriteLine("smalldefend");
            var towers = state.Sites.Where(x => x.owner == 0 && x.structureType == 1).ToList();
            if (state.TouchedSite!=-1&& state.Sites[state.TouchedSite].param1<700&& state.Sites[state.TouchedSite].owner == 0 && state.Sites[state.TouchedSite].structureType == 1 && !state.Units.Any(x =>
                    x.owner == 1 && x.type == 0 && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 200))
            {
                Build(state, state.Sites[state.TouchedSite], "TOWER");
                return;
            }
            if (towers.Count() >= numTowers)
            {
                

                Build(state, towers.OrderBy(x => x.param1).First(), "TOWER");
                return;

            }
            var nonBuilt = state.Sites.Where(x => (x.owner == -1 && x.structureType == -1)).OrderBy(x => x.distXstart).ToList();
            if (!nonBuilt.Any())
            {
                nonBuilt = state.Sites.Where(x => x.owner == 0 && x.structureType == 0).OrderBy(x => x.distXstart)
                    .ToList();
            }

            if (!nonBuilt.Any())
            {
                nonBuilt = state.Sites.Where(x => x.owner != 1 && x.structureType != 1).OrderBy(x => x.dist)
                    .ToList();
            }
            if (!towers.Any())
            {
                Build(state, nonBuilt.First(), "TOWER");
                return;
            }
            if (towers.Count() < numTowers)
            {
                var tower = towers.First();
                Build(state, nonBuilt.OrderBy(x => DistansTo(x.x, x.y, tower.x, tower.y)).First(), "TOWER");
                return;
            }

        }

        public void Build(State state, Site site, string building)
        {
            if (state.TouchedSite == site.siteId)
            {
                Console.WriteLine($"BUILD {site.siteId} {building}");
                return;
            }

            if (state.Queen.health < 90)
            {

            }
            Console.WriteLine($"BUILD {site.siteId} {building}");
        }

        private int DistansTo(int x, int y, int x2, int y2)
        {
            return (int)Math.Sqrt(Math.Pow(Math.Abs(x - x2), 2) + Math.Pow(Math.Abs(y - y2), 2));
        }

        private void BuildTowers(Site[] towers, Site closestSite, Site[] sites)
        {
            var closestTower = towers.OrderBy(x => x.dist).FirstOrDefault();
            if (towers.Count() == maxTowers)
            {

                var backtower = towers.OrderBy(x => x.param1).FirstOrDefault();
                Console.WriteLine($"BUILD {backtower.siteId} TOWER");
                return;
            }

            if (closestTower != null && closestTower.param1 < 400)
            {
                Console.WriteLine($"BUILD {closestTower.siteId} TOWER");
                return;
            }

            var backSite = sites.Where(x => x.structureType == -1).OrderBy(x => x.distXstart).FirstOrDefault();
            Console.WriteLine($"BUILD {backSite.siteId} TOWER");
        }

        private static void BuildBarrack(List<Site> barracks, Site closestSite)
        {
            if (barracks.Count(x => x.param2 == 2) < 1)
            {
                Console.WriteLine($"BUILD {closestSite.siteId} BARRACKS-GIANT");
                return;
            }

            if (barracks.Count(x => x.param2 == 0) < 2)
            {
                Console.WriteLine($"BUILD {closestSite.siteId} BARRACKS-KNIGHT");
                return;
            }

            Console.WriteLine($"BUILD {closestSite.siteId} TOWER");
        }


        private static void BuildMine(Site closestMine, Site closestGold, Site closestSite)
        {


            if (closestMine != null && closestMine.maxMineSize > closestMine.param1)
            {
                Console.WriteLine($"BUILD {closestMine.siteId} MINE");
                return;
            }

            if (closestGold != null)
            {
                Console.WriteLine($"BUILD {closestGold.siteId} MINE");
                return;
            }

            Console.WriteLine($"BUILD {closestSite.siteId} TOWER");
        }
    }


    //private int FindLineCircleIntersections(
    //float cx, float cy, float radius,
    //PointF point1, PointF point2,
    //out PointF intersection1, out PointF intersection2)
    //{
    //    float dx, dy, A, B, C, det, t;

    //    dx = point2.X - point1.X;
    //    dy = point2.Y - point1.Y;

    //    A = dx * dx + dy * dy;
    //    B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
    //    C = (point1.X - cx) * (point1.X - cx) +
    //        (point1.Y - cy) * (point1.Y - cy) -
    //        radius * radius;

    //    det = B * B - 4 * A * C;
    //    if ((A <= 0.0000001) || (det < 0))
    //    {
    //        // No real solutions.
    //        intersection1 = new PointF(float.NaN, float.NaN);
    //        intersection2 = new PointF(float.NaN, float.NaN);
    //        return 0;
    //    }
    //    else if (det == 0)
    //    {
    //        // One solution.
    //        t = -B / (2 * A);
    //        intersection1 =
    //            new PointF(point1.X + t * dx, point1.Y + t * dy);
    //        intersection2 = new PointF(float.NaN, float.NaN);
    //        return 1;
    //    }
    //    else
    //    {
    //        // Two solutions.
    //        t = (float)((-B + Math.Sqrt(det)) / (2 * A));
    //        intersection1 =
    //            new PointF(point1.X + t * dx, point1.Y + t * dy);
    //        t = (float)((-B - Math.Sqrt(det)) / (2 * A));
    //        intersection2 =
    //            new PointF(point1.X + t * dx, point1.Y + t * dy);
    //        return 2;
    //    }
    //}

}
public class Site
{
    public Site()
    {
        gold = -1;
    }
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
    public int distXstart { get; set; }
}

public class Unit
{

    public int y { get; set; }
    public int x { get; set; }
    public int owner { get; set; }
    public int health { get; set; }
    public int type { get; set; }
}


public class State
{

    public List<Site> Sites { get; set; }
    public List<Unit> Units { get; set; }
    public int Gold { get; set; }
    public Unit Queen { get; set; }

    public int TouchedSite { get; set; }
    public Unit EnemyQueen { get; set; }
    public int Turn { get; set; }
}