using tasks.Models.Models;

namespace tasks.Reposetry.IRepos
{
    public interface IRepoTaskItem
    {
        List<TaskItem> Items { get; }
        void AddItem(TaskItem item);
        bool RemoveItem(int id);
        TaskItem? GetItem(int id);
        void UpdateItem(TaskItem item);
    }
}
