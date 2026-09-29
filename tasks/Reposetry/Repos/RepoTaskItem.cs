using tasks.Models;
using tasks.Reposetry.IRepos;

namespace tasks.Reposetry.Repos
{
    public class RepoTaskItem : IRepoTaskItem
    {
        private readonly List<TaskItem> _items = new()
        {
            new TaskItem { Id = 1, Title = "Buy milk", IsCompleted = false },
            new TaskItem { Id = 2, Title = "Finish the Week 2 README", IsCompleted = true },
            new TaskItem { Id = 3, Title = "Review the HTTP status codes", IsCompleted = false }
        };
        private readonly object _lockObject = new();

        public List<TaskItem> Items
        {
            get
            {
                lock (_lockObject)
                {
                    return new List<TaskItem>(_items);
                }
            }
        }

        public void AddItem(TaskItem item)
        {
            lock (_lockObject)
            {
                _items.Add(item);
            }
        }

        public bool RemoveItem(int id)
        {
            lock (_lockObject)
            {
                var item = _items.FirstOrDefault(x => x.Id == id);
                if (item != null)
                {
                    _items.Remove(item);
                    return true;
                }
                return false;
            }
        }

        public TaskItem? GetItem(int id)
        {
            lock (_lockObject)
            {
                return _items.FirstOrDefault(x => x.Id == id);
            }
        }
    }
}
