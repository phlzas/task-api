using tasks.Models;

namespace tasks.Reposetry.IRepos
{
    public interface IRepoTaskItem
    {
        List<TaskItem> Items { get; }
        void AddItem(TaskItem item);
        bool RemoveItem(int id);
        TaskItem? GetItem(int id);
    }
}
