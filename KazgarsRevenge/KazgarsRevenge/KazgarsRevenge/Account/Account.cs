using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace KazgarsRevenge
{
    /// <summary>
    /// Class used to hold information about a user's account
    /// </summary>
    public class Account
    {
        // The Default starting level for a character
        private static readonly int START_CHAR_LEVEL = 1;

        // Start with 0 unlocked floors (have to play tutorial first)
        private static readonly int START_UNLOCKED_FLOORS = 0;

        // TODO how many?
        private static readonly int START_TALENT_POINTS = 0;

        // The name associated with the account
        public string Name
        {
            get;
            private set;
        }

        // The character's level
        public int CharacterLevel
        {
            get;
            private set;
        }

        // The number of floors that have been unlocked
        public int UnlockedFloors
        {
            get;
            private set;
        }

        public int AvailTalentPoints
        {
            get;
            private set;
        }

        // All the abilities the Character has unlocked
        public IList<Ability> UnlockedAbilities
        {
            get;
            private set;
        }


        /// <summary>
        /// Constructor used by the Json Deserializer
        /// </summary>
        [JsonConstructor]
        public Account(string Name, int CharacterLevel, int UnlockedFloors, int AvailTalentPoints, IList<Ability> UnlockedAbilities)
        {
            this.Name = Name;
            this.CharacterLevel = CharacterLevel;
            this.UnlockedFloors = UnlockedFloors;
            this.AvailTalentPoints = AvailTalentPoints;
            this.UnlockedAbilities = UnlockedAbilities;
        }

        /// <summary>
        /// Creates a new Account with the given name, using the default starting values
        /// </summary>
        /// <param name="name"></param>
        public Account(string name)
        {
            this.Name = name;
            this.CharacterLevel = START_CHAR_LEVEL;
            this.UnlockedFloors = 1; // TODO change back to START_UNLOCKED_FLOORS
            this.AvailTalentPoints = START_TALENT_POINTS;

            this.UnlockedAbilities = new List<Ability>();
            // TODO are there any unlocked at the start? - Prob not, just give them some points to assign?
        }

        /// <summary>
        /// Dummy constructor used for testing
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CharacterLevel"></param>
        /// <param name="UnlockedFloors"></param>
        public Account(string name, int CharacterLevel, int UnlockedFloors, int AvailTalentPoints)
        {
            this.Name = name;
            this.CharacterLevel = CharacterLevel;
            this.UnlockedFloors = UnlockedFloors;
            this.AvailTalentPoints = AvailTalentPoints;
            this.UnlockedAbilities = new List<Ability>();
        }

        /// <summary>
        /// Converts the object to Json
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals(obj as Account);
        }

        private bool Equals(Account other)
        {
            // TODO is this good enough?
            return String.Equals(this.Name, other.Name);
        }

        //the warning bothered me :P
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
