namespace AquariumShop
{
    [System.Serializable]
    public class FishInstance
    {
        public FishSpecies species;
        public float hunger;

        public bool IsAlive => hunger < 1f;
        public int Space => species != null ? species.spaceRequired : 1;

        public FishInstance(FishSpecies species)
        {
            this.species = species;
            hunger = 0f;
        }
    }
}
