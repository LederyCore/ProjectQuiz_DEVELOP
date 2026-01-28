using UnityEngine;

public interface ITask
{
    Awaitable<bool> ExecuteAsync();
}