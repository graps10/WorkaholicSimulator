using System.Collections.Generic;
using Entities;

namespace QuestSystem.Base
{
    public class QuestEntitiesList: IList<Entity>
    {
        private readonly List<Entity> _internalList = new();
        public bool ListHasBeenModified { get; private set; }

        public Entity this[int index] { get => _internalList[index]; set => _internalList[index] = value; }

        public int Count => _internalList.Count;
        public bool IsReadOnly => false;

        public void Add(Entity item)
        {
            _internalList.Add(item);
            ListHasBeenModified = true;
        }

        public bool Remove(Entity item)
        {
            bool removed = _internalList.Remove(item);
            if (removed) ListHasBeenModified = true;
            return removed;
        }
        
        public void Clear() { _internalList.Clear(); ListHasBeenModified = true; }
        public bool Contains(Entity item) => _internalList.Contains(item);
        public void CopyTo(Entity[] array, int arrayIndex) => _internalList.CopyTo(array, arrayIndex);
        public IEnumerator<Entity> GetEnumerator() => _internalList.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _internalList.GetEnumerator();
        public int IndexOf(Entity item) => _internalList.IndexOf(item);
        public void Insert(int index, Entity item) { _internalList.Insert(index, item); ListHasBeenModified = true; }
        public void RemoveAt(int index) { _internalList.RemoveAt(index); ListHasBeenModified = true; }
    }
}