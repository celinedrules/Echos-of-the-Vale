using System;
using UnityEngine;
using Utilities;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        private void Update()
        {
            TimerManager.Instance.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }
    }
}