namespace tasks.Models;

// One task in the list. This is your whole "database" for Week 2.
//
// The class is called TaskItem, not Task, on purpose: C# already has a
// System.Threading.Tasks.Task, and naming yours Task causes confusing
// compile errors. This is a very common first stumble with C#.

public class TaskItem
{
    // TODO 1: give it three properties
    //     int  Id      - unique number, starts at 1
    //     string Title - the task text
    //     bool  Done   - true/false, starts false
    //
    // Note: for the POST body you will need a SEPARATE small class later
    // that only has Title (the client should not be able to choose the Id
    // or set Done on create). You will meet that in Stage 3.
}
