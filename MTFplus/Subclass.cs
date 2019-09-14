using Smod2.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTFplus
{
    public struct Subclass
    {
        public readonly string name;
        public readonly Role role;
        public readonly List<ItemType> inventory;
        public readonly int[] imInv;
        public readonly float probability;
        public readonly int[] ammo;
        public readonly string broadcast;
        public int maxHP;

        public Subclass(string name, Role role, List<ItemType> inventory, int[] imInv, float probability, int[] ammo, string broadcast, int maxHP)
        {
            this.name = name;
            this.role = role;
            this.inventory = inventory;
            this.probability = probability;
            this.ammo = ammo;
            this.broadcast = broadcast;
            this.imInv = imInv;
            this.maxHP = maxHP;
        }

        public override bool Equals(object obj)
        {
            return obj is Subclass subclass && this.name == subclass.name;
        }

        public override int GetHashCode()
        {
            var hashCode = -265901448;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            return hashCode;
        }

        public override string ToString()
        {
            return this.name + ":\n"
                + "Role: " + this.role + Environment.NewLine
                + "Inventory: " + this.ParseItems(this.inventory)
                + "Probability: " + this.probability + Environment.NewLine
                + "Ammo5: " + this.ammo[0] + Environment.NewLine
                + "Ammo7: " + this.ammo[1] + Environment.NewLine
                + "Ammo9: " + this.ammo[2] + Environment.NewLine
                + "HP: " + this.maxHP + Environment.NewLine
                + "Broadcast: " + (string.IsNullOrWhiteSpace(this.broadcast) ? "Empty." : this.broadcast);
        }
        private string ParseItems(List<ItemType> items)
        {
            int i, j, size;
            string parsedValue = string.Empty;
            Dictionary<int, int> IMpositions = new Dictionary<int, int>(16);
            if (Events.IMbool)
            {
                for (i = 0; i < 16; i++)
                {
                    if (this.imInv[i] < 0) continue;
                    IMpositions.Add(i, this.imInv[i]);
                }
            }
            size = items.Count + IMpositions.Count;
            for (i = 0, j = 0; i < size; i++)
            {
                if (IMpositions.ContainsKey(i))
                {
                    parsedValue += "IM:" + imInv[i] + (i != size - 1 ? ", " : Environment.NewLine); // if you're copying this code and don't want a new line at the end, substitute NewLine with string.Empty
                }
                else
                {
                    if (!IMpositions.ContainsKey(j)) parsedValue += items[j] + (i != size - 1 ? ", " : Environment.NewLine); // if you're copying this code and don't want a new line at the end, substitute NewLine with string.Empty 
                    j++;
                }
            }
            return parsedValue;
        }

        public static bool operator ==(Subclass left, Subclass right) => left.Equals(right);
        public static bool operator !=(Subclass left, Subclass right) => !(left == right);
    }

    public static class SubclassMethods
    {
        public static Subclass Get(this List<Subclass> subclasses, string name)
        {
            // IEnumerable to not get fucky strings with no relation to our word at all
            IEnumerable<Subclass> matchingSubclasses = subclasses.Where(x => x.name.StartsWith(name));
            Subclass yourSubclass = Empty;
            int shortestDistance = 0xFFFFFFF, currDistance;
            if (matchingSubclasses.Count() <= 0)
            {
                return Empty;
            }
            foreach (Subclass sc in matchingSubclasses)
            {
                currDistance = StringDistance.LevenshteinDistance(sc.name, name);
                if (currDistance < shortestDistance)
                {
                    yourSubclass = sc;
                }
            }
            return yourSubclass;
        }
        public static Subclass Empty = new Subclass(string.Empty, Role.UNASSIGNED, null, null, 0, null, string.Empty, 0);
    }
    public static class StringDistance
    {
        /// <summary>
        /// Compute the distance between two strings. Modified and slightly explained by RogerFK, from: https://www.csharpstar.com/csharp-string-distance-algorithm/
        /// </summary>
        public static int LevenshteinDistance(string arg1, string arg2)
        {
            int n = arg1.Length, m = arg2.Length, i, j, cost;
            int[,] matrix = new int[n + 1, m + 1];

            // Return these values if any string is empty
            if (n == 0) return m;

            if (m == 0) return n;

            // Initialize the matrix
            for (i = 0; i <= n; matrix[i, 0] = i++) ;

            for (j = 0; j <= m; matrix[0, j] = j++) ;

            // Set the values inside the matrix
            for (i = 1; i <= n; i++)
            {
                for (j = 1; j <= m; j++)
                {
                    // Check if they are the same letter to calculate costs
                    cost = (arg2[j - 1] == arg1[i - 1]) ? 0 : 1;

                    // Set the i and j matrix's cell as the distance, so it would be 0 if it's the same string up to that point. That's why the cost matters
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            // Return the end of the path, which contains the maximum "cost" a.k.a. distance to the string
            return matrix[n, m];
        }
    }
}
