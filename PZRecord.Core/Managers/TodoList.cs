using PZRecorder.Core.Tables;

namespace PZRecorder.Core.Managers;

public class TodoListManager(SqlHandler db)
{
    private SqlHandler DB { get; init; } = db;

    public List<TodoList> GetAllLists()
    {
        var list = DB.Conn.Table<TodoList>().ToList();
        list.Sort((x, y) => {
            if (x.Completed == y.Completed) return x.AddTime > y.AddTime ? 1 : -1;
            else
            {
                return x.Completed ? 1 : -1;
            }
        });
        return list;
    }

    public int Insert(TodoList item)
    {
        item.AddTime = DateTime.Now;

        return DB.Conn.Insert(item);
    }
    public int Update(TodoList item)
    {
        return DB.Conn.Update(item);
    }
    public int Delete(int id)
    {
        return DB.Conn.Delete<TodoList>(id);
    }
}
