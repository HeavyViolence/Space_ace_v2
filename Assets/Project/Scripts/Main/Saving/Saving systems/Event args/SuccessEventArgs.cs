using System;

namespace SpaceAce.Main.Saving
{
    public class SuccessEventArgs : EventArgs
    {
        public string ID { get; }

        public SuccessEventArgs(string id)
        {
            if (string.IsNullOrEmpty(id) == true ||
                string.IsNullOrWhiteSpace(id) == true)
            {
                throw new ArgumentNullException();
            }

            ID = id;
        }
    }
}