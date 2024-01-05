using System;
using System.Collections.Generic;

namespace SpaceAce.Gameplay.Inventories
{
    public sealed class Inventory
    {
        public event EventHandler ContentChanged;

        private readonly HashSet<ItemStack> _content = new();

        public int ContentCount => _content.Count;

        public void Add(ItemStack stack)
        {
            if (stack is null)
                throw new ArgumentNullException(nameof(stack),
                    $"Attempted to add an empty {typeof(ItemStack)}!");

            if (_content.TryGetValue(stack, out ItemStack theSameStack) == true)
            {
                theSameStack.Add(stack.Amount);
            }
            else
            {
                _content.Add(stack);
                stack.Depleted += StackDepletedEventHandler;
            }

            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Add(IEnumerable<ItemStack> stacks)
        {
            if (stacks is null)
                throw new ArgumentNullException(nameof(stacks),
                    $"Attempted to add an empty {typeof(IEnumerable<ItemStack>)}!");

            foreach (var stack in stacks) Add(stack);
        }

        public bool Contains(ItemStack stack) => _content.Contains(stack);

        public void Clear()
        {
            foreach (var stack in _content)
                stack.Depleted -= StackDepletedEventHandler;

            _content.Clear();
        }

        public IEnumerable<ItemStack> GetContent() => _content;

        public IEnumerable<ItemStackSavableState> GetContentSavableState()
        {
            List<ItemStackSavableState> states = new(_content.Count);

            foreach (var stack in _content)
            {
                ItemStackSavableState state = stack.GetSavableState();
                states.Add(state);
            }

            return states;
        }

        private void StackDepletedEventHandler(object sender, EventArgs e)
        {
            _content.Remove(sender as ItemStack);
        }
    }
}