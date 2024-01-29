namespace SpaceAce.Gameplay.Items
{
    public readonly struct ItemUsageResult
    {
        public bool Used { get; }
        public object[] Args { get; }

        public ItemUsageResult(bool used, params object[] args)
        { 
            Used = used;
            Args = args;
        }
    }
}