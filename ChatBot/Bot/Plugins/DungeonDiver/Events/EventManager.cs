using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins
{
    /// <summary>
    /// handles managing diving events
    /// </summary>
    public static class EventManager
    {
        /// <summary>
        /// random value for ... randomization
        /// </summary>
        public static Random Rng = new Random();

        /// <summary>
        /// list of events we can hit
        /// </summary>
        public static List<Event> Events = new List<Event>();

        /// <summary>
        /// our base constructor
        /// </summary>
        static EventManager()
        {
            Events.Add(new Event() { name = "", type = EventType.Enemy, gold = 2, xp = 7, prog = 3, chance = 6, description = "" });
            Events.Add(new Event() { name = "", type = EventType.Enemy, gold = 3, xp = 4, prog = 3, chance = 8, description = "" });
            Events.Add(new Event() { name = "", type = EventType.Enemy, gold = 2, xp = 4, prog = 2, chance = 7, description = "" });
            Events.Add(new Event() { name = "", type = EventType.Gilded, gold = 0, xp = 0, prog = 25, chance = .04, description = "[b][color=cyan]You have found a boon for your [/color][/b] {itemtype}[b][color=cyan]! Bonuses with it are now double[/color][/b]!" });
            Events.Add(new Event() { name = "", type = EventType.Gold, gold = 12, xp = 0, prog = 7, chance = 1, description = "You found a discarded pouch containing [color=yellow][b]{amount}[/b][/color] gold!" });
            Events.Add(new Event() { name = "", type = EventType.Gold, gold = 24, xp = 0, prog = 11, chance = .7, description = "You found a small chest containing [color=yellow][b]{amount}[/b][/color] gold!" });
            Events.Add(new Event() { name = "", type = EventType.Xp, gold = 0, xp = 60, prog = 3, chance = .7, description = "You had a long talk with a surprisingly beneign denizien of the dungeon. The event earned you [color=pink][b]{amount}[/b][/color] experience!" });
            Events.Add(new Event() { name = "", type = EventType.Xp, gold = 0, xp = 40, prog = 3, chance = 1.1, description = "You discovered a hidden room, earning [color=pink][b]{amount}[/b][/color] experience!" });
        }

        /// <summary>
        /// let's roll for some events!
        /// </summary>
        /// <param name="progress">how much progress we made in the dive</param>
        /// <param name="fc">floor card</param>
        /// <param name="pc">player card</param>
        /// <param name="gold">gold earned so far</param>
        /// <param name="xp">xp earned so far</param>
        /// <returns>total combined results of the dive, not added to player card</returns>
        public static DiveResults RollForEvents(int progress, FloorCard fc, PlayerCard pc, int gold, int xp)
        {
            DiveResults toReturn = new DiveResults();

            toReturn.gold = gold;
            toReturn.xp = xp;
            toReturn.prog = progress;
            int eventCooldown = 0;

            for (int roll = 0; roll < toReturn.prog; roll++)
            {
                if (eventCooldown > 0)
                {
                    eventCooldown--;
                    continue;
                }

                double chance = Rng.NextDouble() * 100.0;
                var validEvents = Events.Where(x => x.chance >= chance).ToList();
                if (validEvents.Count > 0)
                {
                    toReturn = RollEvent(toReturn, validEvents, out eventCooldown);
                }
            }

            toReturn = CheckGilded(toReturn, pc);
            toReturn = RandomizeThings(toReturn);
            return toReturn;
        }

        /// <summary>
        /// just randomizes stuff
        /// </summary>
        /// <param name="dr">our dive results</param>
        /// <param name="pc">our player card</param>
        /// <param name="fc">our floor card</param>
        /// <returns>randomized dive results</returns>
        static DiveResults RandomizeThings(DiveResults dr)
        {
            for (int i = 0; i < dr.events.Count; i++)
            {
                var eve = dr.events[i];
                if (string.IsNullOrWhiteSpace(eve.description))
                    continue;

                if (eve.type == EventType.Gold)
                {
                    int lowbounds = (int)(eve.gold * .5);
                    int highbounds = (int)(eve.gold * 1.5);

                    eve.gold = Rng.Next(lowbounds, highbounds);

                    dr.gold += eve.gold;
                    dr.events[i] = eve;
                    eve.description = eve.description.Replace("{amount}", dr.events[i].gold.ToString());
                }
                else if (eve.type == EventType.Xp)
                {
                    int lowbounds = eve.xp / 2;
                    int highbounds = (int)(eve.xp * 1.5);
                    eve.xp = Rng.Next(lowbounds, highbounds);
                    dr.xp += eve.xp;
                    dr.events[i] = eve;
                    eve.description = eve.description.Replace("{amount}", dr.events[i].xp.ToString());
                }
            }
            return dr;
        }

        /// <summary>
        /// rolls for a single event
        /// </summary>
        /// <param name="dr">our dive results</param>
        /// <param name="validEvents">list of valid events based on random chance</param>
        /// <param name="eventCooldown">cooldown between events</param>
        /// <returns>updated dive results</returns>
        static DiveResults RollEvent(DiveResults dr, List<Event> validEvents, out int eventCooldown)
        {
            Event ev = new Event();
            Event tev = validEvents[Rng.Next(0, validEvents.Count)];
            ev.name = tev.name;
            ev.gold = tev.gold;
            ev.prog = tev.prog;
            ev.type = tev.type;
            ev.xp = tev.xp;
            ev.description = tev.description;
            ev.chance = tev.chance;

            // prevent getting gilded multiple times in one dive, just in case
            if (ev.type == EventType.Gilded && dr.events.Any(x => x.type == EventType.Gilded))
            {
                eventCooldown = 0;
                return dr;
            }
            else if (ev.type == EventType.Gold && dr.events.Any(x => x.type == EventType.Gold))
            {
                var blah = dr.events.First(x => x.type == EventType.Gold);
                Console.WriteLine(ev.xp + " - " + blah.xp);

                blah.gold += ev.gold;
                eventCooldown = ev.prog;
                Console.WriteLine(ev.xp + " - " + blah.xp);
                return dr;
            }
            else if (ev.type == EventType.Xp && dr.events.Any(x => x.type == EventType.Xp))
            {
                var blah = dr.events.First(x => x.type == EventType.Xp);
                Console.WriteLine(ev.xp + " - " + blah.xp);

                blah.xp += ev.xp;
                eventCooldown = ev.prog;
                Console.WriteLine(ev.xp + " - " + blah.xp);
                return dr;
            }
            else if (ev.type == EventType.Enemy && dr.events.Any(x => x.type == EventType.Enemy))
            {
                dr.events.Add(ev);
                dr.gold += ev.gold;
                dr.xp += ev.xp;
                eventCooldown = ev.prog;
                return dr;
            }
            else if (ev.type == EventType.Gold)
            {
                dr.events.Add(ev);
                eventCooldown = ev.prog;
                Console.WriteLine(ev.gold);
                return dr;
            }
            else
            {
                dr.events.Add(ev);
                eventCooldown = ev.prog;
                return dr;
            }
        }

        /// <summary>
        /// checks if we've earned ourselves a gilded event and adds it to the player card
        /// </summary>
        /// <param name="dr">our dive results</param>
        /// <param name="pc">player card</param>
        /// <returns>returns updated dive results</returns>
        static DiveResults CheckGilded(DiveResults dr, PlayerCard pc)
        {
            List<ItemType> validTypes = new List<ItemType>();
            Event eve = new Event();
            if (!dr.events.Any(x => x.type == EventType.Gilded))
            {
                return dr;
            }

            eve = dr.events.First(x => x.type == EventType.Gilded);
            if (pc.weaponperklvl != 1)
            {
                validTypes.Add(ItemType.Weapon);
            }
            if (pc.gearperklvl != 1)
            {
                validTypes.Add(ItemType.Gear);
            }
            if (pc.specialperklvl != 1)
            {
                validTypes.Add(ItemType.Special);
            }

            if (validTypes.Count == 0)
            {
                dr.events.Remove(eve);
                return dr;
            }

            var upgraded = validTypes[Rng.Next(0, validTypes.Count)];

            switch (upgraded)
            {
                case ItemType.Weapon:
                    pc.weaponperklvl += 1;
                    GameDb.UpdateCard(pc);

                    eve.description = eve.description.Replace("{itemtype}", $"{pc.weapon}");
                    break;
                case ItemType.Gear:
                    pc.gearperklvl += 1;
                    GameDb.UpdateCard(pc);

                    eve.description = eve.description.Replace("{itemtype}", $"{pc.gear}");
                    break;
                case ItemType.Special:
                    pc.specialperklvl += 1;
                    GameDb.UpdateCard(pc);

                    eve.description = eve.description.Replace("{itemtype}", $"{pc.special}");
                    break;
            }

            return dr;
        }
    }
}
