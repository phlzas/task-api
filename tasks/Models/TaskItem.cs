namespace tasks.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Named <c>isCompleted</c>, a documented deviation from the brief's
        /// <c>done</c>. See "Known deviation" in the README. JSON rejects
        /// unmapped members, so <c>done</c> gets a 400 rather than being
        /// silently dropped.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}
