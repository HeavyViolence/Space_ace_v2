using System;

namespace SpaceAce.Main.Saving
{
    public sealed class FailEventArgs : SuccessEventArgs
    {
        public string Message { get; }

        public FailEventArgs(string id, string message) : base(id)
        {
            if (string.IsNullOrEmpty(message) == true ||
                string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException();
            }

            Message = message;
        }
    }
}