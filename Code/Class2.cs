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
        public int CloseEnemyMineId = -1;

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
                if (state.TouchedSite != -1 && !sites[state.TouchedSite].IsUnbuilt)
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

            var queen = units.First(x => x.IsQueen && x.IsPlayers);
            state.Queen = queen;
            state.EnemyQueen = units.First(x => x.IsQueen && x.IsEnemys);
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
                site.distYstart= Math.Abs(startY - site.y);
                //Console.Error.WriteLine($"id:{site.siteId} dist:{site.dist} owner:{site.owner} type: {site.structureType} param1:{site.param1} maxMine:{site.maxMineSize}");
            }
            SetLeastMines(queen);
            state.Sites = sites.ToList();
            state.Units = units.ToList();
            return state;

        }

        private void Rush(State state)
        {
            if (state.Sites.Any(x => x.IsPlayers && x.structureType == 2 && x.param1 > 0))
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

                //skip = 1;
                aimBarrack = state.Sites.Where(x => Math.Abs(x.y - startY) < 400).OrderBy(x => x.distXstart).Skip(skip).First().siteId;
                aimBarrack = state.Sites.Where(x => Math.Abs(x.y - startY) < 400).OrderBy(x => x.distXstart).Skip(skip).Where(x => Math.Abs(x.x - startX) > 400 && Math.Abs(x.y - startY) > 200).OrderBy(x => DistansTo(x.x,x.y,state.Queen.x,state.Queen.y,1,2)).First().siteId;

                Console.Error.WriteLine("aimbarrack: " + aimBarrack + " skip: " + skip);
            }




            var barracks = state.Sites.Where(x => x.IsPlayers && x.IsBarrack).OrderByDescending(x=>x.distXstart);
            var mines = state.Sites.Where(x => x.IsPlayers && x.IsMine);
            var income = state.Sites.Where(x => x.IsPlayers && x.IsMine).Sum(y => y.MineIncome);
            var towers = state.Sites.Where(x => x.IsPlayers && x.IsTower).ToList();



            //if (state.Queen.health < state.EnemyQueen.health && state.Turn > 150 && state.Sites.Count(x => x.IsEnemys && x.IsTower) > 2)
            //{
            //    if (state.Queen.health < 15 && state.Units.Any(x =>
            //            x.IsEnemys && x.IsKnight && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 1100))
            //    {
            //        Console.Error.WriteLine("TowerBreak! but run awaaay");
            //        RunAway(state);
            //        Train();
            //        return;
            //    }
            //    if (income > 1)
            //    {
            //        Console.Error.WriteLine("TowerBreak!");

            //        TowerBreaker(state);
            //        return;
            //    }
            //    SmartBuild(state, state.Sites.OrderBy(x => x.distXstart).First(x => x.gold > 0), "MINE");
            //    Train();
            //    return;
            //}

            if (state.Queen.health < 15 && state.Units.Any(x =>
                    x.IsEnemys && x.IsKnight && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 400))
            {
                RunAway(state);
                TrainBest(state, barracks);
                return;
            }

            if (state.Queen.health < 10 && state.Sites.Any(x =>
                    x.IsEnemys && x.IsKnightBarrack) && state.Sites.Any(x =>
                    x.IsEnemys && x.IsMine) && towers.Count() < 6 && towers.Any(x => x.TowerHP < 550) && (state.EnemyQueen.health < state.Queen.health || mines.Count() > 2))
            {
                SmallDefend(state, 4);
                TrainBest(state, barracks);
                return;
            }

            if (state.EnemyQueen.health < state.Queen.health &&state.Turn>170&& state.Sites.Any(x => x.IsEnemys && x.IsBarrack))
            {
                if (state.Units.Any(x => x.IsEnemys && x.IsKnight))
                {
                    SmallDefend(state);
                    TrainBest(state, barracks);
                    return;
                }

                SmallDefend(state, 4);
                TrainBest(state, barracks);
                return;

            }
            if (!barracks.Any())
            {
                if (mines.Count() >= 2)
                {
                    var newaimBarrack = state.Sites
                        .Where(x => Math.Abs(startX - state.Queen.x) - 20 < x.distXstart && x.distYstart > 200)
                        .OrderBy(x => x.dist).First();
                    aimBarrack = newaimBarrack.siteId;
                    SmartBuild(state, newaimBarrack, "BARRACKS-KNIGHT");
                    Train();
                }

                if (state.TouchedSite != -1 && state.TouchedSite != aimBarrack && (state.Sites[state.TouchedSite].IsUnbuilt
                   || (state.Sites[state.TouchedSite].IsPlayers && state.Sites[state.TouchedSite].IsMineWithIncomeLeft)
                    || (!state.Sites[state.TouchedSite].IsTower && state.Sites[state.TouchedSite].IsEnemys))
                    && state.Units.Count(x => x.IsEnemys && x.IsKnight) == 0)
                {
                    Console.Error.WriteLine("Rush: Building mine when touching");
                    SmartBuild(state, state.Sites[state.TouchedSite], "MINE");
                    Train();
                    return;
                }

                var closesite = state.Sites.FirstOrDefault(x => x.dist - x.r < 100 && x.IsUnbuilt && x.siteId !=aimBarrack);
                if (closesite != null)
                {
                    Console.Error.WriteLine("Rush: Building mine when close");
                    SmartBuild(state, closesite, "MINE");
                    Train();
                    return;
                }

                if (buildFirstBarrack)
                {
                    Console.Error.WriteLine("Rush: Building closest knights");
                    aimBarrack = state.Sites.Where(x => !x.IsTower).OrderBy(x => x.dist).First().siteId;
                    SmartBuild(state, state.Sites[aimBarrack], "BARRACKS-KNIGHT");
                    Train();
                    return;
                    
                }
                
                if (state.Units.Count(x => x.IsEnemys && x.IsKnight) > 0)
                {
                    Console.Error.WriteLine("Rush: Building closest knights because knights incoming");
                    aimBarrack = state.Sites.Where(x => x.IsUnbuilt &&x.distXstart>Math.Abs(state.Queen.x-startX)).OrderBy(x => x.dist).First().siteId;
                    SmartBuild(state, state.Sites[aimBarrack], "BARRACKS-KNIGHT");
                    Train();
                    return;
                }

                Console.Error.WriteLine("Rush: Building first knights");

                SmartBuild(state, state.Sites[aimBarrack], "BARRACKS-KNIGHT");
                Train();
                return;
            }
            if (CloseEnemyBarrackId != -1 && CloseEnemyBarrackId == state.TouchedSite)
            {
                if (!(state.Sites[CloseEnemyBarrackId].IsEnemys && state.Sites[CloseEnemyBarrackId].IsTower))
                {
                    Console.Error.WriteLine("Rush: Building Tower for close enemies");
                    SmartBuild(state, state.Sites[CloseEnemyBarrackId], "TOWER");
                    TrainBest(state, barracks);
                    CloseEnemyBarrackId = -1;
                    return;
                }
                CloseEnemyBarrackId = -1;
            }
            var closeEnemyBarrack = state.Sites.FirstOrDefault(x =>
                x.IsEnemys && x.IsBarrack && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 500 && !InEnemyTowerRange(state, x, 200));
            if (closeEnemyBarrack != null && state.Units.Count(x => x.IsEnemys && x.IsKnight) < 5 && state.Queen.health > 30)
            {
                CloseEnemyBarrackId = closeEnemyBarrack.siteId;
                Console.Error.WriteLine("Rush: Building Tower for close enemies");
                SmartBuild(state, closeEnemyBarrack, "TOWER");
                TrainBest(state, barracks);
                return;
            }

            if (CloseEnemyMineId != -1 && CloseEnemyMineId == state.TouchedSite)
            {
                if (!(state.Sites[CloseEnemyMineId].IsEnemys && state.Sites[CloseEnemyMineId].IsTower))
                {
                    Console.Error.WriteLine("Rush: Building Tower for close enemies Mine");
                    SmartBuild(state, state.Sites[CloseEnemyMineId], "TOWER");
                    TrainBest(state, barracks);
                    CloseEnemyBarrackId = -1;
                    return;
                }
                CloseEnemyMineId = -1;
            }
            var closeEnemyMine = state.Sites.FirstOrDefault(x =>
                x.IsEnemys && x.IsMine && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 500 && !InEnemyTowerRange(state, x, 200));
            if (closeEnemyMine != null && state.Units.Count(x => x.IsEnemys && x.IsKnight) < 5 && state.Queen.health > 30)
            {
                CloseEnemyMineId = closeEnemyMine.siteId;
                Console.Error.WriteLine("Rush: Building Tower for close enemies Mine");
                SmartBuild(state, closeEnemyMine, "TOWER");
                TrainBest(state, barracks);
                return;
            }

            if ((state.Sites.Any(x => x.IsEnemys && x.IsKnightBarrack)||state.Queen.health<46) && state.Turn < 50 &&
                mines.Count() > 1)
            {
                if (!towers.Any())
                {
                    Console.Error.WriteLine("Rush: Building First Tower");
                    var firstTower = state.Sites.Where(x =>
                        x.IsUnbuilt && x.distYstart > state.Sites[aimBarrack].distYstart&&state.Sites.Count(y=>y.IsUnbuilt&&y.distYstart> state.Sites[aimBarrack].distYstart&&y.distXstart<x.distXstart)>2).OrderBy(x=>x.dist).First();
                    SmartBuild(state,firstTower,"TOWER");
                    TrainBest(state, barracks);
                    return;
                }
                
                Console.Error.WriteLine("Rush: Building First Defeence");
                SmallDefend(state);
                TrainBest(state, barracks);
                return;
            }
                buildFirstBarrack = true;
            var goodTowers = state.Sites.Where(x => !x.IsTower && x.siteId != aimBarrack && Math.Abs(x.x - startX) > Math.Abs(state.Queen.x - startX) && (x.owner != 0) && !InEnemyTowerRange(state, x) && x.distXstart < 1500).OrderBy(x => x.dist).ToList();

           
            Console.Error.WriteLine("enemy knights: " + state.Units.Count(x => x.IsEnemys && x.IsKnight));

            if (state.Units.Count(x => x.IsEnemys && x.IsKnight && DistansTo(x.x,x.y,state.Queen.x,state.Queen.y)<600) > 2 && state.Queen.health < 80 || state.EnemyQueen.health < state.Queen.health&& state.Turn>170)
            {  
                    SmallDefend(state);
                TrainBest(state, barracks);
                return;
            }
            if (goodTowers.Count() < 3 || movedToEnd)
            {
                Console.Error.WriteLine("MovedToEnd");
                movedToEnd = true;
                goodTowers = state.Sites.Where(x => !x.IsTower && !(x.IsMine && x.IsPlayers && x.maxMineSize == x.MineIncome) && !(x.IsBarrack && x.IsPlayers) && !InEnemyTowerRange(state, x)&&x.distXstart<1500).OrderBy(x => x.dist).ToList();
                if (!goodTowers.Any())
                {
                    goodTowers = state.Sites.Where(x => x.IsTower && x.IsPlayers && !InEnemyTowerRange(state, x)).OrderBy(x => x.dist).ToList();

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
           
            if ((mines.Count() < 6||!state.Sites.Any(x=>x.IsEnemys&&x.IsBarrack)) && !state.Units.Any(x => x.IsEnemys && x.IsKnight&&DistansTo(x.x,x.y,state.Queen.x,state.Queen.y)<300))
            {
                var goodMines = goodTowers.Where(x => x.gold != 0);
                foreach (var t in goodMines)
                {
                    Console.Error.WriteLine("goodMines: " + t.siteId);

                }

                if (barracks.Count() == 1)
                {
                    if (state.TouchedSite != -1 && !(state.Sites[state.TouchedSite].IsEnemys && state.Sites[state.TouchedSite].IsTower) && state.Sites[state.TouchedSite].distXstart > 900&& !barracks.Any(x=>x.distXstart>900)
                        && !(state.Sites[state.TouchedSite].IsPlayers && state.Sites[state.TouchedSite].IsTower && towers.Count() < 4))
                    {
                        Console.Error.WriteLine("Rush: Building frontline knights barracks");
                        SmartBuild(state, state.Sites[state.TouchedSite], "BARRACKS-KNIGHT");
                        TrainBest(state, barracks);
                        return;
                    }

                }

                if (state.TouchedSite != -1 && (state.Sites[state.TouchedSite].IsMineWithIncomeLeft && state.Sites[state.TouchedSite].IsPlayers &&
                      state.Sites[state.TouchedSite].gold > 0))
                {
                    Console.Error.WriteLine("Rush: Building mine when touching normal way");
                    SmartBuild(state, state.Sites[state.TouchedSite], "MINE");
                    TrainBest(state, barracks);
                    return;
                }

                if (towers.Count(x => x.param1 > 250) < 3)
                {
                    foreach (var site in towers)
                    {
                        Console.Error.WriteLine($"id:{site.siteId} dist:{site.dist} owner:{site.owner} type: {site.structureType} param1:{site.param1} maxMine:{site.maxMineSize}");

                    }
                    Console.Error.WriteLine("Rush: keeping towers up to date");
                    SmallDefend(state);
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
                    SmartBuild(state, goodMines.First(), "MINE");
                    TrainBest(state, barracks);
                    return;
                }
            }

            if (state.TouchedSite != -1 && state.Sites[state.TouchedSite].TowerHP < 700 &&
                state.Sites[state.TouchedSite].IsPlayers && state.Sites[state.TouchedSite].IsTower)
            {
                Console.Error.WriteLine("Rush: Building Tower when thouching in end");
                SmartBuild(state, state.Sites[state.TouchedSite], "TOWER");
                TrainBest(state, barracks);
                return; 
            }
            if (state.Units.Any(x => x.IsEnemys && x.IsKnight && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 300))
            {
                SmallDefend(state);
                TrainBest(state, barracks);
                return;
            }


            if (!goodTowers.Any())
            {
                Console.Error.WriteLine("Rush: No valid towers moving home");
                Move(state, startX, startY);
                TrainBest(state, barracks);
                return;
            }

            Console.Error.WriteLine("Rush: Building Tower fallback");
            SmartBuild(state, goodTowers.First(), "TOWER");
            TrainBest(state, barracks);
            return; 
        }

        private void RunAway(State state)
        {
            Console.Error.WriteLine("Runaway");
            if (state.TouchedSite != -1 && (state.Sites[state.TouchedSite].IsUnbuilt ||
                state.Sites[state.TouchedSite].IsMine))
            {
                SmartBuild(state, state.Sites[state.TouchedSite],"TOWER");
                return;
            }
            var towers = state.Sites.Where(x => x.IsPlayers && x.IsTower).ToList();
            if (towers.Count >= 3 && state.Units.Any(x => x.IsEnemys && x.IsKnight))
            {
                var enemyBarrack =
                    state.Sites.FirstOrDefault(x => x.IsEnemys && x.structureType == 2 && x.param2 == 0);
                if (enemyBarrack != null)
                {
                    var temptower = towers.OrderByDescending(x => DistansTo(x.x, x.y, enemyBarrack.x, enemyBarrack.y)).First();

                    var moveX = 0;
                    var moveY = 0;
                    if (temptower.x < enemyBarrack.x)
                    {

                        moveX = temptower.x - temptower.r - (temptower.param1 > 700 ? 50 : 0);
                    }
                    else
                    {
                        moveX = temptower.x + temptower.r + (temptower.param1 > 700 ? 50 : 0);
                    }
                    if (temptower.y < enemyBarrack.y)
                    {
                        moveY = temptower.y - temptower.r - (temptower.param1 > 700 ? 50 : 0);
                    }
                    else
                    {
                        moveY = temptower.y + temptower.r + (temptower.param1 > 700 ? 50 : 0);
                    }

                    if (Math.Abs(state.Queen.x - moveX) < 5 && Math.Abs(state.Queen.y - moveY) < 5)
                    {
                        SmartBuild(state, temptower, "TOWER");
                        return;
                    }
                    Move(state, moveX, moveY);
                    return;

                }

            }
            Move(state,startX, startY == 0 ? 1000 : 0);
            return;
  
        }

        private void Move(State state,int x, int y)
        {
            var testSites = state.Sites.Where(s=> (s.y< state.Queen.y&&s.y>y|| s.y > state.Queen.y && s.y < y)&&
                                                  (s.x < state.Queen.x && s.x > x || s.x > state.Queen.x && s.x < x) && 
                                                  FindLineCircleIntersections(s.x,s.y,s.r+15,new PointF(state.Queen.x,state.Queen.y),new PointF(x,y),out var point1, out var point2  )>1).OrderBy(s => s.dist);
            if (testSites.Any())
            {
                var closest = testSites.First();
                var tagentPoint = FindTangents(new PointF(closest.x, closest.y), closest.r+15,
                    new PointF(state.Queen.x, state.Queen.y), out var point1, out var point2);
                if (DistansTo(x, y, (int) point1.X, (int) point1.Y) < DistansTo(x, y, (int) point2.X, (int) point2.Y))
                {
                    Console.WriteLine($"MOVE {(int)point1.X} {(int)point1.Y}");
                }
                else
                {
                    Console.WriteLine($"MOVE {(int)point2.X} {(int)point2.Y}");
                }
                return;
            }

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
            
            var income = state.Sites.Where(x => x.IsPlayers && x.IsMine).Sum(y => y.param1);

            if (barracks != null && barracks.Any())
            {
                var bestBarrack = barracks.OrderBy(x => DistansTo(x.x, x.y, state.EnemyQueen.x, state.EnemyQueen.y))
                    .First();
                if (DistansTo(bestBarrack.x, bestBarrack.y, state.EnemyQueen.x, state.EnemyQueen.y) < 400)
                {
                    Train(bestBarrack);
                    return;
                }
           
            }
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
            return state.Sites.Where(x => x.IsEnemys && x.IsTower)
                .Any(y => DistansTo(y.x, y.y, site.x, site.y) +site.r< y.param2-allowance||(y.dist<site.dist&&y.distXstart<site.distXstart));
        }

        private void SmallDefend(State state, int numTowers = 3)
        {
           
            Console.Error.WriteLine("smalldefend");
            var towers = state.Sites.Where(x => x.IsPlayers && x.IsTower).ToList();
            if (towers.Count >= 2&& state.Units.Any(x => x.IsEnemys && x.IsKnight && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 350))
            {
                var enemyBarrack =
                    state.Sites.FirstOrDefault(x => x.IsEnemys && x.structureType == 2 && x.param2 == 0);
                if (enemyBarrack != null)
                {
                    var temptower=towers.OrderByDescending(x => DistansTo(x.x, x.y, enemyBarrack.x, enemyBarrack.y)).First();
                    var moveX = 0;
                    var moveY = 0;
                    if (temptower.x < enemyBarrack.x)
                    {

                        moveX = temptower.x - temptower.r - (temptower.param1 > 700 ? 50 : 0);
                    }
                    else
                    {
                        moveX = temptower.x + temptower.r + (temptower.param1 > 700 ? 50 : 0);
                    }
                    if (temptower.y < enemyBarrack.y)
                    {
                        moveY = temptower.y - temptower.r - (temptower.param1 > 700 ? 50 : 0);
                    }
                    else
                    {
                        moveY = temptower.y + temptower.r + (temptower.param1 > 700 ? 50 : 0);
                    }

                    if (DistansTo(moveX,moveY,state.Queen.x,state.Queen.y)<30)
                    {
                        SmartBuild(state,temptower,"TOWER");
                        return;
                    }
                    Move(state, moveX,moveY);
                    return;
                    
                }
                
            }


                if (state.TouchedSite!=-1&& state.Sites[state.TouchedSite].param1<450&& state.Sites[state.TouchedSite].IsPlayers && state.Sites[state.TouchedSite].IsTower 
                )//&& !state.Units.Any(x =>x.IsEnemys && x.IsKnight && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 200))
            {
                SmartBuild(state, state.Sites[state.TouchedSite], "TOWER");
                return;
            }
            if (towers.Count() >= numTowers)
            {
                

                SmartBuild(state, towers.OrderBy(x => x.param1).First(), "TOWER");
                return;

            }
            var nonBuilt = state.Sites.Where(x => (x.owner == -1 && x.IsUnbuilt && x.distXstart < 1300) && !InEnemyTowerRange(state, x)).OrderBy(x => x.dist).ToList();

            if (towers.Count==1)
            {
                var nonBuiltTemp = nonBuilt.FirstOrDefault(x => x.distXstart < towers.First().distXstart);
                if (nonBuiltTemp != null)
                {
                    SmartBuild(state, nonBuiltTemp, "TOWER");
                    return;
                }

            }
            if (!nonBuilt.Any())
            {
                nonBuilt = state.Sites.Where(x => x.IsPlayers && x.IsMine).OrderBy(x => x.dist)
                    .ToList();
            }

            if (!nonBuilt.Any())
            {
                nonBuilt = state.Sites.Where(x => x.owner != 1 && x.structureType != 1).OrderBy(x => x.dist)
                    .ToList();
            }
            if (!towers.Any())
            {
                SmartBuild(state, nonBuilt.First(), "TOWER");
                return;
            }
            if (towers.Count() < numTowers && towers.Any(x=>x.distXstart-100<Math.Abs(state.Queen.x-startX)))
            {
                var tower = towers.First();
                SmartBuild(state, nonBuilt.OrderBy(x => x.dist).First(), "TOWER");
                return;
            }

            SmartBuild(state, nonBuilt.First(), "TOWER");
            return;

        }

        public void SmartBuild(State state, Site site, string building)
        {
            if (DistansTo(site.x, site.y, state.EnemyQueen.x, state.EnemyQueen.y) < 150)
            {
          
                Build(state, site, "TOWER");
                return;
            }

            if (state.Units.Any(x =>x.owner==1&&x.type==0&& DistansTo(site.x, site.y, x.x, x.y) < 400) && building == "MINE")
            {
                Build(state, site, "TOWER");
                return; 
            }

            Build(state, site, building);
        }

        public void Build(State state, Site site, string building)
        {
            if (state.TouchedSite == site.siteId|| SiteIsBuildable(site))
            {
                Console.WriteLine($"BUILD {site.siteId} {building}");
                return;;
            }
            Move(state,site.x,site.y);
        }

        public bool SiteIsBuildable(Site site)
        {
            return site.dist < 61 + 15 + site.r;
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

            if (MyTurf.Any(x => x.IsEnemys && x.IsTower))
            {
                SetMyTurf(state, 1200);
            }


            if (state.Units.Any(x =>
                x.IsEnemys && x.IsKnight && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 300))
            {

                if (Mines.Count(x => x.IsTower) > 2)
                {
                    Console.Error.WriteLine("upgrade new tower because of close knights");
                    SmartBuild(state, MyTurf.Where(x => x.IsTower).OrderBy(x => x.dist).First(), "TOWER");
                    TrainStructured(state);
                    return;
                }
                Console.Error.WriteLine("build new tower because of close knights");
                SmartBuild(state, Mines.Where(x => x.structureType != 1).OrderBy(x => x.dist).First(), "TOWER");
                TrainStructured(state);
                return;
            }

            if (state.TouchedSite != -1)
            {
                
                if (state.TouchedSite != -1 && (state.Sites[state.TouchedSite].IsUnbuilt||(state.Sites[state.TouchedSite].maxMineSize > state.Sites[state.TouchedSite].param1 && state.Sites[state.TouchedSite].gold > 0)) && Towers.All(x => x.siteId != state.TouchedSite)&& Barracks.All(x => x.siteId != state.TouchedSite))
                {
                    Console.Error.WriteLine("build close mine");
                    SmartBuild(state, state.Sites[state.TouchedSite], "MINE");
                    TrainStructured(state);
                    return;
                }
                if (state.TouchedSite != -1 && state.Sites[state.TouchedSite].IsTower && state.Sites[state.TouchedSite].param1 < 700)
                {
                    Console.Error.WriteLine("build close tower");
                    SmartBuild(state, state.Sites[state.TouchedSite], "TOWER");
                    TrainStructured(state);
                    return;
                }
            }
            if (Barracks.All(x => x.IsUnbuilt))
            {
                Console.Error.WriteLine("build first barrack");

                SmartBuild(state, Barracks.OrderBy(x => x.dist).First(), "BARRACKS-KNIGHT");
                TrainStructured(state);
                return;

            }
            if (Mines.Count(x => x.IsPlayers && x.IsMine) < 3 && !state.Units.Any(x =>
                    x.IsEnemys && x.IsKnight && DistansTo(x.x, x.y, state.Queen.x, state.Queen.y) < 300))
            {
                Console.Error.WriteLine("building mines");
                SmartBuild(state, Mines.Where(x => x.IsUnbuilt).OrderBy(x => x.dist).First(), "MINE");
                TrainStructured(state);
                return;
            }

            

            if (Towers.Any(x => x.structureType != 1)&&state.Sites.Any(x=>x.owner==1&&x.structureType==2&&x.param2==0))
            {
                Console.Error.WriteLine("building tower");
                SmartBuild(state, Towers.Where(x=>x.structureType!=1).OrderBy(x=>x.dist).First(), "TOWER");
                TrainStructured(state);
                return;
            }

            if (state.Gold > 150)
            {
                if (Barracks.Any(x => x.structureType != 2)){
                    //if (Barracks.Any(x => x.structureType != 2&&x.param2!= 2))
                    //{
                    //    Console.Error.WriteLine("building giants");
                    //    SmartBuild(state,Barracks.OrderBy(x=>x.dist).First(),"BARRACKS-GIANT");
                    //    TrainStructured(state);
                    //    return;
                    //}
                    Console.Error.WriteLine("building more knights");
                    SmartBuild(state, Barracks.OrderBy(x => x.dist).First(), "BARRACKS-KNIGHT");
                    TrainStructured(state);
                    return;
                }
            }

            if (MyTurf.Any(x => x.IsUnbuilt))
            {
                if (MyTurf.Any(x => x.IsUnbuilt && x.gold > 0))
                {
                    Console.Error.WriteLine("building turf mines");
                    SmartBuild(state, MyTurf.Where(x => x.IsUnbuilt && x.gold > 0).OrderBy(x => x.dist).First(), "MINE");
                    TrainStructured(state);
                    return;
                }
                Console.Error.WriteLine("building mine mines");
                SmartBuild(state, Mines.Where(x => x.structureType != 0).OrderBy(x => x.dist).First(), "MINE");
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
            else if (Towers.Any(x => x.IsEnemys && x.IsTower))
            {
                Towers = MyTurf.OrderBy(x => DistansTo(x.x, x.y, 700, 500)).Take(Towers.Count(x => !(x.IsEnemys && x.IsTower))).ToList();

            }

            if (!Barracks.Any())
            {
                Barracks = MyTurf.Where(x => Towers.All(y => x.siteId != y.siteId)).OrderByDescending(x => x.distXstart).Take(2).ToList();
                Barracks.Add(MyTurf.Where(x => Math.Abs(startY - x.y) < 300).OrderBy(x => x.distXstart).Skip(1).First());

            }
            else if (Barracks.Any(x => x.IsEnemys && x.IsTower))
            {
                var tempB = Barracks.Where(x => !(x.IsEnemys && x.IsTower)).ToList();
                var tempB2 = MyTurf.OrderByDescending(x => x.distXstart).Take(Barracks.Count(x => !(x.IsEnemys && x.IsTower))).ToList();
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
                

                //if (!state.Units.Any(x => x.IsPlayers && x.IsGiant)&&Barracks.Any(x=>x.param2==2))
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

            if (state.Units.Any(x => x.IsPlayers && x.IsGiant))
            {
                var barracks = state.Sites.Where(x => x.IsPlayers && x.structureType == 2 && x.param2 != 2);
                SmallDefend(state);
                TrainBest(state, barracks);
                return;
            }
            if (state.Sites.Any(x => x.IsPlayers && x.structureType == 2 && x.param2 == 2))
            {
                var giantBarrack = state.Sites.First(x => x.IsPlayers && x.structureType == 2 && x.param2 == 2);
                SmallDefend(state);
                Train(giantBarrack);
                return;

            }

            var giantBarrackToBuild= state.Sites.Where(x => x.owner != 1 && x.IsTower &&x.distXstart>500).OrderBy(x=>x.dist).First();
            SmartBuild(state,giantBarrackToBuild,"BARRACKS-GIANT");
            Train();

        }
    }

    public class PointF
    {

        public PointF(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }
    }

    public static int FindLineCircleIntersections(
    float cx, float cy, float radius,
    PointF point1, PointF point2,
    out PointF intersection1, out PointF intersection2)
    {
        float dx, dy, A, B, C, det, t;

        dx = point2.X - point1.X;
        dy = point2.Y - point1.Y;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
        C = (point1.X - cx) * (point1.X - cx) +
            (point1.Y - cy) * (point1.Y - cy) -
            radius * radius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0))
        {
            // No real solutions.
            intersection1 = new PointF(float.NaN, float.NaN);
            intersection2 = new PointF(float.NaN, float.NaN);
            return 0;
        }
        else if (det == 0)
        {
            // One solution.
            t = -B / (2 * A);
            intersection1 =
                new PointF(point1.X + t * dx, point1.Y + t * dy);
            intersection2 = new PointF(float.NaN, float.NaN);
            return 1;
        }
        else
        {
            // Two solutions.
            t = (float)((-B + Math.Sqrt(det)) / (2 * A));
            intersection1 =
                new PointF(point1.X + t * dx, point1.Y + t * dy);
            t = (float)((-B - Math.Sqrt(det)) / (2 * A));
            intersection2 =
                new PointF(point1.X + t * dx, point1.Y + t * dy);
            return 2;
        }
    }

    public static bool FindTangents(PointF center, float radius,
        PointF external_point, out PointF pt1, out PointF pt2)
    {
        // Find the distance squared from the
        // external point to the circle's center.
        double dx = center.X - external_point.X;
        double dy = center.Y - external_point.Y;
        double D_squared = dx * dx + dy * dy;
        if (D_squared < radius * radius)
        {
            pt1 = new PointF(-1, -1);
            pt2 = new PointF(-1, -1);
            return false;
        }

        // Find the distance from the external point
        // to the tangent points.
        double L = Math.Sqrt(D_squared - radius * radius);

        // Find the points of intersection between
        // the original circle and the circle with
        // center external_point and radius dist.
        FindCircleCircleIntersections(
            center.X, center.Y, radius,
            external_point.X, external_point.Y, (float)L,
            out pt1, out pt2);

        return true;
    }

    public static int FindCircleCircleIntersections(
        float cx0, float cy0, float radius0,
        float cx1, float cy1, float radius1,
        out PointF intersection1, out PointF intersection2)
    {
        // Find the distance between the centers.
        float dx = cx0 - cx1;
        float dy = cy0 - cy1;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > radius0 + radius1)
        {
            // No solutions, the circles are too far apart.
            intersection1 = new PointF(float.NaN, float.NaN);
            intersection2 = new PointF(float.NaN, float.NaN);
            return 0;
        }
        else if (dist < Math.Abs(radius0 - radius1))
        {
            // No solutions, one circle contains the other.
            intersection1 = new PointF(float.NaN, float.NaN);
            intersection2 = new PointF(float.NaN, float.NaN);
            return 0;
        }
        else if ((dist == 0) && (radius0 == radius1))
        {
            // No solutions, the circles coincide.
            intersection1 = new PointF(float.NaN, float.NaN);
            intersection2 = new PointF(float.NaN, float.NaN);
            return 0;
        }
        else
        {
            // Find a and h.
            double a = (radius0 * radius0 -
                        radius1 * radius1 + dist * dist) / (2 * dist);
            double h = Math.Sqrt(radius0 * radius0 - a * a);

            // Find P2.
            double cx2 = cx0 + a * (cx1 - cx0) / dist;
            double cy2 = cy0 + a * (cy1 - cy0) / dist;

            // Get the points P3.
            intersection1 = new PointF(
                (float)(cx2 + h * (cy1 - cy0) / dist),
                (float)(cy2 - h * (cx1 - cx0) / dist));
            intersection2 = new PointF(
                (float)(cx2 - h * (cy1 - cy0) / dist),
                (float)(cy2 + h * (cx1 - cx0) / dist));

            // See if we have 1 or 2 solutions.
            if (dist == radius0 + radius1) return 1;
            return 2;
        }
    }

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
    public int distYstart { get; set; }

    public bool IsTower => structureType == 1;
    public bool IsMine => structureType == 0;
    public bool IsBarrack => structureType == 2;
    public bool IsUnbuilt => structureType == -1&& owner==-1;
    public bool IsKnightBarrack => IsBarrack && param2 == 0;
    public bool IsArcherBarrack => IsBarrack && param2 == 1;
    public bool IsGiantBarrack => IsBarrack && param2 == 2;
    public bool IsEnemys => owner == 1;
    public bool IsPlayers => owner == 0;
    public int TowerHP => param1;
    public int TowerRange => param2;
    public int TrainingTurnsLeft => param1;
    public int MineIncome => param1;
    public bool IsMineWithIncomeLeft => IsMine && maxMineSize > MineIncome&&gold>0;
    public bool IsMineWithGoldLeft => gold > 0;
}

public class Unit
{

    public int y { get; set; }
    public int x { get; set; }
    public int owner { get; set; }
    public int health { get; set; }
    public int type { get; set; }
    public bool IsEnemys => owner == 1;
    public bool IsPlayers => owner == 0;
    public bool IsKnight => type == 0;
    public bool IsQueen => type == -1;
    public bool IsArcher => type == 1;
    public bool IsGiant => type == 2;
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