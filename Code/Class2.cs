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
        public bool buildFirstBarrack = false;
        public int turnsSinceLastTrain = 0;
        public int Turns = 0;
        public int CloseEnemyBarrackId = -1;

        public List<Site> Towers =new List<Site>();
        public List<Site> Barracks =new List<Site>();
        public List<Site> MyTurf =new List<Site>();
        public List<Site> Mines = new List<Site>();
     
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

                //StructuredPlay(state);
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
            Turns++;
            state.Turn= Turns;
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
            if (state.Sites.Any(x => x.owner == 0 && x.structureType == 2 && x.param1 > 0))
            {
                turnsSinceLastTrain = 0;
            }
            if (aimBarrack == -1)
            {
                var skip = 4; //state.Sites.Count(x => Math.Abs(x.y - startY) < 400) - 6;

                if (queenStartHp < 36)
                {
                    skip = 0;// state.Sites.Count(x => Math.Abs(x.y - startY) < 400) - 10;
                }
                else if (queenStartHp < 61)
                {
                    skip = 1;// state.Sites.Count(x => Math.Abs(x.y - startY) < 400) - 8;
                }
                else if (queenStartHp < 80)
                {
                    skip = 2;// state.Sites.Count(x => Math.Abs(x.y - startY) < 400) - 8;
                }
                if (skip > state.Sites.Count(x => Math.Abs(x.y - startY) < 400))
                {
                    skip = 1;
                }

                skip = 1;
                aimBarrack = state.Sites.Where(x => Math.Abs(x.y - startY) < 400).OrderBy(x => x.distXstart).Skip(skip).First().siteId;
                aimBarrack = state.Sites.Where(x => Math.Abs(x.x - startX) > 400 && Math.Abs(x.y - startY) > 200).OrderBy(x => DistansTo(x.x,x.y,state.Queen.x,state.Queen.y,1,2)).First().siteId;

                Console.Error.WriteLine("aimbarrack: " + aimBarrack + " skip: " + skip);
            }




            var barracks = state.Sites.Where(x => x.owner == 0 && x.structureType == 2).OrderByDescending(x=>x.distXstart);
            var mines = state.Sites.Where(x => x.owner == 0 && x.structureType == 0);
            var income = state.Sites.Where(x => x.owner == 0 && x.structureType == 0).Sum(y => y.param1);
            var towers = state.Sites.Where(x => x.owner == 0 && x.structureType == 1).ToList();



            if (state.Queen.health < state.EnemyQueen.health && state.Turn > 150 && state.Sites.Count(x => x.owner == 1 && x.structureType == 1) > 2)
            {
                if (state.Queen.health < 15 && state.Units.Any(x =>
                        x.owner == 1 && x.type == 0 && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 1100))
                {
                    Console.Error.WriteLine("TowerBreak! but run awaaay");
                    RunAway(state);
                    Train();
                    return;
                }
                if (income > 1)
                {
                    Console.Error.WriteLine("TowerBreak!");

                    TowerBreaker(state);
                    return;
                }
                Build(state, state.Sites.OrderBy(x => x.distXstart).First(x => x.gold > 0), "MINE");
                Train();
                return;
            }

            if (state.Queen.health < 15 && state.Units.Any(x =>
                    x.owner == 1 && x.type == 0 && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 900))
            {
                RunAway(state);
                TrainBest(state, barracks);
                return;
            }

            if (state.Queen.health < 10 && state.Sites.Any(x =>
                    x.owner == 1 && x.structureType == 2 && x.param2 == 0) && state.Sites.Any(x =>
                    x.owner == 1 && x.structureType == 0) && towers.Count() < 6 && towers.Any(x => x.param1 < 550) && (state.EnemyQueen.health < state.Queen.health || mines.Count() > 2))
            {
                SmallDefend(state, 5);
                TrainBest(state, barracks);
                return;
            }

            if (state.EnemyQueen.health < state.Queen.health &&state.Turn>170&& state.Sites.Any(x => x.owner == 1 && x.structureType == 2))
            {
                if (state.Units.Any(x => x.owner == 1 && x.type == 0))
                {
                    SmallDefend(state);
                    TrainBest(state, barracks);
                    return;
                }

                SmallDefend(state, 6);
                TrainBest(state, barracks);
                return;

            }
            if (!barracks.Any())
            {
                if (state.TouchedSite != -1 && state.TouchedSite != aimBarrack && ((state.Sites[state.TouchedSite].structureType == -1 && state.Sites[state.TouchedSite].owner == -1)
                   || (state.Sites[state.TouchedSite].structureType == 0 && state.Sites[state.TouchedSite].owner == 0 && state.Sites[state.TouchedSite].maxMineSize > state.Sites[state.TouchedSite].param1)
                    || (state.Sites[state.TouchedSite].structureType != 1 && state.Sites[state.TouchedSite].owner == 1))
                    && state.Units.Count(x => x.owner == 1 && x.type == 0) == 0)
                {
                    Console.Error.WriteLine("Rush: Building mine when touching");
                    Build(state, state.Sites[state.TouchedSite], "MINE");
                    Train();
                    return;
                }

                var closesite = state.Sites.FirstOrDefault(x => x.dist - x.r < 100 && x.structureType == -1 && x.siteId !=aimBarrack);
                if (closesite != null)
                {
                    Console.Error.WriteLine("Rush: Building mine when close");
                    Build(state, closesite, "MINE");
                    Train();
                    return;
                }

                if (buildFirstBarrack)
                {
                    Console.Error.WriteLine("Rush: Building closest knights");
                    aimBarrack = state.Sites.Where(x => x.structureType != 1).OrderBy(x => x.dist).First().siteId;
                    Build(state, state.Sites[aimBarrack], "BARRACKS-KNIGHT");
                    Train();
                    return;
                    
                }
                
                if (state.Units.Count(x => x.owner == 1 && x.type == 0) > 0)
                {
                    Console.Error.WriteLine("Rush: Building closest knights because knights incoming");
                    aimBarrack = state.Sites.Where(x => x.structureType == -1 &&x.distXstart>Math.Abs(state.Queen.x-startX)).OrderBy(x => x.dist).First().siteId;
                    Build(state, state.Sites[aimBarrack], "BARRACKS-KNIGHT");
                    Train();
                    return;
                }

                Console.Error.WriteLine("Rush: Building first knights");

                Build(state, state.Sites[aimBarrack], "BARRACKS-KNIGHT");
                Train();
                return;
            }
            if (CloseEnemyBarrackId != -1 && CloseEnemyBarrackId == state.TouchedSite)
            {
                if (!(state.Sites[CloseEnemyBarrackId].owner == 1 && state.Sites[CloseEnemyBarrackId].structureType == 1))
                {
                    Console.Error.WriteLine("Rush: Building Tower for close enemies");
                    Build(state, state.Sites[CloseEnemyBarrackId], "TOWER");
                    TrainBest(state, barracks);
                    CloseEnemyBarrackId = -1;
                    return;
                }
                CloseEnemyBarrackId = -1;
            }
            var closeEnemyBarrack = state.Sites.FirstOrDefault(x =>
                x.owner == 1 && x.structureType == 2 && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 500 && !InEnemyTowerRange(state, x, 200));
            if (closeEnemyBarrack != null && state.Units.Count(x => x.owner == 1 && x.type == 0) < 5 && state.Queen.health > 30)
            {
                CloseEnemyBarrackId = closeEnemyBarrack.siteId;
                Console.Error.WriteLine("Rush: Building Tower for close enemies");
                Build(state, closeEnemyBarrack, "TOWER");
                TrainBest(state, barracks);
                return;
            }
            if ((state.Sites.Any(x => x.owner == 1 && x.structureType == 2 && x.param2 == 0)||state.Queen.health<46) && state.Turn < 50 &&
                mines.Count() > 1)
            {
                SmallDefend(state);
                TrainBest(state, barracks);
                return;
            }
                buildFirstBarrack = true;
            var goodTowers = state.Sites.Where(x => x.structureType != 1 && x.siteId != aimBarrack && Math.Abs(x.x - startX) > Math.Abs(state.Queen.x - startX) && (x.owner != 0) && !InEnemyTowerRange(state, x) && x.distXstart < 1500).OrderBy(x => x.dist).ToList();

           
            Console.Error.WriteLine("enemy knights: " + state.Units.Count(x => x.owner == 1 && x.type == 0));

            if (state.Units.Count(x => x.owner == 1 && x.type == 0) > 2 && state.Queen.health < 80 || state.EnemyQueen.health < state.Queen.health&& state.Turn>170)
            {  
                    SmallDefend(state);
                TrainBest(state, barracks);
                return;
            }
            if (goodTowers.Count() < 3 || movedToEnd)
            {
                Console.Error.WriteLine("MovedToEnd");
                movedToEnd = true;
                goodTowers = state.Sites.Where(x => x.structureType != 1 && !(x.structureType == 0 && x.owner == 0 && x.maxMineSize == x.param1) && !(x.structureType == 2 && x.owner == 0) && !InEnemyTowerRange(state, x)&&x.distXstart<1500).OrderBy(x => x.dist).ToList();
                if (!goodTowers.Any())
                {
                    goodTowers = state.Sites.Where(x => x.structureType == 1 && x.owner == 0 && !InEnemyTowerRange(state, x)).OrderBy(x => x.dist).ToList();

                }
            }
            foreach (var t in goodTowers)
            {
                Console.Error.WriteLine("goodtower: " + t.siteId);

            }

            
            if (barracks.Count() > 1)
            {
               goodTowers.Add(barracks.OrderBy(x => x.distXstart).First());
                goodTowers = goodTowers.OrderBy(x => x.dist).ToList();

            }
           
            if ((mines.Count() < 6||!state.Sites.Any(x=>x.owner==1&&x.structureType==2)) && !state.Units.Any(x => x.owner == 1 && x.type == 0&&DistansTo(x.x,x.y,state.Queen.x,state.Queen.y)<300))
            {
                var goodMines = goodTowers.Where(x => x.gold != 0);
                foreach (var t in goodMines)
                {
                    Console.Error.WriteLine("goodMines: " + t.siteId);

                }

                if (barracks.Count() == 1)
                {
                    if (state.TouchedSite != -1 && !(state.Sites[state.TouchedSite].owner == 1 && state.Sites[state.TouchedSite].structureType == 1) && state.Sites[state.TouchedSite].distXstart > 900&& !barracks.Any(x=>x.distXstart>900)
                        && !(state.Sites[state.TouchedSite].owner == 0 && state.Sites[state.TouchedSite].structureType == 1 && towers.Count() < 4))
                    {
                        Console.Error.WriteLine("Rush: Building frontline knights barracks");
                        Build(state, state.Sites[state.TouchedSite], "BARRACKS-KNIGHT");
                        TrainBest(state, barracks);
                        return;
                    }

                }

                if (state.TouchedSite != -1 && (state.Sites[state.TouchedSite].structureType == 0 && state.Sites[state.TouchedSite].owner == 0 &&
                     state.Sites[state.TouchedSite].maxMineSize > state.Sites[state.TouchedSite].param1 && state.Sites[state.TouchedSite].gold > 0))
                {
                    Console.Error.WriteLine("Rush: Building mine when touching normal way");
                    Build(state, state.Sites[state.TouchedSite], "MINE");
                    TrainBest(state, barracks);
                    return;
                }

                if (goodMines.Any())
                {
                    foreach (var t in goodMines)
                    {
                        Console.Error.WriteLine("goodMines: " + t.siteId);

                    }
                    Console.Error.WriteLine("Rush: Building mine from list");
                    Build(state, goodMines.First(), "MINE");
                    TrainBest(state, barracks);
                    return;
                }
            }

            if (state.TouchedSite != -1 && state.Sites[state.TouchedSite].param1 < 700 &&
                state.Sites[state.TouchedSite].owner == 0 && state.Sites[state.TouchedSite].structureType == 1)
            {
                Console.Error.WriteLine("Rush: Building Tower when thouching in end");
                Build(state, state.Sites[state.TouchedSite], "TOWER");
                TrainBest(state, barracks);
                return; 
            }
            if (state.Units.Any(x => x.owner == 1 && x.type == 0 && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 300))
            {
                SmallDefend(state);
                TrainBest(state, barracks);
                return;
            }


            if (!goodTowers.Any())
            {
                Console.Error.WriteLine("Rush: No valid towers moving home");
                Move(startX, startY);
                TrainBest(state, barracks);
                return;
            }

            Console.Error.WriteLine("Rush: Building Tower fallback");
            Build(state, goodTowers.First(), "TOWER");
            TrainBest(state, barracks);
            return; 
        }

        private void RunAway(State state)
        {
            if (state.TouchedSite != -1 && (state.Sites[state.TouchedSite].structureType == -1 ||
                state.Sites[state.TouchedSite].structureType == 0))
            {
                Build(state, state.Sites[state.TouchedSite],"TOWER");
                return;
            }

            Move(startX, startY == 0 ? 1000 : 0);
            return;
  
        }

        private void Move(int x, int y)
        {
            Console.WriteLine($"MOVE {x} {y}");
        }

        private void Train(IEnumerable<Site> barracks)
        {

            var s = "TRAIN";
            turnsSinceLastTrain++;
            if (barracks != null)
            {
                
                foreach (var b in barracks.OrderByDescending(x=>x.distXstart))
                {
                    s = s + " " + b.siteId;
                }
            }
            Console.WriteLine(s);
        }
        private void Train(Site barracks = null)
        {
            var s = "TRAIN";
            turnsSinceLastTrain++;
            if (barracks != null)
            {
                s =s +" "+ barracks.siteId;
            }
            Console.WriteLine(s);
        }
        private void TrainBest(State state,IEnumerable<Site> barracks = null)
        {
            
            var income = state.Sites.Where(x => x.owner == 0 && x.structureType == 0).Sum(y => y.param1);
           
            Console.Error.WriteLine("turnsSinceLastTrain: " + turnsSinceLastTrain);
            if (income > 4 && state.Gold + income * 5 < 160 && turnsSinceLastTrain > 2)
            {
                Train();
                return;
            }
            if (barracks != null && barracks.Any())
            {
                Train(barracks.OrderBy(x => DistansTo(x.x, x.y, state.EnemyQueen.x, state.EnemyQueen.y)).First());
            }
            else
            {
                Train();
            }

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

        private bool InEnemyTowerRange(State state, Site site, int allowance=0)
        {
            return state.Sites.Where(x => x.owner == 1 && x.structureType == 1)
                .Any(y => DistansTo(y.x, y.y, site.x, site.y) +site.r< y.param2-allowance||(y.dist<site.dist&&y.distXstart<site.distXstart));
        }

        private void SmallDefend(State state, int numTowers = 3)
        {
            Console.Error.WriteLine("smalldefend");
            var towers = state.Sites.Where(x => x.owner == 0 && x.structureType == 1).ToList();
            if (state.TouchedSite!=-1&& state.Sites[state.TouchedSite].param1<450&& state.Sites[state.TouchedSite].owner == 0 && state.Sites[state.TouchedSite].structureType == 1 
                )//&& !state.Units.Any(x =>x.owner == 1 && x.type == 0 && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 200))
            {
                Build(state, state.Sites[state.TouchedSite], "TOWER");
                return;
            }
            if (towers.Count() >= numTowers)
            {
                

                Build(state, towers.OrderBy(x => x.param1).First(), "TOWER");
                return;

            }
            var nonBuilt = state.Sites.Where(x => (x.owner == -1 && x.structureType == -1&&x.distXstart<1300)&&!InEnemyTowerRange(state,x)).OrderBy(x => x.dist).ToList();
            if (!nonBuilt.Any())
            {
                nonBuilt = state.Sites.Where(x => x.owner == 0 && x.structureType == 0).OrderBy(x => x.dist)
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
            if (towers.Count() < numTowers && towers.Any(x=>x.distXstart-100<Math.Abs(state.Queen.x-startX)))
            {
                var tower = towers.First();
                Build(state, nonBuilt.OrderBy(x => x.dist).First(), "TOWER");
                return;
            }

            Build(state, nonBuilt.First(), "TOWER");
            return;

        }

        public void Build(State state, Site site, string building)
        {
            if (DistansTo(site.x, site.y, state.EnemyQueen.x, state.EnemyQueen.y) < 150)
            {
                Console.WriteLine($"BUILD {site.siteId} TOWER");
                return;
            }

            if (state.Units.Any(x =>x.owner==1&&x.type==0&& DistansTo(site.x, site.y, x.x, x.y) < 400) && building == "MINE")
            {
                Console.WriteLine($"BUILD {site.siteId} TOWER");
                return; 
            }

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

        private int DistansTo(int x, int y, int x2, int y2, double weightX=1,double weightY=1)
        {
            return (int)Math.Sqrt(Math.Pow(weightX*Math.Abs(x - x2), 2) + Math.Pow(weightY*Math.Abs(y - y2), 2));
        }

        public void StructuredPlay(State state)
        {
            if (!MyTurf.Any())
            {
                SetMyTurf(state, 1000);
            }

            if (MyTurf.Any(x => x.owner == 1 && x.structureType == 1))
            {
                SetMyTurf(state, 1200);
            }


            if (state.Units.Any(x =>
                x.owner == 1 && x.type == 0 && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 300))
            {

                if (Mines.Count(x => x.structureType == 1) > 2)
                {
                    Console.Error.WriteLine("upgrade new tower because of close knights");
                    Build(state, MyTurf.Where(x => x.structureType == 1).OrderBy(x => x.dist).First(), "TOWER");
                    TrainStructured(state);
                    return;
                }
                Console.Error.WriteLine("build new tower because of close knights");
                Build(state, Mines.Where(x => x.structureType != 1).OrderBy(x => x.dist).First(), "TOWER");
                TrainStructured(state);
                return;
            }

            if (state.TouchedSite != -1)
            {
                
                if (state.TouchedSite != -1 && (state.Sites[state.TouchedSite].structureType == -1||(state.Sites[state.TouchedSite].maxMineSize > state.Sites[state.TouchedSite].param1 && state.Sites[state.TouchedSite].gold > 0)) && Towers.All(x => x.siteId != state.TouchedSite)&& Barracks.All(x => x.siteId != state.TouchedSite))
                {
                    Console.Error.WriteLine("build close mine");
                    Build(state, state.Sites[state.TouchedSite], "MINE");
                    TrainStructured(state);
                    return;
                }
                if (state.TouchedSite != -1 && state.Sites[state.TouchedSite].structureType == 1 && state.Sites[state.TouchedSite].param1 < 700)
                {
                    Console.Error.WriteLine("build close tower");
                    Build(state, state.Sites[state.TouchedSite], "TOWER");
                    TrainStructured(state);
                    return;
                }
            }
            if (Barracks.All(x => x.structureType == -1))
            {
                Console.Error.WriteLine("build first barrack");

                Build(state, Barracks.OrderBy(x => x.dist).First(), "BARRACKS-KNIGHT");
                TrainStructured(state);
                return;

            }
            if (Mines.Count(x => x.owner == 0 && x.structureType == 0) < 3 && !state.Units.Any(x =>
                    x.owner == 1 && x.type == 0 && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 300))
            {
                Console.Error.WriteLine("building mines");
                Build(state, Mines.Where(x => x.structureType == -1).OrderBy(x => x.dist).First(), "MINE");
                TrainStructured(state);
                return;
            }

            

            if (Towers.Any(x => x.structureType != 1)&&state.Sites.Any(x=>x.owner==1&&x.structureType==2&&x.param2==0))
            {
                Console.Error.WriteLine("building tower");
                Build(state, Towers.Where(x=>x.structureType!=1).OrderBy(x=>x.dist).First(), "TOWER");
                TrainStructured(state);
                return;
            }

            if (state.Gold > 150)
            {
                if (Barracks.Any(x => x.structureType != 2)){
                    //if (Barracks.Any(x => x.structureType != 2&&x.param2!= 2))
                    //{
                    //    Console.Error.WriteLine("building giants");
                    //    Build(state,Barracks.OrderBy(x=>x.dist).First(),"BARRACKS-GIANT");
                    //    TrainStructured(state);
                    //    return;
                    //}
                    Console.Error.WriteLine("building more knights");
                    Build(state, Barracks.OrderBy(x => x.dist).First(), "BARRACKS-KNIGHT");
                    TrainStructured(state);
                    return;
                }
            }

            if (MyTurf.Any(x => x.structureType == -1))
            {
                if (MyTurf.Any(x => x.structureType == -1 && x.gold > 0))
                {
                    Console.Error.WriteLine("building turf mines");
                    Build(state, MyTurf.Where(x => x.structureType == -1 && x.gold > 0).OrderBy(x => x.dist).First(), "MINE");
                    TrainStructured(state);
                    return;
                }
                Console.Error.WriteLine("building mine mines");
                Build(state, Mines.Where(x => x.structureType != 0).OrderBy(x => x.dist).First(), "MINE");
                TrainStructured(state);
                return;
            }
           

        }

        private void SetMyTurf(State state, int distx)
        {
            Console.Error.WriteLine("Resetting my turf");
            MyTurf = state.Sites.Where(x => x.distXstart < distx&&!(x.owner==1&&x.structureType==1)).ToList();
            if (!Towers.Any())
            {
                Towers = MyTurf.OrderBy(x => DistansTo(x.x, x.y, 700, 500)).Take(3).ToList();

            }
            else if (Towers.Any(x => x.owner == 1 && x.structureType == 1))
            {
                Towers = MyTurf.OrderBy(x => DistansTo(x.x, x.y, 700, 500)).Take(Towers.Count(x => !(x.owner == 1 && x.structureType == 1))).ToList();

            }

            if (!Barracks.Any())
            {
                Barracks = MyTurf.Where(x => Towers.All(y => x.siteId != y.siteId)).OrderByDescending(x => x.distXstart).Take(2).ToList();
                Barracks.Add(MyTurf.Where(x => Math.Abs(startY - x.y) < 300).OrderBy(x => x.distXstart).Skip(1).First());

            }
            else if (Barracks.Any(x => x.owner == 1 && x.structureType == 1))
            {
                var tempB = Barracks.Where(x => !(x.owner == 1 && x.structureType == 1)).ToList();
                var tempB2 = MyTurf.OrderByDescending(x => x.distXstart).Take(Barracks.Count(x => !(x.owner == 1 && x.structureType == 1))).ToList();
                Barracks.Clear();
                Barracks.AddRange(tempB2);
                Barracks.AddRange(tempB);
            }

            Mines = MyTurf.Where(x => Towers.All(y => x.siteId != y.siteId) && Barracks.All(y => x.siteId != y.siteId)).ToList();
            foreach (var site in MyTurf)
            {
                Console.Error.WriteLine("myturf: " + site.siteId);
            }
            foreach (var tower in Towers)
            {
                Console.Error.WriteLine("turftower: " + tower.siteId);
            }
            foreach (var site in Barracks)
            {
                Console.Error.WriteLine("turfbarracks: " + site.siteId);
            }
            foreach (var site in Mines)
            {
                Console.Error.WriteLine("turfMines: " + site.siteId);
            }
        }

        private void TrainStructured(State state)
        {
            
            var start = Barracks.FirstOrDefault(x => x.structureType == 2 && x.param2 == 0);
            if (start != null && state.Sites.Count(x=>x.structureType==1&&x.owner==1)<3&&state.Units.Count(x=>x.owner==0&&x.type==0)<2){
            
                Console.Error.WriteLine("Train start");
                Train(new List<Site>() { start });
                return;
                }

            if (state.Gold < 130 && state.Gold > 80 &&MyTurf.Count(x=>x.structureType==0)>2&&!state.Units.Any(x=>x.owner==0&&x.type==0))
            {
                Train();
                return;
            }
            if (state.Gold > 200)
            {
                

                //if (!state.Units.Any(x => x.owner == 0 && x.type == 2)&&Barracks.Any(x=>x.param2==2))
                //{
                //    Console.Error.WriteLine("Train giants");

                //    var giant = Barracks.First(x => x.param2 == 2);
                //    Train(new List<Site>(){giant});
                //    return;
                //}

                if (Barracks.Count > 1)
                {
                    Console.Error.WriteLine("Train knights");
                    var knights = Barracks.Where(x => x.param2 == 0);
                    Train(knights);
                    return;
                }
            }

            Train(Barracks.Where(x=>x.structureType==2));
        }

        private void TowerBreaker(State state)
        {
            if (state.Gold < 250&&!state.Units.Any(x=>x.owner==0&&x.type==2))
            {
                SmallDefend(state);
                Train();
                return;
            }

            if (state.Units.Any(x => x.owner == 0 && x.type == 2))
            {
                var barracks = state.Sites.Where(x => x.owner == 0 && x.structureType == 2 && x.param2 != 2);
                SmallDefend(state);
                TrainBest(state, barracks);
                return;
            }
            if (state.Sites.Any(x => x.owner == 0 && x.structureType == 2 && x.param2 == 2))
            {
                var giantBarrack = state.Sites.First(x => x.owner == 0 && x.structureType == 2 && x.param2 == 2);
                SmallDefend(state);
                Train(giantBarrack);
                return;

            }

            var giantBarrackToBuild= state.Sites.Where(x => x.owner != 1 && x.structureType == 1 &&x.distXstart>500).OrderBy(x=>x.dist).First();
            Build(state,giantBarrackToBuild,"BARRACKS-GIANT");
            Train();

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