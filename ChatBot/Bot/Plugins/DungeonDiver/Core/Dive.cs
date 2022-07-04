using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins
{
    public partial class GameCore : PluginBase
    {
        List<string> MonsterDefeatedBlurbs = new List<string>() { "defeated {monsters} monsters." };
        const string DefeatedBlurbLocation = @"..\..\Data\DefeatedBlurbs.txt";

        /// <summary>
        /// handles loaded our monster blurb messages
        /// </summary>
        public void LoadMonsterDefeatedBlurbs()
        {
            MonsterDefeatedBlurbs = System.IO.File.ReadAllLines(DefeatedBlurbLocation).ToList();
        }

        /// <summary>
        /// calculates how much xp is needed until the next levelup
        /// </summary>
        /// <param name="pc">card of the player</param>
        /// <returns>xp needed to level</returns>
        public int GetXpToLevel(PlayerCard pc)
        {
            int xpneeded = 0;
            for (int i = 1; i <= pc.level; i++)
            {
                xpneeded += (int)(500 * (1 + (.2 * (i - 1))));
            }
            return xpneeded - pc.xp;
        }

        /// <summary>
        /// calculates how much xp the player earned
        /// </summary>
        /// <param name="fc">the current floor</param>
        /// <param name="pc">the player</param>
        /// <returns>xp gained</returns>
        public int CalculateExperience(FloorCard fc, PlayerCard pc)
        {
            // low level bonus
            int difference = fc.floor - pc.level;
            if (difference <= 0) difference = 0;
            if (difference > 5) difference = 5;
            difference *= 5;

            int low = rng.Next(40 + difference, 45 + difference);
            int high = rng.Next(55 + difference, 60 + difference);
            return rng.Next(low, high);
        }

        /// <summary>
        /// calculates the base gold earned based on current floor
        /// </summary>
        /// <param name="fc">our current floor</param>
        /// <returns></returns>
        public int CalculateGold(FloorCard fc)
        {
            int low = 9;
            int high = 12;
            int basegold = rng.Next(low, high);
            return (int)(basegold * (1 + (fc.floor * .1)));
        }

        /// <summary>
        /// calculates how much progress was earned
        /// </summary>
        /// <param name="fc">the current floor</param>
        /// <param name="pc">the player</param>
        /// <returns>amount of earned progress</returns>
        public int CalculateProgress(PlayerCard pc)
        {
            int baseProgressLow = 50;
            int baseProgressHigh = 60;

            int pretotal = (int)(rng.Next(baseProgressLow, baseProgressHigh) * (1 + (pc.level * 0.1)));
            int gearbonus = pc.weaponlvl + pc.gearlvl + pc.speciallvl;

            if (pc.weaponperklvl == 1) gearbonus += pc.weaponlvl;
            if (pc.gearperklvl == 1) gearbonus += pc.gearlvl;
            if (pc.specialperklvl == 1) gearbonus += pc.speciallvl;

            return pretotal + gearbonus;
        }

        /// <summary>
        /// handles actually taking a dive
        /// </summary>
        /// <param name="channel">the source channel</param>
        /// <param name="pc">player making the dive</param>
        public void DiveAction(string channel, PlayerCard pc)
        {
            //let's see if they've cooled down
            var cooldown = (new TimeSpan(0, BaseDiveCooldown, 0) - new TimeSpan(DateTime.Now.Ticks - pc.lastDive.Ticks));
            string nickname = (string.IsNullOrWhiteSpace(pc.nickname)) ? $"[color=white]{pc.name}[/color]" : $"[color=white]{pc.nickname}[/color]";
            if (cooldown.Ticks > 0)
            {
                SystemController.Instance.Respond(channel, $"[b]{nickname}[/b], the dungeon barrier is keeping you from re-entering. {cooldown.Hours} hours, {cooldown.Minutes} minutes, {cooldown.Seconds} seconds of cooldown remaining.", pc.name);
                return;
            }

            // let's dive
            FloorCard fc = FloorDb.GetFloor();
            DiveResults dr = EventManager.RollForEvents(CalculateProgress(pc), fc, pc, CalculateGold(fc), CalculateExperience(fc, pc));

            int curLevel = pc.xp;
            //List<Quest> hitQuests = QuestManager.RollQuests(CalculateProgress(pc), fc, pc);

            // update player progress
            pc.lastDive = DateTime.Now;
            pc.killed += dr.events.Count(x => x.type == EventType.Enemy);

            // temp
            if (fc.floor == 30)
            {
                dr.gold = (int)(dr.gold * 1.5);
            }

            pc.gold += dr.gold;
            pc.xp += dr.xp;

            // check for level up
            bool didLevel = false;
            if (GetXpToLevel(pc) < dr.xp)
            {
                didLevel = true;
                pc.level += 1;
            }

            // update our player
            GameDb.UpdateCard(pc);

            // update floor progress
            fc.currentxp += dr.prog;
            FloorDb.UpdateFloor(fc);

            // if we've cleared the current floor
            if (fc.currentxp > fc.neededxp)
            {
                FloorCard fcnext = FloorDb.GetFloor();
                SystemController.Instance.Respond(channel, $"[b][color=green]FLOOR CLEAR!![/color][/b] Congratulations! Just beyond, however, looms... " + $"{fcnext.name} - {fcnext.notes} [color=green]([b]Floor {fcnext.floor}: {fcnext.currentxp}[/b]/[b]{fcnext.neededxp}[/b])[/color]", pc.name);
            }

            int killedCount             = dr.events.Count(x => x.type == EventType.Enemy);
            string monsterBlurb         = MonsterDefeatedBlurbs[rng.Next(0, MonsterDefeatedBlurbs.Count)].Replace("{monsters}", $"[b][color={enemycolor}]{killedCount}[/color][/b]").Replace("{weapon}", $"{pc.weapon}").Replace("{gear}", $"{pc.gear}").Replace("{special}", $"{pc.special}");

            // if we leveled
            string toSend               = $"{(didLevel ? "[b][color=green]Level up!![/color][/b] " : "")}[b]{nickname}:[/b] ";

            // append our monster blurb if we killed any monsters
            if (killedCount > 0) toSend += $"{monsterBlurb} ";

            // append any event-specific descriptions
            //toSend += string.IsNullOrWhiteSpace(dr.eventReturnDescription) ? "" : dr.eventReturnDescription;
            var prunedResults = dr.events.Where(x => !string.IsNullOrWhiteSpace(x.description));
            if (prunedResults.Any())
            {
                foreach (var res in prunedResults)
                {
                    toSend += res.description + " ";
                }
            }

            // append our summary
            toSend += $"You earned [color={goldcolor}][b]{dr.gold}[/b][/color] total gold, [color={xpcolor}][b]{dr.xp}[/b][/color] total experience, and contributed [color={progcolor}][b]{dr.prog}[/b][/color] progress this dive.";

            // send!
            SystemController.Instance.Respond(channel, toSend, pc.name);
        }
    }
}
