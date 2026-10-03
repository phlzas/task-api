using tasks.Models.Models;
using tasks.Reposetry.IRepos;
using tasks.Entitys;
using Microsoft.EntityFrameworkCore;

namespace tasks.Reposetry.Repos
{
    public class RepoTaskItem : IRepoTaskItem
    {
        private readonly AppDbContext _dbContext;

        public RepoTaskItem(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<TaskItem> Items => _dbContext.TaskItems.ToList();

        public void AddItem(TaskItem item)
        {
            _dbContext.TaskItems.Add(item);
            _dbContext.SaveChanges();
        }

        public TaskItem? GetItem(int id)
        {
            return _dbContext.TaskItems.FirstOrDefault(t => t.Id == id);
        }

        public bool RemoveItem(int id)
        {
            var item = GetItem(id);
            if (item == null)
                return false;

            _dbContext.TaskItems.Remove(item);
            _dbContext.SaveChanges();
            return true;
        }

        public void UpdateItem(TaskItem item)
        {
            _dbContext.TaskItems.Update(item);
            _dbContext.SaveChanges();
        }
    }
}
